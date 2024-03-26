using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace StellarDotnetSdk;

using Transaction = Transaction;

/// <summary>
///     Signer is a helper class that creates <see cref="Xdr.SignerKey" /> objects.
/// </summary>
public static class SignerUtil
{
    /// <summary>
    ///     Creates an <c>ed25519PublicKey</c> <c>xdr.SignerKey</c> from
    ///     a <c>KeyPair</c>.
    /// </summary>
    /// <param name="keyPair"></param>
    /// <returns>An <c>ed25519PublicKey</c> <c>xdr.SignerKey</c> object.</returns>
    public static SignerKey Ed25519PublicKey(KeyPair keyPair)
    {
        if (keyPair == null)
            throw new ArgumentNullException(nameof(keyPair), "keyPair cannot be null");

        return keyPair.XdrSignerKey;
    }

    /// <summary>
    ///     Creates an <c>SHA256</c> <c>xdr.SignerKey</c> from
    ///     the SHA256 hash of a preimage.
    /// </summary>
    /// <param name="hash"></param>
    /// <returns>An <c>SHA256</c> <c>xdr.SignerKey</c> object.</returns>
    public static SignerKey Sha256Hash(byte[] hash)
    {
        if (hash == null)
            throw new ArgumentNullException(nameof(hash), "hash cannot be null");

        var signerKey = new SignerKey();
        var value = CreateUint256(hash);

        signerKey.Discriminant = SignerKeyType.Create(SignerKeyType.SignerKeyTypeEnum.SIGNER_KEY_TYPE_HASH_X);
        signerKey.HashX = value;

        return signerKey;
    }

    /// <summary>
    ///     Creates a <c>preAuthTx</c> <c>xdr.SignerKey</c> from
    ///     an <c>xdr.Transaction</c> object.
    /// </summary>
    /// <param name="tx"></param>
    /// <returns>A <c>preAuthTx</c> <c>xdr.SignerKey</c> object.</returns>
    public static SignerKey PreAuthTx(Transaction tx)
    {
        return PreAuthTx(tx, Network.Current);
    }

    /// <summary>
    ///     Creates a <c>preAuthTx</c> <c>xdr.SignerKey</c> from
    ///     an <c>xdr.Transaction</c> object for the specified network.
    /// </summary>
    /// <param name="tx"></param>
    /// <param name="network"></param>
    /// <returns>A <c>preAuthTx</c> <c>xdr.SignerKey</c> object.</returns>
    public static SignerKey PreAuthTx(Transaction tx, Network network)
    {
        if (tx == null)
            throw new ArgumentNullException(nameof(tx), "tx cannot be null");

        return PreAuthTx(tx.Hash(network));
    }

    /// <summary>
    ///     Creates a <c>preAuthTx</c> <c>xdr.SignerKey</c> from
    ///     a transaction hash.
    /// </summary>
    /// <param name="hash"></param>
    /// <returns>A <c>preAuthTx</c> <c>xdr.SignerKey</c> object.</returns>
    public static SignerKey PreAuthTx(byte[] hash)
    {
        if (hash == null)
            throw new ArgumentNullException(nameof(hash), "hash cannot be null");

        var signerKey = new SignerKey();
        var value = CreateUint256(hash);

        signerKey.Discriminant = SignerKeyType.Create(SignerKeyType.SignerKeyTypeEnum.SIGNER_KEY_TYPE_PRE_AUTH_TX);
        signerKey.PreAuthTx = value;

        return signerKey;
    }

    /// <summary>
    ///     Creates a <c>preAuthTx</c> <c>xdr.SignerKey</c> from a <c>SignedPayloadSigner</c> object.
    /// </summary>
    /// <param name="signedPayloadSigner"></param>
    /// <returns>A <c>xdr.SignerKey</c> object.</returns>
    public static SignerKey SignedPayload(SignedPayloadSigner signedPayloadSigner)
    {
        var signerKey = new SignerKey();
        var payloadSigner = new SignerKey.SignerKeyEd25519SignedPayload
        {
            Payload = signedPayloadSigner.Payload,
            Ed25519 = signedPayloadSigner.SignerAccountID.InnerValue.Ed25519
        };

        signerKey.Discriminant.InnerValue = SignerKeyType.SignerKeyTypeEnum.SIGNER_KEY_TYPE_ED25519_SIGNED_PAYLOAD;
        signerKey.Ed25519SignedPayload = payloadSigner;

        return signerKey;
    }

    /// <summary>
    ///     Creates a Uint256 from a byte hash.
    /// </summary>
    /// <param name="hash"></param>
    /// <returns>A Uint256</returns>
    private static Uint256 CreateUint256(byte[] hash)
    {
        if (hash.Length != 32)
            throw new ArgumentException("hash must be 32 bytes long");

        return new Uint256(hash);
    }
}