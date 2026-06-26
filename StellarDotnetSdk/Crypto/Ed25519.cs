using System;
using StellarDotnetSdk.Compatibility;

namespace StellarDotnetSdk.Crypto;

internal static class Ed25519
{
    public const int SeedLength = 32;
    public const int PublicKeyLength = 32;

    public static byte[] GetPublicKey(byte[] seed)
    {
        Throw.IfNull(seed, nameof(seed));
        if (seed.Length != SeedLength) throw new ArgumentException($"Seed must be {SeedLength} bytes.", nameof(seed));

#if NETSTANDARD2_1
        var kp = Sodium.PublicKeyAuth.GenerateKeyPair(seed);
        return kp.PublicKey;
#else
        var key = NSec.Cryptography.Key.Import(
            NSec.Cryptography.SignatureAlgorithm.Ed25519,
            seed,
            NSec.Cryptography.KeyBlobFormat.RawPrivateKey,
            new NSec.Cryptography.KeyCreationParameters
            {
                ExportPolicy = NSec.Cryptography.KeyExportPolicies.AllowPlaintextExport,
            });
        return key.PublicKey.Export(NSec.Cryptography.KeyBlobFormat.RawPublicKey);
#endif
    }

    public static byte[] Sign(byte[] seed, byte[] data)
    {
        Throw.IfNull(seed, nameof(seed));
        if (seed.Length != SeedLength) throw new ArgumentException($"Seed must be {SeedLength} bytes.", nameof(seed));
        Throw.IfNull(data, nameof(data));

#if NETSTANDARD2_1
        var kp = Sodium.PublicKeyAuth.GenerateKeyPair(seed);
        return Sodium.PublicKeyAuth.SignDetached(data, kp.PrivateKey);
#else
        var key = NSec.Cryptography.Key.Import(
            NSec.Cryptography.SignatureAlgorithm.Ed25519,
            seed,
            NSec.Cryptography.KeyBlobFormat.RawPrivateKey,
            new NSec.Cryptography.KeyCreationParameters
            {
                ExportPolicy = NSec.Cryptography.KeyExportPolicies.AllowPlaintextExport,
            });
        return NSec.Cryptography.SignatureAlgorithm.Ed25519.Sign(key, data);
#endif
    }

    public static bool Verify(byte[] publicKey, byte[] data, byte[] signature)
    {
        Throw.IfNull(publicKey, nameof(publicKey));
        if (publicKey.Length != PublicKeyLength)
            throw new ArgumentException($"PublicKey must be {PublicKeyLength} bytes.", nameof(publicKey));
        Throw.IfNull(data, nameof(data));
        Throw.IfNull(signature, nameof(signature));

#if NETSTANDARD2_1
        return Sodium.PublicKeyAuth.VerifyDetached(signature, data, publicKey);
#else
        var pk = NSec.Cryptography.PublicKey.Import(
            NSec.Cryptography.SignatureAlgorithm.Ed25519,
            publicKey,
            NSec.Cryptography.KeyBlobFormat.RawPublicKey);
        return NSec.Cryptography.SignatureAlgorithm.Ed25519.Verify(pk, data, signature);
#endif
    }
}

