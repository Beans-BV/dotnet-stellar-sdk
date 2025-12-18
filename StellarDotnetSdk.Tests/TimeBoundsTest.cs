using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for <see cref="TimeBounds" /> class.
/// </summary>
[TestClass]
public class TimeBoundsTest
{
    /// <summary>
    ///     Verifies that TimeBounds GetHashCode and Equals work correctly for equal time bounds.
    /// </summary>
    [TestMethod]
    public void GetHashCode_WithEqualTimeBounds_ReturnsSameHashCode()
    {
        // Arrange
        var timeBounds = new TimeBounds(56, 65);
        var timeBounds2 = new TimeBounds(56, 65);

        // Act & Assert
        Assert.AreEqual(timeBounds.GetHashCode(), timeBounds.GetHashCode());
        Assert.AreEqual(timeBounds2, timeBounds2);
    }

    /// <summary>
    ///     Verifies that TimeBounds constructor with zero values creates time bounds with both min and max time set to zero.
    /// </summary>
    [TestMethod]
    public void Constructor_WithZeroValues_SetsBothTimesToZero()
    {
        // Arrange & Act
        var timeBounds = new TimeBounds(0, 0);

        // Assert
        Assert.AreEqual(0, timeBounds.MinTime);
        Assert.AreEqual(0, timeBounds.MaxTime);
    }

    /// <summary>
    ///     Verifies that TimeBounds constructor with zero min time creates time bounds with correct values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithZeroMinTime_SetsCorrectValues()
    {
        // Arrange & Act
        var timeBounds = new TimeBounds(0, 100);

        // Assert
        Assert.AreEqual(0, timeBounds.MinTime);
        Assert.AreEqual(100, timeBounds.MaxTime);
    }

    /// <summary>
    ///     Verifies that TimeBounds constructor with zero max time creates time bounds with correct values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithZeroMaxTime_SetsCorrectValues()
    {
        // Arrange & Act
        var timeBounds = new TimeBounds(100, 0);

        // Assert
        Assert.AreEqual(100, timeBounds.MinTime);
        Assert.AreEqual(0, timeBounds.MaxTime);
    }

    /// <summary>
    ///     Verifies that TimeBounds constructor throws ArgumentException when max time is less than min time.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMaxTimeLessThanMinTime_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => new TimeBounds(20, 10));
    }

    /// <summary>
    ///     Verifies that TimeBounds constructor with DateTime creates time bounds with correct min time and zero max time.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDateTime_SetsCorrectMinTimeAndZeroMaxTime()
    {
        // Arrange
        var now = new DateTime(2018, 12, 01, 17, 30, 30, DateTimeKind.Utc);

        // Act
        var timeBounds = new TimeBounds(now);

        // Assert
        Assert.AreEqual(1543685430, timeBounds.MinTime);
        Assert.AreEqual(0, timeBounds.MaxTime);
    }

    /// <summary>
    ///     Verifies that TimeBounds constructor with maxTime DateTime creates time bounds with zero min time and correct max
    ///     time.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMaxTimeDateTime_SetsZeroMinTimeAndCorrectMaxTime()
    {
        // Arrange
        var now = new DateTime(2018, 12, 01, 17, 30, 30, DateTimeKind.Utc);

        // Act
        var timeBounds = new TimeBounds(maxTime: now);

        // Assert
        Assert.AreEqual(0, timeBounds.MinTime);
        Assert.AreEqual(1543685430, timeBounds.MaxTime);
    }

    /// <summary>
    ///     Verifies that TimeBounds constructor throws ArgumentException when max time DateTime is less than min time
    ///     DateTime.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMaxTimeLessThanMinTimeDateTime_ThrowsArgumentException()
    {
        // Arrange
        var now = new DateTime(2018, 12, 01, 17, 30, 30);
        var yesterday = now.Subtract(TimeSpan.FromDays(1));

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => new TimeBounds(now, yesterday));
    }

    /// <summary>
    ///     Verifies that TimeBounds constructor with DateTime and duration creates time bounds with correct min and max times.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDateTimeAndDuration_SetsCorrectMinAndMaxTimes()
    {
        // Arrange
        var now = new DateTime(2018, 12, 01, 17, 30, 30, DateTimeKind.Utc);
        var duration = TimeSpan.FromDays(2.0);

        // Act
        var timeBounds = new TimeBounds(now, duration);

        // Assert
        Assert.AreEqual(1543685430, timeBounds.MinTime);
        Assert.AreEqual(1543858230, timeBounds.MaxTime);
    }

    /// <summary>
    ///     Verifies that TimeBounds constructor with duration from UtcNow creates time bounds with non-zero min time and max
    ///     time greater than now.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDurationFromUtcNow_SetsCorrectTimeBounds()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var duration = TimeSpan.FromDays(2.0);

        // Act
        var timeBounds = new TimeBounds(duration);
        var maxDateTime = DateTimeOffset.FromUnixTimeSeconds(timeBounds.MaxTime);

        // Assert
        Assert.AreNotEqual(0, timeBounds.MinTime);
        Assert.IsTrue(maxDateTime > now);
    }
}