using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents version 3 extensions to a Stellar account entry, including sequence ledger and sequence time fields.
/// </summary>
public class AccountEntryExtensionV3
{
    private AccountEntryExtensionV3(ExtensionPoint extensionPoint, uint sequenceLedger, ulong sequenceTime)
    {
        ExtensionPoint = extensionPoint;
        SequenceLedger = sequenceLedger;
        SequenceTime = sequenceTime;
    }

    /// <summary>
    ///     Reserved for future use.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; }

    /// <summary>
    ///     The ledger number at which the sequence number was last consumed.
    /// </summary>
    public uint SequenceLedger { get; }

    /// <summary>
    ///     The time at which the sequence number was last consumed.
    /// </summary>
    public ulong SequenceTime { get; }

    /// <summary>
    ///     Creates an <see cref="AccountEntryExtensionV3" /> from an XDR
    ///     <see cref="Xdr.AccountEntryExtensionV3" /> object.
    /// </summary>
    /// <param name="xdr">The XDR extension object.</param>
    /// <returns>An <see cref="AccountEntryExtensionV3" /> instance.</returns>
    public static AccountEntryExtensionV3 FromXdr(Xdr.AccountEntryExtensionV3 xdr)
    {
        return new AccountEntryExtensionV3(ExtensionPoint.FromXdr(xdr.Ext),
            sequenceTime: xdr.SeqTime.InnerValue.InnerValue, sequenceLedger: xdr.SeqLedger.InnerValue);
    }
}