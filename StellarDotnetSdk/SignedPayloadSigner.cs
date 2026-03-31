using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk;

/// <summary>
///     Data model for the https://github.com/stellar/stellar-protocol/blob/master/core/cap-0040.md#xdr-changes signed
///     payload signer
/// </summary>
public class SignedPayloadSigner
{
    /// <summary>Maximum allowed payload length in bytes (64) as defined in CAP-0040.</summary>
    public const int SignedPayloadMaxPayloadLength = 64;

    /// <summary>
    ///     Initializes a new <see cref="SignedPayloadSigner" /> with an XDR account ID and payload.
    /// </summary>
    /// <param name="signerAccountId">The Ed25519 account ID of the signer.</param>
    /// <param name="payload">The payload to be signed (max 64 bytes).</param>
    public SignedPayloadSigner(AccountID signerAccountId, byte[] payload)
    {
        if (signerAccountId == null)
        {
            throw new ArgumentNullException(nameof(signerAccountId), "signerAccountID cannot be null");
        }

        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload), "payload cannot be null");
        }

        if (payload.Length > SignedPayloadMaxPayloadLength)
        {
            throw new ArgumentException($"Invalid payload length, must be less than {SignedPayloadMaxPayloadLength}");
        }

        if (signerAccountId.InnerValue.Discriminant is not
            { InnerValue: PublicKeyType.PublicKeyTypeEnum.PUBLIC_KEY_TYPE_ED25519 })
        {
            throw new ArgumentException(
                "Invalid payload signer, only ED25519 public key accounts are supported at the moment");
        }

        SignerAccountId = signerAccountId;
        Payload = payload;
    }

    /// <summary>
    ///     Initializes a new <see cref="SignedPayloadSigner" /> with raw Ed25519 public key bytes and payload.
    /// </summary>
    /// <param name="signerED25519PublicKey">The raw 32-byte Ed25519 public key of the signer.</param>
    /// <param name="payload">The payload to be signed (max 64 bytes).</param>
    public SignedPayloadSigner(byte[] signerED25519PublicKey, byte[] payload)
        : this(
            new AccountID(new PublicKey
            {
                Discriminant =
                {
                    InnerValue = PublicKeyType.PublicKeyTypeEnum.PUBLIC_KEY_TYPE_ED25519,
                },
                Ed25519 = new Uint256(signerED25519PublicKey),
            }), payload)
    {
    }

    /// <summary>Gets the XDR account ID of the signer.</summary>
    public AccountID SignerAccountId { get; private set; }

    /// <summary>Gets the payload data to be signed.</summary>
    public byte[] Payload { get; private set; }
}