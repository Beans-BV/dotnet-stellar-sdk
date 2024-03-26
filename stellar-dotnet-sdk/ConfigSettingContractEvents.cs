using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractEvents : LedgerEntryConfigSetting
{
    private ConfigSettingContractEvents(uint txMaxContractEventsSizeBytes, long feeContractEvents1KB)
    {
        TxMaxContractEventsSizeBytes = txMaxContractEventsSizeBytes;
        FeeContractEvents1KB = feeContractEvents1KB;
    }

    public uint TxMaxContractEventsSizeBytes { get; }
    public long FeeContractEvents1KB { get; }

    public static ConfigSettingContractEvents FromXdr(ConfigSettingContractEventsV0 xdrConfig)
    {
        return new ConfigSettingContractEvents(
            xdrConfig.TxMaxContractEventsSizeBytes.InnerValue,
            xdrConfig.FeeContractEvents1KB.InnerValue);
    }
}