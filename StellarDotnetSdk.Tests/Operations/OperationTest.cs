﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Claimants;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using xdrSDK = StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using ClaimPredicate = StellarDotnetSdk.Claimants.ClaimPredicate;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using Operation = StellarDotnetSdk.Operations.Operation;

namespace StellarDotnetSdk.Tests.Operations;

[TestClass]
public class OperationTest
{
    [TestMethod]
    [Obsolete("Deprecated")]
    public void TestCreateAccountOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR
        var destination = KeyPair.FromSecretSeed("SDHZGHURAYXKU2KMVHPOXI6JG2Q4BSQUQCEOY72O3QQTCLR2T455PMII");

        const string startingAmount = "1000";
        var operation = new CreateAccountOperation(destination, startingAmount, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (CreateAccountOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(10000000000L, xdr.Body.CreateAccountOp.StartingBalance.InnerValue);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(destination.AccountId, parsedOperation.Destination.AccountId);
        Assert.AreEqual(startingAmount, parsedOperation.StartingBalance);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAAAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAACVAvkAA==",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete("Deprecated")]
    public void TestPaymentOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR
        var destination = KeyPair.FromSecretSeed("SDHZGHURAYXKU2KMVHPOXI6JG2Q4BSQUQCEOY72O3QQTCLR2T455PMII");

        Asset asset = new AssetTypeNative();
        var amount = "1000";

        var operation = new PaymentOperation(destination, asset, amount, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (PaymentOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(10000000000L, xdr.Body.PaymentOp.Amount.InnerValue);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(destination.AccountId, parsedOperation.Destination.AccountId);
        Assert.IsTrue(parsedOperation.Asset is AssetTypeNative);
        Assert.AreEqual(amount, parsedOperation.Amount);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAEAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAAAAAAAAlQL5AA=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestPathPaymentStrictReceiveOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR
        var destination = KeyPair.FromSecretSeed("SDHZGHURAYXKU2KMVHPOXI6JG2Q4BSQUQCEOY72O3QQTCLR2T455PMII");
        // GCGZLB3X2B3UFOFSHHQ6ZGEPEX7XYPEH6SBFMIV74EUDOFZJA3VNL6X4
        var issuer = KeyPair.FromSecretSeed("SBOBVZUN6WKVMI6KIL2GHBBEETEV6XKQGILITNH6LO6ZA22DBMSDCPAG");

        // GAVAQKT2M7B4V3NN7RNNXPU5CWNDKC27MYHKLF5UNYXH4FNLFVDXKRSV
        var pathIssuer1 = KeyPair.FromSecretSeed("SALDLG5XU5AEJWUOHAJPSC4HJ2IK3Z6BXXP4GWRHFT7P7ILSCFFQ7TC5");
        // GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR
        var pathIssuer2 = KeyPair.FromSecretSeed("SA64U7C5C7BS5IHWEPA7YWFN3Z6FE5L6KAMYUIT4AQ7KVTVLD23C6HEZ");

        Asset sendAsset = new AssetTypeNative();
        var sendMax = "0.0001";
        Asset destAsset = new AssetTypeCreditAlphaNum4("USD", issuer.AccountId);
        var destAmount = "0.0001";
        Asset[] path =
        {
            new AssetTypeCreditAlphaNum4("USD", pathIssuer1.AccountId),
            new AssetTypeCreditAlphaNum12("TESTTEST", pathIssuer2.AccountId),
        };

        var operation = new PathPaymentStrictReceiveOperation(
            sendAsset, sendMax, destination, destAsset, destAmount, path, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (PathPaymentStrictReceiveOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(1000L, xdr.Body.PathPaymentStrictReceiveOp.SendMax.InnerValue);
        Assert.AreEqual(1000L, xdr.Body.PathPaymentStrictReceiveOp.DestAmount.InnerValue);
        Assert.IsTrue(parsedOperation.SendAsset is AssetTypeNative);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(destination.AccountId, parsedOperation.Destination.AccountId);
        Assert.AreEqual(sendMax, parsedOperation.SendMax);
        Assert.IsTrue(parsedOperation.DestAsset is AssetTypeCreditAlphaNum4);
        Assert.AreEqual(destAmount, parsedOperation.DestAmount);
        Assert.AreEqual(path.Length, parsedOperation.Path.Length);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAIAAAAAAAAAAAAAA+gAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAABVVNEAAAAAACNlYd30HdCuLI54eyYjyX/fDyH9IJWIr/hKDcXKQbq1QAAAAAAAAPoAAAAAgAAAAFVU0QAAAAAACoIKnpnw8rtrfxa276dFZo1C19mDqWXtG4ufhWrLUd1AAAAAlRFU1RURVNUAAAAAAAAAABE/ttVl8BLV0csW/xgXtbXOVf1lMyDluMiafl0IDVFIg==",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestPathPaymentStrictReceiveEmptyPathOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR
        var destination = KeyPair.FromSecretSeed("SDHZGHURAYXKU2KMVHPOXI6JG2Q4BSQUQCEOY72O3QQTCLR2T455PMII");
        // GCGZLB3X2B3UFOFSHHQ6ZGEPEX7XYPEH6SBFMIV74EUDOFZJA3VNL6X4
        var issuer = KeyPair.FromSecretSeed("SBOBVZUN6WKVMI6KIL2GHBBEETEV6XKQGILITNH6LO6ZA22DBMSDCPAG");

        // GAVAQKT2M7B4V3NN7RNNXPU5CWNDKC27MYHKLF5UNYXH4FNLFVDXKRSV
        var unused1 = KeyPair.FromSecretSeed("SALDLG5XU5AEJWUOHAJPSC4HJ2IK3Z6BXXP4GWRHFT7P7ILSCFFQ7TC5");
        // GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR
        var unused = KeyPair.FromSecretSeed("SA64U7C5C7BS5IHWEPA7YWFN3Z6FE5L6KAMYUIT4AQ7KVTVLD23C6HEZ");

        Asset sendAsset = new AssetTypeNative();
        var sendMax = "0.0001";
        Asset destAsset = new AssetTypeCreditAlphaNum4("USD", issuer.AccountId);
        var destAmount = "0.0001";

        var operation = new PathPaymentStrictReceiveOperation(
            sendAsset, sendMax, destination, destAsset, destAmount, null, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (PathPaymentStrictReceiveOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(1000L, xdr.Body.PathPaymentStrictReceiveOp.SendMax.InnerValue);
        Assert.AreEqual(1000L, xdr.Body.PathPaymentStrictReceiveOp.DestAmount.InnerValue);
        Assert.IsTrue(parsedOperation.SendAsset is AssetTypeNative);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(destination.AccountId, parsedOperation.Destination.AccountId);
        Assert.AreEqual(sendMax, parsedOperation.SendMax);
        Assert.IsTrue(parsedOperation.DestAsset is AssetTypeCreditAlphaNum4);
        Assert.AreEqual(destAmount, parsedOperation.DestAmount);
        Assert.AreEqual(0, parsedOperation.Path.Length);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAIAAAAAAAAAAAAAA+gAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAABVVNEAAAAAACNlYd30HdCuLI54eyYjyX/fDyH9IJWIr/hKDcXKQbq1QAAAAAAAAPoAAAAAA==",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestPathPaymentStrictSendOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR
        var destination = KeyPair.FromSecretSeed("SDHZGHURAYXKU2KMVHPOXI6JG2Q4BSQUQCEOY72O3QQTCLR2T455PMII");
        // GCGZLB3X2B3UFOFSHHQ6ZGEPEX7XYPEH6SBFMIV74EUDOFZJA3VNL6X4
        var issuer = KeyPair.FromSecretSeed("SBOBVZUN6WKVMI6KIL2GHBBEETEV6XKQGILITNH6LO6ZA22DBMSDCPAG");

        // GAVAQKT2M7B4V3NN7RNNXPU5CWNDKC27MYHKLF5UNYXH4FNLFVDXKRSV
        var pathIssuer1 = KeyPair.FromSecretSeed("SALDLG5XU5AEJWUOHAJPSC4HJ2IK3Z6BXXP4GWRHFT7P7ILSCFFQ7TC5");
        // GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR
        var pathIssuer2 = KeyPair.FromSecretSeed("SA64U7C5C7BS5IHWEPA7YWFN3Z6FE5L6KAMYUIT4AQ7KVTVLD23C6HEZ");

        Asset sendAsset = new AssetTypeNative();
        var sendAmount = "0.0001";
        Asset destAsset = new AssetTypeCreditAlphaNum4("USD", issuer.AccountId);
        var destMin = "0.0001";
        Asset[] path =
        {
            new AssetTypeCreditAlphaNum4("USD", pathIssuer1.AccountId),
            new AssetTypeCreditAlphaNum12("TESTTEST", pathIssuer2.AccountId),
        };

        var operation = new PathPaymentStrictSendOperation(
            sendAsset, sendAmount, destination, destAsset, destMin, path, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (PathPaymentStrictSendOperation)Operation.FromXdr(xdr);

        Assert.IsTrue(parsedOperation.SendAsset is AssetTypeNative);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(destination.AccountId, parsedOperation.Destination.AccountId);
        Assert.AreEqual(sendAmount, parsedOperation.SendAmount);
        Assert.IsTrue(parsedOperation.DestAsset is AssetTypeCreditAlphaNum4);
        Assert.AreEqual(destMin, parsedOperation.DestMin);
        Assert.AreEqual(path.Length, parsedOperation.Path.Length);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAA0AAAAAAAAAAAAAA+gAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAABVVNEAAAAAACNlYd30HdCuLI54eyYjyX/fDyH9IJWIr/hKDcXKQbq1QAAAAAAAAPoAAAAAgAAAAFVU0QAAAAAACoIKnpnw8rtrfxa276dFZo1C19mDqWXtG4ufhWrLUd1AAAAAlRFU1RURVNUAAAAAAAAAABE/ttVl8BLV0csW/xgXtbXOVf1lMyDluMiafl0IDVFIg==",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestPathPaymentStrictSendEmptyPathOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR
        var destination = KeyPair.FromSecretSeed("SDHZGHURAYXKU2KMVHPOXI6JG2Q4BSQUQCEOY72O3QQTCLR2T455PMII");
        // GCGZLB3X2B3UFOFSHHQ6ZGEPEX7XYPEH6SBFMIV74EUDOFZJA3VNL6X4
        var issuer = KeyPair.FromSecretSeed("SBOBVZUN6WKVMI6KIL2GHBBEETEV6XKQGILITNH6LO6ZA22DBMSDCPAG");

        // GAVAQKT2M7B4V3NN7RNNXPU5CWNDKC27MYHKLF5UNYXH4FNLFVDXKRSV
        var unused1 = KeyPair.FromSecretSeed("SALDLG5XU5AEJWUOHAJPSC4HJ2IK3Z6BXXP4GWRHFT7P7ILSCFFQ7TC5");
        // GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR
        var unused = KeyPair.FromSecretSeed("SA64U7C5C7BS5IHWEPA7YWFN3Z6FE5L6KAMYUIT4AQ7KVTVLD23C6HEZ");

        Asset sendAsset = new AssetTypeNative();
        var sendAmount = "0.0001";
        Asset destAsset = new AssetTypeCreditAlphaNum4("USD", issuer.AccountId);
        var destMin = "0.0001";

        var operation = new PathPaymentStrictSendOperation(
            sendAsset, sendAmount, destination, destAsset, destMin, null, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (PathPaymentStrictSendOperation)Operation.FromXdr(xdr);

        Assert.IsTrue(parsedOperation.SendAsset is AssetTypeNative);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(destination.AccountId, parsedOperation.Destination.AccountId);
        Assert.AreEqual(sendAmount, parsedOperation.SendAmount);
        Assert.IsTrue(parsedOperation.DestAsset is AssetTypeCreditAlphaNum4);
        Assert.AreEqual(destMin, parsedOperation.DestMin);
        Assert.AreEqual(0, parsedOperation.Path.Length);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAA0AAAAAAAAAAAAAA+gAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAABVVNEAAAAAACNlYd30HdCuLI54eyYjyX/fDyH9IJWIr/hKDcXKQbq1QAAAAAAAAPoAAAAAA==",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestChangeTrustOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var limit = ChangeTrustOperation.MaxLimit;

        var operation = new ChangeTrustOperation(new AssetTypeNative(), limit, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (ChangeTrustOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(long.MaxValue, xdr.Body.ChangeTrustOp.Limit.InnerValue);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)parsedOperation.Asset).Asset is AssetTypeNative);
        Assert.AreEqual(limit, parsedOperation.Limit);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAYAAAAAf/////////8=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestChangeTrustOperationNoLimit()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var operation = new ChangeTrustOperation(new AssetTypeNative(), null, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (ChangeTrustOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(long.MaxValue, xdr.Body.ChangeTrustOp.Limit.InnerValue);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)parsedOperation.Asset).Asset is AssetTypeNative);
        Assert.AreEqual(ChangeTrustOperation.MaxLimit, parsedOperation.Limit);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAYAAAAAf/////////8=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    public void TestManageOfferOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR
        var issuer = KeyPair.FromSecretSeed("SA64U7C5C7BS5IHWEPA7YWFN3Z6FE5L6KAMYUIT4AQ7KVTVLD23C6HEZ");

        Asset selling = new AssetTypeNative();
        var buying = Asset.CreateNonNativeAsset("USD", issuer.AccountId);
        var amount = "0.00001";
        var price = "0.85334384"; // n=5333399 d=6250000
        Price.FromString(price);
        long offerId = 1;

        var operation = new ManageSellOfferOperation(selling, buying, amount, price, offerId, source);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAMAAAAAAAAAAVVTRAAAAAAARP7bVZfAS1dHLFv8YF7W1zlX9ZTMg5bjImn5dCA1RSIAAAAAAAAAZABRYZcAX14QAAAAAAAAAAE=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestManageSellOfferOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR
        var issuer = KeyPair.FromSecretSeed("SA64U7C5C7BS5IHWEPA7YWFN3Z6FE5L6KAMYUIT4AQ7KVTVLD23C6HEZ");

        Asset selling = new AssetTypeNative();
        var buying = Asset.CreateNonNativeAsset("USD", issuer.AccountId);
        var amount = "0.00001";
        var price = "0.85334384"; // n=5333399 d=6250000
        var priceObj = Price.FromString(price);
        long offerId = 1;

        var operation = new ManageSellOfferOperation(selling, buying, amount, price, offerId, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (ManageSellOfferOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(100L, xdr.Body.ManageSellOfferOp.Amount.InnerValue);
        Assert.IsTrue(parsedOperation.Selling is AssetTypeNative);
        Assert.IsTrue(parsedOperation.Buying is AssetTypeCreditAlphaNum4);
        Assert.IsTrue(parsedOperation.Buying.Equals(buying));
        Assert.AreEqual(amount, parsedOperation.Amount);
        Assert.AreEqual(price, parsedOperation.Price);
        Assert.AreEqual(priceObj.Numerator, 5333399);
        Assert.AreEqual(priceObj.Denominator, 6250000);
        Assert.AreEqual(offerId, parsedOperation.OfferId);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAMAAAAAAAAAAVVTRAAAAAAARP7bVZfAS1dHLFv8YF7W1zlX9ZTMg5bjImn5dCA1RSIAAAAAAAAAZABRYZcAX14QAAAAAAAAAAE=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestManageBuyOfferOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR
        var issuer = KeyPair.FromSecretSeed("SA64U7C5C7BS5IHWEPA7YWFN3Z6FE5L6KAMYUIT4AQ7KVTVLD23C6HEZ");

        Asset selling = new AssetTypeNative();
        var buying = Asset.CreateNonNativeAsset("USD", issuer.AccountId);
        var amount = "0.00001";
        var price = "0.85334384"; // n=5333399 d=6250000
        var priceObj = Price.FromString(price);
        long offerId = 1;

        var operation = new ManageBuyOfferOperation(selling, buying, amount, price, offerId, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (ManageBuyOfferOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(100L, xdr.Body.ManageBuyOfferOp.BuyAmount.InnerValue);
        Assert.IsTrue(parsedOperation.Selling is AssetTypeNative);
        Assert.IsTrue(parsedOperation.Buying is AssetTypeCreditAlphaNum4);
        Assert.IsTrue(parsedOperation.Buying.Equals(buying));
        Assert.AreEqual(amount, parsedOperation.BuyAmount);
        Assert.AreEqual(price, parsedOperation.Price);
        Assert.AreEqual(priceObj.Numerator, 5333399);
        Assert.AreEqual(priceObj.Denominator, 6250000);
        Assert.AreEqual(offerId, parsedOperation.OfferId);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAwAAAAAAAAAAVVTRAAAAAAARP7bVZfAS1dHLFv8YF7W1zlX9ZTMg5bjImn5dCA1RSIAAAAAAAAAZABRYZcAX14QAAAAAAAAAAE=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    public void TestCreatePassiveOfferOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR
        var issuer = KeyPair.FromSecretSeed("SA64U7C5C7BS5IHWEPA7YWFN3Z6FE5L6KAMYUIT4AQ7KVTVLD23C6HEZ");

        Asset selling = new AssetTypeNative();
        var buying = Asset.CreateNonNativeAsset("USD", issuer.AccountId);
        var amount = "0.00001";
        var price = "2.93850088"; // n=36731261 d=12500000
        Price.FromString(price);

        var operation = new CreatePassiveSellOfferOperation(selling, buying, amount, price, source);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAQAAAAAAAAAAVVTRAAAAAAARP7bVZfAS1dHLFv8YF7W1zlX9ZTMg5bjImn5dCA1RSIAAAAAAAAAZAIweX0Avrwg",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestCreatePassiveSellOfferOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR
        var issuer = KeyPair.FromSecretSeed("SA64U7C5C7BS5IHWEPA7YWFN3Z6FE5L6KAMYUIT4AQ7KVTVLD23C6HEZ");

        Asset selling = new AssetTypeNative();
        var buying = Asset.CreateNonNativeAsset("USD", issuer.AccountId);
        var amount = "0.00001";
        var price = "2.93850088"; // n=36731261 d=12500000
        var priceObj = Price.FromString(price);

        var operation = new CreatePassiveSellOfferOperation(selling, buying, amount, price, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (CreatePassiveSellOfferOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(100L, xdr.Body.CreatePassiveSellOfferOp.Amount.InnerValue);
        Assert.IsTrue(parsedOperation.Selling is AssetTypeNative);
        Assert.IsTrue(parsedOperation.Buying is AssetTypeCreditAlphaNum4);
        Assert.IsTrue(parsedOperation.Buying.Equals(buying));
        Assert.AreEqual(amount, parsedOperation.Amount);
        Assert.AreEqual(price, parsedOperation.Price);
        Assert.AreEqual(priceObj.Numerator, 36731261);
        Assert.AreEqual(priceObj.Denominator, 12500000);
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAQAAAAAAAAAAVVTRAAAAAAARP7bVZfAS1dHLFv8YF7W1zlX9ZTMg5bjImn5dCA1RSIAAAAAAAAAZAIweX0Avrwg",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestAccountMergeOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR
        var destination = KeyPair.FromSecretSeed("SDHZGHURAYXKU2KMVHPOXI6JG2Q4BSQUQCEOY72O3QQTCLR2T455PMII");

        var operation = new AccountMergeOperation(destination, source);

        var xdr = operation.ToXdr();

        var parsedOperation = (AccountMergeOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(destination.AccountId, parsedOperation.Destination.AccountId);
        Assert.AreEqual(OperationThreshold.HIGH, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAgAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxA=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestManageDataOperation()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var operation = new ManageDataOperation("test", new byte[] { 0, 1, 2, 3, 4 }, source);

        var xdr = operation.ToXdr();

        var parsedOperation = (ManageDataOperation)Operation.FromXdr(xdr);

        Assert.AreEqual("test", parsedOperation.Name);
        Assert.IsNotNull(parsedOperation.Value);
        Assert.IsTrue(new byte[] { 0, 1, 2, 3, 4 }.SequenceEqual(parsedOperation.Value));
        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAoAAAAEdGVzdAAAAAEAAAAFAAECAwQAAAA=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestManageDataOperationEmptyValue()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var operation = new ManageDataOperation("test", (string?)null, source);

        var xdr = operation.ToXdr();

        var parsedOperation = (ManageDataOperation)Operation.FromXdr(xdr);

        Assert.AreEqual("test", parsedOperation.Name);
        Assert.AreEqual(null, parsedOperation.Value);

        Assert.AreEqual("AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAoAAAAEdGVzdAAAAAA=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    public void TestToXdrAmount()
    {
        Assert.AreEqual(0L, Operation.ToXdrAmount("0"));
        Assert.AreEqual(1L, Operation.ToXdrAmount("0.0000001"));
        Assert.AreEqual(10000000L, Operation.ToXdrAmount("1"));
        Assert.AreEqual(11234567L, Operation.ToXdrAmount("1.1234567"));
        Assert.AreEqual(729912843007381L, Operation.ToXdrAmount("72991284.3007381"));
        Assert.AreEqual(729912843007381L, Operation.ToXdrAmount("72991284.30073810"));
        Assert.AreEqual(1014016711446800155L, Operation.ToXdrAmount("101401671144.6800155"));
        Assert.AreEqual(9223372036854775807L, Operation.ToXdrAmount("922337203685.4775807"));

        try
        {
            Operation.ToXdrAmount("0.00000001");
            Assert.Fail();
        }
        catch (ArithmeticException)
        {
        }
        catch (Exception)
        {
            Assert.Fail();
        }

        try
        {
            Operation.ToXdrAmount("72991284.30073811");
            Assert.Fail();
        }
        catch (ArithmeticException)
        {
        }
        catch (Exception)
        {
            Assert.Fail();
        }
    }

    [TestMethod]
    [Obsolete]
    public void TestBumpSequence()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var operation = new BumpSequenceOperation(156L, source);

        var xdr = operation.ToXdr();

        var parsedOperation = (BumpSequenceOperation)Operation.FromXdr(xdr);

        Assert.AreEqual(156L, parsedOperation.BumpTo);
        Assert.AreEqual(OperationThreshold.LOW, parsedOperation.Threshold);

        Assert.AreEqual("AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAsAAAAAAAAAnA==",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestCreateClaimableBalanceOperation()
    {
        // GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");

        // GAS4V4O2B7DW5T7IQRPEEVCRXMDZESKISR7DVIGKZQYYV3OSQ5SH5LVP
        var destination = KeyPair.FromSecretSeed("SBMSVD4KKELKGZXHBUQTIROWUAPQASDX7KEJITARP4VMZ6KLUHOGPTYW");

        var asset = new AssetTypeNative();
        var claimant = new Claimant(destination, ClaimPredicate.Not(ClaimPredicate.BeforeRelativeTime(
            new xdrSDK.Duration(new xdrSDK.Uint64((ulong)TimeSpan.FromHours(7.0).TotalSeconds)))));

        var operation = new CreateClaimableBalanceOperation(asset, "123.45", new[] { claimant }, source);

        var xdr = operation.ToXdr();

        var parsedOperation = (CreateClaimableBalanceOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, parsedOperation.SourceAccount.AccountId);

        Assert.AreEqual(
            "AAAAAQAAAADg3G3hclysZlFitS+s5zWyiiJD5B0STWy5LXCj6i5yxQAAAA4AAAAAAAAAAEmU+aAAAAABAAAAAAAAAAAlyvHaD8duz+iEXkJUUbsHkklIlH46oMrMMYrt0odkfgAAAAMAAAABAAAABQAAAAAAAGJw",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [DataRow("000000006d6a0c142516a9cc7885a85c5aba3a1f4af5181cf9e7a809ac7ae5e4a58c825f")]
    public void TestClaimClaimableBalanceOperationConstructorWithValidArgument(string id)
    {
        var accountId = KeyPair.FromAccountId("GABTTS6N4CT7AUN4LD7IFIUMRD5PSMCW6QTLIQNEFZDEI6ZQVUCQMCLN");
        var operation = new ClaimClaimableBalanceOperation(id, accountId);

        var xdr = operation.ToXdr();
        var parsedOperation = (ClaimClaimableBalanceOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(operation.BalanceId.ToUpper(), parsedOperation.BalanceId.ToUpper());

        Assert.AreEqual(
            "AAAAAQAAAAADOcvN4KfwUbxY/oKijIj6+TBW9Ca0QaQuRkR7MK0FBgAAAA8AAAAAbWoMFCUWqcx4hahcWro6H0r1GBz556gJrHrl5KWMgl8=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [DataRow("")]
    [DataRow("00000000")]
    [DataRow("00846c047755e4a46912336f56096b48ece78ddb5fbf6d90f0eb4ecae5324fbddb")]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4TU")]
    public void TestClaimClaimableBalanceOperationConstructorWithInvalidArgument(string id)
    {
        var accountId = KeyPair.FromAccountId("GABTTS6N4CT7AUN4LD7IFIUMRD5PSMCW6QTLIQNEFZDEI6ZQVUCQMCLN");
        _ = new ClaimClaimableBalanceOperation(id, accountId);
    }

    [TestMethod]
    [Obsolete]
    public void TestBeginSponsoringFutureReservesOperation()
    {
        // GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");

        // GAS4V4O2B7DW5T7IQRPEEVCRXMDZESKISR7DVIGKZQYYV3OSQ5SH5LVP
        var sponsored = KeyPair.FromSecretSeed("SBMSVD4KKELKGZXHBUQTIROWUAPQASDX7KEJITARP4VMZ6KLUHOGPTYW");

        var operation = new BeginSponsoringFutureReservesOperation(sponsored, source);

        var xdr = operation.ToXdr();

        var parsedOperation = (BeginSponsoringFutureReservesOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, parsedOperation.SourceAccount.AccountId);

        Assert.AreEqual(
            "AAAAAQAAAADg3G3hclysZlFitS+s5zWyiiJD5B0STWy5LXCj6i5yxQAAABAAAAAAJcrx2g/Hbs/ohF5CVFG7B5JJSJR+OqDKzDGK7dKHZH4=",
            operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestEndSponsoringFutureReservesOperation()
    {
        // GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");

        var operation = new EndSponsoringFutureReservesOperation(source);

        var xdr = operation.ToXdr();

        var parsedOperation = (EndSponsoringFutureReservesOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, parsedOperation.SourceAccount.AccountId);

        Assert.AreEqual("AAAAAQAAAADg3G3hclysZlFitS+s5zWyiiJD5B0STWy5LXCj6i5yxQAAABE=", operation.ToXdrBase64());
    }

    [TestMethod]
    [Obsolete]
    public void TestRevokeLedgerEntrySponsorshipOperation()
    {
        // GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");

        // GAS4V4O2B7DW5T7IQRPEEVCRXMDZESKISR7DVIGKZQYYV3OSQ5SH5LVP
        var otherAccount = KeyPair.FromSecretSeed("SBMSVD4KKELKGZXHBUQTIROWUAPQASDX7KEJITARP4VMZ6KLUHOGPTYW");

        var ledgerKey = LedgerKey.Account(otherAccount);
        var operation = new RevokeLedgerEntrySponsorshipOperation(ledgerKey, source);

        var xdr = operation.ToXdr();

        var parsedOperation = (RevokeLedgerEntrySponsorshipOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, parsedOperation.SourceAccount.AccountId);

        Assert.AreEqual(
            "AAAAAQAAAADg3G3hclysZlFitS+s5zWyiiJD5B0STWy5LXCj6i5yxQAAABIAAAAAAAAAAAAAAAAlyvHaD8duz+iEXkJUUbsHkklIlH46oMrMMYrt0odkfg==",
            operation.ToXdrBase64());
    }

    [TestMethod]
    public void TestRevokeClaimableBalanceSponsorshipOperation()
    {
        var operation =
            RevokeLedgerEntrySponsorshipOperation.ForClaimableBalance(
                "00000000d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780");

        var xdrOperation = operation.ToXdr();
        var decodedOperation = (RevokeLedgerEntrySponsorshipOperation)Operation.FromXdr(xdrOperation);

        Assert.IsNull(decodedOperation.SourceAccount);
        Assert.AreEqual(((LedgerKeyClaimableBalance)operation.LedgerKey).BalanceId.ToUpper(),
            ((LedgerKeyClaimableBalance)decodedOperation.LedgerKey).BalanceId.ToUpper());
    }

    [TestMethod]
    [Obsolete]
    public void TestRevokeSignerSponsorshipOperation()
    {
        // GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");

        // GAS4V4O2B7DW5T7IQRPEEVCRXMDZESKISR7DVIGKZQYYV3OSQ5SH5LVP
        var otherAccount = KeyPair.FromSecretSeed("SBMSVD4KKELKGZXHBUQTIROWUAPQASDX7KEJITARP4VMZ6KLUHOGPTYW");

        var signerKey = SignerUtil.Ed25519PublicKey(otherAccount);

        var operation = new RevokeSignerSponsorshipOperation(otherAccount, signerKey, source);

        var xdr = operation.ToXdr();

        var parsedOperation = (RevokeSignerSponsorshipOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, parsedOperation.SourceAccount.AccountId);

        Assert.AreEqual(
            "AAAAAQAAAADg3G3hclysZlFitS+s5zWyiiJD5B0STWy5LXCj6i5yxQAAABIAAAABAAAAACXK8doPx27P6IReQlRRuweSSUiUfjqgyswxiu3Sh2R+AAAAACXK8doPx27P6IReQlRRuweSSUiUfjqgyswxiu3Sh2R+",
            operation.ToXdrBase64());
    }

    [TestMethod]
    public void TestFromXdrAmount()
    {
        Assert.AreEqual("0", Operation.FromXdrAmount(0L));
        Assert.AreEqual("0.0000001", Operation.FromXdrAmount(1L));
        Assert.AreEqual("1", Operation.FromXdrAmount(10000000L));
        Assert.AreEqual("1.1234567", Operation.FromXdrAmount(11234567L));
        Assert.AreEqual("72991284.3007381", Operation.FromXdrAmount(729912843007381L));
        Assert.AreEqual("101401671144.6800155", Operation.FromXdrAmount(1014016711446800155L));
        Assert.AreEqual("922337203685.4775807", Operation.FromXdrAmount(9223372036854775807L));
    }

    [TestMethod]
    [Obsolete]
    public void TestClawbackOperation()
    {
        // GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");

        var operation =
            new ClawbackOperation(
                Asset.CreateNonNativeAsset("EUR", "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM"),
                "1000", KeyPair.FromAccountId("GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7"),
                source);

        var xdr = operation.ToXdr();

        var parsedOperation = (ClawbackOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(operation.Amount, parsedOperation.Amount);
        Assert.AreEqual(operation.Asset, parsedOperation.Asset);
        Assert.AreEqual(operation.From.AccountId, parsedOperation.From.AccountId);
    }

    [TestMethod]
    public void TestClawbackClaimableBalanceOperation()
    {
        // GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");

        var operation = new ClawbackClaimableBalanceOperation(
            "00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7", source);

        var xdr = operation.ToXdr();

        var parsedOperation = (ClawbackClaimableBalanceOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(operation.BalanceId.ToUpper(), parsedOperation.BalanceId.ToUpper());
    }

    [TestMethod]
    [Obsolete("Deprecated")]
    public void TestSetTrustlineFlagsOperation()
    {
        // GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");

        var operation = new SetTrustlineFlagsOperation(
            Asset.CreateNonNativeAsset("EUR", "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM"),
            KeyPair.Random(), 1, 1,
            source);

        var xdr = operation.ToXdr();

        var parsedOperation = (SetTrustlineFlagsOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(operation.Asset, parsedOperation.Asset);
        Assert.AreEqual(operation.Trustor.AccountId, parsedOperation.Trustor.AccountId);
        Assert.AreEqual(operation.SetFlags, parsedOperation.SetFlags);
        Assert.AreEqual(operation.ClearFlags, parsedOperation.ClearFlags);
    }

    [TestMethod]
    public void TestLiquidityPoolDepositOperationConstructor1()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var keypairAssetA = KeyPair.Random();
        var keypairAssetB = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypairAssetA.AccountId}");
        var assetB = Asset.Create($"USD:{keypairAssetB.AccountId}");

        var assetAmountA = new AssetAmount(assetA, "100");
        var assetAmountB = new AssetAmount(assetB, "200");

        var minPrice = Price.FromString("0.01");
        var maxPrice = Price.FromString("0.02");

        var operation = new LiquidityPoolDepositOperation(assetAmountA, assetAmountB, minPrice, maxPrice, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (LiquidityPoolDepositOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(operation.LiquidityPoolId, parsedOperation.LiquidityPoolId);
        Assert.AreEqual(operation.MaxAmountA, parsedOperation.MaxAmountA);
        Assert.AreEqual(operation.MaxAmountB, parsedOperation.MaxAmountB);
        Assert.AreEqual(operation.MinPrice, parsedOperation.MinPrice);
        Assert.AreEqual(operation.MaxPrice, parsedOperation.MaxPrice);

        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);
    }

    [TestMethod]
    public void TestLiquidityPoolDepositOperationConstructor2()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var keypairAssetA = KeyPair.Random();
        var keypairAssetB = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypairAssetA.AccountId}");
        var assetB = Asset.Create($"USD:{keypairAssetB.AccountId}");

        var liquidityPoolId = new LiquidityPoolId(
            xdrSDK.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            assetA,
            assetB,
            LiquidityPoolParameters.Fee
        );

        var minPrice = Price.FromString("0.01");
        var maxPrice = Price.FromString("0.02");

        var operation = new LiquidityPoolDepositOperation(liquidityPoolId, "100", "200", minPrice, maxPrice, source);

        var xdr = operation.ToXdr();
        var parsedOperation = (LiquidityPoolDepositOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(operation.LiquidityPoolId, parsedOperation.LiquidityPoolId);
        Assert.AreEqual(operation.MaxAmountA, parsedOperation.MaxAmountA);
        Assert.AreEqual(operation.MaxAmountB, parsedOperation.MaxAmountB);
        Assert.AreEqual(operation.MinPrice, parsedOperation.MinPrice);
        Assert.AreEqual(operation.MaxPrice, parsedOperation.MaxPrice);

        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);
    }

    [TestMethod]
    public void TestTestLiquidityPoolConstructionNotLexicographicOrder()
    {
        var keypairAssetA = KeyPair.Random();
        var keypairAssetB = KeyPair.Random();

        var assetA = Asset.Create($"USD:{keypairAssetB.AccountId}");
        var assetB = Asset.Create($"EUR:{keypairAssetA.AccountId}");

        var assetAmountA = new AssetAmount(assetA, "100");
        var assetAmountB = new AssetAmount(assetB, "200");

        var minPrice = Price.FromString("0.01");
        var maxPrice = Price.FromString("0.02");

        Assert.ThrowsException<ArgumentException>(
            () => new LiquidityPoolDepositOperation(assetAmountA, assetAmountB, minPrice, maxPrice),
            "Asset A must be < Asset B (Lexicographic Order)");
    }

    [TestMethod]
    public void TestLiquidityPoolWithdrawOperationConstructor1()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var keypairAssetA = KeyPair.Random();
        var keypairAssetB = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypairAssetA.AccountId}");
        var assetB = Asset.Create($"USD:{keypairAssetB.AccountId}");

        var assetAmountA = new AssetAmount(assetA, "100");
        var assetAmountB = new AssetAmount(assetB, "200");

        var operation = new LiquidityPoolWithdrawOperation(assetAmountA, assetAmountB, "100", source);

        var xdr = operation.ToXdr();
        var parsedOperation = (LiquidityPoolWithdrawOperation)Operation.FromXdr(xdr);

        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(operation.LiquidityPoolId, parsedOperation.LiquidityPoolId);
        Assert.AreEqual(operation.MinAmountA, parsedOperation.MinAmountA);
        Assert.AreEqual(operation.MinAmountB, parsedOperation.MinAmountB);
        Assert.AreEqual(operation.Amount, parsedOperation.Amount);

        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);
    }

    [TestMethod]
    public void TestLiquidityPoolWithdrawOperationConstructor2()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var keypairAssetA = KeyPair.Random();
        var keypairAssetB = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypairAssetA.AccountId}");
        var assetB = Asset.Create($"USD:{keypairAssetB.AccountId}");

        var liquidityPoolId = new LiquidityPoolId(
            xdrSDK.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            assetA,
            assetB,
            LiquidityPoolParameters.Fee
        );

        var operation = new LiquidityPoolWithdrawOperation(liquidityPoolId, "100", "100", "200", source);

        var xdr = operation.ToXdr();
        var parsedOperation = (LiquidityPoolWithdrawOperation)Operation.FromXdr(xdr);
        Assert.IsNotNull(parsedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, parsedOperation.SourceAccount.AccountId);
        Assert.AreEqual(operation.LiquidityPoolId, parsedOperation.LiquidityPoolId);
        Assert.AreEqual(operation.MinAmountA, parsedOperation.MinAmountA);
        Assert.AreEqual(operation.MinAmountB, parsedOperation.MinAmountB);
        Assert.AreEqual(operation.Amount, parsedOperation.Amount);

        Assert.AreEqual(OperationThreshold.MEDIUM, parsedOperation.Threshold);
    }

    [TestMethod]
    public void TestNotLexicographicOrder()
    {
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var keypairAssetA = KeyPair.Random();
        var keypairAssetB = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypairAssetA.AccountId}");
        var assetB = Asset.Create($"USD:{keypairAssetB.AccountId}");

        var assetAmountA = new AssetAmount(assetA, "100");
        var assetAmountB = new AssetAmount(assetB, "200");

        var ex = Assert.ThrowsException<ArgumentException>(() =>
            new LiquidityPoolWithdrawOperation(assetAmountB, assetAmountA, "100", source));
        Assert.AreEqual("Invalid Liquidity Pool ID", ex.Message);
    }

    [TestMethod]
    public async Task TestUploadContractOperation()
    {
        // GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");
        var helloWasmPath = Path.GetFullPath("TestData/Wasm/soroban_hello_world_contract.wasm");
        var wasm = await File.ReadAllBytesAsync(helloWasmPath);

        var operation = new UploadContractOperation(wasm, source);

        var xdrOperation = operation.ToXdr();

        var decodedOperation = (UploadContractOperation)Operation.FromXdr(xdrOperation);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, decodedOperation.SourceAccount.AccountId);
        CollectionAssert.AreEqual(operation.Wasm, decodedOperation.Wasm);
    }

    [TestMethod]
    public void TestCreateContractOperation()
    {
        // GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");
        const string wasmHash = "c1a650506f7c20c8f4d16aae73f894f302cd011d7ef33adef572f20b34f7653e";
        var arguments = new SCVal[] { new SCString("test"), new SCInt64(100) };
        var operation = CreateContractOperation.FromAddress(wasmHash, source.AccountId, arguments, null, source);

        var xdrOperation = operation.ToXdr();

        var decodedOperation = (CreateContractOperation)Operation.FromXdr(xdrOperation);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, decodedOperation.SourceAccount.AccountId);
        var hostFunction = operation.HostFunction;
        var decodedHostFunction = decodedOperation.HostFunction;

        var executable = (ContractExecutableWasm)hostFunction.Executable;
        var decodedExecutable = (ContractExecutableWasm)decodedHostFunction.Executable;
        Assert.AreEqual(executable.WasmHash.ToLower(), decodedExecutable.WasmHash.ToLower());

        var preimage = (ContractIdAddressPreimage)hostFunction.ContractIdPreimage;
        var decodedPreimage = (ContractIdAddressPreimage)decodedHostFunction.ContractIdPreimage;
        Assert.AreEqual(((ScAccountId)preimage.Address).InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        CollectionAssert.AreEqual(preimage.Salt, decodedPreimage.Salt);

        var decodedArguments = decodedHostFunction.Arguments;
        Assert.AreEqual(arguments.Length, decodedArguments.Length);
        Assert.AreEqual(((SCString)arguments[0]).InnerValue, ((SCString)decodedArguments[0]).InnerValue);
        Assert.AreEqual(((SCInt64)arguments[1]).InnerValue, ((SCInt64)decodedArguments[1]).InnerValue);
    }

    [TestMethod]
    public void TestInvokeContractOperationConstructor1()
    {
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");
        var operation = new InvokeContractOperation(
            new ScContractId("CDSUR2JFKSUORJLUA2FISW7P6ALDTS2PDK6AYQZ7G4CSY5WZS5QVSM47"),
            new SCSymbol("init"),
            [new SCString("test"), new SCBool(true)],
            source);


        var xdrOperation = operation.ToXdr();

        var decodedOperation = (InvokeContractOperation)Operation.FromXdr(xdrOperation);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, decodedOperation.SourceAccount.AccountId);
        var hostFunction = operation.HostFunction;
        var decodedHostFunction = decodedOperation.HostFunction;

        var address = (ScContractId)hostFunction.ContractAddress;
        var decodedAddress = (ScContractId)decodedHostFunction.ContractAddress;
        Assert.AreEqual(address.InnerValue, decodedAddress.InnerValue);
        Assert.AreEqual(hostFunction.FunctionName.InnerValue, decodedHostFunction.FunctionName.InnerValue);

        var arguments = hostFunction.Args;
        var decodedArguments = decodedHostFunction.Args;
        Assert.AreEqual(arguments.Length, decodedArguments.Length);
        Assert.AreEqual(((SCString)arguments[0]).InnerValue, ((SCString)decodedArguments[0]).InnerValue);
        Assert.AreEqual(((SCBool)arguments[1]).InnerValue, ((SCBool)decodedArguments[1]).InnerValue);
    }

    [TestMethod]
    public void TestInvokeContractOperationConstructor2()
    {
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");
        var operation = new InvokeContractOperation(
            "CDSUR2JFKSUORJLUA2FISW7P6ALDTS2PDK6AYQZ7G4CSY5WZS5QVSM47",
            "init",
            [new SCString("test"), new SCBool(true)],
            source);


        var xdrOperation = operation.ToXdr();

        var decodedOperation = (InvokeContractOperation)Operation.FromXdr(xdrOperation);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, decodedOperation.SourceAccount.AccountId);
        var hostFunction = operation.HostFunction;
        var decodedHostFunction = decodedOperation.HostFunction;

        var address = (ScContractId)hostFunction.ContractAddress;
        var decodedAddress = (ScContractId)decodedHostFunction.ContractAddress;
        Assert.AreEqual(address.InnerValue, decodedAddress.InnerValue);
        Assert.AreEqual(hostFunction.FunctionName.InnerValue, decodedHostFunction.FunctionName.InnerValue);

        var arguments = hostFunction.Args;
        var decodedArguments = decodedHostFunction.Args;
        Assert.AreEqual(arguments.Length, decodedArguments.Length);
        Assert.AreEqual(((SCString)arguments[0]).InnerValue, ((SCString)decodedArguments[0]).InnerValue);
        Assert.AreEqual(((SCBool)arguments[1]).InnerValue, ((SCBool)decodedArguments[1]).InnerValue);
    }

    [TestMethod]
    public void TestDeploySacOperation()
    {
        var source = KeyPair.FromSecretSeed("SBPQUZ6G4FZNWFHKUWC5BEYWF6R52E3SEP7R3GWYSM2XTKGF5LNTWW4R");
        Asset asset = new AssetTypeCreditAlphaNum4("VNDT", source.AccountId);
        var operation = CreateContractOperation.FromAsset(asset, null, source);

        var xdrOperation = operation.ToXdr();

        var decodedOperation = (CreateContractOperation)Operation.FromXdr(xdrOperation);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, decodedOperation.SourceAccount.AccountId);
        var hostFunction = operation.HostFunction;
        var decodedHostFunction = decodedOperation.HostFunction;

        Assert.IsInstanceOfType(hostFunction.Executable, typeof(ContractExecutableStellarAsset));
        Assert.IsInstanceOfType(decodedHostFunction.Executable, typeof(ContractExecutableStellarAsset));

        Assert.IsInstanceOfType(hostFunction.ContractIdPreimage, typeof(ContractIdAssetPreimage));
        Assert.IsInstanceOfType(decodedHostFunction.ContractIdPreimage, typeof(ContractIdAssetPreimage));

        var decodedAsset = ((ContractIdAssetPreimage)decodedHostFunction.ContractIdPreimage).Asset;
        Assert.AreEqual(((AssetTypeCreditAlphaNum4)asset).Code, ((AssetTypeCreditAlphaNum4)decodedAsset).Code);
        Assert.AreEqual(((AssetTypeCreditAlphaNum4)asset).Issuer, ((AssetTypeCreditAlphaNum4)decodedAsset).Issuer);
    }
}