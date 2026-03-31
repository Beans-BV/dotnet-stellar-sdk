using System;
using System.Globalization;

namespace StellarDotnetSdk;

/// <summary>
///     Utility class for converting between Stellar amount representations. Stellar stores amounts as
///     64-bit integers with 7 decimal places of precision (1 XLM = 10,000,000 stroops).
/// </summary>
public static class Amount
{
    private static readonly decimal One = new(10000000);


    /// <summary>
    ///     Converts a decimal value to its culture-invariant string representation.
    /// </summary>
    /// <param name="d">The decimal value to convert.</param>
    /// <returns>A culture-invariant string representation of the decimal.</returns>
    public static string DecimalToString(decimal d)
    {
        return d.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Converts an XDR amount (in stroops) to a human-readable decimal string.
    /// </summary>
    /// <param name="value">The XDR amount in stroops (1 unit = 10,000,000 stroops).</param>
    /// <returns>A decimal string representation of the amount.</returns>
    public static string FromXdr(long value)
    {
        var amount = decimal.Divide(new decimal(value), One);
        return DecimalToString(amount);
    }

    /// <summary>
    ///     Converts a human-readable decimal string to an XDR amount (in stroops).
    /// </summary>
    /// <param name="value">The decimal string to convert (e.g., "10.5").</param>
    /// <returns>The amount in stroops as a 64-bit integer.</returns>
    public static long ToXdr(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value), "value cannot be null");
        }

        //This basically takes a decimal value and turns it into a large integer.
        var amount = decimal.Parse(value, CultureInfo.InvariantCulture) * One;

        //MJM: Added to satisfy the OperationTest unit test of making sure a failure
        //happens when casting a decimal with fractional places into a long.
        if (amount % 1 > 0)
        {
            throw new ArithmeticException("Unable to cast decimal with fractional places into long.");
        }

        return (long)amount;
    }
}