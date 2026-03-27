namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Specifies the authorization mode used when simulating or submitting Soroban transactions.
/// </summary>
public enum AuthMode
{
    ENFORCE,
    RECORD,
    RECORD_ALLOW_NONROOT,
}