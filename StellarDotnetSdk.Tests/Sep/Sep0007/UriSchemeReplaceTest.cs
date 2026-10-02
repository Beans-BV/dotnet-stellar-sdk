using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Sep.Sep0007;
using StellarDotnetSdk.Sep.Sep0007.Exceptions;
using StellarDotnetSdk.Tests.Sep.Sep0007.Fixtures;

namespace StellarDotnetSdk.Tests.Sep.Sep0007;

[TestClass]
public class UriSchemeReplaceTest
{
    private static readonly Sep7Replacement[] SpecReplacements =
    {
        new("X", "sourceAccount", Sep7TestVectors.SpecReplaceHintX),
        new("Y", "operations[0].sourceAccount", Sep7TestVectors.SpecReplaceHintY),
        new("Y", "operations[1].destination", Sep7TestVectors.SpecReplaceHintY),
    };

    private static readonly string TxPrefix = "web+stellar:tx?xdr=" + Uri.EscapeDataString(Sep7TestVectors.SpecTxXdr);

    [TestMethod]
    public void ParseReplacements_SpecExample_YieldsOneEntryPerField()
    {
        var replacements = UriScheme.ParseReplacements(Sep7TestVectors.SpecReplace);

        CollectionAssert.AreEqual(SpecReplacements, replacements.ToArray());
    }

    [TestMethod]
    public void ParseUri_SpecEncodedReplace_IsDecodedAndParsed()
    {
        // The encoded form printed in the spec; the typescript-wallet-sdk suite uses the same URI.
        var request = UriScheme.ParseUri(TxPrefix + "&replace=" + Sep7TestVectors.SpecReplaceEncoded);

        Assert.AreEqual(Sep7TestVectors.SpecReplace, request.Replace);
        CollectionAssert.AreEqual(SpecReplacements, request.Replacements.ToArray());
    }

    [TestMethod]
    public void ParseUri_SpecTxExample2_HasOneReplacement()
    {
        var request = UriScheme.ParseUri(Sep7TestVectors.SpecTxExample2);

        Assert.AreEqual(1, request.Replacements.Count);
        Assert.AreEqual(new Sep7Replacement("X", "sourceAccount", "account on which to create the trustline"),
            request.Replacements[0]);
    }

    [TestMethod]
    public void ReplacementsToString_SpecReplacements_MatchesSpecExample()
    {
        // Soneso Flutter vector: composing the three spec replacements yields the spec string exactly.
        Assert.AreEqual(Sep7TestVectors.SpecReplace, UriScheme.ReplacementsToString(SpecReplacements));
    }

    [TestMethod]
    public void ReplacementsToString_RoundTripsThroughParse()
    {
        var replace = UriScheme.ReplacementsToString(SpecReplacements);

        CollectionAssert.AreEqual(SpecReplacements, UriScheme.ParseReplacements(replace).ToArray());
    }

    [TestMethod]
    public void ReplacementsToString_SharedIdentifier_EmitsHintOnce()
    {
        var replace = UriScheme.ReplacementsToString(new[]
        {
            new Sep7Replacement("X", "operations[0].destination", "same account"),
            new Sep7Replacement("X", "operations[1].destination", "same account"),
        });

        Assert.AreEqual("operations[0].destination:X,operations[1].destination:X;X:same account", replace);
    }

    [TestMethod]
    public void ParseReplacements_SonesoVectors()
    {
        var two = UriScheme.ParseReplacements(
            "sourceAccount:X,operations[0].destination:Y;X:account paying fees,Y:receiving account");
        var shared = UriScheme.ParseReplacements(
            "operations[0].destination:X,operations[1].destination:X;X:same account");

        CollectionAssert.AreEqual(new[]
        {
            new Sep7Replacement("X", "sourceAccount", "account paying fees"),
            new Sep7Replacement("Y", "operations[0].destination", "receiving account"),
        }, two.ToArray());
        Assert.AreEqual(2, shared.Count);
        Assert.IsTrue(shared.All(r => r.Id == "X" && r.Hint == "same account"));
    }

    [TestMethod]
    public void ParseReplacements_Empty_YieldsEmptyList()
    {
        Assert.AreEqual(0, UriScheme.ParseReplacements("").Count);
    }

    [TestMethod]
    public void ParseReplacements_HintWithColon_KeepsWholeHint()
    {
        // Only the first ':' separates the identifier from its hint (peer SDKs truncate at the second one).
        var replacements = UriScheme.ParseReplacements("sourceAccount:X;X:pay to: the fee account");

        Assert.AreEqual("pay to: the fee account", replacements[0].Hint);
    }

    [TestMethod]
    public void ParseReplacements_SorobanTxrepPath_IsAccepted()
    {
        var replacements = UriScheme.ParseReplacements(
            "operations[0].body.invokeHostFunctionOp.hostFunction.invokeContract.args[1].address:A;A:recipient");

        Assert.AreEqual("A", replacements[0].Id);
    }

    [TestMethod]
    [DataRow("sourceAccount:X,operations[0].sourceAccount:Y", "no hints section")] // typescript-wallet-sdk vector
    [DataRow("sourceAccount:X", "no hints section")] // typescript-wallet-sdk vector
    [DataRow("sourceAccount:X;Y:The account", "unbalanced")] // spec + typescript-wallet-sdk vector
    [DataRow("sourceAccount:X,fee:Y;X:fee payer", "unbalanced")]
    [DataRow("sourceAccount:X;X:fee payer,Y:unused", "unbalanced")]
    [DataRow("sourceAccount:X;X:a;X:b", "more than one ';'")]
    [DataRow("sourceAccount;X:hint", "'txrep_path:reference_identifier'")]
    [DataRow("sourceAccount:;X:hint", "'txrep_path:reference_identifier'")]
    [DataRow(":X;X:hint", "'txrep_path:reference_identifier'")]
    [DataRow("a:b:X;X:hint", "'txrep_path:reference_identifier'")]
    [DataRow("sourceAccount:X;X", "'reference_identifier:hint'")]
    [DataRow("sourceAccount:X;X:", "'reference_identifier:hint'")]
    [DataRow("sourceAccount:X;:hint", "'reference_identifier:hint'")]
    [DataRow("sourceAccount:X;X:one hint, with a comma", "'reference_identifier:hint'")]
    [DataRow("sourceAccount:X,sourceAccount:X;X:hint", "more than once")]
    [DataRow("sourceAccount:X;X:a,X:b", "more than one hint")]
    [DataRow("tx.sourceAccount:X;X:hint", "'tx.' prefix")]
    [DataRow("signatures[0].signature:X;X:hint", "signature")]
    [DataRow("operations.len:X;X:hint", "metadata")]
    [DataRow("cond.timeBounds._present:X;X:hint", "metadata")]
    [DataRow("operations[a].sourceAccount:X;X:hint", "not a valid Txrep field path")]
    [DataRow("source account:X;X:hint", "not a valid Txrep field path")]
    [DataRow("operations[0]..sourceAccount:X;X:hint", "not a valid Txrep field path")]
    public void ParseReplacements_Invalid_Throws(string replace, string reasonFragment)
    {
        var ex = Assert.ThrowsException<InvalidSep7UriException>(() => UriScheme.ParseReplacements(replace));

        StringAssert.Contains(ex.Message, reasonFragment);
    }

    [TestMethod]
    public void ValidateUri_InvalidReplace_IsRejected()
    {
        var result = UriScheme.ValidateUri(TxPrefix + "&replace=" + Uri.EscapeDataString("sourceAccount:X;Y:The account"));

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.Reason, "unbalanced");
    }

    [TestMethod]
    public void ReplacementsToString_Empty_IsEmptyString()
    {
        Assert.AreEqual("", UriScheme.ReplacementsToString(new List<Sep7Replacement>()));
    }

    [TestMethod]
    [DataRow("X", "tx.sourceAccount", "hint", "'tx.' prefix")]
    [DataRow("X", "not a path", "hint", "not a valid Txrep field path")]
    [DataRow("", "sourceAccount", "hint", "reference identifier")]
    [DataRow("X:Y", "sourceAccount", "hint", "reference identifier")]
    [DataRow("X,Y", "sourceAccount", "hint", "reference identifier")]
    [DataRow("X", "sourceAccount", "", "hint")]
    [DataRow("X", "sourceAccount", "a;b", "hint")]
    [DataRow("X", "sourceAccount", "a,b", "hint")]
    [DataRow("X", "sourceAccount", "a:b", "hint")]
    public void ReplacementsToString_InvalidReplacement_Throws(string id, string path, string hint,
        string reasonFragment)
    {
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            UriScheme.ReplacementsToString(new[] { new Sep7Replacement(id, path, hint) }));

        StringAssert.Contains(ex.Message, reasonFragment);
    }

    [TestMethod]
    public void ReplacementsToString_ConflictingHintsOrRepeatedPath_Throws()
    {
        Assert.ThrowsException<ArgumentException>(() => UriScheme.ReplacementsToString(new[]
        {
            new Sep7Replacement("X", "sourceAccount", "a"),
            new Sep7Replacement("X", "fee", "b"),
        }));
        Assert.ThrowsException<ArgumentException>(() => UriScheme.ReplacementsToString(new[]
        {
            new Sep7Replacement("X", "sourceAccount", "a"),
            new Sep7Replacement("Y", "sourceAccount", "b"),
        }));
    }

    [TestMethod]
    public void Sep7Replacement_NullArguments_Throw()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new Sep7Replacement(null!, "p", "h"));
        Assert.ThrowsException<ArgumentNullException>(() => new Sep7Replacement("i", null!, "h"));
        Assert.ThrowsException<ArgumentNullException>(() => new Sep7Replacement("i", "p", null!));
    }

    [TestMethod]
    public void Sep7Replacement_EqualityIsByValue()
    {
        var a = new Sep7Replacement("X", "fee", "hint");
        var b = new Sep7Replacement("X", "fee", "hint");

        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        Assert.AreNotEqual(a, new Sep7Replacement("X", "fee", "other"));
        Assert.AreEqual("fee:X (hint)", a.ToString());
    }

    // ---- minor-finding fixes (coverage) ----

    [TestMethod]
    public void ReplacementsToString_HintsFollowFirstSeenOrder_NotAlphabetical()
    {
        var value = UriScheme.ReplacementsToString(new[]
        {
            new Sep7Replacement("Y", "sourceAccount", "first"),
            new Sep7Replacement("X", "operations[0].destination", "second"),
        });

        Assert.AreEqual("sourceAccount:Y,operations[0].destination:X;Y:first,X:second", value);
    }

    [TestMethod]
    public void ParseReplacements_SignaturesAnywhereInThePath_AreRejected()
    {
        var ex = Assert.ThrowsException<InvalidSep7UriException>(() =>
            UriScheme.ParseReplacements("feeBump.signatures[0].signature:X;X:h"));

        StringAssert.Contains(ex.Message, "signature");
    }

    [TestMethod]
    public void ParseReplacements_TxSegmentAfterThePrefix_IsAllowed()
    {
        // Only a leading "tx." is forbidden; a later "tx" segment is a real field (the fee bump's inner tx).
        var replacements = UriScheme.ParseReplacements("feeBump.tx.feeSource:X;X:h");

        Assert.AreEqual("feeBump.tx.feeSource", replacements.Single().Path);
    }
}
