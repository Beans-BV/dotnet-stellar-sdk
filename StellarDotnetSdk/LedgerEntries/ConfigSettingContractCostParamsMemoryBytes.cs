using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for smart contract memory byte cost parameters.
/// </summary>
public class ConfigSettingContractCostParamsMemoryBytes : LedgerEntryConfigSetting
{
    private ConfigSettingContractCostParamsMemoryBytes(ConfigSettingContractCostParamEntry[] paramEntries)
    {
        ParamEntries = paramEntries;
    }

    /// <summary>
    ///     The array of cost parameter entries, one per cost type, defining the memory byte cost model.
    /// </summary>
    public ConfigSettingContractCostParamEntry[] ParamEntries { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractCostParamsMemoryBytes" /> from an XDR
    ///     <see cref="ContractCostParams" /> object.
    /// </summary>
    /// <param name="xdrParams">The XDR contract cost parameters.</param>
    /// <returns>A <see cref="ConfigSettingContractCostParamsMemoryBytes" /> instance.</returns>
    public static ConfigSettingContractCostParamsMemoryBytes FromXdr(ContractCostParams xdrParams)
    {
        return new ConfigSettingContractCostParamsMemoryBytes(xdrParams.InnerValue
            .Select(ConfigSettingContractCostParamEntry.FromXdr).ToArray());
    }
}