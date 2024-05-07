using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

public class ContractEvent
{
    private ContractEvent(ExtensionPoint extensionPoint, string? contractId, SCVal[] topics, SCVal data,
        ContractEventType type)
    {
        ExtensionPoint = extensionPoint;
        ContractId = contractId;
        Topics = topics;
        Data = data;
        Type = type;
    }

    public ExtensionPoint ExtensionPoint { get; }
    public string? ContractId { get; }
    public SCVal[] Topics { get; }
    public SCVal? Data { get; }
    public ContractEventType Type { get; }

    public static ContractEvent FromXdr(Xdr.ContractEvent xdrEvent)
    {
        return new ContractEvent(ExtensionPoint.FromXdr(xdrEvent.Ext),
            xdrEvent.ContractID != null ? StrKey.EncodeContractId(xdrEvent.ContractID.InnerValue) : null,
            type: xdrEvent.Type, topics: xdrEvent.Body.V0.Topics.Select(SCVal.FromXdr).ToArray(),
            data: SCVal.FromXdr(xdrEvent.Body.V0.Data));
    }

    /// <summary>
    ///     Used for debugging purpose.
    ///     Returns a string that represents the contract event.
    /// </summary>
    /// <returns>A string that contains the details of the contract event.</returns>
    public string ToString()
    {
        var value = "";
        if (ContractId != null) value += $"- ContractId: {ContractId}\n";
        var data = "Empty";
        if (Data != null)
            data = Data switch
            {
                SCString scString => scString.InnerValue,
                SCSymbol scSymbol => scSymbol.InnerValue,
                SCUint64 scUint64 => scUint64.InnerValue.ToString(),
                _ => data
            };
        value += $"- Data: {data}\n";

        var topics = "";
        if (Topics.Length > 0)
            for (var i = 0; i < Topics.Length; i++)
            {
                var topicString = "";
                topicString = Topics[i] switch
                {
                    SCString scString => scString.InnerValue,
                    SCSymbol scSymbol => scSymbol.InnerValue,
                    SCError error => error.Code.ToString(),
                    _ => topicString
                };
                topics += $"\t+ Topic {i + 1}: {topicString}\n";
            }
        else
            topics = "Empty";

        value += $"- Topics:\n{topics}";
        return value;
    }
}