﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSec.Cryptography;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using FormatException = System.FormatException;
using TimeBounds = StellarDotnetSdk.Transactions.TimeBounds;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace StellarDotnetSdk;

using TimeBounds = TimeBounds;
using Transaction = Transaction;

/// <summary>
///     Implement SEP-10: Stellar Web Authentication.
///     https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0010.md
/// </summary>
public static class WebAuthentication
{
    /// <summary>
    ///     Give a small grace period for the transaction time to account for clock drift.
    /// </summary>
    public const int GracePeriod = 60 * 5;

    /// <summary>
    ///     Build a challenge transaction you can use for Stellar Web Authentication.
    /// </summary>
    /// <param name="serverKeypair">Server signing keypair</param>
    /// <param name="clientAccountId">The client account id that needs authentication</param>
    /// <param name="homeDomain">The server home domain</param>
    /// <param name="webAuthDomain">The server auth domain</param>
    /// <param name="nonce">48 bytes long cryptographic-quality random data</param>
    /// <param name="validFrom">The datetime from which the transaction is valid</param>
    /// <param name="validFor">The transaction lifespan</param>
    /// <param name="network">The network the transaction will be submitted to</param>
    /// <returns>The challenge transaction</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static Transaction BuildChallengeTransaction(
        KeyPair serverKeypair,
        string clientAccountId,
        string homeDomain,
        string webAuthDomain,
        byte[]? nonce = null,
        DateTimeOffset? validFrom = null,
        TimeSpan? validFor = null,
        Network? network = null,
        string? clientDomain = null,
        KeyPair? clientKeypair = null)
    {
        if (string.IsNullOrEmpty(clientAccountId)) throw new ArgumentNullException(nameof(clientAccountId));
        if (StrKey.DecodeVersionByte(clientAccountId) != StrKey.VersionByte.ACCOUNT_ID)
            throw new InvalidWebAuthenticationException($"{nameof(clientAccountId)} is not a valid account id");
        var clientAccountKeypair = KeyPair.FromAccountId(clientAccountId);
        return BuildChallengeTransaction(
            serverKeypair,
            clientAccountKeypair,
            homeDomain,
            webAuthDomain,
            nonce,
            validFrom,
            validFor,
            network,
            clientDomain,
            clientKeypair);
    }

    /// <summary>
    ///     Build a challenge transaction you can use for Stellar Web Authentication.
    /// </summary>
    /// <param name="serverKeypair">Server signing keypair</param>
    /// <param name="clientAccountId">The client account id that needs authentication</param>
    /// <param name="homeDomain">The server home domain</param>
    /// <param name="webAuthDomain">The server auth domain</param>
    /// <param name="nonce">48 bytes long cryptographic-quality random data</param>
    /// <param name="validFrom">The datetime from which the transaction is valid</param>
    /// <param name="validFor">The transaction lifespan</param>
    /// <param name="network">The network the transaction will be submitted to</param>
    /// <param name="clientDomain">Optional Client Domain</param>
    /// <param name="clientDomain">Client Signing Key (Used with Client Domain)</param>
    /// <returns>The challenge transaction</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static Transaction BuildChallengeTransaction(
        KeyPair serverKeypair,
        KeyPair clientAccountId,
        string homeDomain,
        string webAuthDomain,
        byte[]? nonce = null,
        DateTimeOffset? validFrom = null,
        TimeSpan? validFor = null,
        Network? network = null,
        string? clientDomain = null,
        KeyPair? clientSigningKey = null)
    {
        ArgumentNullException.ThrowIfNull(serverKeypair);
        ArgumentNullException.ThrowIfNull(clientAccountId);
        if (string.IsNullOrEmpty(homeDomain)) throw new ArgumentNullException(nameof(homeDomain));
        if (string.IsNullOrEmpty(webAuthDomain)) throw new ArgumentNullException(nameof(webAuthDomain));
        if (!string.IsNullOrEmpty(clientDomain) && clientSigningKey is null)
            throw new ArgumentNullException(nameof(clientSigningKey));

        if (nonce is null)
        {
            var rng = RandomGenerator.Default;
            // A base64 digit represents 6 bites, to generate 64 bytes long base64 encoded strings
            // we need 64 * 6 / 8 bytes.
            nonce = rng.GenerateBytes(48);
        }
        else if (nonce.Length != 48)
        {
            throw new ArgumentException("nonce must be 48 bytes long");
        }

        network ??= Network.Current;
        validFrom ??= DateTimeOffset.Now;
        validFor ??= TimeSpan.FromMinutes(5.0);

        // Sequence number is incremented by 1 before building the transaction, set it to -1 to have 0
        var serverAccount = new Account(serverKeypair, -1);

        var manageDataKey = $"{homeDomain} auth";
        var manageDataValue = Encoding.UTF8.GetBytes(Convert.ToBase64String(nonce));

        var timeBounds = new TimeBounds(validFrom.Value, validFor.Value);

        var operation = new ManageDataOperation(manageDataKey, manageDataValue, clientAccountId);

        var webAuthDataKey = "web_auth_domain";
        var webAuthDataValue = Encoding.UTF8.GetBytes(webAuthDomain);

        var webAuthOperation = new ManageDataOperation(webAuthDataKey, webAuthDataValue, serverKeypair);

        var txBuilder = new TransactionBuilder(serverAccount)
            .AddTimeBounds(timeBounds)
            .AddOperation(operation)
            .AddOperation(webAuthOperation);


        if (!string.IsNullOrEmpty(clientDomain))
        {
            var clientDomainOperation =
                new ManageDataOperation("client_domain", Encoding.UTF8.GetBytes(clientDomain), clientSigningKey);

            txBuilder.AddOperation(clientDomainOperation);
        }

        var tx = txBuilder.Build();

        tx.Sign(serverKeypair, network);

        return tx;
    }

    /// <summary>
    ///     Read a SEP 10 challenge transaction and return the client account id.
    ///     Performs the following checks:
    ///     1. Transaction sequence number is 0
    ///     2. Transaction source account is <paramref name="serverAccountId" />
    ///     3. Transaction has one operation only, of type ManageDataOperation
    ///     4. The ManageDataOperation name and value are correct
    ///     5. Transaction time bounds are still valid
    ///     6. Transaction is signed by server
    /// </summary>
    /// <param name="transaction">The challenge transaction</param>
    /// <param name="serverAccountId">The server account id</param>
    /// <param name="homeDomain">The server home domain</param>
    /// <param name="webAuthDomain">The server auth domain</param>
    /// <param name="network">The network the transaction was submitted to, defaults to Network.Current</param>
    /// <param name="now">Current time, defaults to DateTimeOffset.Now</param>
    /// <returns>The client account id</returns>
    /// <exception cref="InvalidWebAuthenticationException"></exception>
    public static string ReadChallengeTransaction(
        Transaction transaction,
        string serverAccountId,
        string homeDomain,
        string webAuthDomain,
        Network? network = null,
        DateTimeOffset? now = null)
    {
        return ReadChallengeTransaction(
            transaction,
            serverAccountId,
            [homeDomain],
            webAuthDomain,
            network,
            now);
    }

    /// <summary>
    ///     Read a SEP 10 challenge transaction and return the client account id.
    ///     Performs the following checks:
    ///     1. Transaction sequence number is 0
    ///     2. Transaction source account is <paramref name="serverAccountId" />
    ///     3. Transaction has one operation only, of type ManageDataOperation
    ///     4. The ManageDataOperation name and value are correct
    ///     5. Transaction time bounds are still valid
    ///     6. Transaction is signed by server
    /// </summary>
    /// <param name="transaction">The challenge transaction</param>
    /// <param name="serverAccountId">The server account id</param>
    /// <param name="homeDomain">The server home domain</param>
    /// <param name="webAuthDomain">The server auth domain</param>
    /// <param name="network">The network the transaction was submitted to, defaults to Network.Current</param>
    /// <param name="now">Current time, defaults to DateTimeOffset.Now + GracePeriod</param>
    /// <returns>The client account id</returns>
    /// <exception cref="InvalidWebAuthenticationException"></exception>
    public static string ReadChallengeTransaction(
        Transaction transaction,
        string serverAccountId,
        string[] homeDomains,
        string webAuthDomain,
        Network? network = null,
        DateTimeOffset? now = null)
    {
        network ??= Network.Current;

        if (transaction is null)
            throw new InvalidWebAuthenticationException("Challenge transaction cannot be null");

        if (transaction.SequenceNumber != 0)
            throw new InvalidWebAuthenticationException("Challenge transaction sequence number must be 0");

        if (transaction.SourceAccount.IsMuxedAccount)
            throw new InvalidWebAuthenticationException("Challenge transaction source cannot be a muxed account");

        if (transaction.SourceAccount.AccountId != serverAccountId)
            throw new InvalidWebAuthenticationException("Challenge transaction source must be serverAccountId");

        if (transaction.Operations.Length < 1)
            throw new InvalidWebAuthenticationException("Challenge transaction must contain at least one operation");

        if (transaction.Operations[0] is not ManageDataOperation operation)
            throw new InvalidWebAuthenticationException(
                "Challenge transaction operation must be of type ManageDataOperation");

        if (operation.SourceAccount is null)
            throw new InvalidWebAuthenticationException("Challenge transaction operation must have source account");

        if (homeDomains == null || homeDomains.Length == 0)
            throw new InvalidWebAuthenticationException(
                "Invalid homeDomains: a home domain must be provided for verification");

        var matchedHomeDomain = "";

        foreach (var domain in homeDomains)
            if (operation.Name == $"{domain} auth")
            {
                matchedHomeDomain = domain;
                break;
            }

        if (string.IsNullOrEmpty(matchedHomeDomain))
            throw new InvalidWebAuthenticationException(
                "Invalid homeDomains: the transaction's operation key name does not match the expected home domain");

        var subsequentOperations = transaction.Operations;
        foreach (var op in subsequentOperations.Skip(1))
        {
            if (op is not ManageDataOperation opManageData)
                throw new InvalidWebAuthenticationException(
                    "The transaction has operations that are not of type 'manageData'");

            if (opManageData.SourceAccount?.AccountId != serverAccountId && opManageData.Name != "client_domain")
                throw new InvalidWebAuthenticationException("The transaction has operations that are unrecognized");

            var opDataValue = opManageData.Value != null ? Encoding.UTF8.GetString(opManageData.Value) : null;

            if (opManageData.Name == "web_auth_domain" &&
                (opManageData.Value == null || opDataValue != webAuthDomain))
                throw new InvalidWebAuthenticationException(
                    $"Invalid 'web_auth_domain' value. Expected: {webAuthDomain} Actual: {opDataValue}");
        }

        var clientAccountKeypair = operation.SourceAccount;

        if (clientAccountKeypair.IsMuxedAccount)
            throw new InvalidWebAuthenticationException(
                "Challenge transaction operation source account cannot be a muxed account");

        var clientAccountId = clientAccountKeypair.Address;

        if (operation.Value == null)
            throw new InvalidWebAuthenticationException("Challenge transaction operation data must be present");

        var stringValue = Encoding.UTF8.GetString(operation.Value);
        if (stringValue.Length != 64)
            throw new InvalidWebAuthenticationException(
                "Challenge transaction operation data must be 64 bytes long");

        try
        {
            // There is no need to check for decoded value length since we know it's valid base64 and 64 bytes long.
            _ = Convert.FromBase64String(stringValue);
        }
        catch (FormatException)
        {
            throw new InvalidWebAuthenticationException(
                "Challenge transaction operation data must be base64 encoded");
        }

        if (!ValidateSignedBy(transaction, serverAccountId, network))
            throw new InvalidWebAuthenticationException("Challenge transaction not signed by server");

        if (!ValidateTimeBounds(transaction.TimeBounds, now ?? DateTimeOffset.Now))
            throw new InvalidWebAuthenticationException("Challenge transaction expired");

        return clientAccountId;
    }

    public static ICollection<string> VerifyChallengeTransactionThreshold(
        Transaction transaction,
        string serverAccountId,
        int threshold,
        Dictionary<string, int> signerSummary,
        string homeDomain,
        string webAuthDomain,
        Network? network = null,
        DateTimeOffset? now = null)
    {
        var signersFound =
            VerifyChallengeTransactionSigners(transaction, serverAccountId, signerSummary.Keys.ToArray(),
                homeDomain, webAuthDomain, network,
                now);
        var weight = signersFound.Sum(signer => signerSummary[signer]);
        if (weight < threshold)
            throw new InvalidWebAuthenticationException(
                $"Signers with weight {weight} do not meet threshold {threshold}");
        return signersFound;
    }

    /// <summary>
    ///     Verify that all signers of a SEP 10 transaction are accounted for.
    ///     A transaction is verified if it signed by the server account, and all other signatures match the provided
    ///     signers. Additional signers can be provided that do not have a signature, but all signatures must be
    ///     matched to a signer for verification to succeed. If verification succeeds, the list of signers that were
    ///     found is returned, excluding the server account id.
    /// </summary>
    /// <param name="transaction">The challenge transaction</param>
    /// <param name="serverAccountId">The server account id</param>
    /// <param name="signers"></param>
    /// <param name="homeDomain">The server home domain</param>
    /// <param name="webAuthDomain">The server auth domain</param>
    /// <param name="network">The network the transaction was submitted to, defaults to Network.Current</param>
    /// <param name="now">Current time, defaults to DateTimeOffset.Now</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string[] VerifyChallengeTransactionSigners(
        Transaction transaction,
        string serverAccountId,
        ICollection<string> signers,
        string homeDomain,
        string webAuthDomain,
        Network? network = null,
        DateTimeOffset? now = null)
    {
        if (signers.Count == 0)
            throw new ArgumentException($"{nameof(signers)} must be non-empty");

        network ??= Network.Current;

        ReadChallengeTransaction(transaction, serverAccountId, homeDomain, webAuthDomain, network, now);

        // If the client domain is included in the challenge transaction,
        // verify that the transaction is signed by the operation's source account.
        KeyPair? clientSigningKey = null;
        var sourceAccountId = transaction.Operations
            .FirstOrDefault(x => x is ManageDataOperation { Name: "client_domain" })?.SourceAccount?.AccountId;
        if (sourceAccountId != null) clientSigningKey = KeyPair.FromAccountId(sourceAccountId);

        // Remove server signer if present
        var serverKeypair = KeyPair.FromAccountId(serverAccountId);
        var clientSigners = signers.Where(signer => signer != serverKeypair.Address).ToList();

        var additionalSigners = new List<string> { serverKeypair.Address };
        if (clientSigningKey != null) additionalSigners.Add(clientSigningKey.Address);

        var allSigners = clientSigners.Select(signer => (string)signer.Clone()).ToList();
        allSigners.AddRange(additionalSigners);

        var allSignersFound = VerifyTransactionSignatures(transaction, allSigners, network);

        var serverSigner = allSignersFound.FirstOrDefault(signer => signer == serverKeypair.Address);
        var clientSigningKeyFound = false;

        if (clientSigningKey != null)
            clientSigningKeyFound =
                !string.IsNullOrEmpty(allSignersFound.FirstOrDefault(signer => signer == clientSigningKey.Address));

        if (serverSigner is null)
            throw new InvalidWebAuthenticationException("Challenge transaction not signed by server");

        if (clientSigningKey != null && !clientSigningKeyFound)
            throw new InvalidWebAuthenticationException(
                "Challenge Transaction not signed by the source account of the 'client_domain' ");

        if (allSignersFound.Count == 1)
            throw new InvalidWebAuthenticationException("Challenge transaction not signed by client");

        if (allSignersFound.Count != transaction.Signatures.Count)
            throw new InvalidWebAuthenticationException("Challenge transaction has unrecognized signatures");

        return allSignersFound.Where(signer => signer != serverSigner).ToArray();
    }

    /// <summary>
    ///     Verify that a transaction is a valid Stellar Web Authentication transaction.
    ///     Performs the following checks:
    ///     1. Transaction sequence number is 0
    ///     2. Transaction source account is <paramref name="serverAccountId" />
    ///     3. Transaction has one operation only, of type ManageDataOperation
    ///     4. The ManageDataOperation name and value are correct
    ///     5. Transaction time bounds are still valid
    ///     6. Transaction is signed by server and client
    /// </summary>
    /// <param name="transaction">The challenge transaction</param>
    /// <param name="serverAccountId">The server account id</param>
    /// <param name="homeDomain">The server home domain</param>
    /// <param name="webAuthDomain">The server auth domain</param>
    /// <param name="network">The network the transaction was submitted to, defaults to Network.Current</param>
    /// <param name="now">Current time, defaults to DateTimeOffset.Now</param>
    /// <returns>True if the transaction is valid</returns>
    /// <exception cref="InvalidWebAuthenticationException"></exception>
    [Obsolete("Use VerifyChallengeTransactionThreshold and VerifyChallengeTransactionSigners")]
    public static bool VerifyChallengeTransaction(
        Transaction transaction,
        string serverAccountId,
        string homeDomain,
        string webAuthDomain,
        Network? network = null,
        DateTimeOffset? now = null)
    {
        network ??= Network.Current;

        var clientAccountId =
            ReadChallengeTransaction(transaction, serverAccountId, homeDomain, webAuthDomain, network, now);

        if (!ValidateSignedBy(transaction, clientAccountId, network))
            throw new InvalidWebAuthenticationException("Challenge transaction not signed by client");

        return true;
    }

    private static bool ValidateSignedBy(Transaction transaction, string accountId, Network network)
    {
        var signaturesUsed = VerifyTransactionSignatures(transaction, new[] { accountId }, network);
        return signaturesUsed.Count == 1;
    }

    private static bool ValidateTimeBounds(
        TimeBounds? timeBounds,
        DateTimeOffset now)
    {
        if (timeBounds is null || timeBounds.MinTime == 0 || timeBounds.MaxTime == 0) return false;
        var unixNow = now.ToUnixTimeSeconds();
        // Apply grace period to time bounds check
        var graceStart = timeBounds.MinTime - GracePeriod;
        var graceEnd = timeBounds.MaxTime + GracePeriod;

        return graceStart <= unixNow && unixNow <= graceEnd;
    }

    private static ICollection<string> VerifyTransactionSignatures(
        Transaction transaction,
        IEnumerable<string> signers,
        Network network)
    {
        var txHash = transaction.Hash(network);
        var signaturesUsed = new Dictionary<DecoratedSignature, string>();
        var signersFound = new HashSet<string>();

        foreach (var signer in signers)
        {
            var keypair = KeyPair.FromAccountId(signer);
            foreach (var signature in transaction.Signatures)
            {
                if (signaturesUsed.ContainsKey(signature))
                    continue;

                if (!signature.Hint.InnerValue.SequenceEqual(keypair.SignatureHint.InnerValue))
                    continue;

                if (!keypair.Verify(txHash, signature.Signature)) continue;
                signaturesUsed[signature] = keypair.Address;
                signersFound.Add(keypair.Address);
                break;
            }
        }

        return signersFound.ToArray();
    }
}