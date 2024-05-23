using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class CreateClaimableBalanceOperationResponseTest
{
    //Create Claimable Balance
    [TestMethod]
    public void TestSerializationCreateClaimableBalanceOperation()
    {
        var jsonPath = Utils.GetTestDataPath("createClaimableBalance.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertCreateClaimableBalanceData(back);
    }

    [TestMethod]
    public void TestSerializationCreateClaimableBalanceAbsBeforeMaxIntOperation()
    {
        var jsonPath = Utils.GetTestDataPath("createClaimableBalanceAbsBeforeMaxInt.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsTrue(instance is CreateClaimableBalanceOperationResponse);
        var operation = (CreateClaimableBalanceOperationResponse)instance;

        Assert.IsNotNull(operation.Claimants[0].Predicate.AbsBefore);
    }

    private static void AssertCreateClaimableBalanceData(OperationResponse instance)
    {
        Assert.IsTrue(instance is CreateClaimableBalanceOperationResponse);
        var operation = (CreateClaimableBalanceOperationResponse)instance;

        Assert.AreEqual(213223651414017, operation.Id);
        Assert.AreEqual("GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", operation.Sponsor);
        Assert.AreEqual("native", operation.Asset);
        Assert.AreEqual("1.0000000", operation.Amount);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", operation.Claimants[0].Destination);

        var back = new CreateClaimableBalanceOperationResponse
        {
            Sponsor = operation.Sponsor,
            Asset = operation.Asset,
            Amount = operation.Amount,
            Claimants = operation.Claimants,
        };
        Assert.IsNotNull(back);
    }
}