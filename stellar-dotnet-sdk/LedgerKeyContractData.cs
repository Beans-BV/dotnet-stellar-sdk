using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerKeyContractData : LedgerKey
{
    public LedgerKeyContractData(SCAddress contractId, SCVal key, ContractDataDurability durability)
    {
        Contract = contractId;
        Key = key;
        Durability = durability;
    }

    public SCAddress Contract { get; set; }
    public SCVal Key { get; set; }
    public ContractDataDurability Durability { get; set; }

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA },
            ContractData = new xdr.LedgerKey.LedgerKeyContractData
            {
                Contract = Contract.ToXdr(),
                Key = Key.ToXdr(),
                Durability = Durability
            }
        };
    }

    public static LedgerKeyContractData FromXdr(xdr.LedgerKey.LedgerKeyContractData xdr)
    {
        return new LedgerKeyContractData(SCAddress.FromXdr(xdr.Contract), SCAddress.FromSCValXdr(xdr.Key),
            xdr.Durability);
    }
}