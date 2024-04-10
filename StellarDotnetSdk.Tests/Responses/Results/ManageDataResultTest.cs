using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class ManageDataResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAKAAAAAAAAAAA=", typeof(ManageDataSuccess), true);
    }

    [TestMethod]
    public void TestNotSupportedYet()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAK/////wAAAAA=", typeof(ManageDataNotSupportedYet),
            false);
    }

    [TestMethod]
    public void TestNameNotFound()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAK/////gAAAAA=", typeof(ManageDataNameNotFound),
            false);
    }

    [TestMethod]
    public void TestLowReserve()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAK/////QAAAAA=", typeof(ManageDataLowReserve),
            false);
    }

    [TestMethod]
    public void TestInvalidName()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAK/////AAAAAA=", typeof(ManageDataInvalidName),
            false);
    }
}