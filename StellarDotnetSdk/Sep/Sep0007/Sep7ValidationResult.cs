using StellarDotnetSdk.Sep.Sep0007.Exceptions;

namespace StellarDotnetSdk.Sep.Sep0007;

/// <summary>The outcome of validating a SEP-7 URI without throwing.</summary>
public sealed class Sep7ValidationResult
{
    private Sep7ValidationResult(bool isValid, string? reason, Sep7Exception? error)
    {
        IsValid = isValid;
        Reason = reason;
        Error = error;
    }

    /// <summary>Whether the URI passed validation.</summary>
    public bool IsValid { get; }

    /// <summary>Why the URI failed validation; <c>null</c> when <see cref="IsValid" /> is <c>true</c>.</summary>
    public string? Reason { get; }

    /// <summary>
    ///     The exception that made the URI invalid, when there was one, so a caller can tell failures apart by type
    ///     without parsing <see cref="Reason" />: for example <see cref="UriRequestSigningKeyChangedException" />
    ///     (SEP-7 requires alerting the user), <see cref="OriginDomainStellarTomlException" /> (possibly transient)
    ///     or <see cref="InvalidSep7SignatureException" />. <c>null</c> when valid, or when the URI was null.
    /// </summary>
    public Sep7Exception? Error { get; }

    internal static Sep7ValidationResult Valid()
    {
        return new Sep7ValidationResult(true, null, null);
    }

    internal static Sep7ValidationResult Invalid(string reason)
    {
        return new Sep7ValidationResult(false, reason, null);
    }

    internal static Sep7ValidationResult Invalid(Sep7Exception error)
    {
        return new Sep7ValidationResult(false, error.Message, error);
    }
}
