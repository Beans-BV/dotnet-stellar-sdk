// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct AccountEntryExtensionV2
//  {
//      uint32 numSponsored;
//      uint32 numSponsoring;
//      SponsorshipDescriptor signerSponsoringIDs<MAX_SIGNERS>;
//  
//      union switch (int v)
//      {
//      case 0:
//          void;
//      case 3:
//          AccountEntryExtensionV3 v3;
//      }
//      ext;
//  };

//  ===========================================================================
public class AccountEntryExtensionV2
{
    public Uint32 NumSponsored { get; set; }
    public Uint32 NumSponsoring { get; set; }
    public SponsorshipDescriptor[] SignerSponsoringIDs { get; set; }
    public AccountEntryExtensionV2Ext Ext { get; set; }

    public static void Encode(XdrDataOutputStream stream, AccountEntryExtensionV2 encodedAccountEntryExtensionV2)
    {
        Uint32.Encode(stream, encodedAccountEntryExtensionV2.NumSponsored);
        Uint32.Encode(stream, encodedAccountEntryExtensionV2.NumSponsoring);
        var signerSponsoringIDssize = encodedAccountEntryExtensionV2.SignerSponsoringIDs.Length;
        stream.WriteInt(signerSponsoringIDssize);
        for (var i = 0; i < signerSponsoringIDssize; i++)
        {
            SponsorshipDescriptor.Encode(stream, encodedAccountEntryExtensionV2.SignerSponsoringIDs[i]);
        }
        AccountEntryExtensionV2Ext.Encode(stream, encodedAccountEntryExtensionV2.Ext);
    }

    public static AccountEntryExtensionV2 Decode(XdrDataInputStream stream)
    {
        var decodedAccountEntryExtensionV2 = new AccountEntryExtensionV2();
        decodedAccountEntryExtensionV2.NumSponsored = Uint32.Decode(stream);
        decodedAccountEntryExtensionV2.NumSponsoring = Uint32.Decode(stream);
        var signerSponsoringIDssize = stream.ReadInt();
        decodedAccountEntryExtensionV2.SignerSponsoringIDs = new SponsorshipDescriptor[signerSponsoringIDssize];
        for (var i = 0; i < signerSponsoringIDssize; i++)
        {
            decodedAccountEntryExtensionV2.SignerSponsoringIDs[i] = SponsorshipDescriptor.Decode(stream);
        }
        decodedAccountEntryExtensionV2.Ext = AccountEntryExtensionV2Ext.Decode(stream);
        return decodedAccountEntryExtensionV2;
    }

    public class AccountEntryExtensionV2Ext
    {
        public int Discriminant { get; set; }

        public AccountEntryExtensionV3 V3 { get; set; }

        public static void Encode(XdrDataOutputStream stream,
            AccountEntryExtensionV2Ext encodedAccountEntryExtensionV2Ext)
        {
            stream.WriteInt(encodedAccountEntryExtensionV2Ext.Discriminant);
            switch (encodedAccountEntryExtensionV2Ext.Discriminant)
            {
                case 0:
                    break;
                case 3:
                    AccountEntryExtensionV3.Encode(stream, encodedAccountEntryExtensionV2Ext.V3);
                    break;
            }
        }

        public static AccountEntryExtensionV2Ext Decode(XdrDataInputStream stream)
        {
            var decodedAccountEntryExtensionV2Ext = new AccountEntryExtensionV2Ext();
            var discriminant = stream.ReadInt();
            decodedAccountEntryExtensionV2Ext.Discriminant = discriminant;
            switch (decodedAccountEntryExtensionV2Ext.Discriminant)
            {
                case 0:
                    break;
                case 3:
                    decodedAccountEntryExtensionV2Ext.V3 = AccountEntryExtensionV3.Decode(stream);
                    break;
            }

            return decodedAccountEntryExtensionV2Ext;
        }
    }
}