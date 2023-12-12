// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace stellar_dotnet_sdk.xdr;

// === xdr source ============================================================

//  enum SorobanAuthorizedFunctionType
//  {
//      SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN = 0,
//      SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_HOST_FN = 1
//  };

//  ===========================================================================
public class SorobanAuthorizedFunctionType
{
    public enum SorobanAuthorizedFunctionTypeEnum
    {
        SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN = 0,
        SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_HOST_FN = 1
    }

    public SorobanAuthorizedFunctionTypeEnum InnerValue { get; set; } = default;

    public static SorobanAuthorizedFunctionType Create(SorobanAuthorizedFunctionTypeEnum v)
    {
        return new SorobanAuthorizedFunctionType
        {
            InnerValue = v
        };
    }

    public static SorobanAuthorizedFunctionType Decode(XdrDataInputStream stream)
    {
        var value = stream.ReadInt();
        switch (value)
        {
            case 0: return Create(SorobanAuthorizedFunctionTypeEnum.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN);
            case 1:
                return Create(
                    SorobanAuthorizedFunctionTypeEnum.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_HOST_FN);
            default:
                throw new Exception("Unknown enum value: " + value);
        }
    }

    public static void Encode(XdrDataOutputStream stream, SorobanAuthorizedFunctionType value)
    {
        stream.WriteInt((int)value.InnerValue);
    }
}