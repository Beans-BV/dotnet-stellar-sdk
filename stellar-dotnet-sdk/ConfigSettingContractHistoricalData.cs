﻿using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractHistoricalData : LedgerEntryConfigSetting
{
    private ConfigSettingContractHistoricalData(long feeHistorical1KB)
    {
        FeeHistorical1KB = feeHistorical1KB;
    }

    public long FeeHistorical1KB { get; }

    public static ConfigSettingContractHistoricalData FromXdr(ConfigSettingContractHistoricalDataV0 xdrConfig)
    {
        return new ConfigSettingContractHistoricalData(xdrConfig.FeeHistorical1KB.InnerValue);
    }
}