using stellar_dotnet_sdk.xdr;
using XDRTrustLineEntryV1 = stellar_dotnet_sdk.xdr.TrustLineEntry.TrustLineEntryExt.TrustLineEntryV1;

namespace stellar_dotnet_sdk;

public class TrustlineEntryExtensionV1
{
    public Liabilities Liabilities { get; set; }

    public TrustLineEntryExtensionV2? TrustlineExtensionV2 { get; set; }

    public static TrustlineEntryExtensionV1 FromXdr(XDRTrustLineEntryV1 xdrExtensionV1)
    {
        var entryExtensionV1 = new TrustlineEntryExtensionV1
        {
            Liabilities = Liabilities.FromXdr(xdrExtensionV1.Liabilities)
        };
        if (xdrExtensionV1.Ext.Discriminant == 2)
            entryExtensionV1.TrustlineExtensionV2 = TrustLineEntryExtensionV2.FromXdr(xdrExtensionV1.Ext.V2);

        return entryExtensionV1;
    }

    public XDRTrustLineEntryV1 ToXdr()
    {
        return new XDRTrustLineEntryV1
        {
            Liabilities = Liabilities.ToXdr(),
            Ext = new XDRTrustLineEntryV1.TrustLineEntryV1Ext
            {
                Discriminant = TrustlineExtensionV2 != null ? 2 : 0,
                V2 = TrustlineExtensionV2?.ToXdr() ?? new xdr.TrustLineEntryExtensionV2()
            }
        };
    }
}