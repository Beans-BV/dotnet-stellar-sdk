using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.IntegrationTests.Infrastructure;

/// <summary>
///     Base class for Testnet integration tests. Owns a single Server instance per
///     test class lifecycle, configures the Testnet network, and exposes helpers
///     for funding fresh keypairs via Friendbot with bounded retries.
/// </summary>
public abstract class IntegrationTestBase
{
    private static readonly TimeSpan[] BackoffDelays =
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(9),
    };

    // Upper bound on a single honored Retry-After wait, so one server-advised delay
    // cannot blow a test's [CancelAfter] budget.
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(15);

    protected Server Server = null!;

    [OneTimeSetUp]
    public void BaseOneTimeSetUp()
    {
        Network.UseTestNetwork();
        Server = new Server(TestnetConfig.HorizonUrl);
    }

    [OneTimeTearDown]
    public void BaseOneTimeTearDown()
    {
        Server.Dispose();
    }

    /// <summary>
    ///     Generates a fresh random keypair and funds it via the Testnet Friendbot.
    ///     Retries on transient errors (up to 4 attempts: an initial try plus 3 retries with 1s/3s/9s backoff).
    /// </summary>
    protected async Task<KeyPair> CreateFundedAccountAsync()
    {
        var keyPair = KeyPair.Random();
        await FundWithRetryAsync(keyPair.AccountId);
        return keyPair;
    }

    /// <summary>
    ///     Loads the on-chain account state for the given keypair (sequence number, balances, etc).
    /// </summary>
    protected Task<AccountResponse> LoadAccountAsync(KeyPair keyPair)
    {
        return Server.Accounts.Account(keyPair.AccountId);
    }

    private async Task FundWithRetryAsync(string accountId)
    {
        Exception? lastException = null;

        // One initial attempt, then one retry after each backoff delay (BackoffDelays.Length + 1 total).
        var totalAttempts = BackoffDelays.Length + 1;
        for (var attempt = 0; attempt < totalAttempts; attempt++)
        {
            // Server-advised Retry-After for this attempt, if the failure carried one.
            TimeSpan? retryAfter = null;

            try
            {
                var response = await Server.TestNetFriendBot.FundAccount(accountId).Execute();
                if (!string.IsNullOrEmpty(response.Hash))
                {
                    return;
                }

                // Friendbot returned a body without Hash (e.g. error envelope).
                lastException = new InvalidOperationException(
                    $"Friendbot did not return a transaction hash for {accountId}. " +
                    $"Status={response.Status}, Title={response.Title}, Detail={response.Detail}");
            }
            catch (TooManyRequestsException ex)
            {
                lastException = ex;
                retryAfter = ex.RetryAfterDelay;
            }
            catch (ServiceUnavailableException ex)
            {
                lastException = ex;
                retryAfter = ex.RetryAfterDelay;
            }
            catch (HttpRequestException ex) // transient network error
            {
                lastException = ex;
            }

            // Sleep before the next attempt, but not after the final one.
            if (attempt < BackoffDelays.Length)
            {
                // Honor the server-advised Retry-After when present (RFC 7231), clamped to
                // MaxRetryDelay; otherwise fall back to the fixed exponential backoff.
                var backoff = BackoffDelays[attempt];
                var delay = retryAfter is { } ra && ra > backoff
                    ? (ra < MaxRetryDelay ? ra : MaxRetryDelay)
                    : backoff;
                await Task.Delay(delay);
            }
        }

        Assert.Inconclusive(
            $"Friendbot funding failed after {totalAttempts} attempts for {accountId}. " +
            $"This is likely a Testnet rate-limit or outage, not an SDK regression. " +
            $"Last error: {lastException?.Message}");
    }
}