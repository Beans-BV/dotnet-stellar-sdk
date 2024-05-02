﻿using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class SetOptionsOperationResponseTest
{
    //Set Options
    [TestMethod]
    public void TestDeserializeSetOptionsOperation()
    {
        var jsonPath = Utils.GetTestDataPath("setOptions.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertSetOptionsData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSetOptionsOperation()
    {
        var jsonPath = Utils.GetTestDataPath("setOptions.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertSetOptionsData(back);
    }

    private static void AssertSetOptionsData(OperationResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is SetOptionsOperationResponse);
        var operation = (SetOptionsOperationResponse)instance;

        Assert.AreEqual(operation.SignerKey, "GD3ZYXVC7C3ECD5I4E5NGPBFJJSULJ6HJI2FBHGKYFV34DSIWB4YEKJZ");
        Assert.AreEqual(operation.SignerWeight, 1);
        Assert.AreEqual(operation.HomeDomain, "stellar.org");
        Assert.AreEqual(operation.InflationDestination, "GBYWSY4NPLLPTP22QYANGTT7PEHND64P4D4B6LFEUHGUZRVYJK2H4TBE");
        Assert.AreEqual(operation.LowThreshold, 1);
        Assert.AreEqual(operation.MedThreshold, 2);
        Assert.AreEqual(operation.HighThreshold, 3);
        Assert.AreEqual(operation.MasterKeyWeight, 4);
        Assert.AreEqual(operation.SetFlags[0], "auth_required_flag");
        Assert.AreEqual(operation.ClearFlags[0], "auth_revocable_flag");
    }

    //Set Options Non Ed25519 Key
    [TestMethod]
    public void TestDeserializeSetOptionsOperationWithNonEd25519Key()
    {
        var jsonPath = Utils.GetTestDataPath("setOptionsNonEd25519Key.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertSetOptionsOperationWithNonEd25519KeyData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSetOptionsOperationWithNonEd25519Key()
    {
        var jsonPath = Utils.GetTestDataPath("setOptionsNonEd25519Key.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertSetOptionsOperationWithNonEd25519KeyData(back);
    }

    private static void AssertSetOptionsOperationWithNonEd25519KeyData(OperationResponse instance)
    {
        Assert.IsTrue(instance is SetOptionsOperationResponse);
        var operation = (SetOptionsOperationResponse)instance;

        Assert.AreEqual(operation.SignerKey, "TBGFYVCU76LJ7GZOCGR4X7DG2NV42JPG5CKRL42LA5FZOFI3U2WU7ZAL");
    }
}