using System;

namespace stellar_dotnet_sdk;

/// <summary>
///     AccountNotFoundException is thrown when trying to load an account that doesn't exist on the Stellar network.
/// </summary>
public class AccountNotFoundException : Exception
{
    private string _accountId;

    public AccountNotFoundException(string accountId) : base($"Account ID {accountId} not found.")
    {
        _accountId = accountId;
    }
}