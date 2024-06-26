// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  typedef PublicKey AccountID;

//  ===========================================================================
public class AccountID
{
    public AccountID()
    {
    }

    public AccountID(PublicKey value)
    {
        InnerValue = value;
    }

    public PublicKey InnerValue { get; set; }

    public static void Encode(XdrDataOutputStream stream, AccountID encodedAccountID)
    {
        PublicKey.Encode(stream, encodedAccountID.InnerValue);
    }

    public static AccountID Decode(XdrDataInputStream stream)
    {
        var decodedAccountID = new AccountID();
        decodedAccountID.InnerValue = PublicKey.Decode(stream);
        return decodedAccountID;
    }
}