using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using XdrLedgerEntry = StellarDotnetSdk.Xdr.LedgerEntry;
using LedgerEntryChange = StellarDotnetSdk.Xdr.LedgerEntryChange;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using LedgerEntryChangeTypeEnum = StellarDotnetSdk.Xdr.LedgerEntryChangeType.LedgerEntryChangeTypeEnum;

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
            Removed = new StellarDotnetSdk.Xdr.LedgerKey
            {
                Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT),
                Account = new StellarDotnetSdk.Xdr.LedgerKey.LedgerKeyAccount
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

    [TestMethod]
    public void TestLedgerKeyDataWithTooLongName()
    {
        var keypair = KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");
        const string dataName = "This is a 73 characters long string which is too strong for String64 type";
        var ex = Assert.ThrowsException<ArgumentException>(() => LedgerKey.Data(keypair, dataName));
        Assert.IsTrue(ex.Message.Contains("Data name cannot exceed 64 characters."));
    }

    [TestMethod]
    public void TestLedgerKeyDataWithValidName()
    {
        var keypair = KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");
        var ledgerKey = (LedgerKeyData)LedgerKey.Data(keypair, "Test Data");

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyData)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(ledgerKey.DataName, decodedLedgerKey.DataName);
        Assert.AreEqual(ledgerKey.Account.AccountId, decodedLedgerKey.Account.AccountId);
    }

    [TestMethod]
    public void TestLedgerKeyOffer()
    {
        var keypair = KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");
        var ledgerKey = (LedgerKeyOffer)LedgerKey.Offer(keypair, 1234);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyOffer)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(ledgerKey.OfferId, decodedLedgerKey.OfferId);
        Assert.AreEqual(ledgerKey.Seller.AccountId, decodedLedgerKey.Seller.AccountId);
    }

    [TestMethod]
    public void TestLedgerKeyTrustline()
    {
        var keypair = KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");
        var issuer = KeyPair.FromAccountId("GB24C27VKWCBG7NTCT4J2L4MXJGYC3K3SQ4JOTCSPOVVEN7EZEB43XNE");
        var asset = TrustlineAsset.CreateNonNativeAsset("ABCD", issuer.AccountId);
        var ledgerKey = LedgerKey.Trustline(keypair, asset);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyTrustline)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual("ABCD:GB24C27VKWCBG7NTCT4J2L4MXJGYC3K3SQ4JOTCSPOVVEN7EZEB43XNE",
            ((TrustlineAsset.Wrapper)decodedLedgerKey.Asset).Asset.CanonicalName());
        Assert.AreEqual(keypair.AccountId, decodedLedgerKey.Account.AccountId);
    }

    [TestMethod]
    public void TestLedgerKeyClaimableBalanceStringConstructorValid()
    {
        const string balanceId = "d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780";
        var ledgerKey = LedgerKey.ClaimableBalance(balanceId);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyClaimableBalance)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual("d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780",
            decodedLedgerKey.BalanceId.ToLower());
    }

    [TestMethod]
    public void TestLedgerKeyClaimableBalanceStringConstructorInvalid()
    {
        const string balanceId = "00000000d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780";
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            LedgerKey.ClaimableBalance(balanceId));
        Assert.IsTrue(ex.Message.Contains("Claimable balance ID cannot exceed 64 characters."));
    }

    [TestMethod]
    public void TestLedgerKeyClaimableBalanceByteArrayConstructorValid()
    {
        const string balanceId = "d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780";
        var ledgerKey = LedgerKey.ClaimableBalance(Convert.FromHexString(balanceId));

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyClaimableBalance)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(balanceId, decodedLedgerKey.BalanceId.ToLower());
    }

    [TestMethod]
    public void TestLedgerKeyClaimableBalanceByteArrayConstructorInvalid()
    {
        const string balanceId = "00000000d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780";
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            LedgerKey.ClaimableBalance(Convert.FromHexString(balanceId)));
        Assert.IsTrue(ex.Message.Contains("Claimable balance ID byte array must have exactly 32 bytes."));
    }

    [TestMethod]
    public void TestLedgerKeyLiquidityPool()
    {
        var hash = new byte[]
            { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 };
        var ledgerKey = LedgerKey.LiquidityPool(new LiquidityPoolId(hash));

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyLiquidityPool)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        CollectionAssert.AreEqual(hash, decodedLedgerKey.LiquidityPoolId.Hash);
    }

    [TestMethod]
    public void TestLedgerKeyContractDataWithContractBeingContractId()
    {
        var contractId = new SCContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB");
        var key = new SCSymbol("kk");

        var durability = ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT);
        var ledgerKey = (LedgerKeyContractData)LedgerKey.ContractData(contractId, key, durability);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyContractData)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(contractId.InnerValue, ((SCContractId)decodedLedgerKey.Contract).InnerValue);
        Assert.AreEqual(key.InnerValue, ((SCSymbol)decodedLedgerKey.Key).InnerValue);
        Assert.AreEqual(ledgerKey.Durability.InnerValue, decodedLedgerKey.Durability.InnerValue);
    }

    [TestMethod]
    public void TestLedgerKeyContractDataWithWithContractBeingAccountId()
    {
        var accountId = new SCAccountId("GCZFMH32MF5EAWETZTKF3ZV5SEVJPI53UEMDNSW55WBR75GMZJU4U573");
        var key = new SCInt64(122);

        var durability = ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.TEMPORARY);
        var ledgerKey = (LedgerKeyContractData)LedgerKey.ContractData(accountId, key, durability);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyContractData)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(accountId.InnerValue, ((SCAccountId)decodedLedgerKey.Contract).InnerValue);
        Assert.AreEqual(key.InnerValue, ((SCInt64)decodedLedgerKey.Key).InnerValue);
        Assert.AreEqual(ledgerKey.Durability.InnerValue, decodedLedgerKey.Durability.InnerValue);
    }

    [TestMethod]
    public void TestLedgerKeyContractCodeCreationFromValidHashString()
    {
        var ledgerKey =
            (LedgerKeyContractCode)LedgerKey.ContractCode(
                "0102030405060708090001020304050607080900010203040506070809000102");

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyContractCode)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        CollectionAssert.AreEqual(ledgerKey.Hash, decodedLedgerKey.Hash);
    }

    [TestMethod]
    public void TestLedgerKeyContractCodeCreationFromInvalidHashString()
    {
        var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            LedgerKey.ContractCode("01020304050607080900010203040506070809000102030405060708090002"));
        Assert.IsTrue(ex.Message.Contains("Hash must have exactly 32 bytes."));
    }

    [TestMethod]
    public void TestLedgerKeyConfigSetting()
    {
        var ledgerKey = (LedgerKeyConfigSetting)LedgerKey.ConfigSetting(new ConfigSettingID
        {
            InnerValue = ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_STATE_ARCHIVAL,
        });

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyConfigSetting)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(ledgerKey.ConfigSettingId.InnerValue, decodedLedgerKey.ConfigSettingId.InnerValue);
    }

    [TestMethod]
    public void TestLedgerKeyTtlCreationFromValidHashString()
    {
        var ledgerKey = (LedgerKeyTtl)LedgerKey.Ttl("AQIDBAUGBwgJAAECAwQFBgcICQABAgMEBQYHCAkAAQI=");

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyTtl)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        CollectionAssert.AreEqual(ledgerKey.Key, decodedLedgerKey.Key);
    }

    [TestMethod]
    public void TestLedgerKeyTtlCreationFromInvalidHashString()
    {
        var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            LedgerKey.ContractCode("01020304050607080900010203040506070809000102030405060708090001020304"));
        Assert.IsTrue(ex.Message.Contains("Hash must have exactly 32 bytes."));
    }
}