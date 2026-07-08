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
///     the GC without being zeroed. <see cref="Sign" /> and <see cref="Dispose" /> are mutually
///     serialized, so a Dispose concurrent with an in-flight Sign waits for the signature to complete;
///     any Sign call that starts after disposal throws <see cref="ObjectDisposedException" />.
/// </summary>
internal sealed class Ed25519Signer : IDisposable
{
#if NETSTANDARD2_1
    private readonly byte[] _expandedPrivateKey;
#else
    private readonly NSec.Cryptography.Key _key;
#endif
    // Serializes Sign and Dispose. Without it the netstandard2.1 backend can zero the key while another
    // thread is mid-SignDetached and silently return an invalid signature (libsodium accepts an all-zero
    // key without error); a lock is boring but provably closes that window, and its cost is noise next
    // to the ~50µs signature itself.
    private readonly object _gate = new();
    private bool _disposed;

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
        // Sodium's PrivateKey property returns a fresh array per access (which is what makes zeroing it
        // here safe), so scrub the copy this fetch created rather than abandoning it to the GC unzeroed.
        // Best-effort only: Sodium.Core's KeyPair keeps its own internal expanded-key array, which its
        // Dispose does not zero and which is out of our reach.
        using var kp = PublicKeyAuth.GenerateKeyPair(seed);
        var handleCopy = kp.PrivateKey;
        _expandedPrivateKey = (byte[])handleCopy.Clone();
        CryptographicOperations.ZeroMemory(handleCopy);
#else
        // Default key creation parameters: ExportPolicy = None. The signer never exports the key, and
        // allowing plaintext export would defeat the point of keeping it in libsodium secure memory.
        _key = NSec.Cryptography.Key.Import(
            NSec.Cryptography.SignatureAlgorithm.Ed25519,
            seed,
            NSec.Cryptography.KeyBlobFormat.RawPrivateKey);
#endif
    }

    public byte[] Sign(byte[] data)
    {
        Throw.IfNull(data, nameof(data));
        lock (_gate)
        {
            if (_disposed)
            {
                // Guard on both backends: NSec would throw this itself, but the netstandard2.1 path
                // would otherwise sign with the zeroed key and silently produce a garbage signature.
                throw new ObjectDisposedException(nameof(Ed25519Signer));
            }

#if NETSTANDARD2_1
            return PublicKeyAuth.SignDetached(data, _expandedPrivateKey);
#else
            return NSec.Cryptography.SignatureAlgorithm.Ed25519.Sign(_key, data);
#endif
        }
    }

    public void Dispose()
    {
        lock (_gate)
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
}
