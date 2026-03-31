namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     A buying or selling obligation, required to satisfy (selling) or accommodate (buying) transactions.
///     See <a href="https://developers.stellar.org/docs/learn/glossary#liability">Liability</a>
/// </summary>
public class Liabilities
{
    private Liabilities(long buying, long selling)
    {
        Buying = buying;
        Selling = selling;
    }

    /// <summary>
    ///     The buying liability: the total amount of an asset that outstanding buy offers could acquire.
    /// </summary>
    public long Buying { get; }

    /// <summary>
    ///     The selling liability: the total amount of an asset committed to outstanding sell offers.
    /// </summary>
    public long Selling { get; }

    /// <summary>
    ///     Creates a <see cref="Liabilities" /> from an XDR <see cref="Xdr.Liabilities" /> object.
    /// </summary>
    /// <param name="xdr">The XDR liabilities object.</param>
    /// <returns>A <see cref="Liabilities" /> instance.</returns>
    public static Liabilities FromXdr(Xdr.Liabilities xdr)
    {
        return new Liabilities(xdr.Buying.InnerValue, xdr.Selling.InnerValue);
    }
}