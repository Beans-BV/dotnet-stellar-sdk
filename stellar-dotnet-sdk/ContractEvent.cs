using System;
using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ContractEvent
{
    public ExtensionPoint ExtensionPoint { get; set; }
    public string? ContractID { get; set; }
    public SCVal[] Topics { get; set; } = Array.Empty<SCVal>();
    public SCVal Data { get; set; }
    public ContractEventType Type { get; set; }

    public xdr.ContractEvent ToXdr()
    {
        return new xdr.ContractEvent
        {
            Ext = ExtensionPoint.ToXdr(),
            ContractID = ContractID != null ? new xdr.Hash(StrKey.DecodeContractId(ContractID)) : null,
            Type = Type,
            Body = new xdr.ContractEvent.ContractEventBody
            {
                Discriminant = 0,
                V0 = new xdr.ContractEvent.ContractEventBody.ContractEventV0
                {
                    Topics = Topics.Select(x => x.ToXdr()).ToArray(),
                    Data = Data.ToXdr()
                }
            }
        };
    }

    public static ContractEvent FromXdr(xdr.ContractEvent xdrEvent)
    {
        return new ContractEvent
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdrEvent.Ext),
            ContractID = xdrEvent.ContractID != null ? StrKey.EncodeContractId(xdrEvent.ContractID.InnerValue) : null,
            Type = xdrEvent.Type,
            Topics = xdrEvent.Body.V0.Topics.Select(SCVal.FromXdr).ToArray(),
            Data = SCVal.FromXdr(xdrEvent.Body.V0.Data)
        };
    }

    public string ToString()
    {
        var value = "";
        if (ContractID != null) value += $"- ContractId: {ContractID}\n";
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