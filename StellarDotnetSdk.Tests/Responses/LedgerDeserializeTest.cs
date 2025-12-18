using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
/// Unit tests for deserializing ledger responses from JSON.
/// </summary>
[TestClass]
public class LedgerDeserializeTest
{
    /// <summary>
    /// Verifies that LedgerResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLedgerJson_ReturnsDeserializedLedger()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("ledger.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var ledger = JsonSerializer.Deserialize<LedgerResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(ledger);
        AssertTestData(ledger);
    }

    /// <summary>
    /// Verifies that LedgerResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithLedger_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("ledger.json");
        var json = File.ReadAllText(jsonPath);
        var ledger = JsonSerializer.Deserialize<LedgerResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(ledger);
        var back = JsonSerializer.Deserialize<LedgerResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    public static void AssertTestData(LedgerResponse ledger)
    {
        Assert.AreEqual("1fb371bb8f393ff06b20ae7eac52a6df0d60e2f97e27c1fa1635784f6d3e1f57", ledger.Id);
        Assert.AreEqual("1fb371bb8f393ff06b20ae7eac52a6df0d60e2f97e27c1fa1635784f6d3e1f57", ledger.Hash);
        Assert.AreEqual("8042102923460608", ledger.PagingToken);
        Assert.AreEqual("e2e5e6406e2dab62e0e1d85f9893a5305ec300bea6f0aba6da23ed7a68f0c5a6", ledger.PrevHash);
        Assert.AreEqual(1872448, ledger.Sequence);
        Assert.AreEqual(0, ledger.SuccessfulTransactionCount);
        Assert.AreEqual(0, ledger.FailedTransactionCount);
        Assert.AreEqual(0, ledger.OperationCount);
        Assert.AreEqual(new DateTimeOffset(2025, 12, 1, 6, 13, 55, TimeSpan.Zero), ledger.ClosedAt);
        Assert.AreEqual("100000000000.0000000", ledger.TotalCoins);
        Assert.AreEqual("110238.5552711", ledger.FeePool);
        Assert.AreEqual(100L, ledger.BaseFeeInStroops);
        Assert.AreEqual(5000000L, ledger.BaseReserveInStroops);
        Assert.AreEqual(200, ledger.MaxTxSetSize);
        Assert.AreEqual(0, ledger.TxSetOperationCount);
        Assert.AreEqual(24, ledger.ProtocolVersion);
        Assert.AreEqual(
            "AAAAGOLl5kBuLati4OHYX5iTpTBewwC+pvCrptoj7Xpo8MWmuOtF4mZ5W5FqRpP1uBpDcqSXo7AXH6VFEnCD+2MtqUkAAAAAaS0yIwAAAAAAAAABAAAAALVdELK7fShO1cA6R6XhtZDJD1eDVUccxFB7voIE0jyLAAAAQPETnBQa/pWDlgRok4B7iWG5kC4H44HxxgAjETE/PnaavellzXalaZEC/Hjal8xG9DxlxxksoQONg8CgM21tLQvfP2GYBKkv20BXGS3EPddI6neK3FK8SYzoBSTAFLgRGciQFjs4XdBP6iKUa2qDQv7d1sSKqx0QuUu8QurI+OgkABySQA3gtrOnZAAAAAABAKtMnUcAAAAAAAAAAAAAS8YAAABkAExLQAAAAMidHca46Awo/RV/e/e7ZZcoLDlRR05yivq6HB89Y6M0HHVLqz7SEzdtx/CcggGySkDPpv1fuDR/jmnvZU5p/S1NAF9hgG47QLS1NtgivFkn+gxsHrXuzqoxMYbCPjosfgOLkjDUqJCR9uLj3apU0+rlkVaMBK0QREEXiAUPVL81iAAAAAA=",
            ledger.HeaderXdr);
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/1872448",
            ledger.Links.Self.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/1872448/operations{?cursor,limit,order}",
            ledger.Links.Operations.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/1872448/payments{?cursor,limit,order}",
            ledger.Links.Payments.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/1872448/effects{?cursor,limit,order}",
            ledger.Links.Effects.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers/1872448/transactions{?cursor,limit,order}",
            ledger.Links.Transactions.Href);
    }
}