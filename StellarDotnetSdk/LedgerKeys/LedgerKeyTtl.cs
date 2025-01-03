﻿using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyTtl : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyTTL</c> object from a base-64 encoded string of the hash of the ledger entry.
    /// </summary>
    /// <param name="base64String">A base-64 encoded string.</param>
    public LedgerKeyTtl(string base64String) : this(Convert.FromBase64String(base64String))
    {
    }

    private LedgerKeyTtl(byte[] key)
    {
        if (key.Length != 32)
        {
            throw new ArgumentOutOfRangeException(nameof(key), "Key must have exactly 32 bytes.");
        }
        Key = key;
    }

    public byte[] Key { get; }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.TTL },
            Ttl = new Xdr.LedgerKey.LedgerKeyTtl
            {
                KeyHash = new Hash(Key),
            },
        };
    }

    public static LedgerKeyTtl FromXdr(Xdr.LedgerKey.LedgerKeyTtl xdr)
    {
        return new LedgerKeyTtl(Convert.ToBase64String(xdr.KeyHash.InnerValue));
    }
}