namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for parallel smart contract transaction execution, including the
///     maximum number of dependent transaction clusters per ledger.
/// </summary>
public class ConfigSettingContractParallelComputeV0 : LedgerEntryConfigSetting
{
    private ConfigSettingContractParallelComputeV0(uint ledgerMaxDependentTxClusters)
    {
        LedgerMaxDependentTxClusters = ledgerMaxDependentTxClusters;
    }

    /// <summary>
    ///     Maximum number of clusters with dependent transactions allowed in a stage of parallel transaction set component.
    ///     This sets the lower bound on the number of physical threads needed for parallel transaction application.
    /// </summary>
    public uint LedgerMaxDependentTxClusters { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractParallelComputeV0" /> from an XDR
    ///     <see cref="Xdr.ConfigSettingContractParallelComputeV0" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingContractParallelComputeV0" /> instance.</returns>
    public static ConfigSettingContractParallelComputeV0 FromXdr(
        Xdr.ConfigSettingContractParallelComputeV0 xdrConfig
    )
    {
        return new ConfigSettingContractParallelComputeV0(
            xdrConfig.LedgerMaxDependentTxClusters.InnerValue
        );
    }
}