// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct ClaimableBalanceEntry
//  {
//      // Unique identifier for this ClaimableBalanceEntry
//      ClaimableBalanceID balanceID;
//  
//      // List of claimants with associated predicate
//      Claimant claimants<10>;
//  
//      // Any asset including native
//      Asset asset;
//  
//      // Amount of asset
//      int64 amount;
//  
//      // reserved for future use
//      union switch (int v)
//      {
//      case 0:
//          void;
//      case 1:
//          ClaimableBalanceEntryExtensionV1 v1;
//      }
//      ext;
//  };

//  ===========================================================================
public class ClaimableBalanceEntry
{
    public ClaimableBalanceID BalanceID { get; set; }
    public Claimant[] Claimants { get; set; }
    public Asset Asset { get; set; }
    public Int64 Amount { get; set; }
    public ClaimableBalanceEntryExt Ext { get; set; }

    public static void Encode(XdrDataOutputStream stream, ClaimableBalanceEntry encodedClaimableBalanceEntry)
    {
        ClaimableBalanceID.Encode(stream, encodedClaimableBalanceEntry.BalanceID);
        var claimantssize = encodedClaimableBalanceEntry.Claimants.Length;
        stream.WriteInt(claimantssize);
        for (var i = 0; i < claimantssize; i++) Claimant.Encode(stream, encodedClaimableBalanceEntry.Claimants[i]);
        Asset.Encode(stream, encodedClaimableBalanceEntry.Asset);
        Int64.Encode(stream, encodedClaimableBalanceEntry.Amount);
        ClaimableBalanceEntryExt.Encode(stream, encodedClaimableBalanceEntry.Ext);
    }

    public static ClaimableBalanceEntry Decode(XdrDataInputStream stream)
    {
        var decodedClaimableBalanceEntry = new ClaimableBalanceEntry();
        decodedClaimableBalanceEntry.BalanceID = ClaimableBalanceID.Decode(stream);
        var claimantssize = stream.ReadInt();
        decodedClaimableBalanceEntry.Claimants = new Claimant[claimantssize];
        for (var i = 0; i < claimantssize; i++) decodedClaimableBalanceEntry.Claimants[i] = Claimant.Decode(stream);
        decodedClaimableBalanceEntry.Asset = Asset.Decode(stream);
        decodedClaimableBalanceEntry.Amount = Int64.Decode(stream);
        decodedClaimableBalanceEntry.Ext = ClaimableBalanceEntryExt.Decode(stream);
        return decodedClaimableBalanceEntry;
    }

    public class ClaimableBalanceEntryExt
    {
        public int Discriminant { get; set; }

        public ClaimableBalanceEntryExtensionV1 V1 { get; set; }

        public static void Encode(XdrDataOutputStream stream, ClaimableBalanceEntryExt encodedClaimableBalanceEntryExt)
        {
            stream.WriteInt(encodedClaimableBalanceEntryExt.Discriminant);
            switch (encodedClaimableBalanceEntryExt.Discriminant)
            {
                case 0:
                    break;
                case 1:
                    ClaimableBalanceEntryExtensionV1.Encode(stream, encodedClaimableBalanceEntryExt.V1);
                    break;
            }
        }

        public static ClaimableBalanceEntryExt Decode(XdrDataInputStream stream)
        {
            var decodedClaimableBalanceEntryExt = new ClaimableBalanceEntryExt();
            var discriminant = stream.ReadInt();
            decodedClaimableBalanceEntryExt.Discriminant = discriminant;
            switch (decodedClaimableBalanceEntryExt.Discriminant)
            {
                case 0:
                    break;
                case 1:
                    decodedClaimableBalanceEntryExt.V1 = ClaimableBalanceEntryExtensionV1.Decode(stream);
                    break;
            }

            return decodedClaimableBalanceEntryExt;
        }
    }
}