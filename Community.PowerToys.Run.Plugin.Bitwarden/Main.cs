using System.Windows;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Plugin;
using System.Windows.Controls;
using System.Windows.Input;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.Bitwarden;

public class Main : IPlugin, IDelayedExecutionPlugin, IContextMenu, ISettingProvider, IDisposable
{
    public static string PluginID => "CC29F001F973446EA041BBECEEB42F57";
    public string Name => "Bitwarden";
    public string Description => "Searches in your Bitwarden passwords and notes";
    private BitwardenClient _bitwardenClient;
    
    public string ClientIdFilePath { get; private set; }
    public string ClientSecretFilePath { get; private set; }
    public string MasterPasswordFilePath { get; private set; }

    public PluginInitContext Context { get; private set; }

    public IEnumerable<PluginAdditionalOption> AdditionalOptions => [
        new()
        {
            Key = nameof(ClientIdFilePath),
            DisplayLabel = "Client ID",
            DisplayDescription = "Path to the file containing your Bitwarden client ID. Please make sure read access is limited to your user only.",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            TextValue = ClientIdFilePath,
        },
        new()
        {
            Key = nameof(ClientSecretFilePath),
            DisplayLabel = "Client secret",
            DisplayDescription = "Path to the file containing your Bitwarden client secret. Please make sure read access is limited to your user only.",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            TextValue = ClientSecretFilePath,
        },
        new()
        {
            Key = nameof(MasterPasswordFilePath),
            DisplayLabel = "Master password",
            DisplayDescription = "Path to the file containing your Bitwarden master password. Please make sure read access is limited to your user only.",
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox,
            TextValue = MasterPasswordFilePath,
        }
    ];

    public List<Result> Query(Query query) => [];

    public void Init(PluginInitContext context)
    {
        Log.Info("Bitwarden plugin init", GetType());

        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    {
        var list = new List<ContextMenuResult>();

        if (selectedResult?.ContextData is (string username, string password, string mfa))
        {
            if (!string.IsNullOrWhiteSpace(username))
            {
                list.Add(
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy username (Enter)",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        Glyph = "\xE77B",
                        AcceleratorKey = Key.Enter,
                        Action = _ => CopyToClipboard(username),
                    }
                );
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                list.Add(
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy password (Shift + Enter)",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        Glyph = "\xEDAD",
                        AcceleratorKey = Key.Enter,
                        AcceleratorModifiers = ModifierKeys.Shift,
                        Action = _ => CopyToClipboard(password),
                    }
                );
            }

            if (!string.IsNullOrWhiteSpace(mfa))
            {
                list.Add(
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy MFA token (Ctrl + Enter)",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        Glyph = "\xE72E",
                        AcceleratorKey = Key.Enter,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ => CopyToClipboard(mfa),
                    }
                );
            }
        }

        if (selectedResult?.ContextData is (string note))
        {
            if (!string.IsNullOrWhiteSpace(note))
            {
                list.Add(
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy secure note (Enter)",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        Glyph = "\xE70B",
                        AcceleratorKey = Key.Enter,
                        Action = _ => CopyToClipboard(note),
                    }
                );
            }
        }

        return list;
    }

    private bool CopyToClipboard(string text)
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

        ClientIdFilePath = settings.AdditionalOptions.FirstOrDefault(x => x.Key == nameof(ClientIdFilePath))?.TextValue ?? string.Empty;
        ClientSecretFilePath = settings.AdditionalOptions.FirstOrDefault(x => x.Key == nameof(ClientSecretFilePath))?.TextValue ?? string.Empty;
        MasterPasswordFilePath = settings.AdditionalOptions.FirstOrDefault(x => x.Key == nameof(MasterPasswordFilePath))?.TextValue ?? string.Empty;

        AttemptStart().Wait();
    }

    public void Dispose()
    {
    }

    public List<Result> Query(Query query, bool delayedExecution)
    {
        return new List<Result>([
            new Result
            {
                QueryTextDisplay = query.Search,
                Title = "Bitwarden Delayed Test",
                SubTitle = "Subtitle test",
                ToolTipData = new ToolTipData("BitwardenTest", "Bitwarden Test"),
                ContextData = ("username", "password", "mfa")
            }
        ]);
    }

    private async Task AttemptStart()
    {
        if (string.IsNullOrWhiteSpace(ClientIdFilePath) || string.IsNullOrWhiteSpace(ClientSecretFilePath))
        {
            return;
        }
        
        if (!_bitwardenClient.VerifySettings(ClientIdFilePath, ClientSecretFilePath, MasterPasswordFilePath))
        {
            MessageBox.Show("Bitwarden authentication settings are invalid.");
            return;
        }
        
        await _bitwardenClient.Login();
    }
}