using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class LedgerKeyTest
{
    [TestMethod]
    public void TestLedgerKeyAccount()
    {
        var keypair = KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");
        var ledgerKey = (LedgerKeyAccount)LedgerKey.Account(keypair);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyAccount)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(ledgerKey.Account.AccountId, decodedLedgerKey.Account.AccountId);
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
    public void TestLedgerKeyClaimableBalance()
    {
        var balanceId = Util.HexToBytes("c582697b67cbec7f9ce64f4dc67bfb2bfd26318bb9f964f4d70e3f41f650b1e6");
        var ledgerKey = LedgerKey.ClaimableBalance(balanceId);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyClaimableBalance)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        CollectionAssert.AreEqual(balanceId, decodedLedgerKey.BalanceId);
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
        Assert.AreEqual(ledgerKey.ConfigSettingID.InnerValue, decodedLedgerKey.ConfigSettingID.InnerValue);
    }

    [TestMethod]
    public void TestLedgerKeyTtlCreationFromValidHashString()
    {
        var ledgerKey = (LedgerKeyTTL)LedgerKey.TTL("AQIDBAUGBwgJAAECAwQFBgcICQABAgMEBQYHCAkAAQI=");

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyTTL)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

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