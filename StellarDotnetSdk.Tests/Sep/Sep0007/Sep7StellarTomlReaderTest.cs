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
    // A closer of the wrong kind must not end the value: conforming parsers reject the whole document.
    [DataRow("X = [}\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "mismatched bracket")]
    [DataRow("X = {]\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "mismatched bracket")]
    [DataRow("X = [{a = 1]}\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "mismatched bracket")]
    [DataRow("A =", "expected a value")]
    [DataRow("A = '''never closed", "unterminated string")]
    [DataRow("A =\nB = 1", "expected a value")]
    [DataRow("A = 1\rB = 2", "after a value")]
    [DataRow("\"\"\"A\"\"\" = 1", "multi-line string cannot be a key")]
    [DataRow("A = \"\"\"x\"\"\"\"\"\"", "too many quotes")]
    // The header line that ends the root table is checked too; conforming parsers reject all of these.
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[broken", "']' closing a table header")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[a]junk", "after a table header")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[]", "expected a key")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[a.]", "expected a key")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[[a]", "']]' closing an array-of-tables header")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[[a] ]", "']]' closing an array-of-tables header")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[a]]", "after a table header")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[a\nb]", "']' closing a table header")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[ [a]]", "expected a key")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[\"a\nb\"]", "newline")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[a] b = 1", "after a table header")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[", "expected a key")] // '[' as the last character
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[a b]", "']' closing a table header")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[a]\r", "after a table header")] // lone CR
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[\na]", "expected a key")]
    // TOML has no lone-CR newline, and a lone CR is a control character in a comment or string.
    [DataRow("\rURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")]
    [DataRow("A = 1\n \r \nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")] // in a blank line
    [DataRow("# a\rb\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")]
    [DataRow("A = [1,\r2]\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")]
    [DataRow("A = { a = 1 }\nB = [1, # c\r\n 2,\r]\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")]
    [DataRow("A = \"\"\"a\rb\"\"\"\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")]
    [DataRow("A = \"\"\"a\\\n\r b\"\"\"\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")]
    // A CR directly before a CRLF is lone too.
    [DataRow("\r\r\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")]
    [DataRow("# c\r\r\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")]
    [DataRow("A = [1,\r\r\n2]\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")]
    [DataRow("A = \"\"\"a\r\r\nb\"\"\"\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "carriage return")]
    [DataRow("A = \"\"\"a \\  \r\r\n  b\"\"\"\nURI_REQUEST_SIGNING_KEY = \"" + Key + "\"", "invalid escape")]
    // A lone CR as the last character.
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\" # c\r", "carriage return")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n\r", "carriage return")]
    // Already rejected before lone CRs were handled anywhere: a CR ending a value line is not a line end.
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\r[a]", "after a value")]
    // A header redefining the signing key as a table.
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[URI_REQUEST_SIGNING_KEY]", "used as a table")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[[URI_REQUEST_SIGNING_KEY]]", "used as a table")]
    [DataRow("URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n[\"URI_REQUEST_SIGNING_KEY\".x]", "used as a table")]
    public void Throws_WhenTheRootTableIsMalformed(string toml, string reason)
    {
        var ex = Assert.ThrowsException<FormatException>(() => Sep7StellarTomlReader.ReadUriRequestSigningKey(toml));

        StringAssert.Contains(ex.Message, reason);
    }

    // Well-formed headers, as conforming parsers accept them, end the root table without an error.
    [TestMethod]
    [DataRow("[a]")]
    [DataRow("[ a ]")]
    [DataRow("[[a]]")]
    [DataRow("[[ a . b ]]")]
    [DataRow("[a] # a comment")]
    [DataRow("[a]\t\r\nX = 1")]
    [DataRow("[\"quoted key\".b]")]
    [DataRow("['literal'.\"basic\"]")]
    [DataRow("[a-b_c.1]")]
    [DataRow("[\"\"]")] // empty quoted key
    [DataRow("[\"a]b\"]")] // ']' and '#' inside a quoted key
    [DataRow("[\"a#b\"]")]
    [DataRow("[\"\u00E9\"]")]
    [DataRow("[\"a\\\"b\"]")] // escaped quote
    [DataRow("[\ta\t]")]
    [DataRow("[URI_REQUEST_SIGNING_KEY_2]")] // a different key that only starts the same
    [DataRow("[a]\n[broken")] // after the first header nothing is read
    public void ReadsTheKey_WhenTheRootTableEndsInAWellFormedHeader(string header)
    {
        var toml = "URI_REQUEST_SIGNING_KEY = \"" + Key + "\"\n" + header;

        Assert.AreEqual(Key, Sep7StellarTomlReader.ReadUriRequestSigningKey(toml));
    }

    [TestMethod]
    public void ReadsTheKey_WithCrlfLineEndsEverywhere()
    {
        var toml = string.Join("\r\n",
            "\r\n# a comment",
            "A = [1, # in an array",
            "  2]",
            "B = \"\"\"\r\nmulti\r\nline \\\r\n  trimmed\"\"\"",
            "",
            "URI_REQUEST_SIGNING_KEY = \"" + Key + "\" # trailing",
            "[a]",
            "");

        Assert.AreEqual(Key, Sep7StellarTomlReader.ReadUriRequestSigningKey(toml));
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
