using System;

namespace stellar_dotnet_sdk;

/// <summary>
///     Thrown when trying to load an account that doesn't exist on the Stellar network.
/// </summary>
public class AccountNotFoundException : Exception
{
    public AccountNotFoundException(string accountId) : base($"Account ID {accountId} not found.")
    {
    }
}