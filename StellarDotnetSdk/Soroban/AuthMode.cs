using System;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Specifies the authorization mode used when simulating or submitting Soroban transactions.
/// </summary>
/// <remarks>
///     Stellar RPC names these modes <c>enforce</c>, <c>record</c> and <c>record_allow_nonroot</c>, and matches
///     the <c>authMode</c> request field against those literals case-sensitively. The wire spelling is therefore
///     pinned on the members themselves rather than left to whatever enum naming a serializer happens to apply.
///     <para>
///         The type-level <see cref="JsonConverterAttribute" /> is what makes that pinning hold: on its own,
///         <see cref="JsonStringEnumMemberNameAttribute" /> only takes effect when a
///         <see cref="JsonStringEnumConverter" /> is registered on the options in use. Relying on such a global
///         registration is precisely what put <c>RECORD_ALLOW_NONROOT</c> on the wire in releases 15.0.0 through
///         16.0.0-beta, so the contract is attached to the type instead of to any one options instance.
///     </para>
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<AuthMode>))]
public enum AuthMode
{
    /// <summary>
    ///     Enforces all authorization entries, requiring them to be valid and present.
    /// </summary>
    [JsonStringEnumMemberName("enforce")]
    ENFORCE,

    /// <summary>
    ///     Records authorization entries during simulation, automatically capturing required authorizations.
    /// </summary>
    [JsonStringEnumMemberName("record")]
    RECORD,

    /// <summary>
    ///     Records authorization entries during simulation, including non-root authorization.
    /// </summary>
    [JsonStringEnumMemberName("record_allow_nonroot")]
    RECORD_ALLOW_NONROOT,
}

/// <summary>
///     Extension methods for <see cref="AuthMode" />.
/// </summary>
internal static class AuthModeExtensions
{
    /// <summary>
    ///     Maps an <see cref="AuthMode" /> to the string Stellar RPC expects in the <c>authMode</c> request field.
    /// </summary>
    /// <remarks>
    ///     This deliberately repeats the mapping already declared by the
    ///     <see cref="JsonStringEnumMemberNameAttribute" />s on <see cref="AuthMode" />, because the two guard
    ///     different failure modes and neither covers both:
    ///     <list type="bullet">
    ///         <item>
    ///             The attributes make <em>every</em> serialization path correct by default — including a future
    ///             call site that puts the raw enum into a request payload, which is how the wire format broke in
    ///             the first place. They cannot be bypassed by forgetting to call this method.
    ///         </item>
    ///         <item>
    ///             The attributes do not validate. An undefined value — <c>(AuthMode)99</c>, or a member added to
    ///             the enum but not to the attribute set — serializes as a bare JSON number, which RPC rejects with
    ///             a JSON-RPC <c>-32602</c> error that this SDK surfaces to the caller as a <see langword="null" />
    ///             response. This switch throws instead, so the mistake is caught in the caller's own stack frame.
    ///         </item>
    ///     </list>
    ///     Keep both in step when adding a member: the attribute defines the wire value, this switch enforces that
    ///     one exists.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not a defined <see cref="AuthMode" />.</exception>
    internal static string ToRequestValue(this AuthMode authMode)
    {
        return authMode switch
        {
            AuthMode.ENFORCE => "enforce",
            AuthMode.RECORD => "record",
            AuthMode.RECORD_ALLOW_NONROOT => "record_allow_nonroot",
            _ => throw new ArgumentOutOfRangeException(nameof(authMode), authMode, "Unknown authorization mode."),
        };
    }
}