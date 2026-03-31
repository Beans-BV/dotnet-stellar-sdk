namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Specifies the authorization mode used when simulating or submitting Soroban transactions.
/// </summary>
public enum AuthMode
{
    /// <summary>
    ///     Enforces all authorization entries, requiring them to be valid and present.
    /// </summary>
    ENFORCE,

    /// <summary>
    ///     Records authorization entries during simulation, automatically capturing required authorizations.
    /// </summary>
    RECORD,

    /// <summary>
    ///     Records authorization entries during simulation, including non-root authorization.
    /// </summary>
    RECORD_ALLOW_NONROOT,
}