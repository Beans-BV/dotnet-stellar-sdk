using System;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     Thrown when trying to load an account that doesn't exist on the Stellar network.
/// </summary>
public class AccountNotFoundException : Exception
{
    /// <summary>
    ///     Initializes a new instance with the account ID that was not found.
    /// </summary>
    /// <param name="accountId">The Stellar account ID that does not exist on the network.</param>
    public AccountNotFoundException(string accountId) : base($"Account ID {accountId} not found.")
    {
    }
}