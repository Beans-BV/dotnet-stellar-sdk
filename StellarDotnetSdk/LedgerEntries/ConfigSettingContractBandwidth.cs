using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class ConfigSettingContractBandwidth : LedgerEntryConfigSetting
{
    private ConfigSettingContractBandwidth(uint ledgerMaxTxsSizeBytes, uint txMaxSizeBytes, long feeTxSize1Kb)
    {
        LedgerMaxTxsSizeBytes = ledgerMaxTxsSizeBytes;
        TxMaxSizeBytes = txMaxSizeBytes;
        FeeTxSize1Kb = feeTxSize1Kb;
    }

    public uint LedgerMaxTxsSizeBytes { get; }
    public uint TxMaxSizeBytes { get; }
    public long FeeTxSize1Kb { get; }

    public static ConfigSettingContractBandwidth FromXdr(ConfigSettingContractBandwidthV0 xdrConfig)
    {
        return new ConfigSettingContractBandwidth(
            xdrConfig.LedgerMaxTxsSizeBytes.InnerValue,
            xdrConfig.TxMaxSizeBytes.InnerValue,
            xdrConfig.FeeTxSize1KB.InnerValue);
    }
}