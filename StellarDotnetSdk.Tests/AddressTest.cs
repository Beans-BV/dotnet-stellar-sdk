using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class AddressTest
{
    [TestMethod]
    public void TestAccountIdWithInvalidArgument()
    {
        const string invalidAccountId = "Invalid id";
        var ex = Assert.ThrowsException<ArgumentException>(() => new SCAccountId(invalidAccountId));
        Assert.IsTrue(ex.Message.Contains("Invalid account ID"));
    }

    [TestMethod]
    public void TestContractIdWithInvalidArgument()
    {
        const string invalidContractId = "Invalid id";
        var ex = Assert.ThrowsException<ArgumentException>(() => new SCContractId(invalidContractId));
        Assert.AreEqual("Invalid contract id (Parameter 'value')", ex.Message);
    }

    [TestMethod]
    public void TestAccountIdWithValidArgument()
    {
        var scAccountId = new SCAccountId("GCZFMH32MF5EAWETZTKF3ZV5SEVJPI53UEMDNSW55WBR75GMZJU4U573");

        // Act
        var scAccountIdXdrBase64 = scAccountId.ToXdrBase64();
        var fromXdrBase64ScAccountId = (SCAccountId)SCVal.FromXdrBase64(scAccountIdXdrBase64);

        // Assert
        Assert.AreEqual(scAccountId.InnerValue, fromXdrBase64ScAccountId.InnerValue);
    }

    [TestMethod]
    public void TestContractIdWithValidArgument()
    {
        var scContractId = new SCContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB");

        // Act
        var scContractIdXdrBase64 = scContractId.ToXdrBase64();
        var fromXdrBase64ScContractId = (SCContractId)SCVal.FromXdrBase64(scContractIdXdrBase64);

        // Assert
        Assert.AreEqual(scContractId.InnerValue, fromXdrBase64ScContractId.InnerValue);
    }
}