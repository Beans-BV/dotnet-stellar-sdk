using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class BumpSequenceResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAALAAAAAAAAAAA=", typeof(BumpSequenceSuccess), true);
    }

    [TestMethod]
    public void TestBadSeq()
    {
        Utils.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAL/////wAAAAA=", typeof(BumpSequenceBadSeq), false);
    }
}