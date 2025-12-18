using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for <see cref="Network" /> class.
/// </summary>
[TestClass]
public class NetworkTest
{
    [TestCleanup]
    public void ResetNetwork()
    {
        Network.Use(null);
    }

    /// <summary>
    ///     Verifies that UseTestNetwork switches to test network with correct passphrase.
    /// </summary>
    [TestMethod]
    public void UseTestNetwork_SwitchesToTestNetwork_WithCorrectPassphrase()
    {
        // Arrange
        Network.Use(null);

        // Act
        Network.UseTestNetwork();

        // Assert
        Assert.IsNotNull(Network.Current);
        Assert.AreEqual("Test SDF Network ; September 2015", Network.Current.NetworkPassphrase);
    }

    /// <summary>
    ///     Verifies that UsePublicNetwork switches to public network with correct passphrase.
    /// </summary>
    [TestMethod]
    public void UsePublicNetwork_SwitchesToPublicNetwork_WithCorrectPassphrase()
    {
        // Arrange
        Network.Use(null);

        // Act
        Network.UsePublicNetwork();

        // Assert
        Assert.IsNotNull(Network.Current);
        Assert.AreEqual("Public Global Stellar Network ; September 2015", Network.Current.NetworkPassphrase);
    }
}