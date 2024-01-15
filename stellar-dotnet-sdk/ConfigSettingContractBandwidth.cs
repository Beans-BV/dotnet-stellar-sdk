using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractBandwidth : LedgerEntryConfigSetting
{
    public uint LedgerMaxTxsSizeBytes { get; set; }
    public uint TxMaxSizeBytes { get; set; }
    public long FeeTxSize1KB { get; set; }

    public static ConfigSettingContractBandwidth FromXdr(xdr.ConfigSettingContractBandwidthV0 xdrConfig)
    {
        return new ConfigSettingContractBandwidth
        {
            LedgerMaxTxsSizeBytes = xdrConfig.LedgerMaxTxsSizeBytes.InnerValue,
            TxMaxSizeBytes = xdrConfig.TxMaxSizeBytes.InnerValue,
            FeeTxSize1KB = xdrConfig.FeeTxSize1KB.InnerValue
        };
    }

    public xdr.ConfigSettingContractBandwidthV0 ToXdr()
    {
        return new xdr.ConfigSettingContractBandwidthV0
        {
            LedgerMaxTxsSizeBytes = new Uint32(LedgerMaxTxsSizeBytes),
            TxMaxSizeBytes = new Uint32(TxMaxSizeBytes),
            FeeTxSize1KB = new Int64(FeeTxSize1KB)
        };
    }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_BANDWIDTH_V0),
            ContractBandwidth = ToXdr()
        };
    }
}