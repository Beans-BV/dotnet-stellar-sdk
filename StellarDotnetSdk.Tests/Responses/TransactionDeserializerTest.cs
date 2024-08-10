using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class TransactionDeserializerTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("transactionTransaction.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSingleton.GetInstance<TransactionResponse>(json);

        Assert.IsNotNull(transaction);
        AssertTestData(transaction);
        Assert.AreEqual(100L, transaction.FeeCharged);
        Assert.AreEqual(1050L, transaction.MaxFee);
    }

    [TestMethod]
    public void TestDeserializeOfVersionBefore020()
    {
        var jsonPath = Utils.GetTestDataPath("transactionTransactionPre020.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSingleton.GetInstance<TransactionResponse>(json);

        Assert.IsNotNull(transaction);
        AssertTestData(transaction);
        Assert.AreEqual(0L, transaction.FeeCharged);
        Assert.AreEqual(0L, transaction.MaxFee);
    }

    [TestMethod]
    public void TestDeserializeWithTextMemo()
    {
        var jsonPath = Utils.GetTestDataPath("transactionTransactionTextMemo.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSingleton.GetInstance<TransactionResponse>(json);

        Assert.IsNotNull(transaction);
        var memo = (MemoText)transaction.Memo;
        Assert.IsNotNull(memo);
        var encoded = Convert.ToBase64String(memo.MemoBytesValue);
        Assert.AreEqual("6CI8cn49WnAW/uvPOJ2befbuacU=", encoded);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("transactionTransaction.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSingleton.GetInstance<TransactionResponse>(json);
        var serialized = JsonConvert.SerializeObject(transaction);
        var back = JsonConvert.DeserializeObject<TransactionResponse>(serialized);
        Assert.IsNotNull(back);
        Assert.IsTrue(back.Successful);
        AssertTestData(back);

        Assert.IsNotNull(transaction);
        Assert.AreEqual(100L, transaction.FeeCharged);
        Assert.AreEqual(1050L, transaction.MaxFee);
    }

    public static void AssertTestData(TransactionResponse transaction)
    {
        Assert.AreEqual("5c2e4dad596941ef944d72741c8f8f1a4282f8f2f141e81d827f44bf365d626b", transaction.Hash);
        Assert.AreEqual(915744L, transaction.Ledger);
        Assert.AreEqual("2015-11-20T17:01:28Z", transaction.CreatedAt);
        Assert.AreEqual("3933090531512320", transaction.PagingToken);
        Assert.AreEqual("GCUB7JL4APK7LKJ6MZF7Q2JTLHAGNBIUA7XIXD5SQTG52GQ2DAT6XZMK", transaction.SourceAccount);
        Assert.AreEqual(2373051035426646L, transaction.SourceAccountSequence);
        Assert.AreEqual(1, transaction.OperationCount);
        Assert.AreEqual(
            "AAAAAKgfpXwD1fWpPmZL+GkzWcBmhRQH7ouPsoTN3RoaGCfrAAAAZAAIbkcAAB9WAAAAAAAAAANRBBZE6D1qyGjISUGLY5Ldvp31PwAAAAAAAAAAAAAAAAAAAAEAAAABAAAAAP1qe44j+i4uIT+arbD4QDQBt8ryEeJd7a0jskQ3nwDeAAAAAAAAAADA7RnarSzCwj3OT+M2btCMFpVBdqxJS+Sr00qBjtFv7gAAAABLCs/QAAAAAAAAAAEaGCfrAAAAQG/56Cj2J8W/KCZr+oC4sWND1CTGWfaccHNtuibQH8kZIb+qBSDY94g7hiaAXrlIeg9b7oz/XuP3x9MWYw2jtwM=",
            transaction.EnvelopeXdr);
        Assert.AreEqual("AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAA=", transaction.ResultXdr);
        Assert.AreEqual(
            "AAAAAAAAAAEAAAACAAAAAAAN+SAAAAAAAAAAAMDtGdqtLMLCPc5P4zZu0IwWlUF2rElL5KvTSoGO0W/uAAAAAEsKz9AADfkgAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAQAN+SAAAAAAAAAAAP1qe44j+i4uIT+arbD4QDQBt8ryEeJd7a0jskQ3nwDeAAHp6WMr55YACD1BAAAAHgAAAAoAAAAAAAAAAAAAAAABAAAAAAAACgAAAAARC07BokpLTOF+/vVKBwiAlop7hHGJTNeGGlY4MoPykwAAAAEAAAAAK+Lzfd3yDD+Ov0GbYu1g7SaIBrKZeBUxoCunkLuI7aoAAAABAAAAAERmsKL73CyLV/HvjyQCERDXXpWE70Xhyb6MR5qPO3yQAAAAAQAAAABSORGwAdyuanN3sNOHqNSpACyYdkUM3L8VafUu69EvEgAAAAEAAAAAeCzqJNkMM/jLvyuMIfyFHljBlLCtDyj17RMycPuNtRMAAAABAAAAAIEi4R7juq15ymL00DNlAddunyFT4FyUD4muC4t3bobdAAAAAQAAAACaNpLL5YMfjOTdXVEqrAh99LM12sN6He6pHgCRAa1f1QAAAAEAAAAAqB+lfAPV9ak+Zkv4aTNZwGaFFAfui4+yhM3dGhoYJ+sAAAABAAAAAMNJrEvdMg6M+M+n4BDIdzsVSj/ZI9SvAp7mOOsvAD/WAAAAAQAAAADbHA6xiKB1+G79mVqpsHMOleOqKa5mxDpP5KEp/Xdz9wAAAAEAAAAAAAAAAA==",
            transaction.ResultMetaXdr);

        Assert.IsTrue(transaction.Memo is MemoHash);
        var memo = (MemoHash)transaction.Memo;
        Assert.AreEqual("51041644e83d6ac868c849418b6392ddbe9df53f000000000000000000000000", memo.GetHexValue());

        Assert.AreEqual("/accounts/GCUB7JL4APK7LKJ6MZF7Q2JTLHAGNBIUA7XIXD5SQTG52GQ2DAT6XZMK",
            transaction.Links.Account.Href);
        Assert.AreEqual(
            "/transactions/5c2e4dad596941ef944d72741c8f8f1a4282f8f2f141e81d827f44bf365d626b/effects{?cursor,limit,order}",
            transaction.Links.Effects.Href);
        Assert.AreEqual("/ledgers/915744", transaction.Links.Ledger.Href);
        Assert.AreEqual(
            "/transactions/5c2e4dad596941ef944d72741c8f8f1a4282f8f2f141e81d827f44bf365d626b/operations{?cursor,limit,order}",
            transaction.Links.Operations.Href);
        Assert.AreEqual("/transactions?cursor=3933090531512320&order=asc", transaction.Links.Precedes.Href);
        Assert.AreEqual("/transactions/5c2e4dad596941ef944d72741c8f8f1a4282f8f2f141e81d827f44bf365d626b",
            transaction.Links.Self.Href);
        Assert.AreEqual("/transactions?cursor=3933090531512320&order=desc", transaction.Links.Succeeds.Href);
    }

    [TestMethod]
    public void TestDeserializeWithoutMemo()
    {
        var jsonPath = Utils.GetTestDataPath("transactionTransactionWithoutMemo.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSingleton.GetInstance<TransactionResponse>(json);

        Assert.IsNotNull(transaction);
        Assert.IsFalse(transaction.Successful);
        Assert.IsTrue(transaction.Memo is MemoNone);
    }

    [TestMethod]
    public void TestDeserializeWithMemoText()
    {
        var jsonPath = Utils.GetTestDataPath("transactionTransactionWithMemo.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSingleton.GetInstance<TransactionResponse>(json);
        Assert.IsNotNull(transaction);
        var copyTransaction = new TransactionResponse
        {
            Hash = transaction.Hash,
            Ledger = transaction.Ledger,
            CreatedAt = transaction.CreatedAt,
            SourceAccount = transaction.SourceAccount,
            FeeAccount = transaction.FeeAccount,
            Successful = transaction.Successful,
            SourceAccountSequence = transaction.SourceAccountSequence,
            FeeCharged = transaction.FeeCharged,
            MaxFee = transaction.MaxFee,
            OperationCount = transaction.OperationCount,
            EnvelopeXdr = transaction.EnvelopeXdr,
            ResultXdr = transaction.ResultXdr,
            ResultMetaXdr = transaction.ResultMetaXdr,
            Signatures = transaction.Signatures,
            FeeBumpTx = transaction.FeeBumpTx,
            InnerTx = transaction.InnerTx,
            Links = transaction.Links,
            Memo = transaction.Memo,
            PagingToken = transaction.PagingToken,
        };

        Assert.AreEqual(transaction.MemoValue, copyTransaction.MemoValue);
        Assert.IsFalse(transaction.Successful);
    }

    [TestMethod]
    public void TestDeserializeTransactionPreProtocol13()
    {
        var jsonPath = Utils.GetTestDataPath("transactionTransaction.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSingleton.GetInstance<TransactionResponse>(json);

        Assert.IsNotNull(transaction);

        var transaction2 = new TransactionResponse
        {
            Hash = transaction.Hash,
            Ledger = transaction.Ledger,
            CreatedAt = transaction.CreatedAt,
            SourceAccount = transaction.SourceAccount,
            Successful = transaction.Successful,
            SourceAccountSequence = transaction.SourceAccountSequence,
            FeeCharged = transaction.FeeCharged,
            OperationCount = transaction.OperationCount,
            EnvelopeXdr = transaction.EnvelopeXdr,
            ResultXdr = transaction.ResultXdr,
            ResultMetaXdr = transaction.ResultMetaXdr,
            Links = transaction.Links,
            Memo = transaction.Memo,
            PagingToken = transaction.PagingToken,
        };

        Assert.AreEqual(transaction.Hash, transaction2.Hash);
        Assert.AreEqual(transaction.Ledger, transaction2.Ledger);
        Assert.AreEqual(transaction.Successful, transaction.Successful);
        Assert.AreEqual(transaction.SourceAccount, transaction2.SourceAccount);
        Assert.AreEqual(transaction.FeeAccount, transaction2.FeeAccount);
        Assert.AreEqual(transaction.SourceAccountSequence, transaction2.SourceAccountSequence);
        Assert.AreEqual(transaction.FeeCharged, transaction2.FeeCharged);
        Assert.AreEqual(transaction.OperationCount, transaction2.OperationCount);
    }

    [TestMethod]
    public void TestDeserializeFeeBump()
    {
        var jsonPath = Utils.GetTestDataPath("transactionFeeBump.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSingleton.GetInstance<TransactionResponse>(json);
        Assert.IsNotNull(transaction);
        var transaction2 = new TransactionResponse
        {
            Hash = transaction.Hash,
            Ledger = transaction.Ledger,
            CreatedAt = transaction.CreatedAt,
            SourceAccount = transaction.SourceAccount,
            FeeAccount = transaction.FeeAccount,
            Successful = transaction.Successful,
            PagingToken = transaction.PagingToken,
            SourceAccountSequence = transaction.SourceAccountSequence,
            MaxFee = transaction.MaxFee,
            FeeCharged = 123L,
            OperationCount = transaction.OperationCount,
            EnvelopeXdr = transaction.EnvelopeXdr,
            ResultXdr = transaction.ResultXdr,
            ResultMetaXdr = transaction.ResultMetaXdr,
            Memo = transaction.Memo,
            Signatures = transaction.Signatures,
            FeeBumpTx = transaction.FeeBumpTx,
            InnerTx = transaction.InnerTx,
            Links = transaction.Links,
        };


        Assert.AreEqual(transaction.Hash, transaction2.Hash);
        Assert.AreEqual(transaction.Ledger, transaction2.Ledger);
        Assert.AreEqual(transaction.Successful, transaction.Successful);
        Assert.AreEqual(transaction.SourceAccount, transaction2.SourceAccount);
        Assert.AreEqual(transaction.FeeAccount, transaction2.FeeAccount);
        Assert.AreEqual(transaction.SourceAccountSequence, transaction2.SourceAccountSequence);
        Assert.AreEqual(transaction.MaxFee, transaction2.MaxFee);
        Assert.AreEqual(transaction.FeeCharged, transaction2.FeeCharged);
        Assert.AreEqual(transaction.OperationCount, transaction2.OperationCount);
        CollectionAssert.AreEqual(transaction.Signatures, new List<string> { "Hh4e" });

        var feeBumpTransaction = transaction.FeeBumpTx;
        Assert.AreEqual(feeBumpTransaction.Hash, "3dfef7d7226995b504f2827cc63d45ad41e9687bb0a8abcf08ba755fedca0352");
        CollectionAssert.AreEqual(feeBumpTransaction.Signatures, new List<string> { "Hh4e" });

        var innerTransaction = transaction.InnerTx;
        Assert.AreEqual(innerTransaction.Hash, "e98869bba8bce08c10b78406202127f3888c25454cd37b02600862452751f526");
        Assert.AreEqual(innerTransaction.MaxFee, 99L);
        CollectionAssert.AreEqual(innerTransaction.Signatures, new List<string> { "FBQU" });
    }

    [TestMethod]
    public void TestDeserializeMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("transactionMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSingleton.GetInstance<TransactionResponse>(json);

        Assert.IsNotNull(transaction);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", transaction.SourceAccount);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24",
            transaction.AccountMuxed);
        Assert.AreEqual(5123456789UL, transaction.AccountMuxedID);

        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", transaction.FeeAccount);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24",
            transaction.FeeAccountMuxed);
        Assert.AreEqual(5123456789UL, transaction.FeeAccountMuxedID);
    }
}