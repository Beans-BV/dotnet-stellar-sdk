namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents version 1 extensions to a Stellar account entry, including liabilities.
/// </summary>
public class AccountEntryExtensionV1
{
    private AccountEntryExtensionV1(Liabilities liabilities)
    {
        Liabilities = liabilities;
    }

    /// <summary>
    ///     The buying and selling liabilities for this account.
    /// </summary>
    public Liabilities Liabilities { get; }

    /// <summary>
    ///     Version 2 extension fields for this account entry, if present.
    /// </summary>
    public AccountEntryExtensionV2? ExtensionV2 { get; private set; }

    /// <summary>
    ///     Creates an <see cref="AccountEntryExtensionV1" /> from an XDR
    ///     <see cref="Xdr.AccountEntryExtensionV1" /> object.
    /// </summary>
    /// <param name="xdrExtensionV1">The XDR extension object.</param>
    /// <returns>An <see cref="AccountEntryExtensionV1" /> instance.</returns>
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