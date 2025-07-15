﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class StrKeyTest
{
    [TestMethod]
    [DataRow("SCCSHY6F77I3ZQSYFB34ILSUBDOUACEUBXTV5OJ6JW5OGQPDGAYRRF5L")]
    [DataRow("SCKQC5Z32U6TCQI6PM2W6GNEBEW3HLDRGZNARLDRJKKZBWCKJCQZ3MTN")]
    [DataRow("SARAUE3VC5ALDYEPW3J6EMIHKKLAOQNNPT54Q5V3XJ3JQLQ4YUE3XPWR")]
    public void TestDecodeEncodeSeed(string seed)
    {
        var decoded = StrKey.DecodeStellarSecretSeed(seed);
        var encoded = StrKey.EncodeStellarSecretSeed(decoded);

        Assert.AreEqual(seed, encoded);
    }

    [TestMethod]
    [DataRow("GC5ZDZL56FJT75QOWB4OH7KBPBFBBYIJCDNTUNTB3QRFV4Z2IVUE6ESM")]
    [DataRow("GDWIZBODB4V7Q4V5TS4OMPUMDDTDNXMFQOSLVE6JG7PL4MHPTG6XO43Q")]
    [DataRow("GC3OPR6IAOHC3UEJA7VDCVXFBHEUAE4R5UMESSXNEAGLLLTIWO7HEWDG")]
    public void TestDecodeEncodeAccountId(string id)
    {
        var decoded = StrKey.DecodeStellarAccountId(id);
        var encoded = StrKey.EncodeStellarAccountId(decoded);

        Assert.AreEqual(id, encoded);
    }

    [TestMethod]
    [DataRow("CCADCR3WTIFPZM6TV33WE7FO5JB2DZMWQ5IPRATYMXPYQSMCKWSAEMJD")]
    [DataRow("CBF6WC7IY3IHXHXF224AGFRCNDRJ6HEFVJTVM34BK2JQSJSOJIFFAVSP")]
    [DataRow("CBFZDBUL3OC2VRMCFKTV4G6H5R5FNRLDVCIE536CRWWF4RKXWGTSSLDI")]
    public void TestDecodeEncodeContractId(string id)
    {
        var decoded = StrKey.DecodeContractId(id);
        var encoded = StrKey.EncodeContractId(decoded);

        Assert.AreEqual(id, encoded);
    }

    [TestMethod]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ")]
    public void TestDecodeEncodeMuxedEd25119PublicKey(string id)
    {
        var decoded = StrKey.DecodeMuxedEd25519PublicKey(id);
        var encoded = StrKey.EncodeMuxedEd25519PublicKey(decoded);

        Assert.AreEqual(id, encoded);
    }

    [TestMethod]
    public void TestDecodeInvalidVersionByte()
    {
        const string address = "GCZHXL5HXQX5ABDM26LHYRCQZ5OJFHLOPLZX47WEBP3V2PF5AVFK2A5D";
        Assert.ThrowsException<ArgumentException>(() => StrKey.DecodeCheck(StrKey.VersionByte.SEED, address));
        Assert.ThrowsException<ArgumentException>(() =>
            StrKey.DecodeCheck(StrKey.VersionByte.CLAIMABLE_BALANCE, address));
        Assert.ThrowsException<ArgumentException>(() => StrKey.DecodeCheck(StrKey.VersionByte.LIQUIDITY_POOL, address));
        Assert.ThrowsException<ArgumentException>(() => StrKey.DecodeCheck(StrKey.VersionByte.CONTRACT, address));
        Assert.ThrowsException<ArgumentException>(() => StrKey.DecodeCheck(StrKey.VersionByte.SIGNED_PAYLOAD, address));
    }

    [TestMethod]
    [DataRow("SAA6NXOBOXP3RXGAXBW6PGFI5BPK4ODVAWITS4VDOMN5C2M4B66ZML")]
    [DataRow("GCZHXL5HXQX5ABDM26LHYRCQZ5OJFHLOPLZX47WEBP3V2PF5AVFK2A5D")]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ")]
    [ExpectedException(typeof(ArgumentException))]
    public void TestDecodeInvalidSeed(string seed)
    {
        StrKey.DecodeStellarSecretSeed(seed);
    }

    [TestMethod]
    [Obsolete]
    public void TestDecodeEncodeMuxedAccount()
    {
        const string address = "MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ";
        var muxed = StrKey.DecodeStellarMuxedAccount(address);
        Assert.IsTrue(StrKey.IsValidMuxedAccount(address));
        Assert.AreEqual(0UL, muxed.Med25519.Id.InnerValue);

        var encodedKey = StrKey.EncodeStellarAccountId(muxed.Med25519.Ed25519.InnerValue);
        Assert.AreEqual("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ", encodedKey);
        Assert.AreEqual(address,
            StrKey.EncodeStellarMuxedAccount(
                new MuxedAccount(KeyPair.FromPublicKey(muxed.Med25519.Ed25519.InnerValue),
                    muxed.Med25519.Id.InnerValue).ToXdrMuxedAccount()));
    }

    [TestMethod]
    [Obsolete]
    public void TestDecodeEncodeMuxedAccountWithLargeId()
    {
        const string address = "MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVAAAAAAAAAAAAAJLK";
        var muxed = StrKey.DecodeStellarMuxedAccount(address);
        Assert.IsTrue(StrKey.IsValidMuxedAccount(address));
        Assert.AreEqual(9223372036854775808UL, muxed.Med25519.Id.InnerValue);
        var encodedKey = StrKey.EncodeStellarAccountId(muxed.Med25519.Ed25519.InnerValue);
        Assert.AreEqual("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ", encodedKey);
        Assert.AreEqual(address,
            StrKey.EncodeStellarMuxedAccount(
                new MuxedAccount(KeyPair.FromPublicKey(muxed.Med25519.Ed25519.InnerValue),
                    muxed.Med25519.Id.InnerValue).ToXdrMuxedAccount()));
    }


    [TestMethod]
    [DataRow("0000a8198b5e25994c1ca5b0556faeb27325ac746296944144e0a7406d501e8a")]
    [DataRow("01101dbf3abe4b77e80f8aaec9d945eeaef8ca0b992aa613644ee9802b60340e")]
    [DataRow("00065ad985014e7378e380fd69ba48d7a3109dbd6e37f4b2aa1162adfb48f4cd")]
    public void TestEncodeDecodeLiquidityPool(string id)
    {
        var rawData = Convert.FromHexString(id);
        var base32Encoded = StrKey.EncodeLiquidityPool(rawData);
        var decoded = StrKey.DecodeLiquidityPool(base32Encoded);

        CollectionAssert.AreEqual(rawData, decoded);
    }

    [TestMethod]
    [DataRow("003F0C34BF93AD0D9971D04CCC90F705511C838AAD9734A4A2FB0D7A03FC7FE89A")]
    [DataRow("00a26599c23752c92c3e74f60b72081b0b3f4b1a8353f357848f69c82e3fe6373c")]
    [DataRow("00f6d640e0264946d35889231ce65dd63699e90dbcedea642b2d98ead157ee8758")]
    public void TestEncodeDecodeClaimableBalance(string id)
    {
        var rawData = Convert.FromHexString(id);
        var base32 = StrKey.EncodeClaimableBalanceId(rawData);
        var bytes = StrKey.DecodeClaimableBalanceId(base32);
        CollectionAssert.AreEqual(rawData, bytes);
    }


    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4TU")]
    [DataRow("BAAKEZMZYI3VFSJMHZ2PMC3SBANQWP2LDKBVH42XQSHWTSBOH7TDOPHPFA")]
    [DataRow("BAAPNVSA4ATESRWTLCESGHHGLXLDNGPJBW6O32TEFMWZR2WRK7XIOWBOTI")]
    public void TestDecodeEncodeClaimableBalance(string id)
    {
        var decoded = StrKey.DecodeClaimableBalanceId(id);
        var encoded = StrKey.EncodeClaimableBalanceId(decoded);
        Assert.AreEqual(id, encoded);
    }

    [TestMethod]
    [DataRow("0fc9dec967732e70d07d1006eca5389d32121c197cad633b60c227e5cde8b861")]
    [DataRow("09df585742032d9537fd013f77df3cdb3c2d600b3350d51d040c2ed1478130cc")]
    [DataRow("LAARAHN7HK7EW57IB6FK5SOZIXXK56GKBOMSVJQTMRHOTABLMA2A5VVV")]
    [DataRow("MAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSAAAAAAAAAAE2LP26")]
    public void TestIsNotValidLiquidityPool(string id)
    {
        Assert.IsFalse(StrKey.IsValidLiquidityPoolId(id));
    }

    [TestMethod]
    [DataRow("LAARAHN7HK7EW57IB6FK5SOZIXXK56GKBOMSVJQTMRHOTABLMA2A5VHB")]
    [DataRow("LAAABKAZRNPCLGKMDSS3AVLPV2ZHGJNMORRJNFCBITQKOQDNKAPIUSGG")]
    [DataRow("LAAAMWWZQUAU443Y4OAP22N2JDL2GEE5XVXDP5FSVIIWFLP3JD2M2YCG")]
    public void TestIsValidLiquidityPool(string id)
    {
        Assert.IsTrue(StrKey.IsValidLiquidityPoolId(id));
    }

    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4T")]
    [DataRow("LAARAHN7HK7EW57IB6FK5SOZIXXK56GKBOMSVJQTMRHOTABLMA2A5VVV")]
    [DataRow("MAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSAAAAAAAAAAE2LP26")]
    [DataRow("a26599c23752c92c3e74f60b72081b0b3f4b1a8353f357848f69c82e3fe6373c")]
    [DataRow("00000000a26599c23752c92c3e74f60b72081b0b3f4b1a8353f357848f69c82e3fe6373c")]
    public void TestIsNotValidClaimableBalance(string id)
    {
        Assert.IsFalse(StrKey.IsValidClaimableBalanceId(id));
    }

    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4TU")]
    [DataRow("BAAKEZMZYI3VFSJMHZ2PMC3SBANQWP2LDKBVH42XQSHWTSBOH7TDOPHPFA")]
    public void TestIsValidClaimableBalance(string id)
    {
        Assert.IsTrue(StrKey.IsValidClaimableBalanceId(id));
    }

    [TestMethod]
    [DataRow("GCZHXL5HXQX5ABDM26LHYRCQZ5OJFHLOPLZX47WEBP3V2PF5AVFK2A5D")]
    [DataRow("GDWIZBODB4V7Q4V5TS4OMPUMDDTDNXMFQOSLVE6JG7PL4MHPTG6XO43Q")]
    [DataRow("GC3OPR6IAOHC3UEJA7VDCVXFBHEUAE4R5UMESSXNEAGLLLTIWO7HEWDG")]
    public void TestIsValidEd25519PublicKey(string id)
    {
        Assert.IsTrue(StrKey.IsValidEd25519PublicKey(id));
    }

    [TestMethod]
    [DataRow("SAA6NXOBOXP3RXGAXBW6PGFI5BPK4ODVAWITS4VDOMN5C2M4B66ZML", DisplayName = "Secret Key")]
    [DataRow("GAAAAAAAACGC6", DisplayName = "Invalid length (Ed25519 should be 32 bytes, not 5)")]
    [DataRow("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUACUSI",
        DisplayName = "Invalid length (base-32 decoding should yield 35 bytes, not 36)")]
    [DataRow("G47QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVP2I",
        DisplayName = "Invalid algorithm (low 3 bits of version byte are 7)")]
    [DataRow("")]
    public void TestIsNotValidEd25519PublicKey(string id)
    {
        Assert.IsFalse(StrKey.IsValidEd25519PublicKey(id));
    }

    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4T")]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJU")]
    [DataRow("GDWIZBODB4V7Q4V5TS4OMPUMDDTDNXMFQOSLVE6JG7PL4MHPTG6XO43Q")]
    public void TestIsNotValidMuxedEd25519PublicKey(string id)
    {
        Assert.IsFalse(StrKey.IsValidMuxedEd25519PublicKey(id));
    }
    
    [TestMethod]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ")]
    public void TestIsValidMuxedEd25519PublicKey(string id)
    {
        Assert.IsTrue(StrKey.IsValidMuxedEd25519PublicKey(id));
    }
    
    
    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4T")]
    [DataRow("LAARAHN7HK7EW57IB6FK5SOZIXXK56GKBOMSVJQTMRHOTABLMA2A5VVV")]
    [DataRow("MAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSAAAAAAAAAAE2LP26")]
    [DataRow("a26599c23752c92c3e74f60b72081b0b3f4b1a8353f357848f69c82e3fe6373c")]
    [DataRow("CBFZDBUL3OC2VRMCFKTV4G6H5R5FNRLDVCIE536CRWWF4RKXWGTSSLD")]
    public void TestIsNotValidContractId(string id)
    {
        Assert.IsFalse(StrKey.IsValidContractId(id));
    }
    
    [TestMethod]
    [DataRow("CCVEAWF4737OANGCBTM6ARENVOTKJQILJH4ZUJBANGZBPIUMNGYFGAEB")]
    [DataRow("CBFZDBUL3OC2VRMCFKTV4G6H5R5FNRLDVCIE536CRWWF4RKXWGTSSLDI")]
    [DataRow("CCXWM6ZYPYM6272RW3P3USK6XSK4DCUOQQVJ6WJLDABPR6ZDKGNKNLPT")]
    public void TestIsValidContractId(string id)
    {
        Assert.IsTrue(StrKey.IsValidContractId(id));
    }
    
    [TestMethod]
    [DataRow("MAAAAAAAAAAAAAB7BQ2L7E5NBWMXDUCMZSIPOBKRDSBYVLMXGSSKF6YNPIB7Y77ITIADJPA",
        DisplayName = "Invalid length (base-32 decoding should yield 43 bytes, not 44)")]
    [DataRow("M4AAAAAAAAAAAAB7BQ2L7E5NBWMXDUCMZSIPOBKRDSBYVLMXGSSKF6YNPIB7Y77ITIU2K",
        DisplayName = "Invalid algorithm (low 3 bits of version byte are 7)")]
    [DataRow("MAAAAAAAAAAAAAB7BQ2L7E5NBWMXDUCMZSIPOBKRDSBYVLMXGSSKF6YNPIB7Y77ITLVL4", DisplayName = "Invalid checksum")]
    public void TestIsNotValidMuxedAccount(string id)
    {
        Assert.IsFalse(StrKey.IsValidMuxedAccount(id));
    }

    [TestMethod]
    [DataRow("SDJHRQF4GCMIIKAAAQ6IHY42X73FQFLHUULAPSKKD4DFDM7UXWWCRHBE")]
    [DataRow("SCKQC5Z32U6TCQI6PM2W6GNEBEW3HLDRGZNARLDRJKKZBWCKJCQZ3MTN")]
    [DataRow("SARAUE3VC5ALDYEPW3J6EMIHKKLAOQNNPT54Q5V3XJ3JQLQ4YUE3XPWR")]
    public void TestIsValidEd25519SecretSeed(string seed)
    {
        Assert.IsTrue(StrKey.IsValidEd25519SecretSeed(seed));
    }

    [TestMethod]
    [DataRow("GCZHXL5HXQX5ABDM26LHYRCQZ5OJFHLOPLZX47WEBP3V2PF5AVFK2A5D")]
    [DataRow("BAAKEZMZYI3VFSJMHZ2PMC3SBANQWP2LDKBVH42XQSHWTSBOH7TDOPHPFA")]
    public void TestIsNotValidEd25519SecretSeed(string seed)
    {
        Assert.IsFalse(StrKey.IsValidEd25519SecretSeed(seed));
    }

    [TestMethod]
    public void TestValidSignedPayloadEncode()
    {
        var payload = Util.HexToBytes("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");
        var signedPayloadSigner =
            new SignedPayloadSigner(
                StrKey.DecodeStellarAccountId("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ"), payload);
        var encoded = StrKey.EncodeSignedPayload(signedPayloadSigner);
        Assert.AreEqual(encoded,
            "PA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAQACAQDAQCQMBYIBEFAWDANBYHRAEISCMKBKFQXDAMRUGY4DUPB6IBZGM");

        // Valid signed payload with an ed25519 public key and a 29-byte payload.
        payload = Util.HexToBytes("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d");
        signedPayloadSigner =
            new SignedPayloadSigner(
                StrKey.DecodeStellarAccountId("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ"), payload);
        encoded = StrKey.EncodeSignedPayload(signedPayloadSigner);
        Assert.AreEqual(encoded,
            "PA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAOQCAQDAQCQMBYIBEFAWDANBYHRAEISCMKBKFQXDAMRUGY4DUAAAAFGBU");
    }

    [TestMethod]
    public void TestRoundTripSignedPayloadVersionByte()
    {
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
        Assert.AreEqual(hashX, StrKey.EncodeCheck(StrKey.VersionByte.SIGNED_PAYLOAD, data));
        Assert.IsTrue(data.SequenceEqual(StrKey.DecodeCheck(StrKey.VersionByte.SIGNED_PAYLOAD, hashX)));
    }
}