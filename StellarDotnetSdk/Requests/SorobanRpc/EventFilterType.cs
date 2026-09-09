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
///         <c>filter type invalid: if set, type must be either 'system' or 'contract'</c> for it. Every RPC release
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
    ///     Every bit <see cref="EventFilterType" /> defines. Anything outside this mask cannot be expressed on the
    ///     wire.
    /// </summary>
    private const EventFilterType KnownFlags = EventFilterType.System | EventFilterType.Contract;

    /// <summary>
    ///     Returns <see langword="true" /> when <paramref name="value" /> is made up only of defined
    ///     <see cref="EventFilterType" /> flags.
    /// </summary>
    internal static bool IsDefined(this EventFilterType value)
    {
        return (value & ~KnownFlags) == 0;
    }

    /// <summary>
    ///     Maps an <see cref="EventFilterType" /> to the string Stellar RPC expects in an event filter's
    ///     <c>type</c> field.
    /// </summary>
    /// <remarks>
    ///     Members are emitted in declaration order and joined with a bare comma, because RPC splits the value on
    ///     <c>","</c> without trimming. Add a case here when adding a member to <see cref="EventFilterType" />;
    ///     the switch is exhaustive over flag combinations by design, so a missing one fails loudly instead of
    ///     silently putting an unusable value on the wire.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="value" /> contains bits that are not defined <see cref="EventFilterType" />
    ///     flags.
    /// </exception>
    internal static string ToRequestValue(this EventFilterType value)
    {
        return value switch
        {
            EventFilterType.None => "",
            EventFilterType.System => "system",
            EventFilterType.Contract => "contract",
            KnownFlags => "system,contract",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown event filter type."),
        };
    }
}
