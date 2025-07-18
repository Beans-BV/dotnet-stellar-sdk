using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

public class ContractEvent
{
    private ContractEvent(
        ExtensionPoint extensionPoint,
        string? contractId,
        ContractEventType type,
        ContractEventV0? bodyV0
    )
    {
        ExtensionPoint = extensionPoint;
        ContractId = contractId;
        Type = type;
        BodyV0 = bodyV0;
    }

    public ExtensionPoint ExtensionPoint { get; }
    public string? ContractId { get; }
    public ContractEventType Type { get; }
    public ContractEventV0? BodyV0 { get; private set; }

    public static ContractEvent FromXdr(Xdr.ContractEvent xdrEvent)
    {
        var contractEvent = new ContractEvent(
            ExtensionPoint.FromXdr(xdrEvent.Ext),
            xdrEvent.ContractID != null ? StrKey.EncodeContractId(xdrEvent.ContractID.InnerValue.InnerValue) : null,
            xdrEvent.Type,
            null
        );
        if (xdrEvent.Body.Discriminant == 0)
        {
            contractEvent.BodyV0 = ContractEventV0.FromXdr(xdrEvent.Body.V0);
        }
        return contractEvent;
    }
}