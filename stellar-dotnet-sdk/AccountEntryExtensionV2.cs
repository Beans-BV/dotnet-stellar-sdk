using System;
using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class AccountEntryExtensionV2
{
    public uint NumberSponsored { get; set; }

    public uint NumberSponsoring { get; set; }

    public KeyPair[] SignerSponsoringIDs { get; set; } = Array.Empty<KeyPair>();
    public AccountEntryExtensionV3? ExtensionV3 { get; set; }

    public static AccountEntryExtensionV2 FromXdr(xdr.AccountEntryExtensionV2 xdrExtensionV2)
    {
        var entryExtensionV2 = new AccountEntryExtensionV2
        {
            NumberSponsored = xdrExtensionV2.NumSponsored.InnerValue,
            NumberSponsoring = xdrExtensionV2.NumSponsoring.InnerValue,
            SignerSponsoringIDs = xdrExtensionV2.SignerSponsoringIDs
                .Select(x => KeyPair.FromXdrPublicKey(x.InnerValue.InnerValue))
                .ToArray()
        };
        if (xdrExtensionV2.Ext.Discriminant == 3)
            entryExtensionV2.ExtensionV3 = AccountEntryExtensionV3.FromXdr(xdrExtensionV2.Ext.V3);

        return entryExtensionV2;
    }

    public xdr.AccountEntryExtensionV2 ToXdr()
    {
        return new xdr.AccountEntryExtensionV2
        {
            NumSponsored = new Uint32(NumberSponsored),
            NumSponsoring = new Uint32(NumberSponsoring),
            Ext = new xdr.AccountEntryExtensionV2.AccountEntryExtensionV2Ext
            {
                Discriminant = ExtensionV3 != null ? 3 : 0,
                V3 = ExtensionV3 != null ? ExtensionV3.ToXdr() : new xdr.AccountEntryExtensionV3()
            },
            SignerSponsoringIDs = SignerSponsoringIDs.Select(x => new SponsorshipDescriptor
                { InnerValue = new AccountID { InnerValue = x.XdrPublicKey } }).ToArray()
        };
    }
}