namespace StellarDotnetSdk.Responses.SorobanRpc;

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