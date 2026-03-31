using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for smart contract CPU instruction cost parameters.
/// </summary>
public class ConfigSettingContractCostParamsCpuInstructions : LedgerEntryConfigSetting
{
    private ConfigSettingContractCostParamsCpuInstructions(ConfigSettingContractCostParamEntry[] paramEntries)
    {
        ParamEntries = paramEntries;
    }

    /// <summary>
    ///     The array of cost parameter entries, one per cost type, defining the CPU instruction cost model.
    /// </summary>
    public ConfigSettingContractCostParamEntry[] ParamEntries { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractCostParamsCpuInstructions" /> from an XDR
    ///     <see cref="ContractCostParams" /> object.
    /// </summary>
    /// <param name="xdrParams">The XDR contract cost parameters.</param>
    /// <returns>A <see cref="ConfigSettingContractCostParamsCpuInstructions" /> instance.</returns>
    public static ConfigSettingContractCostParamsCpuInstructions FromXdr(ContractCostParams xdrParams)
    {
        return new ConfigSettingContractCostParamsCpuInstructions(
            xdrParams.InnerValue.Select(ConfigSettingContractCostParamEntry.FromXdr).ToArray());
    }
}