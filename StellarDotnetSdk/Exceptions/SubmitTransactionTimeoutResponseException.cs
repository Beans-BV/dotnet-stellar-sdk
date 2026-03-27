using System;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when a transaction submission to the Stellar Horizon server
///     times out before a response is received.
/// </summary>
public class SubmitTransactionTimeoutResponseException : Exception
{
}