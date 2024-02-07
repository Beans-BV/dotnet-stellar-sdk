using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class TransactionMetaTest
{
    [TestMethod]
    public void TestTransactionMetaV3WithMissingProperties()
    {
        var meta = new TransactionMetaV3();
        
        // Act
        var metaXdrBase64 = meta.ToXdrBase64();
        var decodedMeta = TransactionMetaV3.FromXdrBase64(metaXdrBase64);
        
        // Assert
        Assert.IsNull(meta.SorobanMeta);
        Assert.AreEqual(0, decodedMeta.Operations.Length);
        Assert.AreEqual(0, decodedMeta.TransactionChangesAfter.Length);
        Assert.AreEqual(0, decodedMeta.TransactionChangesBefore.Length);
    }

    [TestMethod]
    public void TestSerializingTransactionMetaV3()
    {
        const string metaXdrBase64 = "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";
        var decodedMeta = TransactionMetaV3.FromXdrBase64(metaXdrBase64);
        
        Assert.IsNotNull(decodedMeta.SorobanMeta);
        Assert.AreEqual(21, decodedMeta.SorobanMeta.DiagnosticEvents.Length);
        Assert.IsInstanceOfType(decodedMeta.SorobanMeta.ReturnValue, typeof(SCBool));
        Assert.AreEqual(false, ((SCBool)decodedMeta.SorobanMeta.ReturnValue).InnerValue);
        Assert.AreEqual(0, decodedMeta.Operations.Length);
        
        // Changes before
        Assert.AreEqual(2, decodedMeta.TransactionChangesBefore.Length);
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesBefore[0], typeof(LedgerEntryChangeState));
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesBefore[1], typeof(LedgerEntryChangeUpdated));
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesBefore[0].ChangedEntry, typeof(LedgerEntryAccount));
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesBefore[1].ChangedEntry, typeof(LedgerEntryAccount));
        var changedEntry1Before = decodedMeta.TransactionChangesBefore[0].ChangedEntry as LedgerEntryAccount;
        Assert.AreEqual("GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA", changedEntry1Before!.Account.AccountId);
        Assert.AreEqual(99999564589, changedEntry1Before!.Balance);
        Assert.AreEqual(9168U, changedEntry1Before!.LastModifiedLedgerSeq);
        Assert.AreEqual(34316788695057, changedEntry1Before!.SequenceNumber);
        Assert.AreEqual(0U, changedEntry1Before!.Flags);
        Assert.AreEqual("", changedEntry1Before!.HomeDomain);
        Assert.IsNull(changedEntry1Before!.LedgerExtensionV1);
        
        var changedEntry2Before = decodedMeta.TransactionChangesBefore[1].ChangedEntry as LedgerEntryAccount;
        Assert.AreEqual("GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA", changedEntry2Before!.Account.AccountId);
        Assert.AreEqual(99999564589, changedEntry2Before!.Balance);
        Assert.AreEqual(9168U, changedEntry2Before!.LastModifiedLedgerSeq);
        Assert.AreEqual(34316788695058, changedEntry2Before!.SequenceNumber);
        Assert.AreEqual(0U, changedEntry2Before!.Flags);
        Assert.AreEqual("", changedEntry2Before!.HomeDomain);
        
        // Changes after
        Assert.AreEqual(2, decodedMeta.TransactionChangesAfter.Length);
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesAfter[0], typeof(LedgerEntryChangeState));
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesAfter[1], typeof(LedgerEntryChangeUpdated));
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesAfter[0].ChangedEntry, typeof(LedgerEntryAccount));
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesAfter[1].ChangedEntry, typeof(LedgerEntryAccount));
        var changedEntry1After = decodedMeta.TransactionChangesAfter[0].ChangedEntry as LedgerEntryAccount;
        Assert.AreEqual("GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA", changedEntry1After!.Account.AccountId);
        Assert.AreEqual(99999564589, changedEntry1After!.Balance);
        Assert.AreEqual(9168U, changedEntry1After!.LastModifiedLedgerSeq);
        Assert.AreEqual(34316788695058, changedEntry1After!.SequenceNumber);
        Assert.AreEqual(0U, changedEntry1After!.Flags);
        Assert.AreEqual("", changedEntry1After!.HomeDomain);
        Assert.IsNull(changedEntry1After!.LedgerExtensionV1);
        
        var changedEntry2After = decodedMeta.TransactionChangesAfter[1].ChangedEntry as LedgerEntryAccount;
        Assert.AreEqual("GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA", changedEntry2After!.Account.AccountId);
        Assert.AreEqual(9168U, changedEntry2After!.LastModifiedLedgerSeq);
        Assert.AreEqual(99999569197, changedEntry2After!.Balance);
        Assert.AreEqual(34316788695058, changedEntry2After!.SequenceNumber);
        Assert.AreEqual(0U, changedEntry2After!.Flags);
        Assert.AreEqual("", changedEntry2After!.HomeDomain);
    }
}