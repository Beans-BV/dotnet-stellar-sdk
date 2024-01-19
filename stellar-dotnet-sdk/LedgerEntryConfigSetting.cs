using System;
using stellar_dotnet_sdk.xdr;
using static stellar_dotnet_sdk.xdr.ConfigSettingID.ConfigSettingIDEnum;

namespace stellar_dotnet_sdk;

public class LedgerEntryConfigSetting : LedgerEntry
{
    public static LedgerEntryConfigSetting FromXdrLedgerEntry(xdr.LedgerEntry xdrLedgerEntry)
    {
        if (xdrLedgerEntry.Data.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING)
            throw new ArgumentException("Not a ConfigSettingEntry", nameof(xdrLedgerEntry));
        var xdrConfigSetting = xdrLedgerEntry.Data.ConfigSetting;
        
        LedgerEntryConfigSetting ledgerEntryConfigSetting = xdrConfigSetting.Discriminant.InnerValue switch
        {
            CONFIG_SETTING_CONTRACT_BANDWIDTH_V0 => ConfigSettingContractBandwidth
                .FromXdr(xdrConfigSetting.ContractBandwidth),
            CONFIG_SETTING_CONTRACT_COST_PARAMS_MEMORY_BYTES =>
                ConfigSettingContractCostParamsMemoryBytes.FromXdr(xdrConfigSetting.ContractCostParamsMemBytes),
            CONFIG_SETTING_CONTRACT_COST_PARAMS_CPU_INSTRUCTIONS =>
                ConfigSettingContractCostParamsCpuInstructions.FromXdr(xdrConfigSetting.ContractCostParamsCpuInsns),
            CONFIG_SETTING_CONTRACT_COMPUTE_V0 => ConfigSettingContractCompute
                .FromXdr(xdrConfigSetting.ContractCompute),
            CONFIG_SETTING_CONTRACT_EVENTS_V0 => ConfigSettingContractEvents
                .FromXdr(xdrConfigSetting.ContractEvents),
            CONFIG_SETTING_CONTRACT_EXECUTION_LANES =>
                ConfigSettingContractExecutionLanes.FromXdr(xdrConfigSetting.ContractExecutionLanes),
            CONFIG_SETTING_CONTRACT_HISTORICAL_DATA_V0 =>
                ConfigSettingContractHistoricalData.FromXdr(xdrConfigSetting.ContractHistoricalData),
            CONFIG_SETTING_CONTRACT_LEDGER_COST_V0 =>
                ConfigSettingContractLedgerCost.FromXdr(xdrConfigSetting.ContractLedgerCost),
            CONFIG_SETTING_STATE_ARCHIVAL => StateArchivalSettings.FromXdr(
                xdrConfigSetting.StateArchivalSettings),
            CONFIG_SETTING_EVICTION_ITERATOR => EvictionIterator.FromXdr(
                xdrConfigSetting.EvictionIterator),
            CONFIG_SETTING_CONTRACT_MAX_SIZE_BYTES => new
                ConfigSettingContractMaxSizeBytes(xdrConfigSetting.ContractMaxSizeBytes.InnerValue),
            CONFIG_SETTING_CONTRACT_DATA_KEY_SIZE_BYTES => new
                ConfigSettingContractDataKeySizeBytes(xdrConfigSetting.ContractDataKeySizeBytes
                    .InnerValue),
            CONFIG_SETTING_CONTRACT_DATA_ENTRY_SIZE_BYTES => new
                ConfigSettingContractDataEntrySizeBytes(xdrConfigSetting.ContractDataEntrySizeBytes
                    .InnerValue),
            CONFIG_SETTING_BUCKETLIST_SIZE_WINDOW => new
                ConfigSettingBucketListSizeWindow(xdrConfigSetting.BucketListSizeWindow),
            _ => throw new InvalidOperationException("Unknown ConfigSetting type")
        };
        ExtraFieldsFromXdr(xdrLedgerEntry, ledgerEntryConfigSetting);
        return ledgerEntryConfigSetting;
    }

    public ConfigSettingEntry ToXdr()
    {
        return this switch
        {
            ConfigSettingBucketListSizeWindow bucketListSizeWindow => bucketListSizeWindow.ToXdrConfigSettingEntry(),
            ConfigSettingContractBandwidth bandwidth => bandwidth.ToXdrConfigSettingEntry(),
            ConfigSettingContractCompute compute => compute.ToXdrConfigSettingEntry(),
            ConfigSettingContractDataEntrySizeBytes dataEntrySizeBytes => dataEntrySizeBytes.ToXdrConfigSettingEntry(),
            ConfigSettingContractDataKeySizeBytes dataKeySizeBytes => dataKeySizeBytes.ToXdrConfigSettingEntry(),
            ConfigSettingContractEvents events => events.ToXdrConfigSettingEntry(),
            ConfigSettingContractExecutionLanes executionLanes => executionLanes.ToXdrConfigSettingEntry(),
            ConfigSettingContractHistoricalData historicalData => historicalData.ToXdrConfigSettingEntry(),
            ConfigSettingContractLedgerCost ledgerCost => ledgerCost.ToXdrConfigSettingEntry(),
            ConfigSettingContractMaxSizeBytes maxSizeBytes => maxSizeBytes.ToXdrConfigSettingEntry(),
            ConfigSettingContractCostParamsMemoryBytes paramsMemoryBytes => paramsMemoryBytes.ToXdrConfigSettingEntry(),
            ConfigSettingContractCostParamsCpuInstructions paramsCpuInstructions => paramsCpuInstructions.ToXdrConfigSettingEntry(),
            EvictionIterator evictionIterator => evictionIterator.ToXdrConfigSettingEntry(),
            StateArchivalSettings stateArchivalSettings => stateArchivalSettings.ToXdrConfigSettingEntry(),
            _ => throw new InvalidOperationException("Unknown LedgerEntryConfigSetting type")
        };
    }
}