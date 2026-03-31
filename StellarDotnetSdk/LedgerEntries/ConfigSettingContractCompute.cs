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

    /// <summary>
    ///     Maximum instructions per ledger.
    /// </summary>
    public long LedgerMaxInstructions { get; }

    /// <summary>
    ///     Maximum instructions per transaction.
    /// </summary>
    public long TxMaxInstructions { get; }

    /// <summary>
    ///     Cost of 10,000 instructions (fee rate per instructions increment).
    /// </summary>
    public long FeeRatePerInstructionsIncrement { get; }

    /// <summary>
    ///     Memory limit per transaction in bytes. Unlike instructions, there is no fee for memory, only a limit.
    /// </summary>
    public uint TxMemoryLimit { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractCompute" /> from an XDR
    ///     <see cref="ConfigSettingContractComputeV0" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingContractCompute" /> instance.</returns>
    public static ConfigSettingContractCompute FromXdr(ConfigSettingContractComputeV0 xdrConfig)
    {
        return new ConfigSettingContractCompute(
            xdrConfig.LedgerMaxInstructions.InnerValue,
            xdrConfig.TxMaxInstructions.InnerValue,
            xdrConfig.FeeRatePerInstructionsIncrement.InnerValue,
            xdrConfig.TxMemoryLimit.InnerValue);
    }
}