using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
///     Unit tests for inflation result types.
/// </summary>
[TestClass]
public class InflationResultTest
{
    /// <summary>
    ///     Verifies that InflationSuccess result can be deserialized correctly and contains payouts.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithInflationSuccessXdr_ReturnsInflationSuccessWithPayouts()
    {
        // Arrange
        var xdrBase64 =
            "AAAAAACYloD/////AAAAAQAAAAAAAAAJAAAAAAAAAAIAAAAAKoNGsl81xj8D8XyekzKZXRuSU2KImhHkQj4QWhroY64AAAAAAJiWgAAAAAADLNchwR3S8r1eVV+aPJAT1CkmM4vNhQ3mitHQ34PP5AAAAAABMS0AAAAAAA==";

        // Act
        var tx = Utils.AssertResultOfType(xdrBase64, typeof(InflationSuccess), true);

        // Assert
        var failed = (TransactionResultFailed)tx;
        var op = (InflationSuccess)failed.Results[0];
        Assert.AreEqual(2, op.Payouts.Length);
        var payout = op.Payouts[0];
        Assert.AreEqual("GAVIGRVSL424MPYD6F6J5EZSTFORXESTMKEJUEPEII7BAWQ25BR25DUC", payout.Destination.AccountId);
        Assert.AreEqual("1", payout.Amount);
    }

    /// <summary>
    ///     Verifies that InflationNotTime result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithInflationNotTimeXdr_ReturnsInflationNotTime()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAJ/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(InflationNotTime), false);
    }
}