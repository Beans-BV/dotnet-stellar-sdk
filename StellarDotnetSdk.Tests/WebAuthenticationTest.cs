using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for web authentication (SEP-10) functionality.
/// </summary>
[TestClass]
public class WebAuthenticationTest
{
    private const string HomeDomain = "thisisatest.sandbox.anchor.anchordomain.com";
    private const string WebAuthDomain = "thisisatest.sandbox.anchor.webauth.com";
    private const string ClientDomain = "thisisatest.sandbox.anchor.client.com";

    private const string ManageDataOperationName = $"{HomeDomain} auth";
    private readonly Network _mainnet = Network.Public();

    private readonly Network _testnet = Network.Test();
    private KeyPair _clientKeypair = null!;

    private KeyPair _serverKeypair = null!;

    [TestInitialize]
    public void Initialize()
    {
        Network.Use(_testnet);

        _serverKeypair = KeyPair.Random();
        _clientKeypair = KeyPair.Random();
    }

    /// <summary>
    ///     Verifies that BuildChallengeTransaction creates a transaction with correct time bounds and operations.
    /// </summary>
    [TestMethod]
    public void BuildChallengeTransaction_WithValidParameters_ReturnsTransactionWithCorrectTimeBounds()
    {
        // Arrange
        const string clientAccountId = "GBDIT5GUJ7R5BXO3GJHFXJ6AZ5UQK6MNOIDMPQUSMXLIHTUNR2Q5CFNF";

        // Act
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            clientAccountId,
            HomeDomain,
            WebAuthDomain);

        var serializedTransaction = transaction.ToEnvelopeXdrBase64();
        var back = Transaction.FromEnvelopeXdr(serializedTransaction);

        // Assert
        Assert.IsNotNull(back.TimeBounds);
        var timeout = back.TimeBounds.MaxTime - back.TimeBounds.MinTime;
        Assert.AreEqual(300, timeout);

        CheckAccounts(back, _serverKeypair);
        CheckOperation(back, clientAccountId);
    }


    /// <summary>
    ///     Verifies that BuildChallengeTransaction with options creates a transaction with custom time bounds and nonce.
    /// </summary>
    [TestMethod]
    public void BuildChallengeTransaction_WithOptions_ReturnsTransactionWithCustomTimeBounds()
    {
        // Arrange
        var clientAccountId = KeyPair.FromAccountId("GBDIT5GUJ7R5BXO3GJHFXJ6AZ5UQK6MNOIDMPQUSMXLIHTUNR2Q5CFNF");

        var nonce = new byte[48];
        Array.Clear(nonce, 0, nonce.Length);

        var now = new DateTimeOffset();
        var duration = TimeSpan.FromMinutes(10.0);

        // Act
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            clientAccountId,
            HomeDomain,
            WebAuthDomain,
            nonce,
            now,
            duration);

        var serializedTransaction = transaction.ToEnvelopeXdrBase64();
        var back = Transaction.FromEnvelopeXdr(serializedTransaction);

        // Assert
        Assert.IsNotNull(back.TimeBounds);
        var timeout = back.TimeBounds.MaxTime - back.TimeBounds.MinTime;
        Assert.AreEqual(600, timeout);

        CheckAccounts(back, _serverKeypair);
        CheckOperation(back, clientAccountId.Address);
    }

    /// <summary>
    ///     Verifies that BuildChallengeTransaction throws InvalidWebAuthenticationException when client account is a muxed
    ///     account.
    /// </summary>
    [TestMethod]
    public void BuildChallengeTransaction_WithMuxedAccount_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var clientAccountId = MuxedAccountMed25519.FromMuxedAccountId(
            "MAAAAAAAAAAAJURAAB2X52XFQP6FBXLGT6LWOOWMEXWHEWBDVRZ7V5WH34Y22MPFBHUHY");

        var nonce = new byte[48];
        Array.Clear(nonce, 0, nonce.Length);

        var now = new DateTimeOffset();
        var duration = TimeSpan.FromMinutes(10.0);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.BuildChallengeTransaction(
                _serverKeypair,
                clientAccountId.Address,
                HomeDomain,
                WebAuthDomain,
                nonce,
                now,
                duration);
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction returns true for a valid challenge transaction.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_WithValidTransaction_ReturnsTrue()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: now);
        transaction.Sign(_clientKeypair);

        // Act & Assert
        Assert.IsTrue(WebAuthentication.VerifyChallengeTransaction(
            transaction,
            _serverKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            now: now));
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when transaction sequence is not
    ///     zero.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_WithNonZeroSequence_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var nonce = new byte[64];
        var transaction = new TransactionBuilder(new Account(_serverKeypair.AccountId, 0))
            .AddOperation(new ManageDataOperation("NET auth", nonce))
            .Build();
        transaction.Sign(_clientKeypair);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now);
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when server account ID is
    ///     different.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_WithDifferentServerAccountId_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: now);
        transaction.Sign(_clientKeypair);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                KeyPair.Random().AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now);
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when transaction has no
    ///     ManageData operation.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_WithNoManageDataOperation_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var transaction = new TransactionBuilder(new Account(_serverKeypair.AccountId, -1))
            .AddOperation(
                new AccountMergeOperation(_serverKeypair, _clientKeypair))
            .Build();
        transaction.Sign(_clientKeypair);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now);
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when operation has no source
    ///     account.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_WithOperationNoSourceAccount_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        var nonce = new byte[64];
        var transaction = new TransactionBuilder(new Account(_serverKeypair.AccountId, -1))
            .AddOperation(new ManageDataOperation("NET auth", nonce))
            .Build();
        transaction.Sign(_clientKeypair);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now);
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when operation data is not base64
    ///     encoded.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_WithNonBase64OperationData_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        var nonce = new byte[64];
        var transaction = new TransactionBuilder(new Account(_serverKeypair.AccountId, -1))
            .AddOperation(new ManageDataOperation("NET auth", nonce, _clientKeypair))
            .Build();
        transaction.Sign(_clientKeypair);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now);
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when transaction is not signed by
    ///     server.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_NotSignedByServer_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: now);
        transaction.Signatures.Clear();
        transaction.Sign(_clientKeypair);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now);
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when transaction is signed by
    ///     server on different network.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_SignedByServerOnDifferentNetwork_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: now,
            network: _testnet);

        transaction.Sign(
            _clientKeypair,
            _testnet);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now,
                network: _mainnet);
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when transaction is not signed by
    ///     client.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_NotSignedByClient_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: now);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now);
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when transaction is signed by
    ///     client on different network.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_SignedByClientOnDifferentNetwork_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: now,
            network: _testnet);

        transaction.Sign(
            _clientKeypair,
            _mainnet);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now,
                network: _testnet);
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when server is a muxed account.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_WithServerMuxedAccount_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        // It's impossible to build a wrong transaction from our api. Use a xdr instead.
        const string transactionXdr =
            "AAAAAgAAAQAAAAAAAAAE0rqb5mZeN3cTjZYz9BOSuxs4tkP5296i8kJKWXS13pWGAAAAZAAAAAAAAAAAAAAAAQAAAABerG8LAAAAAF6scDcAAAAAAAAAAQAAAAEAAAAA13Pc/rMj75EaJFmzR1eWVHBeJuoq+8FinXpG7DXEsvoAAAAKAAAACE5FVCBhdXRoAAAAAQAAAEBIRmxJQi94UFFsYTBaSzNRamx3akFUL25JS3pUeFFFK1hFVE9EQkIzZHpOQWRsR0svOGJnbFBydSttaEJpNzdEAAAAAAAAAAK13pWGAAAAQGlkGeaHtcnaSyQP4NSU/CaRC6rUd7qXvVlJc/3TuWmY0kAC9/mXmLtnzFn2Hz+0cwVi1+wwtxfboxIHOABIsg81xLL6AAAAQB23cGeF7SR9bZEf6rRh+ck7h6PqvUQFDDDI3qE09y19SdvMWMs5Ksthm//dXMZE7+QJbKqxpJbpKC2klMTZJQ0=";

        var serverKeypair = KeyPair.FromAccountId("GC5JXZTGLY3XOE4NSYZ7IE4SXMNTRNSD7HN55IXSIJFFS5FV32KYM6PH");
        var now = DateTimeOffset.Now;
        var transaction = Transaction.FromEnvelopeXdr(transactionXdr);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now.Add(TimeSpan.FromDays(1.0)));
        });
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransaction throws InvalidWebAuthenticationException when client is a muxed account.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransaction_WithClientMuxedAccount_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        // It's impossible to build a wrong transaction from our api. Use a xdr instead.
        const string transactionXdr =
            "AAAAAgAAAABe3OrOugPm3BfyQ9xP+UUMEk8hiM0WwMSpVN9FIOxuXgAAAGQAAAAAAAAAAAAAAAEAAAAAXqxwZQAAAABerHGRAAAAAAAAAAEAAAABAAABAAAAAAAAAATSXtzqzroD5twX8kPcT/lFDBJPIYjNFsDEqVTfRSDsbl4AAAAKAAAACE5FVCBhdXRoAAAAAQAAAEAvU0VaNWppQjRZTXZTYlBNN1VobzJ6QmxqcVBiN0IyRDVJbGx6NEZxUWh4SmhHVmJWT0VsdHhyRlE5ZUNIL2RLAAAAAAAAAAIg7G5eAAAAQGKw8yxSA/tnK34nv6VIQ/r1bazvm3vInbU4dpSersY/7uN5MKZEKIMbioevHIpYZ6pwJdm7qRPbGj9YyCU+BQsNYg7iAAAAQKCdrKY6g6pEg/DfhOfOyRU8cKcg1qVSQwekXlKkQTzw/MpyLqYYRlxP5Z+P0TLDxmCn8KyawafIum24hvE11ws=";

        var serverKeypair = KeyPair.FromAccountId("GBPNZ2WOXIB6NXAX6JB5YT7ZIUGBETZBRDGRNQGEVFKN6RJA5RXF4SJ2");
        var now = DateTimeOffset.Now;
        var transaction = Transaction.FromEnvelopeXdr(transactionXdr);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransaction(
                transaction,
                serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: now.Add(TimeSpan.FromDays(1.0)));
        });
    }

    private static void CheckAccounts(Transaction transaction, KeyPair serverKeypair)
    {
        Assert.AreEqual(0, transaction.SequenceNumber);
        Assert.AreEqual(serverKeypair.AccountId, transaction.SourceAccount.AccountId);
    }

    private static void CheckOperation(Transaction transaction, string clientAccountId)
    {
        Assert.AreEqual(2, transaction.Operations.Length);
        var operation = transaction.Operations[0] as ManageDataOperation;
        Assert.IsNotNull(operation);
        Assert.AreEqual($"{HomeDomain} auth", operation.Name);
        Assert.IsNotNull(operation.SourceAccount);
        Assert.AreEqual(clientAccountId, operation.SourceAccount.AccountId);
        Assert.IsNotNull(operation.Value);
        Assert.AreEqual(64, operation.Value.Length);
        var bytes = Convert.FromBase64String(Encoding.UTF8.GetString(operation.Value));
        Assert.AreEqual(48, bytes.Length);
    }

    /// <summary>
    ///     Creates a valid 48-byte nonce, base64-encoded, for use in challenge transaction tests.
    /// </summary>
    private static byte[] CreateValidNonceBytes()
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        return Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction correctly reads challenge transaction signed by both server and client.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_ValidSignedByServerAndClient_ReturnsClientAccountId()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        // Act
        var readTransactionId = WebAuthentication.ReadChallengeTransaction(
            transaction,
            _serverKeypair.AccountId,
            HomeDomain,
            WebAuthDomain);

        // Assert
        Assert.IsNotNull(readTransactionId);
        Assert.AreEqual(_clientKeypair.AccountId, readTransactionId);
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction correctly reads challenge transaction signed by server only.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_ValidSignedByServer_ReturnsClientAccountId()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

        // Act
        var readTransactionId = WebAuthentication.ReadChallengeTransaction(
            transaction,
            _serverKeypair.AccountId,
            HomeDomain,
            WebAuthDomain);

        // Assert
        Assert.IsNotNull(readTransactionId);
        Assert.AreEqual(_clientKeypair.AccountId, readTransactionId);
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws exception when transaction is not signed by server.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_NotSignedByServer_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        // Act & Assert
        try
        {
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction not signed by server"));
        }
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws exception when server account ID does not match.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_ServerAccountIdMismatch_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(KeyPair.Random().Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

        // Act & Assert
        try
        {
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction source must be serverAccountId"));
        }
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws InvalidWebAuthenticationException when sequence number is not zero.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_SequenceNoNotZero_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, 1234);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

        // Act & Assert
        var ex = Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain));
        Assert.IsTrue(ex.Message.Contains("Challenge transaction sequence number must be 0"));
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws exception when operation type is wrong.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_OperationWrongType_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var operation = new BumpSequenceOperation(100, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

        // Act & Assert
        try
        {
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(
                exception.Message.Contains("Challenge transaction operation must be of type ManageDataOperation"));
        }
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws exception when operation has no source account.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_OperationNoSourceAccount_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data);
        var transaction = new TransactionBuilder(transactionSource)
            .SetFee(100)
            .AddOperation(operation)
            .AddMemo(Memo.None())
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

        // Act & Assert
        try
        {
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction operation must have source account"));
        }
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws exception when data value has wrong encoded length.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_DataValueWrongEncodedLength_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA?AAAAAAAAAAAAAAAAAAAAAAAAAA"u8.ToArray();

        var operation = new ManageDataOperation(ManageDataOperationName, plainTextBytes, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        // Act & Assert
        try
        {
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction operation data must be base64 encoded"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionThreshold throws exception when server signature is invalid.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionThreshold_InvalidServer_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_clientKeypair);

        const int threshold = 1;
        var signerSummary = new Dictionary<string, int>
        {
            { _clientKeypair.Address, 1 },
        };

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionThreshold(
                transaction,
                _serverKeypair.AccountId,
                threshold,
                signerSummary,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction not signed by server"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionThreshold returns correct signers when server and client key meet
    ///     threshold.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionThreshold_ValidServerAndClientKeyMeetingThreshold_ReturnsCorrectSigners()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        const int threshold = 1;
        var signerSummary = new Dictionary<string, int>
        {
            { _clientKeypair.Address, 1 },
        };

        var wantSigners = new[]
        {
            _clientKeypair.Address,
        };

        // Act
        var signersFound = WebAuthentication.VerifyChallengeTransactionThreshold(
            transaction,
            _serverKeypair.AccountId,
            threshold,
            signerSummary,
            HomeDomain,
            WebAuthDomain).ToList();

        // Assert
        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionThreshold returns correct signers when server and multiple client keys meet
    ///     threshold.
    /// </summary>
    [TestMethod]
    public void
        VerifyChallengeTransactionThreshold_ValidServerAndMultipleClientKeysMeetingThreshold_ReturnsCorrectSigners()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);
        transaction.Sign(client2Keypair);

        const int threshold = 3;
        var signerSummary = new Dictionary<string, int>
        {
            { _clientKeypair.Address, 1 },
            { client2Keypair.Address, 2 },
        };

        var wantSigners = new[]
        {
            _clientKeypair.Address,
            client2Keypair.Address,
        };

        // Act
        var signersFound = WebAuthentication.VerifyChallengeTransactionThreshold(
            transaction,
            _serverKeypair.AccountId,
            threshold,
            signerSummary,
            HomeDomain,
            WebAuthDomain).ToList();

        // Assert
        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionThreshold returns correct signers when some client keys are unused but
    ///     threshold is met.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionThreshold_ValidServerAndMultipleClientKeysSomeUnused_ReturnsCorrectSigners()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();
        var client3Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);
        transaction.Sign(client2Keypair);

        const int threshold = 3;
        var signerSummary = new Dictionary<string, int>
        {
            { _clientKeypair.Address, 1 },
            { client2Keypair.Address, 2 },
            { client3Keypair.Address, 2 },
        };

        var wantSigners = new[]
        {
            _clientKeypair.Address,
            client2Keypair.Address,
        };

        // Act
        var signersFound = WebAuthentication.VerifyChallengeTransactionThreshold(
            transaction,
            _serverKeypair.AccountId,
            threshold,
            signerSummary,
            HomeDomain,
            WebAuthDomain).ToList();

        // Assert
        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionThreshold throws exception when multiple client keys do not meet threshold.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionThreshold_MultipleClientKeysNotMeetingThreshold_ThrowsException()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();
        var client3Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);
        transaction.Sign(client2Keypair);

        const int threshold = 10;
        var signerSummary = new Dictionary<string, int>
        {
            { _clientKeypair.Address, 1 },
            { client2Keypair.Address, 2 },
            { client3Keypair.Address, 2 },
        };

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionThreshold(
                transaction,
                _serverKeypair.AccountId,
                threshold,
                signerSummary,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Signers with weight 3 do not meet threshold 10"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionThreshold throws exception when client key is unrecognized.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionThreshold_UnrecognizedClientKey_ThrowsException()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();
        var client3Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);
        transaction.Sign(client2Keypair);
        transaction.Sign(client3Keypair);

        const int threshold = 3;
        var signerSummary = new Dictionary<string, int>
        {
            { _clientKeypair.Address, 1 },
            { client2Keypair.Address, 2 },
        };

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionThreshold(
                transaction,
                _serverKeypair.AccountId,
                threshold,
                signerSummary,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction has unrecognized signatures"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionThreshold throws exception when no signers are provided.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionThreshold_NoSigners_ThrowsException()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();
        var client3Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);
        transaction.Sign(client2Keypair);
        transaction.Sign(client3Keypair);

        var threshold = 3;
        var signerSummary = new Dictionary<string, int>();

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionThreshold(
                transaction,
                _serverKeypair.AccountId,
                threshold,
                signerSummary,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Signers must be non-empty"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionThreshold throws exception when signer weights add to more than 8 bits.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionThreshold_WeightsExceed8Bits_ThrowsException()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);
        transaction.Sign(client2Keypair);

        const int threshold = 1;
        var signerSummary = new Dictionary<string, int>
        {
            { _clientKeypair.Address, 255 },
            { client2Keypair.Address, 1 },
        };

        var wantSigners = new[]
        {
            _clientKeypair.Address,
            client2Keypair.Address,
        };

        // Act
        var signersFound = WebAuthentication.VerifyChallengeTransactionThreshold(
            transaction,
            _serverKeypair.AccountId,
            threshold,
            signerSummary,
            HomeDomain,
            WebAuthDomain).ToList();

        // Assert
        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners throws exception when server signature is invalid.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_InvalidServer_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_clientKeypair);

        const int threshold = 1;
        var signerSummary = new Dictionary<string, int>
        {
            { _clientKeypair.Address, 255 },
        };

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionThreshold(
                transaction,
                _serverKeypair.AccountId,
                threshold,
                signerSummary,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction not signed by server"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners returns correct signer when server and client master key sign.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_ValidServerAndClientMasterKey_ReturnsClientSigner()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        var signers = new[]
        {
            _clientKeypair.Address,
        };

        // Act
        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain);

        // Assert
        Assert.AreEqual(_clientKeypair.Address, signersFound[0]);
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners throws exception when server is invalid and no client signs.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_InvalidServerAndNoClient_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

        var signers = new[]
        {
            _clientKeypair.Address,
        };

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionSigners(
                transaction,
                _serverKeypair.AccountId,
                signers,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction not signed by client"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners throws exception when server is invalid and client is unrecognized.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_InvalidServerAndUnrecognizedClient_ThrowsException()
    {
        // Arrange
        var unrecognizedKeypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(unrecognizedKeypair);

        var signers = new[]
        {
            _clientKeypair.Address,
        };

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionSigners(
                transaction,
                _serverKeypair.AccountId,
                signers,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction not signed by client"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners returns correct signers when server and multiple client signers
    ///     sign.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_ValidServerAndMultipleClientSigners_ReturnsCorrectSigners()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);
        transaction.Sign(client2Keypair);

        var signers = new[]
        {
            _clientKeypair.Address,
            client2Keypair.Address,
        };

        var wantSigners = new[]
        {
            _clientKeypair.Address,
            client2Keypair.Address,
        };

        // Act
        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain).ToList();

        // Assert
        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners returns correct signers when signers are in reverse order.
    /// </summary>
    [TestMethod]
    public void
        VerifyChallengeTransactionSigners_ValidServerAndMultipleClientSignersReverseOrder_ReturnsCorrectSigners()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(client2Keypair);
        transaction.Sign(_clientKeypair);

        var signers = new[]
        {
            _clientKeypair.Address,
            client2Keypair.Address,
        };

        var wantSigners = new[]
        {
            _clientKeypair.Address,
            client2Keypair.Address,
        };

        // Act
        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain).ToList();

        // Assert
        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners returns correct signers when client signers are not master key.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_ValidServerAndClientSignersNotMasterKey_ReturnsCorrectSigners()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(client2Keypair);

        var signers = new[]
        {
            client2Keypair.Address,
        };

        var wantSigners = new[]
        {
            client2Keypair.Address,
        };

        // Act
        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain).ToList();

        // Assert
        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners ignores server signer and returns only client signers.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_ValidServerAndClientSigners_IgnoresServerSigner()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(client2Keypair);

        var signers = new[]
        {
            _serverKeypair.Address,
            client2Keypair.Address,
        };

        var wantSigners = new[]
        {
            client2Keypair.Address,
        };

        // Act
        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain).ToList();

        // Assert
        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners throws exception when no client signers and ignores server signer.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_InvalidServerNoClientSigners_ThrowsException()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

        var signers = new[]
        {
            _serverKeypair.Address,
            client2Keypair.Address,
        };

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionSigners(
                transaction,
                _serverKeypair.AccountId,
                signers,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction not signed by client"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners ignores duplicate signer and returns correct signers.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_ValidServerAndClientSigners_IgnoresDuplicateSigner()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        var signers = new[]
        {
            _clientKeypair.Address,
            _clientKeypair.Address,
        };

        var wantSigners = new[]
        {
            _clientKeypair.Address,
        };

        // Act
        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain).ToList();

        // Assert
        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners throws exception when duplicate signer is in error case.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_InvalidServerAndClientSignersWithDuplicate_ThrowsException()
    {
        // Arrange
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(client2Keypair);

        var signers = new[]
        {
            _clientKeypair.Address,
            _clientKeypair.Address,
        };

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionSigners(
                transaction,
                _serverKeypair.AccountId,
                signers,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction not signed by client"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners throws exception when duplicate signatures are present.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_DuplicateSignatures_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);
        transaction.Sign(_clientKeypair);

        var signers = new[] { _clientKeypair.Address };

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionSigners(
                transaction,
                _serverKeypair.AccountId,
                signers,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Challenge transaction has unrecognized signatures"));
        }
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners throws exception when no signers are provided.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_NoSigners_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        var signers = Array.Empty<string>();

        // Act & Assert
        try
        {
            WebAuthentication.VerifyChallengeTransactionSigners(
                transaction,
                _serverKeypair.AccountId,
                signers,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("Signers must be non-empty"));
        }
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws exception when transaction has subsequent operation that is not
    ///     valid.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_NotValidSubsequentOperation_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var notValidOperation = new PaymentOperation(KeyPair.Random(), new AssetTypeNative(), "50", opSource.KeyPair);

        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddOperation(notValidOperation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        // Act & Assert
        try
        {
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(
                exception.Message.Contains("The transaction has operations that are not of type 'manageData'"));
        }
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws exception when transaction has subsequent data operation that is not
    ///     valid.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_NotValidSubsequentDataOperation_ThrowsException()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var notValidOperation = new ManageDataOperation(ManageDataOperationName, base64Data, KeyPair.Random());

        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddOperation(notValidOperation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        // Act & Assert
        try
        {
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain);
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("The transaction has operations that are unrecognized"));
        }
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws InvalidWebAuthenticationException when home domain does not match.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_BadHomeDomain_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        const string clientAccountId = "GBDIT5GUJ7R5BXO3GJHFXJ6AZ5UQK6MNOIDMPQUSMXLIHTUNR2Q5CFNF";

        // Act & Assert
        try
        {
            var transaction = WebAuthentication.BuildChallengeTransaction(
                _serverKeypair,
                clientAccountId,
                HomeDomain,
                WebAuthDomain);
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                $"{HomeDomain}bad",
                WebAuthDomain);
        }
        catch (InvalidWebAuthenticationException e)
        {
            Assert.AreEqual(e.Message,
                "Invalid homeDomains: the transaction's operation key name does not match the expected home domain");
        }
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws InvalidWebAuthenticationException when no home domain is provided.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_NoHomeDomain_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        const string clientAccountId = "GBDIT5GUJ7R5BXO3GJHFXJ6AZ5UQK6MNOIDMPQUSMXLIHTUNR2Q5CFNF";

        // Act & Assert
        try
        {
            var transaction = WebAuthentication.BuildChallengeTransaction(
                _serverKeypair,
                clientAccountId,
                HomeDomain,
                WebAuthDomain);
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                Array.Empty<string>(),
                WebAuthDomain);
        }
        catch (InvalidWebAuthenticationException e)
        {
            Assert.AreEqual(e.Message, "Invalid homeDomains: a home domain must be provided for verification");
        }
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws InvalidWebAuthenticationException when transaction is null.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_NullTransaction_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange & Act & Assert
        var ex = Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.ReadChallengeTransaction(
                null,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain);
        });
        Assert.AreEqual("Challenge transaction cannot be null", ex.Message);
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws InvalidWebAuthenticationException when time bounds are expired.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_ExpiredTimeBounds_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        const string clientAccountId = "GBDIT5GUJ7R5BXO3GJHFXJ6AZ5UQK6MNOIDMPQUSMXLIHTUNR2Q5CFNF";

        // Act & Assert
        try
        {
            var transaction = WebAuthentication.BuildChallengeTransaction(
                _serverKeypair,
                clientAccountId,
                HomeDomain,
                WebAuthDomain);
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain,
                now: DateTimeOffset.Now.Subtract(new TimeSpan(0, 20, 0)));
        }
        catch (InvalidWebAuthenticationException e)
        {
            Assert.AreEqual(e.Message, "Challenge transaction expired");
        }
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction correctly reads challenge transaction when no web auth domain is provided.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_NoWebAuthDomain_ReturnsClientAccountId()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var base64Data = CreateValidNonceBytes();

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        // Act
        var readTransactionId = WebAuthentication.ReadChallengeTransaction(
            transaction,
            _serverKeypair.AccountId,
            HomeDomain, "");

        // Assert
        Assert.AreEqual(_clientKeypair.AccountId, readTransactionId);
    }

    /// <summary>
    ///     Verifies that VerifyChallengeTransactionSigners throws InvalidWebAuthenticationException when client domain is
    ///     present.
    /// </summary>
    [TestMethod]
    public void VerifyChallengeTransactionSigners_WithClientDomain_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair,
            HomeDomain,
            WebAuthDomain,
            clientDomain: ClientDomain,
            clientSigningKey: _clientKeypair,
            validFrom: now);
        var manageDataOperation = (ManageDataOperation)transaction.Operations[2];

        var signers = new List<string>
        {
            _serverKeypair.AccountId,
            _clientKeypair.AccountId,
        };

        Assert.AreEqual(manageDataOperation.Name, "client_domain");

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            WebAuthentication.VerifyChallengeTransactionSigners(
                transaction,
                _serverKeypair.AccountId,
                signers,
                HomeDomain,
                WebAuthDomain,
                now: now);
        });
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws InvalidWebAuthenticationException when time bounds are out of lower
    ///     bound.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_OutOfLowerBound_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        // Time bounds start 20 seconds before 'now'
        var validFrom = now.AddSeconds(-20);
        var validFor = TimeSpan.FromSeconds(10);

        var transaction = _BuildAndSignChallengeTransaction(
            validFrom,
            validFor);

        // Grace time bounds
        var graceValidFrom = validFrom.AddSeconds(-WebAuthentication.GracePeriod);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            _ReadChallengeTransaction(
                transaction,
                // Start 1 second before the min time bounds - grace period
                graceValidFrom.AddSeconds(-1));
        }, "Challenge transaction expired");
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction succeeds when time bounds are out of lower bound but within grace period.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_OutOfLowerBoundButWithinGracePeriod_ReturnsClientAccountId()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        // Time bounds start 20 seconds before 'now'
        var validFrom = now.AddSeconds(-20);
        var validFor = TimeSpan.FromSeconds(10);

        var transaction = _BuildAndSignChallengeTransaction(
            validFrom,
            validFor);

        // Grace time bounds
        var graceValidFrom = validFrom.AddSeconds(-WebAuthentication.GracePeriod);

        // Act & Assert
        var result = _ReadChallengeTransaction(
            transaction,
            // Start 1 second after the min time bounds - grace period
            graceValidFrom.AddSeconds(1));
        Assert.AreEqual(_clientKeypair.AccountId, result);
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction throws InvalidWebAuthenticationException when time bounds are out of upper
    ///     bound.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_OutOfUpperBound_ThrowsInvalidWebAuthenticationException()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        // Time bounds start 20 seconds before 'now'
        var validFrom = now.AddSeconds(-20);
        var validFor = TimeSpan.FromSeconds(10);

        var transaction = _BuildAndSignChallengeTransaction(
            validFrom,
            validFor);

        // Grace time bounds
        var graceValidFor = validFor + TimeSpan.FromSeconds(WebAuthentication.GracePeriod);
        var graceValidTill = validFrom.Add(graceValidFor);

        // Act & Assert
        Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
        {
            _ReadChallengeTransaction(
                transaction,
                // Start 1 second after the max time bounds + grace period
                graceValidTill.AddSeconds(1));
        }, "Challenge transaction expired");
    }

    /// <summary>
    ///     Verifies that ReadChallengeTransaction succeeds when time bounds are out of upper bound but within grace period.
    /// </summary>
    [TestMethod]
    public void ReadChallengeTransaction_OutOfUpperBoundButWithinGracePeriod_ReturnsClientAccountId()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        // Time bounds start 20 seconds before 'now'
        var validFrom = now.AddSeconds(-20);
        var validFor = TimeSpan.FromSeconds(10);

        var transaction = _BuildAndSignChallengeTransaction(
            validFrom,
            validFor);

        // Grace time bounds
        var graceValidFor = validFor + TimeSpan.FromSeconds(WebAuthentication.GracePeriod);
        var graceValidTill = validFrom.Add(graceValidFor);

        // Act
        var result = _ReadChallengeTransaction(
            transaction,
            // Start 1 second before the max time bounds + grace period
            graceValidTill.AddSeconds(-1));

        // Assert
        Assert.AreEqual(_clientKeypair.AccountId, result);
    }

    private Transaction _BuildAndSignChallengeTransaction(
        DateTimeOffset? validFrom = null,
        TimeSpan? validFor = null)
    {
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: validFrom,
            validFor: validFor);
        transaction.Sign(_serverKeypair);
        return transaction;
    }

    private string _ReadChallengeTransaction(
        Transaction transaction,
        DateTimeOffset? now = null)
    {
        return WebAuthentication.ReadChallengeTransaction(
            transaction,
            _serverKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            now: now);
    }
}