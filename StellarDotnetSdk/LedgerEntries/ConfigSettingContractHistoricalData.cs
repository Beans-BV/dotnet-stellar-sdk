using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for smart contract historical data fees.
/// </summary>
public class ConfigSettingContractHistoricalData : LedgerEntryConfigSetting
{
    private ConfigSettingContractHistoricalData(long feeHistorical1Kb)
    {
        FeeHistorical1Kb = feeHistorical1Kb;
    }

    /// <summary>
    ///     Fee for storing 1KB in archives.
    /// </summary>
    public long FeeHistorical1Kb { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractHistoricalData" /> from an XDR
    ///     <see cref="ConfigSettingContractHistoricalDataV0" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingContractHistoricalData" /> instance.</returns>
    public static ConfigSettingContractHistoricalData FromXdr(ConfigSettingContractHistoricalDataV0 xdrConfig)
    {
        return new ConfigSettingContractHistoricalData(xdrConfig.FeeHistorical1KB.InnerValue);
    }
}