﻿using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Assets;

/// <summary>
///     Asset class represents an asset, either the native asset (XLM) or a asset code / issuer account ID pair.
///     An asset code describes an asset code and issuer pair. In the case of the native asset XLM, the issuer will be
///     null.
/// </summary>
public abstract class Asset
{
    /// <summary>
    ///     Returns asset type. Possible types:
    ///     <ul>
    ///         <li>native See <see cref="AssetTypeNative" /> for more information.</li>
    ///         <li>credit_alphanum4 See <see cref="AssetTypeCreditAlphaNum4" /> for more information.</li>
    ///         <li>credit_alphanum12 See <see cref="AssetTypeCreditAlphaNum12" /> for more information.</li>
    ///         <li>liquidity_pool_shares</li>
    ///     </ul>
    /// </summary>
    public abstract string Type { get; }

    public static Asset Create(string canonicalForm)
    {
        if (canonicalForm == "native")
        {
            return new AssetTypeNative();
        }

        var parts = canonicalForm.Split(':');
        if (parts.Length != 2)
        {
            throw new ArgumentException("invalid asset " + canonicalForm);
        }
        return CreateNonNativeAsset(parts[0], parts[1]);
    }

    /// <summary>
    ///     Create an asset base on the parameters.
    /// </summary>
    /// <param name="type">Accepted value: <c>native</c>, otherwise it will be ignored.</param>
    /// <param name="code"></param>
    /// <param name="issuer"></param>
    /// <returns>Asset</returns>
    public static Asset Create(string? type, string? code, string? issuer)
    {
        if (type == "native")
        {
            return new AssetTypeNative();
        }

        if (code == null)
        {
            throw new ArgumentNullException(nameof(code), "Code cannot be null.");
        }
        if (issuer == null)
        {
            throw new ArgumentNullException(nameof(issuer), "Issuer cannot be null.");
        }

        return CreateNonNativeAsset(code, issuer);
    }

    /// <summary>
    ///     Creates one of <seealso cref="AssetTypeCreditAlphaNum4" /> or <seealso cref="AssetTypeCreditAlphaNum12" /> object
    ///     based on a code length
    /// </summary>
    /// <param name="code">Asset code</param>
    /// <param name="issuer">Asset issuer</param>
    public static AssetTypeCreditAlphaNum CreateNonNativeAsset(string code, string issuer)
    {
        if (code.Length >= 1 && code.Length <= 4)
        {
            return new AssetTypeCreditAlphaNum4(code, issuer);
        }
        if (code.Length >= 5 && code.Length <= 12)
        {
            return new AssetTypeCreditAlphaNum12(code, issuer);
        }
        throw new AssetCodeLengthInvalidException();
    }

    /// <summary>
    ///     Generates Asset object from a given XDR object
    /// </summary>
    /// <param name="thisXdr"></param>
    public static Asset FromXdr(Xdr.Asset thisXdr)
    {
        switch (thisXdr.Discriminant.InnerValue)
        {
            case AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE:
                return new AssetTypeNative();
            case AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4:
                var assetCode4 = Util.PaddedByteArrayToString(thisXdr.AlphaNum4.AssetCode.InnerValue);
                var issuer4 = KeyPair.FromXdrPublicKey(thisXdr.AlphaNum4.Issuer.InnerValue);
                return new AssetTypeCreditAlphaNum4(assetCode4, issuer4.AccountId);
            case AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12:
                var assetCode12 = Util.PaddedByteArrayToString(thisXdr.AlphaNum12.AssetCode.InnerValue);
                var issuer12 = KeyPair.FromXdrPublicKey(thisXdr.AlphaNum12.Issuer.InnerValue);
                return new AssetTypeCreditAlphaNum12(assetCode12, issuer12.AccountId);
            default:
                throw new ArgumentException("Unknown asset type " + thisXdr.Discriminant.InnerValue);
        }
    }

    /// <summary>
    ///     Generates XDR object from a given Asset object
    /// </summary>
    public abstract Xdr.Asset ToXdr();

    /// <summary>
    ///     Returns the asset canonical name.
    /// </summary>
    public abstract string CanonicalName();

    public abstract int CompareTo(Asset asset);
}