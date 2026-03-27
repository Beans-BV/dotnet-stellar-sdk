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

    public ConfigSettingContractCostParamEntry[] ParamEntries { get; }

    public static ConfigSettingContractCostParamsCpuInstructions FromXdr(ContractCostParams xdrParams)
    {
        return new ConfigSettingContractCostParamsCpuInstructions(
            xdrParams.InnerValue.Select(ConfigSettingContractCostParamEntry.FromXdr).ToArray());
    }
}