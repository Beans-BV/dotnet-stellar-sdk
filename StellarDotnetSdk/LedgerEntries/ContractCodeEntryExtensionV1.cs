using StellarDotnetSdk.Xdr;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents version 1 extensions to a contract code ledger entry, containing cost inputs for fee estimation.
/// </summary>
public class ContractCodeEntryExtensionV1
{
    private ContractCodeEntryExtensionV1(ExtensionPoint extensionPoint, ContractCodeCostInputs costInputs)
    {
        ExtensionPoint = extensionPoint;
        CostInputs = costInputs;
    }

    /// <summary>
    ///     Reserved for future use.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; }

    /// <summary>
    ///     The cost inputs derived from the contract WASM, used for fee estimation.
    /// </summary>
    public ContractCodeCostInputs CostInputs { get; }

    /// <summary>
    ///     Creates a <see cref="ContractCodeEntryExtensionV1" /> from an XDR
    ///     <see cref="ContractCodeEntry.ContractCodeEntryExt.ContractCodeEntryV1" /> object.
    /// </summary>
    /// <param name="xdrExtensionV1">The XDR extension object.</param>
    /// <returns>A <see cref="ContractCodeEntryExtensionV1" /> instance.</returns>
    public static ContractCodeEntryExtensionV1 FromXdr(
        ContractCodeEntry.ContractCodeEntryExt.ContractCodeEntryV1 xdrExtensionV1)
    {
        return new ContractCodeEntryExtensionV1(
            ExtensionPoint.FromXdr(xdrExtensionV1.Ext),
            ContractCodeCostInputs.FromXdr(xdrExtensionV1.CostInputs)
        );
    }
}