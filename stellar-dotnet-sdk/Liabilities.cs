using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class Liabilities
{
    public Liabilities(long buying, long selling)
    {
        Buying = buying;
        Selling = selling;
    }

    public long Buying { get; }
    public long Selling { get; }

    public xdr.Liabilities ToXdr()
    {
        return new xdr.Liabilities
        {
            Buying = new Int64(Buying),
            Selling = new Int64(Selling)
        };
    }

    public static Liabilities FromXdr(xdr.Liabilities xdr)
    {
        return new Liabilities(xdr.Buying.InnerValue, xdr.Selling.InnerValue);
    }
}