using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractBandwidth : LedgerEntryConfigSetting
{
    private ConfigSettingContractBandwidth(uint ledgerMaxTxsSizeBytes, uint txMaxSizeBytes, long feeTxSize1KB)
    {
        LedgerMaxTxsSizeBytes = ledgerMaxTxsSizeBytes;
        TxMaxSizeBytes = txMaxSizeBytes;
        FeeTxSize1KB = feeTxSize1KB;
    }

    public uint LedgerMaxTxsSizeBytes { get; }
    public uint TxMaxSizeBytes { get; }
    public long FeeTxSize1KB { get; }

    public static ConfigSettingContractBandwidth FromXdr(ConfigSettingContractBandwidthV0 xdrConfig)
    {
        return new ConfigSettingContractBandwidth(
            xdrConfig.LedgerMaxTxsSizeBytes.InnerValue,
            xdrConfig.TxMaxSizeBytes.InnerValue,
            xdrConfig.FeeTxSize1KB.InnerValue);
    }
}