using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Transactions;

/// <summary>
///     LedgerBounds are Preconditions of a transaction per
///     <a href="https://github.com/stellar/stellar-protocol/blob/master/core/cap-0021.md#specification">CAP-21</a>
/// </summary>
public class LedgerBounds
{
    public LedgerBounds(uint minLedger, uint maxLedger)
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