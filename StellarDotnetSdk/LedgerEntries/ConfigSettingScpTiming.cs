using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents the network configuration settings for Stellar Consensus Protocol (SCP) timing parameters, including
///     target close time and timeout values.
/// </summary>
public class ConfigSettingScpTiming : LedgerEntryConfigSetting
{
    private ConfigSettingScpTiming(
        uint ledgerTargetCloseTimeMilliseconds,
        uint nominationTimeoutInitialMilliseconds,
        uint nominationTimeoutIncrementMilliseconds,
        uint ballotTimeoutInitialMilliseconds,
        uint ballotTimeoutIncrementMilliseconds
    )
    {
        LedgerTargetCloseTimeMilliseconds = ledgerTargetCloseTimeMilliseconds;
        NominationTimeoutInitialMilliseconds = nominationTimeoutInitialMilliseconds;
        NominationTimeoutIncrementMilliseconds = nominationTimeoutIncrementMilliseconds;
        BallotTimeoutInitialMilliseconds = ballotTimeoutInitialMilliseconds;
        BallotTimeoutIncrementMilliseconds = ballotTimeoutIncrementMilliseconds;
    }

    /// <summary>
    ///     Target ledger close time in milliseconds.
    /// </summary>
    public uint LedgerTargetCloseTimeMilliseconds { get; }

    /// <summary>
    ///     Initial timeout for the SCP nomination phase in milliseconds.
    /// </summary>
    public uint NominationTimeoutInitialMilliseconds { get; }

    /// <summary>
    ///     Increment added to the nomination timeout after each round, in milliseconds.
    /// </summary>
    public uint NominationTimeoutIncrementMilliseconds { get; }

    /// <summary>
    ///     Initial timeout for the SCP ballot phase in milliseconds.
    /// </summary>
    public uint BallotTimeoutInitialMilliseconds { get; }

    /// <summary>
    ///     Increment added to the ballot timeout after each round, in milliseconds.
    /// </summary>
    public uint BallotTimeoutIncrementMilliseconds { get; }

    /// <summary>
    ///     Creates a <see cref="ConfigSettingScpTiming" /> from an XDR <see cref="ConfigSettingSCPTiming" /> object.
    /// </summary>
    /// <param name="xdrConfig">The XDR config setting object.</param>
    /// <returns>A <see cref="ConfigSettingScpTiming" /> instance.</returns>
    public static ConfigSettingScpTiming FromXdr(ConfigSettingSCPTiming xdrConfig)
    {
        return new ConfigSettingScpTiming(
            xdrConfig.LedgerTargetCloseTimeMilliseconds.InnerValue,
            xdrConfig.NominationTimeoutInitialMilliseconds.InnerValue,
            xdrConfig.NominationTimeoutIncrementMilliseconds.InnerValue,
            xdrConfig.BallotTimeoutInitialMilliseconds.InnerValue,
            xdrConfig.BallotTimeoutIncrementMilliseconds.InnerValue
        );
    }
}