namespace StellarDotnetSdk.Accounts;

/// <summary>
///     Represents a Stellar account identifier, providing access to the account's public key,
///     address, and signing key. Implemented by both standard <see cref="KeyPair" /> and
///     <see cref="MuxedAccountMed25519" /> accounts.
/// </summary>
public interface IAccountId
{
    /// <summary>Gets the XDR muxed account representation.</summary>
    Xdr.MuxedAccount MuxedAccount { get; }

    /// <summary>Gets the <see cref="KeyPair" /> used for signing transactions.</summary>
    KeyPair SigningKey { get; }

    /// <summary>Gets the raw Ed25519 public key bytes.</summary>
    byte[] PublicKey { get; }

    /// <summary>Gets the StrKey-encoded address (G... for standard accounts, M... for muxed accounts).</summary>
    string Address { get; }

    /// <summary>Gets the StrKey-encoded account ID (G... for standard accounts, M... for muxed accounts).</summary>
    string AccountId { get; }

    /// <summary>Gets a value indicating whether this is a muxed (multiplexed) account.</summary>
    bool IsMuxedAccount { get; }
}