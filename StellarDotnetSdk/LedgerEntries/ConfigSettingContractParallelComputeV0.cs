namespace StellarDotnetSdk.LedgerEntries;

public class ConfigSettingContractParallelComputeV0 : LedgerEntryConfigSetting
{
    private ConfigSettingContractParallelComputeV0(uint ledgerMaxDependentTxClusters)
    {
        LedgerMaxDependentTxClusters = ledgerMaxDependentTxClusters;
    }

    public uint LedgerMaxDependentTxClusters { get; }

    public static ConfigSettingContractParallelComputeV0 FromXdr(
        Xdr.ConfigSettingContractParallelComputeV0 xdrConfig
    )
    {
        return new ConfigSettingContractParallelComputeV0(
            xdrConfig.LedgerMaxDependentTxClusters.InnerValue
        );
    }
}