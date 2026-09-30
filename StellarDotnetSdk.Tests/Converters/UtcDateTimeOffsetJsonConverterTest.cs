using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Unit tests for <see cref="UtcDateTimeOffsetJsonConverter" />, which reads offset-less timestamps as UTC.
/// </summary>
[TestClass]
public class UtcDateTimeOffsetJsonConverterTest
{
    private sealed class Holder
    {
        [JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))]
        public DateTimeOffset Value { get; init; }
    }

    private sealed class DefaultHolder
    {
        public DateTimeOffset Value { get; init; }
    }

    private static DateTimeOffset Read(string jsonValue)
    {
        return JsonSerializer.Deserialize<Holder>($"{{\"Value\":{jsonValue}}}", JsonOptions.DefaultOptions)!.Value;
    }

    [TestMethod]
    public void Read_WithoutOffset_IsUtcRegardlessOfTheLocalTimeZone()
    {
        var value = Read("\"2021-04-30T07:42:23\"");

        Assert.AreEqual(new DateTimeOffset(2021, 4, 30, 7, 42, 23, TimeSpan.Zero), value);
        Assert.AreEqual(TimeSpan.Zero, value.Offset);

        // Positive control: on a machine outside UTC, the built-in reader places the same text at a different
        // instant — the defect this converter exists for. (On a UTC machine the two coincide and this is skipped.)
        var local = JsonSerializer.Deserialize<DefaultHolder>("{\"Value\":\"2021-04-30T07:42:23\"}")!.Value;
        if (TimeZoneInfo.Local.GetUtcOffset(new DateTime(2021, 4, 30, 7, 42, 23)) != TimeSpan.Zero)
        {
            Assert.AreNotEqual(value, local);
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public void Read_WithoutOffset_IsUtcUnderANonUtcLocalZone()
    {
        // CI runs in UTC, where reading an offset-less value as local time is indistinguishable from reading it as
        // UTC, so the test above cannot tell the two apart there. Force a local zone with a non-zero offset.
        RunInLocalZone("Asia/Tokyo", () =>
        {
            var value = Read("\"2021-04-30T07:42:23\"");
            Assert.AreEqual(new DateTimeOffset(2021, 4, 30, 7, 42, 23, TimeSpan.Zero), value);

            // Positive control: in this zone the built-in reader does place the text at a local offset.
            var local = JsonSerializer.Deserialize<DefaultHolder>("{\"Value\":\"2021-04-30T07:42:23\"}")!.Value;
            Assert.AreNotEqual(TimeSpan.Zero, local.Offset);
        });
    }

    [TestMethod]
    [DataRow("\"2021-04-30T07:42:23.Z\"", DisplayName = "Fraction separator without digits")]
    [DataRow("\"2021-04-30T07:42:23.+05:30\"", DisplayName = "Fraction separator before an offset")]
    [DataRow("\"9999-12-31T23:59:59.99999995Z\"", DisplayName = "Fraction that rounds past the maximum")]
    public void Read_WithValueOnlyTheReaderAccepts_ThrowsJsonException(string json)
    {
        // Utf8JsonReader accepts these and DateTimeOffset.Parse does not; the failure must stay a JsonException so
        // callers see it wrapped like every other malformed body, never as a raw FormatException.
        Assert.ThrowsException<JsonException>(() => Read(json));
    }

    /// <summary>
    ///     Runs <paramref name="action" /> with the process's local time zone set to <paramref name="zone" /> through
    ///     the <c>TZ</c> variable, which the runtime honours on Linux and macOS; inconclusive where it does not take
    ///     effect and the local zone stays UTC.
    /// </summary>
    private static void RunInLocalZone(string zone, Action action)
    {
        var original = Environment.GetEnvironmentVariable("TZ");
        Environment.SetEnvironmentVariable("TZ", zone);
        TimeZoneInfo.ClearCachedData();
        try
        {
            if (TimeZoneInfo.Local.GetUtcOffset(new DateTime(2021, 4, 30, 7, 42, 23)) == TimeSpan.Zero)
            {
                Assert.Inconclusive("The local time zone could not be moved off UTC on this platform.");
            }

            action();
        }
        finally
        {
            Environment.SetEnvironmentVariable("TZ", original);
            TimeZoneInfo.ClearCachedData();
        }
    }

    [TestMethod]
    [DataRow("\"2021-04-30T07:42:23Z\"")]
    [DataRow("\"2021-04-30T09:42:23+02:00\"")]
    [DataRow("\"2021-04-30T02:42:23-05:00\"")]
    [DataRow("\"2021-04-30T07:42:23.000Z\"")]
    public void Read_WithOffset_ReadsThatInstant(string json)
    {
        Assert.AreEqual(new DateTimeOffset(2021, 4, 30, 7, 42, 23, TimeSpan.Zero), Read(json));
    }

    [TestMethod]
    public void Read_WithOffset_KeepsTheOffset()
    {
        Assert.AreEqual(TimeSpan.FromHours(2), Read("\"2021-04-30T09:42:23+02:00\"").Offset);
    }

    [TestMethod]
    [DataRow("\"30/04/2021 07:42:23\"")]
    [DataRow("\"April 30, 2021\"")]
    [DataRow("\"2021-04-30 07:42:23\"")]
    [DataRow("\"\"")]
    [DataRow("1619768543")]
    [DataRow("null")]
    public void Read_WithNonIsoValue_Throws(string json)
    {
        Assert.ThrowsException<JsonException>(() => Read(json));
    }

    [TestMethod]
    public void Format_WritesUtcWithOnlyTheSignificantFraction()
    {
        Assert.AreEqual("2021-04-30T07:42:23Z",
            UtcDateTimeOffsetJsonConverter.Format(new DateTimeOffset(2021, 4, 30, 9, 42, 23, TimeSpan.FromHours(2))));
        Assert.AreEqual("2021-04-30T07:42:23.5Z",
            UtcDateTimeOffsetJsonConverter.Format(new DateTimeOffset(2021, 4, 30, 7, 42, 23, 500, TimeSpan.Zero)));
    }

    [TestMethod]
    public void Write_RoundTrips()
    {
        var original = new Holder { Value = new DateTimeOffset(2021, 4, 30, 7, 42, 23, 125, TimeSpan.Zero) };

        var json = JsonSerializer.Serialize(original, JsonOptions.DefaultOptions);

        Assert.AreEqual("{\"Value\":\"2021-04-30T07:42:23.125Z\"}", json);
        Assert.AreEqual(original.Value, JsonSerializer.Deserialize<Holder>(json, JsonOptions.DefaultOptions)!.Value);
    }
}
