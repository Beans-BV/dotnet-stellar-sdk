using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Xdr;
using SCBytes = StellarDotnetSdk.Soroban.SCBytes;
using ScContractId = StellarDotnetSdk.Soroban.ScContractId;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using XdrSCVal = StellarDotnetSdk.Xdr.SCVal;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for the derived properties of <see cref="TransactionInfo" /> that decode
///     <see cref="TransactionInfo.ResultMetaXdr" />: <c>ResultValue</c>, <c>WasmHash</c> and <c>CreatedContractId</c>.
/// </summary>
[TestClass]
public class TransactionInfoTest
{
    private const string ContractId = "CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB";

    private const string HashHex = "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F";

    private const string AccountId = "GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ";

    /// <summary>
    ///     A TransactionMeta V4 carrying an SCV_BYTES return value whose XDR padding bytes are non-zero.
    ///     The stream rejects this with an <c>IOException</c>, which is wire corruption rather than an SDK gap.
    /// </summary>
    private const string NonZeroPaddingMetaXdrBase64 =
        "AAAABAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAEAAAANAAAAAQH///8AAAAAAAAAAA==";

    /// <summary>
    ///     A TransactionMeta V4 carrying an SCV_SYMBOL return value whose length prefix (0xFFFFFFFF) overflows a
    ///     signed index. The stream rejects this with an <c>ArgumentOutOfRangeException</c>.
    /// </summary>
    private const string OversizedLengthMetaXdrBase64 =
        "AAAABAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAEAAAAP/////wAAAAAAAAAA";

    /// <summary>
    ///     A TransactionMeta whose union discriminant (21) is not a valid TransactionMeta version.
    /// </summary>
    private const string CorruptMetaXdrBase64 = "AAAAFQ==";

    /// <summary>
    ///     A TransactionMeta cut off after the V3 union discriminant.
    /// </summary>
    private const string TruncatedMetaXdrBase64 = "AAAAAw==";

    private static readonly byte[] Hash = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();

    private static string ToBase64(TransactionMeta meta)
    {
        var stream = new XdrDataOutputStream();
        TransactionMeta.Encode(stream, meta);
        return Convert.ToBase64String(stream.ToArray());
    }

    /// <summary>
    ///     Builds a base64-encoded TransactionMeta V3 (pre-Protocol 23) holding the given Soroban return value,
    ///     or one without Soroban metadata when <paramref name="returnValue" /> is null.
    /// </summary>
    private static string V3MetaXdrBase64(SCVal? returnValue)
    {
        return V3MetaXdrBase64Raw(returnValue?.ToXdr());
    }

    /// <summary>
    ///     As <see cref="V3MetaXdrBase64(SCVal)" />, but taking the return value as raw XDR so that shapes the SDK's
    ///     own <see cref="SCVal" /> types cannot express — such as a bodyless <c>SCV_VEC</c> — can be built.
    /// </summary>
    private static string V3MetaXdrBase64Raw(XdrSCVal? returnValue)
    {
        return ToBase64(new TransactionMeta
        {
            Discriminant = 3,
            V3 = new TransactionMetaV3
            {
                Ext = new ExtensionPoint { Discriminant = 0 },
                TxChangesBefore = new LedgerEntryChanges([]),
                Operations = [],
                TxChangesAfter = new LedgerEntryChanges([]),
                SorobanMeta = returnValue == null
                    ? null
                    : new SorobanTransactionMeta
                    {
                        Ext = new SorobanTransactionMetaExt { Discriminant = 0 },
                        Events = [],
                        ReturnValue = returnValue,
                        DiagnosticEvents = [],
                    },
            },
        });
    }

    /// <summary>
    ///     Builds a base64-encoded TransactionMeta V4 (Protocol 23+). Unlike V3, both the Soroban metadata and the
    ///     return value inside it are independently optional in the XDR schema (<c>SorobanTransactionMetaV2*</c> and
    ///     <c>SCVal* returnValue</c>), so both absences are representable here.
    /// </summary>
    /// <param name="returnValue">The Soroban return value, or null to omit the optional <c>returnValue</c> pointer.</param>
    /// <param name="includeSorobanMeta">False to omit the optional <c>sorobanMeta</c> pointer entirely.</param>
    /// <param name="txChangesAfter">Ledger entry changes to place in <c>txChangesAfter</c>, if any.</param>
    private static string V4MetaXdrBase64(
        SCVal? returnValue,
        bool includeSorobanMeta = true,
        LedgerEntryChange[]? txChangesAfter = null)
    {
        return V4MetaXdrBase64Raw(returnValue?.ToXdr(), includeSorobanMeta, txChangesAfter);
    }

    /// <summary>
    ///     As <see cref="V4MetaXdrBase64" />, but taking the return value as raw XDR so that shapes the SDK's own
    ///     <see cref="SCVal" /> types cannot express — such as a bodyless <c>SCV_MAP</c> — can be built.
    /// </summary>
    private static string V4MetaXdrBase64Raw(
        XdrSCVal? returnValue,
        bool includeSorobanMeta = true,
        LedgerEntryChange[]? txChangesAfter = null)
    {
        return ToBase64(new TransactionMeta
        {
            Discriminant = 4,
            V4 = new TransactionMetaV4
            {
                Ext = new ExtensionPoint { Discriminant = 0 },
                TxChangesBefore = new LedgerEntryChanges([]),
                Operations = [],
                TxChangesAfter = new LedgerEntryChanges(txChangesAfter ?? []),
                SorobanMeta = includeSorobanMeta
                    ? new SorobanTransactionMetaV2
                    {
                        Ext = new SorobanTransactionMetaExt { Discriminant = 0 },
                        ReturnValue = returnValue,
                    }
                    : null,
                Events = [],
                DiagnosticEvents = [],
            },
        });
    }

    /// <summary>
    ///     Builds an XDR <c>SCVal</c> whose optional body pointer is absent. <c>SCV_VEC</c> and <c>SCV_MAP</c> are the
    ///     only two arms of the union declared as XDR optionals (<c>SCVec *vec</c> / <c>SCMap *map</c>), so a
    ///     present-flag of <c>0</c> is well-formed on the wire and leaves the body null. The SDK's own
    ///     <see cref="SCVal" /> types always carry a body, so this shape can only be built from raw XDR.
    /// </summary>
    private static XdrSCVal BodylessXdrSCVal(SCValType.SCValTypeEnum type)
    {
        return new XdrSCVal { Discriminant = SCValType.Create(type) };
    }

    /// <summary>
    ///     Builds a base64-encoded classic TransactionMeta V0 — the original union arm, a bare array of operation
    ///     metadata. It decodes cleanly at the XDR layer but has no SDK wrapper type.
    /// </summary>
    private static string ClassicV0MetaXdrBase64()
    {
        return ToBase64(new TransactionMeta
        {
            Discriminant = 0,
            Operations = [],
        });
    }

    /// <summary>
    ///     Builds a base64-encoded classic TransactionMeta V1 — the shape a non-Soroban transaction from a
    ///     pre-Protocol 20 ledger carries. It decodes cleanly at the XDR layer but has no SDK wrapper type.
    /// </summary>
    private static string ClassicV1MetaXdrBase64()
    {
        return ToBase64(new TransactionMeta
        {
            Discriminant = 1,
            V1 = new TransactionMetaV1
            {
                TxChanges = new LedgerEntryChanges([]),
                Operations = [],
            },
        });
    }

    /// <summary>
    ///     Builds a base64-encoded classic TransactionMeta V2 — the Protocol 13-19 shape. Like V0 and V1 it decodes
    ///     cleanly at the XDR layer, predates Soroban, and has no SDK wrapper type.
    /// </summary>
    private static string ClassicV2MetaXdrBase64()
    {
        return ToBase64(new TransactionMeta
        {
            Discriminant = 2,
            V2 = new TransactionMetaV2
            {
                TxChangesBefore = new LedgerEntryChanges([]),
                Operations = [],
                TxChangesAfter = new LedgerEntryChanges([]),
            },
        });
    }

    /// <summary>
    ///     Builds a ledger entry change that decodes cleanly as XDR but that the SDK cannot map to a domain type:
    ///     a DataEntry whose name exceeds the 64-character limit <c>LedgerEntryData</c>'s constructor enforces.
    ///     The XDR decoder does not enforce <c>string64</c>'s bound, so only the SDK conversion rejects it.
    /// </summary>
    private static LedgerEntryChange UnconvertibleLedgerEntryChange()
    {
        var accountId = new AccountID(KeyPair.FromAccountId(AccountId).XdrPublicKey);
        return new LedgerEntryChange
        {
            Discriminant = LedgerEntryChangeType.Create(
                LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_STATE),
            State = new LedgerEntry
            {
                LastModifiedLedgerSeq = new Uint32(1),
                Ext = new LedgerEntry.LedgerEntryExt { Discriminant = 0 },
                Data = new LedgerEntry.LedgerEntryData
                {
                    Discriminant = LedgerEntryType.Create(LedgerEntryType.LedgerEntryTypeEnum.DATA),
                    Data = new DataEntry
                    {
                        AccountID = accountId,
                        DataName = new String64(new string('a', 70)),
                        DataValue = new DataValue([1, 2, 3]),
                        Ext = new DataEntry.DataEntryExt { Discriminant = 0 },
                    },
                },
            },
        };
    }

    private static TransactionInfo SuccessfulTransaction(string? resultMetaXdr)
    {
        return new TransactionInfo
        {
            Status = TransactionInfo.TransactionStatus.SUCCESS,
            ResultMetaXdr = resultMetaXdr,
        };
    }

    private static TransactionInfo FailedTransaction(string? resultMetaXdr)
    {
        return new TransactionInfo
        {
            Status = TransactionInfo.TransactionStatus.FAILED,
            ResultMetaXdr = resultMetaXdr,
        };
    }

    /// <summary>
    ///     Verifies that ResultValue reads the Soroban return value out of a TransactionMeta V3
    ///     (the format returned for transactions from pre-Protocol 23 ledgers).
    /// </summary>
    [TestMethod]
    public void ResultValue_WithV3Meta_ReturnsSorobanReturnValue()
    {
        // Arrange
        var transaction = SuccessfulTransaction(V3MetaXdrBase64(new SCBytes(Hash)));

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsInstanceOfType(resultValue, typeof(SCBytes));
        CollectionAssert.AreEqual(Hash, ((SCBytes)resultValue!).InnerValue);
    }

    /// <summary>
    ///     Verifies that ResultValue reads the Soroban return value out of a TransactionMeta V4
    ///     (the format returned for transactions from Protocol 23+ ledgers).
    /// </summary>
    [TestMethod]
    public void ResultValue_WithV4Meta_ReturnsSorobanReturnValue()
    {
        // Arrange
        var transaction = SuccessfulTransaction(V4MetaXdrBase64(new SCBytes(Hash)));

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsInstanceOfType(resultValue, typeof(SCBytes));
        CollectionAssert.AreEqual(Hash, ((SCBytes)resultValue!).InnerValue);
    }

    /// <summary>
    ///     Verifies that WasmHash hex-encodes an SCBytes return value carried in a TransactionMeta V3.
    /// </summary>
    [TestMethod]
    public void WasmHash_WithV3MetaBytesReturnValue_ReturnsHexEncodedHash()
    {
        // Arrange
        var transaction = SuccessfulTransaction(V3MetaXdrBase64(new SCBytes(Hash)));

        // Act
        var wasmHash = transaction.WasmHash;

        // Assert
        Assert.AreEqual(HashHex, wasmHash);
        Assert.IsNull(transaction.CreatedContractId);
    }

    /// <summary>
    ///     Verifies that WasmHash hex-encodes an SCBytes return value carried in a TransactionMeta V4.
    /// </summary>
    [TestMethod]
    public void WasmHash_WithV4MetaBytesReturnValue_ReturnsHexEncodedHash()
    {
        // Arrange
        var transaction = SuccessfulTransaction(V4MetaXdrBase64(new SCBytes(Hash)));

        // Act
        var wasmHash = transaction.WasmHash;

        // Assert
        Assert.AreEqual(HashHex, wasmHash);
        Assert.IsNull(transaction.CreatedContractId);
    }

    /// <summary>
    ///     Verifies that CreatedContractId returns the contract ID of an address return value carried in a
    ///     TransactionMeta V3.
    /// </summary>
    [TestMethod]
    public void CreatedContractId_WithV3MetaContractAddressReturnValue_ReturnsContractId()
    {
        // Arrange
        var transaction = SuccessfulTransaction(V3MetaXdrBase64(new ScContractId(ContractId)));

        // Act
        var createdContractId = transaction.CreatedContractId;

        // Assert
        Assert.AreEqual(ContractId, createdContractId);
        Assert.IsNull(transaction.WasmHash);
    }

    /// <summary>
    ///     Verifies that CreatedContractId returns the contract ID of an address return value carried in a
    ///     TransactionMeta V4.
    /// </summary>
    [TestMethod]
    public void CreatedContractId_WithV4MetaContractAddressReturnValue_ReturnsContractId()
    {
        // Arrange
        var transaction = SuccessfulTransaction(V4MetaXdrBase64(new ScContractId(ContractId)));

        // Act
        var createdContractId = transaction.CreatedContractId;

        // Assert
        Assert.AreEqual(ContractId, createdContractId);
        Assert.IsNull(transaction.WasmHash);
    }

    /// <summary>
    ///     Verifies that ResultValue is null for a TransactionMeta V3 without Soroban metadata
    ///     (a classic, non-Soroban transaction).
    /// </summary>
    [TestMethod]
    public void ResultValue_WithV3MetaWithoutSorobanMeta_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(V3MetaXdrBase64(null));

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
    }

    /// <summary>
    ///     Verifies that ResultValue is null for a TransactionMeta V4 without Soroban metadata — the V4 analogue of
    ///     <see cref="ResultValue_WithV3MetaWithoutSorobanMeta_ReturnsNull" />.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithV4MetaWithoutSorobanMeta_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(V4MetaXdrBase64(null, false));

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);
    }

    /// <summary>
    ///     Verifies that ResultValue is null for a TransactionMeta V4 whose Soroban metadata is present but whose
    ///     optional <c>returnValue</c> pointer is absent — a shape only V4 can express, since V3's return value is
    ///     a mandatory XDR field.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithV4MetaWithoutReturnValue_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(V4MetaXdrBase64(null));

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);
    }

    /// <summary>
    ///     Verifies that a Soroban return value still surfaces when an unrelated part of the same metadata cannot be
    ///     converted to an SDK type. Reading through the all-or-nothing <see cref="TransactionInfo.TransactionMeta" />
    ///     would discard the return value here (see issue #224).
    /// </summary>
    [TestMethod]
    public void ResultValue_WithUnconvertibleLedgerEntryChange_StillReturnsSorobanReturnValue()
    {
        // Arrange
        var transaction = SuccessfulTransaction(
            V4MetaXdrBase64(new SCBytes(Hash), txChangesAfter: [UnconvertibleLedgerEntryChange()]));

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsInstanceOfType(resultValue, typeof(SCBytes));
        CollectionAssert.AreEqual(Hash, ((SCBytes)resultValue!).InnerValue);
        Assert.AreEqual(HashHex, transaction.WasmHash);

        // The whole-metadata view cannot represent this payload, which is exactly why ResultValue must not
        // depend on it.
        Assert.IsNull(transaction.TransactionMeta);
    }

    /// <summary>
    ///     Verifies that ResultValue is null for a classic TransactionMeta V0, which decodes cleanly as XDR but
    ///     predates Soroban and so carries no return value.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithClassicV0Meta_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(ClassicV0MetaXdrBase64());

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);
    }

    /// <summary>
    ///     Verifies that ResultValue is null for a classic TransactionMeta V1, which decodes cleanly as XDR but
    ///     predates Soroban and so carries no return value.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithClassicV1Meta_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(ClassicV1MetaXdrBase64());

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);
    }

    /// <summary>
    ///     Verifies that ResultValue is null for a classic TransactionMeta V2, which decodes cleanly as XDR but
    ///     predates Soroban and so carries no return value.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithClassicV2Meta_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(ClassicV2MetaXdrBase64());

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);
    }

    /// <summary>
    ///     Verifies that ResultValue is null for a transaction that did not succeed, even when the metadata
    ///     holds a return value.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithFailedStatus_ReturnsNull()
    {
        // Arrange
        var transaction = FailedTransaction(V3MetaXdrBase64(new SCBytes(Hash)));

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
    }

    /// <summary>
    ///     Verifies that WasmHash respects the SUCCESS gate independently of ResultValue.
    /// </summary>
    [TestMethod]
    public void WasmHash_WithFailedStatus_ReturnsNull()
    {
        // Arrange
        var transaction = FailedTransaction(V3MetaXdrBase64(new SCBytes(Hash)));

        // Act
        var wasmHash = transaction.WasmHash;

        // Assert
        Assert.IsNull(wasmHash);
    }

    /// <summary>
    ///     Verifies that CreatedContractId respects the SUCCESS gate independently of ResultValue.
    /// </summary>
    [TestMethod]
    public void CreatedContractId_WithFailedStatus_ReturnsNull()
    {
        // Arrange
        var transaction = FailedTransaction(V3MetaXdrBase64(new ScContractId(ContractId)));

        // Act
        var createdContractId = transaction.CreatedContractId;

        // Assert
        Assert.IsNull(createdContractId);
    }

    /// <summary>
    ///     Verifies that the derived properties are null when the response carries no <c>resultMetaXdr</c> at all.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithMissingResultMetaXdr_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(null);

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);
        Assert.IsNull(transaction.TransactionMeta);
    }

    /// <summary>
    ///     Verifies that ResultValue is null — rather than throwing — when ResultMetaXdr carries an unknown
    ///     TransactionMeta version, matching the behavior of the TransactionMeta property.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithUnknownMetaVersion_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(CorruptMetaXdrBase64);

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
    }

    /// <summary>
    ///     Verifies that ResultValue is null — rather than throwing — when ResultMetaXdr is truncated.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithTruncatedMeta_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(TruncatedMetaXdrBase64);

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
    }

    /// <summary>
    ///     Verifies that ResultValue is null — rather than throwing — when ResultMetaXdr is not valid base64.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithInvalidBase64Meta_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction("not base64!");

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
    }

    /// <summary>
    ///     Verifies that ResultValue is null — rather than throwing — when the Soroban return value is an
    ///     <c>SCV_VEC</c> whose optional body pointer is absent. That shape is well-formed XDR, so it reaches
    ///     <c>SCVal.FromXdr</c>, which rejects it as unrepresentable with an <c>ArgumentException</c>. Both the
    ///     rejection and this property's handling of it must hold: without the guard in <c>SCVec.FromSCValXdr</c>
    ///     the SDK dereferences null, and without <c>ArgumentException</c> in this getter's filter — or with
    ///     <c>SCVal.FromXdr</c> left outside its <c>try</c> — the exception escapes a property getter.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithV4MetaBodylessVecReturnValue_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(
            V4MetaXdrBase64Raw(BodylessXdrSCVal(SCValType.SCValTypeEnum.SCV_VEC)));

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);

        // The whole-metadata view must agree rather than disagree about the same payload.
        Assert.IsNull(transaction.TransactionMeta);
    }

    /// <summary>
    ///     The <c>SCV_MAP</c> counterpart of
    ///     <see cref="ResultValue_WithV4MetaBodylessVecReturnValue_ReturnsNull" /> — the other XDR-optional arm of
    ///     the SCVal union.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithV4MetaBodylessMapReturnValue_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(
            V4MetaXdrBase64Raw(BodylessXdrSCVal(SCValType.SCValTypeEnum.SCV_MAP)));

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);
        Assert.IsNull(transaction.TransactionMeta);
    }

    /// <summary>
    ///     Verifies the same bodyless-<c>SCV_VEC</c> handling on a TransactionMeta V3, whose Soroban return value is
    ///     a mandatory XDR field and so reaches <c>SCVal.FromXdr</c> by a different route than V4's optional one.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithV3MetaBodylessVecReturnValue_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(
            V3MetaXdrBase64Raw(BodylessXdrSCVal(SCValType.SCValTypeEnum.SCV_VEC)));

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);
        Assert.IsNull(transaction.TransactionMeta);
    }

    /// <summary>
    ///     Verifies that ResultValue is null — rather than throwing — when the metadata's XDR padding bytes are
    ///     non-zero. The stream reports that as an <c>IOException</c>, which is corruption of the payload rather
    ///     than a gap in the SDK, so it must not escape the getter.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithNonZeroXdrPadding_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(NonZeroPaddingMetaXdrBase64);

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);
    }

    /// <summary>
    ///     Verifies that ResultValue is null — rather than throwing — when a length prefix in the metadata is large
    ///     enough to overflow a signed index. Four hostile bytes from an RPC server must not produce an
    ///     <c>ArgumentOutOfRangeException</c> out of a property getter.
    /// </summary>
    [TestMethod]
    public void ResultValue_WithOversizedLengthPrefix_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(OversizedLengthMetaXdrBase64);

        // Act
        var resultValue = transaction.ResultValue;

        // Assert
        Assert.IsNull(resultValue);
        Assert.IsNull(transaction.WasmHash);
        Assert.IsNull(transaction.CreatedContractId);
    }

    /// <summary>
    ///     Verifies that WasmHash is null — rather than throwing — when ResultMetaXdr cannot be decoded.
    /// </summary>
    [TestMethod]
    public void WasmHash_WithUnknownMetaVersion_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(CorruptMetaXdrBase64);

        // Act
        var wasmHash = transaction.WasmHash;

        // Assert
        Assert.IsNull(wasmHash);
    }

    /// <summary>
    ///     Verifies that CreatedContractId is null — rather than throwing — when ResultMetaXdr cannot be decoded.
    /// </summary>
    [TestMethod]
    public void CreatedContractId_WithUnknownMetaVersion_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(CorruptMetaXdrBase64);

        // Act
        var createdContractId = transaction.CreatedContractId;

        // Assert
        Assert.IsNull(createdContractId);
    }

    /// <summary>
    ///     Verifies that the TransactionMeta property is null — rather than throwing — when ResultMetaXdr carries
    ///     an unknown TransactionMeta version, the behavior ResultValue mirrors.
    /// </summary>
    [TestMethod]
    public void TransactionMeta_WithUnknownMetaVersion_ReturnsNull()
    {
        // Arrange
        var transaction = SuccessfulTransaction(CorruptMetaXdrBase64);

        // Act
        var meta = transaction.TransactionMeta;

        // Assert
        Assert.IsNull(meta);
    }

    /// <summary>
    ///     Pins the set of TransactionMeta union discriminants the generated XDR layer knows about. ResultValue
    ///     switches on that discriminant and reports "no return value" for anything outside {3, 4}, which is the
    ///     right answer only while 0-2 are the sole other arms. If a future protocol adds an arm, regenerating
    ///     StellarDotnetSdk.Xdr makes discriminant 5 decodable and this test fails, forcing ResultValue's switch
    ///     to be revisited instead of silently returning null for the new version (see issue #224).
    /// </summary>
    [TestMethod]
    public void TransactionMetaXdr_KnowsExactlyDiscriminantsZeroThroughFour()
    {
        for (var discriminant = 0; discriminant <= 4; discriminant++)
        {
            // Arrange: a payload holding only the discriminant, so decoding it runs out of input while reading
            // the arm's body rather than rejecting the discriminant itself.
            var stream = new XdrDataInputStream(BigEndian(discriminant));

            // Act & Assert
            Assert.ThrowsException<EndOfStreamException>(
                () => TransactionMeta.Decode(stream),
                $"Discriminant {discriminant} should be a known TransactionMeta arm.");
        }

        // Arrange
        var unknown = new XdrDataInputStream(BigEndian(5));

        // Act & Assert
        var exception = Assert.ThrowsException<InvalidDataException>(
            () => TransactionMeta.Decode(unknown),
            "Discriminant 5 is not a known TransactionMeta arm; if this now decodes, the XDR was regenerated " +
            "with a new metadata version and TransactionInfo.ResultValue must handle it explicitly.");
        StringAssert.Contains(exception.Message, "5");
        return;

        static byte[] BigEndian(int value) =>
            [(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value];
    }
}
