using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Unit tests for <see cref="ExactDecimalJsonConverter" />, which reads decimal amounts without rounding.
/// </summary>
[TestClass]
public class ExactDecimalJsonConverterTest
{
    private sealed class Holder
    {
        [JsonConverter(typeof(ExactDecimalJsonConverter))]
        public decimal Value { get; init; }
    }

    private static decimal Read(string jsonValue)
    {
        return JsonSerializer.Deserialize<Holder>($"{{\"Value\":{jsonValue}}}", JsonOptions.DefaultOptions)!.Value;
    }

    [TestMethod]
    [DataRow("\"5.42\"", "5.42")]
    [DataRow("\"5.00\"", "5.00")]
    [DataRow("\"542\"", "542")]
    [DataRow("\"-1.5\"", "-1.5")]
    [DataRow("\"0\"", "0")]
    [DataRow("\"0.0000001\"", "0.0000001")]
    [DataRow("5.42", "5.42")]
    [DataRow("0.1795000", "0.1795000")]
    [DataRow("\"7922816251426433759354395033.5\"", "7922816251426433759354395033.5")]
    [DataRow("\"79228162514264337593543950335\"", "79228162514264337593543950335")]
    [DataRow("\"0.1234567890123456789012345678\"", "0.1234567890123456789012345678")]
    public void Read_WithExactValue_KeepsItsDigitsAndScale(string json, string expected)
    {
        Assert.AreEqual(expected, Read(json).ToString(CultureInfo.InvariantCulture));
    }

    [TestMethod]
    [DataRow("\"1E-7\"", "0.0000001", DisplayName = "Negative exponent, as java.math.BigDecimal writes it")]
    [DataRow("\"1e3\"", "1000", DisplayName = "Positive exponent")]
    [DataRow("1e3", "1000", DisplayName = "Bare number with exponent")]
    [DataRow("\"-5E+2\"", "-500", DisplayName = "Signed exponent")]
    [DataRow("\"1.25E1\"", "12.5", DisplayName = "Fraction shifted by the exponent")]
    [DataRow("\"100E-2\"", "1", DisplayName = "Trailing integer zeros absorb the exponent")]
    [DataRow("\"1.0E-28\"", "0.0000000000000000000000000001", DisplayName = "Smallest representable step")]
    [DataRow("\"0E-40\"", "0", DisplayName = "Zero needs no places whatever its exponent")]
    public void Read_WithExactExponentForm_ReadsTheValueItDenotes(string json, string expected)
    {
        Assert.AreEqual(expected, Read(json).ToString(CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void Read_WithLongExponentForm_ReadsTheExactValueOrThrows()
    {
        // decimal.Parse clamps a long literal's exponent and returns a different number; the converter must
        // never hand that on. Each of these denotes a value decimal holds exactly.
        Assert.AreEqual(1m, Read($"\"1{new string('0', 1001)}e-1001\""));
        Assert.AreEqual(500m, Read($"\"5{new string('0', 10020)}e-10018\""));
        Assert.AreEqual(1m, Read($"\"0.{new string('0', 10000)}1e10001\""));
        // And these cannot be held exactly: rejected, not approximated.
        Assert.ThrowsException<JsonException>(() => Read($"\"1{new string('0', 1001)}e-971\""));
        Assert.ThrowsException<JsonException>(() => Read($"\"1{new string('0', 28)}1e-1\""));
        Assert.ThrowsException<JsonException>(() => Read($"\"1{new string('0', 1001)}e-1030\""));
    }

    [TestMethod]
    public void Read_WithLongPlainLiteral_IsReadAsGivenWithItsScale()
    {
        // Plain literals of any length go straight to decimal.Parse, which reads them exactly; only exponent forms
        // are rewritten first. So a long plain literal keeps the scale decimal can hold, as a short one does.
        Assert.AreEqual("5.0000000000000000000000000000", Read($"\"5.{new string('0', 100)}\"")
            .ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(5.25m, Read($"\"{new string('0', 100)}5.25\""));
        Assert.ThrowsException<JsonException>(() =>
            Read($"\"0.{new string('0', 28)}1{new string('0', 100)}\""));
        Assert.ThrowsException<JsonException>(() =>
            Read($"\"123456789012345678901234567890.{new string('0', 40)}1\""));
    }

    [TestMethod]
    [Timeout(3000)]
    public void Read_WithHugePositiveExponent_IsRejectedWithoutExpandingIt()
    {
        // Fourteen bytes denoting a ten-billion-digit integer: rejected from the exponent alone, never built.
        Assert.ThrowsException<JsonException>(() => Read("\"1e10000000000\""));
        Assert.ThrowsException<JsonException>(() => Read("\"1E+99999999999\""));
    }

    [TestMethod]
    public void Read_WithTrailingZerosBeyondTheMaximumScale_IsAcceptedBecauseNothingIsLost()
    {
        Assert.AreEqual(1.1m, Read("\"1.10000000000000000000000000000000\""));
    }

    [TestMethod]
    [DataRow("\"0.1234567890123456789012345678901\"", DisplayName = "31 fractional digits")]
    [DataRow("\"0.00000000000000000000000000001\"", DisplayName = "Below the smallest representable step")]
    [DataRow("\"12345678901234567890123456789.1\"", DisplayName = "Too many significant digits")]
    [DataRow("\"79228162514264337593543950336\"", DisplayName = "Overflow")]
    [DataRow("0.1234567890123456789012345678901", DisplayName = "Inexact bare number")]
    [DataRow("\"1E-29\"", DisplayName = "Exponent below the smallest representable step")]
    [DataRow("\"1.2345678901234567890123456789012E-5\"", DisplayName = "Exponent form with too many digits")]
    [DataRow("\"1.23456789012345678901234567891E20\"", DisplayName = "Exponent form with too many digits in total")]
    [DataRow("\"1E+29\"", DisplayName = "Exponent overflow")]
    [DataRow("\"1E-99999999999999\"", DisplayName = "Exponent far beyond any scale")]
    public void Read_WithValueThatWouldBeRounded_Throws(string json)
    {
        var ex = Assert.ThrowsException<JsonException>(() => Read(json));
        Assert.IsNotNull(ex.Path, "The serializer should locate the failure.");
    }

    [TestMethod]
    [DataRow("\"\"")]
    [DataRow("\"1e\"")]
    [DataRow("\"1e+\"")]
    [DataRow("\"e5\"")]
    [DataRow("\"1.e5\"")]
    [DataRow("\"1E-7.5\"")]
    [DataRow("\"+1\"")]
    [DataRow("\" 1\"")]
    [DataRow("\"1 \"")]
    [DataRow("\"1,000\"")]
    [DataRow("\"1,5\"")]
    [DataRow("\".5\"")]
    [DataRow("\"5.\"")]
    [DataRow("\"-\"")]
    [DataRow("\"--1\"")]
    [DataRow("\"١\"")]
    [DataRow("\"NaN\"")]
    [DataRow("true")]
    [DataRow("null")]
    [DataRow("{}")]
    public void Read_WithInvalidLiteralOrToken_Throws(string json)
    {
        Assert.ThrowsException<JsonException>(() => Read(json));
    }

    [TestMethod]
    public void Read_ErrorMessage_EscapesAndClampsTheValue()
    {
        var ex = Assert.ThrowsException<JsonException>(() => Read("\"1\\n" + new string('9', 500) + "\""));

        Assert.IsFalse(ex.Message.Contains('\n'));
        StringAssert.Contains(ex.Message, "truncated");
    }

    [TestMethod]
    public void Write_EmitsAnInvariantString()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            Assert.AreEqual("{\"Value\":\"1234.50\"}",
                JsonSerializer.Serialize(new Holder { Value = 1234.50m }, JsonOptions.DefaultOptions));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
