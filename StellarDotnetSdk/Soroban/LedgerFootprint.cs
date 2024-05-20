using System.Linq;
using StellarDotnetSdk.LedgerKeys;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     See
///     <a
///         href="https://developers.stellar.org/docs/learn/smart-contract-internals/contract-interactions/transaction-simulation#footprint">
///         Footprint
///     </a>
/// </summary>
public class LedgerFootprint
{
    public LedgerKey[] ReadOnly { get; init; } = [];
    public LedgerKey[] ReadWrite { get; init; } = [];

    public Xdr.LedgerFootprint ToXdr()
    {
        return new Xdr.LedgerFootprint
        {
            ReadOnly = ReadOnly.Select(x => x.ToXdr()).ToArray(),
            ReadWrite = ReadWrite.Select(x => x.ToXdr()).ToArray()
        };
    }

    public static LedgerFootprint FromXdr(Xdr.LedgerFootprint xdr)
    {
        return new LedgerFootprint
        {
            ReadOnly = xdr.ReadOnly.Select(LedgerKey.FromXdr).ToArray(),
            ReadWrite = xdr.ReadWrite.Select(LedgerKey.FromXdr).ToArray()
        };
    }
}