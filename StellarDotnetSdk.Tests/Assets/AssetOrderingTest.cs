using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Assets;

/// <summary>
///     Pins <see cref="Asset.CompareTo" /> to the order stellar-core uses, which compares the XDR assets field by
///     field (type, zero-padded code bytes, then the issuer's raw 32-byte key). For these fixed-size encodings that is
///     the same as comparing the serialized XDR bytes.
/// </summary>
[TestClass]
public class AssetOrderingTest
{
    // From issue #257: A is lower by key bytes but higher as strkey text, because the issuers first differ at
    // 'V' vs '5' and base32 puts the digits 2-7 after 'Z'.
    private const string LowerIssuer = "GBVGV5HAF2I34PET27RK5KKBMTK2SH2O365ACCDMBSS45KWPGW2UB6C5";
    private const string HigherIssuer = "GB5Z5P4R6AA7HM3BXWFTNPJILIF5LCMV7KABR2GWXRSBSYY2QMQL3K7W";

    /// <summary>
    ///     Verifies that credit assets with the same code order by the issuer's key bytes, not its strkey text.
    /// </summary>
    [TestMethod]
    public void CompareTo_SameCodeIssuersDifferingAtLetterVsDigit_OrdersByKeyBytes()
    {
        // Arrange
        Assert.IsTrue(string.CompareOrdinal(LowerIssuer, HigherIssuer) > 0, "precondition: text order is reversed");
        var lower4 = new AssetTypeCreditAlphaNum4("USD", LowerIssuer);
        var higher4 = new AssetTypeCreditAlphaNum4("USD", HigherIssuer);
        var lower12 = new AssetTypeCreditAlphaNum12("USDCOIN", LowerIssuer);
        var higher12 = new AssetTypeCreditAlphaNum12("USDCOIN", HigherIssuer);

        // Act & Assert
        Assert.IsTrue(lower4.CompareTo(higher4) < 0);
        Assert.IsTrue(higher4.CompareTo(lower4) > 0);
        Assert.IsTrue(lower12.CompareTo(higher12) < 0);
        Assert.IsTrue(higher12.CompareTo(lower12) > 0);
    }

    /// <summary>
    ///     Verifies that liquidity pool parameters accept the issue #257 pair in key-byte order and reject the reverse.
    /// </summary>
    [TestMethod]
    public void LiquidityPoolConstantProductParameters_IssuePair_AcceptsKeyByteOrderAndRejectsReverse()
    {
        AssertPoolOrder(new AssetTypeCreditAlphaNum4("USD", LowerIssuer),
            new AssetTypeCreditAlphaNum4("USD", HigherIssuer));
    }

    /// <summary>
    ///     Verifies the pool order for more issuer pairs whose strkey text order disagrees with their key-byte order,
    ///     generated the same way as the pair in issue #257.
    /// </summary>
    [TestMethod]
    public void LiquidityPoolConstantProductParameters_GeneratedMisorderedPairs_AcceptsKeyByteOrderAndRejectsReverse()
    {
        // Arrange
        var random = new Random(257);
        var pairs = new List<(string Lower, string Higher)>();
        while (pairs.Count < 10)
        {
            var a = RandomIssuer(random);
            var b = RandomIssuer(random);
            var byteOrder = Math.Sign(CompareBytes(StrKey.DecodeEd25519PublicKey(a), StrKey.DecodeEd25519PublicKey(b)));
            if (byteOrder != 0 && byteOrder != Math.Sign(string.CompareOrdinal(a, b)))
            {
                pairs.Add(byteOrder < 0 ? (a, b) : (b, a));
            }
        }

        // Act & Assert
        foreach (var (lower, higher) in pairs)
        {
            AssertPoolOrder(new AssetTypeCreditAlphaNum4("USD", lower), new AssetTypeCreditAlphaNum4("USD", higher));
            AssertPoolOrder(new AssetTypeCreditAlphaNum12("USDCOIN", lower),
                new AssetTypeCreditAlphaNum12("USDCOIN", higher));
        }
    }

    /// <summary>
    ///     Verifies that asset codes order like stellar-core's zero-padded code arrays: a code sorts before every
    ///     longer code it is a prefix of, and digits sort before letters.
    /// </summary>
    [TestMethod]
    public void CompareTo_CodesSharingAPrefix_OrdersLikeZeroPaddedBytes()
    {
        // Arrange
        string[] codes4 = ["A", "A1", "AB", "ABC", "ABCD", "AZ", "B", "Z", "a", "z"];
        string[] codes12 = ["ABCDE", "ABCDE1", "ABCDEF", "ABCDEFGHIJKL", "ABCDZ", "abcde"];

        // Act & Assert
        foreach (var x in codes4)
        {
            foreach (var y in codes4)
            {
                AssertAgreesWithXdr(new AssetTypeCreditAlphaNum4(x, LowerIssuer),
                    new AssetTypeCreditAlphaNum4(y, HigherIssuer));
            }
        }

        foreach (var x in codes12)
        {
            foreach (var y in codes12)
            {
                AssertAgreesWithXdr(new AssetTypeCreditAlphaNum12(x, LowerIssuer),
                    new AssetTypeCreditAlphaNum12(y, HigherIssuer));
            }
        }
    }

    /// <summary>
    ///     Verifies that <see cref="AssetTypeCreditAlphaNum4.CompareTo" /> agrees with a raw-byte comparison of the XDR
    ///     assets over random pairs.
    /// </summary>
    [TestMethod]
    public void CompareTo_RandomCreditAlphaNum4Pairs_AgreesWithXdrByteOrder()
    {
        var random = new Random(4);
        var issuers = Enumerable.Range(0, 32).Select(_ => RandomIssuer(random)).ToArray();
        for (var i = 0; i < 5000; i++)
        {
            AssertAgreesWithXdr(RandomCreditAlphaNum4(random, issuers), RandomCreditAlphaNum4(random, issuers));
        }
    }

    /// <summary>
    ///     Verifies that <see cref="AssetTypeCreditAlphaNum12.CompareTo" /> agrees with a raw-byte comparison of the
    ///     XDR assets over random pairs.
    /// </summary>
    [TestMethod]
    public void CompareTo_RandomCreditAlphaNum12Pairs_AgreesWithXdrByteOrder()
    {
        var random = new Random(12);
        var issuers = Enumerable.Range(0, 32).Select(_ => RandomIssuer(random)).ToArray();
        for (var i = 0; i < 5000; i++)
        {
            AssertAgreesWithXdr(RandomCreditAlphaNum12(random, issuers), RandomCreditAlphaNum12(random, issuers));
        }
    }

    /// <summary>
    ///     Verifies that <see cref="Asset.CompareTo" /> agrees with a raw-byte comparison of the XDR assets over random
    ///     pairs that mix native, alphanum4 and alphanum12 assets.
    /// </summary>
    [TestMethod]
    public void CompareTo_RandomMixedPairs_AgreesWithXdrByteOrder()
    {
        var random = new Random(30);
        var issuers = Enumerable.Range(0, 32).Select(_ => RandomIssuer(random)).ToArray();
        for (var i = 0; i < 5000; i++)
        {
            AssertAgreesWithXdr(RandomAsset(random, issuers), RandomAsset(random, issuers));
        }
    }

    private static void AssertPoolOrder(Asset lower, Asset higher)
    {
        var parameters = new LiquidityPoolConstantProductParameters(lower, higher, LiquidityPoolParameters.Fee);
        Assert.AreEqual(lower, parameters.AssetA);
        Assert.AreEqual(higher, parameters.AssetB);
        _ = new LiquidityPoolId(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, lower,
            higher, LiquidityPoolParameters.Fee);

        Assert.ThrowsException<ArgumentException>(() =>
            new LiquidityPoolConstantProductParameters(higher, lower, LiquidityPoolParameters.Fee));
        Assert.ThrowsException<ArgumentException>(() =>
            new LiquidityPoolId(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, higher,
                lower, LiquidityPoolParameters.Fee));
    }

    private static void AssertAgreesWithXdr(Asset x, Asset y)
    {
        var expected = Math.Sign(CompareBytes(EncodeXdr(x), EncodeXdr(y)));
        Assert.AreEqual(expected, Math.Sign(x.CompareTo(y)),
            $"{x.CanonicalName()} vs {y.CanonicalName()}");
        Assert.AreEqual(-expected, Math.Sign(y.CompareTo(x)),
            $"{y.CanonicalName()} vs {x.CanonicalName()}");
    }

    private static byte[] EncodeXdr(Asset asset)
    {
        var stream = new XDR.XdrDataOutputStream();
        XDR.Asset.Encode(stream, asset.ToXdr());
        return stream.ToArray();
    }

    private static int CompareBytes(byte[] x, byte[] y)
    {
        return x.AsSpan().SequenceCompareTo(y);
    }

    private static string RandomIssuer(Random random)
    {
        var key = new byte[32];
        random.NextBytes(key);
        return KeyPair.FromPublicKey(key).AccountId;
    }

    private static string RandomCode(Random random, int minLength, int maxLength)
    {
        // A small alphabet with digits, upper and lower case, so that equal codes and shared prefixes are common.
        const string alphabet = "019AZaz";
        var length = random.Next(minLength, maxLength + 1);
        return new string(Enumerable.Range(0, length).Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());
    }

    private static AssetTypeCreditAlphaNum4 RandomCreditAlphaNum4(Random random, string[] issuers)
    {
        return new AssetTypeCreditAlphaNum4(RandomCode(random, 1, 2), issuers[random.Next(issuers.Length)]);
    }

    private static AssetTypeCreditAlphaNum12 RandomCreditAlphaNum12(Random random, string[] issuers)
    {
        return new AssetTypeCreditAlphaNum12("ASSE" + RandomCode(random, 1, 2), issuers[random.Next(issuers.Length)]);
    }

    private static Asset RandomAsset(Random random, string[] issuers)
    {
        return random.Next(3) switch
        {
            0 => new AssetTypeNative(),
            1 => RandomCreditAlphaNum4(random, issuers),
            _ => RandomCreditAlphaNum12(random, issuers),
        };
    }
}