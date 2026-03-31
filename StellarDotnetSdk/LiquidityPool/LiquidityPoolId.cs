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
    /// <summary>
    ///     Constructs a <c>LiquidityPoolId</c> by computing the SHA-256 hash of the XDR-encoded pool parameters.
    /// </summary>
    /// <param name="type">The type of the liquidity pool.</param>
    /// <param name="assetA">The first asset in the pool (must be lexicographically less than assetB).</param>
    /// <param name="assetB">The second asset in the pool.</param>
    /// <param name="fee">The fee charged per trade in basis points.</param>
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

    /// <summary>
    ///     Constructs a <c>LiquidityPoolId</c> from a hex-encoded pool ID string.
    /// </summary>
    /// <param name="hex">A hex-encoded string representing the 32-byte pool ID hash.</param>
    [JsonConstructor]
    public LiquidityPoolId(string hex)
    {
        Hash = Util.HexToBytes(hex);
    }

    /// <summary>
    ///     Constructs a <c>LiquidityPoolId</c> from a raw 32-byte hash.
    /// </summary>
    /// <param name="hash">The 32-byte pool ID hash.</param>
    public LiquidityPoolId(byte[] hash)
    {
        Hash = hash;
    }

    /// <summary>
    ///     The 32-byte SHA-256 hash that uniquely identifies the liquidity pool.
    /// </summary>
    public byte[] Hash { get; }

    /// <summary>
    ///     Deserializes a <see cref="LiquidityPoolId" /> from its XDR <see cref="PoolID" /> representation.
    /// </summary>
    /// <param name="poolIdXdr">The XDR pool ID object.</param>
    public static LiquidityPoolId FromXdr(PoolID poolIdXdr)
    {
        return new LiquidityPoolId(poolIdXdr.InnerValue.InnerValue);
    }

    /// <summary>
    ///     Returns the lowercase hex string representation of the pool ID hash.
    /// </summary>
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

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is LiquidityPoolId other && Equals(ToString(), other.ToString());
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}