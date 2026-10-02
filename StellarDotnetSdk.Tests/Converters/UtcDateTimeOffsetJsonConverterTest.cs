using System;
using System.Globalization;
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
    [DoNotParallelize]
    [DataRow("America/New_York", "9999-12-31T23:59:59", 9999, 12, 31, 23, 59, 59,
        DisplayName = "Last second, local zone west of UTC")]
    [DataRow("Asia/Tokyo", "0001-01-01T00:00:00", 1, 1, 1, 0, 0, 0,
        DisplayName = "First second, local zone east of UTC")]
    [DataRow("Asia/Tokyo", "0001-01-01", 1, 1, 1, 0, 0, 0, DisplayName = "First day, local zone east of UTC")]
    public void Read_WithoutOffsetAtTheEdgeOfTheRange_IsUtcInAnyLocalZone(string zone, string text, int year,
        int month, int day, int hour, int minute, int second)
    {
        // Read as local time, these values fall outside DateTimeOffset's range in the given zone, while the same
        // text is in range as UTC. The outcome must not depend on where the SDK runs.
        RunInLocalZone(zone, () =>
        {
            var value = Read($"\"{text}\"");

            Assert.AreEqual(new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero), value);
            Assert.AreEqual(TimeSpan.Zero, value.Offset);
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
    [DataRow("\"2021-04-30T12:42:23+05\"", DisplayName = "Offset in whole hours")]
    public void Read_WithOffset_ReadsThatInstant(string json)
    {
        Assert.AreEqual(new DateTimeOffset(2021, 4, 30, 7, 42, 23, TimeSpan.Zero), Read(json));
    }

    [TestMethod]
    [DataRow("\"2021-04-30\"", 0, 0, DisplayName = "Date only")]
    [DataRow("\"2021-04-30T07:42\"", 7, 42, DisplayName = "Without seconds")]
    public void Read_WithAShorterForm_ReadsItAsUtc(string json, int hour, int minute)
    {
        Assert.AreEqual(new DateTimeOffset(2021, 4, 30, hour, minute, 0, TimeSpan.Zero), Read(json));
    }

    [TestMethod]
    public void Read_WithMoreThanSevenFractionalDigits_RoundsToTheNearestTick()
    {
        // Go's RFC3339Nano and Java's Instant write up to nine digits. A tick is 100 ns, so .123456789 s is
        // 1234567.89 ticks, which rounds to 1234568.
        Assert.AreEqual(new DateTimeOffset(2021, 4, 30, 7, 42, 23, TimeSpan.Zero).AddTicks(1234568),
            Read("\"2021-04-30T07:42:23.123456789Z\""));
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
    // DateTimeOffset.TryParse alone accepts each of the values below.
    [DataRow("\"2021-04-30Z\"", DisplayName = "Offset without a time")]
    [DataRow("\"2021-04-30t07:42:23Z\"", DisplayName = "Lower-case time designator")]
    [DataRow("\"2021-04-30T07:42:23z\"", DisplayName = "Lower-case UTC designator")]
    [DataRow("\"2021-04-30T07:42:23,5Z\"", DisplayName = "Comma before the fraction")]
    [DataRow("\"2021-04-30T07:42:23.12345678901234567Z\"", DisplayName = "Seventeen fractional digits")]
    [DataRow("\" 2021-04-30T07:42:23Z\"", DisplayName = "Leading space")]
    [DataRow("\"2021-04-30T07:42:23Z\\n\"", DisplayName = "Trailing line feed")]
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
    [DataRow("th-TH", DisplayName = "Buddhist calendar")]
    [DataRow("fa-IR", DisplayName = "Persian calendar")]
    public void Format_IgnoresTheCurrentCulture(string cultureName)
    {
        var culture = CultureInfo.CurrentCulture;
        try
        {
            // These cultures default to non-Gregorian calendars: a culture-sensitive format prints 2564 or 1400.
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);

            Assert.AreEqual("2021-04-30T07:42:23.5Z",
                UtcDateTimeOffsetJsonConverter.Format(new DateTimeOffset(2021, 4, 30, 7, 42, 23, 500, TimeSpan.Zero)));
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
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
