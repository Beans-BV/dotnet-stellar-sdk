using System;
using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryClaimableBalance : LedgerEntry
{
    private LedgerEntryClaimableBalance(Hash balanceId, Claimant[] claimants, Asset asset, long amount)
    {
        BalanceId = balanceId;
        Claimants = claimants;
        Asset = asset;
        Amount = amount;
    }

    public Hash BalanceId { get; }
    public Claimant[] Claimants { get; }
    public Asset Asset { get; }
    public long Amount { get; }
    public ClaimableBalanceEntryExtensionV1? ClaimableBalanceEntryExtensionV1 { get; private set; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryClaimableBalance object from a <see cref="xdr.LedgerEntry.LedgerEntryData" />
    ///     object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryClaimableBalance object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid ClaimableBalanceEntry.</exception>
    public static LedgerEntryClaimableBalance FromXdrLedgerEntryData(xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE)
            throw new ArgumentException("Not a ClaimableBalanceEntry", nameof(xdrLedgerEntryData));

        return FromXdr(xdrLedgerEntryData.ClaimableBalance);
    }

    private static LedgerEntryClaimableBalance FromXdr(ClaimableBalanceEntry xdrClaimableBalanceEntry)
    {
        var ledgerEntryClaimableBalance = new LedgerEntryClaimableBalance(
            Hash.FromXdr(xdrClaimableBalanceEntry.BalanceID.V0),
            xdrClaimableBalanceEntry.Claimants.Select(Claimant.FromXdr).ToArray(),
            Asset.FromXdr(xdrClaimableBalanceEntry.Asset),
            xdrClaimableBalanceEntry.Amount.InnerValue);
        if (xdrClaimableBalanceEntry.Ext.Discriminant == 1)
            ledgerEntryClaimableBalance.ClaimableBalanceEntryExtensionV1 =
                ClaimableBalanceEntryExtensionV1.FromXdr(xdrClaimableBalanceEntry.Ext.V1);
        return ledgerEntryClaimableBalance;
    }
}