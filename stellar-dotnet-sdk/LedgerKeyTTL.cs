using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerKeyTTL : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyTTL</c> object from a base-64 encoded string of the hash of the ledger entry.
    /// </summary>
    /// <param name="base64String">A base-64 encoded string.</param>
    public LedgerKeyTTL(string base64String) : this(Convert.FromBase64String(base64String))
    {
    }

    private LedgerKeyTTL(byte[] key)
    {
        if (key.Length != 32) throw new ArgumentOutOfRangeException(nameof(key), "Key must have exactly 32 bytes.");
        Key = key;
    }

    public byte[] Key { get; }

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.TTL },
            Ttl = new xdr.LedgerKey.LedgerKeyTtl
            {
                KeyHash = new xdr.Hash(Key)
            }
        };
    }

    public static LedgerKeyTTL FromXdr(xdr.LedgerKey.LedgerKeyTtl xdr)
    {
        return new LedgerKeyTTL(Convert.ToBase64String(xdr.KeyHash.InnerValue));
    }
}