using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractComputeV0 : LedgerEntryConfigSetting
{
    public long LedgerMaxInstructions { get; set; }
    public long TxMaxInstructions { get; set; }
    public long FeeRatePerInstructionsIncrement { get; set; }
    public uint TxMemoryLimit { get; set; }

    public static ConfigSettingContractComputeV0 FromXdr(xdr.ConfigSettingContractComputeV0 xdrConfig)
    {
        return new ConfigSettingContractComputeV0
        {
            LedgerMaxInstructions = xdrConfig.LedgerMaxInstructions.InnerValue,
            TxMaxInstructions = xdrConfig.TxMaxInstructions.InnerValue,
            FeeRatePerInstructionsIncrement = xdrConfig.FeeRatePerInstructionsIncrement.InnerValue,
            TxMemoryLimit = xdrConfig.TxMemoryLimit.InnerValue
        };
    }

    public xdr.ConfigSettingContractComputeV0 ToXdr()
    {
        return new xdr.ConfigSettingContractComputeV0
        {
            LedgerMaxInstructions = new Int64(LedgerMaxInstructions),
            TxMaxInstructions = new Int64(TxMaxInstructions),
            FeeRatePerInstructionsIncrement = new Int64(FeeRatePerInstructionsIncrement),
            TxMemoryLimit = new Uint32(TxMemoryLimit)
        };
    }

    public ConfigSettingEntry ToXdrConfigSettingEntry()
    {
        return new ConfigSettingEntry
        {
            Discriminant =
                ConfigSettingID.Create(ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_COMPUTE_V0),
            ContractCompute = ToXdr()
        };
    }
}