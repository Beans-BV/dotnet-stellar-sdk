using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Tests.Accounts;

/// <summary>
/// Unit tests for <see cref="AccountFlag"/> enum.
/// </summary>
[TestClass]
public class AccountFlagTest
{
    /// <summary>
    /// Verifies that AccountFlag enum values are correct.
    /// </summary>
    [TestMethod]
    public void AccountFlag_Values_AreCorrect()
    {
        // Act & Assert
        Assert.AreEqual(1, (int)AccountFlag.AUTH_REQUIRED_FLAG);
        Assert.AreEqual(2, (int)AccountFlag.AUTH_REVOCABLE_FLAG);
        Assert.AreEqual(4, (int)AccountFlag.AUTH_IMMUTABLE_FLAG);
        Assert.AreEqual(8, (int)AccountFlag.AUTH_CLAWBACK_FLAG);
    }
}