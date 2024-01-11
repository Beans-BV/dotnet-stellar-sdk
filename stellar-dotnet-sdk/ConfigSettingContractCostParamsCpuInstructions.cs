using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractCostParamsCpuInstructions : LedgerEntryConfigSetting
{
    public ConfigSettingContractCostParamEntry[] paramEntries { get; set; }

    public static ConfigSettingContractCostParamsCpuInstructions FromXdr(ContractCostParams xdrParams)
    {
        return new ConfigSettingContractCostParamsCpuInstructions
        {
            paramEntries = xdrParams.InnerValue.Select(ConfigSettingContractCostParamEntry.FromXdr).ToArray()
        };
    }

    public ContractCostParams ToXdr()
    {
        return new ContractCostParams
        {
            InnerValue = paramEntries.Select(x => x.ToXdr()).ToArray()
        };
    }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum
                    .CONFIG_SETTING_CONTRACT_COST_PARAMS_CPU_INSTRUCTIONS),
            ContractCostParamsCpuInsns = ToXdr()
        };
    }
}