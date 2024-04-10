using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class SetOptionsResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAFAAAAAAAAAAA=", typeof(SetOptionsSuccess), true);
    }

    [TestMethod]
    public void TestLowReserve()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAF/////wAAAAA=", typeof(SetOptionsLowReserve),
            false);
    }

    [TestMethod]
    public void TestTooManySigner()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAF/////gAAAAA=", typeof(SetOptionsTooManySigners),
            false);
    }

    [TestMethod]
    public void TestBadFlag()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAF/////QAAAAA=", typeof(SetOptionsBadFlags), false);
    }

    [TestMethod]
    public void TestInvalidInflation()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAF/////AAAAAA=", typeof(SetOptionsInvalidInflation),
            false);
    }

    [TestMethod]
    public void TestCantChange()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAF////+wAAAAA=", typeof(SetOptionsCantChange),
            false);
    }

    [TestMethod]
    public void TestUnknownFlag()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAF////+gAAAAA=", typeof(SetOptionsUnknownFlag),
            false);
    }

    [TestMethod]
    public void TestThresholdOutOfRange()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAF////+QAAAAA=",
            typeof(SetOptionsThresholdOutOfRange), false);
    }

    [TestMethod]
    public void TestBadSigner()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAF////+AAAAAA=", typeof(SetOptionsBadSigner), false);
    }

    [TestMethod]
    public void TestInvalidHomeDomain()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAF////9wAAAAA=", typeof(SetOptionsInvalidHomeDomain),
            false);
    }
}