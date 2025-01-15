namespace Community.PowerToys.Run.Plugin.Bitwarden.Tests;

[TestClass]
public sealed class MainTests
{
    [TestMethod]
    public async Task AttemptStart_Main_DoesNotThrow()
    {
        var main = new Main(
            "C:\\Users\\LorenzoDemoniere\\Documents\\BitwardenSearch\\clientid.txt",
            "C:\\Users\\LorenzoDemoniere\\Documents\\BitwardenSearch\\clientsecret.txt",
            "C:\\Users\\LorenzoDemoniere\\Documents\\BitwardenSearch\\masterpassword.txt",
            8087
        );

        await main.AttemptStart();
        main.Stop();
        
        // There is no DoesNotThrow
        Assert.IsTrue(true);
    }
}