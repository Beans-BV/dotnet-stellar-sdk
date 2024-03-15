namespace stellar_dotnet_sdk;

public class ClaimableBalanceEntryExtensionV1
{
    private ClaimableBalanceEntryExtensionV1(uint flags)
    {
        Flags = flags;
    }

    public uint Flags { get; }

    public static ClaimableBalanceEntryExtensionV1 FromXdr(xdr.ClaimableBalanceEntryExtensionV1 xdrExtensionV1)
    {
        return new ClaimableBalanceEntryExtensionV1(xdrExtensionV1.Flags.InnerValue);
    }
}