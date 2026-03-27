namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents the collection of events returned from a Soroban transaction simulation or execution,
///     including diagnostic events, transaction events, and contract events in XDR format.
/// </summary>
public class Events
{
    public Events(
        string[]? diagnosticEventsXdr,
        string[]? transactionEventsXdr,
        string[][]? contractEventsXdr
    )
    {
        DiagnosticEventsXdr = diagnosticEventsXdr;
        TransactionEventsXdr = transactionEventsXdr;
        ContractEventsXdr = contractEventsXdr;
    }

    public string[]? DiagnosticEventsXdr { get; }
    public string[]? TransactionEventsXdr { get; }
    public string[][]? ContractEventsXdr { get; }
}