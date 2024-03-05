using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryLiquidityPool : LedgerEntry
{
    private LedgerEntryLiquidityPool(LiquidityPoolID liquidityPoolID, LiquidityPoolEntryBody liquidityPoolBody)
    {
        LiquidityPoolID = liquidityPoolID;
        LiquidityPoolBody = liquidityPoolBody;
    }

    public LiquidityPoolID LiquidityPoolID { get; init; }
    public LiquidityPoolEntryBody LiquidityPoolBody { get; init; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryLiquidityPool object from a <see cref="xdr.LedgerEntry.LedgerEntryData" />
    ///     object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryLiquidityPool object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid LiquidityPoolEntry.</exception>
    public static LedgerEntryLiquidityPool FromXdrLedgerEntryData(xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL)
            throw new ArgumentException("Not a LiquidityPoolEntry", nameof(xdrLedgerEntryData));

        return FromXdr(xdrLedgerEntryData);
    }

    private static LedgerEntryLiquidityPool FromXdr(xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        var xdrLiquidityPoolEntry = xdrLedgerEntryData.LiquidityPool;

        var ledgerEntryLiquidityPool = new LedgerEntryLiquidityPool(
            LiquidityPoolID.FromXdr(xdrLiquidityPoolEntry.LiquidityPoolID),
            LiquidityPoolEntryBody.FromXdr(xdrLiquidityPoolEntry.Body));
        return ledgerEntryLiquidityPool;
    }
}