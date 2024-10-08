﻿using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Memos;

/// <summary>
///     Abstract class for creating various Memo Types.
/// </summary>
public abstract class Memo
{
    /// <summary>
    ///     Creates new MemoNone instance.
    /// </summary>
    public static MemoNone None()
    {
        return new MemoNone();
    }

    /// <summary>
    ///     Creates new {@link MemoText} instance.
    /// </summary>
    /// <param name="text">The text value of a Text Memo.</param>
    public static MemoText Text(string text)
    {
        return new MemoText(text);
    }

    /// <summary>
    ///     Creates new {@link MemoText} instance.
    /// </summary>
    /// <param name="text">The text value of a Text Memo.</param>
    public static MemoText Text(byte[] text)
    {
        return new MemoText(text);
    }

    /// <summary>
    ///     Creates new {@link MemoId} instance.
    /// </summary>
    /// <param name="id">The id value of an Id Memo.</param>
    public static MemoId Id(ulong id)
    {
        return new MemoId(id);
    }

    /// <summary>
    ///     Creates new {@link MemoHash} instance from byte array.
    /// </summary>
    /// <param name="bytes">The byte array of a Hash Memo.</param>
    public static MemoHash Hash(byte[] bytes)
    {
        return new MemoHash(bytes);
    }

    /// <summary>
    ///     Creates new {@link MemoHash} instance from hex-encoded string
    /// </summary>
    /// <param name="hexString">The hex value of a Hash Memo</param>
    public static MemoHash Hash(string hexString)
    {
        return new MemoHash(hexString);
    }

    /// <summary>
    ///     Creates new {@link MemoReturnHash} instance from byte array.
    /// </summary>
    /// <param name="bytes">A byte array of a Return Hash Memo.</param>
    public static MemoReturnHash ReturnHash(byte[] bytes)
    {
        return new MemoReturnHash(bytes);
    }

    /// <summary>
    ///     Creates new {@link MemoReturnHash} instance from hex-encoded string.
    /// </summary>
    /// <param name="hexString">The hex value of a Return Hash Memo.</param>
    public static MemoReturnHash ReturnHash(string hexString)
    {
        return new MemoReturnHash(hexString);
    }

    public static Memo FromXdr(Xdr.Memo memo)
    {
        return memo.Discriminant.InnerValue switch
        {
            MemoType.MemoTypeEnum.MEMO_NONE => None(),
            MemoType.MemoTypeEnum.MEMO_ID => Id(memo.Id.InnerValue),
            MemoType.MemoTypeEnum.MEMO_TEXT => Text(memo.Text),
            MemoType.MemoTypeEnum.MEMO_HASH => Hash(memo.Hash.InnerValue),
            MemoType.MemoTypeEnum.MEMO_RETURN => ReturnHash(memo.RetHash.InnerValue),
            _ => throw new SystemException("Unknown memo type"),
        };
    }

    /// <summary>
    ///     Abstract method for ToXdr
    /// </summary>
    /// <returns>A memo object.</returns>
    public abstract Xdr.Memo ToXdr();

    public abstract override bool Equals(object? obj);
    public abstract override int GetHashCode();
}