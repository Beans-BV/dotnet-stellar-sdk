using StellarDotnetSdk.Xdr;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.LedgerEntries;

public class ContractCodeEntryExtensionV1
{
    private ContractCodeEntryExtensionV1(ExtensionPoint extensionPoint, ContractCodeCostInputs costInputs)
    {
        ExtensionPoint = extensionPoint;
        CostInputs = costInputs;
    }

    public ExtensionPoint ExtensionPoint { get; }
    public ContractCodeCostInputs CostInputs { get; }

    public static ContractCodeEntryExtensionV1 FromXdr(
        ContractCodeEntry.ContractCodeEntryExt.ContractCodeEntryV1 xdrExtensionV1)
    {
        return new ContractCodeEntryExtensionV1(
            ExtensionPoint.FromXdr(xdrExtensionV1.Ext),
            ContractCodeCostInputs.FromXdr(xdrExtensionV1.CostInputs)
        );
    }
}