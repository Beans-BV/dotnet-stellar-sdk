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

    public ConfigSettingContractCostParamEntry[] ParamEntries { get; }

    public static ConfigSettingContractCostParamsMemoryBytes FromXdr(ContractCostParams xdrParams)
    {
        return new ConfigSettingContractCostParamsMemoryBytes(xdrParams.InnerValue
            .Select(ConfigSettingContractCostParamEntry.FromXdr).ToArray());
    }
}