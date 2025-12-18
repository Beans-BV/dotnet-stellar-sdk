using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for <see cref="StrKey" /> class.
/// </summary>
[TestClass]
public class StrKeyTest
{
    /// <summary>
    ///     Verifies that DecodeEd25519SecretSeed and EncodeEd25519SecretSeed round-trip correctly.
    /// </summary>
    [TestMethod]
    [DataRow("SCCSHY6F77I3ZQSYFB34ILSUBDOUACEUBXTV5OJ6JW5OGQPDGAYRRF5L")]
    [DataRow("SCKQC5Z32U6TCQI6PM2W6GNEBEW3HLDRGZNARLDRJKKZBWCKJCQZ3MTN")]
    [DataRow("SARAUE3VC5ALDYEPW3J6EMIHKKLAOQNNPT54Q5V3XJ3JQLQ4YUE3XPWR")]
    public void DecodeEncodeSeed_WithValidSeed_RoundTripsCorrectly(string seed)
    {
        // Arrange
        // Seed provided via DataRow attribute

        // Act
        var decoded = StrKey.DecodeEd25519SecretSeed(seed);
        var encoded = StrKey.EncodeEd25519SecretSeed(decoded);

        // Assert
        Assert.AreEqual(seed, encoded);
    }

    /// <summary>
    ///     Verifies that DecodeEd25519PublicKey and EncodeEd25519PublicKey round-trip correctly.
    /// </summary>
    [TestMethod]
    [DataRow("GC5ZDZL56FJT75QOWB4OH7KBPBFBBYIJCDNTUNTB3QRFV4Z2IVUE6ESM")]
    [DataRow("GDWIZBODB4V7Q4V5TS4OMPUMDDTDNXMFQOSLVE6JG7PL4MHPTG6XO43Q")]
    [DataRow("GC3OPR6IAOHC3UEJA7VDCVXFBHEUAE4R5UMESSXNEAGLLLTIWO7HEWDG")]
    public void DecodeEncodeAccountId_WithValidAccountId_RoundTripsCorrectly(string id)
    {
        // Arrange
        // Account ID provided via DataRow attribute

        // Act
        var decoded = StrKey.DecodeEd25519PublicKey(id);
        var encoded = StrKey.EncodeEd25519PublicKey(decoded);

        // Assert
        Assert.AreEqual(id, encoded);
    }

    /// <summary>
    ///     Verifies that DecodeContractId and EncodeContractId round-trip correctly.
    /// </summary>
    [TestMethod]
    [DataRow("CCADCR3WTIFPZM6TV33WE7FO5JB2DZMWQ5IPRATYMXPYQSMCKWSAEMJD")]
    [DataRow("CBF6WC7IY3IHXHXF224AGFRCNDRJ6HEFVJTVM34BK2JQSJSOJIFFAVSP")]
    [DataRow("CBFZDBUL3OC2VRMCFKTV4G6H5R5FNRLDVCIE536CRWWF4RKXWGTSSLDI")]
    public void DecodeEncodeContractId_WithValidContractId_RoundTripsCorrectly(string id)
    {
        // Arrange
        // Contract ID provided via DataRow attribute

        // Act
        var decoded = StrKey.DecodeContractId(id);
        var encoded = StrKey.EncodeContractId(decoded);

        // Assert
        Assert.AreEqual(id, encoded);
    }

    /// <summary>
    ///     Verifies that DecodeMed25519PublicKey and EncodeMed25519PublicKey round-trip correctly.
    /// </summary>
    [TestMethod]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ")]
    public void DecodeEncodeMuxedEd25519PublicKey_WithValidKey_RoundTripsCorrectly(string id)
    {
        // Arrange
        // Muxed account ID provided via DataRow attribute

        // Act
        var decoded = StrKey.DecodeMed25519PublicKey(id);
        var encoded = StrKey.EncodeMed25519PublicKey(decoded);

        // Assert
        Assert.AreEqual(id, encoded);
    }

    /// <summary>
    ///     Verifies that DecodeCheck throws ArgumentException when version byte does not match the address.
    /// </summary>
    [TestMethod]
    public void DecodeCheck_WithInvalidVersionByte_ThrowsArgumentException()
    {
        // Arrange
        const string address = "GCZHXL5HXQX5ABDM26LHYRCQZ5OJFHLOPLZX47WEBP3V2PF5AVFK2A5D";

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => StrKey.DecodeCheck(StrKey.VersionByte.SEED, address));
        Assert.ThrowsException<ArgumentException>(() =>
            StrKey.DecodeCheck(StrKey.VersionByte.CLAIMABLE_BALANCE, address));
        Assert.ThrowsException<ArgumentException>(() => StrKey.DecodeCheck(StrKey.VersionByte.LIQUIDITY_POOL, address));
        Assert.ThrowsException<ArgumentException>(() => StrKey.DecodeCheck(StrKey.VersionByte.CONTRACT, address));
        Assert.ThrowsException<ArgumentException>(() => StrKey.DecodeCheck(StrKey.VersionByte.SIGNED_PAYLOAD, address));
    }

    /// <summary>
    ///     Verifies that DecodeEd25519SecretSeed throws ArgumentException when seed is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("SAA6NXOBOXP3RXGAXBW6PGFI5BPK4ODVAWITS4VDOMN5C2M4B66ZML")]
    [DataRow("GCZHXL5HXQX5ABDM26LHYRCQZ5OJFHLOPLZX47WEBP3V2PF5AVFK2A5D")]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ")]
    [ExpectedException(typeof(ArgumentException))]
    public void DecodeEd25519SecretSeed_WithInvalidSeed_ThrowsArgumentException(string seed)
    {
        // Act & Assert
        StrKey.DecodeEd25519SecretSeed(seed);
    }

    /// <summary>
    ///     Verifies that DecodeStellarMuxedAccount and EncodeStellarMuxedAccount round-trip correctly.
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void DecodeEncodeMuxedAccount_WithValidMuxedAccount_RoundTripsCorrectly()
    {
        // Arrange
        const string address = "MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ";

        // Act
        var muxed = StrKey.DecodeStellarMuxedAccount(address);

        // Assert
        Assert.IsTrue(StrKey.IsValidMuxedAccount(address));
        Assert.AreEqual(0UL, muxed.Med25519.Id.InnerValue);

        var encodedKey = StrKey.EncodeEd25519PublicKey(muxed.Med25519.Ed25519.InnerValue);
        Assert.AreEqual("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ", encodedKey);
        Assert.AreEqual(address,
            StrKey.EncodeStellarMuxedAccount(
                new MuxedAccountMed25519(KeyPair.FromPublicKey(muxed.Med25519.Ed25519.InnerValue),
                    muxed.Med25519.Id.InnerValue).MuxedAccount));
    }

    /// <summary>
    ///     Verifies that DecodeStellarMuxedAccount and EncodeStellarMuxedAccount round-trip correctly with large ID.
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void DecodeEncodeMuxedAccount_WithLargeId_RoundTripsCorrectly()
    {
        // Arrange
        const string address = "MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVAAAAAAAAAAAAAJLK";

        // Act
        var muxed = StrKey.DecodeStellarMuxedAccount(address);

        // Assert
        Assert.IsTrue(StrKey.IsValidMuxedAccount(address));
        Assert.AreEqual(9223372036854775808UL, muxed.Med25519.Id.InnerValue);
        var encodedKey = StrKey.EncodeEd25519PublicKey(muxed.Med25519.Ed25519.InnerValue);
        Assert.AreEqual("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ", encodedKey);
        Assert.AreEqual(address,
            StrKey.EncodeStellarMuxedAccount(
                new MuxedAccountMed25519(KeyPair.FromPublicKey(muxed.Med25519.Ed25519.InnerValue),
                    muxed.Med25519.Id.InnerValue).MuxedAccount));
    }


    /// <summary>
    ///     Verifies that EncodeLiquidityPoolId and DecodeLiquidityPoolId round-trip correctly.
    /// </summary>
    [TestMethod]
    [DataRow("0000a8198b5e25994c1ca5b0556faeb27325ac746296944144e0a7406d501e8a")]
    [DataRow("01101dbf3abe4b77e80f8aaec9d945eeaef8ca0b992aa613644ee9802b60340e")]
    [DataRow("00065ad985014e7378e380fd69ba48d7a3109dbd6e37f4b2aa1162adfb48f4cd")]
    public void EncodeDecodeLiquidityPool_WithValidId_RoundTripsCorrectly(string id)
    {
        // Arrange
        var rawData = Convert.FromHexString(id);

        // Act
        var base32Encoded = StrKey.EncodeLiquidityPoolId(rawData);
        var decoded = StrKey.DecodeLiquidityPoolId(base32Encoded);

        // Assert
        CollectionAssert.AreEqual(rawData, decoded);
    }

    /// <summary>
    ///     Verifies that DecodePreAuthTx and EncodePreAuthTx round-trip correctly.
    /// </summary>
    [TestMethod]
    [DataRow("TB65MHFA2Z342DX4FNKHH2KCNR5JRM7GIVTWQLKG5Z6L3AAH4UZLZM5K")]
    public void DecodeEncodePreAuthTx_WithValidId_RoundTripsCorrectly(string id)
    {
        // Arrange
        // Pre-auth transaction ID provided via DataRow attribute

        // Act
        var decoded = StrKey.DecodePreAuthTx(id);
        var encoded = StrKey.EncodePreAuthTx(decoded);

        // Assert
        Assert.AreEqual(id, encoded);
    }

    /// <summary>
    ///     Verifies that DecodeSha256Hash and EncodeSha256Hash round-trip correctly.
    /// </summary>
    [TestMethod]
    [DataRow("XB65MHFA2Z342DX4FNKHH2KCNR5JRM7GIVTWQLKG5Z6L3AAH4UZLYIYT")]
    public void DecodeEncodeSha256Hash_WithValidHash_RoundTripsCorrectly(string id)
    {
        // Arrange
        // SHA256 hash provided via DataRow attribute

        // Act
        var decoded = StrKey.DecodeSha256Hash(id);
        var encoded = StrKey.EncodeSha256Hash(decoded);

        // Assert
        Assert.AreEqual(id, encoded);
    }

    /// <summary>
    ///     Verifies that DecodeSignedPayload and EncodeSignedPayload round-trip correctly.
    /// </summary>
    [TestMethod]
    [DataRow("PBJCHUKZMTFSLOMNC7P4TS4VJJBTCYL3XKSOLXAUJSD56C4LHND5SAAAAAE5L4J5XWKV5GPZBUAAAAAYQ4")]
    public void DecodeEncodeSignedPayload_WithValidPayload_RoundTripsCorrectly(string id)
    {
        // Arrange
        // Signed payload provided via DataRow attribute

        // Act
        var decoded = StrKey.DecodeSignedPayload(id);
        var encoded = StrKey.EncodeSignedPayload(decoded);

        // Assert
        Assert.AreEqual(id, encoded);
    }

    /// <summary>
    ///     Verifies that EncodeClaimableBalanceId and DecodeClaimableBalanceId round-trip correctly.
    /// </summary>
    [TestMethod]
    [DataRow("003F0C34BF93AD0D9971D04CCC90F705511C838AAD9734A4A2FB0D7A03FC7FE89A")]
    [DataRow("00a26599c23752c92c3e74f60b72081b0b3f4b1a8353f357848f69c82e3fe6373c")]
    [DataRow("00f6d640e0264946d35889231ce65dd63699e90dbcedea642b2d98ead157ee8758")]
    public void EncodeDecodeClaimableBalance_WithValidId_RoundTripsCorrectly(string id)
    {
        // Arrange
        var rawData = Convert.FromHexString(id);

        // Act
        var base32 = StrKey.EncodeClaimableBalanceId(rawData);
        var bytes = StrKey.DecodeClaimableBalanceId(base32);

        // Assert
        CollectionAssert.AreEqual(rawData, bytes);
    }


    /// <summary>
    ///     Verifies that DecodeClaimableBalanceId and EncodeClaimableBalanceId round-trip correctly.
    /// </summary>
    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4TU")]
    [DataRow("BAAKEZMZYI3VFSJMHZ2PMC3SBANQWP2LDKBVH42XQSHWTSBOH7TDOPHPFA")]
    [DataRow("BAAPNVSA4ATESRWTLCESGHHGLXLDNGPJBW6O32TEFMWZR2WRK7XIOWBOTI")]
    public void DecodeEncodeClaimableBalance_WithValidId_RoundTripsCorrectly(string id)
    {
        // Arrange
        // Claimable balance ID provided via DataRow attribute

        // Act
        var decoded = StrKey.DecodeClaimableBalanceId(id);
        var encoded = StrKey.EncodeClaimableBalanceId(decoded);

        // Assert
        Assert.AreEqual(id, encoded);
    }

    /// <summary>
    ///     Verifies that IsValidLiquidityPoolId returns false for invalid liquidity pool IDs.
    /// </summary>
    [TestMethod]
    [DataRow("0fc9dec967732e70d07d1006eca5389d32121c197cad633b60c227e5cde8b861")]
    [DataRow("09df585742032d9537fd013f77df3cdb3c2d600b3350d51d040c2ed1478130cc")]
    [DataRow("LAARAHN7HK7EW57IB6FK5SOZIXXK56GKBOMSVJQTMRHOTABLMA2A5VVV")]
    [DataRow("MAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSAAAAAAAAAAE2LP26")]
    public void IsValidLiquidityPoolId_WithInvalidId_ReturnsFalse(string id)
    {
        // Arrange
        // Invalid liquidity pool ID provided via DataRow attribute

        // Act & Assert
        Assert.IsFalse(StrKey.IsValidLiquidityPoolId(id));
    }

    /// <summary>
    ///     Verifies that IsValidLiquidityPoolId returns true for valid liquidity pool IDs.
    /// </summary>
    [TestMethod]
    [DataRow("LAARAHN7HK7EW57IB6FK5SOZIXXK56GKBOMSVJQTMRHOTABLMA2A5VHB")]
    [DataRow("LAAABKAZRNPCLGKMDSS3AVLPV2ZHGJNMORRJNFCBITQKOQDNKAPIUSGG")]
    [DataRow("LAAAMWWZQUAU443Y4OAP22N2JDL2GEE5XVXDP5FSVIIWFLP3JD2M2YCG")]
    public void IsValidLiquidityPoolId_WithValidId_ReturnsTrue(string id)
    {
        // Arrange
        // Valid liquidity pool ID provided via DataRow attribute

        // Act & Assert
        Assert.IsTrue(StrKey.IsValidLiquidityPoolId(id));
    }

    /// <summary>
    ///     Verifies that IsValidClaimableBalanceId returns false for invalid claimable balance IDs.
    /// </summary>
    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4T")]
    [DataRow("LAARAHN7HK7EW57IB6FK5SOZIXXK56GKBOMSVJQTMRHOTABLMA2A5VVV")]
    [DataRow("MAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSAAAAAAAAAAE2LP26")]
    [DataRow("a26599c23752c92c3e74f60b72081b0b3f4b1a8353f357848f69c82e3fe6373c")]
    [DataRow("00000000a26599c23752c92c3e74f60b72081b0b3f4b1a8353f357848f69c82e3fe6373c")]
    public void IsValidClaimableBalanceId_WithInvalidId_ReturnsFalse(string id)
    {
        // Arrange
        // Invalid claimable balance ID provided via DataRow attribute

        // Act & Assert
        Assert.IsFalse(StrKey.IsValidClaimableBalanceId(id));
    }

    /// <summary>
    ///     Verifies that IsValidClaimableBalanceId returns true for valid claimable balance IDs.
    /// </summary>
    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4TU")]
    [DataRow("BAAKEZMZYI3VFSJMHZ2PMC3SBANQWP2LDKBVH42XQSHWTSBOH7TDOPHPFA")]
    public void IsValidClaimableBalanceId_WithValidId_ReturnsTrue(string id)
    {
        // Arrange
        // Valid claimable balance ID provided via DataRow attribute

        // Act & Assert
        Assert.IsTrue(StrKey.IsValidClaimableBalanceId(id));
    }

    /// <summary>
    ///     Verifies that IsValidEd25519PublicKey returns true for valid Ed25519 public keys.
    /// </summary>
    [TestMethod]
    [DataRow("GCZHXL5HXQX5ABDM26LHYRCQZ5OJFHLOPLZX47WEBP3V2PF5AVFK2A5D")]
    [DataRow("GDWIZBODB4V7Q4V5TS4OMPUMDDTDNXMFQOSLVE6JG7PL4MHPTG6XO43Q")]
    [DataRow("GC3OPR6IAOHC3UEJA7VDCVXFBHEUAE4R5UMESSXNEAGLLLTIWO7HEWDG")]
    public void IsValidEd25519PublicKey_WithValidKey_ReturnsTrue(string id)
    {
        // Arrange
        // Valid Ed25519 public key provided via DataRow attribute

        // Act & Assert
        Assert.IsTrue(StrKey.IsValidEd25519PublicKey(id));
    }

    /// <summary>
    ///     Verifies that IsValidEd25519PublicKey returns false for invalid Ed25519 public keys.
    /// </summary>
    [TestMethod]
    [DataRow("SAA6NXOBOXP3RXGAXBW6PGFI5BPK4ODVAWITS4VDOMN5C2M4B66ZML", DisplayName = "Secret Key")]
    [DataRow("GAAAAAAAACGC6", DisplayName = "Invalid length (Ed25519 should be 32 bytes, not 5)")]
    [DataRow("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUACUSI",
        DisplayName = "Invalid length (base-32 decoding should yield 35 bytes, not 36)")]
    [DataRow("G47QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVP2I",
        DisplayName = "Invalid algorithm (low 3 bits of version byte are 7)")]
    [DataRow("")]
    public void IsValidEd25519PublicKey_WithInvalidKey_ReturnsFalse(string id)
    {
        // Arrange
        // Invalid Ed25519 public key provided via DataRow attribute

        // Act & Assert
        Assert.IsFalse(StrKey.IsValidEd25519PublicKey(id));
    }

    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4T")]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJU")]
    [DataRow("GDWIZBODB4V7Q4V5TS4OMPUMDDTDNXMFQOSLVE6JG7PL4MHPTG6XO43Q")]
    public void IsValidMed25519PublicKey_WithInvalidKey_ReturnsFalse(string id)
    {
        // Arrange
        // Invalid muxed Ed25519 public key provided via DataRow attribute

        // Act & Assert
        Assert.IsFalse(StrKey.IsValidMed25519PublicKey(id));
    }

    /// <summary>
    ///     Verifies that IsValidMed25519PublicKey returns true for valid muxed Ed25519 public keys.
    /// </summary>
    [TestMethod]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ")]
    public void IsValidMed25519PublicKey_WithValidKey_ReturnsTrue(string id)
    {
        // Arrange
        // Valid muxed Ed25519 public key provided via DataRow attribute

        // Act & Assert
        Assert.IsTrue(StrKey.IsValidMed25519PublicKey(id));
    }


    /// <summary>
    ///     Verifies that IsValidContractId returns false for invalid contract IDs.
    /// </summary>
    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4T")]
    [DataRow("LAARAHN7HK7EW57IB6FK5SOZIXXK56GKBOMSVJQTMRHOTABLMA2A5VVV")]
    [DataRow("MAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSAAAAAAAAAAE2LP26")]
    [DataRow("a26599c23752c92c3e74f60b72081b0b3f4b1a8353f357848f69c82e3fe6373c")]
    [DataRow("CBFZDBUL3OC2VRMCFKTV4G6H5R5FNRLDVCIE536CRWWF4RKXWGTSSLD")]
    public void IsValidContractId_WithInvalidId_ReturnsFalse(string id)
    {
        // Arrange
        // Invalid contract ID provided via DataRow attribute

        // Act & Assert
        Assert.IsFalse(StrKey.IsValidContractId(id));
    }

    /// <summary>
    ///     Verifies that IsValidContractId returns true for valid contract IDs.
    /// </summary>
    [TestMethod]
    [DataRow("CCVEAWF4737OANGCBTM6ARENVOTKJQILJH4ZUJBANGZBPIUMNGYFGAEB")]
    [DataRow("CBFZDBUL3OC2VRMCFKTV4G6H5R5FNRLDVCIE536CRWWF4RKXWGTSSLDI")]
    [DataRow("CCXWM6ZYPYM6272RW3P3USK6XSK4DCUOQQVJ6WJLDABPR6ZDKGNKNLPT")]
    public void IsValidContractId_WithValidId_ReturnsTrue(string id)
    {
        // Arrange
        // Valid contract ID provided via DataRow attribute

        // Act & Assert
        Assert.IsTrue(StrKey.IsValidContractId(id));
    }

    /// <summary>
    ///     Verifies that IsValidMuxedAccount returns false for invalid muxed account IDs.
    /// </summary>
    [TestMethod]
    [DataRow("MAAAAAAAAAAAAAB7BQ2L7E5NBWMXDUCMZSIPOBKRDSBYVLMXGSSKF6YNPIB7Y77ITIADJPA",
        DisplayName = "Invalid length (base-32 decoding should yield 43 bytes, not 44)")]
    [DataRow("M4AAAAAAAAAAAAB7BQ2L7E5NBWMXDUCMZSIPOBKRDSBYVLMXGSSKF6YNPIB7Y77ITIU2K",
        DisplayName = "Invalid algorithm (low 3 bits of version byte are 7)")]
    [DataRow("MAAAAAAAAAAAAAB7BQ2L7E5NBWMXDUCMZSIPOBKRDSBYVLMXGSSKF6YNPIB7Y77ITLVL4", DisplayName = "Invalid checksum")]
    public void IsValidMuxedAccount_WithInvalidId_ReturnsFalse(string id)
    {
        // Arrange
        // Invalid muxed account ID provided via DataRow attribute

        // Act & Assert
        Assert.IsFalse(StrKey.IsValidMuxedAccount(id));
    }

    /// <summary>
    ///     Verifies that IsValidEd25519SecretSeed returns true for valid Ed25519 secret seeds.
    /// </summary>
    [TestMethod]
    [DataRow("SDJHRQF4GCMIIKAAAQ6IHY42X73FQFLHUULAPSKKD4DFDM7UXWWCRHBE")]
    [DataRow("SCKQC5Z32U6TCQI6PM2W6GNEBEW3HLDRGZNARLDRJKKZBWCKJCQZ3MTN")]
    [DataRow("SARAUE3VC5ALDYEPW3J6EMIHKKLAOQNNPT54Q5V3XJ3JQLQ4YUE3XPWR")]
    public void IsValidEd25519SecretSeed_WithValidSeed_ReturnsTrue(string seed)
    {
        // Arrange
        // Valid Ed25519 secret seed provided via DataRow attribute

        // Act & Assert
        Assert.IsTrue(StrKey.IsValidEd25519SecretSeed(seed));
    }

    /// <summary>
    ///     Verifies that IsValidEd25519SecretSeed returns false for invalid Ed25519 secret seeds.
    /// </summary>
    [TestMethod]
    [DataRow("GCZHXL5HXQX5ABDM26LHYRCQZ5OJFHLOPLZX47WEBP3V2PF5AVFK2A5D")]
    [DataRow("BAAKEZMZYI3VFSJMHZ2PMC3SBANQWP2LDKBVH42XQSHWTSBOH7TDOPHPFA")]
    public void IsValidEd25519SecretSeed_WithInvalidSeed_ReturnsFalse(string seed)
    {
        // Arrange
        // Invalid Ed25519 secret seed provided via DataRow attribute

        // Act & Assert
        Assert.IsFalse(StrKey.IsValidEd25519SecretSeed(seed));
    }

    /// <summary>
    ///     Verifies that EncodeSignedPayload correctly encodes signed payload signers with various payload sizes.
    /// </summary>
    [TestMethod]
    public void EncodeSignedPayload_WithValidSignedPayloadSigner_ReturnsCorrectEncodedValue()
    {
        // Arrange
        var payload = Util.HexToBytes("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");
        var signedPayloadSigner =
            new SignedPayloadSigner(
                StrKey.DecodeEd25519PublicKey("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ"), payload);

        // Act
        var encoded = StrKey.EncodeSignedPayload(signedPayloadSigner);

        // Assert
        Assert.AreEqual(encoded,
            "PA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAQACAQDAQCQMBYIBEFAWDANBYHRAEISCMKBKFQXDAMRUGY4DUPB6IBZGM");

        // Valid signed payload with an ed25519 public key and a 29-byte payload.
        payload = Util.HexToBytes("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d");
        signedPayloadSigner =
            new SignedPayloadSigner(
                StrKey.DecodeEd25519PublicKey("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ"), payload);
        encoded = StrKey.EncodeSignedPayload(signedPayloadSigner);
        Assert.AreEqual(encoded,
            "PA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAOQCAQDAQCQMBYIBEFAWDANBYHRAEISCMKBKFQXDAMRUGY4DUAAAAFGBU");
    }

    /// <summary>
    ///     Verifies that EncodeCheck and DecodeCheck round-trip correctly for SIGNED_PAYLOAD version byte.
    /// </summary>
    [TestMethod]
    public void EncodeDecodeCheck_WithSignedPayloadVersionByte_RoundTripsCorrectly()
    {
        // Arrange
        var data = new List<byte>
        {
            //ED25519
            0x36, 0x3e, 0xaa, 0x38, 0x67, 0x84, 0x1f, 0xba,
            0xd0, 0xf4, 0xed, 0x88, 0xc7, 0x79, 0xe4, 0xfe,
            0x66, 0xe5, 0x6a, 0x24, 0x70, 0xdc, 0x98, 0xc0,
            0xec, 0x9c, 0x07, 0x3d, 0x05, 0xc7, 0xb1, 0x03,

            //Payload length
            0x00, 0x00, 0x00, 0x09,

            //Payload
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00,

            //Padding
            0x00, 0x00, 0x00,
        }.ToArray();

        const string hashX = "PA3D5KRYM6CB7OWQ6TWYRR3Z4T7GNZLKERYNZGGA5SOAOPIFY6YQGAAAAAEQAAAAAAAAAAAAAAAAAABBXA";

        // Act
        var encoded = StrKey.EncodeCheck(StrKey.VersionByte.SIGNED_PAYLOAD, data);
        var decoded = StrKey.DecodeCheck(StrKey.VersionByte.SIGNED_PAYLOAD, hashX);

        // Assert
        Assert.AreEqual(hashX, encoded);
        Assert.IsTrue(data.SequenceEqual(decoded));
    }
}