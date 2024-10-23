using System;
using Newtonsoft.Json;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LiquidityPool;

[JsonConverter(typeof(LiquidityPoolIdJsonConverter))]
public class LiquidityPoolID
{
    public LiquidityPoolID(LiquidityPoolType.LiquidityPoolTypeEnum type, Asset assetA, Asset assetB, int fee)
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
    public LiquidityPoolID(string hex)
    {
        Hash = Util.HexToBytes(hex);
    }

    public LiquidityPoolID(byte[] hash)
    {
        Hash = hash;
    }

    public byte[] Hash { get; }

    public static LiquidityPoolID FromXdr(PoolID poolIdXdr)
    {
        return new LiquidityPoolID(poolIdXdr.InnerValue.InnerValue);
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
        return obj is LiquidityPoolID other && Equals(ToString(), other.ToString());
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}