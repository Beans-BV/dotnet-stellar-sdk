// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  enum LedgerEntryType
//  {
//      ACCOUNT = 0,
//      TRUSTLINE = 1,
//      OFFER = 2,
//      DATA = 3,
//      CLAIMABLE_BALANCE = 4,
//      LIQUIDITY_POOL = 5,
//      CONTRACT_DATA = 6,
//      CONTRACT_CODE = 7,
//      CONFIG_SETTING = 8,
//      TTL = 9
//  };

//  ===========================================================================
public class LedgerEntryType
{
    public enum LedgerEntryTypeEnum
    {
        ACCOUNT = 0,
        TRUSTLINE = 1,
        OFFER = 2,
        DATA = 3,
        CLAIMABLE_BALANCE = 4,
        LIQUIDITY_POOL = 5,
        CONTRACT_DATA = 6,
        CONTRACT_CODE = 7,
        CONFIG_SETTING = 8,
        TTL = 9,
    }

    public LedgerEntryTypeEnum InnerValue { get; set; }

    public static LedgerEntryType Create(LedgerEntryTypeEnum v)
    {
        return new LedgerEntryType
        {
            InnerValue = v,
        };
    }

    public static LedgerEntryType Decode(XdrDataInputStream stream)
    {
        var value = stream.ReadInt();
        switch (value)
        {
            case 0: return Create(LedgerEntryTypeEnum.ACCOUNT);
            case 1: return Create(LedgerEntryTypeEnum.TRUSTLINE);
            case 2: return Create(LedgerEntryTypeEnum.OFFER);
            case 3: return Create(LedgerEntryTypeEnum.DATA);
            case 4: return Create(LedgerEntryTypeEnum.CLAIMABLE_BALANCE);
            case 5: return Create(LedgerEntryTypeEnum.LIQUIDITY_POOL);
            case 6: return Create(LedgerEntryTypeEnum.CONTRACT_DATA);
            case 7: return Create(LedgerEntryTypeEnum.CONTRACT_CODE);
            case 8: return Create(LedgerEntryTypeEnum.CONFIG_SETTING);
            case 9: return Create(LedgerEntryTypeEnum.TTL);
            default:
                throw new Exception("Unknown enum value: " + value);
        }
    }

    public static void Encode(XdrDataOutputStream stream, LedgerEntryType value)
    {
        stream.WriteInt((int)value.InnerValue);
    }
}