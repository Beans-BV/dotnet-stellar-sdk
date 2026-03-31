namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     Represents the collection of events returned from a Soroban transaction simulation or execution,
///     including diagnostic events, transaction events, and contract events in XDR format.
/// </summary>
public class Events
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Events" /> class.
    /// </summary>
    /// <param name="diagnosticEventsXdr">Base64-encoded XDR diagnostic event strings.</param>
    /// <param name="transactionEventsXdr">Base64-encoded XDR transaction event strings.</param>
    /// <param name="contractEventsXdr">Base64-encoded XDR contract event strings, grouped by invocation.</param>
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

    /// <summary>
    ///     Base64-encoded XDR diagnostic event strings emitted during contract execution.
    /// </summary>
    public string[]? DiagnosticEventsXdr { get; }

    /// <summary>
    ///     Base64-encoded XDR transaction event strings emitted during execution.
    /// </summary>
    public string[]? TransactionEventsXdr { get; }

    /// <summary>
    ///     Base64-encoded XDR contract event strings, grouped by invocation.
    /// </summary>
    public string[][]? ContractEventsXdr { get; }
}