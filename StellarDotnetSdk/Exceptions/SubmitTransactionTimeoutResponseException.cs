using System;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     The exception that is thrown when the Stellar Horizon server returns an HTTP 504 Gateway Timeout
///     response to a transaction submission, indicating the upstream Stellar Core node did not respond in time.
/// </summary>
public class SubmitTransactionTimeoutResponseException : Exception
{
}