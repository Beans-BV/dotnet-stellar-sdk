using System.Linq;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Represents the version 0 body of a Soroban contract event, containing topics and data.
/// </summary>
public class ContractEventV0
{
    private ContractEventV0(SCVal[] topics, SCVal data)
    {
        Topics = topics;
        Data = data;
    }

    public SCVal[] Topics { get; }
    public SCVal Data { get; }

    public static ContractEventV0 FromXdr(Xdr.ContractEvent.ContractEventBody.ContractEventV0 xdrEvent)
    {
        return new ContractEventV0(
            xdrEvent.Topics.Select(SCVal.FromXdr).ToArray(),
            SCVal.FromXdr(xdrEvent.Data)
        );
    }
}