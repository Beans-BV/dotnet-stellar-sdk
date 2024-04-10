using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class AllowTrustResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAHAAAAAAAAAAA=", typeof(AllowTrustSuccess), true);
    }

    [TestMethod]
    public void TestMalformed()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAH/////wAAAAA=", typeof(AllowTrustMalformed), false);
    }

    [TestMethod]
    public void TestNoTrustLine()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAH/////gAAAAA=", typeof(AllowTrustNoTrustline),
            false);
    }

    [TestMethod]
    public void TestTrustNotRequired()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAH/////QAAAAA=", typeof(AllowTrustNotRequired),
            false);
    }

    [TestMethod]
    public void TestCantRevoke()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAH/////AAAAAA=", typeof(AllowTrustCantRevoke),
            false);
    }

    [TestMethod]
    public void TestSelfNotAllowed()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAH////+wAAAAA=", typeof(AllowTrustSelfNotAllowed),
            false);
    }
}