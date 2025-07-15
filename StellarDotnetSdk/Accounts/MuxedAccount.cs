using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Accounts;

/// <summary>
///     Represents a multiplexed account on Stellar's network.
///     <p>
///         A muxed account is an extension of the regular account that allows multiple entities to share
///         the same ed25519 key pair as their account ID while providing a unique identifier for each
///         entity.
///     </p>
///     <p>
///         A muxed account consists of two parts:
///         The ed25519 account ID, which starts with the letter "G".
///         An optional account multiplexing ID, which is a 64-bit unsigned integer.
///     </p>
///     <see
///         href="https://developers.stellar.org/docs/learn/encyclopedia/transactions-specialized/pooled-accounts-muxed-accounts-memos#muxed-accounts">
///         Muxed Accounts
///     </see>
/// </summary>
public class MuxedAccount
{
    /// <summary>
    ///     Create a new MuxedAccountMed25519 with the given key and id.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="id"></param>
    public MuxedAccount(KeyPair key, ulong? id)
    {
        Id = id;
        Key = key ?? throw new ArgumentNullException(nameof(key));
    }

    public ulong? Id { get; }
    public KeyPair Key { get; }

    public byte[] PublicKey => Key.PublicKey;

    private string? _mAddress;

    public Xdr.MuxedAccount ToXdrMuxedAccount()
    {
        Xdr.MuxedAccount muxedAccount;
        if (IsMuxedAccount)
        {
            muxedAccount = new Xdr.MuxedAccount
            {
                Discriminant =
                    new CryptoKeyType { InnerValue = CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_MUXED_ED25519 },
                Med25519 = new Xdr.MuxedAccount.MuxedAccountMed25519
                {
                    Id = new Uint64(Id!.Value),
                    Ed25519 = new Uint256(Key.PublicKey),
                },
            };
            return muxedAccount;
        }
        muxedAccount = new Xdr.MuxedAccount
        {
            Discriminant = CryptoKeyType.Create(CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_ED25519),
            Ed25519 = new Uint256(Key.PublicKey),
        };

        return muxedAccount;
    }

    /// <inheritdoc />
    public string Address
    {
        get
        {
            if (!IsMuxedAccount)
            {
                return AccountId;
            }
            if (_mAddress != null)
            {
                return _mAddress;
            }
            var writer = new XdrDataOutputStream();
            Xdr.MuxedAccount.MuxedAccountMed25519.Encode(writer, ToXdrMuxedAccount().Med25519);
            _mAddress = StrKey.EncodeMuxedEd25519PublicKey(writer.ToArray());
            return _mAddress;
        }
    }

    ///  <inheritdoc />
    [Obsolete("Deprecated. Use Key.AccountId directly.")]
    public string AccountId => Key.AccountId;


    public KeyPair SigningKey => Key;

    public bool IsMuxedAccount => Id.HasValue;

    public static MuxedAccount FromXdr(Xdr.MuxedAccount xdr)
    {
        switch (xdr.Discriminant.InnerValue)
        {
            case CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_ED25519:
                return new MuxedAccount(
                    KeyPair.FromAccountId(
                        StrKey.EncodeStellarAccountId(xdr.Ed25519.InnerValue)),
                    null
                );
            case CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_MUXED_ED25519:
                return FromXdrMuxedAccountMed25519(xdr.Med25519);
            default:
                throw new ArgumentOutOfRangeException(nameof(xdr.Discriminant.InnerValue),
                    "Unsupported CryptoKeyType.");
        }
    }

    /// <summary>
    ///     Create a new MuxedAccount from the given muxed account address.
    /// </summary>
    /// <param name="address">Can be either a regular account ID (starting with
    ///     "G") or a muxed account address (starting with "M").</param>
    public static MuxedAccount FromMuxedAccountId(string address)
    {
        if (StrKey.IsValidEd25519PublicKey(address))
        {
            return new MuxedAccount(KeyPair.FromAccountId(address), null);
        }
        if (StrKey.TryDecodeMuxedEd25519PublicKey(address, out var decoded))
        {
            var xdrMuxedAccount = Xdr.MuxedAccount.Decode(new XdrDataInputStream(decoded));

            return FromXdrMuxedAccountMed25519(xdrMuxedAccount.Med25519);
        }
        throw new ArgumentException("Invalid address");
    }

    private static MuxedAccount FromXdrMuxedAccountMed25519(Xdr.MuxedAccount.MuxedAccountMed25519 muxed)
    {
        var innerKey = KeyPair.FromPublicKey(muxed.Ed25519.InnerValue);
        var id = muxed.Id.InnerValue;
        return new MuxedAccount(innerKey, id);
    }
}