using StellarDotnetSdk.Xdr;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents a single cost parameter entry for a smart contract cost type, with constant and linear terms used in fee
///     calculation.
/// </summary>
public class ConfigSettingContractCostParamEntry
{
    private ConfigSettingContractCostParamEntry(long constTerm, long linearTerm, ExtensionPoint extensionPoint)
    {
        ExtensionPoint = extensionPoint;
        ConstTerm = constTerm;
        LinearTerm = linearTerm;
    }

    /// <summary>
    ///     Reserved for future use (e.g. higher-order polynomial terms).
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; }

    /// <summary>
    ///     The constant term of the cost model for this cost type.
    /// </summary>
    public long ConstTerm { get; }

    /// <summary>
    ///     The linear term of the cost model for this cost type.
    /// </summary>
    public long LinearTerm { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractCostParamEntry" /> from an XDR
    ///     <see cref="ContractCostParamEntry" /> object.
    /// </summary>
    /// <param name="xdrEntry">The XDR cost parameter entry.</param>
    /// <returns>A <see cref="ConfigSettingContractCostParamEntry" /> instance.</returns>
    public static ConfigSettingContractCostParamEntry FromXdr(ContractCostParamEntry xdrEntry)
    {
        return new ConfigSettingContractCostParamEntry(
            xdrEntry.ConstTerm.InnerValue,
            xdrEntry.LinearTerm.InnerValue,
            ExtensionPoint.FromXdr(xdrEntry.Ext)
        );
    }
}