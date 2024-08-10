namespace StellarDotnetSdk.LedgerEntries;

public class AccountEntryExtensionV1
{
    private AccountEntryExtensionV1(Liabilities liabilities)
    {
        Liabilities = liabilities;
    }

    public Liabilities Liabilities { get; }

    public AccountEntryExtensionV2? ExtensionV2 { get; private set; }

    public static AccountEntryExtensionV1 FromXdr(Xdr.AccountEntryExtensionV1 xdrExtensionV1)
    {
        var entryExtensionV1 = new AccountEntryExtensionV1(Liabilities.FromXdr(xdrExtensionV1.Liabilities));
        if (xdrExtensionV1.Ext.Discriminant == 2)
        {
            entryExtensionV1.ExtensionV2 = AccountEntryExtensionV2.FromXdr(xdrExtensionV1.Ext.V2);
        }

        return entryExtensionV1;
    }
}