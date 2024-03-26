using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class CreateAccountResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        Util.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAAAAAAAAAAAAA=", typeof(CreateAccountSuccess), true);
    }

    [TestMethod]
    public void TestMalformed()
    {
        Util.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAA/////wAAAAA=", typeof(CreateAccountMalformed), false);
    }

    [TestMethod]
    public void TestUnderfunded()
    {
        Util.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAA/////gAAAAA=", typeof(CreateAccountUnderfunded),
            false);
    }

    [TestMethod]
    public void TestLowReserve()
    {
        Util.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAA/////QAAAAA=", typeof(CreateAccountLowReserve), false);
    }

    [TestMethod]
    public void TestAlreadyExist()
    {
        Util.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAA/////AAAAAA=", typeof(CreateAccountAlreadyExists),
            false);
    }
}