using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Tests.Accounts;

/// <summary>
/// Unit tests for <see cref="Account"/> class.
/// </summary>
[TestClass]
public class AccountTest
{
    /// <summary>
    /// Verifies that Account constructor throws ArgumentNullException when arguments are null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullArguments_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => new Account((string)null, 10L));
        Assert.ThrowsException<ArgumentNullException>(() => new Account(KeyPair.Random().AccountId, null));
    }

    /// <summary>
    /// Verifies that Account constructor with string argument creates account with correct KeyPair.
    /// </summary>
    [TestMethod]
    public void Constructor_WithStringArgument_CreatesAccountWithCorrectKeyPair()
    {
        // Arrange
        var keypair = KeyPair.Random();

        // Act
        var account = new Account(keypair.Address, 7);

        // Assert
        Assert.AreEqual(account.AccountId, keypair.AccountId);
        Assert.AreEqual(account.KeyPair.AccountId, keypair.AccountId);
    }

    /// <summary>
    /// Verifies that Account constructor with muxed account creates account with correct account ID and key pair.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMuxedAccount_CreatesAccountWithCorrectAccountId()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var muxed = new MuxedAccountMed25519(keypair, 10);

        // Act
        var account = new Account(muxed, 7);

        // Assert
        Assert.AreNotEqual(account.AccountId, keypair.AccountId);
        Assert.AreEqual(account.AccountId, muxed.AccountId);
        Assert.AreEqual(account.KeyPair.AccountId, keypair.AccountId);
    }

    /// <summary>
    /// Verifies that IncrementedSequenceNumber returns incremented value without modifying account sequence number.
    /// </summary>
    [TestMethod]
    public void IncrementedSequenceNumber_ReturnsIncrementedValue_WithoutModifyingAccount()
    {
        // Arrange
        var random = KeyPair.Random();
        var account = new Account(random.AccountId, 100L);

        // Act
        var incremented = account.IncrementedSequenceNumber;

        // Assert
        Assert.AreEqual(100L, account.SequenceNumber);
        Assert.AreEqual(101L, incremented);
        incremented = account.IncrementedSequenceNumber;
        Assert.AreEqual(100L, account.SequenceNumber);
        Assert.AreEqual(101L, incremented);
    }

    /// <summary>
    /// Verifies that IncrementSequenceNumber increments the account sequence number.
    /// </summary>
    [TestMethod]
    public void IncrementSequenceNumber_IncrementsSequenceNumber()
    {
        // Arrange
        var random = KeyPair.Random();
        var account = new Account(random.AccountId, 100L);

        // Act
        account.IncrementSequenceNumber();

        // Assert
        Assert.AreEqual(account.SequenceNumber, 101L);
    }

    /// <summary>
    /// Verifies that Account getters return correct values.
    /// </summary>
    [TestMethod]
    public void Getters_ReturnCorrectValues()
    {
        // Arrange
        var keypair = KeyPair.Random();
        var account = new Account(keypair.AccountId, 100L);

        // Act & Assert
        Assert.AreEqual(account.KeyPair.AccountId, keypair.AccountId);
        Assert.AreEqual(account.AccountId, keypair.AccountId);
        Assert.AreEqual(account.SequenceNumber, 100L);
    }
}