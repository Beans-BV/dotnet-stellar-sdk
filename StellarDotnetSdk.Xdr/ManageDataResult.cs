// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  union ManageDataResult switch (ManageDataResultCode code)
//  {
//  case MANAGE_DATA_SUCCESS:
//      void;
//  case MANAGE_DATA_NOT_SUPPORTED_YET:
//  case MANAGE_DATA_NAME_NOT_FOUND:
//  case MANAGE_DATA_LOW_RESERVE:
//  case MANAGE_DATA_INVALID_NAME:
//      void;
//  };

//  ===========================================================================
public class ManageDataResult
{
    public ManageDataResultCode Discriminant { get; set; } = new();

    public static void Encode(XdrDataOutputStream stream, ManageDataResult encodedManageDataResult)
    {
        stream.WriteInt((int)encodedManageDataResult.Discriminant.InnerValue);
        switch (encodedManageDataResult.Discriminant.InnerValue)
        {
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_SUCCESS:
                break;
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_NOT_SUPPORTED_YET:
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_NAME_NOT_FOUND:
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_LOW_RESERVE:
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_INVALID_NAME:
                break;
        }
    }

    public static ManageDataResult Decode(XdrDataInputStream stream)
    {
        var decodedManageDataResult = new ManageDataResult();
        var discriminant = ManageDataResultCode.Decode(stream);
        decodedManageDataResult.Discriminant = discriminant;
        switch (decodedManageDataResult.Discriminant.InnerValue)
        {
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_SUCCESS:
                break;
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_NOT_SUPPORTED_YET:
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_NAME_NOT_FOUND:
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_LOW_RESERVE:
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_INVALID_NAME:
                break;
        }

        return decodedManageDataResult;
    }
}