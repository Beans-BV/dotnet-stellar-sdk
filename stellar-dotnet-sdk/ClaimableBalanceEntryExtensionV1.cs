using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class ClaimableBalanceEntryExtensionV1
{
    public uint Flags { get; init; }

    public static ClaimableBalanceEntryExtensionV1 FromXdr(xdr.ClaimableBalanceEntryExtensionV1 xdrExtensionV1)
    {
        var entryExtensionV1 = new ClaimableBalanceEntryExtensionV1
        {
            Flags = xdrExtensionV1.Flags.InnerValue
        };

        return entryExtensionV1;
    }

    public xdr.ClaimableBalanceEntryExtensionV1 ToXdr()
    {
        return new xdr.ClaimableBalanceEntryExtensionV1
        {
            Flags = new Uint32(Flags),
            Ext = new xdr.ClaimableBalanceEntryExtensionV1.ClaimableBalanceEntryExtensionV1Ext
            {
                Discriminant = 0
            }
        };
    }
}