using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class AccountMergeResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        var tx = Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAIAAAAAAAAAAAF9eEAAAAAAA==",
            typeof(AccountMergeSuccess), true);
        var failed = (TransactionResultFailed)tx;
        var op = (AccountMergeSuccess)failed.Results[0];
        Assert.AreEqual("10", op.SourceAccountBalance);
    }

    [TestMethod]
    public void TestMalformed()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAI/////wAAAAA=", typeof(AccountMergeMalformed),
            false);
    }

    [TestMethod]
    public void TestNoAccount()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAI/////gAAAAA=", typeof(AccountMergeNoAccount),
            false);
    }

    [TestMethod]
    public void TestImmutableSet()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAI/////QAAAAA=", typeof(AccountMergeImmutableSet),
            false);
    }

    [TestMethod]
    public void TestHasSubEntry()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAI/////AAAAAA=", typeof(AccountMergeHasSubEntries),
            false);
    }

    [TestMethod]
    public void TestSeqnumTooFar()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAI////+wAAAAA=",
            typeof(AccountMergeSequenceNumberTooFar),
            false);
    }

    [TestMethod]
    public void TestDestFull()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAI////+gAAAAA=", typeof(AccountMergeDestFull),
            false);
    }
}