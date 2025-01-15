using System.Text.Json.Serialization;
using YuiiDev.Bitwarden.VaultItemTypes;

namespace YuiiDev.Bitwarden.Responses;

public struct QueryResponse
{
    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("data")] public DataObject Data { get; set; }

    public bool IsEmpty => (Data.Items?.Count ?? 0) == 0;

    public struct DataObject
    {
        [JsonPropertyName("data")] public List<BaseVaultItem> Items { get; set; }
    }
}