using System.Text.Json.Serialization;

namespace YuiiDev.Bitwarden.VaultItemTypes;

public class IdentityItem : BaseVaultItem
{
    [JsonPropertyName("identity")] public IdentityData Identity { get; set; }

    public struct IdentityData
    {
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("firstName")] public string FirstName { get; set; }
        [JsonPropertyName("middleName")] public string MiddleName { get; set; }
        [JsonPropertyName("lastName")] public string LastName { get; set; }
        [JsonPropertyName("address1")] public string AddressOne { get; set; }
        [JsonPropertyName("address2")] public string AddressTwo { get; set; }
        [JsonPropertyName("address3")] public string AddressThree { get; set; }
        [JsonPropertyName("city")] public string City { get; set; }
        [JsonPropertyName("state")] public string State { get; set; }
        [JsonPropertyName("postalCode")] public string PostalCode { get; set; }
        [JsonPropertyName("country")] public string Country { get; set; }
        [JsonPropertyName("company")] public string Company { get; set; }
        [JsonPropertyName("email")] public string Email { get; set; }
        [JsonPropertyName("phone")] public string Phone { get; set; }
        [JsonPropertyName("ssn")] public string SocialSecurityNumber { get; set; }
        [JsonPropertyName("username")] public string Username { get; set; }
        [JsonPropertyName("passportNumber")] public string DocumentNumber { get; set; }
        [JsonPropertyName("licenseNumber")] public string LicenseNumber { get; set; }
    }
}