using System;
using System.Collections.Generic;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Sep.Sep0010;
using StellarDotnetSdk.Sep.Sep0010.Exceptions;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace StellarDotnetSdk;

/// <summary>
///     Implement SEP-10: Stellar Web Authentication.
///     https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0010.md
/// </summary>
[Obsolete("Use ServerWebAuth instead. This class will be removed in a future version.")]
public static class WebAuthentication
{
    /// <summary>
    ///     Give a small grace period for the transaction time to account for clock drift.
    /// </summary>
    public const int GracePeriod = ServerWebAuth.GracePeriod;

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
        return ServerWebAuth.BuildChallengeTransaction(
            serverKeypair,
            clientAccountId,
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
        return ServerWebAuth.BuildChallengeTransaction(
            serverKeypair,
            clientAccountId,
            homeDomain,
            webAuthDomain,
            nonce,
            validFrom,
            validFor,
            network,
            clientDomain,
            clientSigningKey);
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
        return ServerWebAuth.ReadChallengeTransaction(
            transaction,
            serverAccountId,
            homeDomain,
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
        return ServerWebAuth.ReadChallengeTransaction(
            transaction,
            serverAccountId,
            homeDomains,
            webAuthDomain,
            network,
            now);
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
        return ServerWebAuth.VerifyChallengeTransactionThreshold(
            transaction,
            serverAccountId,
            threshold,
            signerSummary,
            homeDomain,
            webAuthDomain,
            network,
            now);
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
        return ServerWebAuth.VerifyChallengeTransactionSigners(
            transaction,
            serverAccountId,
            signers,
            homeDomain,
            webAuthDomain,
            network,
            now);
    }
}