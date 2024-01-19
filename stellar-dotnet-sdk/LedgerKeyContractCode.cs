using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerKeyContractCode : LedgerKey
{
    public LedgerKeyContractCode(string base64String) : this(new Hash(base64String))
    {
    }
    
    public LedgerKeyContractCode(Hash hash)
    {
        Hash = hash;
    }

    public Hash Hash { get; set; }

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE },
            ContractCode = new xdr.LedgerKey.LedgerKeyContractCode
            {
                Hash = Hash.ToXdr()
            }
        };
    }

    public static LedgerKeyContractCode FromXdr(xdr.LedgerKey.LedgerKeyContractCode xdr)
    {
        return new LedgerKeyContractCode(Convert.ToBase64String(xdr.Hash.InnerValue));
    }
}