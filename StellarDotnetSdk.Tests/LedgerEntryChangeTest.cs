using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Xdr;
using XdrLedgerEntry = StellarDotnetSdk.Xdr.LedgerEntry;
using LedgerEntryChange = StellarDotnetSdk.Xdr.LedgerEntryChange;
using LedgerEntryChangeTypeEnum = StellarDotnetSdk.Xdr.LedgerEntryChangeType.LedgerEntryChangeTypeEnum;
using LedgerKey = StellarDotnetSdk.Xdr.LedgerKey;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class LedgerEntryChangeTest
{
    [TestMethod]
    public void TestDeserializeLedgerEntryChangeCreated()
    {
        // Arrange
        var xdrLedgerEntryChange = new LedgerEntryChange
        {
            Discriminant =
                LedgerEntryChangeType.Create(LedgerEntryChangeTypeEnum.LEDGER_ENTRY_CREATED),
            Created = CreateDummyLedgerEntry(),
        };

        // Act
        var os = new XdrDataOutputStream();
        LedgerEntryChange.Encode(os, xdrLedgerEntryChange);
        var xdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntryChange = LedgerEntries.LedgerEntryChange.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsInstanceOfType(decodedLedgerEntryChange, typeof(LedgerEntryCreated));
        var createdLedger = (LedgerEntryCreated)decodedLedgerEntryChange;
        AssertEqualLedgerDataEntries(xdrLedgerEntryChange.Created.Data.Data,
            (LedgerEntryData)createdLedger.CreatedEntry);
    }

    [TestMethod]
    public void TestDeserializeLedgerEntryChangeRestored()
    {
        // Arrange
        var xdrLedgerEntryChange = new LedgerEntryChange
        {
            Discriminant =
                LedgerEntryChangeType.Create(LedgerEntryChangeTypeEnum.LEDGER_ENTRY_RESTORED),
            Restored = CreateDummyLedgerEntry(),
        };

        // Act
        var os = new XdrDataOutputStream();
        LedgerEntryChange.Encode(os, xdrLedgerEntryChange);
        var xdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntryChange = LedgerEntries.LedgerEntryChange.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsInstanceOfType(decodedLedgerEntryChange, typeof(LedgerEntryRestored));
        var restoredLedger = (LedgerEntryRestored)decodedLedgerEntryChange;
        AssertEqualLedgerDataEntries(xdrLedgerEntryChange.Restored.Data.Data,
            (LedgerEntryData)restoredLedger.RestoredEntry);
    }

    [TestMethod]
    public void TestDeserializeLedgerEntryChangeUpdated()
    {
        // Arrange
        var xdrLedgerEntryChange = new LedgerEntryChange
        {
            Discriminant =
                LedgerEntryChangeType.Create(LedgerEntryChangeTypeEnum.LEDGER_ENTRY_UPDATED),
            Updated = CreateDummyLedgerEntry(),
        };

        // Act
        var os = new XdrDataOutputStream();
        LedgerEntryChange.Encode(os, xdrLedgerEntryChange);
        var xdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntryChange = LedgerEntries.LedgerEntryChange.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsInstanceOfType(decodedLedgerEntryChange, typeof(LedgerEntryUpdated));
        var updatedLedger = (LedgerEntryUpdated)decodedLedgerEntryChange;
        AssertEqualLedgerDataEntries(xdrLedgerEntryChange.Updated.Data.Data,
            (LedgerEntryData)updatedLedger.UpdatedEntry);
    }

    [TestMethod]
    public void TestDeserializeLedgerEntryChangeState()
    {
        // Arrange
        var xdrLedgerEntryChange = new LedgerEntryChange
        {
            Discriminant =
                LedgerEntryChangeType.Create(LedgerEntryChangeTypeEnum.LEDGER_ENTRY_STATE),
            State = CreateDummyLedgerEntry(),
        };

        // Act
        var os = new XdrDataOutputStream();
        LedgerEntryChange.Encode(os, xdrLedgerEntryChange);
        var xdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntryChange = LedgerEntries.LedgerEntryChange.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsInstanceOfType(decodedLedgerEntryChange, typeof(LedgerEntryState));
        var stateLedger = (LedgerEntryState)decodedLedgerEntryChange;
        AssertEqualLedgerDataEntries(xdrLedgerEntryChange.State.Data.Data, (LedgerEntryData)stateLedger.State);
    }

    [TestMethod]
    public void TestDeserializeLedgerEntryChangeRemoved()
    {
        // Arrange
        var xdrLedgerEntryChange = new LedgerEntryChange
        {
            Discriminant =
                LedgerEntryChangeType.Create(LedgerEntryChangeTypeEnum.LEDGER_ENTRY_REMOVED),
            Removed = new LedgerKey
            {
                Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT),
                Account = new LedgerKey.LedgerKeyAccount
                {
                    AccountID = new AccountID(KeyPair.Random().XdrPublicKey),
                },
            },
        };

        // Act
        var os = new XdrDataOutputStream();
        LedgerEntryChange.Encode(os, xdrLedgerEntryChange);
        var xdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntryChange = LedgerEntries.LedgerEntryChange.FromXdrBase64(xdrBase64);

        // Assert
        Assert.IsInstanceOfType(decodedLedgerEntryChange, typeof(LedgerEntryRemoved));
        var removedLedger = (LedgerEntryRemoved)decodedLedgerEntryChange;
        var decodedKey = (LedgerKeyAccount)removedLedger.RemovedKey;
        CollectionAssert.AreEqual(xdrLedgerEntryChange.Removed.Account.AccountID.InnerValue.Ed25519.InnerValue,
            decodedKey.Account.PublicKey);
    }

    private static void AssertEqualLedgerDataEntries(DataEntry xdrEntry, LedgerEntryData entry)
    {
        CollectionAssert.AreEqual(xdrEntry.AccountID.InnerValue.Ed25519.InnerValue,
            entry.Account.PublicKey);
        Assert.AreEqual(xdrEntry.DataName.InnerValue, entry.DataName);
        CollectionAssert.AreEqual(xdrEntry.DataValue.InnerValue, entry.DataValue);
    }

    private static XdrLedgerEntry CreateDummyLedgerEntry()
    {
        var random = new Random();
        var randomInt = random.Next(100, 10000);
        return new XdrLedgerEntry
        {
            Ext = new XdrLedgerEntry.LedgerEntryExt
            {
                Discriminant = 0,
            },
            LastModifiedLedgerSeq = new Uint32(13000),
            Data = new XdrLedgerEntry.LedgerEntryData
            {
                Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.DATA),
                Data = new DataEntry
                {
                    Ext = new DataEntry.DataEntryExt
                    {
                        Discriminant = 0,
                    },
                    AccountID = new AccountID(KeyPair.Random().XdrPublicKey),
                    DataName = new String64("HomeDomain"),
                    DataValue = new DataValue
                    {
                        InnerValue = Encoding.Default.GetBytes(randomInt.ToString()),
                    },
                },
            },
        };
    }
}