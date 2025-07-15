using System;

namespace StellarDotnetSdk.Accounts;

/// <summary>
/// Represents an account identifier.
/// </summary>
public interface IAccountId
{
    /// <summary>
    ///     Returns the <see cref="Xdr.MuxedAccount"/> counterpart of the account.
    /// </summary>
    Xdr.MuxedAccount ToXdrMuxedAccount();

    /// <summary>
    ///     The signing key of the account.
    /// </summary>
    KeyPair SigningKey { get; }

    /// <summary>
    /// The public key associated with this account as a byte array.
    /// </summary>
    byte[] PublicKey { get; }

    /// <summary>
    ///     The address, which can either be a regular account ID (starting with "G")
    /// or a muxed account address (starting with "M"). 
    /// </summary>
    string Address { get; }

    [Obsolete("Deprecated. Use Address instead.")]
    string AccountId { get; }

    /// <summary>
    /// Returns true if this is a muxed account (starting with "M"). False otherwise. 
    /// </summary>
    bool IsMuxedAccount { get; }
}