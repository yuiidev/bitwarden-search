using Community.PowerToys.Run.Plugin.Bitwarden;

namespace Tests;

[TestClass]
public sealed class BitwardenClientTests
{
    private const string ClientIdPath = @"C:\Users\LorenzoDemoniere\Documents\BitwardenSearch\clientid.txt";
    private const string ClientSecretPath = @"C:\Users\LorenzoDemoniere\Documents\BitwardenSearch\clientsecret.txt";
    private const string MasterPasswordPath = @"C:\Users\LorenzoDemoniere\Documents\BitwardenSearch\masterpassword.txt";

    [TestMethod]
    public void VerifySettings_Client_IsValid()
    {
        var client = new BitwardenClient();
        
        var valid = client.VerifySettings(ClientIdPath, ClientSecretPath, MasterPasswordPath);
        
        Assert.IsTrue(valid);
    }
    
    [TestMethod]
    public async Task Login_Client_IsSuccess()
    {
        var client = new BitwardenClient();

        var valid = client.VerifySettings(ClientIdPath, ClientSecretPath, MasterPasswordPath);
        Assert.IsTrue(valid);
        
        var actual = await client.Login();
        Assert.IsTrue(actual);
    }

    [TestMethod]
    public async Task Unlock_Client_IsSuccess()
    {
        var client = new BitwardenClient();
        
        var valid = client.VerifySettings(ClientIdPath, ClientSecretPath, MasterPasswordPath);

        var loggedIn = await client.Login();
        
        var actual = await client.Unlock();
        Assert.IsTrue(actual);
    }

    [TestMethod]
    public async Task Query_Client_IsSuccess()
    {
        var client = new BitwardenClient();
        
        client.VerifySettings(ClientIdPath, ClientSecretPath, MasterPasswordPath);
        await client.Login();
        await client.Unlock();

        await client.Query("");
        Assert.IsTrue(true);
    }
}