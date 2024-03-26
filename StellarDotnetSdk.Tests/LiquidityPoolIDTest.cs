﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class LiquidityPoolIDTest
{
    [TestMethod]
    public void TestCreate()
    {
        var a = Asset.Create("native");
        var b = Asset.CreateNonNativeAsset("ABC", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");

        var id = new LiquidityPoolID(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, a, b,
            LiquidityPoolParameters.Fee);
        Assert.AreEqual("cc22414997d7e3d9a9ac3b1d65ca9cc3e5f35ce33e0bd6a885648b11aaa3b72d", id.ToString());
    }

    [TestMethod]
    public void TestNotLexicographicOrder()
    {
        var a = Asset.Create("native");
        var b = Asset.CreateNonNativeAsset("ABC", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");

        Assert.ThrowsException<ArgumentException>(
            () => new LiquidityPoolID(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, b, a,
                LiquidityPoolParameters.Fee), "Asset A must be < Asset B (Lexicographic Order)");
    }

    [TestMethod]
    public void TestEquality()
    {
        var a = Asset.Create("native");
        var b = Asset.CreateNonNativeAsset("ABC", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");

        var pool1 = new LiquidityPoolID(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, a,
            b, LiquidityPoolParameters.Fee);
        Assert.AreEqual(pool1, pool1);
    }

    [TestMethod]
    public void TestInequality()
    {
        var a = Asset.Create("native");
        var b = Asset.CreateNonNativeAsset("ABC", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");
        var c = Asset.CreateNonNativeAsset("ABCD", "GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");

        var pool1 = new LiquidityPoolID(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, a,
            b, LiquidityPoolParameters.Fee);
        var pool2 = new LiquidityPoolID(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, b,
            c, LiquidityPoolParameters.Fee);
        Assert.AreNotEqual(pool1, pool2);
    }
}