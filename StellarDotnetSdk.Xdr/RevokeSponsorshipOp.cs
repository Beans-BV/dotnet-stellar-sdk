// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  union RevokeSponsorshipOp switch (RevokeSponsorshipType type)
//  {
//  case REVOKE_SPONSORSHIP_LEDGER_ENTRY:
//      LedgerKey ledgerKey;
//  case REVOKE_SPONSORSHIP_SIGNER:
//      struct
//      {
//          AccountID accountID;
//          SignerKey signerKey;
//      } signer;
//  };

//  ===========================================================================
public class RevokeSponsorshipOp
{
    public RevokeSponsorshipType Discriminant { get; set; } = new();

    public LedgerKey LedgerKey { get; set; }
    public RevokeSponsorshipOpSigner Signer { get; set; }

    public static void Encode(XdrDataOutputStream stream, RevokeSponsorshipOp encodedRevokeSponsorshipOp)
    {
        stream.WriteInt((int)encodedRevokeSponsorshipOp.Discriminant.InnerValue);
        switch (encodedRevokeSponsorshipOp.Discriminant.InnerValue)
        {
            case RevokeSponsorshipType.RevokeSponsorshipTypeEnum.REVOKE_SPONSORSHIP_LEDGER_ENTRY:
                LedgerKey.Encode(stream, encodedRevokeSponsorshipOp.LedgerKey);
                break;
            case RevokeSponsorshipType.RevokeSponsorshipTypeEnum.REVOKE_SPONSORSHIP_SIGNER:
                RevokeSponsorshipOpSigner.Encode(stream, encodedRevokeSponsorshipOp.Signer);
                break;
        }
    }

    public static RevokeSponsorshipOp Decode(XdrDataInputStream stream)
    {
        var decodedRevokeSponsorshipOp = new RevokeSponsorshipOp();
        var discriminant = RevokeSponsorshipType.Decode(stream);
        decodedRevokeSponsorshipOp.Discriminant = discriminant;
        switch (decodedRevokeSponsorshipOp.Discriminant.InnerValue)
        {
            case RevokeSponsorshipType.RevokeSponsorshipTypeEnum.REVOKE_SPONSORSHIP_LEDGER_ENTRY:
                decodedRevokeSponsorshipOp.LedgerKey = LedgerKey.Decode(stream);
                break;
            case RevokeSponsorshipType.RevokeSponsorshipTypeEnum.REVOKE_SPONSORSHIP_SIGNER:
                decodedRevokeSponsorshipOp.Signer = RevokeSponsorshipOpSigner.Decode(stream);
                break;
        }

        return decodedRevokeSponsorshipOp;
    }

    public class RevokeSponsorshipOpSigner
    {
        public AccountID AccountID { get; set; }
        public SignerKey SignerKey { get; set; }

        public static void Encode(XdrDataOutputStream stream,
            RevokeSponsorshipOpSigner encodedRevokeSponsorshipOpSigner)
        {
            AccountID.Encode(stream, encodedRevokeSponsorshipOpSigner.AccountID);
            SignerKey.Encode(stream, encodedRevokeSponsorshipOpSigner.SignerKey);
        }

        public static RevokeSponsorshipOpSigner Decode(XdrDataInputStream stream)
        {
            var decodedRevokeSponsorshipOpSigner = new RevokeSponsorshipOpSigner();
            decodedRevokeSponsorshipOpSigner.AccountID = AccountID.Decode(stream);
            decodedRevokeSponsorshipOpSigner.SignerKey = SignerKey.Decode(stream);
            return decodedRevokeSponsorshipOpSigner;
        }
    }
}