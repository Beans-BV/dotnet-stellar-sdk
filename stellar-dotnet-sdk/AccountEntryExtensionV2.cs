using System.Linq;

namespace stellar_dotnet_sdk;

public class AccountEntryExtensionV2
{
    private AccountEntryExtensionV2(uint numberSponsored, uint numberSponsoring, KeyPair[] signerSponsoringIDs)
    {
        NumberSponsored = numberSponsored;
        NumberSponsoring = numberSponsoring;
        SignerSponsoringIDs = signerSponsoringIDs;
    }

    public uint NumberSponsored { get; }

    public uint NumberSponsoring { get; }

    public KeyPair[] SignerSponsoringIDs { get; }
    public AccountEntryExtensionV3? ExtensionV3 { get; private set; }

    public static AccountEntryExtensionV2 FromXdr(xdr.AccountEntryExtensionV2 xdrExtensionV2)
    {
        var entryExtensionV2 = new AccountEntryExtensionV2(
            xdrExtensionV2.NumSponsored.InnerValue,
            xdrExtensionV2.NumSponsoring.InnerValue,
            xdrExtensionV2
                .SignerSponsoringIDs
                .Select(x => KeyPair.FromXdrPublicKey(x.InnerValue.InnerValue))
                .ToArray());
        if (xdrExtensionV2.Ext.Discriminant == 3)
            entryExtensionV2.ExtensionV3 = AccountEntryExtensionV3.FromXdr(xdrExtensionV2.Ext.V3);

        return entryExtensionV2;
    }
}