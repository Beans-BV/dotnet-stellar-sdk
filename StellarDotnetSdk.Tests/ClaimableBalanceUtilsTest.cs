using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class ClaimableBalanceUtilsTest
{
    [TestMethod]
    [DataRow("299a32106238f3b2d84d4142783fe320253bcda775d1bfb7accdb533021ddccf",
        "BAACTGRSCBRDR45S3BGUCQTYH7RSAJJ3ZWTXLUN7W6WM3NJTAIO5ZT2U6I")]
    public void TestFromXdr(string hex, string base32)
    {
        var xdrClaimableBalanceId = new ClaimableBalanceID
        {
            Discriminant =
                ClaimableBalanceIDType.Create(ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum
                    .CLAIMABLE_BALANCE_ID_TYPE_V0),
            V0 = new Hash(Convert.FromHexString(hex)),
        };
        var id = ClaimableBalanceUtils.FromXdr(xdrClaimableBalanceId);
        Assert.AreEqual(id, base32);
    }

    [TestMethod]
    [DataRow("BAACTGRSCBRDR45S3BGUCQTYH7RSAJJ3ZWTXLUN7W6WM3NJTAIO5ZT2U6I",
        "00000000299a32106238f3b2d84d4142783fe320253bcda775d1bfb7accdb533021ddccf")]
    public void TestToHexString(string input, string output)
    {
        var hex = ClaimableBalanceUtils.ToHexString(input).ToLower();
        Assert.AreEqual(output, hex);
    }

    [TestMethod]
    [DataRow("00000000299a32106238f3b2d84d4142783fe320253bcda775d1bfb7accdb533021ddccf",
        "BAACTGRSCBRDR45S3BGUCQTYH7RSAJJ3ZWTXLUN7W6WM3NJTAIO5ZT2U6I")]
    public void TestToBase32String(string input, string output)
    {
        var base32 = ClaimableBalanceUtils.ToBase32String(input);
        Assert.AreEqual(output, base32);
    }
}