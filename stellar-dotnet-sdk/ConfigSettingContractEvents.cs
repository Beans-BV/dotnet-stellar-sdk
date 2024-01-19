using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractEvents : LedgerEntryConfigSetting
{
    public uint TxMaxContractEventsSizeBytes { get; set; }
    public long FeeContractEvents1KB { get; set; }

    public static ConfigSettingContractEvents FromXdr(xdr.ConfigSettingContractEventsV0 xdrConfig)
    {
        return new ConfigSettingContractEvents
        {
            TxMaxContractEventsSizeBytes = xdrConfig.TxMaxContractEventsSizeBytes.InnerValue,
            FeeContractEvents1KB = xdrConfig.FeeContractEvents1KB.InnerValue
        };
    }

    public xdr.ConfigSettingContractEventsV0 ToXdr()
    {
        return new xdr.ConfigSettingContractEventsV0
        {
            TxMaxContractEventsSizeBytes = new Uint32(TxMaxContractEventsSizeBytes),
            FeeContractEvents1KB = new Int64(FeeContractEvents1KB)
        };
    }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_EVENTS_V0),
            ContractEvents = ToXdr()
        };
    }
}