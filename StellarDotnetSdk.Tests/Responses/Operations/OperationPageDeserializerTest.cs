using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class OperationPageDeserializerTest
{
    private readonly string _getTestDataPath = Utils.GetTestDataPath("Responses/operationPage.json");

    [TestMethod]
    public void TestDeserialize()
    {
        var json = File.ReadAllText(_getTestDataPath);
        var operationsPage = JsonSingleton.GetInstance<Page<OperationResponse>>(json);
        Assert.IsNotNull(operationsPage);
        AssertTestData(operationsPage);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var json = File.ReadAllText(_getTestDataPath);
        var operationsPage = JsonSingleton.GetInstance<Page<OperationResponse>>(json);
        var serialized = JsonConvert.SerializeObject(operationsPage, new OperationResponseJsonConverter());
        var back = JsonConvert.DeserializeObject<Page<OperationResponse>>(serialized,
            new OperationResponseJsonConverter());
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    public static void AssertTestData(Page<OperationResponse> operationsPage)
    {
        var createAccountOperation = (CreateAccountOperationResponse)operationsPage.Records[0];
        Assert.AreEqual(createAccountOperation.StartingBalance, "10000.0");
        Assert.AreEqual(createAccountOperation.PagingToken, "3717508943056897");
        Assert.AreEqual(createAccountOperation.Account, "GDFH4NIYMIIAKRVEJJZOIGWKXGQUF3XHJG6ZM6CEA64AMTVDN44LHOQE");
        Assert.AreEqual(createAccountOperation.Funder, "GBS43BF24ENNS3KPACUZVKK2VYPOZVBQO2CISGZ777RYGOPYC2FT6S3K");

        var paymentOperation = (PaymentOperationResponse)operationsPage.Records[4];
        Assert.AreEqual(paymentOperation.Amount, "10.123");
        Assert.AreEqual(paymentOperation.Asset, new AssetTypeNative());
        Assert.AreEqual(paymentOperation.From, "GCYK67DDGBOANS6UODJ62QWGLEB2A7JQ3XUV25HCMLT7CI23PMMK3W6R");
        Assert.AreEqual(paymentOperation.To, "GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H");
    }
}