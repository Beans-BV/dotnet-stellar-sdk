using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ConfigSettingContractCostParamEntry
{
    public ExtensionPoint ExtensionPoint { get; set; }
    public long ConstTerm { get; set; }
    public long LinearTerm { get; set; }

    public static ConfigSettingContractCostParamEntry FromXdr(xdr.ContractCostParamEntry xdrEntry)
    {
        return new ConfigSettingContractCostParamEntry
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdrEntry.Ext),
            ConstTerm = xdrEntry.ConstTerm.InnerValue,
            LinearTerm = xdrEntry.LinearTerm.InnerValue
        };
    }

    public xdr.ContractCostParamEntry ToXdr()
    {
        return new xdr.ContractCostParamEntry
        {
            Ext = ExtensionPoint.ToXdr(),
            ConstTerm = new Int64(ConstTerm),
            LinearTerm = new Int64(LinearTerm)
        };
    }
}