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

/// <summary>
/// Unit tests for <see cref="LedgerKey"/> class and related ledger key types.
/// </summary>
[TestClass]
public class LedgerKeyTest
{
    /// <summary>
    /// Verifies that Account ledger key round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_LedgerKeyAccount_RoundTripsCorrectly()
    {
        // Arrange
        var keypair = KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");
        var ledgerKey = (LedgerKeyAccount)LedgerKey.Account(keypair);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyAccount)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(ledgerKey.Account.AccountId, decodedLedgerKey.Account.AccountId);
    }

    /// <summary>
    /// Verifies that Data ledger key factory method throws ArgumentException when data name exceeds 64 characters.
    /// </summary>
    [TestMethod]
    public void Data_WithTooLongName_ThrowsArgumentException()
    {
        // Arrange
        var keypair = KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");
        const string dataName = "This is a 73 characters long string which is too strong for String64 type";

        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentException>(() => LedgerKey.Data(keypair, dataName));
        Assert.IsTrue(ex.Message.Contains("Data name cannot exceed 64 characters."));
    }

    /// <summary>
    /// Verifies that Data ledger key with valid name round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_LedgerKeyDataWithValidName_RoundTripsCorrectly()
    {
        // Arrange
        var keypair = KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");
        var ledgerKey = (LedgerKeyData)LedgerKey.Data(keypair, "Test Data");

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyData)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(ledgerKey.DataName, decodedLedgerKey.DataName);
        Assert.AreEqual(ledgerKey.Account.AccountId, decodedLedgerKey.Account.AccountId);
    }

    /// <summary>
    /// Verifies that Offer ledger key round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_LedgerKeyOffer_RoundTripsCorrectly()
    {
        // Arrange
        var keypair = KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");
        var ledgerKey = (LedgerKeyOffer)LedgerKey.Offer(keypair, 1234);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyOffer)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(ledgerKey.OfferId, decodedLedgerKey.OfferId);
        Assert.AreEqual(ledgerKey.Seller.AccountId, decodedLedgerKey.Seller.AccountId);
    }

    /// <summary>
    /// Verifies that Trustline ledger key round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_LedgerKeyTrustline_RoundTripsCorrectly()
    {
        // Arrange
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

    /// <summary>
    /// Verifies that ClaimableBalance ledger key created from valid string round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    [DataRow("00000000c582697b67cbec7f9ce64f4dc67bfb2bfd26318bb9f964f4d70e3f41f650b1e6")]
    public void FromXdrBase64_LedgerKeyClaimableBalanceFromValidString_RoundTripsCorrectly(string id)
    {
        // Arrange
        var ledgerKey = LedgerKey.ClaimableBalance(id);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyClaimableBalance)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(id, decodedLedgerKey.BalanceId.ToLower());
    }

    /// <summary>
    /// Verifies that ClaimableBalance ledger key factory method throws ArgumentException when given invalid string.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4TU")]
    [DataRow("d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780")]
    [DataRow("00d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780")]
    public void ClaimableBalance_WithInvalidString_ThrowsArgumentException(string id)
    {
        // Arrange & Act & Assert
        _ = LedgerKey.ClaimableBalance(id);
    }

    /// <summary>
    /// Verifies that LiquidityPool ledger key round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_LedgerKeyLiquidityPool_RoundTripsCorrectly()
    {
        // Arrange
        var hash = new byte[]
            { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 };
        var ledgerKey = LedgerKey.LiquidityPool(new LiquidityPoolId(hash));

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyLiquidityPool)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        CollectionAssert.AreEqual(hash, decodedLedgerKey.LiquidityPoolId.Hash);
    }

    /// <summary>
    /// Verifies that ContractData ledger key with contract being ContractId round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_LedgerKeyContractDataWithContractId_RoundTripsCorrectly()
    {
        // Arrange
        var contractId = new ScContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB");
        var key = new SCSymbol("kk");

        var durability = ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT);
        var ledgerKey = (LedgerKeyContractData)LedgerKey.ContractData(contractId, key, durability);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyContractData)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(contractId.InnerValue, ((ScContractId)decodedLedgerKey.Contract).InnerValue);
        Assert.AreEqual(key.InnerValue, ((SCSymbol)decodedLedgerKey.Key).InnerValue);
        Assert.AreEqual(ledgerKey.Durability.InnerValue, decodedLedgerKey.Durability.InnerValue);
    }

    /// <summary>
    /// Verifies that ContractData ledger key with contract being AccountId round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_LedgerKeyContractDataWithAccountId_RoundTripsCorrectly()
    {
        // Arrange
        var accountId = new ScAccountId("GCZFMH32MF5EAWETZTKF3ZV5SEVJPI53UEMDNSW55WBR75GMZJU4U573");
        var key = new SCInt64(122);

        var durability = ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.TEMPORARY);
        var ledgerKey = (LedgerKeyContractData)LedgerKey.ContractData(accountId, key, durability);

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyContractData)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(accountId.InnerValue, ((ScAccountId)decodedLedgerKey.Contract).InnerValue);
        Assert.AreEqual(key.InnerValue, ((SCInt64)decodedLedgerKey.Key).InnerValue);
        Assert.AreEqual(ledgerKey.Durability.InnerValue, decodedLedgerKey.Durability.InnerValue);
    }

    /// <summary>
    /// Verifies that ContractCode ledger key created from valid hash string round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_LedgerKeyContractCodeFromValidHashString_RoundTripsCorrectly()
    {
        // Arrange
        var ledgerKey =
            (LedgerKeyContractCode)LedgerKey.ContractCode(
                "0102030405060708090001020304050607080900010203040506070809000102");

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyContractCode)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        CollectionAssert.AreEqual(ledgerKey.Hash, decodedLedgerKey.Hash);
    }

    /// <summary>
    /// Verifies that ContractCode ledger key factory method throws ArgumentOutOfRangeException when hash string has invalid length.
    /// </summary>
    [TestMethod]
    public void ContractCode_WithInvalidHashStringLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            LedgerKey.ContractCode("01020304050607080900010203040506070809000102030405060708090002"));
        Assert.IsTrue(ex.Message.Contains("Hash must have exactly 32 bytes."));
    }

    /// <summary>
    /// Verifies that ConfigSetting ledger key round-trips correctly through XDR serialization for various config setting IDs.
    /// </summary>
    [TestMethod]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_MAX_SIZE_BYTES)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_COMPUTE_V0)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_LEDGER_COST_V0)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_HISTORICAL_DATA_V0)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_EVENTS_V0)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_BANDWIDTH_V0)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_COST_PARAMS_CPU_INSTRUCTIONS)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_COST_PARAMS_MEMORY_BYTES)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_DATA_KEY_SIZE_BYTES)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_DATA_ENTRY_SIZE_BYTES)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_STATE_ARCHIVAL)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_EXECUTION_LANES)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_LIVE_SOROBAN_STATE_SIZE_WINDOW)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_EVICTION_ITERATOR)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_PARALLEL_COMPUTE_V0)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_LEDGER_COST_EXT_V0)]
    [DataRow(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_SCP_TIMING)]
    public void FromXdrBase64_LedgerKeyConfigSetting_RoundTripsCorrectly(ConfigSettingID.ConfigSettingIDEnum configSettingId)
    {
        // Arrange
        var ledgerKey = (LedgerKeyConfigSetting)LedgerKey.ConfigSetting(new ConfigSettingID
        {
            InnerValue = configSettingId,
        });

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyConfigSetting)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        Assert.AreEqual(ledgerKey.ConfigSettingId.InnerValue, decodedLedgerKey.ConfigSettingId.InnerValue);
    }

    /// <summary>
    /// Verifies that TTL ledger key created from valid hash string round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_LedgerKeyTtlFromValidHashString_RoundTripsCorrectly()
    {
        // Arrange
        var ledgerKey = (LedgerKeyTtl)LedgerKey.Ttl("AQIDBAUGBwgJAAECAwQFBgcICQABAgMEBQYHCAkAAQI=");

        // Act
        var ledgerKeyXdrBase64 = ledgerKey.ToXdrBase64();
        var decodedLedgerKey = (LedgerKeyTtl)LedgerKey.FromXdrBase64(ledgerKeyXdrBase64);

        // Assert
        CollectionAssert.AreEqual(ledgerKey.Key, decodedLedgerKey.Key);
    }

    /// <summary>
    /// Verifies that ContractCode ledger key factory method throws ArgumentOutOfRangeException when hash string has invalid length (for TTL test case).
    /// </summary>
    [TestMethod]
    public void ContractCode_WithInvalidHashStringLengthForTtl_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            LedgerKey.ContractCode("01020304050607080900010203040506070809000102030405060708090001020304"));
        Assert.IsTrue(ex.Message.Contains("Hash must have exactly 32 bytes."));
    }
}