using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class ClaimClaimableBalanceOperationResponseTest
{
    //Claim Claimable Balance
    [TestMethod]
    public void TestSerializationClaimClaimableBalanceOperation()
    {
        var jsonPath = Utils.GetTestDataPath("claimClaimableBalance.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertClaimClaimableBalanceData(back);
    }

    private static void AssertClaimClaimableBalanceData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ClaimClaimableBalanceOperationResponse);
        var operation = (ClaimClaimableBalanceOperationResponse)instance;

        Assert.AreEqual(214525026504705, operation.Id);
        Assert.AreEqual("00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7",
            operation.BalanceID);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Claimant);
        Assert.IsNull(operation.ClaimantMuxed);
        Assert.IsNull(operation.ClaimantMuxedID);

        var back = new ClaimClaimableBalanceOperationResponse
        {
            BalanceID = operation.BalanceID,
            Claimant = operation.Claimant,
        };
        Assert.IsNotNull(back);
    }

    //Claim Claimable Balance (Muxed)
    [TestMethod]
    public void TestSerializationClaimClaimableBalanceOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("claimClaimableBalanceMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertClaimClaimableBalanceDataMuxed(back);
    }

    private static void AssertClaimClaimableBalanceDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is ClaimClaimableBalanceOperationResponse);
        var operation = (ClaimClaimableBalanceOperationResponse)instance;

        Assert.AreEqual(214525026504705, operation.Id);
        Assert.AreEqual("00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7",
            operation.BalanceID);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Claimant);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24",
            operation.ClaimantMuxed);
        Assert.AreEqual(5123456789UL, operation.ClaimantMuxedID);

        var back = new ClaimClaimableBalanceOperationResponse
        {
            BalanceID = operation.BalanceID,
            Claimant = operation.Claimant,
        };
        Assert.IsNotNull(back);
    }
}