using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using YuiiDev.Bitwarden.Exceptions;
using YuiiDev.Bitwarden.Requests;
using YuiiDev.Bitwarden.Responses;

namespace YuiiDev.Bitwarden;

public class BitwardenClient
{
    private readonly string _bitwardenCliPath;
    private const string BwServeUrl = "http://localhost";
    private readonly int _port;
    private const string AlreadyLoggedInPhrase = "You are already logged in as";
    private string _clientId;
    private string _secret;
    private string _masterPassword;
    private bool _isValid;
    private bool _isLoggedIn;
    private HttpClient _httpClient;
    private static Process? _bwServeProcess;

    public BitwardenClient(int port = 8087)
    {
        _port = port;
        _bitwardenCliPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bw.exe";
        _httpClient = new()
        {
            BaseAddress = new Uri($"{BwServeUrl}:{_port}"),
        };
    }

    public void Start()
    {
        if (!_isValid || !_isLoggedIn)
        {
            throw new BitwardenClientNotReadyException();
        }

        if (_bwServeProcess != null && !_bwServeProcess.HasExited)
        {
            _bwServeProcess.Kill();
            _bwServeProcess.Dispose();
            _bwServeProcess = null;
        }

        _bwServeProcess = new Process();
        _bwServeProcess.StartInfo.FileName = _bitwardenCliPath;
        _bwServeProcess.StartInfo.ArgumentList.Add("serve");
        _bwServeProcess.StartInfo.ArgumentList.Add("--port");
        _bwServeProcess.StartInfo.ArgumentList.Add(_port.ToString());
        _bwServeProcess.Start();
    }

    public void Stop()
    {
        if (_bwServeProcess == null || _bwServeProcess.HasExited) return;

        Lock().Wait();
        _bwServeProcess.Kill();
        _bwServeProcess.Dispose();
        _bwServeProcess = null;
    }

    public async Task<bool> Login()
    {
        if (!_isValid)
        {
            throw new BitwardenClientNotReadyException();
        }

        using var process = new Process();
        process.StartInfo.FileName = _bitwardenCliPath;
        process.StartInfo.EnvironmentVariables.Add("BW_CLIENTID", _clientId);
        process.StartInfo.EnvironmentVariables.Add("BW_CLIENTSECRET", _secret);
        process.StartInfo.ArgumentList.Add("login");
        process.StartInfo.ArgumentList.Add("--apikey");
        process.StartInfo.ArgumentList.Add("--nointeraction");
        process.StartInfo.ArgumentList.Add("--raw");
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        await process.WaitForExitAsync();
        var errors = (await process.StandardError.ReadToEndAsync()).Trim();

        if (!string.IsNullOrEmpty(errors))
        {
            if (errors.Contains(AlreadyLoggedInPhrase))
            {
                _isLoggedIn = true;
                return true;
            }

            Debug.WriteLine(errors);
            return false;
        }

        _isLoggedIn = true;
        return true;
    }

    public async Task<bool> Unlock()
    {
        if (!_isValid || !_isLoggedIn)
        {
            throw new BitwardenClientNotReadyException();
        }

        var response = await _httpClient.PostAsJsonAsync("/unlock", new UnlockRequest
        {
            Password = _masterPassword,
        });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Lock()
    {
        if (!_isValid || !_isLoggedIn)
        {
            throw new BitwardenClientNotReadyException();
        }

        var response = await _httpClient.PostAsync("/lock", null);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> Sync()
    {
        var response = await _httpClient.PostAsync("/sync", null);

        return response.IsSuccessStatusCode;
    }

    public async Task<QueryResponse?> Query(string query)
    {
        if (!_isValid || !_isLoggedIn)
        {
            throw new BitwardenClientNotReadyException();
        }
        
        if (!await Unlock())
        {
            throw new BitwardenUnlockFailedException();
        }

        // Do query
        var request = new QueryRequest
        {
            Search = query,
        };

        var response = await _httpClient.GetFromJsonAsync<QueryResponse>(
            $"/list/object/items?{request.ToQueryString()}",
            new JsonSerializerOptions
            {
                AllowOutOfOrderMetadataProperties = true,
            }
        );

        if (!response.Success)
        {
            return null;
        }

        if (!await Lock())
        {
            throw new BitwardenLockFailedException();
        }
        
        return response;
    }

    public bool VerifySettings(string clientIdFilePath, string clientSecretFilePath, string masterPasswordFilePath)
    {
        if (!File.Exists(clientIdFilePath) || !File.Exists(clientSecretFilePath) ||
            !File.Exists(masterPasswordFilePath))
        {
            return false;
        }

        _clientId = File.ReadLines(clientIdFilePath).FirstOrDefault(string.Empty);
        _secret = File.ReadAllLines(clientSecretFilePath).FirstOrDefault(string.Empty);
        _masterPassword = File.ReadAllLines(masterPasswordFilePath).FirstOrDefault(string.Empty);

        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_secret) || string.IsNullOrEmpty(_masterPassword))
        {
            return false;
        }

        _isValid = true;

        return true;
    }
}