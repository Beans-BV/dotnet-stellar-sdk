// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  enum LedgerHeaderFlags
//  {
//      DISABLE_LIQUIDITY_POOL_TRADING_FLAG = 0x1,
//      DISABLE_LIQUIDITY_POOL_DEPOSIT_FLAG = 0x2,
//      DISABLE_LIQUIDITY_POOL_WITHDRAWAL_FLAG = 0x4
//  };

//  ===========================================================================
public class LedgerHeaderFlags
{
    public enum LedgerHeaderFlagsEnum
    {
        DISABLE_LIQUIDITY_POOL_TRADING_FLAG = 1,
        DISABLE_LIQUIDITY_POOL_DEPOSIT_FLAG = 2,
        DISABLE_LIQUIDITY_POOL_WITHDRAWAL_FLAG = 4
    }

    public LedgerHeaderFlagsEnum InnerValue { get; set; }

    public static LedgerHeaderFlags Create(LedgerHeaderFlagsEnum v)
    {
        return new LedgerHeaderFlags
        {
            InnerValue = v
        };
    }

    public static LedgerHeaderFlags Decode(XdrDataInputStream stream)
    {
        var value = stream.ReadInt();
        switch (value)
        {
            case 1: return Create(LedgerHeaderFlagsEnum.DISABLE_LIQUIDITY_POOL_TRADING_FLAG);
            case 2: return Create(LedgerHeaderFlagsEnum.DISABLE_LIQUIDITY_POOL_DEPOSIT_FLAG);
            case 4: return Create(LedgerHeaderFlagsEnum.DISABLE_LIQUIDITY_POOL_WITHDRAWAL_FLAG);
            default:
                throw new Exception("Unknown enum value: " + value);
        }
    }

    public static void Encode(XdrDataOutputStream stream, LedgerHeaderFlags value)
    {
        stream.WriteInt((int)value.InnerValue);
    }
}