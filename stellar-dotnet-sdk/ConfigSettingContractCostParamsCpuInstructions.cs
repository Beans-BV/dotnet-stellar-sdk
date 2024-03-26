using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

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