using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using Int32 = StellarDotnetSdk.Xdr.Int32;

namespace StellarDotnetSdk;

using xdr_Price = Xdr.Price;

/// <summary>
///     Represents a price as a ratio of two 32-bit integers, used for building Stellar transactions.
/// </summary>
/// <remarks>
///     <para>
///         This class is used when constructing operations that require a price (e.g., manage offer,
///         liquidity pool deposit). It uses <see cref="int"/> (32-bit) for numerator and denominator
///         as required by the Stellar XDR protocol specification.
///     </para>
///     <para>
///         <strong>For deserializing Horizon API responses</strong>, use <see cref="Responses.Price"/>
///         instead, which uses <see cref="long"/> to handle values that may exceed the 32-bit range.
///     </para>
/// </remarks>
/// <seealso cref="Responses.Price"/>
public class Price
{
    /// <summary>
    ///     Creates a new price. Price in Stellar is represented as a fraction.
    /// </summary>
    /// <param name="numerator">The numerator of the price ratio (32-bit integer).</param>
    /// <param name="denominator">The denominator of the price ratio (32-bit integer).</param>
    public Price(int numerator, int denominator)
    {
        Numerator = numerator;
        Denominator = denominator;
    }

    /// <summary>
    ///     The numerator of the price ratio.
    /// </summary>
    [JsonPropertyName("n")]
    public int Numerator { get; }

    /// <summary>
    ///     The denominator of the price ratio.
    /// </summary>
    [JsonPropertyName("d")]
    public int Denominator { get; }

    /// <summary>
    ///     Approximates<code> price</code> to a fraction.
    /// </summary>
    /// <param name="price">Example 1.25</param>
    public static Price FromString(string price)
    {
        if (string.IsNullOrEmpty(price))
        {
            throw new ArgumentNullException(nameof(price), "price cannot be null");
        }

        var maxInt = new decimal(int.MaxValue);
        var number = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
        decimal a;
        decimal f;
        var fractions = new List<decimal[]>
        {
            new[] { new decimal(0), new decimal(1) },
            new[] { new decimal(1), new decimal(0) },
        };
        var i = 2;
        while (true)
        {
            if (number.CompareTo(maxInt) > 0)
            {
                break;
            }

            a = decimal.Floor(number);
            f = decimal.Subtract(number, a);
            var h = decimal.Add(decimal.Multiply(a, fractions[i - 1][0]), fractions[i - 2][0]);
            var k = decimal.Add(decimal.Multiply(a, fractions[i - 1][1]), fractions[i - 2][1]);
            if (h.CompareTo(maxInt) > 0 || k.CompareTo(maxInt) > 0)
            {
                break;
            }
            fractions.Add(new[] { h, k });
            if (f.CompareTo(0m) == 0)
            {
                break;
            }
            number = decimal.Divide(1m, f);
            i += 1;
        }

        var n = fractions[fractions.Count - 1][0];
        var d = fractions[fractions.Count - 1][1];
        return new Price(Convert.ToInt32(n), Convert.ToInt32(d));
    }

    /// <summary>
    ///     Generates Price XDR object.
    /// </summary>
    public xdr_Price ToXdr()
    {
        return new xdr_Price
        {
            D = new Int32(Denominator),
            N = new Int32(Numerator)
        };
    }

    /// <summary>
    ///     Create Price from XDR.
    /// </summary>
    /// <param name="price">Price XDR object.</param>
    /// <returns></returns>
    public static Price FromXdr(xdr_Price price)
    {
        return new Price(price.N.InnerValue, price.D.InnerValue);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Price price)
        {
            return false;
        }

        return Numerator == price.Numerator && Denominator == price.Denominator;
    }

    public override int GetHashCode()
    {
        return (Numerator << 2) ^ Denominator;
    }
}