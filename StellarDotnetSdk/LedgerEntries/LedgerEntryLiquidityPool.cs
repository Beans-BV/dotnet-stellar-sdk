using System;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class LedgerEntryLiquidityPool : LedgerEntry
{
    private LedgerEntryLiquidityPool(LiquidityPoolId liquidityPoolId, LiquidityPoolEntryBody liquidityPoolBody)
    {
        LiquidityPoolId = liquidityPoolId;
        LiquidityPoolBody = liquidityPoolBody;
    }

    public LiquidityPoolId LiquidityPoolId { get; }
    public LiquidityPoolEntryBody LiquidityPoolBody { get; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryLiquidityPool object from a <see cref="Xdr.LedgerEntry.LedgerEntryData" />
    ///     object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryLiquidityPool object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid LiquidityPoolEntry.</exception>
    public static LedgerEntryLiquidityPool FromXdrLedgerEntryData(Xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL)
        {
            throw new ArgumentException("Not a LiquidityPoolEntry", nameof(xdrLedgerEntryData));
        }

        return FromXdr(xdrLedgerEntryData);
    }

    private static LedgerEntryLiquidityPool FromXdr(Xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        var xdrLiquidityPoolEntry = xdrLedgerEntryData.LiquidityPool;

        var ledgerEntryLiquidityPool = new LedgerEntryLiquidityPool(
            LiquidityPoolId.FromXdr(xdrLiquidityPoolEntry.LiquidityPoolID),
            LiquidityPoolEntryBody.FromXdr(xdrLiquidityPoolEntry.Body));

        return ledgerEntryLiquidityPool;
    }
}