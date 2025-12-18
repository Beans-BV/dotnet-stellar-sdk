using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using Memo = StellarDotnetSdk.Memos.Memo;
using SorobanResources = StellarDotnetSdk.Soroban.SorobanResources;
using SorobanTransactionData = StellarDotnetSdk.Soroban.SorobanTransactionData;
using TimeBounds = StellarDotnetSdk.Transactions.TimeBounds;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace StellarDotnetSdk.Tests.Transactions;

/// <summary>
///     Unit tests for <see cref="Transaction" /> class and transaction building functionality.
/// </summary>
[TestClass]
public class TransactionTest
{
    [TestInitialize]
    public void Initialize()
    {
        Network.UseTestNetwork();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Network.Use(null);
    }

    /// <summary>
    ///     Verifies that deprecated TransactionBuilder creates transaction with correct properties and XDR encoding.
    /// </summary>
    [TestMethod]
    public void Build_WithDeprecatedTransactionBuilder_CreatesTransactionWithCorrectProperties()
    {
        // Arrange
        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var sequenceNumber = 2908908335136768L;
        var account = new Account(source.AccountId, sequenceNumber);
        // Test that we do not break the old api. So suppress the warning for now.
#pragma warning disable 0618
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .Build();
#pragma warning restore 0618

        // Act
        transaction.Sign(source);

        // Assert
        Assert.AreEqual(
            "AAAAAF7FIiDToW1fOYUFBC0dmyufJbFTOa2GQESGz+S2h5ViAAAAZAAKVaMAAAABAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAAEqBfIAAAAAAAAAAABtoeVYgAAAEDLki9Oi700N60Lo8gUmEFHbKvYG4QSqXiLIt9T0ru2O5BphVl/jR9tYtHAD+UeDYhgXNgwUxqTEu1WukvEyYcD",
            transaction.ToEnvelopeXdrBase64(TransactionBase.TransactionXdrVersion.V0));

        Assert.AreEqual(
            "AAAAAgAAAABexSIg06FtXzmFBQQtHZsrnyWxUzmthkBEhs/ktoeVYgAAAGQAClWjAAAAAQAAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAO3gUmG83C+VCqO6FztuMtXJF/l7grZA7MjRzqdZ9W8QAAAABKgXyAAAAAAAAAAAAbaHlWIAAABAy5IvTou9NDetC6PIFJhBR2yr2BuEEql4iyLfU9K7tjuQaYVZf40fbWLRwA/lHg2IYFzYMFMakxLtVrpLxMmHAw==",
            transaction.ToEnvelopeXdrBase64());

        Assert.AreEqual(transaction.SourceAccount.AccountId, source.AccountId);
        Assert.AreEqual(transaction.SequenceNumber, sequenceNumber + 1);
        Assert.AreEqual(transaction.Fee, 100U);
    }

    /// <summary>
    ///     Verifies that TransactionBuilder creates transaction with correct properties and XDR encoding on testnet.
    /// </summary>
    [TestMethod]
    public void Build_WithTransactionBuilderOnTestnet_CreatesTransactionWithCorrectProperties()
    {
        // Arrange
        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var sequenceNumber = 2908908335136768L;
        var account = new Account(source.AccountId, sequenceNumber);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .Build();

        // Act
        transaction.Sign(source);

        // Assert
        Assert.AreEqual(
            "AAAAAF7FIiDToW1fOYUFBC0dmyufJbFTOa2GQESGz+S2h5ViAAAAZAAKVaMAAAABAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAAEqBfIAAAAAAAAAAABtoeVYgAAAEDLki9Oi700N60Lo8gUmEFHbKvYG4QSqXiLIt9T0ru2O5BphVl/jR9tYtHAD+UeDYhgXNgwUxqTEu1WukvEyYcD",
            transaction.ToEnvelopeXdrBase64(TransactionBase.TransactionXdrVersion.V0));
        Assert.AreEqual(
            "AAAAAgAAAABexSIg06FtXzmFBQQtHZsrnyWxUzmthkBEhs/ktoeVYgAAAGQAClWjAAAAAQAAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAO3gUmG83C+VCqO6FztuMtXJF/l7grZA7MjRzqdZ9W8QAAAABKgXyAAAAAAAAAAAAbaHlWIAAABAy5IvTou9NDetC6PIFJhBR2yr2BuEEql4iyLfU9K7tjuQaYVZf40fbWLRwA/lHg2IYFzYMFMakxLtVrpLxMmHAw==",
            transaction.ToEnvelopeXdrBase64());
        Assert.AreEqual(transaction.SourceAccount.AccountId, source.AccountId);
        Assert.AreEqual(transaction.SequenceNumber, sequenceNumber + 1);
        Assert.AreEqual(transaction.Fee, 100U);
    }

    /// <summary>
    ///     Verifies that Transaction round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromEnvelopeXdr_WithValidXdr_RoundTripsCorrectly()
    {
        // Arrange
        var transaction = Transaction.FromEnvelopeXdr(
            "AAAAAF7FIiDToW1fOYUFBC0dmyufJbFTOa2GQESGz+S2h5ViAAAAZAAKVaMAAAABAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAAEqBfIAAAAAAAAAAABtoeVYgAAAEDLki9Oi700N60Lo8gUmEFHbKvYG4QSqXiLIt9T0ru2O5BphVl/jR9tYtHAD+UeDYhgXNgwUxqTEu1WukvEyYcD");

        // Act
        var transaction2 = Transaction.FromEnvelopeXdr(transaction.ToEnvelopeXdr());

        // Assert
        Assert.AreEqual(transaction.SourceAccount.AccountId, transaction2.SourceAccount.AccountId);
        Assert.AreEqual(transaction.SequenceNumber, transaction2.SequenceNumber);
        Assert.AreEqual(transaction.Fee, transaction2.Fee);
        Assert.AreEqual(
            ((CreateAccountOperation)transaction.Operations[0]).StartingBalance,
            ((CreateAccountOperation)transaction2.Operations[0]).StartingBalance
        );

        CollectionAssert.AreEqual(transaction.Signatures, transaction2.Signatures);
    }

    /// <summary>
    ///     Verifies that Transaction.FromEnvelopeXdr correctly deserializes transaction with memo.
    /// </summary>
    [TestMethod]
    public void FromEnvelopeXdr_WithMemo_DeserializesCorrectly()
    {
        // Arrange
        var xdrBase64 =
            "AAAAACq1Ixcw1fchtF5aLTSw1zaYAYjb3WbBRd4jqYJKThB9AAAAZAA8tDoAAAALAAAAAAAAAAEAAAAZR29sZCBwYXltZW50IGZvciBzZXJ2aWNlcwAAAAAAAAEAAAAAAAAAAQAAAAARREGslec48mbJJygIwZoLvRtL6/gGL4ss2TOpnOUOhgAAAAFHT0xEAAAAACq1Ixcw1fchtF5aLTSw1zaYAYjb3WbBRd4jqYJKThB9AAAAADuaygAAAAAAAAAAAA==";

        // Act
        var transaction = Transaction.FromEnvelopeXdr(xdrBase64);

        // Assert
        Assert.AreEqual(1, transaction.Operations.Length);
        Assert.IsInstanceOfType(transaction.Memo, typeof(MemoText));
        var op = transaction.Operations[0];
        Assert.IsNull(op.SourceAccount);
        Assert.IsInstanceOfType(op, typeof(PaymentOperation));
        var payment = op as PaymentOperation;
        Assert.IsNotNull(payment);
        Assert.AreEqual("100", payment.Amount);
        var asset = payment.Asset as AssetTypeCreditAlphaNum;
        Assert.IsNotNull(asset);
        Assert.AreEqual("GOLD", asset.Code);
    }

    /// <summary>
    ///     Verifies that TransactionBuilder with memo text creates transaction that round-trips correctly through XDR
    ///     serialization.
    /// </summary>
    [TestMethod]
    public void Build_WithMemoText_RoundTripsCorrectly()
    {
        // Arrange
        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var account = new Account(source.AccountId, 2908908335136768);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .AddMemo(Memo.Text("Hello world!"))
            .Build();

        // Act
        transaction.Sign(source);

        // Assert
        Assert.AreEqual(
            "AAAAAF7FIiDToW1fOYUFBC0dmyufJbFTOa2GQESGz+S2h5ViAAAAZAAKVaMAAAABAAAAAAAAAAEAAAAMSGVsbG8gd29ybGQhAAAAAQAAAAAAAAAAAAAAAO3gUmG83C+VCqO6FztuMtXJF/l7grZA7MjRzqdZ9W8QAAAABKgXyAAAAAAAAAAAAbaHlWIAAABAxzofBhoayuUnz8t0T1UNWrTgmJ+lCh9KaeOGu2ppNOz9UGw0abGLhv+9oWQsstaHx6YjwWxL+8GBvwBUVWRlBQ==",
            transaction.ToEnvelopeXdrBase64(TransactionBase.TransactionXdrVersion.V0));

        Assert.AreEqual(
            "AAAAAgAAAABexSIg06FtXzmFBQQtHZsrnyWxUzmthkBEhs/ktoeVYgAAAGQAClWjAAAAAQAAAAAAAAABAAAADEhlbGxvIHdvcmxkIQAAAAEAAAAAAAAAAAAAAADt4FJhvNwvlQqjuhc7bjLVyRf5e4K2QOzI0c6nWfVvEAAAAASoF8gAAAAAAAAAAAG2h5ViAAAAQMc6HwYaGsrlJ8/LdE9VDVq04JifpQofSmnjhrtqaTTs/VBsNGmxi4b/vaFkLLLWh8emI8FsS/vBgb8AVFVkZQU=",
            transaction.ToEnvelopeXdrBase64());

        var transaction2 = Transaction.FromEnvelopeXdr(transaction.ToEnvelopeXdr());

        Assert.AreEqual(transaction.SourceAccount.AccountId, transaction2.SourceAccount.AccountId);
        Assert.AreEqual(transaction.SequenceNumber, transaction2.SequenceNumber);
        Assert.AreEqual(transaction.Memo, transaction2.Memo);
        Assert.AreEqual(transaction.Fee, transaction2.Fee);
        Assert.AreEqual(
            ((CreateAccountOperation)transaction.Operations[0]).StartingBalance,
            ((CreateAccountOperation)transaction2.Operations[0]).StartingBalance
        );
    }

    /// <summary>
    ///     Verifies that TransactionBuilder with time bounds creates transaction that round-trips correctly through XDR
    ///     serialization.
    /// </summary>
    [TestMethod]
    public void Build_WithTimeBounds_RoundTripsCorrectly()
    {
        // Arrange
        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var account = new Account(source.AccountId, 2908908335136768L);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .AddTimeBounds(new TimeBounds(42, 1337))
            .AddMemo(Memo.Hash("abcdef"))
            .Build();

        // Act
        transaction.Sign(source);

        // Assert
        // Convert transaction to binary XDR and back again to make sure time bounds are correctly de/serialized.
        var decodedTransaction = transaction.ToEnvelopeXdr().V1.Tx;

        Assert.AreEqual(decodedTransaction.Cond.TimeBounds.MinTime.InnerValue.InnerValue, 42U);
        Assert.AreEqual(decodedTransaction.Cond.TimeBounds.MaxTime.InnerValue.InnerValue, 1337U);

        var transaction2 = Transaction.FromEnvelopeXdr(transaction.ToEnvelopeXdr());

        Assert.AreEqual(transaction.SourceAccount.AccountId, transaction2.SourceAccount.AccountId);
        Assert.AreEqual(transaction.SequenceNumber, transaction2.SequenceNumber);
        Assert.AreEqual(transaction.Memo, transaction2.Memo);
        Assert.AreEqual(transaction.TimeBounds, transaction2.TimeBounds);
        Assert.AreEqual(transaction.Fee, transaction2.Fee);
        Assert.AreEqual(
            ((CreateAccountOperation)transaction.Operations[0]).StartingBalance,
            ((CreateAccountOperation)transaction2.Operations[0]).StartingBalance
        );
    }

    /// <summary>
    ///     Verifies that TransactionBuilder with custom fee creates transaction with correct fee.
    /// </summary>
    [TestMethod]
    public void Build_WithCustomFee_SetsCorrectFee()
    {
        // Arrange
        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var account = new Account(source.AccountId, 2908908335136768L);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .SetFee(173)
            .Build();

        // Act & Assert
        // Convert transaction to binary XDR and back again to make sure fee is correctly de/serialized.
        var decodedTransaction = transaction.ToUnsignedEnvelopeXdr().V1.Tx;

        Assert.AreEqual(decodedTransaction.Fee.InnerValue, 173 * 2U);

        var transaction2 = Transaction.FromEnvelopeXdr(transaction.ToUnsignedEnvelopeXdr());

        Assert.AreEqual(transaction.SourceAccount.AccountId, transaction2.SourceAccount.AccountId);
        Assert.AreEqual(transaction.SequenceNumber, transaction2.SequenceNumber);
        Assert.AreEqual(transaction.Memo, transaction2.Memo);
        Assert.AreEqual(transaction.TimeBounds, transaction2.TimeBounds);
        Assert.AreEqual(transaction.Fee, transaction2.Fee);
        Assert.AreEqual(
            ((CreateAccountOperation)transaction.Operations[0]).StartingBalance,
            ((CreateAccountOperation)transaction2.Operations[0]).StartingBalance
        );
        Assert.AreEqual(
            ((CreateAccountOperation)transaction.Operations[1]).StartingBalance,
            ((CreateAccountOperation)transaction2.Operations[1]).StartingBalance
        );
    }

    /// <summary>
    ///     Verifies that TransactionBuilder creates transaction with correct properties and XDR encoding on public network.
    /// </summary>
    [TestMethod]
    public void Build_WithTransactionBuilderOnPublicNetwork_CreatesTransactionWithCorrectProperties()
    {
        // Arrange
        Network.UsePublicNetwork();

        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var account = new Account(source.AccountId, 2908908335136768L);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .Build();

        // Act
        transaction.Sign(source);

        // Assert
        Assert.AreEqual(
            "AAAAAF7FIiDToW1fOYUFBC0dmyufJbFTOa2GQESGz+S2h5ViAAAAZAAKVaMAAAABAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAAEqBfIAAAAAAAAAAABtoeVYgAAAEDzfR5PgRFim5Wdvq9ImdZNWGBxBWwYkQPa9l5iiBdtPLzAZv6qj+iOfSrqinsoF0XrLkwdIcZQVtp3VRHhRoUE",
            transaction.ToEnvelopeXdrBase64(TransactionBase.TransactionXdrVersion.V0));

        Assert.AreEqual(
            "AAAAAgAAAABexSIg06FtXzmFBQQtHZsrnyWxUzmthkBEhs/ktoeVYgAAAGQAClWjAAAAAQAAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAO3gUmG83C+VCqO6FztuMtXJF/l7grZA7MjRzqdZ9W8QAAAABKgXyAAAAAAAAAAAAbaHlWIAAABA830eT4ERYpuVnb6vSJnWTVhgcQVsGJED2vZeYogXbTy8wGb+qo/ojn0q6op7KBdF6y5MHSHGUFbad1UR4UaFBA==",
            transaction.ToEnvelopeXdrBase64());
    }

    /// <summary>
    ///     Verifies that Transaction.Sign with preimage creates signature with correct hint.
    /// </summary>
    [TestMethod]
    public void Sign_WithPreimage_CreatesSignatureWithCorrectHint()
    {
        // Arrange
        Network.UsePublicNetwork();

        var source = KeyPair.FromAccountId("GBBM6BKZPEHWYO3E3YKREDPQXMS4VK35YLNU7NFBRI26RAN7GI5POFBB");
        var destination = KeyPair.FromAccountId("GDJJRRMBK4IWLEPJGIE6SXD2LP7REGZODU7WDC3I2D6MR37F4XSHBKX2");

        var account = new Account(source.AccountId, 0L);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new PaymentOperation(destination, new AssetTypeNative(), "2000"))
            .Build();

        var preimage = new byte[64];

        RandomNumberGenerator.Create().GetBytes(preimage);

        var hash = Util.Hash(preimage);

        // Act
        transaction.Sign(preimage);

        // Assert
        Assert.IsTrue(transaction.Signatures[0].Signature.InnerValue.Equals(preimage));

        var length = hash.Length;
        var rangeHashCopy = hash.Skip(length - 4).Take(4).ToArray();

        Assert.IsTrue(transaction.Signatures[0].Hint.InnerValue.SequenceEqual(rangeHashCopy));
    }

    /// <summary>
    ///     Verifies that Transaction.Sign with pre-signed signature adds signature correctly.
    /// </summary>
    [TestMethod]
    public void Sign_WithPreSignedSignature_AddsSignatureCorrectly()
    {
        // Arrange
        Network.UsePublicNetwork();

        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDJJRRMBK4IWLEPJGIE6SXD2LP7REGZODU7WDC3I2D6MR37F4XSHBKX2");

        var account = new Account(source.AccountId, 0L);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new PaymentOperation(destination, new AssetTypeNative(), "2000"))
            .Build();

        var signatureBytes = source.Sign(transaction.Hash());
        var signatureBase64 = Convert.ToBase64String(signatureBytes);

        // Act
        transaction.Sign(source.AccountId, signatureBase64);

        // Assert
        Assert.IsTrue(transaction.Signatures[0].Signature.InnerValue.SequenceEqual(signatureBytes));
        Assert.IsTrue(transaction.Signatures[0].Hint.InnerValue.SequenceEqual(source.SignatureHint.InnerValue));
    }

    /// <summary>
    ///     Verifies that ToEnvelopeXdrBase64 throws NotEnoughSignaturesException when transaction has no signatures.
    /// </summary>
    [TestMethod]
    public void ToEnvelopeXdrBase64_WithNoSignatures_ThrowsNotEnoughSignaturesException()
    {
        // Arrange
        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var account = new Account(source.AccountId, 2908908335136768L);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .Build();

        // Act & Assert
        var ex = Assert.ThrowsException<NotEnoughSignaturesException>(() => transaction.ToEnvelopeXdrBase64());
        Assert.IsTrue(ex.Message.Contains("Transaction must be signed by at least one signer."));
    }

    /// <summary>
    ///     Verifies that ToUnsignedEnvelopeXdr returns envelope XDR without throwing exception.
    /// </summary>
    [TestMethod]
    public void ToUnsignedEnvelopeXdr_WithUnsignedTransaction_ReturnsEnvelopeXdr()
    {
        // Arrange
        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var account = new Account(source.AccountId, 2908908335136768L);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .Build();

        // Act & Assert
        try
        {
            transaction.ToUnsignedEnvelopeXdr();
        }
        catch (Exception exception)
        {
            Assert.Fail("Expected no exception, but got: " + exception.Message);
        }
    }

    /// <summary>
    ///     Verifies that ToUnsignedEnvelopeXdrBase64 returns base64 encoded envelope XDR without throwing exception.
    /// </summary>
    [TestMethod]
    public void ToUnsignedEnvelopeXdrBase64_WithUnsignedTransaction_ReturnsBase64String()
    {
        // Arrange
        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var account = new Account(source.AccountId, 2908908335136768L);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .Build();

        // Act & Assert
        try
        {
            transaction.ToUnsignedEnvelopeXdrBase64();
        }
        catch (Exception exception)
        {
            Assert.Fail("Expected no exception, but got: " + exception.Message);
        }
    }

    /// <summary>
    ///     Verifies that TransactionBuilder.Build throws exception when no operations are added.
    /// </summary>
    [TestMethod]
    public void Build_WithNoOperations_ThrowsException()
    {
        // Arrange
        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");

        var account = new Account(source.AccountId, 2908908335136768L);

        // Act & Assert
        try
        {
            var unused = new TransactionBuilder(account).Build();
            Assert.Fail();
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("At least one operation required"));
            Assert.AreEqual(2908908335136768L, account.SequenceNumber);
        }
    }

    /// <summary>
    ///     Verifies that TransactionBuilder.AddMemo throws exception when memo is added twice.
    /// </summary>
    [TestMethod]
    public void AddMemo_WhenMemoAlreadyAdded_ThrowsException()
    {
        // Arrange
        // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        // Act & Assert
        try
        {
            var account = new Account(source.AccountId, 2908908335136768L);
            new TransactionBuilder(account)
                .AddOperation(new CreateAccountOperation(destination, "2000"))
                .AddMemo(Memo.None())
                .AddMemo(Memo.None());
            Assert.Fail();
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Memo has been already added."));
        }
    }

    /// <summary>
    ///     Verifies that Transaction.Hash with explicit network argument uses the specified network.
    /// </summary>
    [TestMethod]
    public void Hash_WithExplicitNetwork_UsesSpecifiedNetwork()
    {
        // Arrange
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var sequenceNumber = 2908908335136768L;
        var account = new Account(source.AccountId, sequenceNumber);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .Build();

        Network.UsePublicNetwork();
        var publicNetworkHash = transaction.Hash();

        Network.UseTestNetwork();
        var testNetworkHash = transaction.Hash();

        // Act
        var network = Network.Public();
        var explicitPublicNetworkHash = transaction.Hash(network);

        // Assert
        Assert.IsFalse(testNetworkHash.SequenceEqual(publicNetworkHash));
        Assert.IsTrue(publicNetworkHash.SequenceEqual(explicitPublicNetworkHash));
    }

    /// <summary>
    ///     Verifies that Transaction.FromEnvelopeXdr correctly deserializes transaction with memo ID.
    /// </summary>
    [TestMethod]
    public void FromEnvelopeXdr_WithMemoId_DeserializesCorrectly()
    {
        // Arrange
        // https://github.com/elucidsoft/dotnet-stellar-sdk/issues/208
        var xdrBase64 =
            "AAAAAEdL24Ttos6RnqXCsn8duaV035/QZSC9RXw29IknigHpAAAD6AFb56cAAukDAAAAAQAAAAAAAAAAAAAAAF20fKAAAAACjCiEBz2CpG0AAAABAAAAAAAAAAEAAAAADq+QhtWseqhtnwRIFyZRdLMOVtIqzkujfzUQ22rwZuEAAAAAAAAAAGZeJLcAAAAAAAAAASeKAekAAABAE+X7cGoBhuJ5SDB8WH2B1ZA2RrWIXxGtx+n6wE5d/EggDTpZhRm92b33QqjPUFOfcZ+zbcM+Ny0WR2vcYHEXDA==";

        // Act
        var tx = Transaction.FromEnvelopeXdr(xdrBase64);

        // Assert
        Assert.AreEqual("GBDUXW4E5WRM5EM6UXBLE7Y5XGSXJX472BSSBPKFPQ3PJCJHRIA6SH4C", tx.SourceAccount.AccountId);
    }

    /// <summary>
    ///     Verifies that Transaction with muxed account round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromEnvelopeXdr_WithMuxedAccount_RoundTripsCorrectly()
    {
        // Arrange
        var network = new Network("Standalone Network ; February 2017");
        var source = KeyPair.FromSecretSeed(network.NetworkId);
        var txSource = new MuxedAccountMed25519(source, 0);
        var account = new Account(txSource, 7);
        var destination = KeyPair.FromAccountId("GDQERENWDDSQZS7R7WKHZI3BSOYMV3FSWR7TFUYFTKQ447PIX6NREOJM");
        var amount = "2000";
        var asset = new AssetTypeNative();
        var tx = new TransactionBuilder(account)
            .SetFee(100)
            .AddTimeBounds(new TimeBounds(0, 0))
            .AddOperation(
                new PaymentOperation(destination, asset, amount))
            .AddMemo(new MemoText("Happy birthday!"))
            .Build();

        // Act
        var xdr = tx.ToUnsignedEnvelopeXdrBase64();
        var back = TransactionBuilder.FromEnvelopeXdr(xdr) as Transaction;

        // Assert
        Assert.IsNotNull(back);
        Assert.AreEqual(txSource.Address, back.SourceAccount.Address);
    }

    /// <summary>
    ///     Verifies that Transaction.SignatureBase throws NoNetworkSelectedException when network is null.
    /// </summary>
    [TestMethod]
    public void SignatureBase_WithNullNetwork_ThrowsNoNetworkSelectedException()
    {
        // Arrange
        var network = new Network("Standalone Network ; February 2017");
        var source = KeyPair.FromSecretSeed(network.NetworkId);
        var txSource = new MuxedAccountMed25519(source, 0);
        var account = new Account(txSource, 7);
        var destination = KeyPair.FromAccountId("GDQERENWDDSQZS7R7WKHZI3BSOYMV3FSWR7TFUYFTKQ447PIX6NREOJM");
        var amount = "2000";
        var asset = new AssetTypeNative();
        var tx = new TransactionBuilder(account)
            .SetFee(100)
            .AddTimeBounds(new TimeBounds(0, 0))
            .AddOperation(
                new PaymentOperation(destination, asset, amount))
            .AddMemo(new MemoText("Happy birthday!"))
            .Build();

        // Act & Assert
        Assert.ThrowsException<NoNetworkSelectedException>(() => tx.SignatureBase(null));
    }

    /// <summary>
    ///     Verifies that Transaction.ToXdr throws exception when source account is muxed account.
    /// </summary>
    [TestMethod]
    public void ToXdr_WithMuxedAccount_ThrowsException()
    {
        // Arrange
        var network = new Network("Standalone Network ; February 2017");
        var source = KeyPair.FromSecretSeed(network.NetworkId);
        var txSource = new MuxedAccountMed25519(source, 0);
        var account = new Account(txSource, 7);
        var destination = KeyPair.FromAccountId("GDQERENWDDSQZS7R7WKHZI3BSOYMV3FSWR7TFUYFTKQ447PIX6NREOJM");
        var amount = "2000";
        var asset = new AssetTypeNative();
        var tx = new TransactionBuilder(account)
            .SetFee(100)
            .AddTimeBounds(new TimeBounds(0, 0))
            .AddOperation(
                new PaymentOperation(destination, asset, amount))
            .AddMemo(new MemoText("Happy birthday!"))
            .Build();

        // Act & Assert
        var ex = Assert.ThrowsException<Exception>(() => tx.ToXdr());
        Assert.AreEqual("TransactionEnvelope V0 expects a KeyPair source account", ex.Message);
    }

    /// <summary>
    ///     Verifies that ToUnsignedEnvelopeXdr throws TooManySignaturesException when transaction is signed.
    /// </summary>
    [TestMethod]
    public void ToUnsignedEnvelopeXdr_WithSignedTransaction_ThrowsTooManySignaturesException()
    {
        // Arrange
        var txSource = KeyPair.Random();
        var account = new Account(txSource, 7);
        var destination = KeyPair.FromAccountId("GDQERENWDDSQZS7R7WKHZI3BSOYMV3FSWR7TFUYFTKQ447PIX6NREOJM");
        var amount = "2000";
        var asset = new AssetTypeNative();
        var tx = new TransactionBuilder(account)
            .SetFee(100)
            .AddTimeBounds(new TimeBounds(0, 0))
            .AddOperation(
                new PaymentOperation(destination, asset, amount))
            .AddMemo(new MemoText("Happy birthday!"))
            .Build();

        tx.Sign(KeyPair.Random());

        // Act & Assert
        var ex = Assert.ThrowsException<TooManySignaturesException>(() => tx.ToUnsignedEnvelopeXdr());
        Assert.AreEqual("Transaction must not be signed. Use ToEnvelopeXDR.", ex.Message);
    }

    /// <summary>
    ///     Verifies that TransactionBuilder.Build throws OverflowException when fee multiplied by operation count overflows.
    /// </summary>
    [TestMethod]
    public void Build_WithFeeOverflow_ThrowsOverflowException()
    {
        // Arrange
        var source = KeyPair.Random();
        var txSource = new MuxedAccountMed25519(source, 0);
        var account = new Account(txSource, 7);
        var destination = KeyPair.FromAccountId("GDQERENWDDSQZS7R7WKHZI3BSOYMV3FSWR7TFUYFTKQ447PIX6NREOJM");
        var amount = "2000";
        var asset = new AssetTypeNative();

        // Act & Assert
        Assert.ThrowsException<OverflowException>(() =>
        {
            new TransactionBuilder(account)
                .SetFee(uint.MaxValue)
                .AddTimeBounds(new TimeBounds(0, 0))
                .AddOperation(
                    new PaymentOperation(destination, asset, amount))
                .AddOperation(
                    new PaymentOperation(destination, asset, amount))
                .Build();
        });
    }

    /// <summary>
    ///     Verifies that Transaction with Soroban transaction data round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromEnvelopeXdr_WithSorobanData_RoundTripsCorrectly()
    {
        // Arrange
        var network = new Network("Standalone Network ; February 2017");
        var source = KeyPair.FromSecretSeed(network.NetworkId);
        var txSource = new MuxedAccountMed25519(source, 0);
        var account = new Account(txSource, 7);
        var destination = KeyPair.FromAccountId("GDQERENWDDSQZS7R7WKHZI3BSOYMV3FSWR7TFUYFTKQ447PIX6NREOJM");
        var amount = "2000";
        var asset = new AssetTypeNative();
        var keyAccount = new LedgerKeyAccount(KeyPair.Random());
        var keyData = new LedgerKeyData(KeyPair.Random(), "firstKey");
        var footprint = new LedgerFootprint
        {
            ReadOnly = [keyAccount],
            ReadWrite = [keyData],
        };
        var sorobanData = new SorobanTransactionData(new SorobanResources(footprint, 10, 20, 30), 100);

        var tx = new TransactionBuilder(account)
            .SetFee(100)
            .AddTimeBounds(new TimeBounds(0, 0))
            .AddOperation(
                new PaymentOperation(destination, asset, amount))
            .AddMemo(new MemoText("Happy birthday!"))
            .SetSorobanTransactionData(sorobanData)
            .Build();

        // Act
        var xdr = tx.ToUnsignedEnvelopeXdrBase64();
        var decodedTx = (Transaction)TransactionBuilder.FromEnvelopeXdr(xdr);
        // Assert
        Assert.IsNotNull(decodedTx);
        Assert.AreEqual(txSource.Address, decodedTx.SourceAccount.Address);

        var decodedSorobanData = decodedTx.SorobanTransactionData;
        Assert.IsNotNull(decodedSorobanData);
        Assert.AreEqual(sorobanData.ResourceFee, decodedSorobanData.ResourceFee);
        Assert.AreEqual(sorobanData.Resources.Instructions, decodedSorobanData.Resources.Instructions);
        Assert.AreEqual(sorobanData.Resources.DiskReadBytes, decodedSorobanData.Resources.DiskReadBytes);
        Assert.AreEqual(sorobanData.Resources.WriteBytes, decodedSorobanData.Resources.WriteBytes);

        var decodedFootprint = decodedSorobanData.Resources.Footprint;
        Assert.AreEqual(footprint.ReadOnly.Length, decodedFootprint.ReadOnly.Length);
        Assert.AreEqual(keyAccount.Account.AccountId,
            ((LedgerKeyAccount)decodedFootprint.ReadOnly[0]).Account.AccountId);
        Assert.AreEqual(keyData.Account.AccountId,
            ((LedgerKeyData)decodedFootprint.ReadWrite[0]).Account.AccountId);
        Assert.AreEqual(keyData.DataName, ((LedgerKeyData)decodedFootprint.ReadWrite[0]).DataName);
    }
}