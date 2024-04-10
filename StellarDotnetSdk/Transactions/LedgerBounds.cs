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

    public uint MinLedger { get; }

    public uint MaxLedger { get; }

    public static LedgerBounds FromXdr(Xdr.LedgerBounds xdrLedgerBounds)
    {
        return new LedgerBounds(xdrLedgerBounds.MinLedger.InnerValue, xdrLedgerBounds.MaxLedger.InnerValue);
    }

    public Xdr.LedgerBounds ToXdr()
    {
        return new Xdr.LedgerBounds
        {
            MinLedger = new Uint32(MinLedger),
            MaxLedger = new Uint32(MaxLedger)
        };
    }
}