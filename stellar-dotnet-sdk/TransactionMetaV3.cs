using System;
using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Transaction metadata V3.
///     See: https://soroban.stellar.org/docs/soroban-internals/events#transactionmetav3
/// </summary>
public class TransactionMetaV3
{
    /// <summary>
    ///     We can use this to add more fields.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; private set; } = new ExtensionPointZero();

    /// <summary>
    ///     Transaction level changes before operations are applied if any.
    /// </summary>
    public LedgerEntryChange[] TransactionChangesBefore { get; private set; } = Array.Empty<LedgerEntryChange>();

    /// <summary>
    ///     Transaction level changes after operations are applied if any.
    /// </summary>
    public LedgerEntryChange[] TransactionChangesAfter { get; private set; } = Array.Empty<LedgerEntryChange>();

    /// <summary>
    ///     Meta for each operation.
    /// </summary>
    public LedgerEntryChange[][] Operations { get; private set; } =
        Array.Empty<LedgerEntryChange[]>(); // TODO Unit test with actual data

    /// <summary>
    ///     (Optional) Holds the Soroban transaction metadata.
    /// </summary>
    public SorobanTransactionMeta? SorobanMeta { get; private set; }

    /// <summary>
    ///     Creates the corresponding <c>TransactionMetaV3</c> object from an <c>xdr.TransactionMetaV3</c> object.
    /// </summary>
    /// <param name="xdrTransactionMetaV3">An <c>xdr.TransactionMetaV3</c> object to be converted.</param>
    /// <returns>A <c>TransactionMetaV3</c> object.</returns>
    private static TransactionMetaV3 FromXdr(xdr.TransactionMetaV3 xdrTransactionMetaV3)
    {
        return new TransactionMetaV3
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdrTransactionMetaV3.Ext),
            TransactionChangesBefore = xdrTransactionMetaV3.TxChangesBefore.InnerValue.Select(LedgerEntryChange.FromXdr)
                .ToArray(),
            TransactionChangesAfter = xdrTransactionMetaV3.TxChangesAfter.InnerValue.Select(LedgerEntryChange.FromXdr)
                .ToArray(),
            Operations = xdrTransactionMetaV3.Operations
                .Select(x => x.Changes.InnerValue.Select(LedgerEntryChange.FromXdr).ToArray())
                .ToArray(),
            SorobanMeta = SorobanTransactionMeta.FromXdr(xdrTransactionMetaV3.SorobanMeta)
        };
    }

    /// <summary>
    ///     Creates a new <c>TransactionMetaV3</c> object from the given
    ///     <see cref="xdr.TransactionMetaV3">xdr.TransactionMetaV3</see> base-64 encoded XDR string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>A <c>TransactionMetaV3</c> object decoded and deserialized from the provided string.</returns>
    public static TransactionMetaV3 FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = TransactionMeta.Decode(reader);
        return FromXdr(thisXdr.V3);
    }
}