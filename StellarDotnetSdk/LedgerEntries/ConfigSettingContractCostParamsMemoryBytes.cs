using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

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