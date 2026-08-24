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
///         Two mechanisms carry that spelling, because neither covers every path on its own.
///         <see cref="JsonStringEnumMemberNameAttribute" /> only takes effect through a
///         <see cref="JsonStringEnumConverter" />, so it needs one to be in play; the type-level
///         <see cref="JsonConverterAttribute" /> supplies one for callers whose
///         <see cref="System.Text.Json.JsonSerializerOptions" /> registers none. Between them, every ordinary
///         serialization of an <see cref="AuthMode" /> emits the RPC literal — verified for
///         <c>JsonOptions.DefaultOptions</c>, a bare options instance, and the parameterless
///         <c>JsonSerializer.Serialize</c> overload.
///     </para>
///     <para>
///         Note which of the two actually runs, because it is not the intuitive one. System.Text.Json resolves a
///         converter in a fixed order — a <see cref="JsonConverterAttribute" /> on the <em>property</em> first,
///         then the options' <c>Converters</c> collection (the first entry whose <c>CanConvert</c> matches),
///         then a <see cref="JsonConverterAttribute" /> on the <em>type</em>. A type-level attribute is thus the
///         weakest of the three, not an override: under <c>JsonOptions.DefaultOptions</c>, whose collection
///         includes a catch-all <see cref="JsonStringEnumConverter" />, that global converter resolves this enum
///         and the attribute below is never consulted. The two agree only because
///         <see cref="JsonStringEnumConverter" /> honours the member-name attributes. A consumer who registers
///         their own <see cref="AuthMode" /> converter likewise outranks the attribute below — harmless here,
///         since <c>AuthModeExtensions.ToRequestValue</c> and not the serializer is what puts the value
///         in the <c>authMode</c> request field.
///     </para>
///     <para>
///         What broke releases 15.0.0 through 16.0.0-beta was not the global registration itself but the absence
///         of any member-name mapping to go with it: a bare <see cref="JsonStringEnumConverter" /> falls back to
///         the C# member name, so the boxed enum went out as <c>RECORD_ALLOW_NONROOT</c>.
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
    ///             The attributes make ordinary serialization correct by default — including a future call site
    ///             that puts the raw enum into a request payload, which is how the wire format broke in the first
    ///             place. They cannot be bypassed by forgetting to call this method. (They <em>can</em> be
    ///             outranked by a converter a consumer registers for <see cref="AuthMode" /> on their own
    ///             options; see the remarks on the enum. This method is unaffected by that, which is the second
    ///             reason to keep it.)
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