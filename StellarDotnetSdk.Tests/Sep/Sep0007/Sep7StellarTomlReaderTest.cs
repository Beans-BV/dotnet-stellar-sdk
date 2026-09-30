using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Sep.Sep0001;
using StellarDotnetSdk.Sep.Sep0007;

namespace StellarDotnetSdk.Tests.Sep.Sep0007;

/// <summary>
///     The non-recursive reader SEP-7 uses for the origin domain's stellar.toml. It must find the root
///     <c>URI_REQUEST_SIGNING_KEY</c> exactly where the general <see cref="StellarToml" /> parser does, and must
///     survive the documents that overflow that parser's stack.
/// </summary>
[TestClass]
public class Sep7StellarTomlReaderTest
{
    private const string Key = "GD7ACHBPHSC5OJMJZZBXA7Z5IAUFTH6E6XVLNBPASDQYJ7LO5UIYBDQW";

    [TestMethod]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"")]
    [DataRow("URI_REQUEST_SIGNING_KEY=\"" + Key + "\"")]
    [DataRow("URI_REQUEST_SIGNING_KEY = '" + Key + "'")]
    [DataRow("\"URI_REQUEST_SIGNING_KEY\" = \"" + Key + "\"")]
    [DataRow("'URI_REQUEST_SIGNING_KEY' = \"" + Key + "\"")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\" # trailing comment")]
    [DataRow("\uFEFFURI_REQUEST_SIGNING_KEY = \"" + Key + "\"")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\"\"\n" + Key + "\"\"\"")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\\u0047D7ACHBPHSC5OJMJZZBXA7Z5IAUFTH6E6XVLNBPASDQYJ7LO5UIYBDQW\"")]
    [DataRow("URI_REQUEST_SIGNING_KEY = '''\r\n" + Key + "'''")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\"\"GD7ACHBPHSC5OJMJZZ\\\n    BXA7Z5IAUFTH6E6XVLNBPASDQYJ7LO5UIYBDQW\"\"\"")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\"\"GD7ACHBPHSC5OJMJZZ\\ \t \n    BXA7Z5IAUFTH6E6XVLNBPASDQYJ7LO5UIYBDQW\"\"\"")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\"\"GD7ACHBPHSC5OJMJZZ\\  \r\n\r\n  BXA7Z5IAUFTH6E6XVLNBPASDQYJ7LO5UIYBDQW\"\"\"")]
    public void ReadsTheKey_InEveryStringAndKeyForm(string toml)
    {
        Assert.AreEqual(Key, Sep7StellarTomlReader.ReadUriRequestSigningKey(toml));
    }

    [TestMethod]
    public void ReadsTheKey_FromARealisticDocument_LikeStellarToml()
    {
        var toml = string.Join("\r\n",
            "# Stellar Development Foundation",
            "VERSION=\"2.0.0\"",
            "NETWORK_PASSPHRASE=\"Public Global Stellar Network ; September 2015\"",
            "ACCOUNTS=[",
            "  \"GAENZLGHJGJRCMX5VCHOLHQXU3EMCU5XWDNU4BGGJFNLI2EL354IVBK7\", # comment inside an array",
            "  \"GAOO3LWBC4XF6VWRP5ESJ6IBHAISVJMSBTALHOQM2EZG7Q477UWA6L7U\",",
            "]",
            "DESCRIPTION = '''",
            "URI_REQUEST_SIGNING_KEY = \"not this one, it is inside a string\"",
            "'''",
            "INLINE = { a = [\"]}\", \"[{\"], c = 'x' }",
            "URI_REQUEST_SIGNING_KEY=\"" + Key + "\"",
            "",
            "[DOCUMENTATION]",
            "ORG_NAME=\"Example\"",
            "",
            "[[CURRENCIES]]",
            "code=\"USD\"");

        Assert.AreEqual(Key, Sep7StellarTomlReader.ReadUriRequestSigningKey(toml));
        // The general parser agrees, so the reader is not inventing a different reading of the document.
        Assert.AreEqual(Key, new StellarToml(toml).GeneralInformation.UriRequestSigningKey);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("# only a comment")]
    [DataRow("SIGNING_KEY = \"" + Key + "\"")]
    [DataRow("[DOCUMENTATION]\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"")]
    [DataRow("A = 1\n[[CURRENCIES]]\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"")]
    [DataRow("a.URI_REQUEST_SIGNING_KEY = \"" + Key + "\"")]
    [DataRow("X = \"URI_REQUEST_SIGNING_KEY = \\\"" + Key + "\\\"\"")]
    public void ReturnsNull_WhenTheRootTableHasNoKey(string toml)
    {
        Assert.IsNull(Sep7StellarTomlReader.ReadUriRequestSigningKey(toml));
        // The general parser agrees wherever the document is valid TOML.
        Assert.IsNull(new StellarToml(toml).GeneralInformation.UriRequestSigningKey);
    }

    [TestMethod]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "more than once")]
    [DataRow("URI_REQUEST_SIGNING_KEY = 42", "not a string")]
    [DataRow("URI_REQUEST_SIGNING_KEY = [\"" + Key + "\"]", "not a string")]
    [DataRow("URI_REQUEST_SIGNING_KEY.x = 1", "table")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key, "unterminated string")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"G\nD\"", "newline")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\\q\"", "escape")]
    // A line-ending backslash must be the last non-whitespace character on its line.
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\"\"GD7ACHBPHSC5OJMJZZ\\ BXA7Z5IAUFTH6E6XVLNBPASDQYJ7LO5UIYBDQW\"\"\"", "escape")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\"\"GD7ACHBPHSC5OJMJZZ\\\tBXA7Z5IAUFTH6E6XVLNBPASDQYJ7LO5UIYBDQW\"\"\"", "escape")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\"\"GD7ACHBPHSC5OJMJZZ\\ \"\"\"", "escape")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\"\"GD7ACHBPHSC5OJMJZZ\\\rBXA7Z5IAUFTH6E6XVLNBPASDQYJ7LO5UIYBDQW\"\"\"", "escape")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\\uD800\"", "unicode")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"\\UFFFFFFFF\"", "unicode")]
    [DataRow("URI_REQUEST_SIGNING_KEY \"" + Key + "\"", "'='")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\" junk", "after a value")]
    [DataRow("= 1", "expected a key")]
    [DataRow("A = [1, 2", "unterminated array")]
    [DataRow("A =", "expected a value")]
    [DataRow("A = '''never closed", "unterminated string")]
    [DataRow("A =\nB = 1", "expected a value")]
    [DataRow("A = 1\rB = 2", "after a value")]
    [DataRow("\"\"\"A\"\"\" = 1", "multi-line string cannot be a key")]
    [DataRow("A = \"\"\"x\"\"\"\"\"\"", "too many quotes")]
    public void Throws_WhenTheRootTableIsMalformed(string toml, string reason)
    {
        var ex = Assert.ThrowsException<FormatException>(() => Sep7StellarTomlReader.ReadUriRequestSigningKey(toml));

        StringAssert.Contains(ex.Message, reason);
    }

    [TestMethod]
    public void SkipsEveryKindOfRootValue_BeforeTheKey()
    {
        var toml = string.Join("\n",
            "TRANSFER_SERVER_SEP0024 = \"https://example.com/sep24\"", // digits in a bare key
            "some-key = 1", // hyphen in a bare key
            "PATHS = [ # a comment with ] and ' and \" in it",
            "  'C:\\no\\escapes\\here', # literal strings keep backslashes",
            "]",
            "QUOTES = \"\"\"ends with two quotes\"\"\"\"\"",
            "LITERAL_QUOTES = '''ends with two quotes'''''",
            "URI_REQUEST_SIGNING_KEY = \"" + Key + "\"");

        Assert.AreEqual(Key, Sep7StellarTomlReader.ReadUriRequestSigningKey(toml));
        Assert.AreEqual(Key, new StellarToml(toml).GeneralInformation.UriRequestSigningKey);
    }

    [TestMethod]
    public void ReadsStringValuesExactly()
    {
        // Values are returned as TOML defines them, so a key that is not a clean account id is not "fixed up".
        Assert.AreEqual("a\"\"", Sep7StellarTomlReader.ReadUriRequestSigningKey(
            "URI_REQUEST_SIGNING_KEY = \"\"\"a\"\"\"\"\""));
        Assert.AreEqual("C:\\x", Sep7StellarTomlReader.ReadUriRequestSigningKey(
            "URI_REQUEST_SIGNING_KEY = 'C:\\x'"));
    }

    [TestMethod]
    public void MalformedTomlAfterTheFirstTable_IsNotRead()
    {
        var toml = "URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[DOCUMENTATION]\n this is = = not toml [[[";

        Assert.AreEqual(Key, Sep7StellarTomlReader.ReadUriRequestSigningKey(toml));
    }

    // Each of these overflows the stack of the general parser (Nett) at a small fraction of these sizes on a
    // 512 KiB thread; a stack overflow kills the process. The reader must get through all of them.
    [TestMethod]
    [DataRow("nested arrays")]
    [DataRow("unclosed arrays")]
    [DataRow("nested inline tables")]
    [DataRow("long array")]
    [DataRow("long dotted key")]
    [DataRow("many tables")]
    [DataRow("long dotted table header")]
    public void HostileDocuments_DoNotRecurse(string shape)
    {
        const int n = 200_000;
        var keyLine = "URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n";
        var toml = shape switch
        {
            "nested arrays" => keyLine + "X = " + new string('[', n) + new string(']', n),
            "unclosed arrays" => keyLine + "X = " + new string('[', n),
            "nested inline tables" => keyLine + "X = " + string.Concat(Enumerable.Repeat("{a=", n)) + "1" +
                                      new string('}', n),
            "long array" => keyLine + "X = [" + string.Join(",", Enumerable.Repeat("1", n)) + "]",
            "long dotted key" => string.Join(".", Enumerable.Repeat("a", n)) + " = 1\n" + keyLine,
            "many tables" => keyLine + string.Concat(Enumerable.Range(0, n).Select(i => $"[t{i}]\n")),
            "long dotted table header" => keyLine + "[" + string.Join(".", Enumerable.Repeat("a", n)) + "]",
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };

        try
        {
            Assert.AreEqual(Key, Sep7StellarTomlReader.ReadUriRequestSigningKey(toml));
        }
        catch (FormatException)
        {
            // A clean rejection is as good as a result: the point is that the process survives.
            Assert.AreEqual("unclosed arrays", shape);
        }
    }
}
