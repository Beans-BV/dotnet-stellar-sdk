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

    public long Buying { get; }
    public long Selling { get; }

    public static Liabilities FromXdr(Xdr.Liabilities xdr)
    {
        return new Liabilities(xdr.Buying.InnerValue, xdr.Selling.InnerValue);
    }
}