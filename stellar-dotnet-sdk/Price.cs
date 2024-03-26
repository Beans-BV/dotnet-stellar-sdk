using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using sdkxdr = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class Price
{
    /// <summary>
    ///     Create a new price. Price in Stellar is represented as a fraction.
    /// </summary>
    /// <param name="n">Numerator</param>
    /// <param name="d">Denominator</param>
    public Price(int n, int d)
    {
        Numerator = n;
        Denominator = d;
    }

    [JsonProperty(PropertyName = "n")] public int Numerator { get; }

    [JsonProperty(PropertyName = "d")] public int Denominator { get; }

    /// <summary>
    ///     Approximates<code> price</code> to a fraction.
    /// </summary>
    /// <param name="price">Example 1.25</param>
    public static Price FromString(string price)
    {
        if (string.IsNullOrEmpty(price))
            throw new ArgumentNullException(nameof(price), "price cannot be null");

        var maxInt = new decimal(int.MaxValue);
        var number = Convert.ToDecimal(price, CultureInfo.InvariantCulture);
        decimal a;
        decimal f;
        var fractions = new List<decimal[]>
        {
            new[] { new decimal(0), new decimal(1) },
            new[] { new decimal(1), new decimal(0) }
        };
        var i = 2;
        while (true)
        {
            if (number.CompareTo(maxInt) > 0)
                break;

            a = decimal.Floor(number);
            f = decimal.Subtract(number, a);
            var h = decimal.Add(decimal.Multiply(a, fractions[i - 1][0]), fractions[i - 2][0]);
            var k = decimal.Add(decimal.Multiply(a, fractions[i - 1][1]), fractions[i - 2][1]);
            if (h.CompareTo(maxInt) > 0 || k.CompareTo(maxInt) > 0)
                break;
            fractions.Add(new[] { h, k });
            if (f.CompareTo(0m) == 0)
                break;
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
    public sdkxdr.Price ToXdr()
    {
        return new sdkxdr.Price
        {
            D = new sdkxdr.Int32(Denominator),
            N = new sdkxdr.Int32(Numerator)
        };
    }

    /// <summary>
    ///     Create Price from XDR.
    /// </summary>
    /// <param name="price">Price XDR object.</param>
    /// <returns></returns>
    public static Price FromXdr(sdkxdr.Price price)
    {
        return new Price(price.N.InnerValue, price.D.InnerValue);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Price price)
            return false;

        return Numerator == price.Numerator && Denominator == price.Denominator;
    }

    public override int GetHashCode()
    {
        return (Numerator << 2) ^ Denominator;
    }
}