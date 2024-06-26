﻿using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class BumpSequenceOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeBumpSequenceOperation()
    {
        var jsonPath = Utils.GetTestDataPath("bumpSequence.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertBumpSequenceData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeBumpSequenceOperation()
    {
        var jsonPath = Utils.GetTestDataPath("bumpSequence.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertBumpSequenceData(back);
    }

    private static void AssertBumpSequenceData(OperationResponse instance)
    {
        Assert.IsTrue(instance is BumpSequenceOperationResponse);
        var operation = (BumpSequenceOperationResponse)instance;

        Assert.AreEqual(12884914177L, operation.Id);
        Assert.AreEqual(79473726952833048L, operation.BumpTo);
    }
}