using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerKeyTTL : LedgerKey
{
    public LedgerKeyTTL(string hexString) : this(new Hash(hexString))
    {
    }

    private LedgerKeyTTL(Hash hash)
    {
        Key = hash;
    }
    
    public Hash Key { get; }

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.TTL },
            Ttl = new xdr.LedgerKey.LedgerKeyTtl
            {
                KeyHash = Key.ToXdr()
            }
        };
    }

    public static LedgerKeyTTL FromXdr(xdr.LedgerKey.LedgerKeyTtl xdr)
    {
        return new LedgerKeyTTL(Convert.ToBase64String(xdr.KeyHash.InnerValue));
    }
}