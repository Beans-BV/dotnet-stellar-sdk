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
    /// <summary>
    ///     The set of ledger keys that the transaction will only read.
    /// </summary>
    public LedgerKey[] ReadOnly { get; init; } = [];

    /// <summary>
    ///     The set of ledger keys that the transaction will read and write.
    /// </summary>
    public LedgerKey[] ReadWrite { get; init; } = [];

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.LedgerFootprint" /> XDR object.</returns>
    public Xdr.LedgerFootprint ToXdr()
    {
        return new Xdr.LedgerFootprint
        {
            ReadOnly = ReadOnly.Select(x => x.ToXdr()).ToArray(),
            ReadWrite = ReadWrite.Select(x => x.ToXdr()).ToArray(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="LedgerFootprint" /> from an XDR <see cref="Xdr.LedgerFootprint" /> object.
    /// </summary>
    /// <param name="xdr">The XDR ledger footprint to convert.</param>
    /// <returns>A <see cref="LedgerFootprint" /> instance.</returns>
    public static LedgerFootprint FromXdr(Xdr.LedgerFootprint xdr)
    {
        return new LedgerFootprint
        {
            ReadOnly = xdr.ReadOnly.Select(LedgerKey.FromXdr).ToArray(),
            ReadWrite = xdr.ReadWrite.Select(LedgerKey.FromXdr).ToArray(),
        };
    }
}