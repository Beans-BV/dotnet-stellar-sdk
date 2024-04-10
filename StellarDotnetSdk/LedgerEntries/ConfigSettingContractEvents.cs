using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class ConfigSettingContractEvents : LedgerEntryConfigSetting
{
    private ConfigSettingContractEvents(uint txMaxContractEventsSizeBytes, long feeContractEvents1Kb)
    {
        TxMaxContractEventsSizeBytes = txMaxContractEventsSizeBytes;
        FeeContractEvents1Kb = feeContractEvents1Kb;
    }

    public uint TxMaxContractEventsSizeBytes { get; }
    public long FeeContractEvents1Kb { get; }

    public static ConfigSettingContractEvents FromXdr(ConfigSettingContractEventsV0 xdrConfig)
    {
        return new ConfigSettingContractEvents(
            xdrConfig.TxMaxContractEventsSizeBytes.InnerValue,
            xdrConfig.FeeContractEvents1KB.InnerValue);
    }
}