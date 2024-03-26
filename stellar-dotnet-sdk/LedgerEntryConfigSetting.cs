using System;
using stellar_dotnet_sdk.xdr;
using static stellar_dotnet_sdk.xdr.ConfigSettingID.ConfigSettingIDEnum;

namespace stellar_dotnet_sdk;

/// <summary>
///     Base class for ledger entry config setting types.
/// </summary>
public class LedgerEntryConfigSetting : LedgerEntry
{
    /// <summary>
    ///     Creates the corresponding LedgerEntryConfigSetting object from a <see cref="xdr.LedgerEntry.LedgerEntryData" />
    ///     object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryConfigSetting object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid ConfigSettingEntry.</exception>
    public static LedgerEntryConfigSetting FromXdrLedgerEntryData(xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING)
            throw new ArgumentException("Not a ConfigSettingEntry.", nameof(xdrLedgerEntryData));

        var xdrConfigSetting = xdrLedgerEntryData.ConfigSetting;

        LedgerEntryConfigSetting ledgerEntryConfigSetting = xdrConfigSetting.Discriminant.InnerValue switch
        {
            CONFIG_SETTING_CONTRACT_BANDWIDTH_V0 =>
                ConfigSettingContractBandwidth.FromXdr(xdrConfigSetting.ContractBandwidth),
            CONFIG_SETTING_CONTRACT_COST_PARAMS_MEMORY_BYTES =>
                ConfigSettingContractCostParamsMemoryBytes.FromXdr(xdrConfigSetting.ContractCostParamsMemBytes),
            CONFIG_SETTING_CONTRACT_COST_PARAMS_CPU_INSTRUCTIONS =>
                ConfigSettingContractCostParamsCpuInstructions.FromXdr(xdrConfigSetting.ContractCostParamsCpuInsns),
            CONFIG_SETTING_CONTRACT_COMPUTE_V0 =>
                ConfigSettingContractCompute.FromXdr(xdrConfigSetting.ContractCompute),
            CONFIG_SETTING_CONTRACT_EVENTS_V0 =>
                ConfigSettingContractEvents.FromXdr(xdrConfigSetting.ContractEvents),
            CONFIG_SETTING_CONTRACT_EXECUTION_LANES =>
                ConfigSettingContractExecutionLanes.FromXdr(xdrConfigSetting.ContractExecutionLanes),
            CONFIG_SETTING_CONTRACT_HISTORICAL_DATA_V0 =>
                ConfigSettingContractHistoricalData.FromXdr(xdrConfigSetting.ContractHistoricalData),
            CONFIG_SETTING_CONTRACT_LEDGER_COST_V0 =>
                ConfigSettingContractLedgerCost.FromXdr(xdrConfigSetting.ContractLedgerCost),
            CONFIG_SETTING_STATE_ARCHIVAL =>
                StateArchivalSettings.FromXdr(xdrConfigSetting.StateArchivalSettings),
            CONFIG_SETTING_EVICTION_ITERATOR =>
                EvictionIterator.FromXdr(xdrConfigSetting.EvictionIterator),
            CONFIG_SETTING_CONTRACT_MAX_SIZE_BYTES =>
                ConfigSettingContractMaxSizeBytes.FromXdr(xdrConfigSetting.ContractMaxSizeBytes),
            CONFIG_SETTING_CONTRACT_DATA_KEY_SIZE_BYTES =>
                ConfigSettingContractDataKeySizeBytes.FromXdr(xdrConfigSetting.ContractDataKeySizeBytes),
            CONFIG_SETTING_CONTRACT_DATA_ENTRY_SIZE_BYTES =>
                ConfigSettingContractDataEntrySizeBytes.FromXdr(xdrConfigSetting.ContractDataEntrySizeBytes),
            CONFIG_SETTING_BUCKETLIST_SIZE_WINDOW =>
                ConfigSettingBucketListSizeWindow.FromXdr(xdrConfigSetting.BucketListSizeWindow),
            _ => throw new InvalidOperationException("Unknown ConfigSetting type.")
        };
        return ledgerEntryConfigSetting;
    }
}