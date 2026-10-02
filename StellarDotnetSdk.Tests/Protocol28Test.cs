using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using xdrSDK = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Tests for Protocol 28 support: CAP-85 external executable references (<c>SCV_EXECUTABLE_TAG</c>,
///     <c>CONTRACT_EXECUTABLE_EXTERNAL_REF</c>), CAP-83 <c>STELLAR_VALUE_EMPTY_TX_SET</c>, and the byte-preserving XDR
///     string types that CAP-85 tags depend on.
/// </summary>
/// <remarks>
///     The base64 constants are known-answer vectors produced by the JS reference SDK
///     (@stellar/stellar-sdk v17.2.0); regenerate them with <c>TestData/generate-p28-kat.mjs</c>.
/// </remarks>
[TestClass]
public class Protocol28Test
{
    private const string Owner = "CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7";
    private const string Deployer = "GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP";
    private const string TextTag = "fleet-v1";

    private const string ScvExecutableTagText = "AAAAFgAAAAhmbGVldC12MQ==";
    private const string ScvExecutableTagBinary = "AAAAFgAAAAX//gDDKAAAAA==";
    private const string NonAsciiTag = "flotte-\u00e9-\U0001F680";
    private const string ScvExecutableTagNonAscii = "AAAAFgAAAA5mbG90dGUtw6kt8J+agAAA";

    private const string ContractExecutableExternalRefText =
        "AAAAAgAAAAHTyKBAbK986/22nY1T8Xf2fFIhMGJ621Sl8tMQKKhbFQAAAAhmbGVldC12MQ==";

    private const string ContractExecutableExternalRefBinary =
        "AAAAAgAAAAHTyKBAbK986/22nY1T8Xf2fFIhMGJ621Sl8tMQKKhbFQAAAAX//gDDKAAAAA==";

    private const string CreateContractFromExternalRefOpText =
        "AAAAAAAAABgAAAADAAAAAAAAAAAAAAAACBCpRzCAPu765wRTTqRRvrtSDgEtyUynYbBzll2zoZwBAgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4fIAAAAAIAAAAB08igQGyvfOv9tp2NU/F39nxSITBiettUpfLTECioWxUAAAAIZmxlZXQtdjEAAAABAAAAAwAAAAcAAAAA";

    private const string CreateContractFromExternalRefOpBinary =
        "AAAAAAAAABgAAAADAAAAAAAAAAAAAAAACBCpRzCAPu765wRTTqRRvrtSDgEtyUynYbBzll2zoZwBAgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4fIAAAAAIAAAAB08igQGyvfOv9tp2NU/F39nxSITBiettUpfLTECioWxUAAAAF//4AwygAAAAAAAABAAAAAwAAAAcAAAAA";

    private const string ExternalRefLedgerKeyText =
        "AAAABgAAAAHTyKBAbK986/22nY1T8Xf2fFIhMGJ621Sl8tMQKKhbFQAAABYAAAAIZmxlZXQtdjEAAAAB";

    private const string ExternalRefLedgerKeyBinary =
        "AAAABgAAAAHTyKBAbK986/22nY1T8Xf2fFIhMGJ621Sl8tMQKKhbFQAAABYAAAAF//4AwygAAAAAAAAB";

    private const string ExternalRefLedgerEntryDataText =
        "AAAABgAAAAAAAAAB08igQGyvfOv9tp2NU/F39nxSITBiettUpfLTECioWxUAAAAWAAAACGZsZWV0LXYxAAAAAQAAAA0AAAAgoKGio6SlpqeoqaqrrK2ur7CxsrO0tba3uLm6u7y9vr8=";

    private const string ExternalRefLedgerEntryDataBinary =
        "AAAABgAAAAAAAAAB08igQGyvfOv9tp2NU/F39nxSITBiettUpfLTECioWxUAAAAWAAAABf/+AMMoAAAAAAAAAQAAAA0AAAAgoKGio6SlpqeoqaqrrK2ur7CxsrO0tba3uLm6u7y9vr8=";

    private const string StellarValueEmptyTxSet =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAaMjzgAAAAAAAAAACEBESExQVFhcYGRobHB0eHyAhIiMkJSYnKCkqKywtLi9AQUJDREVGR0hJSktMTU5PUFFSU1RVVldYWVpbXF1eXwAAABwAAAAACBCpRzCAPu765wRTTqRRvrtSDgEtyUynYbBzll2zoZwAAABAgIGCg4SFhoeIiYqLjI2Oj5CRkpOUlZaXmJmam5ydnp+goaKjpKWmp6ipqqusra6vsLGys7S1tre4ubq7vL2+vw==";

    // 0xff never appears in UTF-8, and 0xc3 0x28 is a truncated two-byte sequence.
    private static readonly byte[] BinaryTag = [0xff, 0xfe, 0x00, 0xc3, 0x28];
    private static readonly byte[] Salt = Enumerable.Range(1, 32).Select(i => (byte)i).ToArray();
    private static readonly byte[] WasmHash = Enumerable.Range(0xa0, 32).Select(i => (byte)i).ToArray();

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

    private static string GetLedgerEntriesJson(string entryXdr, string keyXdr, long? latestLedger, long liveUntil)
    {
        var latestLedgerMember = latestLedger is { } ledger ? $",\n    \"latestLedger\": {ledger}" : "";
        return $$"""
                 {
                   "jsonrpc": "2.0",
                   "id": "8675309",
                   "result": {
                     "entries": [
                       {
                         "key": "{{keyXdr}}",
                         "xdr": "{{entryXdr}}",
                         "lastModifiedLedgerSeq": 100,
                         "liveUntilLedgerSeq": {{liveUntil}}
                       }
                     ]{{latestLedgerMember}}
                   }
                 }
                 """;
    }

    private static string BuildContractDataEntryXdr(string contract, SCVal key,
        xdrSDK.ContractDataDurability.ContractDataDurabilityEnum durability, SCVal value)
    {
        var entry = new xdrSDK.LedgerEntry.LedgerEntryData
        {
            Discriminant = xdrSDK.LedgerEntryType.Create(xdrSDK.LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA),
            ContractData = new xdrSDK.ContractDataEntry
            {
                Ext = new xdrSDK.ExtensionPoint { Discriminant = 0 },
                Contract = new ScContractId(contract).ToXdr(),
                Key = key.ToXdr(),
                Durability = xdrSDK.ContractDataDurability.Create(durability),
                Val = value.ToXdr(),
            },
        };
        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.LedgerEntry.LedgerEntryData.Encode(stream, entry);
        return Convert.ToBase64String(stream.ToArray());
    }

    // ---------------------------------------------------------------------------------------------
    // XDR layer: byte-preserving strings
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    ///     A non-UTF-8 tag must survive decode and re-encode byte for byte. Before the XDR string types kept their raw
    ///     bytes, decoding replaced the invalid bytes with U+FFFD and re-encoding produced a different tag, and so the
    ///     key of a different ledger entry.
    /// </summary>
    [TestMethod]
    public void XdrSCVal_ExecutableTagWithNonUtf8Bytes_RoundTripsByteForByte()
    {
        // Act
        var bytes = RoundTrip(ScvExecutableTagBinary, xdrSDK.SCVal.Decode, xdrSDK.SCVal.Encode);

        // Assert
        Assert.AreEqual(ScvExecutableTagBinary, Convert.ToBase64String(bytes));
    }

    /// <summary>
    ///     The generated string typedefs decode into <c>InnerBytes</c> verbatim, while <c>InnerValue</c> stays
    ///     available as a (lossy) UTF-8 view for existing callers.
    /// </summary>
    [TestMethod]
    public void XdrSCString_Decode_KeepsRawBytesAndExposesUtf8View()
    {
        // Act
        var xdrVal = Decode(ScvExecutableTagBinary, xdrSDK.SCVal.Decode);

        // Assert
        CollectionAssert.AreEqual(BinaryTag, xdrVal.ExecutableTag.InnerBytes);
        Assert.AreEqual(Encoding.UTF8.GetString(BinaryTag), xdrVal.ExecutableTag.InnerValue);
    }

    /// <summary>
    ///     Every generated string typedef, not only <c>SCString</c>, decodes and re-encodes non-UTF-8 bytes verbatim.
    /// </summary>
    [TestMethod]
    public void XdrStringTypedefs_NonUtf8Bytes_RoundTripByteForByte()
    {
        // Arrange
        var wire = new xdrSDK.XdrDataOutputStream();
        wire.WriteStringBytes(BinaryTag);
        var bytes = wire.ToArray();

        // Act & Assert
        CollectionAssert.AreEqual(bytes, RoundTrip(Convert.ToBase64String(bytes), xdrSDK.SCSymbol.Decode,
            xdrSDK.SCSymbol.Encode));
        CollectionAssert.AreEqual(bytes, RoundTrip(Convert.ToBase64String(bytes), xdrSDK.String32.Decode,
            xdrSDK.String32.Encode));
        CollectionAssert.AreEqual(bytes, RoundTrip(Convert.ToBase64String(bytes), xdrSDK.String64.Decode,
            xdrSDK.String64.Encode));
        CollectionAssert.AreEqual(bytes, RoundTrip(Convert.ToBase64String(bytes), xdrSDK.SCString.Decode,
            xdrSDK.SCString.Encode));
    }

    /// <summary>
    ///     Setting <c>InnerValue</c> replaces the raw bytes with the UTF-8 encoding of the text, so code written against
    ///     the string-only API keeps producing the same wire bytes.
    /// </summary>
    [TestMethod]
    public void XdrStringTypedefs_InnerValueSetter_EncodesUtf8()
    {
        // Arrange & Act
        var scString = new xdrSDK.SCString("héllo");
        var scSymbol = new xdrSDK.SCSymbol { InnerValue = "transfer" };
        var string32 = new xdrSDK.String32("ok");
        var string64 = new xdrSDK.String64 { InnerValue = null };
        var nullString = new xdrSDK.SCString { InnerValue = null };

        // Assert
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("héllo"), scString.InnerBytes);
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("transfer"), scSymbol.InnerBytes);
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("ok"), string32.InnerBytes);
        Assert.IsNull(string64.InnerBytes);
        Assert.IsNull(string64.InnerValue);
        Assert.IsNull(nullString.InnerBytes);

        // Act & Assert
        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.SCString.Encode(stream, scString);
        var legacy = new xdrSDK.XdrDataOutputStream();
        legacy.WriteString("héllo");
        CollectionAssert.AreEqual(legacy.ToArray(), stream.ToArray());
    }

    /// <summary>
    ///     The <c>InnerValue</c> setter of every string typedef encodes an unpaired surrogate as U+FFFD (EF BF BD), as
    ///     the setter's documentation states and as the string-only API did on the wire, rather than throwing.
    /// </summary>
    [TestMethod]
    public void XdrStringTypedefs_InnerValueSetterWithUnpairedSurrogate_EncodesReplacementCharacter()
    {
        // Arrange
        var text = "a" + (char)0xD800;
        byte[] expected = [0x61, 0xEF, 0xBF, 0xBD];

        // Act
        var scString = new xdrSDK.SCString { InnerValue = text };
        var scSymbol = new xdrSDK.SCSymbol { InnerValue = text };
        var string32 = new xdrSDK.String32 { InnerValue = text };
        var string64 = new xdrSDK.String64 { InnerValue = text };

        // Assert
        CollectionAssert.AreEqual(expected, scString.InnerBytes);
        CollectionAssert.AreEqual(expected, scSymbol.InnerBytes);
        CollectionAssert.AreEqual(expected, string32.InnerBytes);
        CollectionAssert.AreEqual(expected, string64.InnerBytes);
        Assert.AreEqual("a�", scString.InnerValue);
    }

    /// <summary>
    ///     Encoding a string typedef that holds no bytes fails with <see cref="ArgumentNullException" />, as it did when
    ///     <c>InnerValue</c> was a null string.
    /// </summary>
    [TestMethod]
    public void XdrSCString_EncodeWithoutBytes_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() =>
            xdrSDK.SCString.Encode(new xdrSDK.XdrDataOutputStream(), new xdrSDK.SCString()));
    }

    /// <summary>
    ///     A string typedef enforces its declared maximum length in bytes, as every bounded opaque and array does, so an
    ///     oversized value fails when it is encoded instead of later at the network. <c>SCString</c> declares no bound.
    /// </summary>
    [TestMethod]
    public void XdrStringTypedefs_EncodeOverMaxLength_ThrowsArgumentException()
    {
        // Arrange
        static void Encode<T>(Action<xdrSDK.XdrDataOutputStream, T> encode, T value)
        {
            encode(new xdrSDK.XdrDataOutputStream(), value);
        }

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            Encode(xdrSDK.SCSymbol.Encode, new xdrSDK.SCSymbol { InnerBytes = new byte[33] }));
        Assert.ThrowsException<ArgumentException>(() =>
            Encode(xdrSDK.String32.Encode, new xdrSDK.String32 { InnerBytes = new byte[33] }));
        Assert.ThrowsException<ArgumentException>(() =>
            Encode(xdrSDK.String64.Encode, new xdrSDK.String64 { InnerBytes = new byte[65] }));
        // 17 characters, but 34 bytes: the bound counts bytes.
        Assert.ThrowsException<ArgumentException>(() =>
            Encode(xdrSDK.SCSymbol.Encode, new xdrSDK.SCSymbol(new string('é', 17))));
        Encode(xdrSDK.SCSymbol.Encode, new xdrSDK.SCSymbol { InnerBytes = new byte[32] });
        Encode(xdrSDK.String32.Encode, new xdrSDK.String32 { InnerBytes = new byte[32] });
        Encode(xdrSDK.String64.Encode, new xdrSDK.String64 { InnerBytes = new byte[64] });
        Encode(xdrSDK.SCString.Encode, new xdrSDK.SCString { InnerBytes = new byte[1000] });
    }

    /// <summary>
    ///     Decoding rejects a length prefix above the declared maximum with <see cref="System.IO.InvalidDataException" />,
    ///     as the generated decoders do for every bounded opaque and array. <c>SCString</c> declares no bound.
    /// </summary>
    [TestMethod]
    public void XdrStringTypedefs_DecodeOverMaxLength_ThrowsInvalidDataException()
    {
        // Arrange
        static xdrSDK.XdrDataInputStream Wire(int length)
        {
            var stream = new xdrSDK.XdrDataOutputStream();
            stream.WriteStringBytes(new byte[length]);
            return new xdrSDK.XdrDataInputStream(stream.ToArray());
        }

        // Act & Assert
        Assert.ThrowsException<System.IO.InvalidDataException>(() => xdrSDK.SCSymbol.Decode(Wire(33)));
        Assert.ThrowsException<System.IO.InvalidDataException>(() => xdrSDK.String32.Decode(Wire(33)));
        Assert.ThrowsException<System.IO.InvalidDataException>(() => xdrSDK.String64.Decode(Wire(65)));
        Assert.AreEqual(32, xdrSDK.SCSymbol.Decode(Wire(32)).InnerBytes.Length);
        Assert.AreEqual(32, xdrSDK.String32.Decode(Wire(32)).InnerBytes.Length);
        Assert.AreEqual(64, xdrSDK.String64.Decode(Wire(64)).InnerBytes.Length);
        Assert.AreEqual(1000, xdrSDK.SCString.Decode(Wire(1000)).InnerBytes.Length);
    }

    // ---------------------------------------------------------------------------------------------
    // XDR layer: new arms and stripped feature gates
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    ///     CAP-83: a <c>StellarValue</c> whose ext is <c>STELLAR_VALUE_EMPTY_TX_SET</c> decodes, exposes the proposed
    ///     value, and re-encodes to the same bytes. As the CAP specifies, the externalized <c>txSetHash</c> is 0x0 and
    ///     the hash of the dropped transaction set is kept in the proposed value.
    /// </summary>
    [TestMethod]
    public void XdrStellarValue_EmptyTxSetExt_DecodesAndRoundTrips()
    {
        // Act
        var value = Decode(StellarValueEmptyTxSet, xdrSDK.StellarValue.Decode);

        // Assert
        Assert.AreEqual(xdrSDK.StellarValueType.StellarValueTypeEnum.STELLAR_VALUE_EMPTY_TX_SET,
            value.Ext.Discriminant.InnerValue);
        Assert.AreEqual(2, (int)value.Ext.Discriminant.InnerValue);
        var proposed = value.Ext.ProposedValue;
        CollectionAssert.AreEqual(new byte[32], value.TxSetHash.InnerValue);
        CollectionAssert.AreEqual(Enumerable.Range(0x10, 32).Select(i => (byte)i).ToArray(),
            proposed.TxSetHash.InnerValue);
        CollectionAssert.AreEqual(Enumerable.Range(0x40, 32).Select(i => (byte)i).ToArray(),
            proposed.PreviousLedgerHash.InnerValue);
        Assert.AreEqual(28u, proposed.PreviousLedgerVersion.InnerValue);
        Assert.AreEqual(Deployer,
            KeyPair.FromXdrPublicKey(proposed.LcValueSignature.NodeID.InnerValue).AccountId);
        Assert.AreEqual(64, proposed.LcValueSignature.Signature.InnerValue.Length);
        Assert.AreEqual(1758000000UL, value.CloseTime.InnerValue.InnerValue);

        // Act & Assert
        var bytes = RoundTrip(StellarValueEmptyTxSet, xdrSDK.StellarValue.Decode, xdrSDK.StellarValue.Encode);
        Assert.AreEqual(StellarValueEmptyTxSet, Convert.ToBase64String(bytes));
    }

    /// <summary>
    ///     The new enum members carry their stellar-xdr v28.0 wire values.
    /// </summary>
    [TestMethod]
    public void XdrEnums_Protocol28Members_HaveWireValues()
    {
        // Assert
        Assert.AreEqual(22, (int)xdrSDK.SCValType.SCValTypeEnum.SCV_EXECUTABLE_TAG);
        Assert.AreEqual(2,
            (int)xdrSDK.ContractExecutableType.ContractExecutableTypeEnum.CONTRACT_EXECUTABLE_EXTERNAL_REF);
        Assert.AreEqual(2, (int)xdrSDK.StellarValueType.StellarValueTypeEnum.STELLAR_VALUE_EMPTY_TX_SET);
    }

    /// <summary>
    ///     stellar-xdr v28.0 gates CAP-84 (muxed contract addresses) and a <c>TEST_FEATURE</c> type behind
    ///     <c>#ifdef</c>s that are not part of Protocol 28. The generator has no preprocessor, so they are stripped
    ///     from the schemas by hand; none of them may reach the generated code.
    /// </summary>
    [TestMethod]
    public void XdrGeneratedCode_ContainsNoFeatureGatedDefinitions()
    {
        // Arrange
        var assembly = typeof(xdrSDK.SCAddress).Assembly;

        // Act & Assert
        CollectionAssert.DoesNotContain(Enum.GetNames(typeof(xdrSDK.SCAddressType.SCAddressTypeEnum)),
            "SC_ADDRESS_TYPE_MUXED_CONTRACT");
        Assert.AreEqual(5, Enum.GetValues(typeof(xdrSDK.SCAddressType.SCAddressTypeEnum)).Length);
        Assert.IsNull(typeof(xdrSDK.SCAddress).GetProperty("MuxedContract"));
        Assert.IsNull(assembly.GetType("StellarDotnetSdk.Xdr.MuxedContract"));
        Assert.IsNull(assembly.GetType("StellarDotnetSdk.Xdr.TestNextType"));
        Assert.ThrowsException<System.IO.InvalidDataException>(() =>
            xdrSDK.SCAddressType.Decode(new xdrSDK.XdrDataInputStream([0, 0, 0, 5])));
    }

    // ---------------------------------------------------------------------------------------------
    // SDK layer: SCExecutableTag
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    ///     A text tag decodes to <see cref="SCExecutableTag" /> through the <see cref="SCVal.FromXdr" /> dispatch and
    ///     encodes to the reference SDK's bytes.
    /// </summary>
    [TestMethod]
    public void SCExecutableTag_TextTag_MatchesKnownAnswerVector()
    {
        // Act
        var tag = (SCExecutableTag)SCVal.FromXdrBase64(ScvExecutableTagText);

        // Assert
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(TextTag), tag.InnerValue);
        Assert.IsTrue(tag.TryGetUtf8String(out var text));
        Assert.AreEqual(TextTag, text);
        Assert.AreEqual(ScvExecutableTagText, tag.ToXdrBase64());
        Assert.AreEqual(ScvExecutableTagText, new SCExecutableTag(TextTag).ToXdrBase64());
    }

    /// <summary>
    ///     A binary tag keeps its exact bytes, and the strict UTF-8 accessor reports it as not text instead of
    ///     returning a lossy decode.
    /// </summary>
    [TestMethod]
    public void SCExecutableTag_BinaryTag_KeepsBytesAndRejectsUtf8Decode()
    {
        // Act
        var tag = (SCExecutableTag)SCVal.FromXdrBase64(ScvExecutableTagBinary);

        // Assert
        CollectionAssert.AreEqual(BinaryTag, tag.InnerValue);
        Assert.IsFalse(tag.TryGetUtf8String(out var text));
        Assert.IsNull(text);
        Assert.AreEqual(ScvExecutableTagBinary, tag.ToXdrBase64());
        Assert.AreEqual(ScvExecutableTagBinary, new SCExecutableTag(BinaryTag).ToXdrBase64());
    }

    /// <summary>
    ///     A non-ASCII text tag is encoded as UTF-8, matching the reference SDK.
    /// </summary>
    [TestMethod]
    public void SCExecutableTag_NonAsciiTextTag_MatchesKnownAnswerVector()
    {
        // Arrange
        var expected = ((SCExecutableTag)SCVal.FromXdrBase64(ScvExecutableTagNonAscii)).InnerValue;

        // Act & Assert
        Assert.AreEqual(ScvExecutableTagNonAscii, new SCExecutableTag(NonAsciiTag).ToXdrBase64());
        CollectionAssert.AreEqual(expected,
            new ContractExecutableExternalRef(new ScContractId(Owner), NonAsciiTag).Tag);
        var operation = CreateContractOperation.FromExternalRef(Owner, NonAsciiTag, Deployer, salt: Salt);
        CollectionAssert.AreEqual(expected,
            ((ContractExecutableExternalRef)operation.HostFunction.Executable).Tag);
    }

    /// <summary>
    ///     A string holding an unpaired surrogate has no UTF-8 encoding. Every text-tag overload rejects it instead of
    ///     silently encoding U+FFFD, which would name a different tag. (The strings are built here rather than passed
    ///     as <c>DataRow</c>s, because MSTest's test-data serialization replaces a lone surrogate with U+FFFD.)
    /// </summary>
    [TestMethod]
    public void TextTagOverloads_UnpairedSurrogate_ThrowArgumentException()
    {
        // Arrange & Act & Assert
        foreach (var tag in new[] { "a" + (char)0xD800 + "b", ((char)0xDC00).ToString() })
        {
            Assert.AreEqual("tag",
                Assert.ThrowsException<ArgumentException>(() => new SCExecutableTag(tag)).ParamName);
            Assert.AreEqual("tag", Assert.ThrowsException<ArgumentException>(() =>
                new ContractExecutableExternalRef(new ScContractId(Owner), tag)).ParamName);
            Assert.AreEqual("tag", Assert.ThrowsException<ArgumentException>(() =>
                CreateContractOperation.FromExternalRef(Owner, tag, Deployer)).ParamName);
        }
    }

    /// <summary>
    ///     The tag bytes are copied in and out, so changing the caller's array, the array a getter returns, or the XDR
    ///     object built from a tag never changes an already-built tag, reference, or operation.
    /// </summary>
    [TestMethod]
    public void TagBytes_AreCopied_NotAliased()
    {
        // Arrange
        var source = (byte[])BinaryTag.Clone();
        var tag = new SCExecutableTag(source);
        var externalRef = new ContractExecutableExternalRef(new ScContractId(Owner), source);
        var operation = CreateContractOperation.FromExternalRef(Owner, source, Deployer, [new SCUint32(7)], Salt);
        var exception = new ExternalRefNotFoundException(Owner, source, false);

        // Act
        source[0] = 0x00;
        tag.ToXdr().InnerBytes[1] = 0x00;
        externalRef.ToXdr().ExternalRef.Tag.InnerBytes[1] = 0x00;
        tag.InnerValue[3] = 0x00;
        externalRef.Tag[3] = 0x00;
        ((ContractExecutableExternalRef)operation.HostFunction.Executable).Tag[3] = 0x00;

        // Assert
        CollectionAssert.AreEqual(BinaryTag, tag.InnerValue);
        CollectionAssert.AreEqual(BinaryTag, externalRef.Tag);
        CollectionAssert.AreEqual(BinaryTag, exception.Tag);
        Assert.AreEqual(CreateContractFromExternalRefOpBinary, operation.ToXdrBase64());
    }

    /// <summary>
    ///     Byte sequences that .NET's default decoder would silently repair (lone surrogate halves encoded as
    ///     CESU-8, overlong encodings) are not valid UTF-8 and must be rejected by the strict accessor.
    /// </summary>
    [TestMethod]
    [DataRow(new byte[] { 0xed, 0xa0, 0x80 })]
    [DataRow(new byte[] { 0xc0, 0xaf })]
    [DataRow(new byte[] { 0xe2, 0x82 })]
    public void SCExecutableTag_TryGetUtf8String_RejectsMalformedSequences(byte[] bytes)
    {
        // Act & Assert
        Assert.IsFalse(new SCExecutableTag(bytes).TryGetUtf8String(out var text));
        Assert.IsNull(text);
    }

#if !TEST_SDK_NETSTANDARD21
    /// <summary>
    ///     Binary tags are the case the strict UTF-8 accessors exist for, so on .NET 8 and later they detect invalid
    ///     UTF-8 without throwing: a first-chance exception per call is slow in a loop and noisy in a debugger. (The
    ///     netstandard2.1 build has no <c>Utf8.IsValid</c> and still catches the decoder's exception.)
    /// </summary>
    [TestMethod]
    public void TryGetUtf8String_BinaryTag_RaisesNoFirstChanceException()
    {
        // Arrange
        var tag = new SCExecutableTag(BinaryTag);
        var externalRef = new ContractExecutableExternalRef(new ScContractId(Owner), BinaryTag);
        var threadId = Environment.CurrentManagedThreadId;
        var raised = 0;

        void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
        {
            if (Environment.CurrentManagedThreadId == threadId && e.Exception is DecoderFallbackException)
            {
                raised++;
            }
        }

        bool isTagText;
        bool isExternalRefTagText;
        AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
        try
        {
            // Act
            isTagText = tag.TryGetUtf8String(out _);
            isExternalRefTagText = externalRef.TryGetTagUtf8String(out _);
        }
        finally
        {
            AppDomain.CurrentDomain.FirstChanceException -= OnFirstChanceException;
        }

        // Assert
        Assert.IsFalse(isTagText);
        Assert.IsFalse(isExternalRefTagText);
        Assert.AreEqual(0, raised);
    }
#endif

    /// <summary>
    ///     An empty tag is valid XDR and valid (empty) UTF-8.
    /// </summary>
    [TestMethod]
    public void SCExecutableTag_EmptyTag_RoundTrips()
    {
        // Act
        var tag = (SCExecutableTag)SCVal.FromXdrBase64(new SCExecutableTag([]).ToXdrBase64());

        // Assert
        Assert.AreEqual(0, tag.InnerValue.Length);
        Assert.IsTrue(tag.TryGetUtf8String(out var text));
        Assert.AreEqual("", text);
    }

    /// <summary>
    ///     Both constructors reject null.
    /// </summary>
    [TestMethod]
    public void SCExecutableTag_NullTag_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => new SCExecutableTag((byte[])null!));
        Assert.ThrowsException<ArgumentNullException>(() => new SCExecutableTag((string)null!));
    }

    /// <summary>
    ///     <see cref="SCExecutableTag.FromSCValXdr" /> rejects other SCVal types.
    /// </summary>
    [TestMethod]
    public void SCExecutableTag_FromSCValXdrWithOtherType_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            SCExecutableTag.FromSCValXdr(new SCString(TextTag).ToSCValXdr()));
    }

    // ---------------------------------------------------------------------------------------------
    // SDK layer: ContractExecutableExternalRef
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    ///     A text-tag external reference decodes through <see cref="ContractExecutable.FromXdr" /> and re-encodes to
    ///     the reference SDK's bytes.
    /// </summary>
    [TestMethod]
    public void ContractExecutableExternalRef_TextTag_MatchesKnownAnswerVector()
    {
        // Act
        var executable = ContractExecutable.FromXdr(Decode(ContractExecutableExternalRefText,
            xdrSDK.ContractExecutable.Decode));

        // Assert
        var externalRef = (ContractExecutableExternalRef)executable;
        Assert.AreEqual(Owner, ((ScContractId)externalRef.ExecutableOwner).InnerValue);
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(TextTag), externalRef.Tag);
        Assert.IsTrue(externalRef.TryGetTagUtf8String(out var text));
        Assert.AreEqual(TextTag, text);
        AssertEncodes(ContractExecutableExternalRefText, externalRef);
        AssertEncodes(ContractExecutableExternalRefText,
            new ContractExecutableExternalRef(new ScContractId(Owner), TextTag));
    }

    /// <summary>
    ///     A binary-tag external reference keeps its tag bytes verbatim.
    /// </summary>
    [TestMethod]
    public void ContractExecutableExternalRef_BinaryTag_MatchesKnownAnswerVector()
    {
        // Act
        var externalRef = (ContractExecutableExternalRef)ContractExecutable.FromXdr(
            Decode(ContractExecutableExternalRefBinary, xdrSDK.ContractExecutable.Decode));

        // Assert
        CollectionAssert.AreEqual(BinaryTag, externalRef.Tag);
        Assert.IsFalse(externalRef.TryGetTagUtf8String(out var text));
        Assert.IsNull(text);
        AssertEncodes(ContractExecutableExternalRefBinary, externalRef);
        AssertEncodes(ContractExecutableExternalRefBinary,
            new ContractExecutableExternalRef(new ScContractId(Owner), BinaryTag));
    }

    /// <summary>
    ///     A contract instance whose executable is an external reference, as read from <c>getLedgerEntries</c>,
    ///     decodes and round-trips.
    /// </summary>
    [TestMethod]
    public void SCContractInstance_WithExternalRefExecutable_RoundTrips()
    {
        // Arrange
        var instance = new SCContractInstance(
            new ContractExecutableExternalRef(new ScContractId(Owner), BinaryTag), null);

        // Act
        var decoded = (SCContractInstance)SCVal.FromXdrBase64(instance.ToXdrBase64());

        // Assert
        var externalRef = (ContractExecutableExternalRef)decoded.Executable;
        CollectionAssert.AreEqual(BinaryTag, externalRef.Tag);
        Assert.AreEqual(instance.ToXdrBase64(), decoded.ToXdrBase64());
    }

    /// <summary>
    ///     Both constructors reject null arguments.
    /// </summary>
    [TestMethod]
    public void ContractExecutableExternalRef_NullArguments_ThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var owner = new ScContractId(Owner);
        Assert.ThrowsException<ArgumentNullException>(() => new ContractExecutableExternalRef(null!, BinaryTag));
        Assert.ThrowsException<ArgumentNullException>(() => new ContractExecutableExternalRef(owner, (byte[])null!));
        Assert.ThrowsException<ArgumentNullException>(() => new ContractExecutableExternalRef(owner, (string)null!));
    }

    private static void AssertEncodes(string expectedBase64, ContractExecutable executable)
    {
        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.ContractExecutable.Encode(stream, executable.ToXdr());
        Assert.AreEqual(expectedBase64, Convert.ToBase64String(stream.ToArray()));
    }

    // ---------------------------------------------------------------------------------------------
    // SDK layer: CreateContractOperation.FromExternalRef
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    ///     <see cref="CreateContractOperation.FromExternalRef(string, string, string, SCVal[], byte[], IAccountId)" />
    ///     builds the same operation as the reference SDK's <c>Operation.createCustomContract({ externalRef })</c>.
    /// </summary>
    [TestMethod]
    public void FromExternalRef_TextTag_MatchesKnownAnswerVector()
    {
        // Act
        var operation = CreateContractOperation.FromExternalRef(Owner, TextTag, Deployer, [new SCUint32(7)], Salt);

        // Assert
        Assert.AreEqual(CreateContractFromExternalRefOpText, operation.ToXdrBase64());
    }

    /// <summary>
    ///     The <c>byte[]</c> overload passes a binary tag through undecoded.
    /// </summary>
    [TestMethod]
    public void FromExternalRef_BinaryTag_MatchesKnownAnswerVector()
    {
        // Act
        var operation = CreateContractOperation.FromExternalRef(Owner, BinaryTag, Deployer, [new SCUint32(7)], Salt);

        // Assert
        Assert.AreEqual(CreateContractFromExternalRefOpBinary, operation.ToXdrBase64());
    }

    /// <summary>
    ///     A <c>CREATE_CONTRACT_V2</c> operation deploying from an external reference decodes back into a
    ///     <see cref="CreateContractOperation" /> and re-encodes to the same bytes, including through
    ///     <see cref="CreateContractV2HostFunction.FromXdr" />.
    /// </summary>
    [TestMethod]
    public void FromExternalRef_OperationXdr_RoundTrips()
    {
        // Arrange
        var xdrOperation = Decode(CreateContractFromExternalRefOpBinary, xdrSDK.Operation.Decode);

        // Act
        var operation = (CreateContractOperation)Operation.FromXdr(xdrOperation);
        var hostFunction =
            CreateContractV2HostFunction.FromXdr(xdrOperation.Body.InvokeHostFunctionOp.HostFunction.CreateContractV2);

        // Assert
        var externalRef = (ContractExecutableExternalRef)operation.HostFunction.Executable;
        CollectionAssert.AreEqual(BinaryTag, externalRef.Tag);
        Assert.AreEqual(Owner, ((ScContractId)externalRef.ExecutableOwner).InnerValue);
        Assert.AreEqual(CreateContractFromExternalRefOpBinary, operation.ToXdrBase64());
        CollectionAssert.AreEqual(BinaryTag, ((ContractExecutableExternalRef)hostFunction.Executable).Tag);
        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.CreateContractArgsV2.Encode(stream, hostFunction.ToXdr());
        var original = new xdrSDK.XdrDataOutputStream();
        xdrSDK.CreateContractArgsV2.Encode(original,
            xdrOperation.Body.InvokeHostFunctionOp.HostFunction.CreateContractV2);
        CollectionAssert.AreEqual(original.ToArray(), stream.ToArray());
    }

    /// <summary>
    ///     Only a contract can hold the tag entry, so a non-contract owner is rejected before the operation is built.
    /// </summary>
    [TestMethod]
    [DataRow(Deployer)]
    [DataRow("MAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSAAAAAAAAAAE2LP26")]
    [DataRow("not-an-address")]
    [DataRow(null)]
    public void FromExternalRef_NonContractOwner_ThrowsArgumentException(string? owner)
    {
        // Act & Assert
        var exception = Assert.ThrowsException<ArgumentException>(() =>
            CreateContractOperation.FromExternalRef(owner!, TextTag, Deployer));
        Assert.AreEqual("ownerContractId", exception.ParamName);
        exception = Assert.ThrowsException<ArgumentException>(() =>
            CreateContractOperation.FromExternalRef(owner!, BinaryTag, Deployer));
        Assert.AreEqual("ownerContractId", exception.ParamName);
    }

    /// <summary>
    ///     A null tag is rejected by both overloads, and is reported before the owner is checked.
    /// </summary>
    [TestMethod]
    [DataRow(Owner)]
    [DataRow(null)]
    public void FromExternalRef_NullTag_ThrowsArgumentNullException(string? owner)
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() =>
            CreateContractOperation.FromExternalRef(owner!, (string)null!, Deployer));
        Assert.ThrowsException<ArgumentNullException>(() =>
            CreateContractOperation.FromExternalRef(owner!, (byte[])null!, Deployer));
    }

    // ---------------------------------------------------------------------------------------------
    // SDK layer: StellarRpcServer.GetExternalRefWasmHash
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    ///     A live tag entry resolves to its 32-byte Wasm hash, hex-encoded like every other Wasm hash in the SDK, and
    ///     the request asks for exactly the persistent contract-data key the reference SDK builds for the tag, in a single
    ///     <c>getLedgerEntries</c> call.
    /// </summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task GetExternalRefWasmHash_LiveEntry_ReturnsWasmHash(bool binaryTag)
    {
        // Arrange
        var key = binaryTag ? ExternalRefLedgerKeyBinary : ExternalRefLedgerKeyText;
        var entry = binaryTag ? ExternalRefLedgerEntryDataBinary : ExternalRefLedgerEntryDataText;
        using var server = Utils.CreateTestStellarRpcServerCapturingRequest(out var handler,
            GetLedgerEntriesJson(entry, key, 1000, 1001));
        var externalRef = binaryTag
            ? new ContractExecutableExternalRef(new ScContractId(Owner), BinaryTag)
            : new ContractExecutableExternalRef(new ScContractId(Owner), TextTag);

        // Act
        var wasmHash = await server.GetExternalRefWasmHash(externalRef);

        // Assert
        Assert.AreEqual(Util.BytesToHex(WasmHash), wasmHash);
        CollectionAssert.AreEqual(WasmHash, Util.HexToBytes(wasmHash));
        StringAssert.Contains(handler.RequestBody, "\"method\":\"getLedgerEntries\"");
        StringAssert.Contains(handler.RequestBody, $"\"keys\":[\"{key}\"]");
        Mock.Get(handler).Verify(h => h.Send(It.IsAny<System.Net.Http.HttpRequestMessage>()), Times.Once());
    }

    /// <summary>
    ///     A missing tag entry throws <see cref="ExternalRefNotFoundException" /> with <c>IsArchived</c> false.
    /// </summary>
    [TestMethod]
    public async Task GetExternalRefWasmHash_MissingEntry_ThrowsExternalRefNotFoundException()
    {
        // Arrange
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": { "entries": [], "latestLedger": 1000 }
            }
            """;
        using var server = Utils.CreateTestStellarRpcServerWithContent(json);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<ExternalRefNotFoundException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(Owner), TextTag)));

        // Assert
        Assert.IsFalse(exception.IsArchived);
        Assert.AreEqual(Owner, exception.OwnerContractId);
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(TextTag), exception.Tag);
        StringAssert.Contains(exception.Message, "was not found");
        StringAssert.Contains(exception.Message, "\"fleet-v1\"");
    }

    /// <summary>
    ///     A response with no <c>entries</c> member at all is treated as a missing entry.
    /// </summary>
    [TestMethod]
    public async Task GetExternalRefWasmHash_NoEntriesMember_ThrowsExternalRefNotFoundException()
    {
        // Arrange
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": { "latestLedger": 1000 }
            }
            """;
        using var server = Utils.CreateTestStellarRpcServerWithContent(json);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<ExternalRefNotFoundException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(Owner), TextTag)));

        // Assert
        Assert.IsFalse(exception.IsArchived);
    }

    /// <summary>
    ///     <c>getLedgerEntries</c> also returns archived persistent entries. One whose <c>liveUntilLedgerSeq</c> is at or
    ///     below the latest (last closed) ledger cannot be read by any transaction submitted now, because core treats
    ///     an entry as live only while its TTL reaches the ledger being applied. It throws
    ///     <see cref="ExternalRefNotFoundException" /> with <c>IsArchived</c> true, and a binary tag is rendered as hex
    ///     in the message. 0 is the placeholder Stellar RPC reports for an evicted entry.
    /// </summary>
    [TestMethod]
    [DataRow(1000L)]
    [DataRow(999L)]
    [DataRow(0L)]
    public async Task GetExternalRefWasmHash_ArchivedEntry_ThrowsExternalRefNotFoundException(long liveUntil)
    {
        // Arrange
        using var server = Utils.CreateTestStellarRpcServerWithContent(
            GetLedgerEntriesJson(ExternalRefLedgerEntryDataBinary, ExternalRefLedgerKeyBinary, 1000, liveUntil));

        // Act
        var exception = await Assert.ThrowsExceptionAsync<ExternalRefNotFoundException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(Owner), BinaryTag)));

        // Assert
        Assert.IsTrue(exception.IsArchived);
        CollectionAssert.AreEqual(BinaryTag, exception.Tag);
        StringAssert.Contains(exception.Message, "archived");
        StringAssert.Contains(exception.Message, "0xfffe00c328");
    }

    /// <summary>
    ///     A non-contract owner is rejected before any request is sent.
    /// </summary>
    [TestMethod]
    public async Task GetExternalRefWasmHash_NonContractOwner_ThrowsBeforeRequest()
    {
        // Arrange
        using var server = Utils.CreateTestStellarRpcServerCapturingRequest(out var handler,
            GetLedgerEntriesJson(ExternalRefLedgerEntryDataText, ExternalRefLedgerKeyText, 1000, 1001));

        // Act
        var exception = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScAccountId(Deployer), TextTag)));

        // Assert
        Assert.AreEqual("externalRef", exception.ParamName);
        Assert.IsNull(handler.RequestUri);
    }

    /// <summary>
    ///     Deploying from a reference and resolving one apply a single owner rule, so a non-contract owner fails with the
    ///     same message on both paths, and a later change to the rule cannot reach one path and miss the other.
    /// </summary>
    [TestMethod]
    public async Task FromExternalRefAndGetExternalRefWasmHash_NonContractOwner_ThrowSameMessage()
    {
        // Arrange
        using var server = Utils.CreateTestStellarRpcServerWithContent("{}");

        // Act
        var deploying = Assert.ThrowsException<ArgumentException>(() =>
            CreateContractOperation.FromExternalRef(Deployer, TextTag, Deployer));
        var resolving = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScAccountId(Deployer), TextTag)));

        // Assert
        Assert.AreEqual("ownerContractId", deploying.ParamName);
        Assert.AreEqual("externalRef", resolving.ParamName);
        Assert.AreEqual(deploying.Message.Replace("'ownerContractId'", "'externalRef'"), resolving.Message);
    }

    /// <summary>
    ///     A null reference is rejected.
    /// </summary>
    [TestMethod]
    public async Task GetExternalRefWasmHash_NullReference_ThrowsArgumentNullException()
    {
        // Arrange
        using var server = Utils.CreateTestStellarRpcServerWithContent("{}");

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => server.GetExternalRefWasmHash(null!));
    }

    /// <summary>
    ///     A tag entry that does not hold a 32-byte hash cannot exist on a conforming network (the host validates the
    ///     value on write), so it is reported as a protocol error rather than returned. Values one byte short of and one
    ///     byte past a hash are both rejected.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(31)]
    [DataRow(33)]
    public async Task GetExternalRefWasmHash_EntryWithoutWasmHash_ThrowsClientProtocolException(int valueLength)
    {
        // Arrange
        var entry = BuildContractDataEntryXdr(Owner, new SCExecutableTag(TextTag),
            xdrSDK.ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT, new SCBytes(new byte[valueLength]));
        using var server = Utils.CreateTestStellarRpcServerWithContent(
            GetLedgerEntriesJson(entry, ExternalRefLedgerKeyText, 1000, 1001));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ClientProtocolException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(Owner), TextTag)));
    }

    /// <summary>
    ///     Liveness is judged against <c>latestLedger</c>; a response without it is non-conforming, and an archived
    ///     entry must not be reported as live because the comparison had nothing to compare against.
    /// </summary>
    [TestMethod]
    public async Task GetExternalRefWasmHash_NoLatestLedger_ThrowsClientProtocolException()
    {
        // Arrange
        using var server = Utils.CreateTestStellarRpcServerWithContent(
            GetLedgerEntriesJson(ExternalRefLedgerEntryDataText, ExternalRefLedgerKeyText, null, 5));

        // Act
        var exception = await Assert.ThrowsExceptionAsync<ClientProtocolException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(Owner), TextTag)));

        // Assert
        StringAssert.Contains(exception.Message, "latestLedger");
    }

    /// <summary>
    ///     Only the requested entry may be resolved: a contract-data entry holding a 32-byte value under another
    ///     contract, another key, or temporary durability is a non-conforming answer, not the reference's Wasm hash.
    /// </summary>
    [TestMethod]
    [DataRow("contract")]
    [DataRow("key")]
    [DataRow("tag")]
    [DataRow("durability")]
    public async Task GetExternalRefWasmHash_EntryForAnotherKey_ThrowsClientProtocolException(string mismatch)
    {
        // Arrange
        var otherContract = StrKey.EncodeContractId(new byte[32]);
        var entry = BuildContractDataEntryXdr(
            mismatch == "contract" ? otherContract : Owner,
            mismatch switch
            {
                "key" => new SCSymbol("fleet"),
                "tag" => new SCExecutableTag("fleet-v2"),
                _ => new SCExecutableTag(TextTag),
            },
            mismatch == "durability"
                ? xdrSDK.ContractDataDurability.ContractDataDurabilityEnum.TEMPORARY
                : xdrSDK.ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT,
            new SCBytes(WasmHash));
        using var server = Utils.CreateTestStellarRpcServerWithContent(
            GetLedgerEntriesJson(entry, ExternalRefLedgerKeyText, 1000, 1001));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ClientProtocolException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(Owner), TextTag)));
    }

    /// <summary>
    ///     One key is requested, so a response with more than one entry is non-conforming even when the first one is
    ///     the requested entry.
    /// </summary>
    [TestMethod]
    public async Task GetExternalRefWasmHash_MoreThanOneEntry_ThrowsClientProtocolException()
    {
        // Arrange
        var json = $$"""
                     {
                       "jsonrpc": "2.0",
                       "id": "8675309",
                       "result": {
                         "entries": [
                           { "key": "{{ExternalRefLedgerKeyText}}", "xdr": "{{ExternalRefLedgerEntryDataText}}", "liveUntilLedgerSeq": 1001 },
                           { "key": "{{ExternalRefLedgerKeyBinary}}", "xdr": "{{ExternalRefLedgerEntryDataBinary}}", "liveUntilLedgerSeq": 1001 }
                         ],
                         "latestLedger": 1000
                       }
                     }
                     """;
        using var server = Utils.CreateTestStellarRpcServerWithContent(json);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ClientProtocolException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(Owner), TextTag)));
    }

    /// <summary>
    ///     Entry XDR that cannot be decoded (invalid base64, truncated, an unknown discriminant) is reported as
    ///     <see cref="ClientProtocolException" /> with the decode failure as the inner exception, rather than as a raw
    ///     decoder exception the method does not document.
    /// </summary>
    [TestMethod]
    [DataRow("!!!notbase64")]
    [DataRow("AAAABg==")]
    [DataRow("AAAAYw==")]
    public async Task GetExternalRefWasmHash_UndecodableEntry_ThrowsClientProtocolException(string entryXdr)
    {
        // Arrange
        using var server = Utils.CreateTestStellarRpcServerWithContent(
            GetLedgerEntriesJson(entryXdr, ExternalRefLedgerKeyText, 1000, 1001));

        // Act
        var exception = await Assert.ThrowsExceptionAsync<ClientProtocolException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(Owner), TextTag)));

        // Assert
        Assert.IsNotNull(exception.InnerException);
    }

    /// <summary>
    ///     A lowercase contract ID is a valid StrKey for the same contract, so it resolves the entry the server
    ///     returns under the canonical (uppercase) ID.
    /// </summary>
    [TestMethod]
    public async Task GetExternalRefWasmHash_LowercaseOwner_ResolvesCanonicalEntry()
    {
        // Arrange
        using var server = Utils.CreateTestStellarRpcServerWithContent(
            GetLedgerEntriesJson(ExternalRefLedgerEntryDataText, ExternalRefLedgerKeyText, 1000, 1001));

        // Act
        var wasmHash = await server.GetExternalRefWasmHash(
            new ContractExecutableExternalRef(new ScContractId(Owner.ToLowerInvariant()), TextTag));

        // Assert
        Assert.AreEqual(Util.BytesToHex(WasmHash), wasmHash);
    }

    /// <summary>
    ///     A null element in <c>entries</c> does not match the response schema, so the response fails to deserialize
    ///     with <see cref="System.Text.Json.JsonException" />, as for every other <c>getLedgerEntries</c> caller,
    ///     rather than surfacing as a raw <see cref="NullReferenceException" />.
    /// </summary>
    [TestMethod]
    public async Task GetExternalRefWasmHash_NullEntryElement_ThrowsJsonException()
    {
        // Arrange
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": { "entries": [null], "latestLedger": 1000 }
            }
            """;
        using var server = Utils.CreateTestStellarRpcServerWithContent(json);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<System.Text.Json.JsonException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(Owner), TextTag)));
    }

    /// <summary>
    ///     A non-positive <c>latestLedger</c> is not a ledger sequence, so liveness cannot be judged against it.
    /// </summary>
    [TestMethod]
    [DataRow(0L)]
    [DataRow(-1L)]
    public async Task GetExternalRefWasmHash_NonPositiveLatestLedger_ThrowsClientProtocolException(long latestLedger)
    {
        // Arrange
        using var server = Utils.CreateTestStellarRpcServerWithContent(
            GetLedgerEntriesJson(ExternalRefLedgerEntryDataText, ExternalRefLedgerKeyText, latestLedger, 0));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ClientProtocolException>(() =>
            server.GetExternalRefWasmHash(new ContractExecutableExternalRef(new ScContractId(Owner), TextTag)));
    }

    /// <summary>
    ///     The exception message bounds an unbounded tag and renders as hex any tag holding a character that could
    ///     disguise where it ends or what the message says, so a tag cannot bloat or forge log lines.
    /// </summary>
    [TestMethod]
    public void ExternalRefNotFoundException_Message_IsBoundedAndSafe()
    {
        // Act & Assert
        var longTag = Encoding.UTF8.GetBytes(new string('a', 500));
        var longMessage = new ExternalRefNotFoundException(Owner, longTag, false).Message;
        StringAssert.Contains(longMessage, new string('a', 64) + "\"... (500 bytes)");
        Assert.IsFalse(longMessage.Contains(new string('a', 65)));

        var forged = new ExternalRefNotFoundException(Owner, Encoding.UTF8.GetBytes("v1\nERROR forged"), true).Message;
        Assert.IsFalse(forged.Contains('\n'));
        StringAssert.Contains(forged, "0x76310a4552524f5220666f72676564");

        StringAssert.Contains(new ExternalRefNotFoundException(Owner, Encoding.UTF8.GetBytes("fleet-\u00e9"), false)
            .Message, "0x666c6565742dc3a9");
    }

    /// <summary>
    ///     Anything but printable ASCII can forge or hide message content — a quote that closes the tag early, its
    ///     look-alikes, a bidi override, line and paragraph separators, zero-width, combining and invisible tag
    ///     characters (including ones outside the Basic Multilingual Plane), private-use characters, a backslash —
    ///     so such a tag renders as hex.
    /// </summary>
    [TestMethod]
    [DataRow("a\"; Owner: CEVIL, tag: \"b")]
    [DataRow("v1\uFF02. Owner: CFAKE, tag: \uFF02v2")]
    [DataRow("\u0338fleet")]
    [DataRow("abc\U000E0041\U000E0042")]
    [DataRow("ab\U000F0000cd")]
    [DataRow("ab\U0001D173cd")]
    [DataRow("del\u007F")]
    [DataRow("us\u001F")]
    [DataRow("abc\u202Eevil")]
    [DataRow("line1\u2028Owner: FAKE")]
    [DataRow("para\u2029x")]
    [DataRow("zw\u200Bsp")]
    [DataRow("back\\slash")]
    public void ExternalRefNotFoundException_Message_RendersDisguisingCharactersAsHex(string tag)
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes(tag);

        // Act
        var message = new ExternalRefNotFoundException(Owner, bytes, false).Message;

        // Assert
        StringAssert.Contains(message, "0x" + Util.BytesToHex(bytes).ToLowerInvariant());
        Assert.IsFalse(message.Contains(tag));
    }

    /// <summary>
    ///     The constructor is public, so an owner string gets the same treatment as a tag: anything but printable ASCII
    ///     other than <c>"</c> and <c>\</c> is rendered as hex.
    /// </summary>
    [TestMethod]
    [DataRow("C\nFAKE line", "0x430a46414b45206c696e65")]
    [DataRow("C\"x", "0x432278")]
    [DataRow("C\\x", "0x435c78")]
    [DataRow("C1\", tag: \"x\". Owner: \"C2", "0x4331222c207461673a202278222e204f776e65723a20224332")]
    [DataRow("C\u00e9", "0x43c3a9")]
    public void ExternalRefNotFoundException_Message_RendersUnsafeOwnerAsHex(string owner, string expectedHex)
    {
        // Act
        var message = new ExternalRefNotFoundException(owner, [0x61], false).Message;

        // Assert
        Assert.IsFalse(message.Contains('\n'));
        StringAssert.Contains(message, $"Owner: {expectedHex}, tag: \"a\".");
    }

    /// <summary>
    ///     A StrKey owner, in either case, is shown as quoted text, and a null owner or tag as <c>null</c>. The quotes
    ///     keep an owner string that reads <c>null</c>, is empty, or looks like hex distinct from those renderings.
    /// </summary>
    [TestMethod]
    public void ExternalRefNotFoundException_Message_RendersOwnerAsQuotedTextAndNullsAsNull()
    {
        // Act
        var canonical = new ExternalRefNotFoundException(Owner, [0x61], false).Message;
        var lowercase = new ExternalRefNotFoundException(Owner.ToLowerInvariant(), [0x61], false).Message;
        var nulls = new ExternalRefNotFoundException(null, null, false);
        var nullText = new ExternalRefNotFoundException("null", [0x61], false).Message;
        var empty = new ExternalRefNotFoundException("", [0x61], false).Message;
        var hexLike = new ExternalRefNotFoundException("0x61", [0x61], false).Message;
        var hexOfA = new ExternalRefNotFoundException("a.", [0x61], false).Message;

        // Assert
        StringAssert.Contains(canonical, $"Owner: \"{Owner}\", tag: \"a\".");
        StringAssert.Contains(lowercase, $"Owner: \"{Owner.ToLowerInvariant()}\", tag: \"a\".");
        StringAssert.Contains(nulls.Message, "Owner: null, tag: null.");
        Assert.IsNull(nulls.OwnerContractId);
        Assert.IsNull(nulls.Tag);
        StringAssert.Contains(nullText, "Owner: \"null\", tag:");
        StringAssert.Contains(empty, "Owner: \"\", tag:");
        StringAssert.Contains(hexLike, "Owner: \"0x61\", tag:");
        StringAssert.Contains(hexOfA, "Owner: \"a.\", tag:");
    }

    /// <summary>
    ///     An owner string is bounded like a tag: at most 64 bytes are shown, followed by the full length.
    /// </summary>
    [TestMethod]
    public void ExternalRefNotFoundException_Message_BoundsLongOwner()
    {
        // Arrange
        var owner = new string('C', 5000);

        // Act
        var message = new ExternalRefNotFoundException(owner, [0x61], false).Message;

        // Assert
        StringAssert.Contains(message, $"Owner: \"{new string('C', 64)}\"... (5000 bytes), tag: \"a\".");
        Assert.IsTrue(message.Length < 200, message);
    }

    /// <summary>
    ///     A tag of exactly 64 bytes is shown in full with no truncation marker; one more byte truncates it.
    /// </summary>
    [TestMethod]
    public void ExternalRefNotFoundException_Message_TruncatesOnlyPastSixtyFourBytes()
    {
        // Act
        var exact = new ExternalRefNotFoundException(Owner, Encoding.ASCII.GetBytes(new string('a', 64)), false)
            .Message;
        var over = new ExternalRefNotFoundException(Owner, Encoding.ASCII.GetBytes(new string('a', 65)), false)
            .Message;

        // Assert
        StringAssert.Contains(exact, $"tag: \"{new string('a', 64)}\".");
        Assert.IsFalse(exact.Contains("..."));
        StringAssert.Contains(over, $"tag: \"{new string('a', 64)}\"... (65 bytes).");
    }

    /// <summary>
    ///     A tag whose shown prefix is printable ASCII but whose remainder is not is rendered as hex, so the decision
    ///     covers every byte of the tag and not only the bytes shown.
    /// </summary>
    [TestMethod]
    public void ExternalRefNotFoundException_Message_JudgesWholeTagNotJustShownPrefix()
    {
        // Arrange
        var tag = Encoding.ASCII.GetBytes(new string('a', 64)).Concat(new byte[] { 0x0a }).ToArray();

        // Act
        var message = new ExternalRefNotFoundException(Owner, tag, false).Message;

        // Assert
        StringAssert.Contains(message, $"tag: 0x{string.Concat(Enumerable.Repeat("61", 64))}... (65 bytes).");
        Assert.IsFalse(message.Contains("tag: \""));
    }

    /// <summary>
    ///     Printable ASCII, from space to <c>~</c>, renders as text.
    /// </summary>
    [TestMethod]
    public void ExternalRefNotFoundException_Message_RendersPrintableAsciiBoundsAsText()
    {
        // Act
        var message = new ExternalRefNotFoundException(Owner, Encoding.ASCII.GetBytes(" a b~"), false).Message;

        // Assert
        StringAssert.Contains(message, "tag: \" a b~\".");
    }
}
