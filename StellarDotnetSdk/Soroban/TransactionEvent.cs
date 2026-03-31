using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Represents an event emitted during a Soroban transaction, including the stage at which the event occurred
///     and the underlying contract event data.
/// </summary>
public class TransactionEvent
{
    private TransactionEvent(TransactionEventStage stage, ContractEvent @event)
    {
        Stage = stage;
        Event = @event;
    }

    /// <summary>
    ///     The stage during transaction processing at which this event was emitted.
    /// </summary>
    public TransactionEventStage Stage { get; }

    /// <summary>
    ///     The underlying contract event data.
    /// </summary>
    public ContractEvent Event { get; }

    /// <summary>
    ///     Creates the corresponding <c>TransactionEvent</c> object from an <c>xdr.TransactionEvent</c> object.
    /// </summary>
    /// <param name="xdrEvent">An <c>xdr.TransactionEvent</c> object to be converted.</param>
    /// <returns>A <c>TransactionEvent</c> object.</returns>
    public static TransactionEvent FromXdr(Xdr.TransactionEvent xdrEvent)
    {
        return new TransactionEvent(
            xdrEvent.Stage,
            ContractEvent.FromXdr(xdrEvent.Event)
        );
    }

    /// <summary>
    ///     Creates a <c>TransactionEvent</c> object from the base-64 encoded XDR string of an
    ///     <see cref="Xdr.TransactionEvent" /> object.
    /// </summary>
    /// <param name="xdrBase64">
    ///     A base-64 encoded XDR string of an <see cref="Xdr.TransactionEvent">xdr.TransactionEvent</see>
    ///     object.
    /// </param>
    /// <returns>A <c>TransactionEvent</c> object decoded and deserialized from the provided string.</returns>
    public static TransactionEvent FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = Xdr.TransactionEvent.Decode(reader);
        return FromXdr(thisXdr);
    }
}