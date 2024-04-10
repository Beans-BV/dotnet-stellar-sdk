using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk;

/// <summary>
///     Data model for the https://github.com/stellar/stellar-protocol/blob/master/core/cap-0040.md#xdr-changes signed
///     payload signer
/// </summary>
public class SignedPayloadSigner
{
    public const int SignedPayloadMaxPayloadLength = 64;

    public SignedPayloadSigner(AccountID signerAccountId, byte[] payload)
    {
        if (signerAccountId == null)
            throw new ArgumentNullException(nameof(signerAccountId), "signerAccountID cannot be null");

        if (payload == null) throw new ArgumentNullException(nameof(payload), "payload cannot be null");

        if (payload.Length > SignedPayloadMaxPayloadLength)
            throw new ArgumentException($"Invalid payload length, must be less than {SignedPayloadMaxPayloadLength}");

        if (signerAccountId.InnerValue.Discriminant is not
            { InnerValue: PublicKeyType.PublicKeyTypeEnum.PUBLIC_KEY_TYPE_ED25519 })
            throw new ArgumentException(
                "Invalid payload signer, only ED25519 public key accounts are supported at the moment");

        SignerAccountId = signerAccountId;
        Payload = payload;
    }

    public SignedPayloadSigner(byte[] signerED25519PublicKey, byte[] payload)
        : this(
            new AccountID(new PublicKey
            {
                Discriminant =
                {
                    InnerValue = PublicKeyType.PublicKeyTypeEnum.PUBLIC_KEY_TYPE_ED25519
                },
                Ed25519 = new Uint256(signerED25519PublicKey)
            }), payload)
    {
    }

    public AccountID SignerAccountId { get; private set; }
    public byte[] Payload { get; private set; }
}