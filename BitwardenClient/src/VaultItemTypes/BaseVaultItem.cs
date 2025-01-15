using System.Text.Json.Serialization;

namespace YuiiDev.Bitwarden.VaultItemTypes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LoginItem), 1)]
[JsonDerivedType(typeof(SecureNoteItem), 2)]
[JsonDerivedType(typeof(CardItem), 3)]
[JsonDerivedType(typeof(IdentityItem), 4)]
public class BaseVaultItem
{
    [JsonPropertyName("name")] public string Name { get; set; }
}