using System.Text.Json;
using YuiiDev.Bitwarden.Responses;
using YuiiDev.Bitwarden.VaultItemTypes;

namespace YuiiDev.Bitwarden.Tests;

[TestClass]
public class JsonSerializationTests
{
    private const string LoginData = """
                                     {
                                         "success": true,
                                         "data": {
                                             "object": "list",
                                             "data": [
                                                 {
                                                     "passwordHistory": null,
                                                     "revisionDate": "2025-01-02T10:53:50.203Z",
                                                     "creationDate": "2025-01-02T10:52:42.106Z",
                                                     "deletedDate": null,
                                                     "object": "item",
                                                     "id": "a788aaaa-65eb-4b66-8438-aaaa00b3451a",
                                                     "organizationId": null,
                                                     "folderId": "63eaaaae-b1e0-4c58-b428-aaaa00a5e266",
                                                     "type": 1,
                                                     "reprompt": 0,
                                                     "name": "test account",
                                                     "notes": null,
                                                     "favorite": false,
                                                     "login": {
                                                         "fido2Credentials": [],
                                                         "uris": [
                                                             {
                                                                 "match": null,
                                                                 "uri": "https://example.com/"
                                                             }
                                                         ],
                                                         "username": "user@example.com",
                                                         "password": "password",
                                                         "totp": null,
                                                         "passwordRevisionDate": null
                                                     },
                                                     "collectionIds": []
                                                 }
                                             ]
                                         }
                                     }
                                     """;

    private const string SecureNoteData = """
                                          {
                                              "success": true,
                                              "data": {
                                                  "object": "list",
                                                  "data": [
                                                      {
                                                          "passwordHistory": null,
                                                          "revisionDate": "2025-01-15T08:54:48.963Z",
                                                          "creationDate": "2025-01-15T08:54:48.963Z",
                                                          "deletedDate": null,
                                                          "object": "item",
                                                          "id": "09d7b146-aaaa-4aaa-9fbc-b2660092e442",
                                                          "organizationId": null,
                                                          "folderId": null,
                                                          "type": 2,
                                                          "reprompt": 0,
                                                          "name": "Test Secure Note",
                                                          "notes": "Some test content",
                                                          "favorite": false,
                                                          "secureNote": {
                                                              "type": 0
                                                          },
                                                          "collectionIds": []
                                                      }
                                                  ]
                                              }
                                          }
                                          """;

    private const string CardData = """
                                    {
                                        "success": true,
                                        "data": {
                                            "object": "list",
                                            "data": [
                                                {
                                                    "passwordHistory": null,
                                                    "revisionDate": "2025-01-15T06:59:42.300Z",
                                                    "creationDate": "2025-01-15T06:59:42.300Z",
                                                    "deletedDate": null,
                                                    "object": "item",
                                                    "id": "e2aaaa1a-3fc3-4aaa-877a-baaaa0734683",
                                                    "organizationId": null,
                                                    "folderId": null,
                                                    "type": 3,
                                                    "reprompt": 0,
                                                    "name": "Test Card",
                                                    "notes": null,
                                                    "favorite": false,
                                                    "card": {
                                                        "cardholderName": "Test",
                                                        "brand": "Amex",
                                                        "number": "3700 0000 0000 002",
                                                        "expMonth": "3",
                                                        "expYear": "2030",
                                                        "code": "7373"
                                                    },
                                                    "collectionIds": []
                                                }
                                            ]
                                        }
                                    }
                                    """;
    
    private const string IdentityData = """
                                        {
                                            "success": true,
                                            "data": {
                                                "object": "list",
                                                "data": [
                                                    {
                                                        "passwordHistory": null,
                                                        "revisionDate": "2024-05-16T16:41:22.190Z",
                                                        "creationDate": "2024-05-16T15:42:31.490Z",
                                                        "deletedDate": null,
                                                        "object": "item",
                                                        "id": "8deed264-d452-4cf9-b30a-b1720102df49",
                                                        "organizationId": null,
                                                        "folderId": null,
                                                        "type": 4,
                                                        "reprompt": 0,
                                                        "name": "MR Lorenzo Demoniere",
                                                        "notes": null,
                                                        "favorite": false,
                                                        "identity": {
                                                            "title": "Mr",
                                                            "firstName": "John",
                                                            "middleName": "Travis",
                                                            "lastName": "Doe",
                                                            "address1": "Example 1",
                                                            "address2": null,
                                                            "address3": null,
                                                            "city": "Demo City",
                                                            "state": "Demo State",
                                                            "postalCode": "1234AB",
                                                            "country": "United States",
                                                            "company": "ACME Corporation",
                                                            "email": "user@example.com",
                                                            "phone": "+31612345678",
                                                            "ssn": null,
                                                            "username": "john.doe",
                                                            "passportNumber": null,
                                                            "licenseNumber": null
                                                        },
                                                        "collectionIds": []
                                                    }
                                                ]
                                            }
                                        }
                                        """;

    [TestMethod]
    public void Deserialize_LoginData_IsSuccess()
    {
        var obj = JsonSerializer.Deserialize<QueryResponse>(LoginData, new JsonSerializerOptions
        {
            AllowOutOfOrderMetadataProperties = true,
        });
        var first = obj.Data.Items.First();

        Assert.IsNotNull(obj);
        Assert.IsFalse(obj.IsEmpty);
        Assert.IsNotNull(first);
        Assert.IsInstanceOfType<LoginItem>(first);

        var login = (first as LoginItem)!;
        Assert.AreEqual("user@example.com", login.Login.Username);
        Assert.AreEqual("password", login.Login.Password);
        Assert.IsNull(login.Login.MfaCode);
    }

    [TestMethod]
    public void Deserialize_SecureNoteData_IsSuccess()
    {
        var obj = JsonSerializer.Deserialize<QueryResponse>(SecureNoteData, new JsonSerializerOptions
        {
            AllowOutOfOrderMetadataProperties = true,
        });
        var first = obj.Data.Items.First();

        Assert.IsNotNull(obj);
        Assert.IsFalse(obj.IsEmpty);
        Assert.IsNotNull(first);
        Assert.IsInstanceOfType<SecureNoteItem>(first);

        var note = (first as SecureNoteItem)!;
        Assert.AreEqual("Some test content", note.Content);
    }

    [TestMethod]
    public void Deserialize_CardData_IsSuccess()
    {
        var obj = JsonSerializer.Deserialize<QueryResponse>(CardData, new JsonSerializerOptions
        {
            AllowOutOfOrderMetadataProperties = true,
        });
        var first = obj.Data.Items.First();

        Assert.IsNotNull(obj);
        Assert.IsFalse(obj.IsEmpty);
        Assert.IsNotNull(first);
        Assert.IsInstanceOfType<CardItem>(first);

        var card = (first as CardItem)!;
        Assert.AreEqual("Test", card.Card.CardholderName);
        Assert.AreEqual("Amex", card.Card.Brand);
        Assert.AreEqual("3700 0000 0000 002", card.Card.Number);
        Assert.AreEqual("3", card.Card.ExpirationMonth);
        Assert.AreEqual("2030", card.Card.ExpirationYear);
        Assert.AreEqual("7373", card.Card.SecurityCode);
    }

    [TestMethod]
    public void Deserialize_IdentityData_IsSuccess()
    {
        var obj = JsonSerializer.Deserialize<QueryResponse>(IdentityData, new JsonSerializerOptions
        {
            AllowOutOfOrderMetadataProperties = true,
        });
        var first = obj.Data.Items.First();

        Assert.IsNotNull(obj);
        Assert.IsFalse(obj.IsEmpty);
        Assert.IsNotNull(first);
        Assert.IsInstanceOfType<IdentityItem>(first);

        var identity = (first as IdentityItem)!;
        Assert.AreEqual("John", identity.Identity.FirstName);
        Assert.AreEqual("Travis", identity.Identity.MiddleName);
        Assert.AreEqual("Doe", identity.Identity.LastName);
        Assert.AreEqual("Example 1", identity.Identity.AddressOne);
        Assert.AreEqual("Demo City", identity.Identity.City);
        Assert.AreEqual("Demo State", identity.Identity.State);
        Assert.AreEqual("1234AB", identity.Identity.PostalCode);
        Assert.AreEqual("United States", identity.Identity.Country);
        Assert.AreEqual("ACME Corporation", identity.Identity.Company);
        Assert.AreEqual("user@example.com", identity.Identity.Email);
        Assert.AreEqual("+31612345678", identity.Identity.Phone);
        Assert.AreEqual("john.doe", identity.Identity.Username);
    }
}