using System;
using System.Linq;
using System.Security.Cryptography;
using dotnetstandard_bip32;
using NSec.Cryptography;
using StellarDotnetSdk.Xdr;
using PublicKey = NSec.Cryptography.PublicKey;
using xdr_PublicKey = StellarDotnetSdk.Xdr.PublicKey;

namespace StellarDotnetSdk.Accounts;

/// <summary>
///     <see cref="KeyPair" /> represents public (and secret) keys of the account.
///     Currently <see cref="KeyPair" /> only supports ed25519 but in a future this class can be abstraction layer for
///     other public-key signature systems.
/// </summary>
public class KeyPair : IAccountId, IEquatable<KeyPair>
{
    private readonly PublicKey _publicKey;

    private readonly Key? _secretKey;

    private KeyPair(Key secretKey, byte[] seed)
    {
        _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        _publicKey = secretKey.PublicKey;
        SeedBytes = seed ?? throw new ArgumentNullException(nameof(seed));
    }

    /// <summary>
    ///     Creates a new Keypair object from public key.
    /// </summary>
    /// <param name="publicKey"></param>
    public KeyPair(byte[] publicKey)
        : this(publicKey, null, null)
    {
    }

    /// <summary>
    ///     Creates a new Keypair instance from secret. This can either be secret key or secret seed depending on underlying
    ///     public-key signature system. Currently Keypair only supports ed25519.
    /// </summary>
    /// <param name="publicKey"></param>
    /// <param name="privateKey"></param>
    /// <param name="seed"></param>
    public KeyPair(byte[] publicKey, byte[]? privateKey, byte[]? seed)
    {
        _publicKey = NSec.Cryptography.PublicKey.Import(SignatureAlgorithm.Ed25519, publicKey,
            KeyBlobFormat.RawPublicKey);

        if (privateKey != null)
            _secretKey = Key.Import(SignatureAlgorithm.Ed25519, privateKey, KeyBlobFormat.RawPrivateKey,
                new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
        else
            _secretKey = null;

        SeedBytes = seed;
    }

    /// <summary>
    ///     The private key.
    /// </summary>
    public byte[]? PrivateKey => _secretKey?.Export(KeyBlobFormat.RawPrivateKey);

    /// <summary>
    ///     The bytes of the Secret Seed
    /// </summary>
    public byte[]? SeedBytes { get; }

    /// <summary>
    ///     SecretSeed
    /// </summary>
    public string? SecretSeed => SeedBytes != null ? StrKey.EncodeStellarSecretSeed(SeedBytes) : null;

    /// <summary>
    ///     XDR Signature Hint
    /// </summary>
    public SignatureHint SignatureHint
    {
        get
        {
            var stream = new XdrDataOutputStream();
            var accountId = new AccountID(XdrPublicKey);
            AccountID.Encode(stream, accountId);
            var bytes = stream.ToArray();
            var length = bytes.Length;
            var signatureHintBytes = bytes.Skip(length - 4).Take(4).ToArray();

            var signatureHint = new SignatureHint(signatureHintBytes);
            return signatureHint;
        }
    }

    /// <summary>
    ///     XDR Public Key
    /// </summary>
    public xdr_PublicKey XdrPublicKey
    {
        get
        {
            var publicKey = new xdr_PublicKey
            {
                Discriminant = new PublicKeyType
                    { InnerValue = PublicKeyType.PublicKeyTypeEnum.PUBLIC_KEY_TYPE_ED25519 }
            };

            var uint256 = new Uint256(PublicKey);
            publicKey.Ed25519 = uint256;

            return publicKey;
        }
    }

    /// <summary>
    ///     XDR Signer Key
    /// </summary>
    public SignerKey XdrSignerKey
    {
        get
        {
            var signerKey = new SignerKey
            {
                Discriminant = new SignerKeyType
                    { InnerValue = SignerKeyType.SignerKeyTypeEnum.SIGNER_KEY_TYPE_ED25519 }
            };

            var uint256 = new Uint256(PublicKey);
            signerKey.Ed25519 = uint256;

            return signerKey;
        }
    }

    /// <summary>
    ///     The public key.
    /// </summary>
    public byte[] PublicKey => _publicKey.Export(KeyBlobFormat.RawPublicKey);

    /// <summary>
    ///     AccountId
    /// </summary>
    public string AccountId => StrKey.EncodeStellarAccountId(PublicKey);

    /// <summary>
    ///     Address
    /// </summary>
    public string Address => StrKey.EncodeCheck(StrKey.VersionByte.ACCOUNT_ID, PublicKey);

    /// <summary>
    ///     The signing key.
    /// </summary>
    public KeyPair SigningKey => this;

    /// <summary>
    ///     XDR MuxedAccount
    /// </summary>
    public Xdr.MuxedAccount MuxedAccount
    {
        get
        {
            var uint256 = new Uint256(PublicKey);
            var muxedAccount = new Xdr.MuxedAccount
            {
                Discriminant = new CryptoKeyType { InnerValue = CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_ED25519 },
                Ed25519 = uint256
            };
            return muxedAccount;
        }
    }

    public bool IsMuxedAccount => false;

    public bool Equals(KeyPair? other)
    {
        if (other == null) return false;
        if (SeedBytes != null && other.SeedBytes == null) return false;
        if (SeedBytes == null && other.SeedBytes != null) return false;
        return _publicKey.Equals(other._publicKey);
    }

    /// <summary>
    ///     Returns a KeyPair from a Public Key
    /// </summary>
    /// <param name="publicKey"></param>
    /// <returns>
    ///     <see cref="KeyPair" />
    /// </returns>
    public static KeyPair FromXdrPublicKey(xdr_PublicKey publicKey)
    {
        return FromPublicKey(publicKey.Ed25519.InnerValue);
    }

    /// <summary>
    ///     Returns a KeyPair from an XDR SignerKey
    /// </summary>
    /// <param name="signerKey"></param>
    /// <returns>
    ///     <see cref="KeyPair" />
    /// </returns>
    public static KeyPair FromXdrSignerKey(SignerKey signerKey)
    {
        return FromPublicKey(signerKey.Ed25519.InnerValue);
    }

    /// <summary>
    ///     Returns true if this Keypair is capable of signing
    /// </summary>
    /// <returns></returns>
    public bool CanSign()
    {
        return _secretKey != null;
    }

    /// <summary>
    ///     Creates a new Stellar KeyPair from a StrKey encoded Stellar secret seed.
    /// </summary>
    /// <param name="seed">eed Char array containing StrKey encoded Stellar secret seed.</param>
    /// <returns>
    ///     <see cref="KeyPair" />
    /// </returns>
    public static KeyPair FromSecretSeed(string seed)
    {
        var bytes = StrKey.DecodeStellarSecretSeed(seed);
        return FromSecretSeed(bytes);
    }

    /// <summary>
    ///     Creates a new Stellar keypair from a raw 32 byte secret seed.
    /// </summary>
    /// <param name="seed">seed The 32 byte secret seed.</param>
    /// <returns>
    ///     <see cref="KeyPair" />
    /// </returns>
    public static KeyPair FromSecretSeed(byte[] seed)
    {
        var privateKey = Key.Import(SignatureAlgorithm.Ed25519, seed, KeyBlobFormat.RawPrivateKey,
            new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });

        return new KeyPair(privateKey, seed);
    }

    /// <summary>
    ///     Creates a new Stellar KeyPair from a StrKey encoded Stellar account ID.
    /// </summary>
    /// <param name="accountId">accountId The StrKey encoded Stellar account ID.</param>
    /// <returns>
    ///     <see cref="KeyPair" />
    /// </returns>
    public static KeyPair FromAccountId(string accountId)
    {
        var decoded = StrKey.DecodeStellarAccountId(accountId);
        return FromPublicKey(decoded);
    }

    public static KeyPair FromBIP39Seed(string seed, uint accountIndex)
    {
        var bip32 = new BIP32();

        var path = $"m/44'/148'/{accountIndex}'";
        return FromSecretSeed(bip32.DerivePath(path, seed).Key);
    }

    public static KeyPair FromBIP39Seed(byte[] seedBytes, uint accountIndex)
    {
        var seed = seedBytes.ToStringHex();
        return FromBIP39Seed(seed, accountIndex);
    }

    /// <summary>
    ///     Creates a new Stellar keypair from a 32 byte address.
    /// </summary>
    /// <param name="publicKey">publicKey The 32 byte public key.</param>
    /// <returns>
    ///     <see cref="KeyPair" />
    /// </returns>
    public static KeyPair FromPublicKey(byte[] publicKey)
    {
        return new KeyPair(publicKey);
    }

    /// <summary>
    ///     Generates a random Stellar keypair.
    /// </summary>
    /// <returns>a random Stellar keypair</returns>
    public static KeyPair Random()
    {
        var b = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(b);
        }

        return FromSecretSeed(b);
    }

    /// <summary>
    ///     Sign the provided data with the key pair's private key.
    /// </summary>
    /// <param name="data">The data to sign.</param>
    /// <returns>signed bytes, null if the private key for this keypair is null.</returns>
    public byte[] Sign(byte[] data)
    {
        if (_secretKey == null)
            throw new Exception(
                "KeyPair does not contain secret key. Use KeyPair.fromSecretSeed method to create a new KeyPair with a secret key.");

        return SignatureAlgorithm.Ed25519.Sign(_secretKey, data);
    }

    /// <summary>
    ///     Sign a message and return an XDR Decorated Signature
    /// </summary>
    /// <param name="message"></param>
    /// <returns>
    ///     <see cref="DecoratedSignature" />
    /// </returns>
    public DecoratedSignature SignDecorated(byte[] message)
    {
        var rawSig = Sign(message);

        return new DecoratedSignature
        {
            Hint = new SignatureHint(SignatureHint.InnerValue),
            Signature = new Signature(rawSig)
        };
    }

    /// <summary>
    ///     Sign the provided payload data for payload signer where the input is the data being signed.
    /// </summary>
    /// <param name="message"></param>
    /// <returns>
    ///     <see cref="DecoratedSignature" />
    /// </returns>
    public DecoratedSignature SignPayloadDecorated(byte[] signerPayload)
    {
        var payloadSignature = SignDecorated(signerPayload);

        var hint = new byte[4];

        //Copy the last four bytes of the payload into the new hint
        if (signerPayload.Length >= hint.Length)
            Array.Copy(signerPayload, signerPayload.Length - hint.Length, hint, 0, hint.Length);
        else
            Array.Copy(signerPayload, 0, hint, 0, signerPayload.Length);

        //XOR the new hint with this key pair's public key hint
        for (var i = 0; i < hint.Length; i++) hint[i] ^= payloadSignature.Hint.InnerValue[i];
        payloadSignature.Hint.InnerValue = hint;
        return payloadSignature;
    }

    /// <summary>
    ///     Verify the provided data and signature match this key pair's public key.
    /// </summary>
    /// <param name="data">The data that was signed.</param>
    /// <param name="signature">The signature.</param>
    /// <returns>True if they match, false otherwise.</returns>
    public bool Verify(byte[] data, byte[] signature)
    {
        try
        {
            return SignatureAlgorithm.Ed25519.Verify(_publicKey, data, signature);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Verify the provided data and signature match this key pair's public key.
    /// </summary>
    /// <param name="data">The data that was signed.</param>
    /// <param name="signature">The signature.</param>
    /// <returns>True if they match, false otherwise.</returns>
    public bool Verify(byte[] data, Signature signature)
    {
        return Verify(data, signature.InnerValue);
    }
}