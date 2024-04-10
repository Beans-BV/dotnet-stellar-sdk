using StellarDotnetSdk.Xdr;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.LedgerEntries;

public class ConfigSettingContractCostParamEntry
{
    private ConfigSettingContractCostParamEntry(long constTerm, long linearTerm, ExtensionPoint extensionPoint)
    {
        ExtensionPoint = extensionPoint;
        ConstTerm = constTerm;
        LinearTerm = linearTerm;
    }

    public ExtensionPoint ExtensionPoint { get; }
    public long ConstTerm { get; }
    public long LinearTerm { get; }

    public static ConfigSettingContractCostParamEntry FromXdr(ContractCostParamEntry xdrEntry)
    {
        return new ConfigSettingContractCostParamEntry(
            xdrEntry.ConstTerm.InnerValue,
            xdrEntry.LinearTerm.InnerValue,
            ExtensionPoint.FromXdr(xdrEntry.Ext)
        );
    }
}