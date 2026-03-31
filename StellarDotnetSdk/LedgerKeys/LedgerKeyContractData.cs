using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using SCVal = StellarDotnetSdk.Soroban.SCVal;

namespace StellarDotnetSdk.LedgerKeys;

/// <summary>
///     Represents a ledger key for a Soroban smart contract data entry.
///     Used to look up contract storage data from the ledger, identified by
///     the contract address, data key, and durability (persistent or temporary).
/// </summary>
public class LedgerKeyContractData : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyContractData</c> from a contract address, data key, and durability.
    /// </summary>
    /// <param name="contractId">The address of the smart contract.</param>
    /// <param name="key">The key of the data entry within the contract's storage.</param>
    /// <param name="durability">The durability type (persistent or temporary) of the data entry.</param>
    public LedgerKeyContractData(ScAddress contractId, SCVal key, ContractDataDurability durability)
    {
        Contract = contractId;
        Key = key;
        Durability = durability;
    }

    /// <summary>
    ///     The address of the smart contract that owns the data entry.
    /// </summary>
    public ScAddress Contract { get; }

    /// <summary>
    ///     The key of the data entry within the contract's storage.
    /// </summary>
    public SCVal Key { get; }

    /// <summary>
    ///     The durability type (persistent or temporary) of the data entry.
    /// </summary>
    public ContractDataDurability Durability { get; }

    /// <summary>
    ///     Serializes this ledger key to its XDR representation.
    /// </summary>
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
                Durability = Durability,
            },
        };
    }

    /// <summary>
    ///     Deserializes a <see cref="LedgerKeyContractData" /> from its XDR representation.
    /// </summary>
    /// <param name="xdr">The XDR ledger key contract data object.</param>
    public static LedgerKeyContractData FromXdr(Xdr.LedgerKey.LedgerKeyContractData xdr)
    {
        return new LedgerKeyContractData(ScAddress.FromXdr(xdr.Contract), SCVal.FromXdr(xdr.Key),
            xdr.Durability);
    }
}