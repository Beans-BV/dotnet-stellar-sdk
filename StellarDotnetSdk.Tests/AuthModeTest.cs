using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Tests for <see cref="AuthMode" />'s JSON wire format.
///     <para>
///         These cover the serializer paths rather than <c>AuthModeExtensions.ToRequestValue</c>, which the
///         request-level tests in <see cref="StellarRpcServerTest" /> already exercise. The two are
///         independent: the SDK builds the <c>authMode</c> request field through <c>ToRequestValue</c>, so
///         every test that inspects a request body keeps passing even if the type-level
///         <see cref="System.Text.Json.Serialization.JsonConverterAttribute" /> and every
///         <see cref="System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute" /> are deleted. The
///         attributes carry a separately documented public contract — what an ordinary
///         <see cref="JsonSerializer" /> call produces — and these tests are what pins it.
///     </para>
/// </summary>
[TestClass]
public class AuthModeTest
{
    /// <summary>
    ///     Verifies that serializing through the SDK's own options emits the literal Stellar RPC expects.
    /// </summary>
    [DataTestMethod]
    [DataRow(AuthMode.ENFORCE, "\"enforce\"")]
    [DataRow(AuthMode.RECORD, "\"record\"")]
    [DataRow(AuthMode.RECORD_ALLOW_NONROOT, "\"record_allow_nonroot\"")]
    public void Serialize_WithSdkOptions_WritesRpcWireValue(AuthMode authMode, string expected)
    {
        Assert.AreEqual(expected, JsonSerializer.Serialize(authMode, JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies that a caller's own bare options instance — which registers no enum converter of its own —
    ///     also emits the RPC literal, via the type-level converter attribute.
    /// </summary>
    [DataTestMethod]
    [DataRow(AuthMode.ENFORCE, "\"enforce\"")]
    [DataRow(AuthMode.RECORD, "\"record\"")]
    [DataRow(AuthMode.RECORD_ALLOW_NONROOT, "\"record_allow_nonroot\"")]
    public void Serialize_WithBareOptions_WritesRpcWireValue(AuthMode authMode, string expected)
    {
        Assert.AreEqual(expected, JsonSerializer.Serialize(authMode, new JsonSerializerOptions()));
    }

    /// <summary>
    ///     Verifies that the parameterless overload — the one a call site is most likely to reach for by
    ///     accident, and the shape that put <c>RECORD_ALLOW_NONROOT</c> on the wire in 15.0.0 — emits the RPC
    ///     literal too.
    /// </summary>
    [DataTestMethod]
    [DataRow(AuthMode.ENFORCE, "\"enforce\"")]
    [DataRow(AuthMode.RECORD, "\"record\"")]
    [DataRow(AuthMode.RECORD_ALLOW_NONROOT, "\"record_allow_nonroot\"")]
    public void Serialize_WithoutOptions_WritesRpcWireValue(AuthMode authMode, string expected)
    {
        Assert.AreEqual(expected, JsonSerializer.Serialize(authMode));
    }

    /// <summary>
    ///     Verifies that the RPC literals read back to the matching member.
    /// </summary>
    [DataTestMethod]
    [DataRow("\"enforce\"", AuthMode.ENFORCE)]
    [DataRow("\"record\"", AuthMode.RECORD)]
    [DataRow("\"record_allow_nonroot\"", AuthMode.RECORD_ALLOW_NONROOT)]
    public void Deserialize_WithRpcWireValue_ReturnsMatchingMember(string json, AuthMode expected)
    {
        Assert.AreEqual(expected, JsonSerializer.Deserialize<AuthMode>(json, JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies that the C# member-name spellings no longer round-trip.
    ///     <para>
    ///         Applying <see cref="System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute" /> makes
    ///         name matching case-sensitive against the supplied name, so the spellings that worked before the
    ///         wire-format fix now throw. That is a documented breaking change, and it is easy to assume the
    ///         opposite — System.Text.Json's enum converter reads plain member names case-insensitively — so
    ///         it is pinned here rather than left to the release notes.
    ///     </para>
    /// </summary>
    [DataTestMethod]
    [DataRow("\"ENFORCE\"")]
    [DataRow("\"Enforce\"")]
    [DataRow("\"RECORD_ALLOW_NONROOT\"")]
    [DataRow("\"Record_Allow_Nonroot\"")]
    public void Deserialize_WithCSharpMemberNameSpelling_ThrowsJsonException(string json)
    {
        Assert.ThrowsException<JsonException>(
            () => JsonSerializer.Deserialize<AuthMode>(json, JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies that an undefined value serializes as a bare JSON number instead of failing.
    ///     <para>
    ///         This is the gap that <c>AuthModeExtensions.ToRequestValue</c> exists to close — its remarks cite
    ///         exactly this behaviour as the reason it repeats the mapping and throws on undefined values. If a
    ///         future System.Text.Json starts rejecting undefined enum values, this test fails and that
    ///         justification needs rewriting.
    ///     </para>
    /// </summary>
    [TestMethod]
    public void Serialize_WithUndefinedValue_WritesBareNumberWithoutThrowing()
    {
        Assert.AreEqual("99", JsonSerializer.Serialize((AuthMode)99, JsonOptions.DefaultOptions));
    }
}
