﻿using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses;
using stellar_dotnet_sdk.responses.operations;

namespace stellar_dotnet_sdk_test.responses.operations;

[TestClass]
public class ManageOfferOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeManageOfferOperation()
    {
        var json = File.ReadAllText(Path.Combine("testdata/operations/manageOffer", "manageOffer.json"));
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);

        AssertManageOfferData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeManageOfferOperation()
    {
        var json = File.ReadAllText(Path.Combine("testdata/operations/manageOffer", "manageOffer.json"));
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertManageOfferData(back);
    }

    private static void AssertManageOfferData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ManageSellOfferOperationResponse);
        var operation = (ManageSellOfferOperationResponse)instance;

        Assert.AreEqual(operation.OfferId, "96052902");
        Assert.AreEqual(operation.Amount, "243.7500000");

        operation.Price
            .Should().Be("8.0850240");

        operation.PriceRatio.Numerator
            .Should().Be(5054660);

        operation.PriceRatio.Denominator
            .Should().Be(625188);

        Assert.AreEqual(operation.SellingAsset,
            Asset.CreateNonNativeAsset("USD", "GDSRCV5VTM3U7Y3L6DFRP3PEGBNQMGOWSRTGSBWX6Z3H6C7JHRI4XFJP"));
        Assert.AreEqual(operation.BuyingAsset, new AssetTypeNative());
    }
}