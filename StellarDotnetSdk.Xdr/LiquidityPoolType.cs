// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  enum LiquidityPoolType
//  {
//      LIQUIDITY_POOL_CONSTANT_PRODUCT = 0
//  };

//  ===========================================================================
public class LiquidityPoolType
{
    public enum LiquidityPoolTypeEnum
    {
        LIQUIDITY_POOL_CONSTANT_PRODUCT = 0,
    }

    public LiquidityPoolTypeEnum InnerValue { get; set; }

    public static LiquidityPoolType Create(LiquidityPoolTypeEnum v)
    {
        return new LiquidityPoolType
        {
            InnerValue = v,
        };
    }

    public static LiquidityPoolType Decode(XdrDataInputStream stream)
    {
        var value = stream.ReadInt();
        switch (value)
        {
            case 0: return Create(LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT);
            default:
                throw new Exception("Unknown enum value: " + value);
        }
    }

    public static void Encode(XdrDataOutputStream stream, LiquidityPoolType value)
    {
        stream.WriteInt((int)value.InnerValue);
    }
}