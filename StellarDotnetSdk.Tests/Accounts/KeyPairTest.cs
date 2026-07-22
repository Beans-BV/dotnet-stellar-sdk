using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Tests.Accounts;

/// <summary>
///     Unit tests for <see cref="KeyPair" /> class.
/// </summary>
[TestClass]
public class KeyPairTest
{
    private const string Seed = "1123740522f11bfef6b3671f51e159ccf589ccf8965262dd5f97d1721d383dd4";

    /// <summary>
    ///     Verifies that Sign method generates correct signature for given data.
    /// </summary>
    [TestMethod]
    public void Sign_WithData_ReturnsCorrectSignature()
    {
        // Arrange
        const string expectedSig =
            "587d4b472eeef7d07aafcd0b049640b0bb3f39784118c2e2b73a04fa2f64c9c538b4b2d0f5335e968a480021fdc23e98c0ddf424cb15d8131df8cb6c4bb58309";
        var keyPair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
        const string data = "hello world";
        var bytes = Encoding.UTF8.GetBytes(data);

        // Act
        var sig = keyPair.Sign(bytes);

        // Assert
        Assert.IsTrue(Util.HexToBytes(expectedSig).SequenceEqual(sig));
    }

    /// <summary>
    ///     Signing twice with the same KeyPair must produce the identical known-answer signature: the
    ///     second call goes through the cached Ed25519 signer (expanded once per KeyPair, not per call),
    ///     which must not change the output.
    /// </summary>
    [TestMethod]
    public void Sign_CalledRepeatedly_ReturnsSameSignatureViaCachedSigner()
    {
        const string expectedSig =
            "587d4b472eeef7d07aafcd0b049640b0bb3f39784118c2e2b73a04fa2f64c9c538b4b2d0f5335e968a480021fdc23e98c0ddf424cb15d8131df8cb6c4bb58309";
        var keyPair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
        var bytes = Encoding.UTF8.GetBytes("hello world");

        var first = keyPair.Sign(bytes);
        var second = keyPair.Sign(bytes);

        Assert.IsTrue(Util.HexToBytes(expectedSig).SequenceEqual(first));
        Assert.IsTrue(first.SequenceEqual(second));
        Assert.IsTrue(keyPair.Verify(bytes, second));
    }

    /// <summary>
    ///     Verifies that Verify method returns true for valid signature.
    /// </summary>
    [TestMethod]
    public void Verify_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        const string sig =
            "587d4b472eeef7d07aafcd0b049640b0bb3f39784118c2e2b73a04fa2f64c9c538b4b2d0f5335e968a480021fdc23e98c0ddf424cb15d8131df8cb6c4bb58309";
        const string data = "hello world";
        var keyPair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
        var bytes = Encoding.UTF8.GetBytes(data);

        // Act & Assert
        Assert.IsTrue(keyPair.Verify(bytes, Util.HexToBytes(sig)));
    }

    /// <summary>
    ///     Verifies that Verify method returns false for invalid signature.
    /// </summary>
    [TestMethod]
    public void Verify_WithInvalidSignature_ReturnsFalse()
    {
        // Arrange
        const string badSig =
            "687d4b472eeef7d07aafcd0b049640b0bb3f39784118c2e2b73a04fa2f64c9c538b4b2d0f5335e968a480021fdc23e98c0ddf424cb15d8131df8cb6c4bb58309";
        byte[] corrupt = [0x00];
        const string data = "hello world";
        var keyPair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
        var bytes = Encoding.UTF8.GetBytes(data);

        // Act & Assert
        Assert.IsFalse(keyPair.Verify(bytes, Util.HexToBytes(badSig)));
        Assert.IsFalse(keyPair.Verify(bytes, corrupt));
    }

    /// <summary>
    ///     Verifies that FromSecretSeed creates KeyPair with correct address and secret seed.
    /// </summary>
    [TestMethod]
    public void FromSecretSeed_WithValidSeeds_CreatesKeyPairWithCorrectAddress()
    {
        // Arrange
        var keypairs = new Dictionary<string, string>
        {
            {
                "SDJHRQF4GCMIIKAAAQ6IHY42X73FQFLHUULAPSKKD4DFDM7UXWWCRHBE",
                "GCZHXL5HXQX5ABDM26LHYRCQZ5OJFHLOPLZX47WEBP3V2PF5AVFK2A5D"
            },
            {
                "SDTQN6XUC3D2Z6TIG3XLUTJIMHOSON2FMSKCTM2OHKKH2UX56RQ7R5Y4",
                "GDEAOZWTVHQZGGJY6KG4NAGJQ6DXATXAJO3AMW7C4IXLKMPWWB4FDNFZ"
            },
            {
                "SDIREFASXYQVEI6RWCQW7F37E6YNXECQJ4SPIOFMMMJRU5CMDQVW32L5",
                "GD2EVR7DGDLNKWEG366FIKXO2KCUAIE3HBUQP4RNY7LEZR5LDKBYHMM6"
            },
            {
                "SDAPE6RHEJ7745VQEKCI2LMYKZB3H6H366I33A42DG7XKV57673XLCC2",
                "GDLXVH2BTLCLZM53GF7ELZFF4BW4MHH2WXEA4Z5Z3O6DPNZNR44A56UJ"
            },
            {
                "SDYZ5IYOML3LTWJ6WIAC2YWORKVO7GJRTPPGGNJQERH72I6ZCQHDAJZN",
                "GABXJTV7ELEB2TQZKJYEGXBUIG6QODJULKJDI65KZMIZZG2EACJU5EA7"
            },
        };

        // Act & Assert
        foreach (var (key, accountId) in keypairs)
        {
            var keypair = KeyPair.FromSecretSeed(key);

            Assert.AreEqual(accountId, keypair.Address);
            Assert.AreEqual(key, keypair.SecretSeed);
        }
    }

    /// <summary>
    ///     Verifies that CanSign returns true for KeyPair created from secret seed and false for KeyPair created from account
    ///     ID.
    /// </summary>
    [TestMethod]
    public void CanSign_WithSecretSeed_ReturnsTrue()
    {
        // Arrange
        var keyPairWithSecret = KeyPair.FromSecretSeed("SDJHRQF4GCMIIKAAAQ6IHY42X73FQFLHUULAPSKKD4DFDM7UXWWCRHBE");
        var keyPairWithoutSecret = KeyPair.FromAccountId("GABXJTV7ELEB2TQZKJYEGXBUIG6QODJULKJDI65KZMIZZG2EACJU5EA7");

        // Act & Assert
        Assert.IsTrue(keyPairWithSecret.CanSign());
        Assert.IsFalse(keyPairWithoutSecret.CanSign());
    }

    /// <summary>
    ///     Verifies that Sign throws InvalidOperationException when KeyPair does not contain secret key.
    /// </summary>
    [TestMethod]
    public void Sign_WithoutSecretKey_ThrowsException()
    {
        // Arrange
        var keyPair = KeyPair.FromAccountId("GDEAOZWTVHQZGGJY6KG4NAGJQ6DXATXAJO3AMW7C4IXLKMPWWB4FDNFZ");
        const string data = "hello world";

        // Act & Assert
        var e = Assert.ThrowsException<InvalidOperationException>(
            () => keyPair.Sign(Encoding.UTF8.GetBytes(data)));
        Assert.AreEqual(
            "KeyPair does not contain secret key. Use KeyPair.FromSecretSeed method to create a new KeyPair with a secret key.",
            e.Message);
    }

    /// <summary>
    ///     Verifies that Equals returns false when comparing with null.
    /// </summary>
    [TestMethod]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var keyPair = KeyPair.FromAccountId("GDEAOZWTVHQZGGJY6KG4NAGJQ6DXATXAJO3AMW7C4IXLKMPWWB4FDNFZ");

        // Act & Assert
        Assert.IsFalse(keyPair.Equals(null!));
    }

    /// <summary>
    ///     Verifies that Equals returns false when comparing KeyPair with secret key to KeyPair without secret key.
    /// </summary>
    [TestMethod]
    public void Equals_WithSecretKeyAndWithout_ReturnsFalse()
    {
        // Arrange
        var keyPair = KeyPair.FromSecretSeed("SDJHRQF4GCMIIKAAAQ6IHY42X73FQFLHUULAPSKKD4DFDM7UXWWCRHBE");
        var otherKeyPair = KeyPair.FromAccountId(keyPair.AccountId);

        // Act & Assert
        Assert.IsFalse(keyPair.Equals(otherKeyPair));
        Assert.IsFalse(otherKeyPair.Equals(keyPair));
    }

    /// <summary>
    ///     Verifies that Equals returns true when comparing KeyPairs with same secret key.
    /// </summary>
    [TestMethod]
    public void Equals_WithSameSecretKey_ReturnsTrue()
    {
        // Arrange
        var keyPair = KeyPair.FromSecretSeed("SDJHRQF4GCMIIKAAAQ6IHY42X73FQFLHUULAPSKKD4DFDM7UXWWCRHBE");
        Assert.IsNotNull(keyPair.SecretSeed);
        var otherKeyPair = KeyPair.FromSecretSeed(keyPair.SecretSeed);

        // Act & Assert
        Assert.IsTrue(keyPair.Equals(otherKeyPair));
        Assert.IsTrue(otherKeyPair.Equals(keyPair));
    }

    /// <summary>
    ///     Verifies that Equals returns true when comparing KeyPairs with only public key and same account ID.
    /// </summary>
    [TestMethod]
    public void Equals_WithOnlyPublicKey_ReturnsTrue()
    {
        // Arrange
        var keyPair = KeyPair.FromAccountId("GDEAOZWTVHQZGGJY6KG4NAGJQ6DXATXAJO3AMW7C4IXLKMPWWB4FDNFZ");
        var otherKeyPair = KeyPair.FromAccountId(keyPair.AccountId);

        // Act & Assert
        Assert.IsTrue(keyPair.Equals(otherKeyPair));
    }

    /// <summary>
    ///     Verifies that SignPayloadDecorated creates correct signature with payload signer.
    /// </summary>
    [TestMethod]
    public void SignPayloadDecorated_WithPayload_CreatesCorrectSignature()
    {
        // Arrange
        var keypair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
        // the hint from this keypair is [254,66,4,55]
        var payload = new byte[] { 1, 2, 3, 4, 5 };
        var expectedBytes = new byte[] { 0xFF & 252, 65, 0, 50 };

        // Act
        var sig = keypair.SignPayloadDecorated(payload);

        // Assert
        CollectionAssert.AreEqual(expectedBytes, sig.Hint.InnerValue);
    }


    /// <summary>
    ///     Verifies that SignPayloadDecorated creates signature with payload signer when hint is less than expected.
    /// </summary>
    [TestMethod]
    public void SignPayloadDecorated_WithPayloadLessThanHint_CreatesSignature()
    {
        // Arrange
        var keypair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
        // the hint from this keypair is [254,66,4,55]
        var payload = new byte[] { 1, 2, 3 };
        var expectedBytes = new byte[] { 255, 64, 7, 55 };

        // Act
        var sig = keypair.SignPayloadDecorated(payload);

        // Assert
        CollectionAssert.AreEqual(expectedBytes, sig.Hint.InnerValue);
    }

    /// <summary>
    ///     Verifies that FromPublicKey rejects a public key that is not exactly 32 bytes at construction time.
    /// </summary>
    [DataTestMethod]
    [DataRow(0)]
    [DataRow(16)]
    [DataRow(31)]
    [DataRow(33)]
    public void FromPublicKey_WithWrongLengthKey_ThrowsArgumentException(int length)
    {
        Assert.ThrowsException<ArgumentException>(() => KeyPair.FromPublicKey(new byte[length]));
    }

    /// <summary>
    ///     Verifies that the constructor rejects a null public key.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullPublicKey_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new KeyPair(null!, null, null));
    }

    /// <summary>
    ///     Verifies that the constructor rejects a private key that is not exactly 32 bytes at construction time.
    /// </summary>
    [DataTestMethod]
    [DataRow(16)]
    [DataRow(33)]
    public void Constructor_WithWrongLengthPrivateKey_ThrowsArgumentException(int length)
    {
        var publicKey = KeyPair.FromAccountId("GDEAOZWTVHQZGGJY6KG4NAGJQ6DXATXAJO3AMW7C4IXLKMPWWB4FDNFZ")
            .PublicKey;

        Assert.ThrowsException<ArgumentException>(() => new KeyPair(publicKey, new byte[length], null));
    }

    /// <summary>
    ///     Verifies that the constructor rejects a seed that is not exactly 32 bytes at construction time.
    /// </summary>
    [DataTestMethod]
    [DataRow(16)]
    [DataRow(33)]
    public void Constructor_WithWrongLengthSeed_ThrowsArgumentException(int length)
    {
        var publicKey = KeyPair.FromAccountId("GDEAOZWTVHQZGGJY6KG4NAGJQ6DXATXAJO3AMW7C4IXLKMPWWB4FDNFZ")
            .PublicKey;

        Assert.ThrowsException<ArgumentException>(() => new KeyPair(publicKey, null, new byte[length]));
    }

    /// <summary>
    ///     Verifies that FromSecretSeed rejects a seed that is not exactly 32 bytes at construction time.
    /// </summary>
    [DataTestMethod]
    [DataRow(0)]
    [DataRow(16)]
    [DataRow(31)]
    [DataRow(33)]
    public void FromSecretSeed_WithWrongLengthSeed_ThrowsArgumentException(int length)
    {
        Assert.ThrowsException<ArgumentException>(() => KeyPair.FromSecretSeed(new byte[length]));
    }

    /// <summary>
    ///     Verifies that a KeyPair constructed with a seed but no private key cannot sign.
    /// </summary>
    [TestMethod]
    public void Constructor_WithSeedOnly_CannotSign()
    {
        // Arrange
        var publicKey = KeyPair.FromAccountId("GDEAOZWTVHQZGGJY6KG4NAGJQ6DXATXAJO3AMW7C4IXLKMPWWB4FDNFZ")
            .PublicKey;
        var unrelatedSeed = Util.HexToBytes(Seed);

        // Act
        var keyPair = new KeyPair(publicKey, null, unrelatedSeed);

        // Assert
        Assert.IsFalse(keyPair.CanSign());
        Assert.IsNull(keyPair.PrivateKey);
        Assert.IsNotNull(keyPair.SecretSeed);
        Assert.ThrowsException<InvalidOperationException>(() => keyPair.Sign(Encoding.UTF8.GetBytes("hello world")));
    }

    /// <summary>
    ///     Verifies that a KeyPair constructed with a private key but no seed can sign without exposing a secret seed.
    /// </summary>
    [TestMethod]
    public void Constructor_WithPrivateKeyOnly_CanSignWithoutExposingSecretSeed()
    {
        // Arrange
        var privateKey = Util.HexToBytes(Seed);
        var fullKeyPair = KeyPair.FromSecretSeed(privateKey);

        // Act
        var keyPair = new KeyPair(fullKeyPair.PublicKey, privateKey, null);

        // Assert
        Assert.IsTrue(keyPair.CanSign());
        Assert.IsNotNull(keyPair.PrivateKey);
        Assert.IsNull(keyPair.SecretSeed);
        Assert.IsNull(keyPair.SeedBytes);
        var data = Encoding.UTF8.GetBytes("hello world");
        Assert.IsTrue(keyPair.Verify(data, keyPair.Sign(data)));
    }

    /// <summary>
    ///     Verifies that a KeyPair holding only a private key equals a public-only KeyPair with the same public key,
    ///     since neither holds a secret seed.
    /// </summary>
    [TestMethod]
    public void Equals_WithPrivateKeyOnlyAndPublicOnly_ReturnsTrue()
    {
        // Arrange
        var privateKey = Util.HexToBytes(Seed);
        var fullKeyPair = KeyPair.FromSecretSeed(privateKey);

        // Act
        var keyPair = new KeyPair(fullKeyPair.PublicKey, privateKey, null);
        var publicOnlyKeyPair = KeyPair.FromPublicKey(fullKeyPair.PublicKey);

        // Assert
        Assert.IsTrue(keyPair.Equals(publicOnlyKeyPair));
        Assert.IsTrue(publicOnlyKeyPair.Equals(keyPair));
    }

    /// <summary>
    ///     Verifies that disposing a keypair after signing blocks further signing but keeps
    ///     public-key members and previously produced signatures usable.
    /// </summary>
    [TestMethod]
    public void Dispose_AfterSigning_BlocksSigningButKeepsPublicKeyMembersUsable()
    {
        // Arrange
        var keyPair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
        var data = Encoding.UTF8.GetBytes("hello world");
        var signature = keyPair.Sign(data);

        // Act
        keyPair.Dispose();

        // Assert
        var e = Assert.ThrowsException<ObjectDisposedException>(() => keyPair.Sign(data));
        Assert.AreEqual(nameof(KeyPair), e.ObjectName);
        Assert.ThrowsException<ObjectDisposedException>(() => keyPair.SignDecorated(data));
        Assert.IsTrue(keyPair.Verify(data, signature));
        Assert.IsNotNull(keyPair.AccountId);
        Assert.IsNotNull(keyPair.SecretSeed);
    }

    /// <summary>
    ///     Verifies that disposing before the first Sign call also blocks all three signing entry
    ///     points (no signer is lazily created from the retained seed afterwards).
    /// </summary>
    [TestMethod]
    public void Dispose_BeforeFirstSign_SignThrows()
    {
        // Arrange
        var keyPair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
        var data = Encoding.UTF8.GetBytes("hello world");

        // Act
        keyPair.Dispose();

        // Assert
        var e = Assert.ThrowsException<ObjectDisposedException>(() => keyPair.Sign(data));
        Assert.AreEqual(nameof(KeyPair), e.ObjectName);
        Assert.ThrowsException<ObjectDisposedException>(() => keyPair.SignDecorated(data));
        Assert.ThrowsException<ObjectDisposedException>(() => keyPair.SignPayloadDecorated(data));
    }

    /// <summary>
    ///     Verifies that Dispose is idempotent, harmless on keypairs that cannot sign, and does not
    ///     affect equality — and that a disposed public-only keypair reports the disposed state
    ///     (ObjectDisposedException) rather than the missing-secret-key state from Sign.
    /// </summary>
    [TestMethod]
    public void Dispose_IsIdempotent_AndHarmlessOnPublicOnlyKeyPairs()
    {
        // Arrange
        var keyPair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
        var sameSeedKeyPair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
        var publicOnlyKeyPair = KeyPair.FromPublicKey(keyPair.PublicKey);

        // Act
        keyPair.Dispose();
        keyPair.Dispose();
        publicOnlyKeyPair.Dispose();

        // Assert
        Assert.IsTrue(keyPair.Equals(sameSeedKeyPair));
        Assert.IsNotNull(publicOnlyKeyPair.AccountId);
        Assert.ThrowsException<ObjectDisposedException>(
            () => publicOnlyKeyPair.Sign(Encoding.UTF8.GetBytes("hello world")));
    }

    /// <summary>
    ///     Regression test for the Dispose-vs-Sign race: before Sign and Dispose were serialized inside
    ///     the signer, a Dispose could zero the expanded key mid-signature on the netstandard2.1 backend
    ///     and Sign would silently return invalid bytes (probe-measured at ~0.3% of contended calls).
    ///     Every concurrent Sign must therefore either return a valid signature or throw
    ///     ObjectDisposedException — nothing in between. The iteration count makes a regression
    ///     near-certain to surface while the assertions can never fail spuriously on correct code.
    /// </summary>
    [TestMethod]
    public void Sign_ConcurrentWithDispose_NeverReturnsInvalidSignature()
    {
        var data = Encoding.UTF8.GetBytes("race probe");
        var verifier = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));

        for (var i = 0; i < 3000; i++)
        {
            var keyPair = KeyPair.FromSecretSeed(Util.HexToBytes(Seed));
            // Alternate between racing the lazy signer creation and racing the published-signer fast path.
            if (i % 2 == 0)
            {
                keyPair.Sign(data);
            }

            using var start = new ManualResetEventSlim(false);
            var signTask = Task.Run(() =>
            {
                start.Wait();
                try
                {
                    var signature = keyPair.Sign(data);
                    Assert.IsTrue(
                        verifier.Verify(data, signature),
                        "Sign returned an invalid signature instead of throwing during a concurrent Dispose.");
                }
                catch (ObjectDisposedException)
                {
                    // The only acceptable failure mode.
                }
            });
            var disposeTask = Task.Run(() =>
            {
                start.Wait();
                Thread.SpinWait(Random.Shared.Next(0, 2000));
                keyPair.Dispose();
            });
            start.Set();
            Task.WaitAll(signTask, disposeTask);
        }
    }
}
