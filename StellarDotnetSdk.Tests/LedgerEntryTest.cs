using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Claimants;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using AccountEntryExtensionV1 = StellarDotnetSdk.Xdr.AccountEntryExtensionV1;
using AccountEntryExtensionV2 = StellarDotnetSdk.Xdr.AccountEntryExtensionV2;
using AccountEntryExtensionV3 = StellarDotnetSdk.Xdr.AccountEntryExtensionV3;
using Asset = StellarDotnetSdk.Assets.Asset;
using ClaimableBalanceEntryExtensionV1 = StellarDotnetSdk.Xdr.ClaimableBalanceEntryExtensionV1;
using Claimant = StellarDotnetSdk.Xdr.Claimant;
using ClaimPredicate = StellarDotnetSdk.Xdr.ClaimPredicate;
using ConfigSettingContractLedgerCostExtV0 = StellarDotnetSdk.Xdr.ConfigSettingContractLedgerCostExtV0;
using ConfigSettingContractParallelComputeV0 = StellarDotnetSdk.LedgerEntries.ConfigSettingContractParallelComputeV0;
using ContractCodeCostInputs = StellarDotnetSdk.Xdr.ContractCodeCostInputs;
using EvictionIterator = StellarDotnetSdk.LedgerEntries.EvictionIterator;
using ExtensionPoint = StellarDotnetSdk.Xdr.ExtensionPoint;
using Int32 = StellarDotnetSdk.Xdr.Int32;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using LedgerEntry = StellarDotnetSdk.LedgerEntries.LedgerEntry;
using Liabilities = StellarDotnetSdk.Xdr.Liabilities;
using SCAddress = StellarDotnetSdk.Xdr.SCAddress;
using SCString = StellarDotnetSdk.Soroban.SCString;
using SCVal = StellarDotnetSdk.Xdr.SCVal;
using StateArchivalSettings = StellarDotnetSdk.LedgerEntries.StateArchivalSettings;
using String64 = StellarDotnetSdk.Xdr.String64;
using TrustLineEntryExtensionV2 = StellarDotnetSdk.Xdr.TrustLineEntryExtensionV2;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class LedgerEntryTest
{
    private const string ContractId = "CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB";

    private readonly Asset _alphaNum12Asset =
        Asset.CreateNonNativeAsset("VNDCUSD", "GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");

    private readonly Asset _alphaNum4Asset =
        Asset.CreateNonNativeAsset("VNDC", "GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");

    private readonly KeyPair _keyPair =
        KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");

    private readonly Asset _nativeAsset = new AssetTypeNative();

    private readonly KeyPair _signerSponsoringId = KeyPair.Random();

    private readonly TrustlineAsset _trustlineAlphaNum4Asset =
        TrustlineAsset.CreateNonNativeAsset("VNDT", "GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7");

    private ClaimableBalanceEntry InitBasicXdrClaimableBalanceEntry()
    {
        return new ClaimableBalanceEntry
        {
            BalanceID = new ClaimableBalanceID
            {
                Discriminant = ClaimableBalanceIDType.Create(ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum
                    .CLAIMABLE_BALANCE_ID_TYPE_V0),
                V0 = new Hash([
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2,
                ]),
            },
            Claimants =
            [
                new Claimant
                {
                    Discriminant = ClaimantType.Create(ClaimantType.ClaimantTypeEnum.CLAIMANT_TYPE_V0),
                    V0 = new Claimant.ClaimantV0
                    {
                        Destination = new AccountID(_keyPair.XdrPublicKey),
                        Predicate = new ClaimPredicate
                        {
                            Discriminant = ClaimPredicateType.Create(ClaimPredicateType.ClaimPredicateTypeEnum
                                .CLAIM_PREDICATE_BEFORE_RELATIVE_TIME),
                            RelBefore = new Int64(10000),
                        },
                    },
                },
            ],
            Asset = _alphaNum12Asset.ToXdr(),
            Amount = new Int64(1000),
            Ext = new ClaimableBalanceEntry.ClaimableBalanceEntryExt
            {
                Discriminant = 0,
            },
        };
    }

    private StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData InitBasicAccountEntry()
    {
        var accountEntry = new AccountEntry
        {
            AccountID = new AccountID(_keyPair.XdrPublicKey),
            Balance = new Int64(1000),
            SeqNum = new SequenceNumber(new Int64(1)),
            NumSubEntries = new Uint32(1),
            InflationDest = null,
            Flags = new Uint32(1),
            HomeDomain = new String32(""),
            Thresholds = new Thresholds([1, 2, 3, 0]),
            Signers = [],
            Ext = new AccountEntry.AccountEntryExt { Discriminant = 0 },
        };
        return new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT),
            Account = accountEntry,
        };
    }

    private ContractDataEntry InitBasicContractDataEntry()
    {
        return new ContractDataEntry
        {
            Ext = new ExtensionPoint(),
            Contract = new ScContractId(ContractId).ToXdr(),
            Key = new SCVal
            {
                Discriminant = SCValType.Create(SCValType.SCValTypeEnum.SCV_STRING),
                Str = new StellarDotnetSdk.Xdr.SCString("key 1"),
            },
            Durability =
                ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT),
            Val = new SCVal
            {
                Discriminant = SCValType.Create(SCValType.SCValTypeEnum.SCV_U64),
                U64 = new Uint64(1000000),
            },
        };
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithTooLongHomeDomain()
    {
        var xdrLedgerEntry = InitBasicAccountEntry();
        xdrLedgerEntry.Account.HomeDomain = new String32("123456789012345678901234567890123456");
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntry);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var ex = Assert.ThrowsException<ArgumentException>(() => LedgerEntry.FromXdrBase64(entryXdrBase64));
        Assert.IsTrue(ex.Message.Contains("Home domain cannot exceed 32 characters"));
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithMissingLedgerEntryExtensionAndAccountExtensionAndEmptySigners()
    {
        var xdrLedgerEntry = InitBasicAccountEntry();
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntry);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());

        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.IsNull(decodedLedgerEntry.LedgerExtensionV1);
        Assert.IsNull(decodedLedgerEntry.AccountExtensionV1);
        Assert.AreEqual(0, decodedLedgerEntry.Signers.Length);
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithAccountExtensionV1Only()
    {
        var xdrLiabilities = new Liabilities
        {
            Buying = new Int64(100),
            Selling = new Int64(200),
        };
        var xdrLedgerEntry = InitBasicAccountEntry();
        xdrLedgerEntry.Account.Ext = new AccountEntry.AccountEntryExt
        {
            Discriminant = 1,
            V1 = new AccountEntryExtensionV1
            {
                Ext = new AccountEntryExtensionV1.AccountEntryExtensionV1Ext
                {
                    Discriminant = 0,
                },
                Liabilities = xdrLiabilities,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntry);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());

        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        // Account extensions
        // V1
        var decodedExtensionV1 = decodedLedgerEntry.AccountExtensionV1;
        Assert.IsNotNull(decodedExtensionV1);
        var decodedLiabilities = decodedExtensionV1.Liabilities;
        Assert.AreEqual(xdrLiabilities.Buying.InnerValue, decodedLiabilities.Buying);
        Assert.AreEqual(xdrLiabilities.Selling.InnerValue, decodedLiabilities.Selling);
        // V2
        Assert.IsNull(decodedExtensionV1.ExtensionV2);
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithAccountExtensionV1AndV2()
    {
        var xdrLiabilities = new Liabilities
        {
            Buying = new Int64(100),
            Selling = new Int64(200),
        };
        var xdrLedgerEntry = InitBasicAccountEntry();
        var xdrExtensionV2 = new AccountEntryExtensionV2
        {
            NumSponsored = new Uint32(5),
            NumSponsoring = new Uint32(7),
            SignerSponsoringIDs =
            [
                new SponsorshipDescriptor
                {
                    InnerValue = new AccountID(_signerSponsoringId.XdrPublicKey),
                },
            ],
            Ext = new AccountEntryExtensionV2.AccountEntryExtensionV2Ext
            {
                Discriminant = 0,
            },
        };
        xdrLedgerEntry.Account.Ext = new AccountEntry.AccountEntryExt
        {
            Discriminant = 1,
            V1 = new AccountEntryExtensionV1
            {
                Ext = new AccountEntryExtensionV1.AccountEntryExtensionV1Ext
                {
                    Discriminant = 2,
                    V2 = xdrExtensionV2,
                },
                Liabilities = xdrLiabilities,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntry);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());

        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        // Account extensions
        // V1
        var decodedExtensionV1 = decodedLedgerEntry.AccountExtensionV1;
        Assert.IsNotNull(decodedExtensionV1);
        var decodedLiabilities = decodedExtensionV1.Liabilities;
        Assert.AreEqual(xdrLiabilities.Buying.InnerValue, decodedLiabilities.Buying);
        Assert.AreEqual(xdrLiabilities.Selling.InnerValue, decodedLiabilities.Selling);

        // V2
        var decodedExtensionV2 = decodedExtensionV1.ExtensionV2;

        Assert.IsNotNull(decodedExtensionV2);
        Assert.AreEqual(xdrExtensionV2.NumSponsored.InnerValue,
            decodedExtensionV2.NumberSponsored);
        Assert.AreEqual(xdrExtensionV2.NumSponsoring.InnerValue,
            decodedExtensionV2.NumberSponsoring);
        Assert.AreEqual(xdrExtensionV2.SignerSponsoringIDs.Length,
            decodedExtensionV2.SignerSponsoringIDs.Length);
        for (var i = 0; i < xdrExtensionV2.SignerSponsoringIDs.Length; i++)
        {
            var decodedId = decodedExtensionV2.SignerSponsoringIDs[i];
            Assert.IsNotNull(decodedId);
            CollectionAssert.AreEqual(
                xdrExtensionV2.SignerSponsoringIDs[i].InnerValue.InnerValue.Ed25519.InnerValue,
                decodedId.XdrPublicKey.Ed25519.InnerValue);
        }

        // V3
        Assert.IsNull(decodedExtensionV2.ExtensionV3);
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithAllThreeAccountExtensions()
    {
        var xdrLiabilities = new Liabilities
        {
            Buying = new Int64(100),
            Selling = new Int64(200),
        };
        var xdrLedgerEntry = InitBasicAccountEntry();
        var xdrExtensionV3 = new AccountEntryExtensionV3
        {
            Ext = new ExtensionPoint(),
            SeqLedger = new Uint32(11),
            SeqTime = new TimePoint(new Uint64(10000)),
        };
        var xdrExtensionV2 = new AccountEntryExtensionV2
        {
            NumSponsored = new Uint32(5),
            NumSponsoring = new Uint32(7),
            SignerSponsoringIDs =
                [new SponsorshipDescriptor { InnerValue = new AccountID(_signerSponsoringId.XdrPublicKey) }],
            Ext = new AccountEntryExtensionV2.AccountEntryExtensionV2Ext
            {
                Discriminant = 3,
                V3 = xdrExtensionV3,
            },
        };
        xdrLedgerEntry.Account.Ext = new AccountEntry.AccountEntryExt
        {
            Discriminant = 1,
            V1 = new AccountEntryExtensionV1
            {
                Ext = new AccountEntryExtensionV1.AccountEntryExtensionV1Ext
                {
                    Discriminant = 2,
                    V2 = xdrExtensionV2,
                },
                Liabilities = xdrLiabilities,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntry);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());

        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        // Account extensions
        // V1
        var decodedExtensionV1 = decodedLedgerEntry.AccountExtensionV1;
        Assert.IsNotNull(decodedExtensionV1);
        var decodedLiabilities = decodedExtensionV1.Liabilities;
        Assert.AreEqual(xdrLiabilities.Buying.InnerValue, decodedLiabilities.Buying);
        Assert.AreEqual(xdrLiabilities.Selling.InnerValue, decodedLiabilities.Selling);

        // V2
        var decodedExtensionV2 = decodedExtensionV1.ExtensionV2;

        Assert.IsNotNull(decodedExtensionV2);
        Assert.AreEqual(xdrExtensionV2.NumSponsored.InnerValue,
            decodedExtensionV2.NumberSponsored);
        Assert.AreEqual(xdrExtensionV2.NumSponsoring.InnerValue,
            decodedExtensionV2.NumberSponsoring);
        Assert.AreEqual(xdrExtensionV2.SignerSponsoringIDs.Length,
            decodedExtensionV2.SignerSponsoringIDs.Length);
        for (var i = 0; i < xdrExtensionV2.SignerSponsoringIDs.Length; i++)
        {
            var decodedId = decodedExtensionV2.SignerSponsoringIDs[i];
            Assert.IsNotNull(decodedId);
            CollectionAssert.AreEqual(
                xdrExtensionV2.SignerSponsoringIDs[i].InnerValue.InnerValue.Ed25519.InnerValue,
                decodedId.XdrPublicKey.Ed25519.InnerValue);
        }

        // V3
        var decodedExtensionV3 = decodedExtensionV2.ExtensionV3;
        Assert.IsNotNull(decodedExtensionV3);
        Assert.IsInstanceOfType(decodedExtensionV3.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(xdrExtensionV3.SeqLedger.InnerValue,
            decodedExtensionV3.SequenceLedger);
        Assert.AreEqual(xdrExtensionV3.SeqTime.InnerValue.InnerValue,
            decodedExtensionV3.SequenceTime);
    }

    [TestMethod]
    public void TestLedgerEntryAccountWithAllPropertiesPopulated()
    {
        var xdrLedgerEntry = InitBasicAccountEntry();
        var xdrAccountEntry = xdrLedgerEntry.Account;
        xdrAccountEntry.InflationDest = new AccountID(KeyPair.Random().XdrPublicKey);
        xdrAccountEntry.HomeDomain = new String32("xdr.stellar.org");
        xdrAccountEntry.Signers = new[]
        {
            new StellarDotnetSdk.Xdr.Signer
            {
                Key = _keyPair.XdrSignerKey,
                Weight = new Uint32(2),
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntry);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());

        var decodedLedgerEntry = (LedgerEntryAccount)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrAccountEntry.Balance.InnerValue,
            decodedLedgerEntry.Balance);
        Assert.AreEqual(xdrAccountEntry.NumSubEntries.InnerValue,
            decodedLedgerEntry.NumberSubEntries);
        Assert.AreEqual(xdrAccountEntry.SeqNum.InnerValue.InnerValue,
            decodedLedgerEntry.SequenceNumber);
        Assert.AreEqual(xdrAccountEntry.Flags.InnerValue,
            decodedLedgerEntry.Flags);
        CollectionAssert.AreEqual(xdrAccountEntry.Thresholds.InnerValue,
            decodedLedgerEntry.Thresholds);
        Assert.AreEqual(xdrAccountEntry.HomeDomain.InnerValue,
            decodedLedgerEntry.HomeDomain);
        CollectionAssert.AreEqual(xdrAccountEntry.AccountID.InnerValue.Ed25519.InnerValue,
            decodedLedgerEntry.Account.XdrPublicKey.Ed25519.InnerValue);
        Assert.AreEqual(xdrAccountEntry.Signers.Length,
            decodedLedgerEntry.Signers.Length);
        for (var i = 0; i < xdrAccountEntry.Signers.Length; i++)
        {
            var signer = xdrAccountEntry.Signers[i];
            var decodedSigner = decodedLedgerEntry.Signers[i];
            CollectionAssert.AreEqual(signer.Key.Ed25519.InnerValue,
                decodedSigner.Key.Ed25519.InnerValue);
            Assert.AreEqual(signer.Weight.InnerValue,
                decodedSigner.Weight);
        }

        var decodedInflationDest = decodedLedgerEntry.InflationDest;
        Assert.IsNotNull(decodedInflationDest);
        CollectionAssert.AreEqual(xdrAccountEntry.InflationDest.InnerValue.Ed25519.InnerValue,
            decodedInflationDest.XdrPublicKey.Ed25519.InnerValue);
    }

    [TestMethod]
    public void TestLedgerEntryOfferWithAllPropertiesPopulated()
    {
        var xdrOfferEntry = new OfferEntry
        {
            SellerID = new AccountID(_keyPair.XdrPublicKey),
            OfferID = new Int64(1000),
            Selling = _alphaNum12Asset.ToXdr(),
            Buying = _alphaNum12Asset.ToXdr(),
            Amount = new Int64(100),
            Price = new StellarDotnetSdk.Xdr.Price
            {
                N = new Int32(1),
                D = new Int32(10),
            },
            Flags = new Uint32(10),
            Ext = new OfferEntry.OfferEntryExt
            {
                Discriminant = 0,
            },
        };
        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.OFFER),
            Offer = xdrOfferEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        // Act
        var decodedLedgerEntry = (LedgerEntryOffer)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrOfferEntry.OfferID.InnerValue, decodedLedgerEntry.OfferId);
        var sellingAsset = xdrOfferEntry.Selling.AlphaNum12;
        var buyingAsset = xdrOfferEntry.Buying.AlphaNum12;
        var decodedSellingAsset = (AssetTypeCreditAlphaNum12)decodedLedgerEntry.Selling;
        var decodedBuyingAsset = (AssetTypeCreditAlphaNum12)decodedLedgerEntry.Buying;
        CollectionAssert.AreEqual(sellingAsset.AssetCode.InnerValue,
            Util.PaddedByteArray(decodedSellingAsset.Code, 12));
        CollectionAssert.AreEqual(sellingAsset.Issuer.InnerValue.Ed25519.InnerValue,
            KeyPair.FromAccountId(decodedSellingAsset.Issuer).XdrPublicKey.Ed25519.InnerValue);
        CollectionAssert.AreEqual(buyingAsset.AssetCode.InnerValue, Util.PaddedByteArray(decodedBuyingAsset.Code, 12));
        CollectionAssert.AreEqual(buyingAsset.Issuer.InnerValue.Ed25519.InnerValue,
            KeyPair.FromAccountId(decodedBuyingAsset.Issuer).XdrPublicKey.Ed25519.InnerValue);
        Assert.AreEqual(xdrOfferEntry.Price.N.InnerValue,
            decodedLedgerEntry.Price.Numerator);
        Assert.AreEqual(xdrOfferEntry.Price.D.InnerValue,
            decodedLedgerEntry.Price.Denominator);
        Assert.AreEqual(xdrOfferEntry.Amount.InnerValue,
            decodedLedgerEntry.Amount);
        CollectionAssert.AreEqual(xdrOfferEntry.SellerID.InnerValue.Ed25519.InnerValue,
            decodedLedgerEntry.SellerId.XdrPublicKey.Ed25519.InnerValue);
        Assert.AreEqual(xdrOfferEntry.Flags.InnerValue,
            decodedLedgerEntry.Flags);
        Assert.IsNull(decodedLedgerEntry.OfferExtension); // Currently, no offer entry extension is available
    }

    [TestMethod]
    public void TestLedgerEntryTrustlineWithTrustlineExtensionV1Only()
    {
        var xdrTrustlineEntry = new TrustLineEntry
        {
            AccountID = new AccountID(_keyPair.XdrPublicKey),
            Asset = _trustlineAlphaNum4Asset.ToXdr(),
            Balance = new Int64(1000),
            Limit = new Int64(10000),
            Flags = new Uint32(1),
            Ext = new TrustLineEntry.TrustLineEntryExt
            {
                Discriminant = 1,
                V1 = new TrustLineEntry.TrustLineEntryExt.TrustLineEntryV1
                {
                    Liabilities = new Liabilities
                    {
                        Buying = new Int64(100),
                        Selling = new Int64(100),
                    },
                    Ext = new TrustLineEntry.TrustLineEntryExt.TrustLineEntryV1.TrustLineEntryV1Ext
                    {
                        Discriminant = 0,
                    },
                },
            },
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE),
            TrustLine = xdrTrustlineEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        // Act
        var decodedLedgerEntry = (LedgerEntryTrustline)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        CollectionAssert.AreEqual(xdrTrustlineEntry.AccountID.InnerValue.Ed25519.InnerValue,
            decodedLedgerEntry.Account.XdrPublicKey.Ed25519.InnerValue);
        var asset = xdrTrustlineEntry.Asset.AlphaNum4;
        var decodedAsset = (AssetTypeCreditAlphaNum4)((TrustlineAsset.Wrapper)decodedLedgerEntry.Asset).Asset;
        CollectionAssert.AreEqual(asset.AssetCode.InnerValue,
            Util.PaddedByteArray(decodedAsset.Code, 4));
        CollectionAssert.AreEqual(asset.Issuer.InnerValue.Ed25519.InnerValue,
            KeyPair.FromAccountId(decodedAsset.Issuer).XdrPublicKey.Ed25519.InnerValue);
        Assert.AreEqual(xdrTrustlineEntry.Flags.InnerValue,
            decodedLedgerEntry.Flags);
        Assert.AreEqual(xdrTrustlineEntry.Balance.InnerValue,
            decodedLedgerEntry.Balance);
        Assert.AreEqual(xdrTrustlineEntry.Limit.InnerValue,
            decodedLedgerEntry.Limit);

        // Trustline extensions
        // V1
        var liabilities = xdrTrustlineEntry.Ext.V1.Liabilities;
        var decodedTrustlineExtensionV1 = decodedLedgerEntry.TrustlineExtensionV1;
        Assert.IsNotNull(decodedTrustlineExtensionV1);
        var decodedLiabilities = decodedTrustlineExtensionV1.Liabilities;
        Assert.AreEqual(liabilities.Buying.InnerValue,
            decodedLiabilities.Buying);
        Assert.AreEqual(liabilities.Selling.InnerValue,
            decodedLiabilities.Selling);

        Assert.IsNull(decodedTrustlineExtensionV1.TrustlineExtensionV2);
    }

    [TestMethod]
    public void TestLedgerEntryTrustlineWithAllPropertiesPopulated()
    {
        var xdrExtensionV2 = new TrustLineEntryExtensionV2
        {
            Ext = new TrustLineEntryExtensionV2.TrustLineEntryExtensionV2Ext
            {
                Discriminant = 0,
            },
            LiquidityPoolUseCount = new Int32(20),
        };
        var xdrTrustlineEntry = new TrustLineEntry
        {
            AccountID = new AccountID(_keyPair.XdrPublicKey),
            Asset = _trustlineAlphaNum4Asset.ToXdr(),
            Balance = new Int64(1000),
            Limit = new Int64(10000),
            Flags = new Uint32(1),
            Ext = new TrustLineEntry.TrustLineEntryExt
            {
                Discriminant = 1,
                V1 = new TrustLineEntry.TrustLineEntryExt.TrustLineEntryV1
                {
                    Liabilities = new Liabilities
                    {
                        Buying = new Int64(100),
                        Selling = new Int64(100),
                    },
                    Ext = new TrustLineEntry.TrustLineEntryExt.TrustLineEntryV1.TrustLineEntryV1Ext
                    {
                        Discriminant = 2,
                        V2 = xdrExtensionV2,
                    },
                },
            },
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE),
            TrustLine = xdrTrustlineEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        // Act
        var decodedLedgerEntry = (LedgerEntryTrustline)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        CollectionAssert.AreEqual(xdrTrustlineEntry.AccountID.InnerValue.Ed25519.InnerValue,
            decodedLedgerEntry.Account.XdrPublicKey.Ed25519.InnerValue);
        var asset = xdrTrustlineEntry.Asset.AlphaNum4;
        var decodedAsset = (AssetTypeCreditAlphaNum4)((TrustlineAsset.Wrapper)decodedLedgerEntry.Asset).Asset;
        CollectionAssert.AreEqual(asset.AssetCode.InnerValue,
            Util.PaddedByteArray(decodedAsset.Code, 4));
        CollectionAssert.AreEqual(asset.Issuer.InnerValue.Ed25519.InnerValue,
            KeyPair.FromAccountId(decodedAsset.Issuer).XdrPublicKey.Ed25519.InnerValue);
        Assert.AreEqual(xdrTrustlineEntry.Flags.InnerValue,
            decodedLedgerEntry.Flags);
        Assert.AreEqual(xdrTrustlineEntry.Balance.InnerValue,
            decodedLedgerEntry.Balance);
        Assert.AreEqual(xdrTrustlineEntry.Limit.InnerValue,
            decodedLedgerEntry.Limit);

        // Trustline extensions
        // V1
        var liabilities = xdrTrustlineEntry.Ext.V1.Liabilities;
        var decodedTrustlineExtensionV1 = decodedLedgerEntry.TrustlineExtensionV1;
        Assert.IsNotNull(decodedTrustlineExtensionV1);
        var decodedLiabilities = decodedTrustlineExtensionV1.Liabilities;
        Assert.AreEqual(liabilities.Buying.InnerValue,
            decodedLiabilities.Buying);
        Assert.AreEqual(liabilities.Selling.InnerValue,
            decodedLiabilities.Selling);

        var decodedExtensionV2 = decodedTrustlineExtensionV1.TrustlineExtensionV2;
        Assert.IsNotNull(decodedExtensionV2);
        Assert.AreEqual(xdrExtensionV2.LiquidityPoolUseCount.InnerValue,
            decodedExtensionV2.LiquidityPoolUseCount);
    }

    [TestMethod]
    public void TestLedgerEntryDataWithAllPropertiesPopulated()
    {
        var xdrDataEntry = new DataEntry
        {
            AccountID = new AccountID(_keyPair.XdrPublicKey),
            DataName = new String64("DataName"),
            DataValue = new DataValue(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }),

            Ext = new DataEntry.DataEntryExt
            {
                Discriminant = 0,
            },
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.DATA),
            Data = xdrDataEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        // Act
        var decodedLedgerEntry = (LedgerEntryData)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        CollectionAssert.AreEqual(xdrDataEntry.AccountID.InnerValue.Ed25519.InnerValue,
            decodedLedgerEntry.Account.XdrPublicKey.Ed25519.InnerValue);
        CollectionAssert.AreEqual(xdrDataEntry.DataValue.InnerValue,
            decodedLedgerEntry.DataValue);
        Assert.AreEqual(xdrDataEntry.DataName.InnerValue,
            decodedLedgerEntry.DataName);
        Assert.IsNull(decodedLedgerEntry.DataExtension); // Currently, no data entry extension is available
    }

    [TestMethod]
    public void TestLedgerEntryClaimableBalanceWithMissingClaimableBalanceExtension()
    {
        var xdrClaimableBalanceEntry = InitBasicXdrClaimableBalanceEntry();

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE),
            ClaimableBalance = xdrClaimableBalanceEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntry = (LedgerEntryClaimableBalance)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrClaimableBalanceEntry.Claimants.Length,
            decodedLedgerEntry.Claimants.Length);
        for (var i = 0; i < xdrClaimableBalanceEntry.Claimants.Length; i++)
        {
            var xdrClaimant = xdrClaimableBalanceEntry.Claimants[i].V0;
            var decodedClaimant = decodedLedgerEntry.Claimants[i];
            CollectionAssert.AreEqual(xdrClaimant.Destination.InnerValue.Ed25519.InnerValue,
                decodedClaimant.Destination.XdrPublicKey.Ed25519.InnerValue);
            var predicate = xdrClaimant.Predicate;
            var decodedPredicate = (ClaimPredicateBeforeRelativeTime)decodedClaimant.Predicate;
            Assert.AreEqual(predicate.RelBefore.InnerValue,
                decodedPredicate.Duration);
        }

        Assert.AreEqual(ClaimableBalanceUtils.FromXdr(xdrClaimableBalanceEntry.BalanceID),
            decodedLedgerEntry.BalanceId);

        Assert.AreEqual(xdrClaimableBalanceEntry.Amount.InnerValue,
            decodedLedgerEntry.Amount);

        var decodedAsset = (AssetTypeCreditAlphaNum12)decodedLedgerEntry.Asset;
        CollectionAssert.AreEqual(xdrClaimableBalanceEntry.Asset.AlphaNum12.AssetCode.InnerValue,
            Util.PaddedByteArray(decodedAsset.Code, 12));

        Assert.IsNull(decodedLedgerEntry.ClaimableBalanceEntryExtensionV1);
    }

    [TestMethod]
    public void TestLedgerEntryClaimableBalanceWithAllPropertiesPopulated()
    {
        var xdrClaimableBalanceEntry = InitBasicXdrClaimableBalanceEntry();

        var xdrExtension = new ClaimableBalanceEntryExtensionV1
        {
            Ext = new ClaimableBalanceEntryExtensionV1.ClaimableBalanceEntryExtensionV1Ext
            {
                Discriminant = 0,
            },
            Flags = new Uint32(4),
        };
        xdrClaimableBalanceEntry.Ext = new ClaimableBalanceEntry.ClaimableBalanceEntryExt
        {
            Discriminant = 1,
            V1 = xdrExtension,
        };
        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE),
            ClaimableBalance = xdrClaimableBalanceEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntry = (LedgerEntryClaimableBalance)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrClaimableBalanceEntry.Claimants.Length,
            decodedLedgerEntry.Claimants.Length);
        for (var i = 0; i < xdrClaimableBalanceEntry.Claimants.Length; i++)
        {
            var xdrClaimant = xdrClaimableBalanceEntry.Claimants[i].V0;
            var decodedClaimant = decodedLedgerEntry.Claimants[i];
            CollectionAssert.AreEqual(xdrClaimant.Destination.InnerValue.Ed25519.InnerValue,
                decodedClaimant.Destination.XdrPublicKey.Ed25519.InnerValue);
            var decodedPredicate = (ClaimPredicateBeforeRelativeTime)decodedClaimant.Predicate;
            Assert.AreEqual(xdrClaimant.Predicate.RelBefore.InnerValue,
                decodedPredicate.Duration);
        }
        Assert.AreEqual(ClaimableBalanceUtils.FromXdr(xdrClaimableBalanceEntry.BalanceID),
            decodedLedgerEntry.BalanceId);

        Assert.AreEqual(xdrClaimableBalanceEntry.Amount.InnerValue,
            decodedLedgerEntry.Amount);

        var decodedAsset = (AssetTypeCreditAlphaNum12)decodedLedgerEntry.Asset;
        CollectionAssert.AreEqual(xdrClaimableBalanceEntry.Asset.AlphaNum12.AssetCode.InnerValue,
            Util.PaddedByteArray(decodedAsset.Code, 12));

        var decodedExtension = decodedLedgerEntry.ClaimableBalanceEntryExtensionV1;
        Assert.IsNotNull(decodedExtension);
        Assert.AreEqual(xdrExtension.Flags.InnerValue,
            decodedExtension.Flags);
    }

    [TestMethod]
    public void TestLedgerEntryLiquidityPoolWithAllPropertiesPopulated()
    {
        var xdrLiquidityPoolEntry = new LiquidityPoolEntry
        {
            LiquidityPoolID = new PoolID
            {
                InnerValue = new Hash([
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2,
                ]),
            },
            Body = new LiquidityPoolEntry.LiquidityPoolEntryBody
            {
                Discriminant =
                    LiquidityPoolType.Create(LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT),
                ConstantProduct = new LiquidityPoolEntry.LiquidityPoolEntryBody.LiquidityPoolEntryConstantProduct
                {
                    Params = new LiquidityPoolConstantProductParameters
                    {
                        AssetA = _nativeAsset.ToXdr(),
                        AssetB = _alphaNum4Asset.ToXdr(),
                        Fee = new Int32(100),
                    },
                    ReserveA = new Int64(1000),
                    ReserveB = new Int64(2000),
                    TotalPoolShares = new Int64(1000000),
                    PoolSharesTrustLineCount = new Int64(10),
                },
            },
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL),
            LiquidityPool = xdrLiquidityPoolEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntry = (LedgerEntryLiquidityPool)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        CollectionAssert.AreEqual(xdrLiquidityPoolEntry.LiquidityPoolID.InnerValue.InnerValue,
            decodedLedgerEntry.LiquidityPoolId.Hash);

        var product = xdrLiquidityPoolEntry.Body.ConstantProduct;
        var decodedProduct = (LiquidityPoolConstantProduct)decodedLedgerEntry.LiquidityPoolBody;
        Assert.AreEqual(product.ReserveA.InnerValue,
            decodedProduct.ReserveA);
        Assert.AreEqual(product.ReserveB.InnerValue,
            decodedProduct.ReserveB);
        Assert.AreEqual(product.TotalPoolShares.InnerValue,
            decodedProduct.TotalPoolShares);
        Assert.AreEqual(product.PoolSharesTrustLineCount.InnerValue,
            decodedProduct.PoolSharesTrustLineCount);
        var assetB = product.Params.AssetB.AlphaNum4;
        var decodedAssetB = (AssetTypeCreditAlphaNum4)decodedProduct.Parameters.AssetB;

        Assert.AreEqual(product.Params.Fee.InnerValue,
            decodedProduct.Parameters.Fee);
        Assert.AreEqual("native",
            decodedProduct.Parameters.AssetA.Type);
        CollectionAssert.AreEqual(assetB.AssetCode.InnerValue,
            Util.PaddedByteArray(decodedAssetB.Code, 4));
        CollectionAssert.AreEqual(assetB.Issuer.InnerValue.Ed25519.InnerValue,
            KeyPair.FromAccountId(decodedAssetB.Issuer).XdrPublicKey.Ed25519.InnerValue);
    }

    [TestMethod]
    public void TestLedgerEntryContractDataWithWithContractBeingAContractAddress()
    {
        var xdrContractDataEntry = InitBasicContractDataEntry();

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA),
            ContractData = xdrContractDataEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntry = (LedgerEntryContractData)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.IsInstanceOfType(decodedLedgerEntry.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(StrKey.EncodeContractId(xdrContractDataEntry.Contract.ContractId.InnerValue.InnerValue),
            ((ScContractId)decodedLedgerEntry.Contract).InnerValue);
        Assert.AreEqual(xdrContractDataEntry.Durability.InnerValue,
            decodedLedgerEntry.Durability.InnerValue);
        Assert.AreEqual(xdrContractDataEntry.Key.Str.InnerValue,
            ((SCString)decodedLedgerEntry.Key).InnerValue);
        Assert.AreEqual(xdrContractDataEntry.Val.U64.InnerValue,
            ((SCUint64)decodedLedgerEntry.Value).InnerValue);
    }

    [TestMethod]
    public void TestLedgerEntryContractDataWithWithContractBeingAnAccountAddress()
    {
        var xdrContractDataEntry = InitBasicContractDataEntry();
        xdrContractDataEntry.Contract = new SCAddress
        {
            Discriminant = SCAddressType.Create(SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_ACCOUNT),
            AccountId = new AccountID(KeyPair.FromAccountId("GCZFMH32MF5EAWETZTKF3ZV5SEVJPI53UEMDNSW55WBR75GMZJU4U573")
                .XdrPublicKey),
        };
        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA),
            ContractData = xdrContractDataEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntry = (LedgerEntryContractData)LedgerEntry.FromXdrBase64(entryXdrBase64);

        var decodedAccountId = (ScAccountId)decodedLedgerEntry.Contract;
        // Assert
        Assert.IsInstanceOfType(decodedLedgerEntry.ExtensionPoint, typeof(ExtensionPointZero));
        CollectionAssert.AreEqual(xdrContractDataEntry.Contract.AccountId.InnerValue.Ed25519.InnerValue,
            KeyPair.FromAccountId(decodedAccountId.InnerValue).XdrPublicKey.Ed25519.InnerValue);
        Assert.AreEqual(xdrContractDataEntry.Durability.InnerValue,
            decodedLedgerEntry.Durability.InnerValue);
        Assert.AreEqual(xdrContractDataEntry.Key.Str.InnerValue,
            ((SCString)decodedLedgerEntry.Key).InnerValue);
        Assert.AreEqual(xdrContractDataEntry.Val.U64.InnerValue,
            ((SCUint64)decodedLedgerEntry.Value).InnerValue);
    }

    [TestMethod]
    public void TestLedgerEntryContractCodeV0WithAllPropertiesPopulated()
    {
        var xdrContractCodeEntry = new ContractCodeEntry
        {
            Ext = new ContractCodeEntry.ContractCodeEntryExt
            {
                Discriminant = 0,
            },
            Hash = new Hash([
                1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 3,
            ]),
            Code =
            [
                1, 2, 3, 4,
            ],
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE),
            ContractCode = xdrContractCodeEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntry = (LedgerEntryContractCode)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        CollectionAssert.AreEqual(xdrContractCodeEntry.Code,
            decodedLedgerEntry.Code);
        CollectionAssert.AreEqual(xdrContractCodeEntry.Hash.InnerValue,
            decodedLedgerEntry.Hash);
        Assert.IsNull(decodedLedgerEntry.ContractCodeExtensionV1);
    }

    [TestMethod]
    public void TestLedgerEntryContractCodeV1WithAllPropertiesPopulated()
    {
        var xdrContractCodeEntry = new ContractCodeEntry
        {
            Ext = new ContractCodeEntry.ContractCodeEntryExt
            {
                Discriminant = 1,
                V1 = new ContractCodeEntry.ContractCodeEntryExt.ContractCodeEntryV1
                {
                    Ext = new ExtensionPoint
                    {
                        Discriminant = 0,
                    },
                    CostInputs = new ContractCodeCostInputs
                    {
                        Ext = new ExtensionPoint
                        {
                            Discriminant = 0,
                        },
                        NInstructions = new Uint32(1),
                        NFunctions = new Uint32(2),
                        NGlobals = new Uint32(3),
                        NTableEntries = new Uint32(4),
                        NTypes = new Uint32(5),
                        NDataSegments = new Uint32(6),
                        NElemSegments = new Uint32(7),
                        NImports = new Uint32(8),
                        NExports = new Uint32(9),
                        NDataSegmentBytes = new Uint32(10),
                    },
                },
            },
            Hash = new Hash([
                1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2,
            ]),
            Code =
            [
                1, 2, 3,
            ],
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE),
            ContractCode = xdrContractCodeEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntry = (LedgerEntryContractCode)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        CollectionAssert.AreEqual(xdrContractCodeEntry.Code,
            decodedLedgerEntry.Code);
        CollectionAssert.AreEqual(xdrContractCodeEntry.Hash.InnerValue,
            decodedLedgerEntry.Hash);
        Assert.IsNotNull(decodedLedgerEntry.ContractCodeExtensionV1);
        var xdrCostInputs = xdrContractCodeEntry.Ext.V1.CostInputs;
        var decodedCostInputs = decodedLedgerEntry.ContractCodeExtensionV1.CostInputs;
        Assert.IsNotNull(decodedCostInputs);

        Assert.AreEqual(xdrCostInputs.NInstructions.InnerValue,
            decodedCostInputs.NInstructions);
        Assert.AreEqual(xdrCostInputs.NFunctions.InnerValue,
            decodedCostInputs.NFunctions);
        Assert.AreEqual(xdrCostInputs.NGlobals.InnerValue,
            decodedCostInputs.NGlobals);
        Assert.AreEqual(xdrCostInputs.NTableEntries.InnerValue,
            decodedCostInputs.NTableEntries);
        Assert.AreEqual(xdrCostInputs.NTypes.InnerValue,
            decodedCostInputs.NTypes);
        Assert.AreEqual(xdrCostInputs.NDataSegments.InnerValue,
            decodedCostInputs.NDataSegments);
        Assert.AreEqual(xdrCostInputs.NElemSegments.InnerValue,
            decodedCostInputs.NElemSegments);
        Assert.AreEqual(xdrCostInputs.NImports.InnerValue,
            decodedCostInputs.NImports);
        Assert.AreEqual(xdrCostInputs.NExports.InnerValue,
            decodedCostInputs.NExports);
        Assert.AreEqual(xdrCostInputs.NDataSegmentBytes.InnerValue,
            decodedCostInputs.NDataSegmentBytes);

        Assert.IsInstanceOfType(decodedLedgerEntry.ContractCodeExtensionV1.ExtensionPoint,
            typeof(ExtensionPointZero));
        Assert.IsInstanceOfType(decodedCostInputs.ExtensionPoint,
            typeof(ExtensionPointZero));
    }

    [TestMethod]
    public void TestLedgerEntryConfigSettingContractBandwidthV0()
    {
        var xdrConfigSetting = new ConfigSettingContractBandwidthV0
        {
            LedgerMaxTxsSizeBytes = new Uint32(1024),
            TxMaxSizeBytes = new Uint32(2048),
            FeeTxSize1KB = new Int64(3072),
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = new ConfigSettingEntry
            {
                Discriminant =
                    ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_BANDWIDTH_V0),
                ContractBandwidth = xdrConfigSetting,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingContractBandwidth)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.TxMaxSizeBytes.InnerValue,
            decodedConfigSetting.TxMaxSizeBytes);
        Assert.AreEqual(xdrConfigSetting.FeeTxSize1KB.InnerValue,
            decodedConfigSetting.FeeTxSize1Kb);
        Assert.AreEqual(xdrConfigSetting.LedgerMaxTxsSizeBytes.InnerValue,
            decodedConfigSetting.LedgerMaxTxsSizeBytes);
    }

    [TestMethod]
    public void TestConfigSettingContractCostParamsMemoryBytes()
    {
        var xdrConfigSetting = new ContractCostParams
        {
            InnerValue = new ContractCostParamEntry[]
            {
                new()
                {
                    Ext = new ExtensionPoint(),
                    ConstTerm = new Int64(10),
                    LinearTerm = new Int64(20),
                },
                new()
                {
                    Ext = new ExtensionPoint(),
                    ConstTerm = new Int64(30),
                    LinearTerm = new Int64(40),
                },
            },
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = new ConfigSettingEntry
            {
                Discriminant = ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum
                    .CONFIG_SETTING_CONTRACT_COST_PARAMS_MEMORY_BYTES),
                ContractCostParamsMemBytes = xdrConfigSetting,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting =
            (ConfigSettingContractCostParamsMemoryBytes)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(2, xdrConfigSetting.InnerValue.Length);
        Assert.AreEqual(2, decodedConfigSetting.ParamEntries.Length);
        for (var i = 0; i < xdrConfigSetting.InnerValue.Length; i++)
        {
            var xdrParamEntry = xdrConfigSetting.InnerValue[i];
            var decodedParamEntry = decodedConfigSetting.ParamEntries[i];
            Assert.AreEqual(xdrParamEntry.LinearTerm.InnerValue,
                decodedParamEntry.LinearTerm);
            Assert.AreEqual(xdrParamEntry.ConstTerm.InnerValue,
                decodedParamEntry.ConstTerm);
            Assert.IsInstanceOfType(decodedParamEntry.ExtensionPoint,
                typeof(ExtensionPointZero));
        }
    }

    [TestMethod]
    public void TestConfigSettingContractCostParamsCpuInstructions()
    {
        var xdrConfigSetting = new ContractCostParams
        {
            InnerValue = new ContractCostParamEntry[]
            {
                new()
                {
                    Ext = new ExtensionPoint(),
                    ConstTerm = new Int64(100),
                    LinearTerm = new Int64(200),
                },
                new()
                {
                    Ext = new ExtensionPoint(),
                    ConstTerm = new Int64(300),
                    LinearTerm = new Int64(400),
                },
            },
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = new ConfigSettingEntry
            {
                Discriminant = ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum
                    .CONFIG_SETTING_CONTRACT_COST_PARAMS_CPU_INSTRUCTIONS),
                ContractCostParamsCpuInsns = xdrConfigSetting,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting =
            (ConfigSettingContractCostParamsCpuInstructions)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(2, xdrConfigSetting.InnerValue.Length);
        Assert.AreEqual(2, decodedConfigSetting.ParamEntries.Length);
        for (var i = 0; i < xdrConfigSetting.InnerValue.Length; i++)
        {
            var xdrParamEntry = xdrConfigSetting.InnerValue[i];
            var decodedParamEntry = decodedConfigSetting.ParamEntries[i];
            Assert.AreEqual(xdrParamEntry.LinearTerm.InnerValue,
                decodedParamEntry.LinearTerm);
            Assert.AreEqual(xdrParamEntry.ConstTerm.InnerValue,
                decodedParamEntry.ConstTerm);
            Assert.IsInstanceOfType(decodedParamEntry.ExtensionPoint,
                typeof(ExtensionPointZero));
        }
    }

    [TestMethod]
    public void TestConfigSettingContractComputeV0()
    {
        var xdrConfigSetting = new ConfigSettingContractComputeV0
        {
            LedgerMaxInstructions = new Int64(10),
            TxMaxInstructions = new Int64(20),
            FeeRatePerInstructionsIncrement = new Int64(30),
            TxMemoryLimit = new Uint32(40),
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = new ConfigSettingEntry
            {
                Discriminant =
                    ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_COMPUTE_V0),
                ContractCompute = xdrConfigSetting,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingContractCompute)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.LedgerMaxInstructions.InnerValue,
            decodedConfigSetting.LedgerMaxInstructions);
        Assert.AreEqual(xdrConfigSetting.FeeRatePerInstructionsIncrement.InnerValue,
            decodedConfigSetting.FeeRatePerInstructionsIncrement);
        Assert.AreEqual(xdrConfigSetting.TxMaxInstructions.InnerValue,
            decodedConfigSetting.TxMaxInstructions);
        Assert.AreEqual(xdrConfigSetting.TxMemoryLimit.InnerValue,
            decodedConfigSetting.TxMemoryLimit);
    }

    [TestMethod]
    public void TestConfigSettingContractEventsV0()
    {
        var xdrConfigSetting = new ConfigSettingContractEventsV0
        {
            TxMaxContractEventsSizeBytes = new Uint32(100),
            FeeContractEvents1KB = new Int64(1100),
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = new ConfigSettingEntry
            {
                Discriminant =
                    ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_EVENTS_V0),
                ContractEvents = xdrConfigSetting,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingContractEvents)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.FeeContractEvents1KB.InnerValue,
            decodedConfigSetting.FeeContractEvents1Kb);
        Assert.AreEqual(xdrConfigSetting.TxMaxContractEventsSizeBytes.InnerValue,
            decodedConfigSetting.TxMaxContractEventsSizeBytes);
    }

    [TestMethod]
    public void TestConfigSettingContractExecutionLanesV0()
    {
        var xdrConfigSetting = new ConfigSettingContractExecutionLanesV0
        {
            LedgerMaxTxCount = new Uint32(1000),
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = new ConfigSettingEntry
            {
                Discriminant =
                    ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_EXECUTION_LANES),
                ContractExecutionLanes = xdrConfigSetting,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingContractExecutionLanes)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.LedgerMaxTxCount.InnerValue,
            decodedConfigSetting.LedgerMaxTxCount);
    }

    [TestMethod]
    public void TestConfigSettingContractHistoricalDataV0()
    {
        var xdrConfigSetting = new ConfigSettingContractHistoricalDataV0
        {
            FeeHistorical1KB = new Int64(1000),
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = new ConfigSettingEntry
            {
                Discriminant =
                    ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum
                        .CONFIG_SETTING_CONTRACT_HISTORICAL_DATA_V0),
                ContractHistoricalData = xdrConfigSetting,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingContractHistoricalData)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.FeeHistorical1KB.InnerValue,
            decodedConfigSetting.FeeHistorical1Kb);
    }

    [TestMethod]
    public void TestConfigSettingContractLedgerCostV0()
    {
        var xdrConfigSetting = new ConfigSettingContractLedgerCostV0
        {
            LedgerMaxDiskReadEntries = new Uint32(10),
            LedgerMaxDiskReadBytes = new Uint32(20),
            LedgerMaxWriteLedgerEntries = new Uint32(30),
            LedgerMaxWriteBytes = new Uint32(40),
            TxMaxDiskReadEntries = new Uint32(50),
            TxMaxDiskReadBytes = new Uint32(60),
            TxMaxWriteLedgerEntries = new Uint32(70),
            TxMaxWriteBytes = new Uint32(80),
            FeeDiskReadLedgerEntry = new Int64(90),
            FeeWriteLedgerEntry = new Int64(100),
            FeeDiskRead1KB = new Int64(110),
            SorobanStateTargetSizeBytes = new Int64(120),
            RentFee1KBSorobanStateSizeLow = new Int64(130),
            RentFee1KBSorobanStateSizeHigh = new Int64(140),
            SorobanStateRentFeeGrowthFactor = new Uint32(150),
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = new ConfigSettingEntry
            {
                Discriminant =
                    ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_LEDGER_COST_V0),
                ContractLedgerCost = xdrConfigSetting,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingContractLedgerCost)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.LedgerMaxDiskReadEntries.InnerValue,
            decodedConfigSetting.LedgerMaxDiskReadEntries);
        Assert.AreEqual(xdrConfigSetting.LedgerMaxDiskReadBytes.InnerValue,
            decodedConfigSetting.LedgerMaxDiskReadBytes);
        Assert.AreEqual(xdrConfigSetting.LedgerMaxWriteLedgerEntries.InnerValue,
            decodedConfigSetting.LedgerMaxWriteLedgerEntries);
        Assert.AreEqual(xdrConfigSetting.LedgerMaxWriteBytes.InnerValue,
            decodedConfigSetting.LedgerMaxWriteBytes);
        Assert.AreEqual(xdrConfigSetting.TxMaxDiskReadEntries.InnerValue,
            decodedConfigSetting.TxMaxDiskReadEntries);
        Assert.AreEqual(xdrConfigSetting.TxMaxDiskReadBytes.InnerValue,
            decodedConfigSetting.TxMaxDiskReadBytes);
        Assert.AreEqual(xdrConfigSetting.TxMaxWriteLedgerEntries.InnerValue,
            decodedConfigSetting.TxMaxWriteLedgerEntries);
        Assert.AreEqual(xdrConfigSetting.TxMaxWriteBytes.InnerValue,
            decodedConfigSetting.TxMaxWriteBytes);
        Assert.AreEqual(xdrConfigSetting.FeeDiskReadLedgerEntry.InnerValue,
            decodedConfigSetting.FeeDiskReadLedgerEntry);
        Assert.AreEqual(xdrConfigSetting.FeeWriteLedgerEntry.InnerValue,
            decodedConfigSetting.FeeWriteLedgerEntry);
        Assert.AreEqual(xdrConfigSetting.FeeDiskRead1KB.InnerValue,
            decodedConfigSetting.FeeDiskRead1Kb);
        Assert.AreEqual(xdrConfigSetting.SorobanStateTargetSizeBytes.InnerValue,
            decodedConfigSetting.SorobanStateTargetSizeBytes);
        Assert.AreEqual(xdrConfigSetting.RentFee1KBSorobanStateSizeLow.InnerValue,
            decodedConfigSetting.RentFee1KbSorobanStateSizeLow);
        Assert.AreEqual(xdrConfigSetting.RentFee1KBSorobanStateSizeHigh.InnerValue,
            decodedConfigSetting.RentFee1KbSorobanStateSizeHigh);
        Assert.AreEqual(xdrConfigSetting.SorobanStateRentFeeGrowthFactor.InnerValue,
            decodedConfigSetting.SorobanStateRentFeeGrowthFactor);
    }

    [TestMethod]
    public void TestStateArchivalSettings()
    {
        var xdrConfigSetting = new StellarDotnetSdk.Xdr.StateArchivalSettings
        {
            MaxEntryTTL = new Uint32(10),
            MinTemporaryTTL = new Uint32(20),
            MinPersistentTTL = new Uint32(30),
            PersistentRentRateDenominator = new Int64(40),
            TempRentRateDenominator = new Int64(50),
            MaxEntriesToArchive = new Uint32(60),
            LiveSorobanStateSizeWindowSampleSize = new Uint32(70),
            LiveSorobanStateSizeWindowSamplePeriod = new Uint32(80),
            EvictionScanSize = new Uint32(90),
            StartingEvictionScanLevel = new Uint32(100),
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = new ConfigSettingEntry
            {
                Discriminant =
                    ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_STATE_ARCHIVAL),
                StateArchivalSettings = xdrConfigSetting,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (StateArchivalSettings)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.MaxEntryTTL.InnerValue,
            decodedConfigSetting.MaxEntryTtl);
        Assert.AreEqual(xdrConfigSetting.MinTemporaryTTL.InnerValue,
            decodedConfigSetting.MinTemporaryTtl);
        Assert.AreEqual(xdrConfigSetting.MinPersistentTTL.InnerValue,
            decodedConfigSetting.MinPersistentTtl);
        Assert.AreEqual(xdrConfigSetting.PersistentRentRateDenominator.InnerValue,
            decodedConfigSetting.PersistentRentRateDenominator);
        Assert.AreEqual(xdrConfigSetting.TempRentRateDenominator.InnerValue,
            decodedConfigSetting.TempRentRateDenominator);
        Assert.AreEqual(xdrConfigSetting.MaxEntriesToArchive.InnerValue,
            decodedConfigSetting.MaxEntriesToArchive);
        Assert.AreEqual(xdrConfigSetting.LiveSorobanStateSizeWindowSampleSize.InnerValue,
            decodedConfigSetting.LiveSorobanStateSizeWindowSampleSize);
        Assert.AreEqual(xdrConfigSetting.LiveSorobanStateSizeWindowSamplePeriod.InnerValue,
            decodedConfigSetting.LiveSorobanStateSizeWindowSamplePeriod);
        Assert.AreEqual(xdrConfigSetting.EvictionScanSize.InnerValue,
            decodedConfigSetting.EvictionScanSize);
        Assert.AreEqual(xdrConfigSetting.StartingEvictionScanLevel.InnerValue,
            decodedConfigSetting.StartingEvictionScanLevel);
    }

    [TestMethod]
    public void TestConfigSettingEvictionIterator()
    {
        var xdrConfigSetting = new StellarDotnetSdk.Xdr.EvictionIterator
        {
            BucketListLevel = new Uint32(10),
            IsCurrBucket = true,
            BucketFileOffset = new Uint64(100),
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = new ConfigSettingEntry
            {
                Discriminant =
                    ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_EVICTION_ITERATOR),
                EvictionIterator = xdrConfigSetting,
            },
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (EvictionIterator)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.IsCurrBucket,
            decodedConfigSetting.IsCurrBucket);
        Assert.AreEqual(xdrConfigSetting.BucketListLevel.InnerValue,
            decodedConfigSetting.BucketListLevel);
        Assert.AreEqual(xdrConfigSetting.BucketFileOffset.InnerValue,
            decodedConfigSetting.BucketFileOffset);
    }

    [TestMethod]
    public void TestConfigSettingContractMaxSizeBytes()
    {
        var xdrConfigSetting = new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_MAX_SIZE_BYTES),
            ContractMaxSizeBytes = new Uint32(10),
        };
        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = xdrConfigSetting,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingContractMaxSizeBytes)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.ContractMaxSizeBytes.InnerValue,
            decodedConfigSetting.InnerValue);
    }

    [TestMethod]
    public void TestConfigSettingContractDataKeySizeBytes()
    {
        var xdrConfigSetting = new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum
                    .CONFIG_SETTING_CONTRACT_DATA_KEY_SIZE_BYTES),
            ContractDataKeySizeBytes = new Uint32(10),
        };
        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = xdrConfigSetting,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingContractDataKeySizeBytes)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.ContractDataKeySizeBytes.InnerValue,
            decodedConfigSetting.InnerValue);
    }

    [TestMethod]
    public void TestConfigSettingContractDataEntrySizeBytes()
    {
        var xdrConfigSetting = new ConfigSettingEntry
        {
            Discriminant = ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum
                .CONFIG_SETTING_CONTRACT_DATA_ENTRY_SIZE_BYTES),
            ContractDataEntrySizeBytes = new Uint32(10),
        };
        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = xdrConfigSetting,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingContractDataEntrySizeBytes)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.ContractDataEntrySizeBytes.InnerValue,
            decodedConfigSetting.InnerValue);
    }

    [TestMethod]
    public void TestConfigSettingLiveSorobanStateSizeWindow()
    {
        var xdrConfigSetting = new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum
                    .CONFIG_SETTING_LIVE_SOROBAN_STATE_SIZE_WINDOW),
            LiveSorobanStateSizeWindow = new Uint64[] { new(100), new(200) },
        };
        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = xdrConfigSetting,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingLiveSorobanStateSizeWindow)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.LiveSorobanStateSizeWindow.Length,
            decodedConfigSetting.InnerValue.Length);
        for (var i = 0; i < decodedConfigSetting.InnerValue.Length; i++)
        {
            Assert.AreEqual(xdrConfigSetting.LiveSorobanStateSizeWindow[i].InnerValue,
                decodedConfigSetting.InnerValue[i]);
        }
    }

    [TestMethod]
    public void TestConfigSettingContractParallelComputeV0()
    {
        var xdrConfigSetting = new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum
                    .CONFIG_SETTING_CONTRACT_PARALLEL_COMPUTE_V0),
            ContractParallelCompute = new StellarDotnetSdk.Xdr.ConfigSettingContractParallelComputeV0
            {
                LedgerMaxDependentTxClusters = new Uint32(1234),
            },
        };
        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = xdrConfigSetting,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting = (ConfigSettingContractParallelComputeV0)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.ContractParallelCompute.LedgerMaxDependentTxClusters.InnerValue,
            decodedConfigSetting.LedgerMaxDependentTxClusters);
    }

    [TestMethod]
    public void TestConfigSettingContractLedgerCostExtV0()
    {
        var xdrConfigSetting = new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum
                    .CONFIG_SETTING_CONTRACT_LEDGER_COST_EXT_V0),
            ContractLedgerCostExt = new ConfigSettingContractLedgerCostExtV0
            {
                TxMaxFootprintEntries = new Uint32(233),
                FeeWrite1KB = new Int64(1555),
            },
        };
        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = xdrConfigSetting,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting =
            (LedgerEntries.ConfigSettingContractLedgerCostExtV0)LedgerEntry.FromXdrBase64(
                entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.ContractLedgerCostExt.TxMaxFootprintEntries.InnerValue,
            decodedConfigSetting.TxMaxFootprintEntries);
        Assert.AreEqual(xdrConfigSetting.ContractLedgerCostExt.FeeWrite1KB.InnerValue,
            decodedConfigSetting.FeeWrite1Kb);
    }

    [TestMethod]
    public void TestConfigSettingScpTiming()
    {
        var xdrConfigSetting = new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum
                    .CONFIG_SETTING_SCP_TIMING),
            ContractSCPTiming = new ConfigSettingSCPTiming
            {
                LedgerTargetCloseTimeMilliseconds = new Uint32(1000),
                BallotTimeoutInitialMilliseconds = new Uint32(200),
                BallotTimeoutIncrementMilliseconds = new Uint32(100),
                NominationTimeoutInitialMilliseconds = new Uint32(500),
                NominationTimeoutIncrementMilliseconds = new Uint32(250),
            },
        };
        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING),
            ConfigSetting = xdrConfigSetting,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedConfigSetting =
            (ConfigSettingScpTiming)LedgerEntry.FromXdrBase64(
                entryXdrBase64);

        // Assert
        Assert.AreEqual(xdrConfigSetting.ContractSCPTiming.LedgerTargetCloseTimeMilliseconds.InnerValue,
            decodedConfigSetting.LedgerTargetCloseTimeMilliseconds);
        Assert.AreEqual(xdrConfigSetting.ContractSCPTiming.NominationTimeoutInitialMilliseconds.InnerValue,
            decodedConfigSetting.NominationTimeoutInitialMilliseconds);
        Assert.AreEqual(xdrConfigSetting.ContractSCPTiming.NominationTimeoutIncrementMilliseconds.InnerValue,
            decodedConfigSetting.NominationTimeoutIncrementMilliseconds);
        Assert.AreEqual(xdrConfigSetting.ContractSCPTiming.BallotTimeoutInitialMilliseconds.InnerValue,
            decodedConfigSetting.BallotTimeoutInitialMilliseconds);
        Assert.AreEqual(xdrConfigSetting.ContractSCPTiming.BallotTimeoutIncrementMilliseconds.InnerValue,
            decodedConfigSetting.BallotTimeoutIncrementMilliseconds);
    }

    [TestMethod]
    public void TestLedgerEntryTtlWithAllPropertiesPopulated()
    {
        var xdrTtlEntry = new TTLEntry
        {
            KeyHash = new Hash(new byte[]
                { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 }),
            LiveUntilLedgerSeq = new Uint32(100000),
        };

        var xdrLedgerEntryData = new StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData
        {
            Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.TTL),
            Ttl = xdrTtlEntry,
        };
        var os = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.LedgerEntry.LedgerEntryData.Encode(os, xdrLedgerEntryData);
        var entryXdrBase64 = Convert.ToBase64String(os.ToArray());
        var decodedLedgerEntry = (LedgerEntryTtl)LedgerEntry.FromXdrBase64(entryXdrBase64);

        // Assert
        CollectionAssert.AreEqual(xdrTtlEntry.KeyHash.InnerValue,
            decodedLedgerEntry.KeyHash);
        Assert.AreEqual(xdrTtlEntry.LiveUntilLedgerSeq.InnerValue,
            decodedLedgerEntry.LiveUntilLedgerSequence);
    }
}