using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.xdr;
using AccountEntryExtensionV1 = stellar_dotnet_sdk.AccountEntryExtensionV1;
using AccountEntryExtensionV2 = stellar_dotnet_sdk.AccountEntryExtensionV2;
using AccountEntryExtensionV3 = stellar_dotnet_sdk.AccountEntryExtensionV3;
using Asset = stellar_dotnet_sdk.Asset;
using ClaimableBalanceEntryExtensionV1 = stellar_dotnet_sdk.ClaimableBalanceEntryExtensionV1;
using Claimant = stellar_dotnet_sdk.Claimant;
using ClaimPredicate = stellar_dotnet_sdk.ClaimPredicate;
using ConfigSettingContractBandwidthV0 = stellar_dotnet_sdk.ConfigSettingContractBandwidthV0;
using ConfigSettingContractComputeV0 = stellar_dotnet_sdk.ConfigSettingContractComputeV0;
using ConfigSettingContractEventsV0 = stellar_dotnet_sdk.ConfigSettingContractEventsV0;
using ConfigSettingContractExecutionLanesV0 = stellar_dotnet_sdk.ConfigSettingContractExecutionLanesV0;
using ConfigSettingContractHistoricalDataV0 = stellar_dotnet_sdk.ConfigSettingContractHistoricalDataV0;
using ConfigSettingContractLedgerCostV0 = stellar_dotnet_sdk.ConfigSettingContractLedgerCostV0;
using EvictionIterator = stellar_dotnet_sdk.EvictionIterator;
using Hash = stellar_dotnet_sdk.Hash;
using LedgerEntry = stellar_dotnet_sdk.LedgerEntry;
using LedgerEntryExtensionV1 = stellar_dotnet_sdk.LedgerEntryExtensionV1;
using Liabilities = stellar_dotnet_sdk.Liabilities;
using LiquidityPoolConstantProductParameters = stellar_dotnet_sdk.LiquidityPoolConstantProductParameters;
using Price = stellar_dotnet_sdk.Price;
using SCString = stellar_dotnet_sdk.SCString;
using StateArchivalSettings = stellar_dotnet_sdk.StateArchivalSettings;
using TrustLineEntryExtensionV2 = stellar_dotnet_sdk.TrustLineEntryExtensionV2;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class LedgerEntryTest
{
    private const string HomeDomain = "https://example.com";

    private readonly Asset _alphaNum12Asset =
        Asset.CreateNonNativeAsset("VNDCUSD", "GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");

    private readonly Hash _hash = new(new byte[]
        { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 });

    private readonly KeyPair _inflationDest = KeyPair.Random();

    private readonly KeyPair _keyPair =
        KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");

    private readonly LedgerEntryExtensionV1 _ledgerExtensionV1 = new() { SponsoringID = KeyPair.Random() };

    private readonly KeyPair _signerSponsoringId = KeyPair.Random();
    private readonly byte[] _thresholds = { 1, 2, 3, 4 };

    private readonly TrustlineAsset _trustlineAlphaNum4Asset =
        TrustlineAsset.CreateNonNativeAsset("VNDT", "GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");

    private readonly Asset _alphaNum4Asset =
        Asset.CreateNonNativeAsset("VNDC", "GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");

    private KeyPair _issuer = KeyPair.Random();
    private readonly Asset _nativeAsset = Asset.Create("native", null, null);

    private LedgerEntryAccount InitBasicLedgerEntryAccount()
    {
        return new LedgerEntryAccount
        {
            Account = _keyPair,
            Thresholds = _thresholds,
            Balance = 10000,
            Flags = 1,
            HomeDomain = HomeDomain,
            SequenceNumber = 1,
            NumberSubEntries = 0,
            LastModifiedLedgerSeq = 10,
            InflationDest = _inflationDest
        };
    }

    private LedgerEntryTrustline InitBasicLedgerEntryTrustline()
    {
        return new LedgerEntryTrustline
        {
            LastModifiedLedgerSeq = 10000,
            LedgerExtensionV1 = _ledgerExtensionV1,
            Account = _keyPair,
            Asset = _trustlineAlphaNum4Asset,
            Balance = 1000,
            Limit = 10000,
            Flags = 1000
        };
    }

    private LedgerEntryClaimableBalance InitBasicLedgerEntryClaimableBalance()
    {
        return new LedgerEntryClaimableBalance
        {
            LastModifiedLedgerSeq = 10000,
            LedgerExtensionV1 = _ledgerExtensionV1,
            BalanceId = new Hash(new byte[]
                { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 }),
            Claimants = new Claimant[]
            {
                new()
                {
                    Destination = _keyPair,
                    Predicate = ClaimPredicate.BeforeRelativeTime(10000)
                }
            },
            Asset = _alphaNum12Asset,
            Amount = 1000
        };
    }

    private LedgerEntryContractData InitBasicLedgerEntryContractData()
    {
        return new LedgerEntryContractData
        {
            LastModifiedLedgerSeq = 10000,
            LedgerExtensionV1 = _ledgerExtensionV1,
            Key = new SCString("key 1"),
            Value = new SCUint64(1000000),
            Contract = new SCContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB"),
            Durability = ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT),
            ExtensionPoint = new ExtensionPointZero()
        };
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithTooLongHomeDomain()
    {
        var ex = Assert.ThrowsException<ArgumentException>(
            () => new LedgerEntryAccount
                { HomeDomain = "123456789012345678901234567890123" });
        Assert.IsTrue(ex.Message.Contains("Home domain cannot exceed 32 characters"));
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithTooManyBytesThresholds()
    {
        var ex = Assert.ThrowsException<ArgumentException>(
            () => new LedgerEntryAccount
                { Thresholds = new byte[] { 1, 2, 3, 5, 6 } });
        Assert.IsTrue(ex.Message.Contains("Thresholds cannot exceed 4 bytes"));
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithMissingLedgerEntryExtensionAndAccountExtensionAndEmptySigners()
    {
        var ledgerEntry = InitBasicLedgerEntryAccount();

        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.IsNull(decodedLedgerEntry.LedgerExtensionV1);
        Assert.IsNull(decodedLedgerEntry.AccountExtensionV1);
        Assert.AreEqual(0, decodedLedgerEntry.Signers.Length);
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithLedgerExtension()
    {
        var ledgerEntry = InitBasicLedgerEntryAccount();

        ledgerEntry.LedgerExtensionV1 = _ledgerExtensionV1;

        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(ledgerEntry.LedgerExtensionV1.SponsoringID.AccountId,
            decodedLedgerEntry.LedgerExtensionV1!.SponsoringID.AccountId);
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithAccountExtensionV1Only()
    {
        var ledgerEntry = InitBasicLedgerEntryAccount();

        ledgerEntry.AccountExtensionV1 = new AccountEntryExtensionV1
        {
            Liabilities = new Liabilities(100, 200)
        };

        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        // Account extensions
        // V1
        var liabilities = ledgerEntry.AccountExtensionV1.Liabilities;
        var decodedLiabilities = decodedLedgerEntry.AccountExtensionV1!.Liabilities;
        Assert.AreEqual(liabilities.Buying, decodedLiabilities.Buying);
        Assert.AreEqual(liabilities.Selling, decodedLiabilities.Selling);
        // V2
        Assert.IsNull(ledgerEntry.AccountExtensionV1.ExtensionV2);
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithAccountExtensionV1AndV2()
    {
        var ledgerEntry = InitBasicLedgerEntryAccount();

        var signerSponsoringIDs = new[] { _signerSponsoringId };

        ledgerEntry.AccountExtensionV1 = new AccountEntryExtensionV1
        {
            Liabilities = new Liabilities(100, 200),
            ExtensionV2 = new AccountEntryExtensionV2
            {
                NumberSponsored = 5,
                NumberSponsoring = 7,
                SignerSponsoringIDs = signerSponsoringIDs
            }
        };

        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        // Account extensions
        // V1
        var liabilities = ledgerEntry.AccountExtensionV1.Liabilities;
        var decodedLiabilities = decodedLedgerEntry.AccountExtensionV1!.Liabilities;
        Assert.AreEqual(liabilities.Buying, decodedLiabilities.Buying);
        Assert.AreEqual(liabilities.Selling, decodedLiabilities.Selling);

        // V2
        var extensionV2 = ledgerEntry.AccountExtensionV1.ExtensionV2;
        var decodedExtensionV2 = decodedLedgerEntry.AccountExtensionV1.ExtensionV2;

        Assert.AreEqual(extensionV2.NumberSponsored, decodedExtensionV2!.NumberSponsored);
        Assert.AreEqual(extensionV2.NumberSponsoring, decodedExtensionV2.NumberSponsoring);
        Assert.AreEqual(extensionV2.SignerSponsoringIDs.Length, decodedExtensionV2.SignerSponsoringIDs.Length);
        for (var i = 0; i < extensionV2.SignerSponsoringIDs.Length; i++)
            Assert.AreEqual(extensionV2.SignerSponsoringIDs[i].AccountId,
                decodedExtensionV2.SignerSponsoringIDs[i].AccountId);
        // V3
        Assert.IsNull(ledgerEntry.AccountExtensionV1.ExtensionV2.ExtensionV3);
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithAllThreeAccountExtensions()
    {
        var ledgerEntry = InitBasicLedgerEntryAccount();

        var signerSponsoringIDs = new[] { _signerSponsoringId };

        ledgerEntry.AccountExtensionV1 = new AccountEntryExtensionV1
        {
            Liabilities = new Liabilities(100, 200),
            ExtensionV2 = new AccountEntryExtensionV2
            {
                NumberSponsored = 5,
                NumberSponsoring = 7,
                SignerSponsoringIDs = signerSponsoringIDs,
                ExtensionV3 = new AccountEntryExtensionV3
                {
                    ExtensionPoint = new ExtensionPointZero(),
                    SequenceLedger = 11,
                    SequenceTime = 10000
                }
            }
        };

        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        // Account extensions
        // V1
        var liabilities = ledgerEntry.AccountExtensionV1.Liabilities;
        var decodedLiabilities = decodedLedgerEntry.AccountExtensionV1!.Liabilities;
        Assert.AreEqual(liabilities.Buying, decodedLiabilities.Buying);
        Assert.AreEqual(liabilities.Selling, decodedLiabilities.Selling);

        // V2
        var extensionV2 = ledgerEntry.AccountExtensionV1.ExtensionV2;
        var decodedExtensionV2 = decodedLedgerEntry.AccountExtensionV1.ExtensionV2;

        Assert.AreEqual(extensionV2.NumberSponsored, decodedExtensionV2!.NumberSponsored);
        Assert.AreEqual(extensionV2.NumberSponsoring, decodedExtensionV2.NumberSponsoring);
        Assert.AreEqual(extensionV2.SignerSponsoringIDs.Length, decodedExtensionV2.SignerSponsoringIDs.Length);
        for (var i = 0; i < extensionV2.SignerSponsoringIDs.Length; i++)
            Assert.AreEqual(extensionV2.SignerSponsoringIDs[i].AccountId,
                decodedExtensionV2.SignerSponsoringIDs[i].AccountId);
        // V3
        var extensionV3 = extensionV2.ExtensionV3;
        var decodedExtensionV3 = decodedExtensionV2.ExtensionV3;

        Assert.AreEqual(extensionV3.ExtensionPoint.ToXdrBase64(), decodedExtensionV3!.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(extensionV3.SequenceLedger, decodedExtensionV3.SequenceLedger);
        Assert.AreEqual(extensionV3.SequenceTime, decodedExtensionV3.SequenceTime);
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithAllPropertiesPopulated()
    {
        var signerSponsoringIDs = new[] { _signerSponsoringId };
        var ledgerEntry = InitBasicLedgerEntryAccount();
        ledgerEntry.LedgerExtensionV1 = _ledgerExtensionV1;
        ledgerEntry.AccountExtensionV1 = new AccountEntryExtensionV1
        {
            Liabilities = new Liabilities(100, 200),
            ExtensionV2 = new AccountEntryExtensionV2
            {
                NumberSponsored = 5,
                NumberSponsoring = 7,
                SignerSponsoringIDs = signerSponsoringIDs,
                ExtensionV3 = new AccountEntryExtensionV3
                {
                    ExtensionPoint = new ExtensionPointZero(),
                    SequenceLedger = 11,
                    SequenceTime = 10000
                }
            }
        };

        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(ledgerEntry.Balance, decodedLedgerEntry.Balance);
        Assert.AreEqual(ledgerEntry.NumberSubEntries, decodedLedgerEntry.NumberSubEntries);
        Assert.AreEqual(ledgerEntry.SequenceNumber, decodedLedgerEntry.SequenceNumber);
        Assert.AreEqual(ledgerEntry.Flags, decodedLedgerEntry.Flags);
        CollectionAssert.AreEqual(ledgerEntry.Thresholds, decodedLedgerEntry.Thresholds);
        Assert.AreEqual(ledgerEntry.HomeDomain, decodedLedgerEntry.HomeDomain);
        Assert.AreEqual(ledgerEntry.LastModifiedLedgerSeq, decodedLedgerEntry.LastModifiedLedgerSeq);
        Assert.AreEqual(ledgerEntry.Account.AccountId, decodedLedgerEntry.Account.AccountId);
        Assert.AreEqual(ledgerEntry.Signers.Length, decodedLedgerEntry.Signers.Length);
        for (var i = 0; i < ledgerEntry.Signers.Length; i++)
        {
            var signer = ledgerEntry.Signers[i];
            var decodedSigner = decodedLedgerEntry.Signers[i];
            Assert.AreEqual(signer.Key.HashX, decodedSigner.Key.HashX);
            Assert.AreEqual(signer.Weight, decodedSigner.Weight);
        }

        Assert.AreEqual(ledgerEntry.InflationDest.AccountId, decodedLedgerEntry.InflationDest.AccountId);
        Assert.AreEqual(ledgerEntry.LedgerExtensionV1.SponsoringID.AccountId,
            decodedLedgerEntry.LedgerExtensionV1.SponsoringID.AccountId);

        // Account extensions
        // V1
        var liabilities = ledgerEntry.AccountExtensionV1.Liabilities;
        var decodedLiabilities = decodedLedgerEntry.AccountExtensionV1!.Liabilities;
        Assert.AreEqual(liabilities.Buying, decodedLiabilities.Buying);
        Assert.AreEqual(liabilities.Selling, decodedLiabilities.Selling);

        // V2
        var extensionV2 = ledgerEntry.AccountExtensionV1.ExtensionV2;
        var decodedExtensionV2 = decodedLedgerEntry.AccountExtensionV1.ExtensionV2;

        Assert.AreEqual(extensionV2.NumberSponsored, decodedExtensionV2!.NumberSponsored);
        Assert.AreEqual(extensionV2.NumberSponsoring, decodedExtensionV2!.NumberSponsoring);
        Assert.AreEqual(extensionV2.SignerSponsoringIDs.Length, decodedExtensionV2.SignerSponsoringIDs.Length);
        for (var i = 0; i < extensionV2.SignerSponsoringIDs.Length; i++)
            Assert.AreEqual(extensionV2.SignerSponsoringIDs[i].AccountId,
                decodedExtensionV2.SignerSponsoringIDs[i].AccountId);

        // V3
        var extensionV3 = extensionV2.ExtensionV3;
        var decodedExtensionV3 = decodedExtensionV2.ExtensionV3;

        Assert.AreEqual(extensionV3.ExtensionPoint.ToXdrBase64(), decodedExtensionV3!.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(extensionV3.SequenceLedger, decodedExtensionV3.SequenceLedger);
        Assert.AreEqual(extensionV3.SequenceTime, decodedExtensionV3.SequenceTime);
    }

    [TestMethod]
    public void TestLedgerEntryOfferWithAllPropertiesPopulated()
    {
        var ledgerEntry = new LedgerEntryOffer
        {
            LastModifiedLedgerSeq = 100,
            SellerID = _keyPair,
            OfferID = 1000,
            Amount = 100,
            Buying = _alphaNum12Asset,
            Selling = _alphaNum12Asset,
            Price = new Price(1, 10),
            Flags = 10,
            LedgerExtensionV1 = _ledgerExtensionV1
        };

        // Act
        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry = (LedgerEntryOffer)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(ledgerEntry.OfferID, decodedLedgerEntry.OfferID);
        Assert.IsTrue(ledgerEntry.Selling.CompareTo(decodedLedgerEntry.Selling) == 0);
        Assert.IsTrue(ledgerEntry.Buying.CompareTo(decodedLedgerEntry.Buying) == 0);
        Assert.AreEqual(ledgerEntry.Price, decodedLedgerEntry.Price);
        Assert.AreEqual(ledgerEntry.Amount, decodedLedgerEntry.Amount);
        Assert.AreEqual(ledgerEntry.SellerID.AccountId, decodedLedgerEntry.SellerID.AccountId);
        Assert.AreEqual(ledgerEntry.Flags, decodedLedgerEntry.Flags);
        Assert.IsNull(decodedLedgerEntry.OfferExtension); // Currently, no offer entry extension is available
    }

    [TestMethod]
    public void TestLedgerEntryTrustlineWithTrustlineExtensionV1Only()
    {
        var ledgerEntry = InitBasicLedgerEntryTrustline();
        ledgerEntry.TrustlineExtensionV1 = new TrustlineEntryExtensionV1
        {
            Liabilities = new Liabilities(100, 100)
        };

        // Act
        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry = (LedgerEntryTrustline)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(ledgerEntry.Account.AccountId, decodedLedgerEntry.Account.AccountId);
        Assert.AreEqual(((TrustlineAsset.Wrapper)ledgerEntry.Asset).Asset,
            ((TrustlineAsset.Wrapper)decodedLedgerEntry.Asset).Asset);
        Assert.AreEqual(ledgerEntry.Flags, decodedLedgerEntry.Flags);
        Assert.AreEqual(ledgerEntry.Balance, decodedLedgerEntry.Balance);
        Assert.AreEqual(ledgerEntry.Limit, decodedLedgerEntry.Limit);
        Assert.AreEqual(ledgerEntry.LastModifiedLedgerSeq, decodedLedgerEntry.LastModifiedLedgerSeq);

        Assert.AreEqual(ledgerEntry.LedgerExtensionV1!.SponsoringID.AccountId,
            decodedLedgerEntry.LedgerExtensionV1!.SponsoringID.AccountId);

        // Trustline extensions
        // V1
        var liabilities = ledgerEntry.TrustlineExtensionV1.Liabilities;
        var decodedLiabilities = decodedLedgerEntry.TrustlineExtensionV1!.Liabilities;
        Assert.AreEqual(liabilities.Buying, decodedLiabilities.Buying);
        Assert.AreEqual(liabilities.Selling, decodedLiabilities.Selling);

        Assert.IsNull(decodedLedgerEntry.TrustlineExtensionV1.TrustlineExtensionV2);
    }

    [TestMethod]
    public void TestLedgerEntryTrustlineWithAllPropertiesPopulated()
    {
        var ledgerEntry = InitBasicLedgerEntryTrustline();
        ledgerEntry.TrustlineExtensionV1 = new TrustlineEntryExtensionV1
        {
            Liabilities = new Liabilities(100, 100),
            TrustlineExtensionV2 = new TrustLineEntryExtensionV2
            {
                LiquidityPoolUseCount = 20
            }
        };

        // Act
        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry = (LedgerEntryTrustline)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(ledgerEntry.Account.AccountId, decodedLedgerEntry.Account.AccountId);
        Assert.AreEqual(((TrustlineAsset.Wrapper)ledgerEntry.Asset).Asset,
            ((TrustlineAsset.Wrapper)decodedLedgerEntry.Asset).Asset);
        Assert.AreEqual(ledgerEntry.Flags, decodedLedgerEntry.Flags);
        Assert.AreEqual(ledgerEntry.Balance, decodedLedgerEntry.Balance);
        Assert.AreEqual(ledgerEntry.Limit, decodedLedgerEntry.Limit);
        Assert.AreEqual(ledgerEntry.LastModifiedLedgerSeq, decodedLedgerEntry.LastModifiedLedgerSeq);

        Assert.AreEqual(ledgerEntry.LedgerExtensionV1!.SponsoringID.AccountId,
            decodedLedgerEntry.LedgerExtensionV1!.SponsoringID.AccountId);

        // Trustline extensions
        // V1
        var liabilities = ledgerEntry.TrustlineExtensionV1.Liabilities;
        var decodedLiabilities = decodedLedgerEntry.TrustlineExtensionV1!.Liabilities;
        Assert.AreEqual(liabilities.Buying, decodedLiabilities.Buying);
        Assert.AreEqual(liabilities.Selling, decodedLiabilities.Selling);

        // V2
        var extensionV2 = ledgerEntry.TrustlineExtensionV1.TrustlineExtensionV2;
        var decodedExtensionV2 = decodedLedgerEntry.TrustlineExtensionV1.TrustlineExtensionV2;
        Assert.AreEqual(extensionV2.LiquidityPoolUseCount, decodedExtensionV2!.LiquidityPoolUseCount);
    }

    [TestMethod]
    public void TestLedgerEntryDataWithAllPropertiesPopulated()
    {
        var ledgerEntry = new LedgerEntryData
        {
            LastModifiedLedgerSeq = 20000,
            LedgerExtensionV1 = _ledgerExtensionV1,
            AccountID = _keyPair,
            DataName = "LedgerEntryDataName1",
            DataValue = new byte[] { 1, 2, 3, 4, 5 }
        };

        // Act
        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedEntryData = (LedgerEntryData)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        CollectionAssert.AreEqual(ledgerEntry.DataValue, decodedEntryData.DataValue);
        Assert.AreEqual(ledgerEntry.DataName, decodedEntryData.DataName);
        Assert.AreEqual(ledgerEntry.AccountID.AccountId, decodedEntryData.AccountID.AccountId);
        Assert.AreEqual(ledgerEntry.LastModifiedLedgerSeq, decodedEntryData.LastModifiedLedgerSeq);
        Assert.AreEqual(ledgerEntry.LedgerExtensionV1.SponsoringID.AccountId,
            decodedEntryData.LedgerExtensionV1!.SponsoringID.AccountId);
        Assert.IsNull(decodedEntryData.DataExtension); // Currently, no data entry extension is available
    }

    [TestMethod]
    public void TestLedgerEntryClaimableBalanceWithTooLongBalanceId()
    {
        var ledgerEntry = InitBasicLedgerEntryClaimableBalance();
        var ex = Assert.ThrowsException<ArgumentException>(
            () => ledgerEntry.BalanceId = new Hash(new byte[]
                { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 }));

        Assert.IsTrue(ex.Message.Contains("Hash must have exactly 32 bytes."));
    }

    [TestMethod]
    public void TestLedgerEntryClaimableBalanceWithTooShortBalanceId()
    {
        var ledgerEntry = InitBasicLedgerEntryClaimableBalance();
        var ex = Assert.ThrowsException<ArgumentException>(
            () => ledgerEntry.BalanceId = new Hash(new byte[]
                { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1 }));

        Assert.IsTrue(ex.Message.Contains("Hash must have exactly 32 bytes."));
    }

    [TestMethod]
    public void TestLedgerEntryClaimableBalanceWithMissingClaimableBalanceExtension()
    {
        var ledgerEntry = InitBasicLedgerEntryClaimableBalance();

        // Act
        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry =
            (LedgerEntryClaimableBalance)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(ledgerEntry.Claimants.Length, decodedLedgerEntry.Claimants.Length);
        for (var i = 0; i < ledgerEntry.Claimants.Length; i++)
        {
            Assert.AreEqual(ledgerEntry.Claimants[i].Destination.AccountId,
                decodedLedgerEntry.Claimants[i].Destination.AccountId);
            var predicate = (ClaimPredicateBeforeRelativeTime)ledgerEntry.Claimants[i].Predicate;
            var decodedPredicate = (ClaimPredicateBeforeRelativeTime)decodedLedgerEntry.Claimants[i].Predicate;
            Assert.AreEqual(predicate.Duration.InnerValue.InnerValue, decodedPredicate.Duration.InnerValue.InnerValue);
        }

        CollectionAssert.AreEqual(ledgerEntry.BalanceId.InnerValue,
            decodedLedgerEntry.BalanceId.InnerValue);
        Assert.AreEqual(ledgerEntry.Amount, decodedLedgerEntry.Amount);
        Assert.AreEqual(ledgerEntry.Asset, decodedLedgerEntry.Asset);
        Assert.AreEqual(ledgerEntry.LastModifiedLedgerSeq,
            decodedLedgerEntry.LastModifiedLedgerSeq);
        Assert.AreEqual(ledgerEntry.LedgerExtensionV1!.SponsoringID.AccountId,
            decodedLedgerEntry.LedgerExtensionV1!.SponsoringID.AccountId);
        Assert.IsNull(ledgerEntry.ClaimableBalanceEntryExtensionV1);
    }

    [TestMethod]
    public void TestLedgerEntryClaimableBalanceWithAllPropertiesPopulated()
    {
        var ledgerEntry = InitBasicLedgerEntryClaimableBalance();

        ledgerEntry.ClaimableBalanceEntryExtensionV1 = new ClaimableBalanceEntryExtensionV1
        {
            Flags = 10000
        };

        // Act
        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry =
            (LedgerEntryClaimableBalance)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(ledgerEntry.Claimants.Length, decodedLedgerEntry.Claimants.Length);
        for (var i = 0; i < ledgerEntry.Claimants.Length; i++)
        {
            Assert.AreEqual(ledgerEntry.Claimants[i].Destination.AccountId,
                decodedLedgerEntry.Claimants[i].Destination.AccountId);
            var predicate = (ClaimPredicateBeforeRelativeTime)ledgerEntry.Claimants[i].Predicate;
            var decodedPredicate = (ClaimPredicateBeforeRelativeTime)decodedLedgerEntry.Claimants[i].Predicate;
            Assert.AreEqual(predicate.Duration.InnerValue.InnerValue, decodedPredicate.Duration.InnerValue.InnerValue);
        }

        CollectionAssert.AreEqual(ledgerEntry.BalanceId.InnerValue,
            decodedLedgerEntry.BalanceId.InnerValue);
        Assert.AreEqual(ledgerEntry.Amount, decodedLedgerEntry.Amount);
        Assert.AreEqual(ledgerEntry.Asset, decodedLedgerEntry.Asset);
        Assert.AreEqual(ledgerEntry.LastModifiedLedgerSeq,
            decodedLedgerEntry.LastModifiedLedgerSeq);
        Assert.AreEqual(ledgerEntry.LedgerExtensionV1!.SponsoringID.AccountId,
            decodedLedgerEntry.LedgerExtensionV1!.SponsoringID.AccountId);
        Assert.AreEqual(ledgerEntry.ClaimableBalanceEntryExtensionV1.Flags,
            decodedLedgerEntry.ClaimableBalanceEntryExtensionV1!.Flags);
    }

    [TestMethod]
    public void TestLedgerEntryLiquidityPoolWithAllPropertiesPopulated()
    {
        var ledgerEntry = new LedgerEntryLiquidityPool
        {
            LedgerExtensionV1 = _ledgerExtensionV1,
            LastModifiedLedgerSeq = 1200,
            LiquidityPoolID = new LiquidityPoolID
            {
                Hash = _hash.InnerValue
            },
            LiquidityPoolBody = new LiquidityPoolConstantProduct
            {
                ReserveA = 1000,
                ReserveB = 2000,
                TotalPoolShares = 30000,
                PoolSharesTrustLineCount = 100,
                Parameters = new LiquidityPoolConstantProductParameters(_nativeAsset, _alphaNum4Asset, 100)
            }
        };

        // Act
        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry =
            (LedgerEntryLiquidityPool)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        CollectionAssert.AreEqual(ledgerEntry.LiquidityPoolID.Hash, decodedLedgerEntry.LiquidityPoolID.Hash);

        var product = (LiquidityPoolConstantProduct)ledgerEntry.LiquidityPoolBody;
        var decodedProduct = (LiquidityPoolConstantProduct)decodedLedgerEntry.LiquidityPoolBody;
        Assert.AreEqual(product.ReserveA, decodedProduct.ReserveA);
        Assert.AreEqual(product.ReserveB, decodedProduct.ReserveB);
        Assert.AreEqual(product.TotalPoolShares, decodedProduct.TotalPoolShares);
        Assert.AreEqual(product.PoolSharesTrustLineCount, decodedProduct.PoolSharesTrustLineCount);
        Assert.AreEqual(product.Parameters.Fee, decodedProduct.Parameters.Fee);
        // Assert.AreEqual(product.Parameters.AssetA, decodedProduct.Parameters.AssetA);
        // Assert.AreEqual(product.Parameters.AssetB, decodedProduct.Parameters.AssetB);

        Assert.AreEqual(ledgerEntry.LastModifiedLedgerSeq, ledgerEntry.LastModifiedLedgerSeq);
        Assert.AreEqual(ledgerEntry.LedgerExtensionV1.SponsoringID.AccountId,
            decodedLedgerEntry.LedgerExtensionV1!.SponsoringID.AccountId);
    }

    [TestMethod]
    public void TestLedgerEntryContractDataWithWithContractBeingAContractAddress()
    {
        var ledgerEntry = InitBasicLedgerEntryContractData();

        // Act
        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry =
            (LedgerEntryContractData)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(((SCContractId)ledgerEntry.Contract).InnerValue,
            ((SCContractId)decodedLedgerEntry.Contract).InnerValue);
        Assert.AreEqual(ledgerEntry.Durability.InnerValue, decodedLedgerEntry.Durability.InnerValue);
        Assert.AreEqual(((SCString)ledgerEntry.Key).InnerValue,
            ((SCString)decodedLedgerEntry.Key).InnerValue);
        Assert.AreEqual(((SCUint64)ledgerEntry.Value).InnerValue,
            ((SCUint64)decodedLedgerEntry.Value).InnerValue);
        Assert.AreEqual(ledgerEntry.LedgerExtensionV1!.SponsoringID.AccountId,
            decodedLedgerEntry.LedgerExtensionV1!.SponsoringID.AccountId);
        Assert.AreEqual(ledgerEntry.LastModifiedLedgerSeq, decodedLedgerEntry.LastModifiedLedgerSeq);
    }

    [TestMethod]
    public void TestLedgerEntryContractDataWithWithContractBeingAnAccountAddress()
    {
        var ledgerEntry = InitBasicLedgerEntryContractData();
        ledgerEntry.Contract = new SCAccountId("GCZFMH32MF5EAWETZTKF3ZV5SEVJPI53UEMDNSW55WBR75GMZJU4U573");

        // Act
        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry =
            (LedgerEntryContractData)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(((SCAccountId)ledgerEntry.Contract).InnerValue,
            ((SCAccountId)decodedLedgerEntry.Contract).InnerValue);
        Assert.AreEqual(ledgerEntry.Durability.InnerValue, decodedLedgerEntry.Durability.InnerValue);
        Assert.AreEqual(((SCString)ledgerEntry.Key).InnerValue,
            ((SCString)decodedLedgerEntry.Key).InnerValue);
        Assert.AreEqual(((SCUint64)ledgerEntry.Value).InnerValue,
            ((SCUint64)decodedLedgerEntry.Value).InnerValue);
        Assert.AreEqual(ledgerEntry.LedgerExtensionV1!.SponsoringID.AccountId,
            decodedLedgerEntry.LedgerExtensionV1!.SponsoringID.AccountId);
        Assert.AreEqual(ledgerEntry.LastModifiedLedgerSeq, decodedLedgerEntry.LastModifiedLedgerSeq);
    }

    [TestMethod]
    public void TestLedgerEntryContractCodeWithAllPropertiesPopulated()
    {
        var ledgerEntry = new LedgerEntryContractCode
        {
            LastModifiedLedgerSeq = 1032,
            Code = new byte[] { 1, 2, 3 },
            LedgerExtensionV1 = _ledgerExtensionV1,
            ExtensionPoint = new ExtensionPointZero(),
            Hash = _hash
        };

        // Act
        var entryXdrBase64 = ledgerEntry.ToXdrBase64();
        var decodedLedgerEntry =
            (LedgerEntryContractCode)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        CollectionAssert.AreEqual(ledgerEntry.Code, decodedLedgerEntry.Code);
        CollectionAssert.AreEqual(ledgerEntry.Hash.InnerValue, decodedLedgerEntry.Hash.InnerValue);
        Assert.AreEqual(ledgerEntry.LedgerExtensionV1.SponsoringID.AccountId,
            decodedLedgerEntry.LedgerExtensionV1!.SponsoringID.AccountId);
        Assert.AreEqual(ledgerEntry.LastModifiedLedgerSeq, decodedLedgerEntry.LastModifiedLedgerSeq);
    }


    [TestMethod]
    public void TestLedgerEntryConfigSettingContractBandwidthV0()
    {
        var configSetting = new ConfigSettingContractBandwidthV0
        {
            LastModifiedLedgerSeq = 10,
            LedgerExtensionV1 = null,
            LedgerMaxTxsSizeBytes = 1024,
            TxMaxSizeBytes = 1024,
            FeeTxSize1KB = 100
        };

        // Act
        var ledgerEntryXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting = (ConfigSettingContractBandwidthV0)LedgerEntry.FromXdrBase64(ledgerEntryXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.TxMaxSizeBytes, decodedConfigSetting.TxMaxSizeBytes);
        Assert.AreEqual(configSetting.FeeTxSize1KB, decodedConfigSetting.FeeTxSize1KB);
        Assert.AreEqual(configSetting.LedgerMaxTxsSizeBytes, decodedConfigSetting.LedgerMaxTxsSizeBytes);
    }

    [TestMethod]
    public void TestConfigSettingContractCostParamsMemoryBytes()
    {
        var configSetting =
            new ConfigSettingContractCostParamsMemoryBytes
            {
                LastModifiedLedgerSeq = 100,
                paramEntries = new ConfigSettingContractCostParamEntry[]
                {
                    new()
                    {
                        LinearTerm = 100,
                        ExtensionPoint = new ExtensionPointZero(),
                        ConstTerm = 100000
                    },
                    new()
                    {
                        LinearTerm = 200,
                        ConstTerm = 400,
                        ExtensionPoint = new ExtensionPointZero()
                    }
                }
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting =
            (ConfigSettingContractCostParamsMemoryBytes)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.paramEntries.Length, decodedConfigSetting.paramEntries.Length);
        for (var i = 0; i < configSetting.paramEntries.Length; i++)
        {
            var paramEntry = configSetting.paramEntries[i];
            var decodedParamEntry = decodedConfigSetting.paramEntries[i];
            Assert.AreEqual(paramEntry.ConstTerm, decodedParamEntry.ConstTerm);
            Assert.AreEqual(paramEntry.LinearTerm, decodedParamEntry.LinearTerm);
        }
    }

    [TestMethod]
    public void TestConfigSettingContractCostParamsCpuInstructions()
    {
        var configSetting =
            new ConfigSettingContractCostParamsCpuInstructions
            {
                LastModifiedLedgerSeq = 500,
                paramEntries = new ConfigSettingContractCostParamEntry[]
                {
                    new()
                    {
                        LinearTerm = 120,
                        ExtensionPoint = new ExtensionPointZero(),
                        ConstTerm = 108000
                    },
                    new()
                    {
                        LinearTerm = 2300,
                        ConstTerm = 3400,
                        ExtensionPoint = new ExtensionPointZero()
                    }
                }
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting =
            (ConfigSettingContractCostParamsCpuInstructions)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.paramEntries.Length, decodedConfigSetting.paramEntries.Length);
        for (var i = 0; i < configSetting.paramEntries.Length; i++)
        {
            var paramEntry = configSetting.paramEntries[i];
            var decodedParamEntry = decodedConfigSetting.paramEntries[i];
            Assert.AreEqual(paramEntry.ConstTerm, decodedParamEntry.ConstTerm);
            Assert.AreEqual(paramEntry.LinearTerm, decodedParamEntry.LinearTerm);
        }
    }

    [TestMethod]
    public void TestConfigSettingContractComputeV0()
    {
        var configSetting =
            new ConfigSettingContractComputeV0
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null,
                LedgerMaxInstructions = 110,
                TxMaxInstructions = 1000,
                FeeRatePerInstructionsIncrement = 1000,
                TxMemoryLimit = 1000
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting = (ConfigSettingContractComputeV0)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.LedgerMaxInstructions, decodedConfigSetting.LedgerMaxInstructions);
        Assert.AreEqual(configSetting.TxMaxInstructions, decodedConfigSetting.TxMaxInstructions);
        Assert.AreEqual(configSetting.TxMemoryLimit, decodedConfigSetting.TxMemoryLimit);
        Assert.AreEqual(configSetting.FeeRatePerInstructionsIncrement,
            decodedConfigSetting.FeeRatePerInstructionsIncrement);
    }

    [TestMethod]
    public void TestConfigSettingContractEventsV0()
    {
        var configSetting =
            new ConfigSettingContractEventsV0
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null,
                TxMaxContractEventsSizeBytes = 333,
                FeeContractEvents1KB = 1024
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting = (ConfigSettingContractEventsV0)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.TxMaxContractEventsSizeBytes, decodedConfigSetting.TxMaxContractEventsSizeBytes);
        Assert.AreEqual(configSetting.FeeContractEvents1KB, decodedConfigSetting.FeeContractEvents1KB);
    }

    [TestMethod]
    public void TestConfigSettingContractExecutionLanesV0()
    {
        var configSetting =
            new ConfigSettingContractExecutionLanesV0
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null,
                LedgerMaxTxCount = 10000
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting =
            (ConfigSettingContractExecutionLanesV0)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.LedgerMaxTxCount, decodedConfigSetting.LedgerMaxTxCount);
    }

    [TestMethod]
    public void TestConfigSettingContractHistoricalDataV0()
    {
        var configSetting =
            new ConfigSettingContractHistoricalDataV0
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null,
                FeeHistorical1KB = 10000
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting =
            (ConfigSettingContractHistoricalDataV0)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.FeeHistorical1KB, decodedConfigSetting.FeeHistorical1KB);
    }

    [TestMethod]
    public void TestConfigSettingContractLedgerCostV0()
    {
        var configSetting =
            new ConfigSettingContractLedgerCostV0
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null,
                LedgerMaxReadLedgerEntries = 123,
                LedgerMaxReadBytes = 22,
                LedgerMaxWriteLedgerEntries = 11,
                LedgerMaxWriteBytes = 34,
                TxMaxReadLedgerEntries = 55,
                TxMaxReadBytes = 110,
                TxMaxWriteLedgerEntries = 120,
                TxMaxWriteBytes = 130,
                FeeReadLedgerEntry = 140,
                FeeWriteLedgerEntry = 150,
                FeeRead1KB = 160,
                BucketListTargetSizeBytes = 170,
                WriteFee1KBBucketListLow = 180,
                WriteFee1KBBucketListHigh = 220,
                BucketListWriteFeeGrowthFactor = 2230
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting = (ConfigSettingContractLedgerCostV0)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.LedgerMaxReadLedgerEntries, decodedConfigSetting.LedgerMaxReadLedgerEntries);
        Assert.AreEqual(configSetting.LedgerMaxReadBytes, decodedConfigSetting.LedgerMaxReadBytes);
        Assert.AreEqual(configSetting.LedgerMaxWriteLedgerEntries, decodedConfigSetting.LedgerMaxWriteLedgerEntries);
        Assert.AreEqual(configSetting.LedgerMaxWriteBytes, decodedConfigSetting.LedgerMaxWriteBytes);
        Assert.AreEqual(configSetting.TxMaxReadLedgerEntries, decodedConfigSetting.TxMaxReadLedgerEntries);
        Assert.AreEqual(configSetting.TxMaxReadBytes, decodedConfigSetting.TxMaxReadBytes);
        Assert.AreEqual(configSetting.TxMaxWriteLedgerEntries, decodedConfigSetting.TxMaxWriteLedgerEntries);
        Assert.AreEqual(configSetting.TxMaxWriteBytes, decodedConfigSetting.TxMaxWriteBytes);
        Assert.AreEqual(configSetting.FeeReadLedgerEntry, decodedConfigSetting.FeeReadLedgerEntry);
        Assert.AreEqual(configSetting.FeeWriteLedgerEntry, decodedConfigSetting.FeeWriteLedgerEntry);
        Assert.AreEqual(configSetting.FeeRead1KB, decodedConfigSetting.FeeRead1KB);
        Assert.AreEqual(configSetting.BucketListTargetSizeBytes, decodedConfigSetting.BucketListTargetSizeBytes);
        Assert.AreEqual(configSetting.WriteFee1KBBucketListLow, decodedConfigSetting.WriteFee1KBBucketListLow);
        Assert.AreEqual(configSetting.WriteFee1KBBucketListHigh, decodedConfigSetting.WriteFee1KBBucketListHigh);
        Assert.AreEqual(configSetting.BucketListWriteFeeGrowthFactor,
            decodedConfigSetting.BucketListWriteFeeGrowthFactor);
    }

    [TestMethod]
    public void TestStateArchivalSettings()
    {
        var configSetting =
            new StateArchivalSettings
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null,
                MaxEntryTTL = 23,
                MinTemporaryTTL = 12,
                MinPersistentTTL = 111,
                PersistentRentRateDenominator = 120,
                TempRentRateDenominator = 130,
                MaxEntriesToArchive = 140,
                BucketListSizeWindowSampleSize = 330,
                EvictionScanSize = 340,
                StartingEvictionScanLevel = 3440
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting = (StateArchivalSettings)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.MaxEntryTTL, decodedConfigSetting.MaxEntryTTL);
        Assert.AreEqual(configSetting.MinTemporaryTTL, decodedConfigSetting.MinTemporaryTTL);
        Assert.AreEqual(configSetting.MinPersistentTTL, decodedConfigSetting.MinPersistentTTL);
        Assert.AreEqual(configSetting.PersistentRentRateDenominator,
            decodedConfigSetting.PersistentRentRateDenominator);
        Assert.AreEqual(configSetting.TempRentRateDenominator, decodedConfigSetting.TempRentRateDenominator);
        Assert.AreEqual(configSetting.MaxEntriesToArchive, decodedConfigSetting.MaxEntriesToArchive);
        Assert.AreEqual(configSetting.BucketListSizeWindowSampleSize,
            decodedConfigSetting.BucketListSizeWindowSampleSize);
        Assert.AreEqual(configSetting.EvictionScanSize, decodedConfigSetting.EvictionScanSize);
        Assert.AreEqual(configSetting.StartingEvictionScanLevel, decodedConfigSetting.StartingEvictionScanLevel);
    }

    [TestMethod]
    public void TestConfigSettingEvictionIterator()
    {
        var configSetting =
            new EvictionIterator
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null,
                BucketListLevel = 10,
                IsCurrBucket = false,
                BucketFileOffset = 10
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting = (EvictionIterator)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.BucketFileOffset, decodedConfigSetting.BucketFileOffset);
        Assert.AreEqual(configSetting.BucketListLevel, decodedConfigSetting.BucketListLevel);
        Assert.AreEqual(configSetting.IsCurrBucket, decodedConfigSetting.IsCurrBucket);
    }

    [TestMethod]
    public void TestConfigSettingContractMaxSizeBytes()
    {
        var configSetting =
            new ConfigSettingContractMaxSizeBytes(100)
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting = (ConfigSettingContractMaxSizeBytes)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.InnerValue, decodedConfigSetting.InnerValue);
    }

    [TestMethod]
    public void TestConfigSettingContractDataKeySizeBytes()
    {
        var configSetting =
            new ConfigSettingContractDataKeySizeBytes(100)
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting =
            (ConfigSettingContractDataKeySizeBytes)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.InnerValue, decodedConfigSetting.InnerValue);
    }

    [TestMethod]
    public void TestConfigSettingContractDataEntrySizeBytes()
    {
        var configSetting =
            new ConfigSettingContractDataEntrySizeBytes(100)
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting =
            (ConfigSettingContractDataEntrySizeBytes)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        Assert.AreEqual(configSetting.InnerValue, decodedConfigSetting.InnerValue);
    }

    [TestMethod]
    public void TestConfigSettingBucketListSizeWindow()
    {
        var configSetting =
            new ConfigSettingBucketListSizeWindow(new ulong[] { 1, 2, 3, 4 })
            {
                LastModifiedLedgerSeq = 500,
                LedgerExtensionV1 = null
            };

        // Act
        var configSettingXdrBase64 = configSetting.ToXdrBase64();
        var decodedConfigSetting = (ConfigSettingBucketListSizeWindow)LedgerEntry.FromXdrBase64(configSettingXdrBase64);

        // Assert
        Assert.AreEqual(configSetting.LastModifiedLedgerSeq, decodedConfigSetting.LastModifiedLedgerSeq);
        Assert.IsNull(decodedConfigSetting.LedgerExtensionV1);
        CollectionAssert.AreEqual(configSetting.InnerValue, decodedConfigSetting.InnerValue);
    }


    [TestMethod]
    public void TestLedgerEntryTtlWithAllPropertiesPopulated()
    {
        var entryTtl = new LedgerEntryTTL
        {
            LastModifiedLedgerSeq = 11032,
            LedgerExtensionV1 = _ledgerExtensionV1,
            KeyHash = _hash,
            LiveUntilLedgerSequence = 1000
        };

        // Act
        var entryTtlXdrBase64 = entryTtl.ToXdrBase64();
        var decodedTtl =
            (LedgerEntryTTL)LedgerEntry.FromXdrBase64(entryTtlXdrBase64);

        // Assert
        Assert.AreEqual(entryTtl.LiveUntilLedgerSequence, decodedTtl.LiveUntilLedgerSequence);
        CollectionAssert.AreEqual(entryTtl.KeyHash.InnerValue, decodedTtl.KeyHash.InnerValue);
        Assert.AreEqual(entryTtl.LedgerExtensionV1!.SponsoringID.AccountId,
            decodedTtl.LedgerExtensionV1!.SponsoringID.AccountId);
        Assert.AreEqual(entryTtl.LastModifiedLedgerSeq, decodedTtl.LastModifiedLedgerSeq);
    }
}