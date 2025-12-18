using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for OperationResponseJsonConverter.
///     Focus: polymorphic type selection based on type_i discriminator.
/// </summary>
[TestClass]
public class OperationResponseJsonConverterTest
{
    private const string BaseJson = """
                                    {
                                        "id": 12345,
                                        "paging_token": "12345",
                                        "source_account": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2",
                                        "created_at": "2023-01-01T00:00:00Z",
                                        "transaction_hash": "abc123",
                                        "_links": {
                                            "self": {"href": "https://horizon.stellar.org/operations/12345"},
                                            "transaction": {"href": "https://horizon.stellar.org/transactions/abc123"},
                                            "effects": {"href": "https://horizon.stellar.org/operations/12345/effects"},
                                            "succeeds": {"href": "https://horizon.stellar.org/effects?cursor=12345&order=asc"},
                                            "precedes": {"href": "https://horizon.stellar.org/effects?cursor=12345&order=desc"}
                                        }
                                    """;

    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    ///     Tests deserialization of create_account operation with type_i=0.
    ///     Verifies that type_i=0 deserializes to CreateAccountOperationResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateAccountOperationTypeI0_ReturnsCreateAccountOperationResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 0,
                              "type": "create_account",
                              "starting_balance": "10000.0",
                              "funder": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2",
                              "account": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<OperationResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(CreateAccountOperationResponse));
    }

    /// <summary>
    ///     Tests deserialization of payment operation with type_i=1.
    ///     Verifies that type_i=1 deserializes to PaymentOperationResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentOperationTypeI1_ReturnsPaymentOperationResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 1,
                              "type": "payment",
                              "from": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2",
                              "to": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2",
                              "amount": "100.0",
                              "asset_type": "native"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<OperationResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(PaymentOperationResponse));
    }

    /// <summary>
    ///     Tests deserialization of set_options operation with type_i=5.
    ///     Verifies that type_i=5 deserializes to SetOptionsOperationResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSetOptionsOperationTypeI5_ReturnsSetOptionsOperationResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 5,
                              "type": "set_options"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<OperationResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(SetOptionsOperationResponse));
    }

    /// <summary>
    ///     Tests deserialization of inflation operation with type_i=9.
    ///     Verifies that type_i=9 deserializes to InflationOperationResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithInflationOperationTypeI9_ReturnsInflationOperationResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 9,
                              "type": "inflation"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<OperationResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(InflationOperationResponse));
    }

    /// <summary>
    ///     Tests deserialization of manage_data operation with type_i=10.
    ///     Verifies that type_i=10 deserializes to ManageDataOperationResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageDataOperationTypeI10_ReturnsManageDataOperationResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 10,
                              "type": "manage_data",
                              "name": "test",
                              "value": "dGVzdA=="
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<OperationResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ManageDataOperationResponse));
    }

    /// <summary>
    ///     Tests deserialization of revoke_sponsorship operation with type_i=18.
    ///     Verifies that type_i=18 deserializes to RevokeSponsorshipOperationResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRevokeSponsorshipOperationTypeI18_ReturnsRevokeSponsorshipOperationResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 18,
                              "type": "revoke_sponsorship"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<OperationResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(RevokeSponsorshipOperationResponse));
    }

    /// <summary>
    ///     Tests deserialization of invoke_host_function operation with type_i=24.
    ///     Verifies that type_i=24 deserializes to InvokeHostFunctionOperationResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithInvokeHostFunctionOperationTypeI24_ReturnsInvokeHostFunctionOperationResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 24,
                              "type": "invoke_host_function",
                              "function": "HostFunctionTypeHostFunctionTypeInvokeContract"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<OperationResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(InvokeHostFunctionOperationResponse));
    }

    /// <summary>
    ///     Tests deserialization of restore_footprint operation with type_i=26.
    ///     Verifies that type_i=26 deserializes to RestoreFootprintOperationResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRestoreFootprintOperationTypeI26_ReturnsRestoreFootprintOperationResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 26,
                              "type": "restore_footprint"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<OperationResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(RestoreFootprintOperationResponse));
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for unknown type_i values.
    ///     Verifies proper error handling when type_i does not match any known operation type.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithUnknownTypeI_ThrowsJsonException()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 999,
                              "type": "unknown"
                              }
                              """;

        // Act & Assert
        JsonSerializer.Deserialize<OperationResponse>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when type_i property is missing.
    ///     Verifies validation for required type_i property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithMissingTypeI_ThrowsJsonException()
    {
        // Arrange
        var json = """
                   {
                       "id": 12345,
                       "type": "unknown"
                   }
                   """;

        // Act & Assert
        JsonSerializer.Deserialize<OperationResponse>(json, _options);
    }
}