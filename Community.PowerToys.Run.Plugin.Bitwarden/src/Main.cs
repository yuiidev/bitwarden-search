using System.Windows;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Plugin;
using System.Windows.Controls;
using Wox.Plugin.Logger;
using YuiiDev.Bitwarden;

namespace Community.PowerToys.Run.Plugin.Bitwarden;

public class Main : IPlugin, IDelayedExecutionPlugin, IContextMenu, ISettingProvider, IDisposable
{
    public static string PluginID => "CC29F001F973446EA041BBECEEB42F57";
    public static string PluginName => "Bitwarden";
    public string Name => PluginName;
    public string Description => "Searches in your Bitwarden passwords and notes";
    private BitwardenClient _bitwardenClient;

    public string ClientIdFilePath { get; private set; }
    public string ClientSecretFilePath { get; private set; }
    public string MasterPasswordFilePath { get; private set; }
    public int ServePort { get; private set; } = 8087;

    public PluginInitContext Context { get; private set; }

    public IEnumerable<PluginAdditionalOption> AdditionalOptions =>
    [
        new()
        {
            Key = nameof(ClientIdFilePath),
            DisplayLabel = "Client ID",
            DisplayDescription =
                "Path to the file containing your Bitwarden client ID. Please make sure read access is limited to your user only.",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            TextValue = ClientIdFilePath,
        },
        new()
        {
            Key = nameof(ClientSecretFilePath),
            DisplayLabel = "Client secret",
            DisplayDescription =
                "Path to the file containing your Bitwarden client secret. Please make sure read access is limited to your user only.",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            TextValue = ClientSecretFilePath,
        },
        new()
        {
            Key = nameof(MasterPasswordFilePath),
            DisplayLabel = "Master password",
            DisplayDescription =
                "Path to the file containing your Bitwarden master password. Please make sure read access is limited to your user only.",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            TextValue = MasterPasswordFilePath,
        },
        new()
        {
            Key = nameof(ServePort),
            DisplayLabel = "Serve port",
            DisplayDescription =
                "Port to host the Bitwarden serve HTTP server on.",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Numberbox,
            NumberValue = ServePort,
        }
    ];

    public Main()
    {
        // To allow plugin instantiation
    }

    /// <summary>
    /// Only used for testing.
    /// </summary>
    /// <param name="clientIdFilePath"></param>
    /// <param name="clientSecretFilePath"></param>
    /// <param name="masterPasswordFilePath"></param>
    /// <param name="servePort"></param>
    public Main(string clientIdFilePath, string clientSecretFilePath, string masterPasswordFilePath, int servePort)
    {
        ClientIdFilePath = clientIdFilePath;
        ClientSecretFilePath = clientSecretFilePath;
        MasterPasswordFilePath = masterPasswordFilePath;
        ServePort = servePort;
    }

    public List<Result> Query(Query query) => [];

    public void Init(PluginInitContext context)
    {
        Log.Info("Bitwarden plugin init", GetType());

        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    {
        return ResultConverter.ConvertContextMenu(selectedResult);
    }

    public static bool CopyToClipboard(string text)
    {
        try
        {
            Clipboard.SetText(text);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Control CreateSettingPanel()
    {
        throw new NotImplementedException();
    }

    public void UpdateSettings(PowerLauncherPluginSettings settings)
    {
        Log.Info("Update settings", GetType());

        ClientIdFilePath =
            settings.AdditionalOptions.FirstOrDefault(x => x.Key == nameof(ClientIdFilePath))?.TextValue ??
            string.Empty;
        ClientSecretFilePath =
            settings.AdditionalOptions.FirstOrDefault(x => x.Key == nameof(ClientSecretFilePath))?.TextValue ??
            string.Empty;
        MasterPasswordFilePath =
            settings.AdditionalOptions.FirstOrDefault(x => x.Key == nameof(MasterPasswordFilePath))?.TextValue ??
            string.Empty;
        ServePort = (int)Math.Floor(
            settings.AdditionalOptions.FirstOrDefault(x => x.Key == nameof(ServePort))?.NumberValue ?? 8087
        );
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _bitwardenClient.Stop();
    }

    public List<Result> Query(Query query, bool delayedExecution)
    {
        if (_bitwardenClient == null)
        {
            _ = AttemptStart();
        }
        
        var metaResults = new List<Result>();

        if (_bitwardenClient.IsUnlocked)
        {
            metaResults.Add(new Result
                {
                    Title = "Lock",
                    SubTitle = Name,
                    Action = context =>
                    {
                        _ = _bitwardenClient.Lock();
                        return true;
                    },
                    Score = -10,
                }
            );
        }
        else
        {
            metaResults.Add(new Result
                {
                    Title = "Unlock",
                    SubTitle = Name,
                    Action = context =>
                    {
                        _ = _bitwardenClient.Unlock();
                        return true;
                    },
                    Score = -10,
                }
            );

            return metaResults;
        }
        
        metaResults.Add(new Result
        {
            Title = "Sync vault",
            Action = context =>
            {
                _ = _bitwardenClient.Sync();
                return true;
            }
        });
        
        var queryTask = _bitwardenClient.Query(query.Search);

        queryTask.Wait();

        var queryResponse = queryTask.Result;
        var convertResults = ResultConverter.ConvertResults(queryResponse);
        convertResults.AddRange(metaResults);
        
        return convertResults;
    }

    public async Task AttemptStart()
    {
        _bitwardenClient = new BitwardenClient(ServePort);
        _bitwardenClient.Notified += BitwardenClientOnNotified;

        if (string.IsNullOrWhiteSpace(ClientIdFilePath) || string.IsNullOrWhiteSpace(ClientSecretFilePath))
        {
            return;
        }

        if (!_bitwardenClient.VerifySettings(ClientIdFilePath, ClientSecretFilePath, MasterPasswordFilePath))
        {
            MessageBox.Show("Bitwarden auth file settings are invalid.");
            return;
        }

        if (!await _bitwardenClient.Login())
        {
            MessageBox.Show("Bitwarden auth file contents are invalid.");
        }

        _bitwardenClient.Start();
    }

    private void BitwardenClientOnNotified(object? sender, string message)
    {
        Log.Info("client said: " + message, GetType());
    }

    public void Stop()
    {
        _bitwardenClient.Stop();
    }
}