using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyContractData : LedgerKey
{
    public LedgerKeyContractData(SCAddress contractId, SCVal key, ContractDataDurability durability)
    {
        Contract = contractId;
        Key = key;
        Durability = durability;
    }

    public SCAddress Contract { get; }
    public SCVal Key { get; }
    public ContractDataDurability Durability { get; }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA },
            ContractData = new Xdr.LedgerKey.LedgerKeyContractData
            {
                Contract = Contract.ToXdr(),
                Key = Key.ToXdr(),
                Durability = Durability
            }
        };
    }

    public static LedgerKeyContractData FromXdr(Xdr.LedgerKey.LedgerKeyContractData xdr)
    {
        return new LedgerKeyContractData(SCAddress.FromXdr(xdr.Contract), SCVal.FromXdr(xdr.Key),
            xdr.Durability);
    }
}