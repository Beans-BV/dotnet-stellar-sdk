using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NET10_0_OR_GREATER && !TEST_SDK_NETSTANDARD21
using NSec.Cryptography;
using Sodium;
#endif

namespace StellarDotnetSdk.Tests.Crypto;

#if NET10_0_OR_GREATER && !TEST_SDK_NETSTANDARD21

/// <summary>
///     Cross-provider Ed25519 compatibility tests between NSec and Sodium.Core (net10.0 only).
///     net8.0 cannot load both native libsodium versions; netstandard2.1 uses Sodium via the SDK.
/// </summary>
[TestClass]
public class Ed25519CrossProviderTest
{
    private const string KeyPairTestSeedHex =
        "1123740522f11bfef6b3671f51e159ccf589ccf8965262dd5f97d1721d383dd4";

    private const string KeyPairTestSignatureHex =
        "587d4b472eeef7d07aafcd0b049640b0bb3f39784118c2e2b73a04fa2f64c9c538b4b2d0f5335e968a480021fdc23e98c0ddf424cb15d8131df8cb6c4bb58309";

    private static readonly byte[] Rfc8032Seed = Convert.FromHexString(
        "9d61b19deffd5a60ba844af492ec2cc44449c5697b326919703bac031cae7f60");

    private static readonly byte[] Rfc8032PublicKey = Convert.FromHexString(
        "d75a980182b10ab7d54bfed3c964073a0ee172f3daa62325af021a68f707511a");

    private static readonly byte[] Rfc8032Signature = Convert.FromHexString(
        "e5564300c360ac729086e2cc806e828a84877f1eb8e5d974d873e06522490155" +
        "5fb8821590a33bacc61e39701cf9b46bd25bf5f0595bbe24655141438e7a100b");

    [TestMethod]
    public void Rfc8032_TestVector1_ProducesExpectedPublicKeyAndSignature()
    {
        var message = Array.Empty<byte>();

        CollectionAssert.AreEqual(Rfc8032PublicKey, Ed25519Nsec.GetPublicKey(Rfc8032Seed));
        CollectionAssert.AreEqual(Rfc8032PublicKey, Ed25519Sodium.GetPublicKey(Rfc8032Seed));

        CollectionAssert.AreEqual(Rfc8032Signature, Ed25519Nsec.Sign(Rfc8032Seed, message));
        CollectionAssert.AreEqual(Rfc8032Signature, Ed25519Sodium.Sign(Rfc8032Seed, message));
    }

    [TestMethod]
    public void KeyPairTestVector_ProducesExpectedSignatureForHelloWorld()
    {
        var seed = StellarDotnetSdk.Util.HexToBytes(KeyPairTestSeedHex);
        var message = Encoding.UTF8.GetBytes("hello world");
        var expectedSignature = StellarDotnetSdk.Util.HexToBytes(KeyPairTestSignatureHex);

        CollectionAssert.AreEqual(expectedSignature, Ed25519Nsec.Sign(seed, message));
        CollectionAssert.AreEqual(expectedSignature, Ed25519Sodium.Sign(seed, message));
    }

    [TestMethod]
    public void SameSeed_ProducesSamePublicKey()
    {
        var seed = RandomNumberGenerator.GetBytes(32);

        var nsecPublicKey = Ed25519Nsec.GetPublicKey(seed);
        var sodiumPublicKey = Ed25519Sodium.GetPublicKey(seed);

        CollectionAssert.AreEqual(nsecPublicKey, sodiumPublicKey);
        Assert.AreEqual(32, nsecPublicKey.Length);
    }

    [TestMethod]
    public void SameSeedAndMessage_ProducesSameSignature()
    {
        var seed = RandomNumberGenerator.GetBytes(32);
        var message = RandomNumberGenerator.GetBytes(128);

        var nsecSignature = Ed25519Nsec.Sign(seed, message);
        var sodiumSignature = Ed25519Sodium.Sign(seed, message);

        CollectionAssert.AreEqual(nsecSignature, sodiumSignature);
        Assert.AreEqual(64, nsecSignature.Length);
    }

    [TestMethod]
    public void NSecSignature_VerifiesWithSodium()
    {
        var seed = RandomNumberGenerator.GetBytes(32);
        var message = RandomNumberGenerator.GetBytes(256);

        var publicKey = Ed25519Sodium.GetPublicKey(seed);
        var signature = Ed25519Nsec.Sign(seed, message);

        Assert.IsTrue(Ed25519Sodium.Verify(publicKey, message, signature));
    }

    [TestMethod]
    public void SodiumSignature_VerifiesWithNSec()
    {
        var seed = RandomNumberGenerator.GetBytes(32);
        var message = RandomNumberGenerator.GetBytes(256);

        var publicKey = Ed25519Nsec.GetPublicKey(seed);
        var signature = Ed25519Sodium.Sign(seed, message);

        Assert.IsTrue(Ed25519Nsec.Verify(publicKey, message, signature));
    }

    [TestMethod]
    public void MutatedMessage_FailsVerificationOnBothProviders()
    {
        var seed = RandomNumberGenerator.GetBytes(32);
        var message = RandomNumberGenerator.GetBytes(64);

        var publicKey = Ed25519Nsec.GetPublicKey(seed);
        var signature = Ed25519Sodium.Sign(seed, message);

        message[0] ^= 0x01;

        Assert.IsFalse(Ed25519Nsec.Verify(publicKey, message, signature));
        Assert.IsFalse(Ed25519Sodium.Verify(publicKey, message, signature));
    }

    [TestMethod]
    public void MutatedSignature_FailsVerificationOnBothProviders()
    {
        var seed = RandomNumberGenerator.GetBytes(32);
        var message = RandomNumberGenerator.GetBytes(64);

        var publicKey = Ed25519Nsec.GetPublicKey(seed);
        var signature = Ed25519Nsec.Sign(seed, message);
        signature[0] ^= 0x01;

        Assert.IsFalse(Ed25519Nsec.Verify(publicKey, message, signature));
        Assert.IsFalse(Ed25519Sodium.Verify(publicKey, message, signature));
    }

    [TestMethod]
    public void InvalidSeedLength_ThrowsOnBothProviders()
    {
        var invalidSeed = RandomNumberGenerator.GetBytes(31);

        Assert.ThrowsException<ArgumentException>(() => Ed25519Nsec.GetPublicKey(invalidSeed));
        Assert.ThrowsException<ArgumentException>(() => Ed25519Sodium.GetPublicKey(invalidSeed));
        Assert.ThrowsException<ArgumentException>(() => Ed25519Nsec.Sign(invalidSeed, [1]));
        Assert.ThrowsException<ArgumentException>(() => Ed25519Sodium.Sign(invalidSeed, [1]));
    }

    [TestMethod]
    public void Randomized_ProvidersAreEquivalent_For1000Iterations()
    {
        for (var i = 0; i < 1000; i++)
        {
            var seed = RandomNumberGenerator.GetBytes(32);
            var message = RandomNumberGenerator.GetBytes(RandomNumberGenerator.GetInt32(0, 4096));

            var nsecPublicKey = Ed25519Nsec.GetPublicKey(seed);
            var sodiumPublicKey = Ed25519Sodium.GetPublicKey(seed);
            CollectionAssert.AreEqual(nsecPublicKey, sodiumPublicKey);

            var nsecSignature = Ed25519Nsec.Sign(seed, message);
            var sodiumSignature = Ed25519Sodium.Sign(seed, message);
            CollectionAssert.AreEqual(nsecSignature, sodiumSignature);

            Assert.IsTrue(Ed25519Sodium.Verify(nsecPublicKey, message, nsecSignature));
            Assert.IsTrue(Ed25519Nsec.Verify(sodiumPublicKey, message, sodiumSignature));

            var mutatedSignature = (byte[])nsecSignature.Clone();
            mutatedSignature[0] ^= 0x01;
            Assert.IsFalse(Ed25519Nsec.Verify(nsecPublicKey, message, mutatedSignature));
            Assert.IsFalse(Ed25519Sodium.Verify(sodiumPublicKey, message, mutatedSignature));
        }
    }

    private static class Ed25519Nsec
    {
        public const int SeedLength = 32;
        public const int PublicKeyLength = 32;

        public static byte[] GetPublicKey(byte[] seed)
        {
            ValidateSeed(seed);

            using var key = Key.Import(
                SignatureAlgorithm.Ed25519,
                seed,
                KeyBlobFormat.RawPrivateKey,
                new KeyCreationParameters
                {
                    ExportPolicy = KeyExportPolicies.AllowPlaintextExport,
                });

            return key.PublicKey.Export(KeyBlobFormat.RawPublicKey);
        }

        public static byte[] Sign(byte[] seed, byte[] message)
        {
            ValidateSeed(seed);

            using var key = Key.Import(
                SignatureAlgorithm.Ed25519,
                seed,
                KeyBlobFormat.RawPrivateKey,
                new KeyCreationParameters
                {
                    ExportPolicy = KeyExportPolicies.AllowPlaintextExport,
                });

            return SignatureAlgorithm.Ed25519.Sign(key, message);
        }

        public static bool Verify(byte[] publicKey, byte[] message, byte[] signature)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            if (publicKey.Length != PublicKeyLength)
            {
                throw new ArgumentException($"PublicKey must be {PublicKeyLength} bytes.", nameof(publicKey));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            var pk = PublicKey.Import(
                SignatureAlgorithm.Ed25519,
                publicKey,
                KeyBlobFormat.RawPublicKey);

            return SignatureAlgorithm.Ed25519.Verify(pk, message, signature);
        }

        private static void ValidateSeed(byte[] seed)
        {
            if (seed == null)
            {
                throw new ArgumentNullException(nameof(seed));
            }

            if (seed.Length != SeedLength)
            {
                throw new ArgumentException($"Seed must be {SeedLength} bytes.", nameof(seed));
            }
        }
    }

    private static class Ed25519Sodium
    {
        public const int SeedLength = 32;
        public const int PublicKeyLength = 32;

        public static byte[] GetPublicKey(byte[] seed)
        {
            ValidateSeed(seed);
            var kp = PublicKeyAuth.GenerateKeyPair(seed);
            return kp.PublicKey;
        }

        public static byte[] Sign(byte[] seed, byte[] message)
        {
            ValidateSeed(seed);
            var kp = PublicKeyAuth.GenerateKeyPair(seed);
            return PublicKeyAuth.SignDetached(message, kp.PrivateKey);
        }

        public static bool Verify(byte[] publicKey, byte[] message, byte[] signature)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            if (publicKey.Length != PublicKeyLength)
            {
                throw new ArgumentException($"PublicKey must be {PublicKeyLength} bytes.", nameof(publicKey));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            return PublicKeyAuth.VerifyDetached(signature, message, publicKey);
        }

        private static void ValidateSeed(byte[] seed)
        {
            if (seed == null)
            {
                throw new ArgumentNullException(nameof(seed));
            }

            if (seed.Length != SeedLength)
            {
                throw new ArgumentException($"Seed must be {SeedLength} bytes.", nameof(seed));
            }
        }
    }
}
#endif
