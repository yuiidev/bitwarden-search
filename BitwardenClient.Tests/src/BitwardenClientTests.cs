namespace YuiiDev.Bitwarden.Tests;

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

        client.VerifySettings(ClientIdPath, ClientSecretPath, MasterPasswordPath);
        
        var actual = await client.Login();
        
        Assert.IsTrue(actual);
    }

    [TestMethod]
    public async Task StartStop_Client_IsSuccess()
    {
        var client = new BitwardenClient();
        
        client.VerifySettings(ClientIdPath, ClientSecretPath, MasterPasswordPath);
        await client.Login();
        client.Start();
        client.Stop();
    }

    [TestMethod]
    public async Task Unlock_Client_IsSuccess()
    {
        var client = new BitwardenClient();
        
        client.VerifySettings(ClientIdPath, ClientSecretPath, MasterPasswordPath);
        await client.Login();
        client.Start();
        
        var actual = await client.Unlock();
        Assert.IsTrue(actual);
        
        client.Stop();
    }

    [TestMethod]
    public async Task Query_Client_IsSuccess()
    {
        var client = new BitwardenClient();
        
        client.VerifySettings(ClientIdPath, ClientSecretPath, MasterPasswordPath);
        await client.Login();
        client.Start();
        await client.Unlock();
        
        var response = await client.Query("github");
        client.Stop();
        
        Assert.IsFalse(response.IsEmpty);
        Assert.IsTrue(response.Success);
    }

    [TestMethod]
    public async Task Sync_Client_IsSuccess()
    {
        var client = new BitwardenClient();
        
        client.VerifySettings(ClientIdPath, ClientSecretPath, MasterPasswordPath);
        await client.Login();
        client.Start();
        var actual = await client.Sync();
        client.Stop();
        
        Assert.IsTrue(actual);
        
    }
}