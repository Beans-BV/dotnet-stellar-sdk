using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

public class DiagnosticEvent
{
    private DiagnosticEvent(bool inSuccessfulContractCall, ContractEvent @event)
    {
        InSuccessfulContractCall = inSuccessfulContractCall;
        Event = @event;
    }

    public bool InSuccessfulContractCall { get; }

    public ContractEvent Event { get; }

    /// <summary>
    ///     Creates the corresponding <c>DiagnosticEvent</c> object from an <c>xdr.DiagnosticEvent</c> object.
    /// </summary>
    /// <param name="xdrDiagnosticEvent">An <c>xdr.DiagnosticEvent</c> object to be converted.</param>
    /// <returns>A <c>DiagnosticEvent</c> object.</returns>
    public static DiagnosticEvent FromXdr(Xdr.DiagnosticEvent xdrDiagnosticEvent)
    {
        return new DiagnosticEvent(xdrDiagnosticEvent.InSuccessfulContractCall,
            ContractEvent.FromXdr(xdrDiagnosticEvent.Event));
    }

    /// <summary>
    ///     Used for debugging purpose.
    ///     Returns a string that represents the diagnostic event.
    /// </summary>
    /// <returns>A string that contains the details of the diagnostic event.</returns>
    public string ToString()
    {
        return $"InSuccessfulContractCall: {InSuccessfulContractCall.ToString()}\n{Event}";
    }

    /// <summary>
    ///     Creates a <c>DiagnosticEvent</c> object from the base-64 encoded XDR string of an
    ///     <see cref="Xdr.DiagnosticEvent" /> object.
    /// </summary>
    /// <param name="xdrBase64">
    ///     A base-64 encoded XDR string of an <see cref="Xdr.DiagnosticEvent">xdr.DiagnosticEvent</see>
    ///     object.
    /// </param>
    /// <returns>A <c>DiagnosticEvent</c> object decoded and deserialized from the provided string.</returns>
    public static DiagnosticEvent FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = Xdr.DiagnosticEvent.Decode(reader);
        return FromXdr(thisXdr);
    }
}