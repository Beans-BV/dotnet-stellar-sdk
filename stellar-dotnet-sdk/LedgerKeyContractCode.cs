using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerKeyContractCode : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyContractCode</c> object from a base-64 encoded string of the hash of the ledger entry.
    ///     Use this to fetch contract wasm byte-code.
    /// </summary>
    /// <param name="base64String">A base-64 encoded string.</param>
    public LedgerKeyContractCode(string base64String) : this(Convert.FromBase64String(base64String))
    {
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyContractCode</c> object from the byte array of the hash of the ledger entry.
    /// </summary>
    /// <param name="hash">a 32-element byte array.</param>
    public LedgerKeyContractCode(byte[] hash)
    {
        if (hash.Length != 32) throw new ArgumentOutOfRangeException(nameof(hash), "Hash must have exactly 32 bytes.");
        Hash = hash;
    }

    public byte[] Hash { get; }

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE },
            ContractCode = new xdr.LedgerKey.LedgerKeyContractCode
            {
                Hash = new Hash(Hash)
            }
        };
    }

    public static LedgerKeyContractCode FromXdr(xdr.LedgerKey.LedgerKeyContractCode xdr)
    {
        return new LedgerKeyContractCode(Convert.ToBase64String(xdr.Hash.InnerValue));
    }
}