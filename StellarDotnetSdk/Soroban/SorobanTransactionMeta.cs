using System;
using System.Linq;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Holds the Soroban transaction metadata.
/// </summary>
public class SorobanTransactionMeta
{
    public SorobanTransactionMetaExtensionV1? SorobanTransactionMetaExtensionV1 { get; private set; }

    public SCVal ReturnValue { get; private init; } = new SCVoid();

    /// <summary>
    ///     Custom events populated by the contracts themselves. One list per operation.
    /// </summary>
    public ContractEvent[] Events { get; private init; } = []; // TODO Unit test

    /// <summary>
    ///     Diagnostics events that are not hashed. One list per operation.
    ///     This will contain all contract and diagnostic events. Even ones that were emitted in a failed contract call.
    /// </summary>
    public DiagnosticEvent[] DiagnosticEvents { get; private init; } = [];

    /// <summary>
    ///     Creates the corresponding <c>SorobanTransactionMeta</c> object from an <c>xdr.SorobanTransactionMeta</c> object.
    /// </summary>
    /// <param name="xdrSorobanTransactionMeta">An <c>xdr.SorobanTransactionMeta</c> object to be converted.</param>
    /// <returns>A <c>SorobanTransactionMeta</c> object. Returns null if the provided object is null.</returns>
    public static SorobanTransactionMeta? FromXdr(Xdr.SorobanTransactionMeta? xdrSorobanTransactionMeta)
    {
        if (xdrSorobanTransactionMeta == null)
        {
            return null;
        }
        var meta = new SorobanTransactionMeta
        {
            ReturnValue = SCVal.FromXdr(xdrSorobanTransactionMeta.ReturnValue),
            Events = xdrSorobanTransactionMeta.Events.Select(ContractEvent.FromXdr).ToArray(),
            DiagnosticEvents = xdrSorobanTransactionMeta.DiagnosticEvents.Select(DiagnosticEvent.FromXdr).ToArray(),
        };

        if (xdrSorobanTransactionMeta.Ext.Discriminant == 1)
        {
            meta.SorobanTransactionMetaExtensionV1 =
                SorobanTransactionMetaExtensionV1.FromXdr(xdrSorobanTransactionMeta.Ext.V1);
        }

        return meta;
    }

    /// <summary>
    ///     Used for debugging purpose.
    ///     Returns a string that represents the events emitted in a human-readable format.
    /// </summary>
    /// <returns>A string that contains the details of the emitted events.</returns>
    public string ToDebugString()
    {
        var value = "";
        for (var i = 0; i < Events.Length; i++)
        {
            value += $"Event {i + 1}\n:{Events[i].ToString()}\n";
        }
        for (var i = 0; i < DiagnosticEvents.Length; i++)
        {
            value += $"Diagnostic event {i + 1}:\n{DiagnosticEvents[i].ToString()}\n";
        }
        return value;
    }
}