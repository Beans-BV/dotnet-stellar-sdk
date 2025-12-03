using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class TransactionDeserializerTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("transaction.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSerializer.Deserialize<TransactionResponse>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(transaction);
        AssertTransaction(transaction);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("transaction.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSerializer.Deserialize<TransactionResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(transaction);
        var back = JsonSerializer.Deserialize<TransactionResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTransaction(back);
    }

    public static void AssertTransaction(TransactionResponse transaction)
    {
        Assert.AreEqual("de5001a7b15240dedb08ced3c384c8b7f449eed67c2fea7c2d8734efe07646bc", transaction.Id);
        Assert.AreEqual("de5001a7b15240dedb08ced3c384c8b7f449eed67c2fea7c2d8734efe07646bc", transaction.Hash);
        Assert.AreEqual(true, transaction.Successful);
        Assert.AreEqual(220L, transaction.Ledger);

        Assert.AreEqual("2025-08-14T17:41:19Z", transaction.CreatedAt);
        Assert.AreEqual("944892809216", transaction.PagingToken);
        Assert.AreEqual("GAIH3ULLFQ4DGSECF2AR555KZ4KNDGEKN4AFI4SU2M7B43MGK3QJZNSR", transaction.SourceAccount);
        Assert.AreEqual(794568949761L, transaction.SourceAccountSequence);
        Assert.AreEqual(20, transaction.OperationCount);
        Assert.AreEqual("GAIH3ULLFQ4DGSECF2AR555KZ4KNDGEKN4AFI4SU2M7B43MGK3QJZNSR", transaction.FeeAccount);
        Assert.AreEqual(2000L, transaction.FeeCharged);
        Assert.AreEqual(2000L, transaction.MaxFee);
        Assert.AreEqual(
            "AAAAAgAAAAAQfdFrLDgzSIIugR73qs8U0ZiKbwBUclTTPh5thlbgnAAAB9AAAAC5AAAAAQAAAAEAAAAAAAAAAAAAAABoniDoAAAAAAAAABQAAAAAAAAAAAAAAAC82Kyaig4NS1cu0imiYSqeOj761GSrtditj8FaCCzwywAAAAA8M2CAAAAAAAAAAAAAAAAAHGxZhDtghkvWf+ywIHgVAeuZrFN8w+TQbhkO4sUBdIMAAAAAPDNggAAAAAAAAAAAAAAAAKYzkX9fj8Cb8novrFZON7XXbcvL6CUTUCX0HLuq9okHAAAAADwzYIAAAAAAAAAAAAAAAABid1J1OGwCocYJi9ytvjZ0sJ9g07KnYyV0HTNdCMmlDQAAAAA8M2CAAAAAAAAAAAAAAAAA4gi5I6vwXeb4ENWqVa6WK0ULerMJMhGlnAKO97uaWfEAAAAAPDNggAAAAAAAAAAAAAAAAMXUOalfhI6o6TvFbQYsWCa2TdQ8ElqoIEdFv6ccCgy6AAAAADwzYIAAAAAAAAAAAAAAAACZ3MrQMkk6nAgU7JuGIoz+Tv1/ur7kYXEqUL/9mAC7CgAAAAA8M2CAAAAAAAAAAAAAAAAAtN9gdv5uDusEe7/2YzvqEWHw6CS+SRvNIXVSIA7041oAAAAAPDNggAAAAAAAAAAAAAAAAAQT8ngggF7WhKWppwle1oHCc61COgCFBT4RNYbxhaEkAAAAADwzYIAAAAAAAAAAAAAAAABLbuK6to7eV8HMuT/Xp+ImRYvZ8EZ+vH3qe4h7qG1zxgAAAAA8M2CAAAAAAAAAAAAAAAAAwl9vMuA8xXgSKIX3x3wpX/CpCPp3qE7UtNYDz3TP/b8AAAAAPDNggAAAAAAAAAAAAAAAAIAUty0/bQgUSFuoyaaZhJnlnUNdcf06e8izbNMmo629AAAAADwzYIAAAAAAAAAAAAAAAAC1BtdFikSebxUCEyuJLeHXhpCUGVza7cHCC7Bir2rcbAAAAAA8M2CAAAAAAAAAAAAAAAAA7fQfjSPrkREpzSMENwsW7F+hLD9NAawK7hDHOlhm3d8AAAAAPDNggAAAAAAAAAAAAAAAAEbogi/zJsB10K+A20p1ukxmt2X6nHO/ptjnXs+ePiHmAAAAADwzYIAAAAAAAAAAAAAAAABWf/GOSA2F0sI2Lk0ICbYQvXclh4pwVP73vFZh2JZm3gAAAAA8M2CAAAAAAAAAAAAAAAAA38tYe4bmXF9yxowOUPi6BeFhHYAvK0N1z8Y2UZpo2xAAAAAAPDNggAAAAAAAAAAAAAAAAKlv1esnasUbdd3zRCsLf9EEYfj0YB62O7iu2zhZGywTAAAAADwzYIAAAAAAAAAAAAAAAAB+NReduvXXjHqRftGfV+znDLUWUmIqp4FpLSv/NuXNMwAAAAA8M2CAAAAAAAAAAAAAAAAAh5DwzFYtFrvWaYtOyZm2PYFQm9T/jyue3y7eGQKVDRwAAAAAPDNggAAAAAAAAAABhlbgnAAAAECPWpjBffwUuvkS6QYRtvzvRx3PkIQfiXpzLKRrlv2BnvpApMUyNIxM2uuf4L9XlSVEtANM8ozdVpOiUrLnODEC",
            transaction.EnvelopeXdr);
        Assert.AreEqual(
            "AAAAAAAAB9AAAAAAAAAAFAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
            transaction.ResultXdr);
        Assert.AreEqual(
            "AAAAAgAAAAMAAAC5AAAAAAAAAAAQfdFrLDgzSIIugR73qs8U0ZiKbwBUclTTPh5thlbgnAFjRXhdigAAAAAAuQAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAEAAADcAAAAAAAAAAAQfdFrLDgzSIIugR73qs8U0ZiKbwBUclTTPh5thlbgnAFjRXhdifgwAAAAuQAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAA==",
            transaction.FeeMetaXdr);
        Assert.IsTrue(transaction.Memo is MemoNone);

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GAIH3ULLFQ4DGSECF2AR555KZ4KNDGEKN4AFI4SU2M7B43MGK3QJZNSR",
            transaction.Links.Account.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/de5001a7b15240dedb08ced3c384c8b7f449eed67c2fea7c2d8734efe07646bc/effects{?cursor,limit,order}",
            transaction.Links.Effects.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/220",
            transaction.Links.Ledger.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/de5001a7b15240dedb08ced3c384c8b7f449eed67c2fea7c2d8734efe07646bc/operations{?cursor,limit,order}",
            transaction.Links.Operations.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions?order=asc&cursor=944892809216",
            transaction.Links.Precedes.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/de5001a7b15240dedb08ced3c384c8b7f449eed67c2fea7c2d8734efe07646bc",
            transaction.Links.Self.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions?order=desc&cursor=944892809216",
            transaction.Links.Succeeds.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/de5001a7b15240dedb08ced3c384c8b7f449eed67c2fea7c2d8734efe07646bc",
            transaction.Links.Transaction.Href);

        Assert.IsNull(transaction.AccountMuxed);
        Assert.IsNull(transaction.AccountMuxedId);
        Assert.IsNull(transaction.FeeAccountMuxed);
        Assert.IsNull(transaction.FeeAccountMuxedId);
        Assert.IsNull(transaction.InnerTx);
        Assert.IsNull(transaction.FeeBumpTx);
        Assert.IsNull(transaction.MemoBytes);

        Assert.AreEqual(
            "j1qYwX38FLr5EukGEbb870cdz5CEH4l6cyyka5b9gZ76QKTFMjSMTNrrn+C/V5UlRLQDTPKM3VaTolKy5zgxAg==",
            transaction.Signatures[0]);
        Assert.IsNotNull(transaction.Preconditions);
        Assert.AreEqual("0", transaction.Preconditions.TimeBounds.MinTime);
        Assert.AreEqual("1755193576", transaction.Preconditions.TimeBounds.MaxTime);

        Assert.IsNull(transaction.Preconditions.ExtraSigners);
        Assert.IsNull(transaction.Preconditions.LedgerBounds);
        Assert.IsNull(transaction.Preconditions.MinAccountSequence);
        Assert.IsNull(transaction.Preconditions.MinAccountSequenceAge);
        Assert.IsNull(transaction.Preconditions.MinAccountSequenceLedgerGap);
    }

    private static void AssertTransactionMuxedAccount(TransactionResponse transaction)
    {
        Assert.AreEqual("f08b48818071da17668aefe815597ea76aad825247ed077211d25b5a3699f26a", transaction.Id);
        Assert.AreEqual("f08b48818071da17668aefe815597ea76aad825247ed077211d25b5a3699f26a", transaction.Hash);
        Assert.AreEqual(1874918L, transaction.Ledger);
        Assert.AreEqual(true, transaction.Successful);
        Assert.AreEqual("2025-12-01T09:39:58Z", transaction.CreatedAt);
        Assert.AreEqual("8052711492685824", transaction.PagingToken);
        Assert.AreEqual("GBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RBL5S", transaction.SourceAccount);
        Assert.AreEqual("MBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RAAAAAAAAAAAAAH52S",
            transaction.AccountMuxed);
        Assert.AreEqual(1UL, transaction.AccountMuxedId);
        Assert.AreEqual(8052569758760961L, transaction.SourceAccountSequence);
        Assert.AreEqual(1, transaction.OperationCount);
        Assert.AreEqual("GBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RBL5S", transaction.FeeAccount);
        Assert.AreEqual("MBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RAAAAAAAAAAAAAH52S",
            transaction.FeeAccountMuxed);
        Assert.AreEqual(1UL, transaction.FeeAccountMuxedId);
        Assert.AreEqual(100L, transaction.FeeCharged);
        Assert.AreEqual(100L, transaction.MaxFee);
        Assert.AreEqual(
            "AAAAAgAAAQAAAAAAAAAAAXJtyYFfivXjDYP8NlcnGcQ0R4nhsRMonQ4jVYcN37kQAAAAZAAcm8UAAAABAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAABAAAAABB90WssODNIgi6BHveqzxTRmIpvAFRyVNM+Hm2GVuCcAAAAAAAAAAAGQixAAAAAAAAAAAEN37kQAAAAQFZA+QwGyVZ5SaipitDr/SDqGaz6VFz/SElnqbUT96bVFBO4OiJ/5E5cty/8j7Sqba3Y3CKiRJDM0U/r8uhROQ4=",
            transaction.EnvelopeXdr);
        Assert.AreEqual(
            "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAABAAAAAAAAAAA=",
            transaction.ResultXdr);
        Assert.AreEqual(
            "AAAAAgAAAAMAHJvFAAAAAAAAAABybcmBX4r14w2D/DZXJxnENEeJ4bETKJ0OI1WHDd+5EAAAABdIdugAABybxQAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAEAHJvmAAAAAAAAAABybcmBX4r14w2D/DZXJxnENEeJ4bETKJ0OI1WHDd+5EAAAABdIduecABybxQAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAA==",
            transaction.FeeMetaXdr);
        Assert.IsTrue(transaction.Memo is MemoNone);

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts/GBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RBL5S",
            transaction.Links.Account.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/f08b48818071da17668aefe815597ea76aad825247ed077211d25b5a3699f26a/effects{?cursor,limit,order}",
            transaction.Links.Effects.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/1874918",
            transaction.Links.Ledger.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/f08b48818071da17668aefe815597ea76aad825247ed077211d25b5a3699f26a/operations{?cursor,limit,order}",
            transaction.Links.Operations.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions?order=asc&cursor=8052711492685824",
            transaction.Links.Precedes.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/f08b48818071da17668aefe815597ea76aad825247ed077211d25b5a3699f26a",
            transaction.Links.Self.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/transactions?order=desc&cursor=8052711492685824",
            transaction.Links.Succeeds.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/transactions/f08b48818071da17668aefe815597ea76aad825247ed077211d25b5a3699f26a",
            transaction.Links.Transaction.Href);

        Assert.AreEqual(
            "VkD5DAbJVnlJqKmK0Ov9IOoZrPpUXP9ISWeptRP3ptUUE7g6In/kTly3L/yPtKptrdjcIqJEkMzRT+vy6FE5Dg==",
            transaction.Signatures[0]);
        Assert.IsNotNull(transaction.Preconditions);
        Assert.AreEqual("0", transaction.Preconditions.TimeBounds.MinTime);
        Assert.IsNull(transaction.Preconditions.TimeBounds.MaxTime);
    }


    [TestMethod]
    public void TestDeserializeWithMemoText()
    {
        var jsonPath = Utils.GetTestDataPath("transactionWithTextMemo.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSerializer.Deserialize<TransactionResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(transaction);


        // Skip checking the main properties as they have been tested in other test methods
        Assert.AreEqual("XLM e2e monitor transaction", transaction.MemoValue);
        Assert.AreEqual("WExNIGUyZSBtb25pdG9yIHRyYW5zYWN0aW9u", transaction.MemoBytes);
        Assert.AreEqual("text", transaction.MemoType);
        Assert.IsTrue(transaction.Memo is MemoText);
    }

    //
    [TestMethod]
    public void TestDeserializeFeeBump()
    {
        var jsonPath = Utils.GetTestDataPath("transactionFeeBump.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSerializer.Deserialize<TransactionResponse>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(transaction);

        // Skip checking the main properties as they have been tested in other test methods
        Assert.IsNotNull(transaction.FeeBumpTx);
        Assert.AreEqual(1, transaction.FeeBumpTx.Signatures.Count);
        Assert.AreEqual("Hh4e", transaction.FeeBumpTx.Signatures[0]);
        Assert.AreEqual("3dfef7d7226995b504f2827cc63d45ad41e9687bb0a8abcf08ba755fedca0352", transaction.FeeBumpTx.Hash);

        Assert.IsNotNull(transaction.InnerTx);
        Assert.AreEqual(1, transaction.InnerTx.Signatures.Count);
        Assert.AreEqual("FBQU", transaction.InnerTx.Signatures[0]);
        Assert.AreEqual("e98869bba8bce08c10b78406202127f3888c25454cd37b02600862452751f526", transaction.InnerTx.Hash);
        Assert.AreEqual("99", transaction.InnerTx.MaxFee);
    }

    [TestMethod]
    public void TestDeserializeMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("transactionMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var transaction = JsonSerializer.Deserialize<TransactionResponse>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(transaction);
        AssertTransactionMuxedAccount(transaction);
    }
}