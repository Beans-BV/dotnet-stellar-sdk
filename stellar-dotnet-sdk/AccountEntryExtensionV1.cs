namespace stellar_dotnet_sdk;

public class AccountEntryExtensionV1
{
    public Liabilities Liabilities { get; set; }

    public AccountEntryExtensionV2? ExtensionV2 { get; set; }

    public static AccountEntryExtensionV1 FromXdr(xdr.AccountEntryExtensionV1 xdrExtensionV1)
    {
        var entryExtensionV1 = new AccountEntryExtensionV1
        {
            Liabilities = Liabilities.FromXdr(xdrExtensionV1.Liabilities)
        };
        if (xdrExtensionV1.Ext.Discriminant == 2)
            entryExtensionV1.ExtensionV2 = AccountEntryExtensionV2.FromXdr(xdrExtensionV1.Ext.V2);

        return entryExtensionV1;
    }

    public xdr.AccountEntryExtensionV1 ToXdr()
    {
        return new xdr.AccountEntryExtensionV1
        {
            Liabilities = Liabilities.ToXdr(),
            Ext = new xdr.AccountEntryExtensionV1.AccountEntryExtensionV1Ext
            {
                Discriminant = ExtensionV2 != null ? 2 : 0,
                V2 = ExtensionV2?.ToXdr() ?? new xdr.AccountEntryExtensionV2()
            }
        };
    }
}