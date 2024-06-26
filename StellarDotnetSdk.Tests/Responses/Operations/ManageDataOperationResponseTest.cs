﻿using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class ManageDataOperationResponseTest
{
    //Manage Data
    [TestMethod]
    public void TestManageDataOperation()
    {
        var jsonPath = Utils.GetTestDataPath("manageData.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertManageDataData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeManageDataOperation()
    {
        var jsonPath = Utils.GetTestDataPath("manageData.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertManageDataData(back);
    }

    private static void AssertManageDataData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ManageDataOperationResponse);
        var operation = (ManageDataOperationResponse)instance;

        Assert.AreEqual(operation.Id, 14336188517191688L);
        Assert.AreEqual(operation.Name, "CollateralValue");
        Assert.AreEqual(operation.Value, "MjAwMA==");
    }

    //Manage Data Value Empty
    [TestMethod]
    public void TestDeserializeManageDataOperationValueEmpty()
    {
        var jsonPath = Utils.GetTestDataPath("manageDataValueEmpty.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertManageDataValueEmptyData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeManageDataOperationValueEmpty()
    {
        var jsonPath = Utils.GetTestDataPath("manageDataValueEmpty.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertManageDataValueEmptyData(back);
    }

    private static void AssertManageDataValueEmptyData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ManageDataOperationResponse);
        var operation = (ManageDataOperationResponse)instance;

        Assert.AreEqual(operation.Value, null);
    }
}