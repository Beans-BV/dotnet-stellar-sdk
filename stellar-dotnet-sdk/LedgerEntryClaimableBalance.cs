using System;
using System.Linq;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

public class LedgerEntryClaimableBalance : LedgerEntry
{
    public Hash BalanceId { get; set; } 
    public Claimant[] Claimants { get; set; }
    public Asset Asset { get; set; }
    public long Amount { get; set; }
    public ClaimableBalanceEntryExtensionV1? ClaimableBalanceEntryExtensionV1 { get; set; }

    public static LedgerEntryClaimableBalance FromXdrLedgerEntry(xdr.LedgerEntry xdrLedgerEntry)
    {
        if (xdrLedgerEntry.Data.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE)
            throw new ArgumentException("Not a ClaimableBalanceEntry", nameof(xdrLedgerEntry));

        var xdrClaimableBalanceEntry = xdrLedgerEntry.Data.ClaimableBalance;
        var ledgerEntryClaimableBalance = new LedgerEntryClaimableBalance
        {
            BalanceId = Hash.FromXdr(xdrClaimableBalanceEntry.BalanceID.V0),
            Claimants = xdrClaimableBalanceEntry.Claimants.Select(Claimant.FromXdr).ToArray(),
            Asset = Asset.FromXdr(xdrClaimableBalanceEntry.Asset),
            Amount = xdrClaimableBalanceEntry.Amount.InnerValue
        };
        ExtraFieldsFromXdr(xdrLedgerEntry, ledgerEntryClaimableBalance);
        if (xdrClaimableBalanceEntry.Ext.Discriminant == 1)
            ledgerEntryClaimableBalance.ClaimableBalanceEntryExtensionV1 =
                ClaimableBalanceEntryExtensionV1.FromXdr(xdrClaimableBalanceEntry.Ext.V1);

        return ledgerEntryClaimableBalance;
    }

    public ClaimableBalanceEntry ToXdr()
    {
        return new ClaimableBalanceEntry
        {
            BalanceID = new ClaimableBalanceID
            {
                Discriminant = ClaimableBalanceIDType.Create(ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum
                    .CLAIMABLE_BALANCE_ID_TYPE_V0),
                V0 = BalanceId.ToXdr()
            },
            Claimants = Claimants.Select(x => x.ToXdr()).ToArray(),
            Asset = Asset.ToXdr(),
            Amount = new Int64(Amount),
            Ext = new ClaimableBalanceEntry.ClaimableBalanceEntryExt
            {
                Discriminant = ClaimableBalanceEntryExtensionV1 != null
                    ? 1
                    : 0,
                V1 = ClaimableBalanceEntryExtensionV1?.ToXdr() ?? new xdr.ClaimableBalanceEntryExtensionV1()
            }
        };
    }
}