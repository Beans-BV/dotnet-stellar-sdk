// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct SignedSurveyResponseMessage
//  {
//      Signature responseSignature;
//      SurveyResponseMessage response;
//  };

//  ===========================================================================
public class SignedSurveyResponseMessage
{
    public Signature ResponseSignature { get; set; }
    public SurveyResponseMessage Response { get; set; }

    public static void Encode(XdrDataOutputStream stream,
        SignedSurveyResponseMessage encodedSignedSurveyResponseMessage)
    {
        Signature.Encode(stream, encodedSignedSurveyResponseMessage.ResponseSignature);
        SurveyResponseMessage.Encode(stream, encodedSignedSurveyResponseMessage.Response);
    }

    public static SignedSurveyResponseMessage Decode(XdrDataInputStream stream)
    {
        var decodedSignedSurveyResponseMessage = new SignedSurveyResponseMessage();
        decodedSignedSurveyResponseMessage.ResponseSignature = Signature.Decode(stream);
        decodedSignedSurveyResponseMessage.Response = SurveyResponseMessage.Decode(stream);
        return decodedSignedSurveyResponseMessage;
    }
}