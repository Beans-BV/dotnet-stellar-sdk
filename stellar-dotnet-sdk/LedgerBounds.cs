using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

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

    public static LedgerBounds FromXdr(xdr.LedgerBounds xdrLedgerBounds)
    {
        return new LedgerBounds(xdrLedgerBounds.MinLedger.InnerValue, xdrLedgerBounds.MaxLedger.InnerValue);
    }

    public xdr.LedgerBounds ToXdr()
    {
        return new xdr.LedgerBounds
        {
            MinLedger = new Uint32 { InnerValue = MinLedger },
            MaxLedger = new Uint32 { InnerValue = MaxLedger }
        };
    }
}