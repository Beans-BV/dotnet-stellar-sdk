using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class PaymentResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAABAAAAAAAAAAA=", typeof(PaymentSuccess), true);
    }

    [TestMethod]
    public void TestMalformed()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAB/////wAAAAA=", typeof(PaymentMalformed), false);
    }

    [TestMethod]
    public void TestUnderfunded()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAB/////gAAAAA=", typeof(PaymentUnderfunded), false);
    }

    [TestMethod]
    public void TestSrcNoTrust()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAB/////QAAAAA=", typeof(PaymentSrcNoTrust), false);
    }

    [TestMethod]
    public void TestSrcNotAuthorized()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAB/////AAAAAA=", typeof(PaymentSrcNotAuthorized),
            false);
    }

    [TestMethod]
    public void TestNoDestination()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAB////+wAAAAA=", typeof(PaymentNoDestination),
            false);
    }

    [TestMethod]
    public void TestNoTrust()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAB////+gAAAAA=", typeof(PaymentNoTrust), false);
    }

    [TestMethod]
    public void TestNotAuthorized()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAB////+QAAAAA=", typeof(PaymentNotAuthorized),
            false);
    }

    [TestMethod]
    public void TestLineFull()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAB////+AAAAAA=", typeof(PaymentLineFull), false);
    }

    [TestMethod]
    public void TestNoIssuer()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAB////9wAAAAA=", typeof(PaymentNoIssuer), false);
    }
}