using System;
using System.Linq;
using StellarDotnetSdk.Xdr;
using LedgerEntryChange = StellarDotnetSdk.LedgerEntries.LedgerEntryChange;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Transaction metadata V4.
/// </summary>
public class TransactionMetaV4
{
    /// <summary>
    ///     We can use this to add more fields.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; private set; } = new ExtensionPointZero();

    /// <summary>
    ///     Transaction level changes before operations are applied if any.
    /// </summary>
    public LedgerEntryChange[] TransactionChangesBefore { get; private set; } = [];

    /// <summary>
    ///     Transaction level changes after operations are applied if any.
    /// </summary>
    public LedgerEntryChange[] TransactionChangesAfter { get; private set; } = [];

    /// <summary>
    ///     Meta for each operation.
    /// </summary>
    public OperationMetaV2[] Operations { get; private set; } = [];

    /// <summary>
    ///  Transaction-level events (like fee payment)
    /// </summary>
    public TransactionEvent[] Events { get; private set; }

    /// <summary>
    ///     Diagnostics events that are not hashed. One list per operation.
    ///     This will contain all contract and diagnostic events. Even ones that were emitted in a failed contract call.
    /// </summary>
    public DiagnosticEvent[] DiagnosticEvents { get; private set; }

    /// <summary>
    ///     (Optional) Holds the Soroban transaction metadata.
    /// </summary>
    public SorobanTransactionMetaV2? SorobanMeta { get; private set; }

    /// <summary>
    ///     Creates the corresponding <c>TransactionMetaV3</c> object from an <c>xdr.TransactionMetaV3</c> object.
    /// </summary>
    /// <param name="xdrMetaV4">An <c>xdr.TransactionMetaV3</c> object to be converted.</param>
    /// <returns>A <c>TransactionMetaV3</c> object.</returns>
    private static TransactionMetaV4 FromXdr(Xdr.TransactionMetaV4 xdrMetaV4)
    {
        return new TransactionMetaV4
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdrMetaV4.Ext),
            TransactionChangesBefore = xdrMetaV4.TxChangesBefore.InnerValue
                .Select(LedgerEntryChange.FromXdr)
                .ToArray(),
            TransactionChangesAfter = xdrMetaV4.TxChangesAfter.InnerValue
                .Select(LedgerEntryChange.FromXdr)
                .ToArray(),
            Operations = xdrMetaV4.Operations
                .Select(OperationMetaV2.FromXdr)
                .ToArray(),
            SorobanMeta = SorobanTransactionMetaV2.FromXdr(xdrMetaV4.SorobanMeta),
            Events = xdrMetaV4.Events.Select(TransactionEvent.FromXdr).ToArray(),
            DiagnosticEvents = xdrMetaV4.DiagnosticEvents.Select(DiagnosticEvent.FromXdr).ToArray(),
        };
    }

    /// <summary>
    ///     Creates a new <c>TransactionMetaV4</c> object from the given
    ///     <see cref="Xdr.TransactionMetaV4">xdr.TransactionMetaV3</see> base-64 encoded XDR string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>A <c>TransactionMetaV4</c> object decoded and deserialized from the provided string.</returns>
    public static TransactionMetaV4 FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = TransactionMeta.Decode(reader);
        return FromXdr(thisXdr.V4);
    }
}