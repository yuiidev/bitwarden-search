using System.Text.Json.Serialization;

namespace YuiiDev.Bitwarden.VaultItemTypes;

public class LoginItem : BaseVaultItem
{
    [JsonPropertyName("login")] public LoginData Login { get; set; }

    public struct LoginData
    {
        [JsonPropertyName("username")] public string Username { get; set; }
        [JsonPropertyName("password")] public string Password { get; set; }
        [JsonPropertyName("totp")] public string MfaCode { get; set; }
    }
}