using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Sep.Sep0009;

namespace StellarDotnetSdk.Tests.Sep.Sep0009;

/// <summary>
///     Tests for CardKycFields class functionality.
/// </summary>
[TestClass]
public class CardKycFieldsTest
{
    /// <summary>
    ///     Verifies that all properties can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void Properties_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var fields = new CardKycFields
        {
            Number = "4111111111111111",
            ExpirationDate = "29-11",
            Cvc = "123",
            HolderName = "John Doe",
            Network = "Visa",
            PostalCode = "12345",
            CountryCode = "US",
            StateOrProvince = "NY",
            City = "New York",
            Address = "123 Main St\nNew York, NY 12345",
            Token = "tok_visa_1234",
        };

        // Assert
        fields.Number.Should().Be("4111111111111111");
        fields.ExpirationDate.Should().Be("29-11");
        fields.Cvc.Should().Be("123");
        fields.HolderName.Should().Be("John Doe");
        fields.Network.Should().Be("Visa");
        fields.PostalCode.Should().Be("12345");
        fields.CountryCode.Should().Be("US");
        fields.StateOrProvince.Should().Be("NY");
        fields.City.Should().Be("New York");
        fields.Address.Should().Be("123 Main St\nNew York, NY 12345");
        fields.Token.Should().Be("tok_visa_1234");
    }

    /// <summary>
    ///     Verifies that GetFields returns empty dictionary when no fields are set.
    /// </summary>
    [TestMethod]
    public void GetFields_WithNoFieldsSet_ReturnsEmptyDictionary()
    {
        // Arrange
        var fields = new CardKycFields();

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetFields includes all set fields with correct keys.
    /// </summary>
    [TestMethod]
    public void GetFields_WithAllFieldsSet_ReturnsAllFields()
    {
        // Arrange
        var fields = new CardKycFields
        {
            Number = "4111111111111111",
            ExpirationDate = "29-11",
            Cvc = "123",
            HolderName = "John Doe",
            Network = "Visa",
            PostalCode = "12345",
            CountryCode = "US",
            StateOrProvince = "NY",
            City = "New York",
            Address = "123 Main St\nNew York, NY 12345",
            Token = "tok_visa_1234",
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().HaveCount(11);
        result.Should().ContainKey(CardKycFields.NumberFieldKey).WhoseValue.Should().Be("4111111111111111");
        result.Should().ContainKey(CardKycFields.ExpirationDateFieldKey).WhoseValue.Should().Be("29-11");
        result.Should().ContainKey(CardKycFields.CvcFieldKey).WhoseValue.Should().Be("123");
        result.Should().ContainKey(CardKycFields.HolderNameFieldKey).WhoseValue.Should().Be("John Doe");
        result.Should().ContainKey(CardKycFields.NetworkFieldKey).WhoseValue.Should().Be("Visa");
        result.Should().ContainKey(CardKycFields.PostalCodeFieldKey).WhoseValue.Should().Be("12345");
        result.Should().ContainKey(CardKycFields.CountryCodeFieldKey).WhoseValue.Should().Be("US");
        result.Should().ContainKey(CardKycFields.StateOrProvinceFieldKey).WhoseValue.Should().Be("NY");
        result.Should().ContainKey(CardKycFields.CityFieldKey).WhoseValue.Should().Be("New York");
        result.Should().ContainKey(CardKycFields.AddressFieldKey).WhoseValue.Should()
            .Be("123 Main St\nNew York, NY 12345");
        result.Should().ContainKey(CardKycFields.TokenFieldKey).WhoseValue.Should().Be("tok_visa_1234");
    }

    /// <summary>
    ///     Verifies that GetFields only includes set fields, excluding null values.
    /// </summary>
    [TestMethod]
    public void GetFields_WithPartialFieldsSet_ReturnsOnlySetFields()
    {
        // Arrange
        var fields = new CardKycFields
        {
            Number = "4111111111111111",
            ExpirationDate = "29-11",
            Cvc = "123",
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().HaveCount(3);
        result.Should().ContainKey(CardKycFields.NumberFieldKey);
        result.Should().ContainKey(CardKycFields.ExpirationDateFieldKey);
        result.Should().ContainKey(CardKycFields.CvcFieldKey);
        result.Should().NotContainKey(CardKycFields.HolderNameFieldKey);
        result.Should().NotContainKey(CardKycFields.NetworkFieldKey);
    }

    /// <summary>
    ///     Verifies that all field keys are prefixed with "card.".
    /// </summary>
    [TestMethod]
    public void FieldKeys_ArePrefixedWithCard()
    {
        // Assert
        CardKycFields.NumberFieldKey.Should().StartWith("card.");
        CardKycFields.ExpirationDateFieldKey.Should().StartWith("card.");
        CardKycFields.CvcFieldKey.Should().StartWith("card.");
        CardKycFields.HolderNameFieldKey.Should().StartWith("card.");
        CardKycFields.NetworkFieldKey.Should().StartWith("card.");
        CardKycFields.PostalCodeFieldKey.Should().StartWith("card.");
        CardKycFields.CountryCodeFieldKey.Should().StartWith("card.");
        CardKycFields.StateOrProvinceFieldKey.Should().StartWith("card.");
        CardKycFields.CityFieldKey.Should().StartWith("card.");
        CardKycFields.AddressFieldKey.Should().StartWith("card.");
        CardKycFields.TokenFieldKey.Should().StartWith("card.");
    }

    /// <summary>
    ///     Verifies that field key constants match SEP-9 specification.
    /// </summary>
    [TestMethod]
    public void FieldKeyConstants_MatchSep9Specification()
    {
        // Assert
        CardKycFields.KeyPrefix.Should().Be("card.");
        CardKycFields.NumberFieldKey.Should().Be("card.number");
        CardKycFields.ExpirationDateFieldKey.Should().Be("card.expiration_date");
        CardKycFields.CvcFieldKey.Should().Be("card.cvc");
        CardKycFields.HolderNameFieldKey.Should().Be("card.holder_name");
        CardKycFields.NetworkFieldKey.Should().Be("card.network");
        CardKycFields.PostalCodeFieldKey.Should().Be("card.postal_code");
        CardKycFields.CountryCodeFieldKey.Should().Be("card.country_code");
        CardKycFields.StateOrProvinceFieldKey.Should().Be("card.state_or_province");
        CardKycFields.CityFieldKey.Should().Be("card.city");
        CardKycFields.AddressFieldKey.Should().Be("card.address");
        CardKycFields.TokenFieldKey.Should().Be("card.token");
    }
}