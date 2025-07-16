using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using ContractEvent = StellarDotnetSdk.Xdr.ContractEvent;
using ExtensionPoint = StellarDotnetSdk.Xdr.ExtensionPoint;
using XdrSCVal = StellarDotnetSdk.Xdr.SCVal;
using TransactionEvent = StellarDotnetSdk.Xdr.TransactionEvent;
using XdrTransactionMetaV3 = StellarDotnetSdk.Soroban.TransactionMetaV3;
using XdrTransactionMetaV4 = StellarDotnetSdk.Xdr.TransactionMetaV4;
using TransactionEventStageEnum = StellarDotnetSdk.Xdr.TransactionEventStage.TransactionEventStageEnum;
using ContractEventTypeEnum = StellarDotnetSdk.Xdr.ContractEventType.ContractEventTypeEnum;
using DiagnosticEvent = StellarDotnetSdk.Xdr.DiagnosticEvent;
using Int32 = StellarDotnetSdk.Xdr.Int32;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using LedgerEntry = StellarDotnetSdk.Xdr.LedgerEntry;
using LedgerEntryChange = StellarDotnetSdk.Xdr.LedgerEntryChange;
using OperationMetaV2 = StellarDotnetSdk.Xdr.OperationMetaV2;
using SCString = StellarDotnetSdk.Xdr.SCString;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using SorobanTransactionMetaV2 = StellarDotnetSdk.Xdr.SorobanTransactionMetaV2;
using ClaimableBalanceIDTypeEnum = StellarDotnetSdk.Xdr.ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum;
using LedgerEntryChangeTypeEnum = StellarDotnetSdk.Xdr.LedgerEntryChangeType.LedgerEntryChangeTypeEnum;
using LedgerEntryTypeEnum = StellarDotnetSdk.Xdr.LedgerEntryType.LedgerEntryTypeEnum;
using AssetTypeEnum = StellarDotnetSdk.Xdr.AssetType.AssetTypeEnum;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using SCVec = StellarDotnetSdk.Soroban.SCVec;
using TransactionMeta = StellarDotnetSdk.Soroban.TransactionMeta;
using TransactionMetaV4 = StellarDotnetSdk.Soroban.TransactionMetaV4;

namespace StellarDotnetSdk.Tests.Transactions;

[TestClass]
public class TransactionMetaTest
{
    [TestMethod]
    public void TestDeserializingTransactionMetaV3()
    {
        // Failure trying to create a contract with wrong cost
        var metaXdrBase64 =
            "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";
        var decodedMeta = TransactionMeta.FromXdrBase64(metaXdrBase64) as XdrTransactionMetaV3;
        Assert.IsNotNull(decodedMeta);
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
        var decodedMeta1 = TransactionMeta.FromXdrBase64(metaXdrBase64) as XdrTransactionMetaV3;
        Assert.IsNotNull(decodedMeta1);
        Assert.IsInstanceOfType(decodedMeta1.ExtensionPoint, typeof(ExtensionPointZero));
        sorobanMeta = decodedMeta1.SorobanMeta;
        Assert.IsNotNull(sorobanMeta);
        Assert.AreEqual(0, sorobanMeta.Events.Length);
        Assert.AreEqual(1, sorobanMeta.DiagnosticEvents.Length);
        Assert.IsInstanceOfType(sorobanMeta.ReturnValue, typeof(SCBool));
        Assert.AreEqual(false, ((SCBool)sorobanMeta.ReturnValue).InnerValue);
        Assert.AreEqual(0, decodedMeta1.Operations.Length);

        Assert.AreEqual(false, sorobanMeta.DiagnosticEvents[0].InSuccessfulContractCall);
        var contractEvent = sorobanMeta.DiagnosticEvents[0].Event;
        Assert.IsNotNull(contractEvent);
        Assert.IsNull(contractEvent.ContractId);
        Assert.IsInstanceOfType(contractEvent.ExtensionPoint, typeof(ExtensionPointZero));
        var eventBody = contractEvent.BodyV0;
        Assert.IsNotNull(eventBody);
        Assert.AreEqual(2, eventBody.Topics.Length);
        Assert.AreEqual(ContractEventTypeEnum.DIAGNOSTIC, contractEvent.Type.InnerValue);
        Assert.IsNotNull(eventBody.Data);
        Assert.IsInstanceOfType(eventBody.Data, typeof(SCVec));
        var eventData = (SCVec)eventBody.Data;
        Assert.AreEqual(3, eventData.InnerValue.Length);
        Assert.IsInstanceOfType(eventData.InnerValue[0], typeof(StellarDotnetSdk.Soroban.SCString));
        Assert.AreEqual("trying to access an archived contract data entry",
            ((StellarDotnetSdk.Soroban.SCString)eventData.InnerValue[0]).InnerValue);
        Assert.IsInstanceOfType(eventData.InnerValue[1], typeof(ScContractId));
        Assert.AreEqual("CDMTUCYPBMWUFESK2EZA6ZZMSEX3NNOMZEXZD2VVJGZ332DYTKCEBFI5",
            ((ScContractId)eventData.InnerValue[1]).InnerValue);
        Assert.IsInstanceOfType(eventData.InnerValue[2], typeof(SCLedgerKeyContractInstance));
        var topics = eventBody.Topics;
        Assert.AreEqual(2, topics.Length);
        Assert.IsInstanceOfType(topics[0], typeof(SCSymbol));
        Assert.IsInstanceOfType(topics[1], typeof(SCValueError));
        Assert.AreEqual("error", ((SCSymbol)topics[0]).InnerValue);
        Assert.AreEqual(SCErrorCode.SCErrorCodeEnum.SCEC_INVALID_INPUT, ((SCValueError)topics[1]).Code);
    }

    [TestMethod]
    public void TestDeserializeTransactionMetaV4()
    {
        var txEvent1 = new TransactionEvent
        {
            Stage = TransactionEventStage.Create(
                TransactionEventStageEnum.TRANSACTION_EVENT_STAGE_BEFORE_ALL_TXS),
            Event = CreateRandomContractEvent(),
        };
        var txEvent2 = new TransactionEvent
        {
            Stage = TransactionEventStage.Create(
                TransactionEventStageEnum.TRANSACTION_EVENT_STAGE_AFTER_TX),
            Event = CreateRandomContractEvent(),
        };
        var txEvent3 = new TransactionEvent
        {
            Stage = TransactionEventStage.Create(
                TransactionEventStageEnum.TRANSACTION_EVENT_STAGE_AFTER_ALL_TXS),
            Event = CreateRandomContractEvent(),
        };
        var xdrMetaExtV1 = new SorobanTransactionMetaExtV1
        {
            Ext = new ExtensionPoint(),
            RentFeeCharged = CreateRandomInt64(),
            TotalNonRefundableResourceFeeCharged = CreateRandomInt64(),
            TotalRefundableResourceFeeCharged = CreateRandomInt64(),
        };
        var sorobanMetaV2 = new SorobanTransactionMetaV2
        {
            Ext = new SorobanTransactionMetaExt
            {
                Discriminant = 1,
                V1 = xdrMetaExtV1,
            },
            ReturnValue = CreateRandomScInt64(),
        };

        var operationMetaV2 = new OperationMetaV2
        {
            Ext = new ExtensionPoint
            {
                Discriminant = 0,
            },
            Events =
            [
                CreateRandomContractEvent(),
                CreateRandomContractEvent(),
            ],
            Changes = new LedgerEntryChanges
            (
                [
                    CreateRandomLedgerEntryChangeCreated(),
                ]
            ),
        };
        var diagnosticEvent1 = new DiagnosticEvent
        {
            Event = CreateRandomContractEvent(),
            InSuccessfulContractCall = true,
        };
        var diagnosticEvent2 = new DiagnosticEvent
        {
            Event = CreateRandomContractEvent(),
            InSuccessfulContractCall = false,
        };
        var metaV4 = new XdrTransactionMetaV4
        {
            Events = [txEvent1, txEvent2, txEvent3],
            Ext = new ExtensionPoint
            {
                Discriminant = 0,
            },
            SorobanMeta = sorobanMetaV2,
            Operations = [operationMetaV2],
            TxChangesAfter = new LedgerEntryChanges(
                [
                    CreateRandomLedgerEntryChangeCreated(),
                ]
            ),
            DiagnosticEvents = [diagnosticEvent1, diagnosticEvent2],
            TxChangesBefore = new LedgerEntryChanges(
                [
                    CreateRandomLedgerEntryChangeCreated(),
                ]
            ),
        };
        var xdrMeta = new StellarDotnetSdk.Xdr.TransactionMeta()
        {
            Discriminant = 4,
            V4 = metaV4,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.TransactionMeta.Encode(os, xdrMeta);

        var metaXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedMeta =
            TransactionMeta.FromXdrBase64(metaXdrBase64) as TransactionMetaV4;
        Assert.IsNotNull(decodedMeta);

        Assert.IsInstanceOfType(decodedMeta.ExtensionPoint,
            typeof(ExtensionPointZero));

        // Transaction events
        Assert.AreEqual(metaV4.Events.Length,
            decodedMeta.Events.Length);

        for (var i = 0; i < metaV4.Events.Length; i++)
        {
            Assert.AreEqual(metaV4.Events[i].Stage.InnerValue,
                decodedMeta.Events[i].Stage.InnerValue);

            // Contract events
            AssertEqualContractEvents(metaV4.Events[i].Event,
                decodedMeta.Events[i].Event);
        }

        // Diagnostic events
        Assert.AreEqual(metaV4.DiagnosticEvents.Length,
            decodedMeta.DiagnosticEvents.Length);
        for (var i = 0; i < metaV4.DiagnosticEvents.Length; i++)
        {
            Assert.AreEqual(metaV4.DiagnosticEvents[i].InSuccessfulContractCall,
                decodedMeta.DiagnosticEvents[i].InSuccessfulContractCall);
            AssertEqualContractEvents(metaV4.DiagnosticEvents[i].Event,
                decodedMeta.DiagnosticEvents[i].Event);
        }

        // Operation meta
        Assert.AreEqual(metaV4.Operations.Length,
            decodedMeta.Operations.Length);
        for (var i = 0; i < metaV4.Operations.Length; i++)
        {
            var decodedOperation = decodedMeta.Operations[i];
            Assert.IsInstanceOfType(decodedOperation.ExtensionPoint,
                typeof(ExtensionPointZero));
            var metaOperation = metaV4.Operations[i];
            Assert.AreEqual(metaOperation.Events.Length,
                decodedOperation.Events.Length);
            for (var j = 0; j < metaOperation.Events.Length; j++)
            {
                AssertEqualContractEvents(metaOperation.Events[j],
                    decodedOperation.Events[j]);
            }

            // Ledger entry changes
            Assert.AreEqual(metaV4.Operations[i].Changes.InnerValue.Length,
                decodedOperation.Changes.Length);
            for (var k = 0; k < metaOperation.Changes.InnerValue.Length; k++)
            {
                Assert.IsInstanceOfType(decodedOperation.Changes[k], typeof(LedgerEntryCreated));
                var xdrCreatedEntry = metaOperation.Changes.InnerValue[k].Created.Data.ClaimableBalance;
                var createdEntry =
                    ((LedgerEntryCreated)decodedOperation.Changes[k]).CreatedEntry as LedgerEntryClaimableBalance;
                AssertEqualLedgerEntryClaimableBalances(createdEntry, xdrCreatedEntry);
            }
        }

        // Transaction changes before
        Assert.AreEqual(metaV4.TxChangesBefore.InnerValue.Length,
            decodedMeta.TransactionChangesBefore.Length);
        for (var i = 0; i < metaV4.TxChangesBefore.InnerValue.Length; i++)
        {
            var xdrCreatedEntry = metaV4.TxChangesBefore.InnerValue[i].Created.Data.ClaimableBalance;
            var createdEntry = ((LedgerEntryCreated)decodedMeta.TransactionChangesBefore[i]).CreatedEntry
                as LedgerEntryClaimableBalance;
            AssertEqualLedgerEntryClaimableBalances(createdEntry, xdrCreatedEntry);
        }

        // Transaction changes after
        Assert.AreEqual(metaV4.TxChangesAfter.InnerValue.Length,
            decodedMeta.TransactionChangesAfter.Length);
        for (var i = 0; i < metaV4.TxChangesAfter.InnerValue.Length; i++)
        {
            var xdrCreatedEntry = metaV4.TxChangesAfter.InnerValue[i].Created.Data.ClaimableBalance;
            var createdEntry = ((LedgerEntryCreated)decodedMeta.TransactionChangesAfter[i]).CreatedEntry
                as LedgerEntryClaimableBalance;
            AssertEqualLedgerEntryClaimableBalances(createdEntry, xdrCreatedEntry);
        }

        // Soroban meta
        var decodedSorobanMeta = decodedMeta.SorobanMeta;
        Assert.IsNotNull(decodedSorobanMeta);

        Assert.IsNotNull(decodedSorobanMeta.ReturnValue);
        Assert.AreEqual(SCVal.FromXdr(sorobanMetaV2.ReturnValue).ToXdrBase64(),
            decodedSorobanMeta.ReturnValue.ToXdrBase64());

        // Soroban meta extension
        var decodedMetaExtV1 = decodedSorobanMeta.SorobanTransactionMetaExtensionV1;
        Assert.IsNotNull(decodedMetaExtV1);
        Assert.IsInstanceOfType(decodedMetaExtV1.ExtensionPoint,
            typeof(ExtensionPointZero));
        Assert.AreEqual(xdrMetaExtV1.RentFeeCharged.InnerValue,
            decodedMetaExtV1.RentFeeCharged);
        Assert.AreEqual(xdrMetaExtV1.TotalNonRefundableResourceFeeCharged.InnerValue,
            decodedMetaExtV1.TotalNonRefundableResourceFeeCharged);
        Assert.AreEqual(xdrMetaExtV1.TotalRefundableResourceFeeCharged.InnerValue,
            decodedMetaExtV1.TotalRefundableResourceFeeCharged);
    }

    private static void AssertEqualLedgerEntryClaimableBalances(LedgerEntryClaimableBalance? createdEntry,
        ClaimableBalanceEntry xdrCreatedEntry)
    {
        Assert.IsNotNull(createdEntry);
        Assert.IsNull(createdEntry.ClaimableBalanceEntryExtensionV1);
        Assert.AreEqual(xdrCreatedEntry.Amount.InnerValue, createdEntry.Amount);
        CollectionAssert.AreEqual(xdrCreatedEntry.BalanceID.V0.InnerValue, createdEntry.BalanceId);
        Assert.AreEqual(xdrCreatedEntry.Claimants.Length, createdEntry.Claimants.Length);
        for (var l = 0; l < createdEntry.Claimants.Length; l++)
        {
            CollectionAssert.AreEqual(xdrCreatedEntry.Claimants[l].V0.Destination.InnerValue.Ed25519.InnerValue,
                createdEntry.Claimants[l].Destination.XdrPublicKey.Ed25519.InnerValue);
        }
        Assert.IsTrue(StellarDotnetSdk.Assets.Asset.FromXdr(xdrCreatedEntry.Asset).Equals(createdEntry.Asset));
    }

    private static void AssertEqualContractEvents(
        ContractEvent xdrContractEvent,
        Soroban.ContractEvent contractEvent
    )
    {
        Assert.AreEqual(xdrContractEvent.Type.InnerValue,
            contractEvent.Type.InnerValue);
        Assert.AreEqual(StrKey.EncodeContractId(xdrContractEvent.ContractID.InnerValue.InnerValue),
            contractEvent.ContractId);
        Assert.IsInstanceOfType(contractEvent.ExtensionPoint,
            typeof(ExtensionPointZero));

        // Event V0
        var contractEventV0 = xdrContractEvent.Body.V0;
        var decodedContractEvenV0 = contractEvent.BodyV0;
        Assert.IsNotNull(decodedContractEvenV0);
        Assert.AreEqual(contractEventV0.Topics.Length,
            decodedContractEvenV0.Topics.Length);
        for (var j = 0; j < contractEventV0.Topics.Length; j++)
        {
            Assert.AreEqual(SCVal.FromXdr(contractEventV0.Topics[j]).ToXdrBase64(),
                decodedContractEvenV0.Topics[j].ToXdrBase64());
        }
        Assert.AreEqual(SCVal.FromXdr(contractEventV0.Data).ToXdrBase64(),
            decodedContractEvenV0.Data.ToXdrBase64());
    }

    private static string GenerateRandomString(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }

    private static XdrSCVal CreateRandomScString()
    {
        return new XdrSCVal
        {
            Discriminant = SCValType.Create(SCValType.SCValTypeEnum.SCV_STRING),
            Str = new SCString
            {
                InnerValue = GenerateRandomString(20),
            },
        };
    }

    private static XdrSCVal CreateScInt32()
    {
        var random = new Random();
        var randomInt = random.Next(100, 20000);
        return new XdrSCVal
        {
            Discriminant = SCValType.Create(SCValType.SCValTypeEnum.SCV_I32),
            I32 = new Int32
            {
                InnerValue = randomInt,
            },
        };
    }

    private static XdrSCVal CreateRandomScInt64()
    {
        var random = new Random();
        var randomInt = random.Next(100, 20000);
        return new XdrSCVal
        {
            Discriminant = SCValType.Create(SCValType.SCValTypeEnum.SCV_I64),
            I64 = new Int64
            {
                InnerValue = randomInt,
            },
        };
    }

    private static Int64 CreateRandomInt64()
    {
        var random = new Random();
        var randomInt = random.Next(100, 20000);
        return new Int64(randomInt);
    }

    private static LedgerEntryChange CreateRandomLedgerEntryChangeCreated()
    {
        var createdEntry = new LedgerEntryChange
        {
            Discriminant =
                LedgerEntryChangeType.Create(LedgerEntryChangeTypeEnum.LEDGER_ENTRY_CREATED),
            Created = new LedgerEntry
            {
                Ext = new LedgerEntry.LedgerEntryExt
                {
                    Discriminant = 0,
                },
                LastModifiedLedgerSeq = new Uint32(100),
                Data = new LedgerEntry.LedgerEntryData
                {
                    Discriminant = LedgerEntryType.Create(LedgerEntryTypeEnum.CLAIMABLE_BALANCE),
                    ClaimableBalance = new ClaimableBalanceEntry
                    {
                        Ext = new ClaimableBalanceEntry.ClaimableBalanceEntryExt
                        {
                            Discriminant = 0,
                        },
                        Amount = CreateRandomInt64(),
                        Asset = new Asset
                        {
                            Discriminant = AssetType.Create(AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4),
                            AlphaNum4 = new AlphaNum4
                            {
                                AssetCode = new AssetCode4
                                {
                                    InnerValue = Encoding.Default.GetBytes(GenerateRandomString(4)),
                                },
                                Issuer = new AccountID(KeyPair.Random().XdrPublicKey),
                            },
                        },
                        BalanceID = new ClaimableBalanceID
                        {
                            Discriminant =
                                ClaimableBalanceIDType.Create(ClaimableBalanceIDTypeEnum.CLAIMABLE_BALANCE_ID_TYPE_V0),
                            V0 = new Hash(
                                Convert.FromHexString(
                                    "a26599c23752c92c3e74f60b72081b0b3f4b1a8353f357848f69c82e3fe6373c")),
                        },
                        Claimants = [],
                    },
                },
            },
        };
        return createdEntry;
    }

    private static ContractEvent CreateRandomContractEvent()
    {
        var xdrContractEvenV0 = new ContractEvent.ContractEventBody.ContractEventV0
        {
            Data = CreateScInt32(),
            Topics =
            [
                CreateRandomScString(),
                CreateRandomScInt64(),
            ],
        };
        var xdrContractEvent = new ContractEvent
        {
            Body = new ContractEvent.ContractEventBody
            {
                Discriminant = 0,
                V0 = xdrContractEvenV0,
            },
            ContractID = new ContractID
            {
                InnerValue =
                    new Hash(
                        StrKey.DecodeContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB")),
            },
            Ext = new ExtensionPoint
            {
                Discriminant = 0,
            },
            Type = ContractEventType.Create(
                Enum.GetValues<ContractEventTypeEnum>()[
                    new Random().Next(3) // 3 is the number of enum values
                ]
            ),
        };
        return xdrContractEvent;
    }
}