using System;
#if NETSTANDARD2_1
using System.Security.Cryptography;
using Sodium;
#endif
using StellarDotnetSdk.Compatibility;

namespace StellarDotnetSdk.Crypto;

/// <summary>
///     A reusable Ed25519 signing handle: the 32-byte seed is expanded/imported once at construction and
///     reused for every signature, instead of re-deriving the key material on each <c>Sign</c> call.
///     Dispose to deterministically release the cached key material: an undisposed NSec key is released
///     when its handle is finalized, while an undisposed netstandard2.1 expanded-key copy is reclaimed by
///     the GC without being zeroed. Like <c>KeyPair.Dispose</c>, <see cref="Dispose" /> is not safe to
///     call concurrently with an in-flight <see cref="Sign" />.
/// </summary>
internal sealed class Ed25519Signer : IDisposable
{
#if NETSTANDARD2_1
    private readonly byte[] _expandedPrivateKey;
#else
    private readonly NSec.Cryptography.Key _key;
#endif
    private volatile bool _disposed;

    public Ed25519Signer(byte[] seed)
    {
        Throw.IfNull(seed, nameof(seed));
        if (seed.Length != Ed25519.SeedLength)
        {
            throw new ArgumentException($"Seed must be {Ed25519.SeedLength} bytes.", nameof(seed));
        }

#if NETSTANDARD2_1
        // Copy the expanded key out of the Sodium handle so the cached key's lifetime is independent of
        // the handle's — never alias a disposable handle's internal buffer, whatever Dispose does to it.
        using var kp = PublicKeyAuth.GenerateKeyPair(seed);
        _expandedPrivateKey = (byte[])kp.PrivateKey.Clone();
#else
        _key = NSec.Cryptography.Key.Import(
            NSec.Cryptography.SignatureAlgorithm.Ed25519,
            seed,
            NSec.Cryptography.KeyBlobFormat.RawPrivateKey,
            new NSec.Cryptography.KeyCreationParameters
            {
                ExportPolicy = NSec.Cryptography.KeyExportPolicies.AllowPlaintextExport,
            });
#endif
    }

    public byte[] Sign(byte[] data)
    {
        Throw.IfNull(data, nameof(data));
        if (_disposed)
        {
            // Explicit guard on both backends for sequential use-after-dispose: NSec would throw this
            // itself, but the netstandard2.1 path would otherwise sign with the zeroed key and silently
            // produce a garbage signature. A Dispose racing an in-flight Sign is outside the contract
            // (see the class doc) — the volatile flag only narrows that window, it cannot close it.
            throw new ObjectDisposedException(nameof(Ed25519Signer));
        }

#if NETSTANDARD2_1
        return PublicKeyAuth.SignDetached(data, _expandedPrivateKey);
#else
        return NSec.Cryptography.SignatureAlgorithm.Ed25519.Sign(_key, data);
#endif
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
#if NETSTANDARD2_1
        CryptographicOperations.ZeroMemory(_expandedPrivateKey);
#else
        _key.Dispose();
#endif
    }
}