namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents version 0 extensions to the contract ledger cost configuration, including footprint entry limits and
///     write fees.
/// </summary>
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

    /// <summary>
    ///     Maximum number of read-only and read-write entries in the transaction footprint.
    /// </summary>
    public uint TxMaxFootprintEntries { get; }

    /// <summary>
    ///     Flat fee per 1KB of data written to the ledger, independent of the entry type being written.
    /// </summary>
    public long FeeWrite1Kb { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingContractLedgerCostExtV0" /> from an XDR
    ///     <see cref="Xdr.ConfigSettingContractLedgerCostExtV0" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingContractLedgerCostExtV0" /> instance.</returns>
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