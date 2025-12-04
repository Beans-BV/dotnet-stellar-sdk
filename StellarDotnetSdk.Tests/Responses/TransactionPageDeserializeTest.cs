using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class TransactionPageDeserializeTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("transactionPage.json");
        var json = File.ReadAllText(jsonPath);
        var transactionsPage = JsonSerializer.Deserialize<Page<TransactionResponse>>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(transactionsPage);
        AssertTestData(transactionsPage);
    }

    public static void AssertTestData(Page<TransactionResponse> transactionsPage)
    {
        Assert.AreEqual(5, transactionsPage.Records.Count);

        var record1 = transactionsPage.Records[0];
        Assert.AreEqual("b9d0b2292c4e09e8eb22d036171491e87b8d2086bf8b265874c8d182cb9c9020", record1.Id);
        Assert.AreEqual("794568953856", record1.PagingToken);
        Assert.AreEqual(true, record1.Successful);
        Assert.AreEqual("b9d0b2292c4e09e8eb22d036171491e87b8d2086bf8b265874c8d182cb9c9020", record1.Hash);
        Assert.AreEqual(185L, record1.Ledger);
        Assert.AreEqual("2025-08-14T17:38:24Z", record1.CreatedAt);
        Assert.AreEqual("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H", record1.SourceAccount);
        Assert.AreEqual(1L, record1.SourceAccountSequence);
        Assert.AreEqual("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H", record1.FeeAccount);
        Assert.AreEqual(1100L, record1.FeeCharged);
        Assert.AreEqual(1100L, record1.MaxFee);
        Assert.AreEqual(11, record1.OperationCount);

        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions?cursor=&limit=5&order=asc",
            transactionsPage.Links.Self.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions?cursor=794568953856&limit=5&order=desc",
            transactionsPage.Links.Prev.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions?cursor=957777711104&limit=5&order=asc",
            transactionsPage.Links.Next.Href);

        var record5 = transactionsPage.Records[4];
        Assert.AreEqual("57075101b7272595f5ec1f1625e4be4c0cd9d774ae5f13bbd3295f1578420fbf", record5.Id);
        Assert.AreEqual("957777711104", record5.PagingToken);
        Assert.AreEqual(true, record5.Successful);
        Assert.AreEqual("57075101b7272595f5ec1f1625e4be4c0cd9d774ae5f13bbd3295f1578420fbf", record5.Hash);
        Assert.AreEqual(223L, record5.Ledger);
        Assert.AreEqual("2025-08-14T17:41:34Z", record5.CreatedAt);
        Assert.AreEqual("GAIH3ULLFQ4DGSECF2AR555KZ4KNDGEKN4AFI4SU2M7B43MGK3QJZNSR", record5.SourceAccount);
        Assert.AreEqual(794568949764L, record5.SourceAccountSequence);
        Assert.AreEqual("GAIH3ULLFQ4DGSECF2AR555KZ4KNDGEKN4AFI4SU2M7B43MGK3QJZNSR", record5.FeeAccount);
        Assert.AreEqual(2000L, record5.FeeCharged);
        Assert.AreEqual(2000L, record5.MaxFee);
        Assert.AreEqual(20, record5.OperationCount);
    }
}