using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

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