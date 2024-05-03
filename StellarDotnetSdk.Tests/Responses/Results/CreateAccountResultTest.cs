using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class CreateAccountResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAAAAAAAAAAAAA=", typeof(CreateAccountSuccess), true);
    }

    [TestMethod]
    public void TestMalformed()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAA/////wAAAAA=", typeof(CreateAccountMalformed),
            false);
    }

    [TestMethod]
    public void TestUnderfunded()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAA/////gAAAAA=", typeof(CreateAccountUnderfunded),
            false);
    }

    [TestMethod]
    public void TestLowReserve()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAA/////QAAAAA=", typeof(CreateAccountLowReserve),
            false);
    }

    [TestMethod]
    public void TestAlreadyExist()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAA/////AAAAAA=", typeof(CreateAccountAlreadyExists),
            false);
    }
}