using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for smart contract compute limits, including instruction caps and
///     memory limits.
/// </summary>
public class ConfigSettingContractCompute : LedgerEntryConfigSetting
{
    private ConfigSettingContractCompute(long ledgerMaxInstructions, long txMaxInstructions,
        long feeRatePerInstructionsIncrement, uint txMemoryLimit)
    {
        LedgerMaxInstructions = ledgerMaxInstructions;
        TxMaxInstructions = txMaxInstructions;
        FeeRatePerInstructionsIncrement = feeRatePerInstructionsIncrement;
        TxMemoryLimit = txMemoryLimit;
    }

    public long LedgerMaxInstructions { get; }
    public long TxMaxInstructions { get; }
    public long FeeRatePerInstructionsIncrement { get; }
    public uint TxMemoryLimit { get; }

    public static ConfigSettingContractCompute FromXdr(ConfigSettingContractComputeV0 xdrConfig)
    {
        return new ConfigSettingContractCompute(
            xdrConfig.LedgerMaxInstructions.InnerValue,
            xdrConfig.TxMaxInstructions.InnerValue,
            xdrConfig.FeeRatePerInstructionsIncrement.InnerValue,
            xdrConfig.TxMemoryLimit.InnerValue);
    }
}