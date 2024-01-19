using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractCostParamsMemoryBytes : LedgerEntryConfigSetting
{
    public ConfigSettingContractCostParamEntry[] paramEntries { get; set; }

    public static ConfigSettingContractCostParamsMemoryBytes FromXdr(ContractCostParams xdrParams)
    {
        return new ConfigSettingContractCostParamsMemoryBytes
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
                    .CONFIG_SETTING_CONTRACT_COST_PARAMS_MEMORY_BYTES),
            ContractCostParamsMemBytes = ToXdr()
        };
    }
}