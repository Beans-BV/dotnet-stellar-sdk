using System;
using StellarDotnetSdk.Compatibility;

namespace StellarDotnetSdk.Crypto;

/// <summary>
///     A reusable Ed25519 signing handle: the 32-byte seed is expanded/imported once at construction and
///     reused for every signature, instead of re-deriving the key material on each <c>Sign</c> call.
/// </summary>
internal sealed class Ed25519Signer
{
#if NETSTANDARD2_1
    private readonly byte[] _expandedPrivateKey;
#else
    private readonly NSec.Cryptography.Key _key;
#endif

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
        using var kp = Sodium.PublicKeyAuth.GenerateKeyPair(seed);
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

#if NETSTANDARD2_1
        return Sodium.PublicKeyAuth.SignDetached(data, _expandedPrivateKey);
#else
        return NSec.Cryptography.SignatureAlgorithm.Ed25519.Sign(_key, data);
#endif
    }
}
