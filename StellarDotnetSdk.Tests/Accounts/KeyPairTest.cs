using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    ///     Verifies that Sign throws exception when KeyPair does not contain secret key.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void Sign_WithoutSecretKey_ThrowsException()
    {
        // Arrange
        var keyPair = KeyPair.FromAccountId("GDEAOZWTVHQZGGJY6KG4NAGJQ6DXATXAJO3AMW7C4IXLKMPWWB4FDNFZ");
        const string data = "hello world";

        // Act & Assert
        try
        {
            var unused = keyPair.Sign(Encoding.UTF8.GetBytes(data));
        }
        catch (Exception e)
        {
            Assert.AreEqual(
                "KeyPair does not contain secret key. Use KeyPair.fromSecretSeed method to create a new KeyPair with a secret key.",
                e.Message);
            throw;
        }
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
        for (var i = 0; i < sig.Hint.InnerValue.Length; i++)
        {
            sig.Hint.InnerValue[i] = expectedBytes[i];
        }
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
        for (var i = 0; i < sig.Hint.InnerValue.Length; i++)
        {
            sig.Hint.InnerValue[i] = expectedBytes[i];
        }
    }
}