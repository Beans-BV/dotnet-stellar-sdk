using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Accounts;

public class MuxedAccountMed25519 : IAccountId
{
    /// <summary>
    ///     Create a new MuxedAccountMed25519 with the given key and id.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="id"></param>
    public MuxedAccountMed25519(KeyPair key, ulong id)
    {
        Id = id;
        Key = key ?? throw new ArgumentNullException(nameof(key));
    }

    public ulong Id { get; }
    public KeyPair Key { get; }
    public byte[] PublicKey => Key.PublicKey;

    /// <summary>
    ///     Get the xdr MuxedAccount.
    /// </summary>
    public Xdr.MuxedAccount MuxedAccount
    {
        get
        {
            var muxedAccount = new Xdr.MuxedAccount
            {
                Discriminant =
                    new CryptoKeyType { InnerValue = CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_MUXED_ED25519 },
                Med25519 = new Xdr.MuxedAccount.MuxedAccountMed25519
                {
                    Id = new Uint64(Id),
                    Ed25519 = new Uint256(Key.PublicKey),
                },
            };

            return muxedAccount;
        }
    }

    /// <summary>
    ///     Get the MuxedAccount address, starting with M.
    /// </summary>
    public string Address => StrKey.EncodeStellarMuxedAccount(MuxedAccount);

    /// <summary>
    ///     Get the MuxedAccount account id, starting with M.
    /// </summary>
    public string AccountId => Address;

    /// <summary>
    ///     Return the signing key for the muxed account.
    /// </summary>
    public KeyPair SigningKey => Key;

    public bool IsMuxedAccount => true;

    /// <summary>
    ///     Create a new MuxedAccountMed25519 from the xdr object.
    /// </summary>
    /// <param name="muxed"></param>
    /// <returns></returns>
    public static MuxedAccountMed25519 FromMuxedAccountXdr(Xdr.MuxedAccount.MuxedAccountMed25519 muxed)
    {
        var innerKey = KeyPair.FromPublicKey(muxed.Ed25519.InnerValue);
        var id = muxed.Id.InnerValue;
        return new MuxedAccountMed25519(innerKey, id);
    }

    /// <summary>
    ///     Create a new MuxedAccountMed25519 from an account id in the format "M...".
    /// </summary>
    /// <param name="muxedAccountId"></param>
    /// <returns></returns>
    public static MuxedAccountMed25519 FromMuxedAccountId(string muxedAccountId)
    {
        var muxedAccount = StrKey.DecodeStellarMuxedAccount(muxedAccountId);
        var key = KeyPair.FromPublicKey(muxedAccount.Med25519.Ed25519.InnerValue);
        return new MuxedAccountMed25519(key, muxedAccount.Med25519.Id.InnerValue);
    }
}