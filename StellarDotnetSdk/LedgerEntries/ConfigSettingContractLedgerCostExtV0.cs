namespace StellarDotnetSdk.LedgerEntries;

public class ConfigSettingContractLedgerCostExtV0 : LedgerEntryConfigSetting
{
    private ConfigSettingContractLedgerCostExtV0(
        uint txMaxFootprintEntries,
        long feeWrite1Kb
    )
    {
        TxMaxFootprintEntries = txMaxFootprintEntries;
        FeeWrite1Kb = feeWrite1Kb;
    }

    public uint TxMaxFootprintEntries { get; }
    public long FeeWrite1Kb { get; }

    public static ConfigSettingContractLedgerCostExtV0 FromXdr(
        Xdr.ConfigSettingContractLedgerCostExtV0 xdrConfig
    )
    {
        return new ConfigSettingContractLedgerCostExtV0(
            xdrConfig.TxMaxFootprintEntries.InnerValue,
            xdrConfig.FeeWrite1KB.InnerValue
        );
    }
}