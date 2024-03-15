using System;
using System.Linq;

namespace stellar_dotnet_sdk;

/// <summary>
///     Holds the Soroban transaction metadata.
/// </summary>
public class SorobanTransactionMeta
{
    /// <summary>
    ///     We can use this to add more fields.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; private init; } = new ExtensionPointZero();


    public SCVal ReturnValue { get; private init; } = new SCVoid();

    /// <summary>
    ///     Custom events populated by the contracts themselves. One list per operation.
    /// </summary>
    public ContractEvent[] Events { get; private init; } = Array.Empty<ContractEvent>(); // TODO Unit test

    /// <summary>
    ///     Diagnostics events that are not hashed. One list per operation.
    ///     This will contain all contract and diagnostic events. Even ones that were emitted in a failed contract call.
    /// </summary>
    public DiagnosticEvent[] DiagnosticEvents { get; private init; } = Array.Empty<DiagnosticEvent>();

    /// <summary>
    ///     Creates the corresponding <c>SorobanTransactionMeta</c> object from an <c>xdr.SorobanTransactionMeta</c> object.
    /// </summary>
    /// <param name="xdrSorobanTransactionMeta">An <c>xdr.SorobanTransactionMeta</c> object to be converted.</param>
    /// <returns>A <c>SorobanTransactionMeta</c> object. Returns null if the provided object is null.</returns>
    public static SorobanTransactionMeta? FromXdr(xdr.SorobanTransactionMeta? xdrSorobanTransactionMeta)
    {
        if (xdrSorobanTransactionMeta == null) return null;
        return new SorobanTransactionMeta
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdrSorobanTransactionMeta.Ext),
            ReturnValue = SCVal.FromXdr(xdrSorobanTransactionMeta.ReturnValue),
            Events = xdrSorobanTransactionMeta.Events.Select(ContractEvent.FromXdr).ToArray(),
            DiagnosticEvents = xdrSorobanTransactionMeta.DiagnosticEvents.Select(DiagnosticEvent.FromXdr).ToArray()
        };
    }

    /// <summary>
    ///     Used for debugging purpose.
    ///     Returns a string that represents the events emitted in a human-readable format.
    /// </summary>
    /// <returns>A string that contains the details of the emitted events.</returns>
    public string ToString()
    {
        var value = "";
        for (var i = 0; i < Events.Length; i++) value += $"Event {i + 1}\n:{Events[i].ToString()}\n";
        for (var i = 0; i < DiagnosticEvents.Length; i++)
            value += $"Diagnostic event {i + 1}:\n{DiagnosticEvents[i].ToString()}\n";
        return value;
    }
}