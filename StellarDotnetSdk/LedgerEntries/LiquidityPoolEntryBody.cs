using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Abstract base class for liquidity pool entry bodies, representing the type-specific data of a liquidity pool.
/// </summary>
public abstract class LiquidityPoolEntryBody
{
    /// <summary>
    ///     Creates the corresponding <see cref="LiquidityPoolEntryBody" /> subclass from an XDR
    ///     <see cref="LiquidityPoolEntry.LiquidityPoolEntryBody" /> object.
    /// </summary>
    /// <param name="xdrBody">The XDR liquidity pool entry body.</param>
    /// <returns>A <see cref="LiquidityPoolEntryBody" /> subclass instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown for unknown liquidity pool types.</exception>
    public static LiquidityPoolEntryBody FromXdr(LiquidityPoolEntry.LiquidityPoolEntryBody xdrBody)
    {
        return xdrBody.Discriminant.InnerValue switch
        {
            LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT => LiquidityPoolConstantProduct
                .FromXdrLiquidityPoolEntryBody(xdrBody),
            _ => throw new InvalidOperationException("Unknown liquidity pool type"),
        };
    }
}