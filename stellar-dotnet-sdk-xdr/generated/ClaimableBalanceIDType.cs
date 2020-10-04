// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace stellar_dotnet_sdk.xdr
{
// === xdr source ============================================================
//  enum ClaimableBalanceIDType
//  {
//      CLAIMABLE_BALANCE_ID_TYPE_V0 = 0
//  };
//  ===========================================================================
    public class ClaimableBalanceIDType
    {
        public enum ClaimableBalanceIDTypeEnum
        {
            CLAIMABLE_BALANCE_ID_TYPE_V0 = 0,
        }

        public ClaimableBalanceIDTypeEnum InnerValue { get; set; } = default(ClaimableBalanceIDTypeEnum);

        public static ClaimableBalanceIDType Create(ClaimableBalanceIDTypeEnum v)
        {
            return new ClaimableBalanceIDType
            {
                InnerValue = v
            };
        }

        public static ClaimableBalanceIDType Decode(XdrDataInputStream stream)
        {
            int value = stream.ReadInt();
            switch (value)
            {
                case 0: return Create(ClaimableBalanceIDTypeEnum.CLAIMABLE_BALANCE_ID_TYPE_V0);
                default:
                    throw new Exception("Unknown enum value: " + value);
            }
        }

        public static void Encode(XdrDataOutputStream stream, ClaimableBalanceIDType value)
        {
            stream.WriteInt((int) value.InnerValue);
        }
    }
}