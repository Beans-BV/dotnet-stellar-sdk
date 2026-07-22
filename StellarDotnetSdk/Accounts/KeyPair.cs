using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading;
using dotnetstandard_bip32;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Crypto;
using StellarDotnetSdk.Xdr;
using xdr_PublicKey = StellarDotnetSdk.Xdr.PublicKey;

namespace StellarDotnetSdk.Accounts;

/// <summary>
///     Represents the public (and optionally secret) keys of a Stellar account, providing methods
///     for signing data, verifying signatures, and encoding/decoding account identifiers.
/// </summary>
/// <remarks>
///     Currently only supports Ed25519 keys, but is designed to serve as an abstraction layer for
///     other public-key signature systems in the future. Use factory methods such as
///     <see cref="FromSecretSeed(string)" />, <see cref="FromAccountId" />, or <see cref="Random" />
///     to create instances.
///     Signing caches expanded key material (libsodium secure memory on net8.0/net10.0) for the
///     lifetime of the instance; call <see cref="Dispose()" /> on signing keypairs to release it
///     deterministically. Disposal releases signing resources only — it does not erase the stored
///     seed (<see cref="SecretSeed" />, <see cref="SeedBytes" />, <see cref="PrivateKey" /> remain
///     readable). Disposal is optional: an undisposed NSec key is released when its handle is
///     finalized, while on netstandard2.1 an undisposed expanded-key copy is reclaimed by the GC
///     without being zeroed. Keypairs that never signed hold no cached material, though disposal
///     still disables <see cref="Sign" />.
/// </remarks>
[JsonConverter(typeof(KeyPairJsonConverter))]
public class KeyPair : IAccountId, IEquatable<KeyPair>, IDisposable
{
    private readonly byte[]? _privateKey;
    private readonly byte[] _publicKey;

    private volatile bool _disposed;

    // Lazily created on first Sign and reused, so the Ed25519 key expansion/import runs once per KeyPair
    // instead of once per signature (roughly a 2x cost per signature otherwise).
    private Ed25519Signer? _signer;

    /// <summary>
    ///     Creates a new Keypair object from public key.
    /// </summary>
    /// <param name="publicKey">The 32-byte ed25519 public key.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publicKey" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="publicKey" /> is not exactly 32 bytes long.</exception>
    public KeyPair(byte[] publicKey)
        : this(publicKey, null, null)
    {
    }

    /// <summary>
    ///     Creates a new Keypair instance from secret. This can either be secret key or secret seed depending on underlying
    ///     public-key signature system. Currently Keypair only supports ed25519.
    /// </summary>
    /// <param name="publicKey">The 32-byte ed25519 public key.</param>
    /// <param name="privateKey">The optional 32-byte raw private key; enables signing when provided.</param>
    /// <param name="seed">
    ///     The optional 32-byte secret seed, exposed via <see cref="SeedBytes" /> and <see cref="SecretSeed" />
    ///     .
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publicKey" /> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="publicKey" />, <paramref name="privateKey" /> or <paramref name="seed" />
    ///     is not exactly 32 bytes long.
    /// </exception>
    public KeyPair(byte[] publicKey, byte[]? privateKey, byte[]? seed)
    {
        if (publicKey == null)
        {
            throw new ArgumentNullException(nameof(publicKey));
        }
        if (publicKey.Length != Ed25519.PublicKeyLength)
        {
            throw new ArgumentException($"PublicKey must be {Ed25519.PublicKeyLength} bytes.", nameof(publicKey));
        }
        if (privateKey != null && privateKey.Length != Ed25519.SeedLength)
        {
            throw new ArgumentException($"PrivateKey must be {Ed25519.SeedLength} bytes.", nameof(privateKey));
        }
        if (seed != null && seed.Length != Ed25519.SeedLength)
        {
            throw new ArgumentException($"Seed must be {Ed25519.SeedLength} bytes.", nameof(seed));
        }

        _publicKey = (byte[])publicKey.Clone();
        _privateKey = privateKey != null ? (byte[])privateKey.Clone() : null;
        SeedBytes = seed != null ? (byte[])seed.Clone() : null;
    }

    /// <summary>
    ///     The private key.
    /// </summary>
    public byte[]? PrivateKey => _privateKey is null ? null : (byte[])_privateKey.Clone();

    /// <summary>
    ///     The bytes of the Secret Seed
    /// </summary>
    public byte[]? SeedBytes { get; }

    /// <summary>
    ///     SecretSeed
    /// </summary>
    public string? SecretSeed => SeedBytes != null ? StrKey.EncodeEd25519SecretSeed(SeedBytes) : null;

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
                    { InnerValue = PublicKeyType.PublicKeyTypeEnum.PUBLIC_KEY_TYPE_ED25519 },
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
                    { InnerValue = SignerKeyType.SignerKeyTypeEnum.SIGNER_KEY_TYPE_ED25519 },
            };

            var uint256 = new Uint256(PublicKey);
            signerKey.Ed25519 = uint256;

            return signerKey;
        }
    }

    /// <summary>
    ///     The public key.
    /// </summary>
    public byte[] PublicKey => (byte[])_publicKey.Clone();

    /// <summary>
    ///     AccountId
    /// </summary>
    public string AccountId => StrKey.EncodeEd25519PublicKey(PublicKey);

    /// <summary>
    ///     Address
    /// </summary>
    public string Address => StrKey.EncodeCheck(StrKey.VersionByte.ACCOUNT_ID, PublicKey);

    /// <summary>
    ///     Gets the <see cref="KeyPair" /> used for signing transactions. For a standard (non-muxed) account,
    ///     this returns the current instance itself, since the key pair is its own signing key.
    /// </summary>
    [JsonIgnore]
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
                Ed25519 = uint256,
            };
            return muxedAccount;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether this account is a muxed (multiplexed) account.
    ///     Always returns <c>false</c> for <see cref="KeyPair" />; muxed accounts are represented
    ///     by <see cref="MuxedAccountMed25519" />.
    /// </summary>
    public bool IsMuxedAccount => false;

    /// <summary>
    ///     Determines whether the specified <see cref="KeyPair" /> is equal to this instance.
    ///     Two key pairs are considered equal if they share the same public key and both either
    ///     contain or lack a secret seed.
    /// </summary>
    /// <param name="other">The <see cref="KeyPair" /> to compare with this instance.</param>
    /// <returns><c>true</c> if the key pairs are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(KeyPair? other)
    {
        if (other == null)
        {
            return false;
        }
        if (SeedBytes != null && other.SeedBytes == null)
        {
            return false;
        }
        if (SeedBytes == null && other.SeedBytes != null)
        {
            return false;
        }
        return _publicKey.SequenceEqual(other._publicKey);
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
        return _privateKey != null;
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
        var bytes = StrKey.DecodeEd25519SecretSeed(seed);
        return FromSecretSeed(bytes);
    }

    /// <summary>
    ///     Creates a new Stellar keypair from a raw 32 byte secret seed.
    /// </summary>
    /// <param name="seed">seed The 32 byte secret seed.</param>
    /// <returns>
    ///     <see cref="KeyPair" />
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="seed" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed" /> is not exactly 32 bytes long.</exception>
    public static KeyPair FromSecretSeed(byte[] seed)
    {
        var publicKey = Ed25519.GetPublicKey(seed);
        return new KeyPair(publicKey, seed, seed);
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
        var decoded = StrKey.DecodeEd25519PublicKey(accountId);
        return FromPublicKey(decoded);
    }

    /// <summary>
    ///     Derives a Stellar <see cref="KeyPair" /> from a BIP-39 mnemonic seed using the
    ///     standard Stellar derivation path (<c>m/44'/148'/{accountIndex}'</c>).
    /// </summary>
    /// <param name="seed">The hex-encoded BIP-39 seed.</param>
    /// <param name="accountIndex">The account index in the derivation path.</param>
    /// <returns>A <see cref="KeyPair" /> derived from the given seed and account index.</returns>
    public static KeyPair FromBIP39Seed(string seed, uint accountIndex)
    {
        var bip32 = new BIP32();

        var path = $"m/44'/148'/{accountIndex}'";
        return FromSecretSeed(bip32.DerivePath(path, seed).Key);
    }

    /// <summary>
    ///     Derives a Stellar <see cref="KeyPair" /> from a BIP-39 mnemonic seed using the
    ///     standard Stellar derivation path (<c>m/44'/148'/{accountIndex}'</c>).
    /// </summary>
    /// <param name="seedBytes">The raw BIP-39 seed bytes.</param>
    /// <param name="accountIndex">The account index in the derivation path.</param>
    /// <returns>A <see cref="KeyPair" /> derived from the given seed and account index.</returns>
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publicKey" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="publicKey" /> is not exactly 32 bytes long.</exception>
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
    /// <returns>The signed bytes.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when this keypair has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this keypair does not contain a private key.</exception>
    public byte[] Sign(byte[] data)
    {
        // Disposal is checked first so a disposed keypair throws ObjectDisposedException regardless of
        // whether it holds a private key, matching the documented Dispose contract.
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(KeyPair));
        }
        if (_privateKey == null)
        {
            throw new InvalidOperationException(
                "KeyPair does not contain secret key. Use KeyPair.FromSecretSeed method to create a new KeyPair with a secret key.");
        }

        var signer = Volatile.Read(ref _signer);
        if (signer == null)
        {
            signer = new Ed25519Signer(_privateKey);
            var existing = Interlocked.CompareExchange(ref _signer, signer, null);
            if (existing != null)
            {
                // Benign race: another thread published first and both instances are functionally
                // identical; scrub the loser's key material instead of leaving it to the GC.
                signer.Dispose();
                signer = existing;
            }
            else if (_disposed)
            {
                // Dispose ran between the guard above and publication; unpublish so no live key
                // material outlives the Dispose call.
                Interlocked.Exchange(ref _signer, null)?.Dispose();
                throw new ObjectDisposedException(nameof(KeyPair));
            }
        }

        return signer.Sign(data);
    }

    /// <summary>
    ///     Sign a message and return an XDR Decorated Signature
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <returns>
    ///     <see cref="DecoratedSignature" />
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown when this keypair has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this keypair does not contain a private key.</exception>
    public DecoratedSignature SignDecorated(byte[] message)
    {
        var rawSig = Sign(message);

        return new DecoratedSignature
        {
            Hint = new SignatureHint(SignatureHint.InnerValue),
            Signature = new Signature(rawSig),
        };
    }

    /// <summary>
    ///     Sign the provided payload data for payload signer where the input is the data being signed.
    /// </summary>
    /// <param name="signerPayload">The payload to sign.</param>
    /// <returns>
    ///     <see cref="DecoratedSignature" />
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown when this keypair has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this keypair does not contain a private key.</exception>
    public DecoratedSignature SignPayloadDecorated(byte[] signerPayload)
    {
        var payloadSignature = SignDecorated(signerPayload);

        var hint = new byte[4];

        //Copy the last four bytes of the payload into the new hint
        if (signerPayload.Length >= hint.Length)
        {
            Array.Copy(signerPayload, signerPayload.Length - hint.Length, hint, 0, hint.Length);
        }
        else
        {
            Array.Copy(signerPayload, 0, hint, 0, signerPayload.Length);
        }

        //XOR the new hint with this key pair's public key hint
        for (var i = 0; i < hint.Length; i++)
        {
            hint[i] ^= payloadSignature.Hint.InnerValue[i];
        }
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
            return Ed25519.Verify(_publicKey, data, signature);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or CryptographicException)
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

    /// <summary>
    ///     Releases the cached Ed25519 signing handle created by <see cref="Sign" />: the libsodium
    ///     secure-memory key is freed on net8.0/net10.0 and the expanded private key copy is zeroed on
    ///     netstandard2.1. Subsequent <see cref="Sign" />/<see cref="SignDecorated" />/
    ///     <see cref="SignPayloadDecorated" /> calls throw <see cref="ObjectDisposedException" />;
    ///     public-key members (<see cref="Verify(byte[], byte[])" />, <see cref="AccountId" />, equality)
    ///     and the stored seed (<see cref="SecretSeed" />, <see cref="SeedBytes" />) remain usable —
    ///     disposal releases signing resources, it does not erase the seed. Safe to call multiple times
    ///     and on keypairs that never signed. Safe to call concurrently with <see cref="Sign" />: an
    ///     in-flight signature completes normally and any signing call that starts after disposal throws.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases the cached Ed25519 signing handle. See <see cref="Dispose()" />.
    /// </summary>
    /// <param name="disposing">True when called from <see cref="Dispose()" />, false from a finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }
        _disposed = true;
        Interlocked.Exchange(ref _signer, null)?.Dispose();
    }
}
