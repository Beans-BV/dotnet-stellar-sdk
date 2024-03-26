using StellarDotnetSdk.Xdr;
using Soroban_ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.LedgerEntries;

public class ConfigSettingContractCostParamEntry
{
    public Soroban_ExtensionPoint ExtensionPoint { get; set; }
    public long ConstTerm { get; set; }
    public long LinearTerm { get; set; }

    public static ConfigSettingContractCostParamEntry FromXdr(ContractCostParamEntry xdrEntry)
    {
        return new ConfigSettingContractCostParamEntry
        {
            ExtensionPoint = Soroban_ExtensionPoint.FromXdr(xdrEntry.Ext),
            ConstTerm = xdrEntry.ConstTerm.InnerValue,
            LinearTerm = xdrEntry.LinearTerm.InnerValue
        };
    }

    public ContractCostParamEntry ToXdr()
    {
        return new ContractCostParamEntry
        {
            Ext = ExtensionPoint.ToXdr(),
            ConstTerm = new Int64(ConstTerm),
            LinearTerm = new Int64(LinearTerm)
        };
    }
}