using System;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     AccountRequiresMemoException is thrown when a transaction is trying to submit a payment operation to an account
///     that requires a memo.
///     See SEP0029: https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0029.md
/// </summary>
public class AccountRequiresMemoException : Exception
{
    public AccountRequiresMemoException(string message) : base(message)
    {
    }
}