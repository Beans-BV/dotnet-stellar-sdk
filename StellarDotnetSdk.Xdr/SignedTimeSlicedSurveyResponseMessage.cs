// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct SignedTimeSlicedSurveyResponseMessage
//  {
//      Signature responseSignature;
//      TimeSlicedSurveyResponseMessage response;
//  };

//  ===========================================================================
public class SignedTimeSlicedSurveyResponseMessage
{
    public Signature ResponseSignature { get; set; }
    public TimeSlicedSurveyResponseMessage Response { get; set; }

    public static void Encode(XdrDataOutputStream stream,
        SignedTimeSlicedSurveyResponseMessage encodedSignedTimeSlicedSurveyResponseMessage)
    {
        Signature.Encode(stream, encodedSignedTimeSlicedSurveyResponseMessage.ResponseSignature);
        TimeSlicedSurveyResponseMessage.Encode(stream, encodedSignedTimeSlicedSurveyResponseMessage.Response);
    }

    public static SignedTimeSlicedSurveyResponseMessage Decode(XdrDataInputStream stream)
    {
        var decodedSignedTimeSlicedSurveyResponseMessage = new SignedTimeSlicedSurveyResponseMessage();
        decodedSignedTimeSlicedSurveyResponseMessage.ResponseSignature = Signature.Decode(stream);
        decodedSignedTimeSlicedSurveyResponseMessage.Response = TimeSlicedSurveyResponseMessage.Decode(stream);
        return decodedSignedTimeSlicedSurveyResponseMessage;
    }
}