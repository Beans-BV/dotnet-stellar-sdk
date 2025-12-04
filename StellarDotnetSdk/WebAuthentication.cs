using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using FormatException = System.FormatException;
using TimeBounds = StellarDotnetSdk.Transactions.TimeBounds;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace StellarDotnetSdk;

/// <summary>
///     Implement SEP-10: Stellar Web Authentication.
///     https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0010.md
/// </summary>
public static class WebAuthentication
{
    private const string WebAuthDataKey = "web_auth_domain";
    private const string ClientDomainDataKey = "client_domain";
    private const string AuthSuffix = " auth";
    private const int ChallengeNonceLength = 48;

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
    /// <param name="clientDomain">The network the transaction will be submitted to</param>
    /// <param name="clientKeypair">The network the transaction will be submitted to</param>
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
        if (string.IsNullOrEmpty(clientAccountId))
        {
            throw new ArgumentNullException(nameof(clientAccountId));
        }
        if (StrKey.DecodeVersionByte(clientAccountId) != StrKey.VersionByte.ACCOUNT_ID)
        {
            throw new InvalidWebAuthenticationException($"{nameof(clientAccountId)} is not a valid account id");
        }

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
            clientKeypair
        );
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
    /// <param name="clientSigningKey">Client Signing Key (Used with Client Domain)</param>
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
        ArgumentException.ThrowIfNullOrEmpty(homeDomain);
        ArgumentException.ThrowIfNullOrEmpty(webAuthDomain);
        if (!string.IsNullOrEmpty(clientDomain))
        {
            ArgumentNullException.ThrowIfNull(clientSigningKey);
        }

        if (nonce is null)
        {
            using var rng = RandomNumberGenerator.Create();
            nonce = new byte[ChallengeNonceLength];
            rng.GetBytes(nonce);
        }
        else if (nonce.Length != ChallengeNonceLength)
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

        var webAuthDataValue = Encoding.UTF8.GetBytes(webAuthDomain);

        var webAuthOperation = new ManageDataOperation(WebAuthDataKey, webAuthDataValue, serverKeypair);

        var transactionBuilder = new TransactionBuilder(serverAccount)
            .AddTimeBounds(timeBounds)
            .AddOperation(operation)
            .AddOperation(webAuthOperation);

        if (!string.IsNullOrEmpty(clientDomain))
        {
            var clientDomainDataValue = Encoding.UTF8.GetBytes(clientDomain);
            var clientDomainOperation =
                new ManageDataOperation(ClientDomainDataKey, clientDomainDataValue, clientSigningKey);

            transactionBuilder.AddOperation(clientDomainOperation);
        }

        var transaction = transactionBuilder.Build();

        transaction.Sign(serverKeypair, network);

        return transaction;
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
    /// <param name="homeDomains">The server home domains</param>
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
        now ??= DateTimeOffset.UtcNow;

        ValidateTransaction(transaction, serverAccountId);

        var operation = (ManageDataOperation)transaction.Operations[0];
        ValidateHomeDomains(homeDomains, operation);

        ValidateSubsequentOperations(transaction, serverAccountId, webAuthDomain);

        if (!ValidateSignedBy(transaction, serverAccountId, network!))
        {
            throw new InvalidWebAuthenticationException("Challenge transaction not signed by server");
        }
        if (!ValidateTimeBounds(transaction.TimeBounds, now.Value))
        {
            throw new InvalidWebAuthenticationException("Challenge transaction expired");
        }

        return operation.SourceAccount!.Address;
    }

    public static ICollection<string> VerifyChallengeTransactionThreshold(
        Transaction transaction,
        string serverAccountId,
        int threshold,
        IDictionary<string, int> signerSummary,
        string homeDomain,
        string webAuthDomain,
        Network? network = null,
        DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(signerSummary);
        var signersFound = VerifyChallengeTransactionSigners(
            transaction,
            serverAccountId,
            signerSummary.Keys.ToArray(),
            homeDomain,
            webAuthDomain,
            network,
            now);
        var weight = signersFound.Sum(signer => signerSummary[signer]);
        if (weight < threshold)
        {
            throw new InvalidWebAuthenticationException(
                $"Signers with weight {weight} do not meet threshold {threshold}");
        }

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
        ArgumentNullException.ThrowIfNull(signers);
        ArgumentNullException.ThrowIfNull(transaction);
        if (signers.Count == 0)
        {
            throw new ArgumentException("Signers must be non-empty", nameof(signers));
        }

        network ??= Network.Current;

        ReadChallengeTransaction(transaction, serverAccountId, homeDomain, webAuthDomain, network, now);

        var serverKeypair = KeyPair.FromAccountId(serverAccountId);

        var clientSigningKey = GetClientSigningKey(transaction);

        var allVerifiedSigners =
            VerifyTransactionSignatures(transaction, signers, serverKeypair, clientSigningKey, network);

        ValidateSignatures(allVerifiedSigners, serverKeypair, clientSigningKey, transaction.Signatures.Count);

        return allVerifiedSigners
            .Where(signer => !string.Equals(signer, serverKeypair.Address, StringComparison.Ordinal))
            .ToArray();
    }

    private static void ValidateTransaction(Transaction transaction, string serverAccountId)
    {
        if (transaction is null)
        {
            throw new InvalidWebAuthenticationException("Challenge transaction cannot be null");
        }

        if (transaction.SequenceNumber != 0)
        {
            throw new InvalidWebAuthenticationException("Challenge transaction sequence number must be 0");
        }

        if (transaction.SourceAccount.IsMuxedAccount)
        {
            throw new InvalidWebAuthenticationException("Challenge transaction source cannot be a muxed account");
        }

        if (!string.Equals(transaction.SourceAccount.AccountId, serverAccountId, StringComparison.Ordinal))
        {
            throw new InvalidWebAuthenticationException("Challenge transaction source must be serverAccountId");
        }

        if (transaction.Operations.Length < 1)
        {
            throw new InvalidWebAuthenticationException("Challenge transaction must contain at least one operation");
        }

        if (transaction.Operations[0] is not ManageDataOperation operation)
        {
            throw new InvalidWebAuthenticationException(
                "Challenge transaction operation must be of type ManageDataOperation");
        }

        if (operation.SourceAccount is null)
        {
            throw new InvalidWebAuthenticationException("Challenge transaction operation must have source account");
        }

        if (operation.SourceAccount.IsMuxedAccount)
        {
            throw new InvalidWebAuthenticationException(
                "Challenge transaction operation source account cannot be a muxed account");
        }
        if (operation.Value == null)
        {
            throw new InvalidWebAuthenticationException("Challenge transaction operation data must be present");
        }

        var stringValue = Encoding.UTF8.GetString(operation.Value);
        if (stringValue.Length != 64)
        {
            throw new InvalidWebAuthenticationException(
                "Challenge transaction operation data must be 64 bytes long");
        }

        try
        {
            // Validate base64 format - no need to store the result
            _ = Convert.FromBase64String(stringValue);
        }
        catch (FormatException)
        {
            throw new InvalidWebAuthenticationException(
                "Challenge transaction operation data must be base64 encoded");
        }
    }

    private static void ValidateHomeDomains(string[] homeDomains, ManageDataOperation operation)
    {
        if (homeDomains == null || homeDomains.Length == 0)
        {
            throw new InvalidWebAuthenticationException(
                "Invalid homeDomains: a home domain must be provided for verification");
        }
        var matchedHomeDomain = homeDomains.FirstOrDefault(domain =>
            string.Equals(operation.Name, domain + AuthSuffix, StringComparison.Ordinal));

        if (string.IsNullOrEmpty(matchedHomeDomain))
        {
            throw new InvalidWebAuthenticationException(
                "Invalid homeDomains: the transaction's operation key name does not match the expected home domain");
        }
    }

    private static void ValidateSubsequentOperations(Transaction transaction, string serverAccountId,
        string webAuthDomain)
    {
        foreach (var op in transaction.Operations.Skip(1))
        {
            if (op is not ManageDataOperation opManageData)
            {
                throw new InvalidWebAuthenticationException(
                    "The transaction has operations that are not of type 'manageData'");
            }

            var isServerSource = string.Equals(opManageData.SourceAccount?.AccountId, serverAccountId,
                StringComparison.Ordinal);
            var isClientDomainOperation =
                string.Equals(opManageData.Name, ClientDomainDataKey, StringComparison.Ordinal);

            if (!isServerSource && !isClientDomainOperation)
            {
                throw new InvalidWebAuthenticationException("The transaction has operations that are unrecognized");
            }

            ValidateWebAuthOperation(opManageData, webAuthDomain);
        }
    }

    private static void ValidateWebAuthOperation(ManageDataOperation operation, string webAuthDomain)
    {
        if (!string.Equals(operation.Name, WebAuthDataKey, StringComparison.Ordinal))
        {
            return;
        }

        var opDataValue = operation.Value != null ? Encoding.UTF8.GetString(operation.Value) : null;

        if (operation.Value == null || !string.Equals(opDataValue, webAuthDomain, StringComparison.Ordinal))
        {
            throw new InvalidWebAuthenticationException(
                $"Invalid '{WebAuthDataKey}' value. Expected: {webAuthDomain} Actual: {opDataValue}");
        }
    }

    private static KeyPair? GetClientSigningKey(Transaction transaction)
    {
        var sourceAccountId = transaction.Operations
            .OfType<ManageDataOperation>()
            .FirstOrDefault(op => op.Name == ClientDomainDataKey)
            ?.SourceAccount?.AccountId;

        return sourceAccountId != null ? KeyPair.FromAccountId(sourceAccountId) : null;
    }

    /// <summary>
    ///     Checks if a transaction has been signed by one or more of the signers.
    /// </summary>
    /// <param name="transaction">Transaction to be checked</param>
    /// <param name="signers">A list of signers</param>
    /// <param name="serverKeypair">(Optional) Server key pair</param>
    /// <param name="clientSigningKey">(Optional) Client signing key pair</param>
    /// <param name="network">Network</param>
    /// <returns>A list of signers that were found to have signed the transaction</returns>
    private static HashSet<string> VerifyTransactionSignatures(
        Transaction transaction,
        ICollection<string> signers,
        KeyPair? serverKeypair,
        KeyPair? clientSigningKey,
        Network network)
    {
        var allSigners = new HashSet<string>(signers, StringComparer.Ordinal);

        if (serverKeypair != null)
        {
            allSigners.Add(serverKeypair.Address);
        }
        if (clientSigningKey != null)
        {
            allSigners.Add(clientSigningKey.Address);
        }

        var transactionHash = transaction.Hash(network);
        var usedSignatures = new HashSet<DecoratedSignature>();
        var verifiedSigners = new HashSet<string>(StringComparer.Ordinal);

        // Group signatures by base64 hint upfront for faster lookups
        var signaturesByHint = transaction.Signatures
            .ToLookup(s => Convert.ToBase64String(s.Hint.InnerValue), StringComparer.Ordinal);

        foreach (var signer in allSigners)
        {
            var keypair = KeyPair.FromAccountId(signer);
            var signerHintKey = Convert.ToBase64String(keypair.SignatureHint.InnerValue);

            // Only check signatures with matching hints
            foreach (var signature in signaturesByHint[signerHintKey])
            {
                if (usedSignatures.Contains(signature))
                {
                    continue;
                }
                if (!keypair.Verify(transactionHash, signature.Signature))
                {
                    continue;
                }
                usedSignatures.Add(signature);
                verifiedSigners.Add(keypair.Address);
                break;
            }
        }
        return verifiedSigners;
    }

    private static void ValidateSignatures(
        ICollection<string> allVerifiedSigners,
        KeyPair serverKeypair,
        KeyPair? clientSigningKey,
        int expectedSignatureCount)
    {
        var hasServerSignature = allVerifiedSigners.Contains(serverKeypair.Address, StringComparer.Ordinal);

        if (!hasServerSignature)
        {
            throw new InvalidWebAuthenticationException("Challenge transaction not signed by server");
        }

        if (clientSigningKey != null)
        {
            var hasClientSignature = allVerifiedSigners.Contains(clientSigningKey.Address, StringComparer.Ordinal);
            if (!hasClientSignature)
            {
                throw new InvalidWebAuthenticationException(
                    $"Challenge Transaction not signed by the source account of the '{ClientDomainDataKey}'");
            }
        }

        if (allVerifiedSigners.Count == 1)
        {
            throw new InvalidWebAuthenticationException("Challenge transaction not signed by client");
        }

        if (allVerifiedSigners.Count != expectedSignatureCount)
        {
            throw new InvalidWebAuthenticationException("Challenge transaction has unrecognized signatures");
        }
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

        var clientAccountId = ReadChallengeTransaction(
            transaction,
            serverAccountId,
            homeDomain,
            webAuthDomain,
            network,
            now);

        var valid = ValidateSignedBy(
            transaction,
            clientAccountId,
            network);

        if (!valid)
        {
            throw new InvalidWebAuthenticationException("Challenge transaction not signed by client");
        }

        return true;
    }

    private static bool ValidateSignedBy(
        Transaction transaction,
        string accountId,
        Network network)
    {
        var signaturesUsed = VerifyTransactionSignatures(
            transaction,
            [accountId],
            null,
            null,
            network
        );
        return signaturesUsed.Count == 1;
    }

    private static bool ValidateTimeBounds(TimeBounds? timeBounds, DateTimeOffset now)
    {
        if (timeBounds is null || timeBounds.MinTime == 0 || timeBounds.MaxTime == 0)
        {
            return false;
        }

        var unixNow = now.ToUnixTimeSeconds();
        // Apply a grace period to time bounds check
        var graceStart = timeBounds.MinTime - GracePeriod;
        var graceEnd = timeBounds.MaxTime + GracePeriod;

        return graceStart <= unixNow && unixNow <= graceEnd;
    }
}