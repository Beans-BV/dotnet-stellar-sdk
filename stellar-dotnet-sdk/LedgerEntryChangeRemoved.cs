using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryChangeRemoved : LedgerEntryChange
{
    public LedgerKey Removed { get; set; }

    public xdr.LedgerEntryChange ToXdrLedgerEntryChange()
    {
        return new xdr.LedgerEntryChange
        {
            Discriminant =
                LedgerEntryChangeType.Create(LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_REMOVED),
            Removed = Removed.ToXdr()
        };
    }

    public static LedgerEntryChange FromXdrLedgerEntryChange(xdr.LedgerEntryChange xdrChange)
    {
        return new LedgerEntryChangeRemoved
        {
            Removed = LedgerKey.FromXdr(xdrChange.Removed)
        };
    }
}