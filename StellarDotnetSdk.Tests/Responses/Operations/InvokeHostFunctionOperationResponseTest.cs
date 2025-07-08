using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;
using MuxedIdType =
    StellarDotnetSdk.Responses.Operations.InvokeHostFunctionOperationResponse.AssetContractBalanceChange.MuxedIdType;

namespace StellarDotnetSdk.Tests.Responses.Operations;

public class InvokeHostFunctionOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeInvokeHostFunctionOperation()
    {
        var jsonPath = Utils.GetTestDataPath("invokeHostFunction.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeInvokeHostFunctionOperation()
    {
        var jsonPath = Utils.GetTestDataPath("invokeHostFunction.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertData(back);
    }

    private void AssertData(OperationResponse operationResponse)
    {
        Assert.IsTrue(operationResponse is InvokeHostFunctionOperationResponse);
        var operation = (InvokeHostFunctionOperationResponse)operationResponse;

        Assert.AreEqual(2224793063425L, operation.Id);
        Assert.AreEqual("2224793063425", operation.PagingToken);
        var parameters = operation.Parameters;
        Assert.IsNotNull(parameters);
        Assert.AreEqual(5, parameters.Length);
        Assert.AreEqual("Address", parameters[0].Type);
        Assert.AreEqual("AAAAEgAAAAGw7oy+G8a9SeTIE5E/EuJYl5JfwF0eZJWk8S7LmE7fwA==", parameters[0].Value);
        Assert.AreEqual("Sym", parameters[1].Type);
        Assert.AreEqual("AAAADwAAAAh0cmFuc2Zlcg==", parameters[1].Value);
        Assert.AreEqual("Address", parameters[2].Type);
        Assert.AreEqual("AAAAEgAAAAAAAAAAwT6e0zIpycpZ5/unUFyQAjXNeSxfmidj8tQWkeD9dCQ=", parameters[2].Value);
        Assert.AreEqual("Address", parameters[3].Type);
        Assert.AreEqual("AAAAEgAAAAAAAAAAWLfEosjyl6qPPSRxKB/fzOyv5I5WYzE+wY4Spz7KmKE=", parameters[3].Value);
        Assert.AreEqual("I128", parameters[4].Type);
        Assert.AreEqual("4ef3d81fba4b7db959080e4894cb8b2575418b8da9aa484f6306a79a3f63de3d", operation.TransactionHash);
        Assert.AreEqual(24, operation.TypeId);
        Assert.AreEqual("invoke_host_function", operation.Type);
        Assert.AreEqual("GDAT5HWTGIU4TSSZ4752OUC4SABDLTLZFRPZUJ3D6LKBNEPA7V2CIG54", operation.SourceAccount);
        Assert.AreEqual("456", operation.Salt);
        Assert.AreEqual("123", operation.Address);
        Assert.AreEqual("2224793063425", operation.Function);
        var balanceChanges = operation.AssetBalanceChanges;
        Assert.IsNotNull(balanceChanges);
        Assert.AreEqual(1, balanceChanges.Length);
        var balanceChange = balanceChanges[0];
        Assert.IsNotNull(balanceChange);
        Assert.AreEqual(
            Asset.CreateNonNativeAsset("HELLO", "GDJKBIYIPBE2NC5XIZX6GCFZHVWFUA7ONMQUOOVTLIM3BESTI4BYADAN"),
            balanceChange.Asset);
        Assert.AreEqual("500.0000000", balanceChange.Amount);
        Assert.AreEqual("HELLO", balanceChange.AssetCode);
        Assert.AreEqual("GDJKBIYIPBE2NC5XIZX6GCFZHVWFUA7ONMQUOOVTLIM3BESTI4BYADAN", balanceChange.AssetIssuer);
        Assert.AreEqual("credit_alphanum12", balanceChange.AssetType);
        Assert.AreEqual(MuxedIdType.UINT64, balanceChange.DestinationMuxedIdType);
        Assert.AreEqual("123456789", balanceChange.DestinationMuxedId);
    }
}