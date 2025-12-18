using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests.Operations;

/// <summary>
/// Tests for SetOptionsOperation class functionality.
/// </summary>
[TestClass]
public class SetOptionsOperationTest
{
    private readonly KeyPair _source =
        KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

    /// <summary>
    /// Verifies that SetOptionsOperation with all options set round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void SetOptionsOperation_WithAllOptions_RoundTripsThroughXdr()
    {
        // Arrange
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        // GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR
        var inflationDestination = KeyPair.FromSecretSeed("SDHZGHURAYXKU2KMVHPOXI6JG2Q4BSQUQCEOY72O3QQTCLR2T455PMII");
        // GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR
        var signer =
            SignerUtil.Ed25519PublicKey(
                KeyPair.FromSecretSeed("SA64U7C5C7BS5IHWEPA7YWFN3Z6FE5L6KAMYUIT4AQ7KVTVLD23C6HEZ"));

        var clearFlags = 1;
        var setFlags = 1;
        var masterKeyWeight = 1;
        var lowThreshold = 2;
        var mediumThreshold = 3;
        var highThreshold = 4;
        var homeDomain = "stellar.org";
        var signerWeight = 1;

        var operation = new SetOptionsOperation(source)
            .SetInflationDestination(inflationDestination)
            .SetClearFlags(clearFlags)
            .SetSetFlags(setFlags)
            .SetMasterKeyWeight(masterKeyWeight)
            .SetLowThreshold(lowThreshold)
            .SetMediumThreshold(mediumThreshold)
            .SetHighThreshold(highThreshold)
            .SetHomeDomain(homeDomain)
            .SetSigner("GBCP5W2VS7AEWV2HFRN7YYC623LTSV7VSTGIHFXDEJU7S5BAGVCSETRR", signerWeight);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (SetOptionsOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.IsNotNull(decodedOperation.InflationDestination);
        Assert.AreEqual(inflationDestination.AccountId, decodedOperation.InflationDestination.AccountId);
        Assert.AreEqual(1U, decodedOperation.ClearFlags);
        Assert.AreEqual(1U, decodedOperation.SetFlags);
        Assert.AreEqual(1U, decodedOperation.MasterKeyWeight);
        Assert.AreEqual(2U, decodedOperation.LowThreshold);
        Assert.AreEqual(3U, decodedOperation.MediumThreshold);
        Assert.AreEqual(4U, decodedOperation.HighThreshold);
        Assert.AreEqual(homeDomain, decodedOperation.HomeDomain);
        Assert.IsNotNull(decodedOperation.Signer);
        Assert.AreEqual(signer.Discriminant.InnerValue, decodedOperation.Signer.Key.Discriminant.InnerValue);
        CollectionAssert.AreEqual(signer.Ed25519.InnerValue, decodedOperation.Signer.Key.Ed25519.InnerValue);
        Assert.AreEqual(1U, decodedOperation.Signer.Weight);
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, decodedOperation.SourceAccount.AccountId);
        Assert.AreEqual(OperationThreshold.HIGH, decodedOperation.Threshold);

        Assert.AreEqual(
            "AAAAAQAAAAC7JAuE3XvquOnbsgv2SRztjuk4RoBVefQ0rlrFMMQvfAAAAAUAAAABAAAAAO3gUmG83C+VCqO6FztuMtXJF/l7grZA7MjRzqdZ9W8QAAAAAQAAAAEAAAABAAAAAQAAAAEAAAABAAAAAQAAAAIAAAABAAAAAwAAAAEAAAAEAAAAAQAAAAtzdGVsbGFyLm9yZwAAAAABAAAAAET+21WXwEtXRyxb/GBe1tc5V/WUzIOW4yJp+XQgNUUiAAAAAQ==",
            operation.ToXdrBase64());
    }

    /// <summary>
    /// Verifies that SetOptionsOperation with single field (home domain) round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void SetOptionsOperation_WithSingleField_RoundTripsThroughXdr()
    {
        // Arrange
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        var homeDomain = "stellar.org";
        var operation = new SetOptionsOperation(source).SetHomeDomain(homeDomain);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (SetOptionsOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.AreEqual(null, decodedOperation.InflationDestination);
        Assert.AreEqual(null, decodedOperation.ClearFlags);
        Assert.AreEqual(null, decodedOperation.SetFlags);
        Assert.AreEqual(null, decodedOperation.MasterKeyWeight);
        Assert.AreEqual(null, decodedOperation.LowThreshold);
        Assert.AreEqual(null, decodedOperation.MediumThreshold);
        Assert.AreEqual(null, decodedOperation.HighThreshold);
        Assert.AreEqual(homeDomain, decodedOperation.HomeDomain);
        Assert.AreEqual(null, decodedOperation.Signer);
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, decodedOperation.SourceAccount.AccountId);
    }

    /// <summary>
    /// Verifies that SetOptionsOperation with SHA256 hash signer round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void SetOptionsOperation_WithSha256HashSigner_RoundTripsThroughXdr()
    {
        // Arrange
        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var source = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");
        var preimage = "stellar.org"u8.ToArray();
        var hash = Util.Hash(preimage);

        var operation = new SetOptionsOperation(source)
            .SetSigner(SignerUtil.Sha256Hash(hash), 10);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (SetOptionsOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.AreEqual(null, decodedOperation.InflationDestination);
        Assert.AreEqual(null, decodedOperation.ClearFlags);
        Assert.AreEqual(null, decodedOperation.SetFlags);
        Assert.AreEqual(null, decodedOperation.MasterKeyWeight);
        Assert.AreEqual(null, decodedOperation.LowThreshold);
        Assert.AreEqual(null, decodedOperation.MediumThreshold);
        Assert.AreEqual(null, decodedOperation.HighThreshold);
        Assert.AreEqual(null, decodedOperation.HomeDomain);
        Assert.IsNotNull(decodedOperation.Signer);
        Assert.IsTrue(hash.SequenceEqual(decodedOperation.Signer.Key.HashX.InnerValue));
        Assert.AreEqual(10U, decodedOperation.Signer.Weight);
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(source.AccountId, decodedOperation.SourceAccount.AccountId);
    }

    /// <summary>
    /// Verifies that SetOptionsOperation with PreAuthTx signer round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void SetOptionsOperation_WithPreAuthTxSigner_RoundTripsThroughXdr()
    {
        // Arrange
        Network.UseTestNetwork();

        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var sequenceNumber = 2908908335136768L;
        var account = new Account(source.AccountId, sequenceNumber);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(0, TransactionPreconditions.TimeoutInfinite) })
            .Build();

        // GC5SIC4E3V56VOHJ3OZAX5SJDTWY52JYI2AFK6PUGSXFVRJQYQXXZBZF
        var opSource = KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        var operation = new SetOptionsOperation(opSource)
            .SetSigner(SignerUtil.PreAuthTx(transaction), 10);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (SetOptionsOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.AreEqual(operation.InflationDestination, decodedOperation.InflationDestination);
        Assert.AreEqual(operation.ClearFlags, decodedOperation.ClearFlags);
        Assert.AreEqual(operation.SetFlags, decodedOperation.SetFlags);
        Assert.AreEqual(operation.MasterKeyWeight, decodedOperation.MasterKeyWeight);
        Assert.AreEqual(operation.LowThreshold, decodedOperation.LowThreshold);
        Assert.AreEqual(operation.MediumThreshold, decodedOperation.MediumThreshold);
        Assert.AreEqual(operation.HighThreshold, decodedOperation.HighThreshold);
        Assert.AreEqual(operation.HomeDomain, decodedOperation.HomeDomain);
        Assert.IsNotNull(decodedOperation.Signer);
        Assert.IsNotNull(operation.Signer);
        Assert.IsTrue(transaction.Hash().SequenceEqual(decodedOperation.Signer.Key.PreAuthTx.InnerValue));
        Assert.AreEqual(operation.Signer.Weight, decodedOperation.Signer.Weight);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(operation.SourceAccount.AccountId, decodedOperation.SourceAccount.AccountId);
    }

    /// <summary>
    /// Verifies that SetOptionsOperation with signed payload signer round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void SetOptionsOperation_WithSignedPayloadSigner_RoundTripsThroughXdr()
    {
        // Arrange
        const string payloadSignerStrKey = "GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ";
        var payload = Util.HexToBytes("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");
        var signedPayloadSigner = new SignedPayloadSigner(StrKey.DecodeEd25519PublicKey(payloadSignerStrKey), payload);
        var signerKey = SignerUtil.SignedPayload(signedPayloadSigner);
        var operation = new SetOptionsOperation(_source)
            .SetSigner(signerKey, 1);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (SetOptionsOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.IsNotNull(decodedOperation.SourceAccount);
        // verify round trip between xdr and pojo
        Assert.AreEqual(_source.AccountId, decodedOperation.SourceAccount.AccountId);
        Assert.IsNotNull(decodedOperation.Signer);
        CollectionAssert.AreEqual(signedPayloadSigner.SignerAccountId.InnerValue.Ed25519.InnerValue,
            decodedOperation.Signer.Key.Ed25519SignedPayload.Ed25519.InnerValue);
        CollectionAssert.AreEqual(signedPayloadSigner.Payload,
            decodedOperation.Signer.Key.Ed25519SignedPayload.Payload);
    }
}