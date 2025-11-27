namespace StellarDotnetSdk.Responses;

/// <summary>
///     Interface for responses that support pagination.
///     Responses implementing this interface can be used with Horizon's cursor-based pagination.
/// </summary>
public interface IPagingToken
{
    /// <summary>
    ///     A cursor value for use in pagination.
    ///     This token can be used to retrieve the next or previous page of results.
    /// </summary>
    string PagingToken { get; }
}