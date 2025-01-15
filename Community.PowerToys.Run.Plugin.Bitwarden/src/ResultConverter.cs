using System.Windows.Input;
using Wox.Plugin;
using YuiiDev.Bitwarden.Responses;
using YuiiDev.Bitwarden.VaultItemTypes;

namespace Community.PowerToys.Run.Plugin.Bitwarden;

public static class ResultConverter
{
    public static List<Result> ConvertResults(QueryResponse queryResponse)
    {
        var results = new List<Result>();

        foreach (var vaultItem in queryResponse.Data.Items)
        {
            var result = new Result
            {
                Title = vaultItem.Name,
                SubTitle = $"{vaultItem.GetFriendlyName()}",
            };

            switch (vaultItem)
            {
                case LoginItem loginItem:
                    result.ContextData = loginItem.Login;
                    result.SubTitle += $" ― ({loginItem.Login.Username})";
                    result.Glyph = "\xE77B";
                    break;
                case SecureNoteItem secureNoteItem:
                    result.ContextData = secureNoteItem;
                    result.Glyph = "\xE70B";
                    break;
                case CardItem cardItem:
                    result.ContextData = cardItem.Card;
                    result.SubTitle += $" ― ({cardItem.Card.Brand})";
                    result.Glyph = "\xE8C7";
                    break;
                case IdentityItem identityItem:
                    result.ContextData = identityItem.Identity;
                    result.SubTitle +=
                        $"({identityItem.Identity.FirstName} {identityItem.Identity.MiddleName} {identityItem.Identity.LastName})";
                    result.Glyph = "\xF427";
                    break;
                default:
                    throw new InvalidOperationException($"Unknown result type: {vaultItem.GetType().FullName}");
            }

            results.Add(result);
        }

        return results;
    }

    public static List<ContextMenuResult> ConvertContextMenu(Result selectedResult)
    {
        var contextMenuItems = new List<ContextMenuResult>();
        var contextData = selectedResult.ContextData;

        if (contextData == null)
        {
            return contextMenuItems;
        }

        switch (contextData)
        {
            case LoginItem.LoginData loginData:
                if (!string.IsNullOrWhiteSpace(loginData.Username))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy username (Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xE77B",
                            AcceleratorKey = Key.Enter,
                            Action = _ => Main.CopyToClipboard(loginData.Username),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(loginData.Password))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy password (Shift + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xEDAD",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Shift,
                            Action = _ => Main.CopyToClipboard(loginData.Password),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(loginData.MfaCode)) // ec19
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy MFA token (Ctrl + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xEA18",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Control,
                            Action = _ => Main.CopyToClipboard(loginData.MfaCode),
                        }
                    );
                }

                break;
            case SecureNoteItem secureNoteItem:
                if (!string.IsNullOrWhiteSpace(secureNoteItem.Content))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy secure note (Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xE70B",
                            AcceleratorKey = Key.Enter,
                            Action = _ => Main.CopyToClipboard(secureNoteItem.Content),
                        }
                    );
                }

                break;
            case CardItem.CardData cardData:
                if (!string.IsNullOrWhiteSpace(cardData.Number))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy card number (Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xE70B",
                            AcceleratorKey = Key.Enter,
                            Action = _ => Main.CopyToClipboard(cardData.Number),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(cardData.CardholderName))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy cardholder name (Shift + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xE70B",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Shift,
                            Action = _ => Main.CopyToClipboard(cardData.CardholderName),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(cardData.ExpirationMonth))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy expiration month (Ctrl + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xE787",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Control,
                            Action = _ => Main.CopyToClipboard(cardData.ExpirationMonth),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(cardData.ExpirationYear))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy expiration year (Alt + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xE787",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Alt,
                            Action = _ => Main.CopyToClipboard(cardData.ExpirationYear),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(cardData.SecurityCode))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy security code (Ctrl + Shift + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xEDAD",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Shift & ModifierKeys.Control,
                            Action = _ => Main.CopyToClipboard(cardData.SecurityCode),
                        }
                    );
                }

                break;
            case IdentityItem.IdentityData identityData:
                if (!string.IsNullOrWhiteSpace(identityData.FirstName))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy first name (Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xF427",
                            AcceleratorKey = Key.Enter,
                            Action = _ => Main.CopyToClipboard(identityData.FirstName),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(identityData.MiddleName))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy middle name (Ctrl + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xF427",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Control,
                            Action = _ => Main.CopyToClipboard(identityData.MiddleName),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(identityData.LastName))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy last name (Shift + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xF427",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Shift,
                            Action = _ => Main.CopyToClipboard(identityData.LastName),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(identityData.AddressOne))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy address one (Alt + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xE80F",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Alt,
                            Action = _ => Main.CopyToClipboard(identityData.AddressOne),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(identityData.City))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy city (Ctrl + Alt + Shift + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xEC06",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Control & ModifierKeys.Shift & ModifierKeys.Alt,
                            Action = _ => Main.CopyToClipboard(identityData.City),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(identityData.State))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy state/province",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xE707",
                            Action = _ => Main.CopyToClipboard(identityData.State),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(identityData.PostalCode))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy postal code (Ctrl + Alt + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xEDB3",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Control & ModifierKeys.Alt,
                            Action = _ => Main.CopyToClipboard(identityData.PostalCode),
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(identityData.Phone))
                {
                    contextMenuItems.Add(
                        new ContextMenuResult
                        {
                            PluginName = Main.PluginName,
                            Title = "Copy phone number (Ctrl + Shift + Enter)",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            Glyph = "\xE717",
                            AcceleratorKey = Key.Enter,
                            AcceleratorModifiers = ModifierKeys.Control & ModifierKeys.Shift,
                            Action = _ => Main.CopyToClipboard(identityData.Phone),
                        }
                    );
                }

                break;
        }

        return contextMenuItems;
    }
}