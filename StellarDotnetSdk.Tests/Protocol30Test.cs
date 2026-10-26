using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Soroban;
using xdrSDK = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Tests for Protocol 30 support: CAP-84 muxed contract addresses (<c>SC_ADDRESS_TYPE_MUXED_CONTRACT</c> and
///     the <c>W...</c> strkey), CAP-88 millisecond close times (<c>STELLAR_VALUE_SIGNED_MS</c>,
///     <c>STELLAR_VALUE_EMPTY_TX_SET_MS</c>), CAP-87 ML-DSA cost types and the widened contract spec names.
/// </summary>
/// <remarks>
///     The strkey vectors are the SEP-23 test cases. The base64 constants are known-answer vectors produced by the Go
///     reference SDK (go-stellar-sdk v0.8.0, stellar-xdr v30.0); regenerate them with
///     <c>TestData/generate-p30-kat.go</c>.
/// </remarks>
[TestClass]
public class Protocol30Test
{
    // SEP-23: "Valid muxed contract address" and the same contract with an ID above long.MaxValue.
    private const string MuxedContractId0 = "WA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAAAWWC";
    private const string MuxedContractIdLarge = "WA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVAAAAAAAAAAAACWJY";
    private const string Contract = "CA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUWDA";

    // SEP-23: "Valid multiplexed account in which unsigned id exceeds maximum signed 64-bit integer".
    private const string MuxedAccountIdLarge = "MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVAAAAAAAAAAAAAJLK";
    private const string Ed25519 = "GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ";

    private const ulong LargeId = 9223372036854775808UL;

    private const string ScvMuxedContract = "AAAAEgAAAAWAAAAAAAAAAD8MNL+TrQ2ZcdBMzJD3BVEcg4qtlzSkovsNegP8f+ia";

    private const string StellarValueSignedMs =
        "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyAAAAAAauoTEAAAAAAAAAADAAABoaJadvsAAAAAPww0v5OtDZlx0EzMkPcFURyDiq2XNKSi+w16A/x/6JoAAABAQEFCQ0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4eXp7fH1+fw==";

    private const string StellarValueEmptyTxSetMs =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAauoTEAAAAAAAAAAEAAABoaJadvsBAgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4fIIGCg4SFhoeIiYqLjI2Oj5CRkpOUlZaXmJmam5ydnp+gAAAAHgAAAAA/DDS/k60NmXHQTMyQ9wVRHIOKrZc0pKL7DXoD/H/omgAAAEBAQUJDREVGR0hJSktMTU5PUFFSU1RVVldYWVpbXF1eX2BhYmNkZWZnaGlqa2xtbm9wcXJzdHV2d3h5ent8fX5/";

    private const string LedgerHeaderSignedMs =
        "AAAAHoGCg4SFhoeIiYqLjI2Oj5CRkpOUlZaXmJmam5ydnp+gAQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyAAAAAAauoTEAAAAAAAAAADAAABoaJadvsAAAAAPww0v5OtDZlx0EzMkPcFURyDiq2XNKSi+w16A/x/6JoAAABAQEFCQ0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4eXp7fH1+fwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD0JAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==";

    // 2026-11-03T17:00:00.123Z
    private const ulong CloseTimeMs = 1793725200123UL;

    private static readonly byte[] TxSetHash = Enumerable.Range(0x01, 32).Select(i => (byte)i).ToArray();
    private static readonly byte[] PreviousLedgerHash = Enumerable.Range(0x81, 32).Select(i => (byte)i).ToArray();

    private static byte[] RoundTrip<T>(string base64, Func<xdrSDK.XdrDataInputStream, T> decode,
        Action<xdrSDK.XdrDataOutputStream, T> encode)
    {
        var decoded = decode(new xdrSDK.XdrDataInputStream(Convert.FromBase64String(base64)));
        var stream = new xdrSDK.XdrDataOutputStream();
        encode(stream, decoded);
        return stream.ToArray();
    }

    private static T Decode<T>(string base64, Func<xdrSDK.XdrDataInputStream, T> decode)
    {
        return decode(new xdrSDK.XdrDataInputStream(Convert.FromBase64String(base64)));
    }

    // ---------------------------------------------------------------------------------------------
    // StrKey: W... (SEP-23)
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    ///     The SEP-23 muxed contract vectors decode to the 32-byte contract ID followed by the 8-byte big-endian ID,
    ///     and re-encode to the same strkey.
    /// </summary>
    [TestMethod]
    [DataRow(MuxedContractId0, 0UL)]
    [DataRow(MuxedContractIdLarge, LargeId)]
    public void DecodeMuxedContractId_WithSep23Vector_ReturnsContractIdThenBigEndianId(string strKey, ulong id)
    {
        // Act
        var decoded = StrKey.DecodeMuxedContractId(strKey);

        // Assert
        Assert.AreEqual(40, decoded.Length);
        CollectionAssert.AreEqual(StrKey.DecodeContractId(Contract), decoded[..32]);
        var idBytes = BitConverter.GetBytes(id);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(idBytes);
        }
        CollectionAssert.AreEqual(idBytes, decoded[32..]);
        Assert.AreEqual(strKey, StrKey.EncodeMuxedContractId(decoded));
        Assert.IsTrue(StrKey.IsValidMuxedContractId(strKey));
        Assert.AreEqual(StrKey.VersionByte.MUXED_CONTRACT, StrKey.DecodeVersionByte(strKey));
    }

    /// <summary>
    ///     The SEP-23 invalid muxed contract vectors are rejected.
    /// </summary>
    [TestMethod]
    [DataRow("WA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAAAWWD",
        DisplayName = "Unused trailing bit set")]
    [DataRow("WA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAAAWWCA",
        DisplayName = "Length congruent to 6 mod 8")]
    [DataRow("WA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAAAAIOUI",
        DisplayName = "Decodes to 44 bytes")]
    [DataRow("W47QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAADXHW",
        DisplayName = "Invalid algorithm")]
    [DataRow("WA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAAAWWA",
        DisplayName = "Invalid checksum")]
    public void IsValidMuxedContractId_WithSep23InvalidVector_ReturnsFalse(string strKey)
    {
        // Act & Assert
        Assert.IsFalse(StrKey.IsValidMuxedContractId(strKey));
        Assert.ThrowsException<ArgumentException>(() => StrKey.DecodeMuxedContractId(strKey));
    }

    /// <summary>
    ///     The internal raw-payload helpers put the contract ID before the big-endian ID and reject payloads of the
    ///     wrong length.
    /// </summary>
    [TestMethod]
    public void RawMuxedContractId_RoundTripsAndRejectsWrongLengths()
    {
        // Arrange
        var contractId = StrKey.DecodeContractId(Contract);

        // Act
        var raw = StrKey.ToRawMuxedContractId(contractId, LargeId);
        var (decodedContractId, decodedId) = StrKey.FromRawMuxedContractId(raw);

        // Assert
        Assert.AreEqual(MuxedContractIdLarge, StrKey.EncodeMuxedContractId(raw));
        CollectionAssert.AreEqual(contractId, decodedContractId);
        Assert.AreEqual(LargeId, decodedId);
        Assert.ThrowsException<ArgumentException>(() => StrKey.ToRawMuxedContractId(new byte[31], 0));
        Assert.ThrowsException<ArgumentException>(() => StrKey.ToRawMuxedContractId(new byte[33], 0));
        Assert.ThrowsException<ArgumentException>(() => StrKey.FromRawMuxedContractId(new byte[39]));
        Assert.ThrowsException<ArgumentException>(() => StrKey.FromRawMuxedContractId(new byte[41]));
    }

    /// <summary>
    ///     A W... strkey is not accepted where a C... or M... strkey is expected, and vice versa.
    /// </summary>
    [TestMethod]
    public void IsValid_WithMuxedContractAndOtherTypes_DoesNotCrossAccept()
    {
        // Assert
        Assert.IsFalse(StrKey.IsValidContractId(MuxedContractId0));
        Assert.IsFalse(StrKey.IsValidMed25519PublicKey(MuxedContractId0));
        Assert.IsFalse(StrKey.IsValidMuxedContractId(Contract));
        Assert.IsFalse(StrKey.IsValidMuxedContractId(MuxedAccountIdLarge));
    }

    // ---------------------------------------------------------------------------------------------
    // SDK layer: ScMuxedContractId
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    ///     A muxed contract address encodes to the same <c>SCV_ADDRESS</c> XDR as the Go reference SDK: the XDR struct
    ///     carries the ID before the contract ID, the reverse of the strkey payload.
    /// </summary>
    [TestMethod]
    public void ScMuxedContractId_ToXdrBase64_MatchesReferenceVector()
    {
        // Arrange
        var address = new ScMuxedContractId(MuxedContractIdLarge);

        // Act
        var xdr = address.ToXdr();

        // Assert
        Assert.AreEqual(ScvMuxedContract, address.ToXdrBase64());
        Assert.AreEqual(xdrSDK.SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_MUXED_CONTRACT,
            xdr.Discriminant.InnerValue);
        Assert.AreEqual(LargeId, xdr.MuxedContract.Id.InnerValue);
        CollectionAssert.AreEqual(StrKey.DecodeContractId(Contract), xdr.MuxedContract.ContractId.InnerValue.InnerValue);
    }

    /// <summary>
    ///     Decoding the reference <c>SCV_ADDRESS</c> XDR yields an <see cref="ScMuxedContractId" /> with the SEP-23
    ///     strkey, its contract ID and its muxed ID.
    /// </summary>
    [TestMethod]
    public void ScVal_FromXdrBase64_WithMuxedContract_ReturnsScMuxedContractId()
    {
        // Act
        var address = SCVal.FromXdrBase64(ScvMuxedContract);

        // Assert
        Assert.IsInstanceOfType(address, typeof(ScMuxedContractId));
        var muxed = (ScMuxedContractId)address;
        Assert.AreEqual(MuxedContractIdLarge, muxed.InnerValue);
        Assert.AreEqual(Contract, muxed.ContractId);
        Assert.AreEqual(LargeId, muxed.Id);
    }

    /// <summary>
    ///     The contract-and-ID constructor builds the same address as the strkey, including the stellar-horizon
    ///     fixture for a <c>contract_credited</c> effect to a muxed contract.
    /// </summary>
    [TestMethod]
    [DataRow(Contract, 0UL, MuxedContractId0)]
    [DataRow(Contract, LargeId, MuxedContractIdLarge)]
    [DataRow("CAA3QKIP2SNVXUJTB4HKOGF55JTSSMQGED3FZYNHMNSXYV3DRRMAWA3Y", 987654321987654321UL,
        "WAA3QKIP2SNVXUJTB4HKOGF55JTSSMQGED3FZYNHMNSXYV3DRRMAWDNU3JPX55ASWEDNS")]
    public void ScMuxedContractId_FromContractIdAndId_EncodesExpectedStrKey(string contractId, ulong id,
        string expected)
    {
        // Act
        var address = new ScMuxedContractId(contractId, id);

        // Assert
        Assert.AreEqual(expected, address.InnerValue);
        Assert.AreEqual(contractId, address.ContractId);
        Assert.AreEqual(id, address.Id);
    }

    /// <summary>
    ///     <see cref="ScMuxedContractId" /> rejects anything but a W... strkey, and the contract, muxed account and
    ///     account address types reject W....
    /// </summary>
    [TestMethod]
    public void AddressConstructors_WithWrongStrKeyType_ThrowArgumentException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => new ScMuxedContractId(Contract));
        Assert.ThrowsException<ArgumentException>(() => new ScMuxedContractId(MuxedAccountIdLarge));
        Assert.ThrowsException<ArgumentException>(() => new ScMuxedContractId(MuxedContractId0, 1));
        Assert.ThrowsException<ArgumentException>(() => new ScContractId(MuxedContractId0));
        Assert.ThrowsException<ArgumentException>(() => new ScMuxedAccountId(MuxedContractId0));
        Assert.ThrowsException<ArgumentException>(() => new ScAccountId(MuxedContractId0));
    }

    // ---------------------------------------------------------------------------------------------
    // XDR layer: CAP-88 StellarValue and LedgerHeader
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    ///     CAP-88: a <c>StellarValue</c> with the <c>STELLAR_VALUE_SIGNED_MS</c> ext decodes, exposes the millisecond
    ///     close time (whose whole seconds equal <c>closeTime</c>) and the signature, and re-encodes to the same bytes.
    /// </summary>
    [TestMethod]
    public void XdrStellarValue_SignedMsExt_DecodesAndRoundTrips()
    {
        // Act
        var value = Decode(StellarValueSignedMs, xdrSDK.StellarValue.Decode);

        // Assert
        Assert.AreEqual(xdrSDK.StellarValueType.StellarValueTypeEnum.STELLAR_VALUE_SIGNED_MS,
            value.Ext.Discriminant.InnerValue);
        CollectionAssert.AreEqual(TxSetHash, value.TxSetHash.InnerValue);
        Assert.AreEqual(CloseTimeMs, value.Ext.SignedMsValue.CloseTimeMs.InnerValue.InnerValue);
        Assert.AreEqual(CloseTimeMs / 1000, value.CloseTime.InnerValue.InnerValue);
        Assert.AreEqual(Ed25519,
            StrKey.EncodeEd25519PublicKey(value.Ext.SignedMsValue.LcValueSignature.NodeID.InnerValue.Ed25519
                .InnerValue));
        Assert.AreEqual(64, value.Ext.SignedMsValue.LcValueSignature.Signature.InnerValue.Length);
        Assert.AreEqual(StellarValueSignedMs,
            Convert.ToBase64String(RoundTrip(StellarValueSignedMs, xdrSDK.StellarValue.Decode,
                xdrSDK.StellarValue.Encode)));
    }

    /// <summary>
    ///     CAP-88: a <c>StellarValue</c> with the <c>STELLAR_VALUE_EMPTY_TX_SET_MS</c> ext decodes, exposes the
    ///     proposed value with its millisecond close time, and re-encodes to the same bytes.
    /// </summary>
    [TestMethod]
    public void XdrStellarValue_EmptyTxSetMsExt_DecodesAndRoundTrips()
    {
        // Act
        var value = Decode(StellarValueEmptyTxSetMs, xdrSDK.StellarValue.Decode);

        // Assert
        Assert.AreEqual(xdrSDK.StellarValueType.StellarValueTypeEnum.STELLAR_VALUE_EMPTY_TX_SET_MS,
            value.Ext.Discriminant.InnerValue);
        CollectionAssert.AreEqual(new byte[32], value.TxSetHash.InnerValue);
        var proposed = value.Ext.ProposedMsValue;
        Assert.AreEqual(CloseTimeMs, proposed.CloseTimeMs.InnerValue.InnerValue);
        CollectionAssert.AreEqual(TxSetHash, proposed.TxSetHash.InnerValue);
        CollectionAssert.AreEqual(PreviousLedgerHash, proposed.PreviousLedgerHash.InnerValue);
        Assert.AreEqual(30u, proposed.PreviousLedgerVersion.InnerValue);
        Assert.AreEqual(StellarValueEmptyTxSetMs,
            Convert.ToBase64String(RoundTrip(StellarValueEmptyTxSetMs, xdrSDK.StellarValue.Decode,
                xdrSDK.StellarValue.Encode)));
    }

    /// <summary>
    ///     A Protocol 30 <c>LedgerHeader</c>, whose <c>scpValue</c> uses a CAP-88 arm, decodes and re-encodes. With
    ///     the v28 XDR this threw on the unknown <c>StellarValueType</c>.
    /// </summary>
    [TestMethod]
    public void XdrLedgerHeader_WithSignedMsScpValue_DecodesAndRoundTrips()
    {
        // Act
        var header = Decode(LedgerHeaderSignedMs, xdrSDK.LedgerHeader.Decode);

        // Assert
        Assert.AreEqual(30u, header.LedgerVersion.InnerValue);
        Assert.AreEqual(1000000u, header.LedgerSeq.InnerValue);
        Assert.AreEqual(CloseTimeMs, header.ScpValue.Ext.SignedMsValue.CloseTimeMs.InnerValue.InnerValue);
        Assert.AreEqual(LedgerHeaderSignedMs,
            Convert.ToBase64String(RoundTrip(LedgerHeaderSignedMs, xdrSDK.LedgerHeader.Decode,
                xdrSDK.LedgerHeader.Encode)));
    }

    // ---------------------------------------------------------------------------------------------
    // XDR layer: enum wire values and spec limits
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    ///     The new enum members carry their stellar-xdr v30.0 wire values.
    /// </summary>
    [TestMethod]
    public void XdrEnums_Protocol30Members_HaveWireValues()
    {
        // Assert
        Assert.AreEqual(5, (int)xdrSDK.SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_MUXED_CONTRACT);
        Assert.AreEqual(3, (int)xdrSDK.StellarValueType.StellarValueTypeEnum.STELLAR_VALUE_SIGNED_MS);
        Assert.AreEqual(4, (int)xdrSDK.StellarValueType.StellarValueTypeEnum.STELLAR_VALUE_EMPTY_TX_SET_MS);
        Assert.AreEqual(86, (int)xdrSDK.ContractCostType.ContractCostTypeEnum.MlDsa44DecodeVerifyingKey);
        Assert.AreEqual(94, (int)xdrSDK.ContractCostType.ContractCostTypeEnum.VerifyMlDsa87Sig);
        Assert.AreEqual(1024, xdrSDK.Constants.SC_SPEC_TYPE_NAME_LIMIT);
    }

    /// <summary>
    ///     The first discriminant past the Protocol 30 members is still rejected, so a value the schema does not
    ///     define cannot decode as a known address or ledger value type.
    /// </summary>
    [TestMethod]
    public void XdrEnums_FirstUndefinedDiscriminant_ThrowsInvalidDataException()
    {
        // Act & Assert
        Assert.ThrowsException<System.IO.InvalidDataException>(() =>
            xdrSDK.SCAddressType.Decode(new xdrSDK.XdrDataInputStream([0, 0, 0, 6])));
        Assert.ThrowsException<System.IO.InvalidDataException>(() =>
            xdrSDK.StellarValueType.Decode(new xdrSDK.XdrDataInputStream([0, 0, 0, 5])));
    }

    /// <summary>
    ///     stellar-xdr#312 widened <c>SCSpecEventV0.name</c> from an <c>SCSymbol</c> (at most 32 bytes) to
    ///     <c>string&lt;1024&gt;</c>; an event name longer than 32 bytes now round-trips.
    /// </summary>
    [TestMethod]
    public void XdrSCSpecEventV0_WithNameLongerThanSymbol_RoundTrips()
    {
        // Arrange
        var name = new string('e', 100);
        var spec = new xdrSDK.SCSpecEventV0
        {
            Doc = "",
            Lib = "",
            Name = name,
            PrefixTopics = [],
            Params = [],
            DataFormat = xdrSDK.SCSpecEventDataFormat.Create(
                xdrSDK.SCSpecEventDataFormat.SCSpecEventDataFormatEnum.SC_SPEC_EVENT_DATA_FORMAT_MAP),
        };
        var stream = new xdrSDK.XdrDataOutputStream();

        // Act
        xdrSDK.SCSpecEventV0.Encode(stream, spec);
        var decoded = xdrSDK.SCSpecEventV0.Decode(new xdrSDK.XdrDataInputStream(stream.ToArray()));

        // Assert
        Assert.AreEqual(name, decoded.Name);
    }
}
