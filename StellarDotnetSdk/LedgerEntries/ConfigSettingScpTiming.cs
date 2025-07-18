using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

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

    public uint LedgerTargetCloseTimeMilliseconds { get; }
    public uint NominationTimeoutInitialMilliseconds { get; }
    public uint NominationTimeoutIncrementMilliseconds { get; }
    public uint BallotTimeoutInitialMilliseconds { get; }
    public uint BallotTimeoutIncrementMilliseconds { get; }

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