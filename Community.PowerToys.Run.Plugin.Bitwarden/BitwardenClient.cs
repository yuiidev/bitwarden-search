using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using ThreadState = System.Diagnostics.ThreadState;

namespace Community.PowerToys.Run.Plugin.Bitwarden;

public partial class BitwardenClient
{
    private readonly string _bitwardenCliPath;
    private const string AlreadyLoggedInPhrase = "You are already logged in as";
    private string _clientId;
    private string _secret;
    private string _masterPasswordFile;
    private string _sessionKey;
    private bool _isReady;
    private bool _isLoggedIn;
    private bool _isUnlocked;

    public BitwardenClient()
    {
        _bitwardenCliPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bw.exe";
    }

    public async Task<bool> Login()
    {
        if (!_isReady)
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
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        await process.WaitForExitAsync();
        var content = await process.StandardOutput.ReadToEndAsync();
        var errors = (await process.StandardError.ReadToEndAsync()).Trim();
        Debug.WriteLine(content);

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
        if (!_isLoggedIn)
        {
            throw new BitwardenClientNotReadyException();
        }

        using var process = new Process();
        process.StartInfo.FileName = _bitwardenCliPath;
        process.StartInfo.ArgumentList.Add("unlock");
        process.StartInfo.ArgumentList.Add($"--passwordfile={_masterPasswordFile}");
        process.StartInfo.ArgumentList.Add("--nointeraction");
        process.StartInfo.ArgumentList.Add("--raw");
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        await process.WaitForExitAsync();

        var content = (await process.StandardOutput.ReadToEndAsync()).Trim();
        var errors = await process.StandardError.ReadToEndAsync();

        if (!string.IsNullOrEmpty(errors.Trim()))
        {
            Debug.WriteLine(errors);
            return false;
        }

        _sessionKey = content;
        
        if (string.IsNullOrWhiteSpace(_sessionKey))
        {
            return false;
        }

        _isUnlocked = true;
        Debug.WriteLine(content);

        return true;
    }

    public async Task Query(string query)
    {
        if (!_isUnlocked || string.IsNullOrWhiteSpace(_sessionKey))
        {
            throw new BitwardenClientNotReadyException();
        }

        using var process = new Process();
        process.StartInfo.FileName = _bitwardenCliPath;
        process.StartInfo.ArgumentList.Add("list");
        process.StartInfo.ArgumentList.Add("items");
        process.StartInfo.ArgumentList.Add("--nointeraction");
        process.StartInfo.ArgumentList.Add("--response");
        process.StartInfo.ArgumentList.Add("--pretty");

        if (!string.IsNullOrWhiteSpace(query))
        {
            process.StartInfo.ArgumentList.Add($"--search");
            process.StartInfo.ArgumentList.Add($"{query}");
        }

        process.StartInfo.EnvironmentVariables.Add("BW_SESSION", _sessionKey);
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        var content = await process.StandardOutput.ReadToEndAsync();
        var errors = (await process.StandardError.ReadToEndAsync()).Trim();

        if (!string.IsNullOrWhiteSpace(errors))
        {
            Debug.WriteLine("Error:\n" + errors);
            return;
        }

        Debug.WriteLine("Results:\n" + content);
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
        _masterPasswordFile = masterPasswordFilePath;

        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_secret) || string.IsNullOrEmpty(_masterPasswordFile))
        {
            return false;
        }

        _isReady = true;

        return true;
    }
}