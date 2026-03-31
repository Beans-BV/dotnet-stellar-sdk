using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Transactions;

/// <summary>
///     Ledger bounds are a precondition of a transaction.
///     See:
///     <a
///         href="https://developers.stellar.org/docs/learn/fundamentals/stellar-data-structures/operations-and-transactions#ledger-bounds">
///         Ledger
///         bounds
///     </a>
/// </summary>
public class LedgerBounds
{
    private LedgerBounds(uint minLedger, uint maxLedger)
    {
        MinLedger = minLedger;
        MaxLedger = maxLedger;
    }

    /// <summary>
    ///     Gets the minimum ledger sequence number. The transaction is valid only at or after this ledger.
    ///     A value of 0 means no lower bound.
    /// </summary>
    public uint MinLedger { get; }

    /// <summary>
    ///     Gets the maximum ledger sequence number. The transaction is valid only at or before this ledger.
    ///     A value of 0 means no upper bound.
    /// </summary>
    public uint MaxLedger { get; }

    /// <summary>
    ///     Creates a <see cref="LedgerBounds" /> from its XDR representation.
    /// </summary>
    /// <param name="xdrLedgerBounds">The XDR ledger bounds object.</param>
    /// <returns>A new <see cref="LedgerBounds" /> instance.</returns>
    public static LedgerBounds FromXdr(Xdr.LedgerBounds xdrLedgerBounds)
    {
        return new LedgerBounds(xdrLedgerBounds.MinLedger.InnerValue, xdrLedgerBounds.MaxLedger.InnerValue);
    }

    /// <summary>
    ///     Converts this instance to its XDR <see cref="Xdr.LedgerBounds" /> representation.
    /// </summary>
    /// <returns>An XDR ledger bounds object.</returns>
    public Xdr.LedgerBounds ToXdr()
    {
        return new Xdr.LedgerBounds
        {
            MinLedger = new Uint32(MinLedger),
            MaxLedger = new Uint32(MaxLedger),
        };
    }
}