using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractHistoricalDataV0 : LedgerEntryConfigSetting
{
    public long FeeHistorical1KB { get; set; }

    public static ConfigSettingContractHistoricalDataV0 FromXdr(xdr.ConfigSettingContractHistoricalDataV0 xdrConfig)
    {
        return new ConfigSettingContractHistoricalDataV0
        {
            FeeHistorical1KB = xdrConfig.FeeHistorical1KB.InnerValue
        };
    }

    public xdr.ConfigSettingContractHistoricalDataV0 ToXdr()
    {
        return new xdr.ConfigSettingContractHistoricalDataV0
        {
            FeeHistorical1KB = new Int64(FeeHistorical1KB)
        };
    }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_HISTORICAL_DATA_V0),
            ContractHistoricalData = ToXdr()
        };
    }
}