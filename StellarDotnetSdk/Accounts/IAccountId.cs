namespace StellarDotnetSdk.Accounts;

/// <summary>
///     Represents a Stellar account identifier, providing access to the account's public key,
///     address, and signing key. Implemented by both standard <see cref="KeyPair" /> and
///     <see cref="MuxedAccountMed25519" /> accounts.
/// </summary>
public interface IAccountId
{
    Xdr.MuxedAccount MuxedAccount { get; }
    KeyPair SigningKey { get; }
    byte[] PublicKey { get; }
    string Address { get; }
    string AccountId { get; }
    bool IsMuxedAccount { get; }
}