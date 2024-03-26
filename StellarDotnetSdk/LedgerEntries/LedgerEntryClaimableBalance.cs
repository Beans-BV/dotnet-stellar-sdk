using System;
using System.Linq;
using StellarDotnetSdk.Xdr;
using Assets_Asset = StellarDotnetSdk.Assets.Asset;
using claimant_Claimant = StellarDotnetSdk.Claimant.Claimant;

namespace StellarDotnetSdk.LedgerEntries;

public class LedgerEntryClaimableBalance : LedgerEntry
{
    private LedgerEntryClaimableBalance(byte[] balanceId, claimant_Claimant[] claimants, Assets_Asset asset,
        long amount)
    {
        BalanceId = balanceId;
        Claimants = claimants;
        Asset = asset;
        Amount = amount;
    }

    public byte[] BalanceId { get; }
    public claimant_Claimant[] Claimants { get; }
    public Assets_Asset Asset { get; }
    public long Amount { get; }
    public ClaimableBalanceEntryExtensionV1? ClaimableBalanceEntryExtensionV1 { get; private set; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryClaimableBalance object from a <see cref="Xdr.LedgerEntry.LedgerEntryData" />
    ///     object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryClaimableBalance object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid ClaimableBalanceEntry.</exception>
    public static LedgerEntryClaimableBalance FromXdrLedgerEntryData(Xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE)
            throw new ArgumentException("Not a ClaimableBalanceEntry", nameof(xdrLedgerEntryData));

        return FromXdr(xdrLedgerEntryData.ClaimableBalance);
    }

    private static LedgerEntryClaimableBalance FromXdr(ClaimableBalanceEntry xdrClaimableBalanceEntry)
    {
        var ledgerEntryClaimableBalance = new LedgerEntryClaimableBalance(
            xdrClaimableBalanceEntry.BalanceID.V0.InnerValue,
            xdrClaimableBalanceEntry.Claimants.Select(claimant_Claimant.FromXdr).ToArray(),
            Assets_Asset.FromXdr(xdrClaimableBalanceEntry.Asset),
            xdrClaimableBalanceEntry.Amount.InnerValue);

        if (xdrClaimableBalanceEntry.Ext.Discriminant == 1)
            ledgerEntryClaimableBalance.ClaimableBalanceEntryExtensionV1 =
                ClaimableBalanceEntryExtensionV1.FromXdr(xdrClaimableBalanceEntry.Ext.V1);

        return ledgerEntryClaimableBalance;
    }
}