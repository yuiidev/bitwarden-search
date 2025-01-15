using System.Text.Json.Serialization;

namespace YuiiDev.Bitwarden.VaultItemTypes;

public class CardItem : BaseVaultItem
{
    [JsonPropertyName("card")] public CardData Card { get; set; }

    public struct CardData
    {
        [JsonPropertyName("cardholderName")] public string CardholderName { get; set; }
        [JsonPropertyName("brand")] public string Brand { get; set; }
        [JsonPropertyName("number")] public string Number { get; set; }
        [JsonPropertyName("expMonth")] public string ExpirationMonth { get; set; }
        [JsonPropertyName("expYear")] public string ExpirationYear { get; set; }
        [JsonPropertyName("code")] public string SecurityCode { get; set; }
    }

    public override string GetFriendlyName()
    {
        return "Card";
    }
}