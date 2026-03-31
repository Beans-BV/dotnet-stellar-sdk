using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Accounts;

/// <summary>
///     Provides factory methods for creating account identifiers from XDR muxed account representations.
/// </summary>
public static class MuxedAccount
{
    /// <summary>
    ///     Creates an <see cref="IAccountId" /> from an XDR <see cref="Xdr.MuxedAccount" />.
    ///     Returns a <see cref="KeyPair" /> for standard Ed25519 accounts, or a
    ///     <see cref="MuxedAccountMed25519" /> for multiplexed accounts.
    /// </summary>
    /// <param name="muxedAccount">The XDR muxed account to convert.</param>
    /// <returns>An <see cref="IAccountId" /> representing the account.</returns>
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