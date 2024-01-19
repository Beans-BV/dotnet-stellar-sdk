using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryLiquidityPool : LedgerEntry
{
    public LiquidityPoolID LiquidityPoolID { get; set; }
    public LiquidityPoolEntryBody LiquidityPoolBody { get; set; }

    public static LedgerEntryLiquidityPool FromXdrLedgerEntry(xdr.LedgerEntry xdrLedgerEntry)
    {
        if (xdrLedgerEntry.Data.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL)
            throw new ArgumentException("Not a LiquidityPoolEntry", nameof(xdrLedgerEntry));
        var xdrLiquidityPoolEntry = xdrLedgerEntry.Data.LiquidityPool;

        var ledgerEntryLiquidityPool = new LedgerEntryLiquidityPool
        {
            LiquidityPoolID = LiquidityPoolID.FromXdr(xdrLiquidityPoolEntry.LiquidityPoolID),
            LiquidityPoolBody = LiquidityPoolEntryBody.FromXdr(xdrLiquidityPoolEntry.Body)
        };
        ExtraFieldsFromXdr(xdrLedgerEntry, ledgerEntryLiquidityPool);
        return ledgerEntryLiquidityPool;
    }

    public LiquidityPoolEntry ToXdr()
    {
        return new LiquidityPoolEntry
        {
            Body = LiquidityPoolBody.ToXdr(),
            LiquidityPoolID = LiquidityPoolID.ToXdr()
        };
    }
}