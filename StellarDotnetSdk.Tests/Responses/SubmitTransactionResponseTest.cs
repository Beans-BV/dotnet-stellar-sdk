using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for <see cref="SubmitTransactionResponse" /> class.
/// </summary>
[TestClass]
public class SubmitTransactionResponseTest
{
    /// <summary>
    ///     Verifies that SubmitTransactionResponse with transaction failure can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithTransactionFailureJson_ReturnsDeserializedFailureResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("submitTransactionTransactionFailure.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var submitTransactionResponse =
            JsonSerializer.Deserialize<SubmitTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(submitTransactionResponse);
        Assert.AreEqual(false, submitTransactionResponse.IsSuccess);
        Assert.AreEqual(
            "AAAAAKpmDL6Z4hvZmkTBkYpHftan4ogzTaO4XTB7joLgQnYYAAAAZAAAAAAABeoyAAAAAAAAAAEAAAAAAAAAAQAAAAAAAAABAAAAAD3sEVVGZGi/NoC3ta/8f/YZKMzyi9ZJpOi0H47x7IqYAAAAAAAAAAAF9eEAAAAAAAAAAAA=",
            submitTransactionResponse.EnvelopeXdr);
        Assert.AreEqual("AAAAAAAAAAD////4AAAAAA==", submitTransactionResponse.ResultXdr);
        Assert.IsNotNull(submitTransactionResponse.SubmitTransactionResponseExtras);
        Assert.IsNotNull(submitTransactionResponse.SubmitTransactionResponseExtras.ExtrasResultCodes);
        Assert.AreEqual("tx_no_source_account",
            submitTransactionResponse.SubmitTransactionResponseExtras.ExtrasResultCodes.TransactionResultCode);
    }

    /// <summary>
    ///     Verifies that SubmitTransactionResponse with operation failure can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithOperationFailureJson_ReturnsDeserializedFailureResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("submitTransactionOperationFailure.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var submitTransactionResponse =
            JsonSerializer.Deserialize<SubmitTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(submitTransactionResponse);
        Assert.AreEqual(false, submitTransactionResponse.IsSuccess);
        Assert.AreEqual(
            "AAAAAF2O0axA67+p2jMunG6G188kDSHIvqQ13d9l29YCSA/uAAAAZAAvvc0AAAABAAAAAAAAAAEAAAAAAAAAAQAAAAAAAAABAAAAAD3sEVVGZGi/NoC3ta/8f/YZKMzyi9ZJpOi0H47x7IqYAAAAAAAAAAAF9eEAAAAAAAAAAAECSA/uAAAAQFuZVAjftHa+JZes1VxSk8naOfjjAz9V86mY1AZf8Ik6PtTsBpDsCfG57EYsq4jWyZcT+vhXyWsw5evF1ELqMw4=",
            submitTransactionResponse.EnvelopeXdr);
        Assert.AreEqual("AAAAAAAAAGT/////AAAAAQAAAAAAAAAB////+wAAAAA=", submitTransactionResponse.ResultXdr);
        Assert.IsNotNull(submitTransactionResponse.SubmitTransactionResponseExtras);
        Assert.IsNotNull(submitTransactionResponse.SubmitTransactionResponseExtras.ExtrasResultCodes);
        Assert.AreEqual("tx_failed",
            submitTransactionResponse.SubmitTransactionResponseExtras.ExtrasResultCodes.TransactionResultCode);
        Assert.IsNotNull(submitTransactionResponse.SubmitTransactionResponseExtras.ExtrasResultCodes
            .OperationsResultCodes);
        Assert.AreEqual("op_no_destination",
            submitTransactionResponse.SubmitTransactionResponseExtras.ExtrasResultCodes.OperationsResultCodes[0]);
    }

    /// <summary>
    ///     Verifies that SubmitTransactionResponse with success can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSuccessJson_ReturnsDeserializedSuccessResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("submitTransactionSuccess.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var submitTransactionResponse =
            JsonSerializer.Deserialize<SubmitTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(submitTransactionResponse);
        Assert.AreEqual(true, submitTransactionResponse.IsSuccess);
        Assert.AreEqual("ee14b93fcd31d4cfe835b941a0a8744e23a6677097db1fafe0552d8657bed940",
            submitTransactionResponse.Hash);
        Assert.AreEqual(3128812, submitTransactionResponse.Ledger);
        Assert.AreEqual(
            "AAAAADSMMRmQGDH6EJzkgi/7PoKhphMHyNGQgDp2tlS/dhGXAAAAZAAT3TUAAAAwAAAAAAAAAAAAAAABAAAAAAAAAAMAAAABSU5SAAAAAAA0jDEZkBgx+hCc5IIv+z6CoaYTB8jRkIA6drZUv3YRlwAAAAFVU0QAAAAAADSMMRmQGDH6EJzkgi/7PoKhphMHyNGQgDp2tlS/dhGXAAAAAAX14QAAAAAKAAAAAQAAAAAAAAAAAAAAAAAAAAG/dhGXAAAAQLuStfImg0OeeGAQmvLkJSZ1MPSkCzCYNbGqX5oYNuuOqZ5SmWhEsC7uOD9ha4V7KengiwNlc0oMNqBVo22S7gk=",
            submitTransactionResponse.EnvelopeXdr);
        Assert.AreEqual(
            "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAADAAAAAAAAAAAAAAAAAAAAADSMMRmQGDH6EJzkgi/7PoKhphMHyNGQgDp2tlS/dhGXAAAAAAAAAPEAAAABSU5SAAAAAAA0jDEZkBgx+hCc5IIv+z6CoaYTB8jRkIA6drZUv3YRlwAAAAFVU0QAAAAAADSMMRmQGDH6EJzkgi/7PoKhphMHyNGQgDp2tlS/dhGXAAAAAAX14QAAAAAKAAAAAQAAAAAAAAAAAAAAAA==",
            submitTransactionResponse.ResultXdr);
        Assert.AreEqual(241L, submitTransactionResponse.GetOfferIdFromResult(0));
    }

    /// <summary>
    ///     Verifies that SubmitTransactionResponse without offer ID can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithNoOfferIdJson_ReturnsResponseWithoutOfferId()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("submitTransactionNoOfferId.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var submitTransactionResponse =
            JsonSerializer.Deserialize<SubmitTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(submitTransactionResponse);
        Assert.AreEqual(true, submitTransactionResponse.IsSuccess);
        Assert.IsNull(submitTransactionResponse.GetOfferIdFromResult(0));
    }
}