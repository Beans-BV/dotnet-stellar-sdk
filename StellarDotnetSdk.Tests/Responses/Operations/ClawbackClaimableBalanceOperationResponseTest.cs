﻿using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class ClawbackClaimableBalance
{
    //Clawback Claimable Balance
    [TestMethod]
    public void TestClawbackClaimableBalance()
    {
        var jsonPath = Utils.GetTestDataPath("clawbackClaimableBalance.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertClawbackClaimableBalanceData(back);
    }

    private static void AssertClawbackClaimableBalanceData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ClawbackClaimableBalanceOperationResponse);
        var operation = (ClawbackClaimableBalanceOperationResponse)instance;

        Assert.AreEqual(214525026504705, operation.Id);
        Assert.AreEqual("00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7",
            operation.BalanceId);
    }
}