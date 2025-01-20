﻿using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class EndSponsoringFutureReservesOperationResponseTest
{
    //End Sponsoring Future Reserves
    [TestMethod]
    public void TestSerializationEndSponsoringFutureReservesOperation()
    {
        var jsonPath = Utils.GetTestDataPath("endSponsoringFutureReserves.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton2.GetInstance<OperationResponse>(json);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertEndSponsoringFutureReservesData(back);
    }

    private static void AssertEndSponsoringFutureReservesData(OperationResponse instance)
    {
        Assert.IsTrue(instance is EndSponsoringFutureReservesOperationResponse);
        var operation = (EndSponsoringFutureReservesOperationResponse)instance;

        Assert.AreEqual(215542933753859, operation.Id);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.BeginSponsor);
        Assert.IsNull(operation.BeginSponsorMuxed);
        Assert.IsNull(operation.BeginSponsorMuxedID);

        var back = new EndSponsoringFutureReservesOperationResponse
        {
            BeginSponsor = operation.BeginSponsor,
        };
        Assert.IsNotNull(back);
    }

    //End Sponsoring Future Reserves (Muxed)
    [TestMethod]
    public void TestSerializationEndSponsoringFutureReservesOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("endSponsoringFutureReservesMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton2.GetInstance<OperationResponse>(json);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertEndSponsoringFutureReservesDataMuxed(back);
    }

    private static void AssertEndSponsoringFutureReservesDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is EndSponsoringFutureReservesOperationResponse);
        var operation = (EndSponsoringFutureReservesOperationResponse)instance;

        Assert.AreEqual(215542933753859, operation.Id);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.BeginSponsor);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24",
            operation.BeginSponsorMuxed);
        Assert.AreEqual(5123456789UL, operation.BeginSponsorMuxedID);

        var back = new EndSponsoringFutureReservesOperationResponse
        {
            BeginSponsor = operation.BeginSponsor,
        };
        Assert.IsNotNull(back);
    }
}