using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for deserializing <see cref="SimulateTransactionResponse" /> from Stellar RPC payloads.
/// </summary>
[TestClass]
public class SimulateTransactionResponseDeserializerTest
{
    /// <summary>
    ///     Verifies that a <c>minResourceFee</c> within the 32-bit range is deserialized unchanged.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSmallMinResourceFee_ReturnsValue()
    {
        // Arrange
        const string json = """{"minResourceFee":"100","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(100L, response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that a <c>minResourceFee</c> above <see cref="uint.MaxValue" /> deserializes at full width.
    ///     Stellar RPC declares the field as an <c>int64</c>, and values beyond 4 294 967 295 stroops (~429 XLM) are
    ///     reachable on large uploads and restores.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithMinResourceFeeAboveUintMaxValue_ReturnsFullValue()
    {
        // Arrange
        const string json = """{"minResourceFee":"5000000000","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(5_000_000_000L, response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that the largest value the <c>int64</c> wire type can carry deserializes without loss.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithMaximumInt64MinResourceFee_ReturnsFullValue()
    {
        // Arrange
        const string json = """{"minResourceFee":"9223372036854775807","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(long.MaxValue, response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that an omitted <c>minResourceFee</c> — the shape Stellar RPC returns for a failed simulation —
    ///     stays null.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithoutMinResourceFee_ReturnsNull()
    {
        // Arrange
        const string json = """{"error":"host invocation failed","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that <see cref="SimulateTransactionResponse.SorobanAuthorization" /> reports "no entries" rather
    ///     than throwing when the response carries no <c>results</c> at all. The property indexes <c>Results[0]</c>,
    ///     so without the length half of its guard this dereferences a null array.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithoutResults_ReturnsNull()
    {
        // Arrange
        const string json = """{"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies the same for an empty <c>results</c> array, which is the shape Stellar RPC returns for a
    ///     simulation that produced no results. Without the length half of the guard this throws
    ///     <see cref="System.IndexOutOfRangeException" />.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithEmptyResults_ReturnsNull()
    {
        // Arrange
        const string json = """{"results":[],"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies that a <c>null</c> element in <c>results</c> is rejected at deserialization rather than
    ///     deserializing into an array that holds it.
    ///     <para>
    ///         This payload used to be accepted, and answering it was the job of
    ///         <see cref="SimulateTransactionResponse.SorobanAuthorization" />'s internal
    ///         <c>Results[0] == null</c> check — which returns <see langword="null" />, i.e. <em>no
    ///         authorization required</em>. That is the dangerous reading: the documented assemble-and-submit
    ///         flow guards on exactly that null and would send the transaction with no auth entries. Rejecting
    ///         the payload removes the ambiguity at the point the bad data arrives.
    ///     </para>
    /// </summary>
    [TestMethod]
    public void Deserialize_WithNullResultElement_ThrowsJsonException()
    {
        const string json = """{"results":[null],"latestLedger":1}""";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions));
        StringAssert.Contains(exception.Message, "'results' array contains a null element at index 0");

        // Positive control: the same shape without the null element deserializes, so the rejection is about the
        // null element and not about the surrounding payload.
        var ok = JsonSerializer.Deserialize<SimulateTransactionResponse>(
            """{"results":[{"xdr":"AAAAAwAAABQ="}],"latestLedger":1}""", JsonOptions.DefaultOptions);
        Assert.AreEqual(1, ok!.Results!.Length);
        Assert.IsNull(ok.SorobanAuthorization);
    }

    /// <summary>
    ///     Every array guard must be attachable by the System.Text.Json <em>source generator</em>, which can only
    ///     construct a converter that is public and has a public parameterless constructor. It does not fail loudly
    ///     when it cannot: it emits <c>SYSLIB1220</c> and silently drops the converter, after which the property's
    ///     own <see cref="ArgumentException" /> guard surfaces out of <c>JsonSerializer.Deserialize</c> in place of
    ///     the documented <see cref="JsonException" /> — the exception contract inverts for exactly those consumers.
    ///     Asserting the shape here catches a regression that no behavioural test using reflection-based
    ///     serialization can see.
    /// </summary>
    [DataTestMethod]
    [DataRow(typeof(StateChangesArrayJsonConverter))]
    [DataRow(typeof(SimulationResultsArrayJsonConverter))]
    [DataRow(typeof(SimulationEventsArrayJsonConverter))]
    [DataRow(typeof(SorobanAuthArrayJsonConverter))]
    public void ArrayGuardConverters_AreSourceGeneratorCompatible(Type converterType)
    {
        Assert.IsTrue(converterType.IsPublic,
            $"{converterType.Name} is not public, so the source generator cannot construct it.");
        Assert.IsNotNull(converterType.GetConstructor(Type.EmptyTypes),
            $"{converterType.Name} has no public parameterless constructor, so the source generator cannot " +
            "construct it and would silently drop the guard.");
    }

    /// <summary>
    ///     Covers the defensive <c>Results[0] == null</c> check that
    ///     <see cref="SimulateTransactionResponse.SorobanAuthorization" /> still carries. Deserialization can no
    ///     longer produce that state, but the array is stored by reference rather than copied, so a caller
    ///     holding it can write a null back in afterwards — which is exactly the branch this pins.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WhenAResultElementIsNulledAfterAssignment_ReturnsNull()
    {
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(
            """{"results":[{"xdr":"AAAAAwAAABQ="}],"latestLedger":1}""", JsonOptions.DefaultOptions)!;
        Assert.IsNotNull(response.Results);

        response.Results[0] = null!;   // the aliasing hole the guard exists for

        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies the same guard on <c>results[i].auth</c>. Without it a null element was only
    ///     <em>incidentally</em> contained: <see cref="SimulateTransactionResponse.SorobanAuthorization" /> fed it
    ///     to the base64 decoder, and the resulting <see cref="System.ArgumentNullException" /> was caught by the
    ///     decode-failure filter and reported as a malformed XDR blob — which is not what went wrong, and is no
    ///     guarantee at all for a caller reading <c>Results[0].Auth</c> directly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithNullAuthElement_ThrowsJsonException()
    {
        const string json = """{"results":[{"xdr":"AAAAAwAAABQ=","auth":[null]}],"latestLedger":1}""";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions));
        StringAssert.Contains(exception.Message, "'auth' array contains a null element at index 0");

        // Positive control: an absent auth array is the ordinary shape for a simulation needing no
        // authorization, and must keep deserializing.
        var ok = JsonSerializer.Deserialize<SimulateTransactionResponse>(
            """{"results":[{"xdr":"AAAAAwAAABQ="}],"latestLedger":1}""", JsonOptions.DefaultOptions);
        Assert.IsNull(ok!.Results![0].Auth);
    }

    /// <summary>
    ///     Verifies the same guard on <c>events</c>. Before it, <c>"events":[null]</c> handed back an array whose
    ///     first <c>Length</c> read threw <see cref="System.NullReferenceException" /> at the caller, despite the
    ///     non-nullable element type.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithNullEventElement_ThrowsJsonException()
    {
        const string json = """{"events":["AAAA",null],"latestLedger":1}""";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions));
        StringAssert.Contains(exception.Message, "'events' array contains a null element at index 1");

        var ok = JsonSerializer.Deserialize<SimulateTransactionResponse>(
            """{"events":["AAAA"],"latestLedger":1}""", JsonOptions.DefaultOptions);
        Assert.AreEqual(1, ok!.Events!.Length);
    }

    /// <summary>
    ///     Verifies that the C# assignment path reports a rejected <em>argument</em> as
    ///     <see cref="ArgumentException" />, not as a <see cref="JsonException" /> raised from an object
    ///     initializer that has nothing to do with JSON. The wire path keeps reporting
    ///     <see cref="JsonException" /> — the tests above pin that — because the property's converter runs
    ///     before the value is ever assigned.
    /// </summary>
    [TestMethod]
    public void Construct_WithNullArrayElement_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new SimulateTransactionResponse { StateChanges = new SimulateTransactionResponse.LedgerEntryChange[1] });
        Assert.ThrowsException<ArgumentException>(() =>
            new SimulateTransactionResponse { Events = new string[1] });
        Assert.ThrowsException<ArgumentException>(() =>
            new SimulateTransactionResponse
                { Results = new SimulateTransactionResponse.SimulateInvokeHostFunctionResult[1] });
        Assert.ThrowsException<ArgumentException>(() =>
            new SimulateTransactionResponse.SimulateInvokeHostFunctionResult { Auth = new string[1] });

        // Positive control: arrays with no null element are accepted, so the rejection is about the element and
        // not about assigning the property at all.
        var ok = new SimulateTransactionResponse
        {
            StateChanges = [],
            Events = ["AAAA"],
            Results = [new SimulateTransactionResponse.SimulateInvokeHostFunctionResult()],
        };
        Assert.AreEqual(1, ok.Events!.Length);
    }

    /// <summary>
    ///     Verifies that <see cref="JsonException.Path" /> locates a failure inside a guarded array within the
    ///     whole document, not within the array. Reading the array delegates to a fresh serializer session,
    ///     which restarts the path, so every failure inside these four arrays — this one is an ordinary type
    ///     mismatch, nothing to do with null elements — was reported as <c>$[1].type</c>. An unguarded property
    ///     on the same payload is the control: its path was never affected and must stay unchanged.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithErrorInsideGuardedArray_ReportsFullDocumentPath()
    {
        // A type mismatch inside a guarded array: nothing to do with null elements, and previously reported
        // as "$[1].type" with no indication of which array it came from.
        var guarded = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(
                """{"stateChanges":[{"type":"created"},{"type":12345}]}""", JsonOptions.DefaultOptions));
        Assert.AreEqual("$.stateChanges", guarded.Path);
        StringAssert.Contains(guarded.Message, "'stateChanges'");

        var nullElement = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(
                """{"stateChanges":[{"type":"created"},null]}""", JsonOptions.DefaultOptions));
        Assert.AreEqual("$.stateChanges", nullElement.Path);

        // The case that makes a computed path unworkable: `auth` is NOT a root-level property. A path derived
        // from this converter's own field name would read "$.auth[1]" — pointing at a property that does not
        // exist on the response. The ambient path is coarser here (`results` is itself guarded, so the
        // failure is rewrapped again on the way out and resolves at the outer array) but it is a true prefix
        // of where the value actually lives, which a computed path would not be. Both field names survive in
        // the message chain.
        var nested = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(
                """{"results":[{"auth":["AAAA"]},{"auth":["AAAA",null]}]}""", JsonOptions.DefaultOptions));
        Assert.AreEqual("$.results", nested.Path);
        StringAssert.Contains(nested.Message, "'results'");
        StringAssert.Contains(nested.Message, "'auth' array contains a null element at index 1");

        // Control: an unguarded property reaches no converter of ours and must report the same path as before.
        var unguarded = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(
                """{"restorePreamble":{"minResourceFee":"not-a-number"}}""", JsonOptions.DefaultOptions));
        Assert.AreEqual("$.restorePreamble.minResourceFee", unguarded.Path);
    }

    /// <summary>
    ///     Verifies that registering a guard converter on <see cref="JsonSerializerOptions.Converters" /> — the
    ///     way every other converter in this SDK is registered — reports an ordinary exception. Both directions
    ///     delegate to <see cref="JsonSerializer" /> for the array, which resolves back to the converter when it
    ///     is registered globally; that recursion used to terminate the process with an uncatchable
    ///     <c>StackOverflowException</c>, so this test can only exist because the guard exists.
    /// </summary>
    [TestMethod]
    public void GuardConverter_WhenRegisteredGlobally_ThrowsInvalidOperationException()
    {
        var options = new JsonSerializerOptions { Converters = { new StateChangesArrayJsonConverter() } };

        var read = Assert.ThrowsException<InvalidOperationException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse.LedgerEntryChange[]>(
                """[{"type":"created"}]""", options));
        StringAssert.Contains(read.Message, "JsonSerializerOptions.Converters");
        // The misregistered converter must name itself, not the shared base.
        StringAssert.Contains(read.Message, nameof(StateChangesArrayJsonConverter));

        Assert.ThrowsException<InvalidOperationException>(() =>
            JsonSerializer.Serialize(
                new[] { new SimulateTransactionResponse.LedgerEntryChange { Type = "created" } }, options));

        // `Events` and `Auth` are both string[], guarded by two different sealed converters. Misregistering
        // either one poisons every delegated string[] read, so the payload fails whichever converter is
        // attached to the property — that part is unavoidable, and correct, because the options really are
        // misconfigured. What the exact-type check buys is the diagnosis: the converter that was actually
        // misregistered names itself, instead of the innocent one failing first and blaming the shared base.
        var authRegistered = new JsonSerializerOptions(JsonOptions.DefaultOptions)
            { Converters = { new SorobanAuthArrayJsonConverter() } };
        var blamed = Assert.ThrowsException<InvalidOperationException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>("""{"events":["AAAA"]}""", authRegistered));
        StringAssert.Contains(blamed.Message, nameof(SorobanAuthArrayJsonConverter));
        Assert.IsFalse(blamed.Message.Contains(nameof(SimulationEventsArrayJsonConverter)),
            "the innocent converter attached to the property must not be blamed");

        // Positive control: the same converter attached the intended way (a property-level [JsonConverter],
        // which is how SimulateTransactionResponse declares it) still works through the SDK's own options.
        var ok = JsonSerializer.Deserialize<SimulateTransactionResponse>(
            """{"stateChanges":[{"type":"created"}]}""", JsonOptions.DefaultOptions);
        Assert.AreEqual("created", ok!.StateChanges![0].Type);
    }

    /// <summary>
    ///     Verifies that serialization enforces the same non-null element invariant as deserialization. A
    ///     property-level converter owns both directions, so nothing else — <c>RespectNullableAnnotations</c>
    ///     included — is left to police the element type on the way out, and the array is stored by reference
    ///     rather than copied, so a caller that still holds it can write a null in after the accessor's check
    ///     passed. That is the reachable path this guards.
    /// </summary>
    [TestMethod]
    public void Serialize_WithNullElementWrittenInAfterAssignment_ThrowsJsonException()
    {
        var events = new[] { "AAAA" };
        var response = new SimulateTransactionResponse { Events = events };
        events[0] = null!; // Aliased write: the init accessor already ran and cannot see this.

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Serialize(response, JsonOptions.DefaultOptions));
        StringAssert.Contains(exception.Message, "'events' array contains a null element at index 0");

        // Positive control: the same response serializes cleanly while the array holds no null.
        events[0] = "AAAA";
        StringAssert.Contains(JsonSerializer.Serialize(response, JsonOptions.DefaultOptions), "\"Events\"");
    }

    /// <summary>
    ///     Verifies that a result carrying no <c>auth</c> key reports "no entries" rather than throwing. Stellar RPC
    ///     tags the field <c>auth,omitempty</c>, so this is the ordinary shape for a simulation that requires no
    ///     authorization; without the guard the entry loop dereferences a null array.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithoutAuth_ReturnsNull()
    {
        // Arrange
        const string json = """{"results":[{"xdr":"AAAAAQ=="}],"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies that a <c>stateChanges</c> entry with no <c>type</c> is rejected rather than leaving the
    ///     non-nullable <see cref="SimulateTransactionResponse.LedgerEntryChange.Type" /> holding
    ///     <see langword="null" />. <c>RespectNullableAnnotations</c> catches an explicit <c>null</c> but never an
    ///     absent property, so only <c>[JsonRequired]</c> closes this.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithStateChangeMissingType_ThrowsJsonException()
    {
        const string json = """{"latestLedger":1,"stateChanges":[{"key":"AAAA"}]}""";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions));
        // System.Text.Json names the CLR property, not the JSON one. Assert on that exact phrase so the test
        // cannot pass on the incidental "type" inside "JSON deserialization for type '...'".
        StringAssert.Contains(exception.Message, "missing required properties including: 'Type'");
    }

    /// <summary>
    ///     Verifies that a <c>null</c> <em>element</em> in <c>stateChanges</c> is rejected at deserialization.
    ///     <c>RespectNullableAnnotations</c> constrains the array reference, never its contents, so this
    ///     previously produced an array holding <see langword="null" /> despite the non-nullable element type —
    ///     and the very loop <c>[JsonRequired]</c> on <c>Type</c> was added to protect then threw
    ///     <see cref="System.NullReferenceException" /> instead of <see cref="JsonException" />.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithNullStateChangeElement_ThrowsJsonException()
    {
        const string json = """{"latestLedger":1,"stateChanges":[{"type":"created"},null]}""";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions));
        StringAssert.Contains(exception.Message, "'stateChanges' array contains a null element at index 1");

        // Positive control: the same payload without the null element deserializes, so the rejection is about
        // the null and not about the surrounding shape.
        var ok = JsonSerializer.Deserialize<SimulateTransactionResponse>(
            """{"latestLedger":1,"stateChanges":[{"type":"created"}]}""", JsonOptions.DefaultOptions);
        Assert.AreEqual("created", ok!.StateChanges![0].Type);
    }

    /// <summary>
    ///     Verifies that the empty <c>type</c> Stellar RPC v23.0.0/v23.0.1 emitted for pre-allocated no-op state
    ///     changes still deserializes: <c>[JsonRequired]</c> enforces presence, not membership of the three
    ///     documented literals.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithEmptyStateChangeType_IsAccepted()
    {
        const string json = """{"latestLedger":1,"stateChanges":[{"type":""}]}""";

        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(response);
        Assert.AreEqual("", response.StateChanges![0].Type);
        Assert.IsNull(response.StateChanges[0].Key);
    }

    /// <summary>
    ///     Verifies that a <c>restorePreamble</c> with no <c>minResourceFee</c> is rejected. The property is a
    ///     value type, so without <c>[JsonRequired]</c> a truncated preamble silently yielded a zero restore fee
    ///     and a caller following the documented flow would submit the RestoreFootprint operation underfunded.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRestorePreambleMissingMinResourceFee_ThrowsJsonException()
    {
        const string json = """{"latestLedger":1,"restorePreamble":{"transactionData":"AAAA"}}""";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions));
        StringAssert.Contains(exception.Message, "missing required properties including: 'MinResourceFee'");
    }

    /// <summary>
    ///     Verifies that serializing a response does not invoke the decoding
    ///     <see cref="SimulateTransactionResponse.SorobanAuthorization" /> getter. Serialization reads every
    ///     property, so without <c>[JsonIgnore]</c> an <see cref="InvalidDataException" /> escapes from inside
    ///     <see cref="JsonSerializer.Serialize{TValue}(TValue,JsonSerializerOptions)" /> for a caller who is only
    ///     trying to log or cache the response.
    /// </summary>
    [TestMethod]
    public void Serialize_WithMalformedAuthEntry_DoesNotInvokeTheDecodingGetter()
    {
        // "AAAAZAAAAAA=" is eight bytes whose credentials discriminant (100) no decoder recognises.
        const string json = """{"latestLedger":42,"results":[{"auth":["AAAAZAAAAAA="],"xdr":"AAAAAQ=="}]}""";
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(response);
        // Positive control: the getter really does throw for this payload, so the assertion below is not vacuous.
        Assert.ThrowsException<InvalidDataException>(() => _ = response.SorobanAuthorization);

        var serialized = JsonSerializer.Serialize(response, JsonOptions.DefaultOptions);

        StringAssert.Contains(serialized, "\"Results\"");
        Assert.IsFalse(serialized.Contains("SorobanAuthorization"),
            "SorobanAuthorization must not be part of the serialized payload; it is derived from Results[0].Auth.");
    }

    /// <summary>
    ///     Verifies that an auth entry naming a credentials discriminant no decoder recognises is normalized to
    ///     <see cref="InvalidDataException" /> with the originating exception preserved, rather than leaking the
    ///     generated decoder's own exception out of a property getter.
    /// </summary>
    /// <remarks>
    ///     This is the "unknown discriminant" arm specifically — eight bytes are enough to reach it, and it is a
    ///     different path from the truncation and invalid-base64 cases. Note that the
    ///     <see cref="InvalidOperationException" /> arm of the getter's filter (raised by
    ///     <c>SorobanCredentials.FromXdr</c>'s default case) is <em>not</em> reachable from here and so has no
    ///     test: the XDR enum defines exactly discriminants 0-3, <c>SorobanCredentials.FromXdr</c> handles all
    ///     four, and the generated <c>SorobanCredentialsType.Decode</c> rejects anything outside that range with
    ///     <see cref="InvalidDataException" /> before the dispatch is reached. That arm is a backstop against a
    ///     future protocol bump regenerating the enum, not a live path.
    /// </remarks>
    [TestMethod]
    public void SorobanAuthorization_WithUnknownCredentialsDiscriminant_ThrowsInvalidDataException()
    {
        // 00 00 00 64 00 00 00 00 — credentials discriminant 100, which no decoder recognises.
        const string json = """{"results":[{"auth":["AAAAZAAAAAA="]}]}""";
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(response);

        var exception = Assert.ThrowsException<InvalidDataException>(() => _ = response.SorobanAuthorization);
        StringAssert.Contains(exception.Message, "Malformed authorization entry XDR at index 0");
        // The originating decoder exception must survive as InnerException, not be flattened away.
        Assert.IsNotNull(exception.InnerException);
        StringAssert.Contains(exception.InnerException.Message, "100");
    }
}
