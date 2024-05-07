using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class ConfigSettingContractHistoricalData : LedgerEntryConfigSetting
{
    private ConfigSettingContractHistoricalData(long feeHistorical1Kb)
    {
        FeeHistorical1Kb = feeHistorical1Kb;
    }

    public long FeeHistorical1Kb { get; }

    public static ConfigSettingContractHistoricalData FromXdr(ConfigSettingContractHistoricalDataV0 xdrConfig)
    {
        return new ConfigSettingContractHistoricalData(xdrConfig.FeeHistorical1KB.InnerValue);
    }
}