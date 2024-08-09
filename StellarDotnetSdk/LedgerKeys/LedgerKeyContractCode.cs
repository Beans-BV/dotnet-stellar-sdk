using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyContractCode : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyContractCode</c> object from a hex encoded string of the hash of the ledger entry.
    ///     Use this to fetch contract wasm byte-code.
    /// </summary>
    /// <param name="wasmHash">A hex-encoded string of the Wasm bytes of a compiled smart contract.</param>
    public LedgerKeyContractCode(string wasmHash) : this(Convert.FromHexString(wasmHash))
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

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE },
            ContractCode = new Xdr.LedgerKey.LedgerKeyContractCode
            {
                Hash = new Hash(Hash)
            }
        };
    }

    public static LedgerKeyContractCode FromXdr(Xdr.LedgerKey.LedgerKeyContractCode xdr)
    {
        return new LedgerKeyContractCode(Convert.ToHexString(xdr.Hash.InnerValue));
    }
}