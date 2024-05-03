using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class ChangeTrustResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAGAAAAAAAAAAA=", typeof(ChangeTrustSuccess), true);
    }

    [TestMethod]
    public void TestMalformed()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAG/////wAAAAA=", typeof(ChangeTrustMalformed),
            false);
    }

    [TestMethod]
    public void TestNoIssuer()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAG/////gAAAAA=", typeof(ChangeTrustNoIssuer), false);
    }

    [TestMethod]
    public void TestInvalidLimit()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAG/////QAAAAA=", typeof(ChangeTrustInvalidLimit),
            false);
    }

    [TestMethod]
    public void TestLowReserve()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAG/////AAAAAA=", typeof(ChangeTrustLowReserve),
            false);
    }

    [TestMethod]
    public void TestSelfNotAllowed()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAG////+wAAAAA=", typeof(ChangeTrustSelfNotAllowed),
            false);
    }
}