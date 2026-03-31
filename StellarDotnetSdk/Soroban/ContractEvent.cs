using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Represents an event emitted by a Soroban smart contract during execution.
/// </summary>
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

    /// <summary>
    ///     Reserved for future use.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; }

    /// <summary>
    ///     The contract ID that emitted this event, or <c>null</c> if the event is not tied to a specific contract.
    /// </summary>
    public string? ContractId { get; }

    /// <summary>
    ///     The type of the contract event (e.g., system, contract, or diagnostic).
    /// </summary>
    public ContractEventType Type { get; }

    /// <summary>
    ///     The version 0 body of the event containing topics and data, or <c>null</c> if not present.
    /// </summary>
    public ContractEventV0? BodyV0 { get; private set; }

    /// <summary>
    ///     Creates a new <see cref="ContractEvent" /> from an XDR <see cref="Xdr.ContractEvent" /> object.
    /// </summary>
    /// <param name="xdrEvent">The XDR contract event to convert.</param>
    /// <returns>A <see cref="ContractEvent" /> instance.</returns>
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