using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LiquidityPool;

/// <summary>
///     Represents the unique identifier of a Stellar liquidity pool, derived from
///     the SHA-256 hash of the pool's parameters (type, assets, and fee).
/// </summary>
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

    /// <summary>
    ///     Converts this liquidity pool ID to its XDR <see cref="PoolID" /> representation.
    /// </summary>
    /// <returns>A <see cref="PoolID" /> XDR object containing the pool hash.</returns>
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