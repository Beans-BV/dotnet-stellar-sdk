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

    /// <summary>
    ///     The list of topic <see cref="SCVal" /> values that categorize this event.
    /// </summary>
    public SCVal[] Topics { get; }

    /// <summary>
    ///     The event data payload as an <see cref="SCVal" />.
    /// </summary>
    public SCVal Data { get; }

    /// <summary>
    ///     Creates a new <see cref="ContractEventV0" /> from an XDR contract event body.
    /// </summary>
    /// <param name="xdrEvent">The XDR contract event body to convert.</param>
    /// <returns>A <see cref="ContractEventV0" /> instance.</returns>
    public static ContractEventV0 FromXdr(Xdr.ContractEvent.ContractEventBody.ContractEventV0 xdrEvent)
    {
        return new ContractEventV0(
            xdrEvent.Topics.Select(SCVal.FromXdr).ToArray(),
            SCVal.FromXdr(xdrEvent.Data)
        );
    }
}