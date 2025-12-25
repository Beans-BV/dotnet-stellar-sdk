using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Sep.Sep0009;

namespace StellarDotnetSdk.Tests.Sep.Sep0009;

/// <summary>
///     Tests for OrganizationKycFields class functionality.
/// </summary>
[TestClass]
public class OrganizationKycFieldsTest
{
    /// <summary>
    ///     Verifies that all text properties can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void TextProperties_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var registrationDate = new DateOnly(2020, 1, 1);
        var fields = new OrganizationKycFields
        {
            Name = "Acme Corp",
            VatNumber = "VAT123456",
            RegistrationNumber = "REG789012",
            RegistrationDate = registrationDate,
            RegisteredAddress = "123 Business St",
            ShareholderName = "John Doe",
            AddressCountryCode = "USA",
            StateOrProvince = "NY",
            City = "New York",
            PostalCode = "10001",
            DirectorName = "Jane Smith",
            Website = "https://acme.com",
            Email = "contact@acme.com",
            Phone = "+14155552671",
        };

        // Assert
        fields.Name.Should().Be("Acme Corp");
        fields.VatNumber.Should().Be("VAT123456");
        fields.RegistrationNumber.Should().Be("REG789012");
        fields.RegistrationDate.Should().Be(registrationDate);
        fields.RegisteredAddress.Should().Be("123 Business St");
        fields.ShareholderName.Should().Be("John Doe");
        fields.AddressCountryCode.Should().Be("USA");
        fields.StateOrProvince.Should().Be("NY");
        fields.City.Should().Be("New York");
        fields.PostalCode.Should().Be("10001");
        fields.DirectorName.Should().Be("Jane Smith");
        fields.Website.Should().Be("https://acme.com");
        fields.Email.Should().Be("contact@acme.com");
        fields.Phone.Should().Be("+14155552671");
    }

    /// <summary>
    ///     Verifies that int properties can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void IntProperties_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var fields = new OrganizationKycFields
        {
            NumberOfShareholders = 5,
        };

        // Assert
        fields.NumberOfShareholders.Should().Be(5);
    }

    /// <summary>
    ///     Verifies that binary file properties can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void FileProperties_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var photoIncorporationDoc = new byte[] { 1, 2, 3, 4 };
        var photoProofAddress = new byte[] { 5, 6, 7, 8 };

        var fields = new OrganizationKycFields
        {
            PhotoIncorporationDoc = photoIncorporationDoc,
            PhotoProofAddress = photoProofAddress,
        };

        // Assert
        fields.PhotoIncorporationDoc.Should().BeEquivalentTo(photoIncorporationDoc);
        fields.PhotoProofAddress.Should().BeEquivalentTo(photoProofAddress);
    }

    /// <summary>
    ///     Verifies that GetFields returns empty dictionary when no fields are set.
    /// </summary>
    [TestMethod]
    public void GetFields_WithNoFieldsSet_ReturnsEmptyDictionary()
    {
        // Arrange
        var fields = new OrganizationKycFields();

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetFields includes all set text fields with correct keys.
    /// </summary>
    [TestMethod]
    public void GetFields_WithAllTextFieldsSet_ReturnsAllFields()
    {
        // Arrange
        var registrationDate = new DateOnly(2020, 1, 1);
        var fields = new OrganizationKycFields
        {
            Name = "Acme Corp",
            VatNumber = "VAT123456",
            RegistrationNumber = "REG789012",
            RegistrationDate = registrationDate,
            RegisteredAddress = "123 Business St",
            ShareholderName = "John Doe",
            AddressCountryCode = "USA",
            StateOrProvince = "NY",
            City = "New York",
            PostalCode = "10001",
            DirectorName = "Jane Smith",
            Website = "https://acme.com",
            Email = "contact@acme.com",
            Phone = "+14155552671",
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().ContainKey(OrganizationKycFields.NameFieldKey).WhoseValue.Should().Be("Acme Corp");
        result.Should().ContainKey(OrganizationKycFields.VatNumberFieldKey).WhoseValue.Should().Be("VAT123456");
        result.Should().ContainKey(OrganizationKycFields.RegistrationNumberFieldKey).WhoseValue.Should()
            .Be("REG789012");
        result.Should().ContainKey(OrganizationKycFields.RegistrationDateFieldKey).WhoseValue.Should().Be("2020-01-01");
        result.Should().ContainKey(OrganizationKycFields.RegisteredAddressFieldKey).WhoseValue.Should()
            .Be("123 Business St");
        result.Should().ContainKey(OrganizationKycFields.ShareholderNameFieldKey).WhoseValue.Should().Be("John Doe");
        result.Should().ContainKey(OrganizationKycFields.AddressCountryCodeFieldKey).WhoseValue.Should().Be("USA");
        result.Should().ContainKey(OrganizationKycFields.StateOrProvinceFieldKey).WhoseValue.Should().Be("NY");
        result.Should().ContainKey(OrganizationKycFields.CityFieldKey).WhoseValue.Should().Be("New York");
        result.Should().ContainKey(OrganizationKycFields.PostalCodeFieldKey).WhoseValue.Should().Be("10001");
        result.Should().ContainKey(OrganizationKycFields.DirectorNameFieldKey).WhoseValue.Should().Be("Jane Smith");
        result.Should().ContainKey(OrganizationKycFields.WebsiteFieldKey).WhoseValue.Should().Be("https://acme.com");
        result.Should().ContainKey(OrganizationKycFields.EmailFieldKey).WhoseValue.Should().Be("contact@acme.com");
        result.Should().ContainKey(OrganizationKycFields.PhoneFieldKey).WhoseValue.Should().Be("+14155552671");
    }

    /// <summary>
    ///     Verifies that GetFields formats DateOnly values as ISO 8601 date-only strings.
    /// </summary>
    [TestMethod]
    public void GetFields_WithDateFields_FormatsAsIso8601DateOnly()
    {
        // Arrange
        var fields = new OrganizationKycFields
        {
            RegistrationDate = new DateOnly(2020, 1, 15),
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().ContainKey(OrganizationKycFields.RegistrationDateFieldKey).WhoseValue.Should().Be("2020-01-15");
    }

    /// <summary>
    ///     Verifies that GetFields formats int values as strings.
    /// </summary>
    [TestMethod]
    public void GetFields_WithIntFields_FormatsAsString()
    {
        // Arrange
        var fields = new OrganizationKycFields
        {
            NumberOfShareholders = 5,
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().ContainKey(OrganizationKycFields.NumberOfShareholdersFieldKey).WhoseValue.Should().Be("5");
    }

    /// <summary>
    ///     Verifies that GetFields includes nested FinancialAccount fields with organization prefix.
    /// </summary>
    [TestMethod]
    public void GetFields_WithFinancialAccount_IncludesNestedFieldsWithPrefix()
    {
        // Arrange
        var fields = new OrganizationKycFields
        {
            Name = "Acme Corp",
            FinancialAccount = new FinancialAccountKycFields
            {
                BankName = "Test Bank",
                BankAccountNumber = "1234567890",
            },
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().ContainKey(OrganizationKycFields.NameFieldKey);
        result.Should().ContainKey(OrganizationKycFields.KeyPrefix + FinancialAccountKycFields.BankNameFieldKey)
            .WhoseValue.Should().Be("Test Bank");
        result.Should()
            .ContainKey(OrganizationKycFields.KeyPrefix + FinancialAccountKycFields.BankAccountNumberFieldKey)
            .WhoseValue.Should().Be("1234567890");
    }

    /// <summary>
    ///     Verifies that GetFields includes nested Card fields.
    /// </summary>
    [TestMethod]
    public void GetFields_WithCard_IncludesNestedFields()
    {
        // Arrange
        var fields = new OrganizationKycFields
        {
            Name = "Acme Corp",
            Card = new CardKycFields
            {
                Number = "4111111111111111",
                ExpirationDate = "29-11",
            },
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().ContainKey(OrganizationKycFields.NameFieldKey);
        result.Should().ContainKey(CardKycFields.NumberFieldKey).WhoseValue.Should().Be("4111111111111111");
        result.Should().ContainKey(CardKycFields.ExpirationDateFieldKey).WhoseValue.Should().Be("29-11");
    }

    /// <summary>
    ///     Verifies that GetFiles returns empty dictionary when no files are set.
    /// </summary>
    [TestMethod]
    public void GetFiles_WithNoFilesSet_ReturnsEmptyDictionary()
    {
        // Arrange
        var fields = new OrganizationKycFields();

        // Act
        var result = fields.GetFiles();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetFiles includes all set file fields with correct keys.
    /// </summary>
    [TestMethod]
    public void GetFiles_WithAllFilesSet_ReturnsAllFiles()
    {
        // Arrange
        var photoIncorporationDoc = new byte[] { 1, 2, 3 };
        var photoProofAddress = new byte[] { 4, 5, 6 };

        var fields = new OrganizationKycFields
        {
            PhotoIncorporationDoc = photoIncorporationDoc,
            PhotoProofAddress = photoProofAddress,
        };

        // Act
        var result = fields.GetFiles();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainKey(OrganizationKycFields.PhotoIncorporationDocFileKey).WhoseValue.Should()
            .BeEquivalentTo(photoIncorporationDoc);
        result.Should().ContainKey(OrganizationKycFields.PhotoProofAddressFileKey).WhoseValue.Should()
            .BeEquivalentTo(photoProofAddress);
    }

    /// <summary>
    ///     Verifies that GetFiles only includes set files, excluding null values.
    /// </summary>
    [TestMethod]
    public void GetFiles_WithPartialFilesSet_ReturnsOnlySetFiles()
    {
        // Arrange
        var photoIncorporationDoc = new byte[] { 1, 2, 3 };
        var fields = new OrganizationKycFields
        {
            PhotoIncorporationDoc = photoIncorporationDoc,
        };

        // Act
        var result = fields.GetFiles();

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(OrganizationKycFields.PhotoIncorporationDocFileKey);
        result.Should().NotContainKey(OrganizationKycFields.PhotoProofAddressFileKey);
    }

    /// <summary>
    ///     Verifies that all field keys are prefixed with "organization.".
    /// </summary>
    [TestMethod]
    public void FieldKeys_ArePrefixedWithOrganization()
    {
        // Assert
        OrganizationKycFields.NameFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.VatNumberFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.RegistrationNumberFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.RegistrationDateFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.RegisteredAddressFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.NumberOfShareholdersFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.ShareholderNameFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.AddressCountryCodeFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.StateOrProvinceFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.CityFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.PostalCodeFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.DirectorNameFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.WebsiteFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.EmailFieldKey.Should().StartWith("organization.");
        OrganizationKycFields.PhoneFieldKey.Should().StartWith("organization.");
    }

    /// <summary>
    ///     Verifies that field key constants match SEP-9 specification.
    /// </summary>
    [TestMethod]
    public void FieldKeyConstants_MatchSep9Specification()
    {
        // Assert
        OrganizationKycFields.KeyPrefix.Should().Be("organization.");
        OrganizationKycFields.NameFieldKey.Should().Be("organization.name");
        OrganizationKycFields.VatNumberFieldKey.Should().Be("organization.VAT_number");
        OrganizationKycFields.RegistrationNumberFieldKey.Should().Be("organization.registration_number");
        OrganizationKycFields.RegistrationDateFieldKey.Should().Be("organization.registration_date");
        OrganizationKycFields.RegisteredAddressFieldKey.Should().Be("organization.registered_address");
        OrganizationKycFields.NumberOfShareholdersFieldKey.Should().Be("organization.number_of_shareholders");
        OrganizationKycFields.ShareholderNameFieldKey.Should().Be("organization.shareholder_name");
        OrganizationKycFields.AddressCountryCodeFieldKey.Should().Be("organization.address_country_code");
        OrganizationKycFields.StateOrProvinceFieldKey.Should().Be("organization.state_or_province");
        OrganizationKycFields.CityFieldKey.Should().Be("organization.city");
        OrganizationKycFields.PostalCodeFieldKey.Should().Be("organization.postal_code");
        OrganizationKycFields.DirectorNameFieldKey.Should().Be("organization.director_name");
        OrganizationKycFields.WebsiteFieldKey.Should().Be("organization.website");
        OrganizationKycFields.EmailFieldKey.Should().Be("organization.email");
        OrganizationKycFields.PhoneFieldKey.Should().Be("organization.phone");
    }

    /// <summary>
    ///     Verifies that file key constants match SEP-9 specification.
    /// </summary>
    [TestMethod]
    public void FileKeyConstants_MatchSep9Specification()
    {
        // Assert
        OrganizationKycFields.PhotoIncorporationDocFileKey.Should().Be("organization.photo_incorporation_doc");
        OrganizationKycFields.PhotoProofAddressFileKey.Should().Be("organization.photo_proof_address");
    }
}