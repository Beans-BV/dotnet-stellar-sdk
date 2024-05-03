using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using SCString = StellarDotnetSdk.Soroban.SCString;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using SCVec = StellarDotnetSdk.Soroban.SCVec;
using TransactionMetaV3 = StellarDotnetSdk.Soroban.TransactionMetaV3;

namespace StellarDotnetSdk.Tests.Transactions;

[TestClass]
public class TransactionMetaTest
{
    [TestMethod]
    public void TestSerializingTransactionMetaV3()
    {
        // Failure trying to create a contract with wrong cost
        var metaXdrBase64 =
            "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";
        var decodedMeta = TransactionMetaV3.FromXdrBase64(metaXdrBase64);

        Assert.IsInstanceOfType(decodedMeta.ExtensionPoint, typeof(ExtensionPointZero));

        var sorobanMeta = decodedMeta.SorobanMeta;
        Assert.IsNotNull(sorobanMeta);
        Assert.AreEqual(0, sorobanMeta.Events.Length);
        Assert.AreEqual(21, sorobanMeta.DiagnosticEvents.Length);
        Assert.IsInstanceOfType(sorobanMeta.ReturnValue, typeof(SCBool));
        Assert.AreEqual(false, ((SCBool)sorobanMeta.ReturnValue).InnerValue);
        Assert.AreEqual(0, decodedMeta.Operations.Length);

        // Changes before
        Assert.AreEqual(2, decodedMeta.TransactionChangesBefore.Length);
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesBefore[0], typeof(LedgerEntryState));
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesBefore[1], typeof(LedgerEntryUpdated));

        Assert.IsInstanceOfType(decodedMeta.TransactionChangesBefore[0], typeof(LedgerEntryState));
        var change0Before = (LedgerEntryState)decodedMeta.TransactionChangesBefore[0];

        Assert.IsInstanceOfType(decodedMeta.TransactionChangesBefore[1], typeof(LedgerEntryUpdated));
        var change1Before = (LedgerEntryUpdated)decodedMeta.TransactionChangesBefore[1];

        var changedEntry0Before = change0Before.State as LedgerEntryAccount;
        Assert.IsNotNull(changedEntry0Before);
        Assert.AreEqual("GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA",
            changedEntry0Before.Account.AccountId);
        Assert.AreEqual(99999564589, changedEntry0Before.Balance);
        Assert.AreEqual(9168U, changedEntry0Before.LastModifiedLedgerSeq);
        Assert.AreEqual(34316788695057, changedEntry0Before.SequenceNumber);
        Assert.AreEqual(0U, changedEntry0Before.Flags);
        Assert.AreEqual("", changedEntry0Before.HomeDomain);
        Assert.IsNull(changedEntry0Before.LedgerExtensionV1);

        var changedEntry1Before = change1Before.UpdatedEntry as LedgerEntryAccount;
        Assert.IsNotNull(changedEntry1Before);
        Assert.AreEqual("GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA",
            changedEntry1Before.Account.AccountId);
        Assert.AreEqual(99999564589, changedEntry1Before.Balance);
        Assert.AreEqual(9168U, changedEntry1Before.LastModifiedLedgerSeq);
        Assert.AreEqual(34316788695058, changedEntry1Before.SequenceNumber);
        Assert.AreEqual(0U, changedEntry1Before.Flags);
        Assert.AreEqual("", changedEntry1Before.HomeDomain);

        // Changes after
        Assert.AreEqual(2, decodedMeta.TransactionChangesAfter.Length);

        Assert.IsInstanceOfType(decodedMeta.TransactionChangesAfter[0], typeof(LedgerEntryState));
        var change0After = (LedgerEntryState)decodedMeta.TransactionChangesAfter[0];
        Assert.IsInstanceOfType(decodedMeta.TransactionChangesAfter[1], typeof(LedgerEntryUpdated));
        var change1After = (LedgerEntryUpdated)decodedMeta.TransactionChangesAfter[1];

        Assert.IsInstanceOfType(change0After.State, typeof(LedgerEntryAccount));
        Assert.IsInstanceOfType(change1After.UpdatedEntry, typeof(LedgerEntryAccount));

        var changedEntry0After = change0After.State as LedgerEntryAccount;
        Assert.IsNotNull(changedEntry0After);
        Assert.AreEqual("GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA",
            changedEntry0After.Account.AccountId);
        Assert.AreEqual(99999564589, changedEntry0After.Balance);
        Assert.AreEqual(9168U, changedEntry0After.LastModifiedLedgerSeq);
        Assert.AreEqual(34316788695058, changedEntry0After.SequenceNumber);
        Assert.AreEqual(0U, changedEntry0After.Flags);
        Assert.AreEqual("", changedEntry0After.HomeDomain);
        Assert.IsNull(changedEntry0After.LedgerExtensionV1);

        var changedEntry1After = change1After.UpdatedEntry as LedgerEntryAccount;
        Assert.IsNotNull(changedEntry1After);
        Assert.AreEqual("GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA",
            changedEntry1After.Account.AccountId);
        Assert.AreEqual(9168U, changedEntry1After.LastModifiedLedgerSeq);
        Assert.AreEqual(99999569197, changedEntry1After.Balance);
        Assert.AreEqual(34316788695058, changedEntry1After.SequenceNumber);
        Assert.AreEqual(0U, changedEntry1After.Flags);
        Assert.AreEqual("", changedEntry1After.HomeDomain);

        // Failure trying to access an archived contract data entry
        metaXdrBase64 =
            "AAAAAwAAAAAAAAACAAAAAwAGnCwAAAAAAAAAACMRtl/+ZI8994htM6K35GWqLqFTU3LGv/gzRqx0bXTQAAAAF0dVKacAAB82AAAAQgAAAAQAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAMAAAAAAAab4AAAAABl5SjbAAAAAAAAAAEABpwsAAAAAAAAAAAjEbZf/mSPPfeIbTOit+Rlqi6hU1Nyxr/4M0asdG100AAAABdHVSmnAAAfNgAAAEMAAAAEAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAGnCwAAAAAZeUqbAAAAAAAAAAAAAAAAgAAAAMABpwsAAAAAAAAAAAjEbZf/mSPPfeIbTOit+Rlqi6hU1Nyxr/4M0asdG100AAAABdHVSmnAAAfNgAAAEMAAAAEAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAGnCwAAAAAZeUqbAAAAAAAAAABAAacLAAAAAAAAAAAIxG2X/5kjz33iG0zorfkZaouoVNTcsa/+DNGrHRtdNAAAAAXR1U79AAAHzYAAABDAAAABAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAwAAAAAABpwsAAAAAGXlKmwAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAVlcnJvcgAAAAAAAAIAAAAIAAAAAgAAABAAAAABAAAAAwAAAA4AAAAwdHJ5aW5nIHRvIGFjY2VzcyBhbiBhcmNoaXZlZCBjb250cmFjdCBkYXRhIGVudHJ5AAAAEgAAAAHZOgsPCy1CkkrRMg9nLJEvtrXMyS+R6rVJs73oeJqEQAAAABQ=";
        decodedMeta = TransactionMetaV3.FromXdrBase64(metaXdrBase64);

        Assert.IsInstanceOfType(decodedMeta.ExtensionPoint, typeof(ExtensionPointZero));
        sorobanMeta = decodedMeta.SorobanMeta;
        Assert.IsNotNull(sorobanMeta);
        Assert.AreEqual(0, sorobanMeta.Events.Length);
        Assert.AreEqual(1, sorobanMeta.DiagnosticEvents.Length);
        Assert.IsInstanceOfType(sorobanMeta.ReturnValue, typeof(SCBool));
        Assert.AreEqual(false, ((SCBool)sorobanMeta.ReturnValue).InnerValue);
        Assert.AreEqual(0, decodedMeta.Operations.Length);

        Assert.AreEqual(false, sorobanMeta.DiagnosticEvents[0].InSuccessfulContractCall);
        var @event = sorobanMeta.DiagnosticEvents[0].Event;
        Assert.IsNotNull(@event);
        Assert.IsNull(@event.ContractId);
        Assert.IsInstanceOfType(@event.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(2, @event.Topics.Length);
        Assert.AreEqual(ContractEventType.ContractEventTypeEnum.DIAGNOSTIC, @event.Type.InnerValue);
        Assert.IsNotNull(@event.Data);
        Assert.IsInstanceOfType(@event.Data, typeof(SCVec));
        var eventData = (SCVec)@event.Data;
        Assert.AreEqual(3, eventData.InnerValue.Length);
        Assert.IsInstanceOfType(eventData.InnerValue[0], typeof(SCString));
        Assert.AreEqual("trying to access an archived contract data entry",
            ((SCString)eventData.InnerValue[0]).InnerValue);
        Assert.IsInstanceOfType(eventData.InnerValue[1], typeof(SCContractId));
        Assert.AreEqual("CDMTUCYPBMWUFESK2EZA6ZZMSEX3NNOMZEXZD2VVJGZ332DYTKCEBFI5",
            ((SCContractId)eventData.InnerValue[1]).InnerValue);
        Assert.IsInstanceOfType(eventData.InnerValue[2], typeof(SCLedgerKeyContractInstance));
        var topics = @event.Topics;
        Assert.AreEqual(2, topics.Length);
        Assert.IsInstanceOfType(topics[0], typeof(SCSymbol));
        Assert.IsInstanceOfType(topics[1], typeof(SCValueError));
        Assert.AreEqual("error", ((SCSymbol)topics[0]).InnerValue);
        Assert.AreEqual(SCErrorCode.SCErrorCodeEnum.SCEC_INVALID_INPUT, ((SCValueError)topics[1]).Code);
    }
}