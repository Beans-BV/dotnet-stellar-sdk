using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Accounts;

public static class MuxedAccount
{
    public static IAccountId FromXdrMuxedAccount(Xdr.MuxedAccount muxedAccount)
    {
        return muxedAccount.Discriminant.InnerValue switch
        {
            CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_ED25519 => KeyPair.FromPublicKey(
                muxedAccount.Ed25519.InnerValue),
            CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_MUXED_ED25519 => MuxedAccountMed25519.FromMuxedAccountXdr(
                muxedAccount.Med25519),
            _ => throw new InvalidOperationException("Invalid MuxedAccount type"),
        };
    }
}