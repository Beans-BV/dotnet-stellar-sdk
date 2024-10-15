using System;
using Newtonsoft.Json;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LiquidityPool;

[JsonConverter(typeof(LiquidityPoolIdJsonConverter))]
public class LiquidityPoolId
{
    public LiquidityPoolId(LiquidityPoolType.LiquidityPoolTypeEnum type, Asset assetA, Asset assetB, int fee)
    {
        var xdrDataOutputStream = new XdrDataOutputStream();

        try
        {
            Xdr.LiquidityPoolParameters.Encode(xdrDataOutputStream,
                LiquidityPoolParameters.Create(type, assetA, assetB, fee).ToXdr());
        }
        catch (Exception e)
        {
            throw new ArgumentException("Invalid Liquidity Pool ID", e);
        }

        Hash = Util.Hash(xdrDataOutputStream.ToArray());
    }

    [JsonConstructor]
    public LiquidityPoolId(string hex)
    {
        Hash = Util.HexToBytes(hex);
    }

    public LiquidityPoolId(byte[] hash)
    {
        Hash = hash;
    }

    public byte[] Hash { get; }

    public static LiquidityPoolId FromXdr(PoolID poolIdXdr)
    {
        return new LiquidityPoolId(poolIdXdr.InnerValue.InnerValue);
    }

    public override string ToString()
    {
        return Util.BytesToHex(Hash).ToLowerInvariant();
    }

    public PoolID ToXdr()
    {
        return new PoolID(new Hash(Hash));
    }

    public override bool Equals(object? obj)
    {
        return obj is LiquidityPoolId other && Equals(ToString(), other.ToString());
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}