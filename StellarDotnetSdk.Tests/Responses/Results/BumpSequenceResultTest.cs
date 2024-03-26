using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

[TestClass]
public class BumpSequenceResultTest
{
    [TestMethod]
    public void TestSuccess()
    {
        Util.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAALAAAAAAAAAAA=", typeof(BumpSequenceSuccess), true);
    }

    [TestMethod]
    public void TestBadSeq()
    {
        Util.AssertResultOfType("AAAAAACYloD/////AAAAAQAAAAAAAAAL/////wAAAAA=", typeof(BumpSequenceBadSeq), false);
    }
}