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

    [TestMethod]
    public void TestBuildChallengeTransaction()
    {
        const string clientAccountId = "GBDIT5GUJ7R5BXO3GJHFXJ6AZ5UQK6MNOIDMPQUSMXLIHTUNR2Q5CFNF";
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            clientAccountId,
            HomeDomain,
            WebAuthDomain);

        var serializedTransaction = transaction.ToEnvelopeXdrBase64();
        var back = Transaction.FromEnvelopeXdr(serializedTransaction);

        Assert.IsNotNull(back.TimeBounds);
        var timeout = back.TimeBounds.MaxTime - back.TimeBounds.MinTime;
        Assert.AreEqual(300, timeout);

        CheckAccounts(back, _serverKeypair);
        CheckOperation(back, clientAccountId);
    }


    [TestMethod]
    public void TestBuildChallengeTransactionWithOptions()
    {
        var clientAccountId = KeyPair.FromAccountId("GBDIT5GUJ7R5BXO3GJHFXJ6AZ5UQK6MNOIDMPQUSMXLIHTUNR2Q5CFNF");

        var nonce = new byte[48];
        Array.Clear(nonce, 0, nonce.Length);

        var now = new DateTimeOffset();
        var duration = TimeSpan.FromMinutes(10.0);

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

        Assert.IsNotNull(back.TimeBounds);
        var timeout = back.TimeBounds.MaxTime - back.TimeBounds.MinTime;
        Assert.AreEqual(600, timeout);

        CheckAccounts(back, _serverKeypair);
        CheckOperation(back, clientAccountId.Address);
    }

    [TestMethod]
    public void TestBuildChallengeTransactionFailsWithMuxedAccount()
    {
        var clientAccountId = MuxedAccountMed25519.FromMuxedAccountId(
            "MAAAAAAAAAAAJURAAB2X52XFQP6FBXLGT6LWOOWMEXWHEWBDVRZ7V5WH34Y22MPFBHUHY");

        var nonce = new byte[48];
        Array.Clear(nonce, 0, nonce.Length);

        var now = new DateTimeOffset();
        var duration = TimeSpan.FromMinutes(10.0);

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

    [TestMethod]
    public void TestVerifyChallengeTransactionReturnsTrueForValidTransaction()
    {
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: now);
        transaction.Sign(_clientKeypair);

        Assert.IsTrue(WebAuthentication.VerifyChallengeTransaction(
            transaction,
            _serverKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            now: now));
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfSequenceIsNotZero()
    {
        var now = DateTimeOffset.Now;

        var nonce = new byte[64];
        var transaction = new TransactionBuilder(new Account(_serverKeypair.AccountId, 0))
            .AddOperation(new ManageDataOperation("NET auth", nonce))
            .Build();
        transaction.Sign(_clientKeypair);

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfServerAccountIdIsDifferent()
    {
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: now);
        transaction.Sign(_clientKeypair);

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfTransactionHasNoManageDataOperation()
    {
        var now = DateTimeOffset.Now;

        var transaction = new TransactionBuilder(new Account(_serverKeypair.AccountId, -1))
            .AddOperation(
                new AccountMergeOperation(_serverKeypair, _clientKeypair))
            .Build();
        transaction.Sign(_clientKeypair);

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfOperationHasNoSourceAccount()
    {
        var now = DateTimeOffset.Now;
        var nonce = new byte[64];
        var transaction = new TransactionBuilder(new Account(_serverKeypair.AccountId, -1))
            .AddOperation(new ManageDataOperation("NET auth", nonce))
            .Build();
        transaction.Sign(_clientKeypair);

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfOperationDataIsNotBase64Encoded()
    {
        var now = DateTimeOffset.Now;
        var nonce = new byte[64];
        var transaction = new TransactionBuilder(new Account(_serverKeypair.AccountId, -1))
            .AddOperation(new ManageDataOperation("NET auth", nonce, _clientKeypair))
            .Build();
        transaction.Sign(_clientKeypair);

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfNotSignedByServer()
    {
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: now);
        transaction.Signatures.Clear();
        transaction.Sign(_clientKeypair);

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfSignedByServerOnDifferentNetwork()
    {
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

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfNotSignedByClient()
    {
        var now = DateTimeOffset.Now;

        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: now);

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfSignedByClientOnDifferentNetwork()
    {
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

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfServerIsMuxedAccount()
    {
        // It's impossible to build a wrong transaction from our api. Use a xdr instead.
        const string transactionXdr =
            "AAAAAgAAAQAAAAAAAAAE0rqb5mZeN3cTjZYz9BOSuxs4tkP5296i8kJKWXS13pWGAAAAZAAAAAAAAAAAAAAAAQAAAABerG8LAAAAAF6scDcAAAAAAAAAAQAAAAEAAAAA13Pc/rMj75EaJFmzR1eWVHBeJuoq+8FinXpG7DXEsvoAAAAKAAAACE5FVCBhdXRoAAAAAQAAAEBIRmxJQi94UFFsYTBaSzNRamx3akFUL25JS3pUeFFFK1hFVE9EQkIzZHpOQWRsR0svOGJnbFBydSttaEJpNzdEAAAAAAAAAAK13pWGAAAAQGlkGeaHtcnaSyQP4NSU/CaRC6rUd7qXvVlJc/3TuWmY0kAC9/mXmLtnzFn2Hz+0cwVi1+wwtxfboxIHOABIsg81xLL6AAAAQB23cGeF7SR9bZEf6rRh+ck7h6PqvUQFDDDI3qE09y19SdvMWMs5Ksthm//dXMZE7+QJbKqxpJbpKC2klMTZJQ0=";

        var serverKeypair = KeyPair.FromAccountId("GC5JXZTGLY3XOE4NSYZ7IE4SXMNTRNSD7HN55IXSIJFFS5FV32KYM6PH");
        var now = DateTimeOffset.Now;
        var transaction = Transaction.FromEnvelopeXdr(transactionXdr);
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

    [TestMethod]
    public void TestVerifyChallengeTransactionThrowsIfClientIsMuxedAccount()
    {
        // It's impossible to build a wrong transaction from our api. Use a xdr instead.
        const string transactionXdr =
            "AAAAAgAAAABe3OrOugPm3BfyQ9xP+UUMEk8hiM0WwMSpVN9FIOxuXgAAAGQAAAAAAAAAAAAAAAEAAAAAXqxwZQAAAABerHGRAAAAAAAAAAEAAAABAAABAAAAAAAAAATSXtzqzroD5twX8kPcT/lFDBJPIYjNFsDEqVTfRSDsbl4AAAAKAAAACE5FVCBhdXRoAAAAAQAAAEAvU0VaNWppQjRZTXZTYlBNN1VobzJ6QmxqcVBiN0IyRDVJbGx6NEZxUWh4SmhHVmJWT0VsdHhyRlE5ZUNIL2RLAAAAAAAAAAIg7G5eAAAAQGKw8yxSA/tnK34nv6VIQ/r1bazvm3vInbU4dpSersY/7uN5MKZEKIMbioevHIpYZ6pwJdm7qRPbGj9YyCU+BQsNYg7iAAAAQKCdrKY6g6pEg/DfhOfOyRU8cKcg1qVSQwekXlKkQTzw/MpyLqYYRlxP5Z+P0TLDxmCn8KyawafIum24hvE11ws=";

        var serverKeypair = KeyPair.FromAccountId("GBPNZ2WOXIB6NXAX6JB5YT7ZIUGBETZBRDGRNQGEVFKN6RJA5RXF4SJ2");
        var now = DateTimeOffset.Now;
        var transaction = Transaction.FromEnvelopeXdr(transactionXdr);
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

    [TestMethod]
    public void TestReadChallengeTransactionValidSignedByServerAndClient()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        var readTransactionId = WebAuthentication.ReadChallengeTransaction(
            transaction,
            _serverKeypair.AccountId,
            HomeDomain,
            WebAuthDomain);
        Assert.IsNotNull(readTransactionId);
        Assert.AreEqual(_clientKeypair.AccountId, readTransactionId);
    }

    [TestMethod]
    public void TestReadChallengeTransactionValidSignedByServer()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);


        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

        var readTransactionId = WebAuthentication.ReadChallengeTransaction(
            transaction,
            _serverKeypair.AccountId,
            HomeDomain,
            WebAuthDomain);
        Assert.IsNotNull(readTransactionId);
        Assert.AreEqual(_clientKeypair.AccountId, readTransactionId);
    }

    [TestMethod]
    public void TestReadChallengeTransactionInvalidNotSignedByServer()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

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

    [TestMethod]
    public void TestReadChallengeTransactionInvalidServerAccountIdMismatch()
    {
        var transactionSource = new Account(KeyPair.Random().Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

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

    [TestMethod]
    public void TestReadChallengeTransactionInvalidSequenceNoNotZero()
    {
        var transactionSource = new Account(_serverKeypair.Address, 1234);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

        var ex = Assert.ThrowsException<InvalidWebAuthenticationException>(() =>
            WebAuthentication.ReadChallengeTransaction(
                transaction,
                _serverKeypair.AccountId,
                HomeDomain,
                WebAuthDomain));
        Assert.IsTrue(ex.Message.Contains("Challenge transaction sequence number must be 0"));
    }

    [TestMethod]
    public void TestReadChallengeTransactionInvalidOperationWrongType()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var operation = new BumpSequenceOperation(100, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

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

    [TestMethod]
    public void TestReadChallengeTransactionInvalidOperationNoSourceAccount()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data);
        var transaction = new TransactionBuilder(transactionSource)
            .SetFee(100)
            .AddOperation(operation)
            .AddMemo(Memo.None())
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);

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

    [TestMethod]
    public void TestReadChallengeTransactionInvalidDataValueWrongEncodedLength()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA?AAAAAAAAAAAAAAAAAAAAAAAAAA"u8.ToArray();

        var operation = new ManageDataOperation(ManageDataOperationName, plainTextBytes, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThresholdInvalidServer()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThresholdValidServerAndClientKeyMeetingThreshold()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

        var signersFound = WebAuthentication.VerifyChallengeTransactionThreshold(
            transaction,
            _serverKeypair.AccountId,
            threshold,
            signerSummary,
            HomeDomain,
            WebAuthDomain).ToList();

        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionThresholdValidServerAndMultipleClientKeyMeetingThreshold()
    {
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

        var signersFound = WebAuthentication.VerifyChallengeTransactionThreshold(
            transaction,
            _serverKeypair.AccountId,
            threshold,
            signerSummary,
            HomeDomain,
            WebAuthDomain).ToList();

        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionThresholdValidServerAndMultipleClientKeyMeetingThresholdSomeUnused()
    {
        var client2Keypair = KeyPair.Random();
        var client3Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

        var signersFound = WebAuthentication.VerifyChallengeTransactionThreshold(
            transaction,
            _serverKeypair.AccountId,
            threshold,
            signerSummary,
            HomeDomain,
            WebAuthDomain).ToList();

        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionThresholdInvalidServerAndMultipleClientKeyNotMeetingThreshold()
    {
        var client2Keypair = KeyPair.Random();
        var client3Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThresholdInvalidClientKeyUnrecognized()
    {
        var client2Keypair = KeyPair.Random();
        var client3Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThresholdInvalidNoSigners()
    {
        var client2Keypair = KeyPair.Random();
        var client3Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionThresholdWeightsAddToMoreThan8Bits()
    {
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

        var signersFound = WebAuthentication.VerifyChallengeTransactionThreshold(
            transaction,
            _serverKeypair.AccountId,
            threshold,
            signerSummary,
            HomeDomain,
            WebAuthDomain).ToList();

        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersInvalidServer()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersValidServerAndClientMasterKey()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain);

        Assert.AreEqual(_clientKeypair.Address, signersFound[0]);
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersInvalidServerAndNoClient()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersInvalidServerAndUnrecognizedClient()
    {
        var unrecognizedKeypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersValidServerAndMultipleClientSigners()
    {
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain).ToList();

        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersValidServerAndMultipleClientSignersReverseOrder()
    {
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain).ToList();

        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersValidServerAndClientSignersNotMasterKey()
    {
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain).ToList();

        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersValidServerAndClientSignersIgnoresServerSigner()
    {
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain).ToList();

        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersInvalidServerNoClientSignersIgnoresServerSigner()
    {
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersValidServerAndClientSignersIgnoresDuplicateSigner()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

        var signersFound = WebAuthentication.VerifyChallengeTransactionSigners(
            transaction,
            _serverKeypair.AccountId,
            signers,
            HomeDomain,
            WebAuthDomain).ToList();

        for (var i = 0; i < wantSigners.Length; i++)
        {
            Assert.AreEqual(signersFound[i], wantSigners[i]);
        }
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersInvalidServerAndClientSignersIgnoresDuplicateSignerInError()
    {
        var client2Keypair = KeyPair.Random();

        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersInvalidServerAndClientSignersFailsDuplicateSignatures()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionSignersInvalidNoSigners()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        var signers = Array.Empty<string>();

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

    [TestMethod]
    public void TestVerifyChallengeTransactionNotValidSubsequentOperation()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestVerifyChallengeTransactionNotValidSubsequentDataOperation()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

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

    [TestMethod]
    public void TestReadChallengeTransactionBadHomeDomain()
    {
        const string clientAccountId = "GBDIT5GUJ7R5BXO3GJHFXJ6AZ5UQK6MNOIDMPQUSMXLIHTUNR2Q5CFNF";

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

    [TestMethod]
    public void TestReadChallengeTransactionNoHomeDomain()
    {
        const string clientAccountId = "GBDIT5GUJ7R5BXO3GJHFXJ6AZ5UQK6MNOIDMPQUSMXLIHTUNR2Q5CFNF";

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

    [TestMethod]
    public void TestReadChallengeTransactionNoTransaction()
    {
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

    [TestMethod]
    public void TestReadChallengeTransactionExpiredTimeBounds()
    {
        const string clientAccountId = "GBDIT5GUJ7R5BXO3GJHFXJ6AZ5UQK6MNOIDMPQUSMXLIHTUNR2Q5CFNF";

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

    [TestMethod]
    public void TestReadChallengeTransactionNoWebAuthDomain()
    {
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

        var operation = new ManageDataOperation(ManageDataOperationName, base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        transaction.Sign(_clientKeypair);

        var readTransactionId = WebAuthentication.ReadChallengeTransaction(
            transaction,
            _serverKeypair.AccountId,
            HomeDomain, "");

        Assert.AreEqual(_clientKeypair.AccountId, readTransactionId);
    }

    [TestMethod]
    public void TestVerifyChallengeTransactionWithClientDomain()
    {
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

    [TestMethod]
    public void TestReadChallengeTransactionWithOutOfLowerBound()
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

    [TestMethod]
    public void TestReadChallengeTransactionWithOutOfLowerBoundButWithinGracePeriod()
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

    [TestMethod]
    public void TestReadChallengeTransactionWithOutOfUpperBound()
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

    [TestMethod]
    public void TestReadChallengeTransactionWithOutOfUpperBoundButWithinGracePeriod()
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