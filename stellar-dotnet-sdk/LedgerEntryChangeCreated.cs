using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryChangeCreated : LedgerEntryChange
{
    public xdr.LedgerEntryChange ToXdrLedgerEntryChange()
    {
        return new xdr.LedgerEntryChange
        {
            Discriminant =
                LedgerEntryChangeType.Create(LedgerEntryChangeType.LedgerEntryChangeTypeEnum.LEDGER_ENTRY_CREATED),
            Created = ChangedEntry.ToXdr()
        };
    }

    public static LedgerEntryChange FromXdrLedgerEntryChange(xdr.LedgerEntryChange xdrChange)
    {
        return new LedgerEntryChangeCreated
        {
            ChangedEntry = LedgerEntry.FromXdr(xdrChange.Created)
        };
    }
}