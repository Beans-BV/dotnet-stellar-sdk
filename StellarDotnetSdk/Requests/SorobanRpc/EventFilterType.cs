using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Requests.SorobanRpc;

/// <summary>
///     The contract event types a <see cref="GetEventsRequest.EventFilter" /> can select. Values are combinable:
///     <c>EventFilterType.System | EventFilterType.Contract</c> matches both.
/// </summary>
/// <remarks>
///     <para>
///         Stellar RPC transports this as a single string holding a comma-separated set — <c>"contract"</c>,
///         <c>"system,contract"</c> — and validates every member against the literals <c>system</c> and
///         <c>contract</c>. The comparison is case-sensitive and the separator is a bare comma: RPC rejects
///         <c>"system, contract"</c> exactly as it rejects a typo. The wire spelling is therefore produced by
///         <see cref="EventFilterTypeJsonConverter" /> rather than by the built-in enum converter, which would
///         write the <c>", "</c> that <c>Enum.ToString()</c> uses for a flags combination.
///     </para>
///     <para>
///         That converter is attached in three places, because System.Text.Json resolves converters in a fixed
///         order — a <see cref="JsonConverterAttribute" /> on the <em>property</em>, then the options'
///         <c>Converters</c> collection (first match wins), then a <see cref="JsonConverterAttribute" /> on the
///         <em>type</em>. The attribute on this type alone is the weakest of the three and is outranked by the
///         <c>JsonStringEnumConverter</c> that <c>JsonOptions.DefaultOptions</c> registers for every enum, so
///         <see cref="GetEventsRequest.EventFilter.Type" /> carries a property-level attribute (which nothing can
///         override), <c>JsonOptions.DefaultOptions</c> registers the converter ahead of
///         <c>JsonStringEnumConverter</c>, and the type-level attribute below covers a bare
///         <see cref="System.Text.Json.JsonSerializerOptions" /> that registers neither.
///     </para>
///     <para>
///         <c>diagnostic</c> is deliberately absent. It was a legal filter value up to Stellar RPC v22.1.5, but
///         Protocol 23 removed diagnostic events from the <c>getEvents</c> stream and RPC v23.0.0 onwards answers
///         <c>filter N invalid: filter type invalid: if set, type must be either 'system' or 'contract'</c>
///         for it, where <c>N</c> is the 1-based index of the offending filter. Every RPC release
///         this SDK supports rejects it, so it is not offered.
///     </para>
/// </remarks>
[Flags]
[JsonConverter(typeof(EventFilterTypeJsonConverter))]
public enum EventFilterType
{
    /// <summary>
    ///     No type filter. Serializes to the empty string, which Stellar RPC treats exactly like omitting the field:
    ///     events of every type are returned. Prefer leaving <see cref="GetEventsRequest.EventFilter.Type" />
    ///     <see langword="null" /> to express this.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Events emitted by the host itself rather than by contract code. Wire value <c>system</c>.
    /// </summary>
    System = 1,

    /// <summary>
    ///     Events emitted by contract code. Wire value <c>contract</c>.
    /// </summary>
    Contract = 2,
}

/// <summary>
///     Extension methods for <see cref="EventFilterType" />.
/// </summary>
internal static class EventFilterTypeExtensions
{
    /// <summary>
    ///     Maps an <see cref="EventFilterType" /> to the string Stellar RPC expects in an event filter's
    ///     <c>type</c> field, or reports that the value has no wire spelling.
    /// </summary>
    /// <param name="value">The value to map.</param>
    /// <param name="requestValue">
    ///     The wire spelling when this returns <see langword="true" />; the empty string otherwise.
    /// </param>
    /// <returns><see langword="true" /> when <paramref name="value" /> has a wire spelling.</returns>
    /// <remarks>
    ///     <para>
    ///         Members are emitted in declaration order and joined with a bare comma, because RPC splits the value
    ///         on <c>","</c> without trimming. Add a case here when adding a member to
    ///         <see cref="EventFilterType" />.
    ///     </para>
    ///     <para>
    ///         This switch is the single source of truth for which values are serializable, and both
    ///         <see cref="IsDefined" /> and <see cref="Converters.EventFilterTypeJsonConverter" />'s
    ///         <c>Write</c> derive their answer from it rather than testing a separate mask. That matters:
    ///         two independent declarations of one set drift apart silently, and the specific way they would
    ///         drift here is that a newly added member is admitted by the guard and then has no case to match —
    ///         which is exactly how an <see cref="ArgumentOutOfRangeException" /> used to escape from inside
    ///         <c>JsonSerializer.Serialize</c>. Deriving both from this one switch means a missing case fails at
    ///         the guard instead, as a <c>JsonException</c>, and cannot put an unusable value on the wire.
    ///     </para>
    /// </remarks>
    internal static bool TryToRequestValue(this EventFilterType value, out string requestValue)
    {
        switch (value)
        {
            case EventFilterType.None:
                requestValue = "";
                return true;
            case EventFilterType.System:
                requestValue = "system";
                return true;
            case EventFilterType.Contract:
                requestValue = "contract";
                return true;
            case EventFilterType.System | EventFilterType.Contract:
                requestValue = "system,contract";
                return true;
            default:
                requestValue = "";
                return false;
        }
    }

    /// <summary>
    ///     Returns <see langword="true" /> when <paramref name="value" /> is made up only of defined
    ///     <see cref="EventFilterType" /> flags — equivalently, when it has a wire spelling.
    /// </summary>
    internal static bool IsDefined(this EventFilterType value)
    {
        return value.TryToRequestValue(out _);
    }
}
