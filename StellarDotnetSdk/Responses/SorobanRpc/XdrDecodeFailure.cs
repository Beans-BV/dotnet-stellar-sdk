using System;
using System.IO;
// Deliberately NOT `using StellarDotnetSdk.Exceptions`: that namespace declares its own FormatException,
// which would silently shadow System.FormatException in the filter below and stop it recognising the
// invalid-base64 failures it exists for. The one SDK exception type needed here is aliased in instead.
using AssetCodeLengthInvalidException = StellarDotnetSdk.Exceptions.AssetCodeLengthInvalidException;

namespace StellarDotnetSdk.Responses.SorobanRpc;

/// <summary>
///     The one declaration of which exceptions mean "this server-supplied base64 XDR blob is bad", shared by every
///     response property that decodes one. Each caller decides what to do with such a failure —
///     <c>SimulateTransactionResponse</c> normalizes it to <see cref="InvalidDataException" />,
///     <c>TransactionInfo</c> reports it as <c>null</c> — but none of them keeps its own list, so they cannot drift
///     apart over what counts as a bad payload.
/// </summary>
internal static class XdrDecodeFailure
{
    /// <summary>
    ///     Recognizes the exceptions that decoding an attacker- or server-controlled base64 XDR blob can produce,
    ///     through both the generated <c>StellarDotnetSdk.Xdr</c> readers and the SDK's own <c>FromXdr</c> dispatch
    ///     (<c>SCVal</c>, <c>ScAddress</c>, <c>Asset</c>, <c>TrustlineAsset</c>, …), which raises its own argument-
    ///     and state-validation exceptions.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The set is broader than the one <c>Sep45Challenge</c> uses, because that method decodes with the
    ///         generated <c>Xdr.SorobanAuthorizationEntry.Decode</c> and stops there.
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="InvalidDataException" /> — an unknown enum discriminant, an over-large element count,
    ///             or a fixed-width read past the end of the buffer, raised by the generated XDR decoders.
    ///         </item>
    ///         <item>
    ///             <see cref="IOException" /> (and <see cref="EndOfStreamException" />) — truncated input, or
    ///             non-zero opaque padding.
    ///         </item>
    ///         <item><see cref="FormatException" /> — invalid base64, or a length prefix that runs off the buffer.</item>
    ///         <item><see cref="IndexOutOfRangeException" /> — a read past the end of the backing array.</item>
    ///         <item>
    ///             <see cref="ArgumentException" /> (and its <see cref="ArgumentNullException" /> /
    ///             <see cref="ArgumentOutOfRangeException" /> subtypes) — a null entry, a length prefix beyond
    ///             <see cref="int.MaxValue" />, an <c>SCV_VEC</c>/<c>SCV_MAP</c> whose optional body is absent, a
    ///             metadata version this SDK does not model, or a decoded field rejected by the domain type it is
    ///             handed to.
    ///         </item>
    ///         <item>
    ///             <see cref="InvalidOperationException" /> — a discriminant the SDK's own <c>FromXdr</c> dispatch
    ///             does not accept, e.g. an <c>SCAddress</c> that decodes but is not a legal
    ///             <c>invokeHostFunction</c> argument. (Not <c>SorobanCredentials.FromXdr</c>: the generated
    ///             <c>SorobanCredentialsType.Decode</c> rejects an unknown discriminant first, with
    ///             <see cref="InvalidDataException" />.) <c>TransactionInfo.ResultValue</c> deliberately excludes
    ///             this one at its call site; see its catch clause.
    ///         </item>
    ///         <item>
    ///             <see cref="AssetCodeLengthInvalidException" /> — a <c>TRUSTLINE</c> footprint key or ledger-entry
    ///             change, or a <c>createContract</c> argument, carrying an asset code that is empty once its
    ///             trailing NUL padding is stripped, or that is too short for its <c>ALPHANUM12</c> discriminant.
    ///             Four zero bytes in an otherwise well-formed blob are enough. It derives straight from
    ///             <see cref="Exception" />, so no hierarchy above covers it and it has to be named — which is
    ///             exactly why an earlier hand-rolled filter in <c>TransactionInfo</c> missed it and let it escape
    ///             <c>TransactionMeta</c>.
    ///         </item>
    ///     </list>
    ///     <para>
    ///         The list is empirical, not proven exhaustive: it covers every exception type observed across a fuzz
    ///         of the real decoders plus a sweep of the SDK exception types reachable from these roots. A payload
    ///         that provokes something outside it — including <see cref="OutOfMemoryException" /> from the
    ///         unbounded allocation the generated array decoders still permit — propagates, by design: callers
    ///         report bad input, not every conceivable failure.
    ///     </para>
    /// </remarks>
    internal static bool IsPayloadFailure(Exception exception)
    {
        return exception is InvalidDataException or IOException or FormatException or IndexOutOfRangeException
            or ArgumentException or InvalidOperationException or AssetCodeLengthInvalidException;
    }
}
