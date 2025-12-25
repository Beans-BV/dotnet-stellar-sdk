using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Sep.Sep0009;

namespace StellarDotnetSdk.Tests.Sep.Sep0009;

/// <summary>
///     Tests for NaturalPersonKycFields class functionality.
/// </summary>
[TestClass]
public class NaturalPersonKycFieldsTest
{
    /// <summary>
    ///     Verifies that all text properties can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void TextProperties_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var fields = new NaturalPersonKycFields
        {
            LastName = "Doe",
            FirstName = "John",
            AdditionalName = "Middle",
            AddressCountryCode = "USA",
            StateOrProvince = "NY",
            City = "New York",
            PostalCode = "10001",
            Address = "123 Main St\nNew York, NY 10001",
            MobileNumber = "+14155552671",
            MobileNumberFormat = "E.164",
            EmailAddress = "john@example.com",
            BirthPlace = "New York, NY, USA",
            BirthCountryCode = "USA",
            TaxId = "123-45-6789",
            TaxIdName = "SSN",
            EmployerName = "Acme Corp",
            EmployerAddress = "456 Business Ave",
            LanguageCode = "en",
            IdType = "passport",
            IdCountryCode = "USA",
            IdNumber = "123456789",
            IpAddress = "192.168.1.1",
            Sex = "male",
            ReferralId = "ref123",
        };

        // Assert
        fields.LastName.Should().Be("Doe");
        fields.FirstName.Should().Be("John");
        fields.AdditionalName.Should().Be("Middle");
        fields.AddressCountryCode.Should().Be("USA");
        fields.StateOrProvince.Should().Be("NY");
        fields.City.Should().Be("New York");
        fields.PostalCode.Should().Be("10001");
        fields.Address.Should().Be("123 Main St\nNew York, NY 10001");
        fields.MobileNumber.Should().Be("+14155552671");
        fields.MobileNumberFormat.Should().Be("E.164");
        fields.EmailAddress.Should().Be("john@example.com");
        fields.BirthPlace.Should().Be("New York, NY, USA");
        fields.BirthCountryCode.Should().Be("USA");
        fields.TaxId.Should().Be("123-45-6789");
        fields.TaxIdName.Should().Be("SSN");
        fields.EmployerName.Should().Be("Acme Corp");
        fields.EmployerAddress.Should().Be("456 Business Ave");
        fields.LanguageCode.Should().Be("en");
        fields.IdType.Should().Be("passport");
        fields.IdCountryCode.Should().Be("USA");
        fields.IdNumber.Should().Be("123456789");
        fields.IpAddress.Should().Be("192.168.1.1");
        fields.Sex.Should().Be("male");
        fields.ReferralId.Should().Be("ref123");
    }

    /// <summary>
    ///     Verifies that DateOnly properties can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void DateProperties_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var birthDate = new DateOnly(1990, 1, 15);
        var idIssueDate = new DateOnly(2020, 5, 10);
        var idExpirationDate = new DateOnly(2030, 5, 10);

        var fields = new NaturalPersonKycFields
        {
            BirthDate = birthDate,
            IdIssueDate = idIssueDate,
            IdExpirationDate = idExpirationDate,
        };

        // Assert
        fields.BirthDate.Should().Be(birthDate);
        fields.IdIssueDate.Should().Be(idIssueDate);
        fields.IdExpirationDate.Should().Be(idExpirationDate);
    }

    /// <summary>
    ///     Verifies that int properties can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void IntProperties_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var fields = new NaturalPersonKycFields
        {
            Occupation = 1234,
        };

        // Assert
        fields.Occupation.Should().Be(1234);
    }

    /// <summary>
    ///     Verifies that binary file properties can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void FileProperties_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var photoIdFront = new byte[] { 1, 2, 3, 4 };
        var photoIdBack = new byte[] { 5, 6, 7, 8 };
        var notaryApproval = new byte[] { 9, 10, 11, 12 };
        var photoProofResidence = new byte[] { 13, 14, 15, 16 };
        var proofOfIncome = new byte[] { 17, 18, 19, 20 };
        var proofOfLiveness = new byte[] { 21, 22, 23, 24 };

        var fields = new NaturalPersonKycFields
        {
            PhotoIdFront = photoIdFront,
            PhotoIdBack = photoIdBack,
            NotaryApprovalOfPhotoId = notaryApproval,
            PhotoProofResidence = photoProofResidence,
            ProofOfIncome = proofOfIncome,
            ProofOfLiveness = proofOfLiveness,
        };

        // Assert
        fields.PhotoIdFront.Should().BeEquivalentTo(photoIdFront);
        fields.PhotoIdBack.Should().BeEquivalentTo(photoIdBack);
        fields.NotaryApprovalOfPhotoId.Should().BeEquivalentTo(notaryApproval);
        fields.PhotoProofResidence.Should().BeEquivalentTo(photoProofResidence);
        fields.ProofOfIncome.Should().BeEquivalentTo(proofOfIncome);
        fields.ProofOfLiveness.Should().BeEquivalentTo(proofOfLiveness);
    }

    /// <summary>
    ///     Verifies that GetFields returns empty dictionary when no fields are set.
    /// </summary>
    [TestMethod]
    public void GetFields_WithNoFieldsSet_ReturnsEmptyDictionary()
    {
        // Arrange
        var fields = new NaturalPersonKycFields();

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
        var fields = new NaturalPersonKycFields
        {
            LastName = "Doe",
            FirstName = "John",
            AdditionalName = "Middle",
            AddressCountryCode = "USA",
            StateOrProvince = "NY",
            City = "New York",
            PostalCode = "10001",
            Address = "123 Main St",
            MobileNumber = "+14155552671",
            MobileNumberFormat = "E.164",
            EmailAddress = "john@example.com",
            BirthPlace = "New York",
            BirthCountryCode = "USA",
            TaxId = "123-45-6789",
            TaxIdName = "SSN",
            EmployerName = "Acme Corp",
            EmployerAddress = "456 Business Ave",
            LanguageCode = "en",
            IdType = "passport",
            IdCountryCode = "USA",
            IdNumber = "123456789",
            IpAddress = "192.168.1.1",
            Sex = "male",
            ReferralId = "ref123",
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().ContainKey(NaturalPersonKycFields.LastNameFieldKey).WhoseValue.Should().Be("Doe");
        result.Should().ContainKey(NaturalPersonKycFields.FirstNameFieldKey).WhoseValue.Should().Be("John");
        result.Should().ContainKey(NaturalPersonKycFields.AdditionalNameFieldKey).WhoseValue.Should().Be("Middle");
        result.Should().ContainKey(NaturalPersonKycFields.AddressCountryCodeFieldKey).WhoseValue.Should().Be("USA");
        result.Should().ContainKey(NaturalPersonKycFields.StateOrProvinceFieldKey).WhoseValue.Should().Be("NY");
        result.Should().ContainKey(NaturalPersonKycFields.CityFieldKey).WhoseValue.Should().Be("New York");
        result.Should().ContainKey(NaturalPersonKycFields.PostalCodeFieldKey).WhoseValue.Should().Be("10001");
        result.Should().ContainKey(NaturalPersonKycFields.AddressFieldKey).WhoseValue.Should().Be("123 Main St");
        result.Should().ContainKey(NaturalPersonKycFields.MobileNumberFieldKey).WhoseValue.Should().Be("+14155552671");
        result.Should().ContainKey(NaturalPersonKycFields.MobileNumberFormatFieldKey).WhoseValue.Should().Be("E.164");
        result.Should().ContainKey(NaturalPersonKycFields.EmailAddressFieldKey).WhoseValue.Should()
            .Be("john@example.com");
        result.Should().ContainKey(NaturalPersonKycFields.BirthPlaceFieldKey).WhoseValue.Should().Be("New York");
        result.Should().ContainKey(NaturalPersonKycFields.BirthCountryCodeFieldKey).WhoseValue.Should().Be("USA");
        result.Should().ContainKey(NaturalPersonKycFields.TaxIdFieldKey).WhoseValue.Should().Be("123-45-6789");
        result.Should().ContainKey(NaturalPersonKycFields.TaxIdNameFieldKey).WhoseValue.Should().Be("SSN");
        result.Should().ContainKey(NaturalPersonKycFields.EmployerNameFieldKey).WhoseValue.Should().Be("Acme Corp");
        result.Should().ContainKey(NaturalPersonKycFields.EmployerAddressFieldKey).WhoseValue.Should()
            .Be("456 Business Ave");
        result.Should().ContainKey(NaturalPersonKycFields.LanguageCodeFieldKey).WhoseValue.Should().Be("en");
        result.Should().ContainKey(NaturalPersonKycFields.IdTypeFieldKey).WhoseValue.Should().Be("passport");
        result.Should().ContainKey(NaturalPersonKycFields.IdCountryCodeFieldKey).WhoseValue.Should().Be("USA");
        result.Should().ContainKey(NaturalPersonKycFields.IdNumberFieldKey).WhoseValue.Should().Be("123456789");
        result.Should().ContainKey(NaturalPersonKycFields.IpAddressFieldKey).WhoseValue.Should().Be("192.168.1.1");
        result.Should().ContainKey(NaturalPersonKycFields.SexFieldKey).WhoseValue.Should().Be("male");
        result.Should().ContainKey(NaturalPersonKycFields.ReferralIdFieldKey).WhoseValue.Should().Be("ref123");
    }

    /// <summary>
    ///     Verifies that GetFields formats DateOnly values as ISO 8601 date-only strings.
    /// </summary>
    [TestMethod]
    public void GetFields_WithDateFields_FormatsAsIso8601DateOnly()
    {
        // Arrange
        var fields = new NaturalPersonKycFields
        {
            BirthDate = new DateOnly(1990, 1, 15),
            IdIssueDate = new DateOnly(2020, 5, 10),
            IdExpirationDate = new DateOnly(2030, 5, 10),
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().ContainKey(NaturalPersonKycFields.BirthDateFieldKey).WhoseValue.Should().Be("1990-01-15");
        result.Should().ContainKey(NaturalPersonKycFields.IdIssueDateFieldKey).WhoseValue.Should().Be("2020-05-10");
        result.Should().ContainKey(NaturalPersonKycFields.IdExpirationDateFieldKey).WhoseValue.Should()
            .Be("2030-05-10");
    }

    /// <summary>
    ///     Verifies that GetFields formats int values as strings.
    /// </summary>
    [TestMethod]
    public void GetFields_WithIntFields_FormatsAsString()
    {
        // Arrange
        var fields = new NaturalPersonKycFields
        {
            Occupation = 1234,
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().ContainKey(NaturalPersonKycFields.OccupationFieldKey).WhoseValue.Should().Be("1234");
    }

    /// <summary>
    ///     Verifies that GetFields includes nested FinancialAccount fields.
    /// </summary>
    [TestMethod]
    public void GetFields_WithFinancialAccount_IncludesNestedFields()
    {
        // Arrange
        var fields = new NaturalPersonKycFields
        {
            FirstName = "John",
            LastName = "Doe",
            FinancialAccount = new FinancialAccountKycFields
            {
                BankName = "Test Bank",
                BankAccountNumber = "1234567890",
            },
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().ContainKey(NaturalPersonKycFields.FirstNameFieldKey);
        result.Should().ContainKey(NaturalPersonKycFields.LastNameFieldKey);
        result.Should().ContainKey(FinancialAccountKycFields.BankNameFieldKey).WhoseValue.Should().Be("Test Bank");
        result.Should().ContainKey(FinancialAccountKycFields.BankAccountNumberFieldKey).WhoseValue.Should()
            .Be("1234567890");
    }

    /// <summary>
    ///     Verifies that GetFields includes nested Card fields.
    /// </summary>
    [TestMethod]
    public void GetFields_WithCard_IncludesNestedFields()
    {
        // Arrange
        var fields = new NaturalPersonKycFields
        {
            FirstName = "John",
            LastName = "Doe",
            Card = new CardKycFields
            {
                Number = "4111111111111111",
                ExpirationDate = "29-11",
            },
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().ContainKey(NaturalPersonKycFields.FirstNameFieldKey);
        result.Should().ContainKey(NaturalPersonKycFields.LastNameFieldKey);
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
        var fields = new NaturalPersonKycFields();

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
        var photoIdFront = new byte[] { 1, 2, 3 };
        var photoIdBack = new byte[] { 4, 5, 6 };
        var notaryApproval = new byte[] { 7, 8, 9 };
        var photoProofResidence = new byte[] { 10, 11, 12 };
        var proofOfIncome = new byte[] { 13, 14, 15 };
        var proofOfLiveness = new byte[] { 16, 17, 18 };

        var fields = new NaturalPersonKycFields
        {
            PhotoIdFront = photoIdFront,
            PhotoIdBack = photoIdBack,
            NotaryApprovalOfPhotoId = notaryApproval,
            PhotoProofResidence = photoProofResidence,
            ProofOfIncome = proofOfIncome,
            ProofOfLiveness = proofOfLiveness,
        };

        // Act
        var result = fields.GetFiles();

        // Assert
        result.Should().HaveCount(6);
        result.Should().ContainKey(NaturalPersonKycFields.PhotoIdFrontFileKey).WhoseValue.Should()
            .BeEquivalentTo(photoIdFront);
        result.Should().ContainKey(NaturalPersonKycFields.PhotoIdBackFileKey).WhoseValue.Should()
            .BeEquivalentTo(photoIdBack);
        result.Should().ContainKey(NaturalPersonKycFields.NotaryApprovalOfPhotoIdFileKey).WhoseValue.Should()
            .BeEquivalentTo(notaryApproval);
        result.Should().ContainKey(NaturalPersonKycFields.PhotoProofResidenceFileKey).WhoseValue.Should()
            .BeEquivalentTo(photoProofResidence);
        result.Should().ContainKey(NaturalPersonKycFields.ProofOfIncomeFileKey).WhoseValue.Should()
            .BeEquivalentTo(proofOfIncome);
        result.Should().ContainKey(NaturalPersonKycFields.ProofOfLivenessFileKey).WhoseValue.Should()
            .BeEquivalentTo(proofOfLiveness);
    }

    /// <summary>
    ///     Verifies that GetFiles only includes set files, excluding null values.
    /// </summary>
    [TestMethod]
    public void GetFiles_WithPartialFilesSet_ReturnsOnlySetFiles()
    {
        // Arrange
        var photoIdFront = new byte[] { 1, 2, 3 };
        var fields = new NaturalPersonKycFields
        {
            PhotoIdFront = photoIdFront,
        };

        // Act
        var result = fields.GetFiles();

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(NaturalPersonKycFields.PhotoIdFrontFileKey);
        result.Should().NotContainKey(NaturalPersonKycFields.PhotoIdBackFileKey);
        result.Should().NotContainKey(NaturalPersonKycFields.NotaryApprovalOfPhotoIdFileKey);
    }

    /// <summary>
    ///     Verifies that field key constants match SEP-9 specification.
    /// </summary>
    [TestMethod]
    public void FieldKeyConstants_MatchSep9Specification()
    {
        // Assert
        NaturalPersonKycFields.LastNameFieldKey.Should().Be("last_name");
        NaturalPersonKycFields.FamilyNameFieldKey.Should().Be("family_name");
        NaturalPersonKycFields.FirstNameFieldKey.Should().Be("first_name");
        NaturalPersonKycFields.GivenNameFieldKey.Should().Be("given_name");
        NaturalPersonKycFields.AdditionalNameFieldKey.Should().Be("additional_name");
        NaturalPersonKycFields.AddressCountryCodeFieldKey.Should().Be("address_country_code");
        NaturalPersonKycFields.StateOrProvinceFieldKey.Should().Be("state_or_province");
        NaturalPersonKycFields.CityFieldKey.Should().Be("city");
        NaturalPersonKycFields.PostalCodeFieldKey.Should().Be("postal_code");
        NaturalPersonKycFields.AddressFieldKey.Should().Be("address");
        NaturalPersonKycFields.MobileNumberFieldKey.Should().Be("mobile_number");
        NaturalPersonKycFields.MobileNumberFormatFieldKey.Should().Be("mobile_number_format");
        NaturalPersonKycFields.EmailAddressFieldKey.Should().Be("email_address");
        NaturalPersonKycFields.BirthDateFieldKey.Should().Be("birth_date");
        NaturalPersonKycFields.BirthPlaceFieldKey.Should().Be("birth_place");
        NaturalPersonKycFields.BirthCountryCodeFieldKey.Should().Be("birth_country_code");
        NaturalPersonKycFields.TaxIdFieldKey.Should().Be("tax_id");
        NaturalPersonKycFields.TaxIdNameFieldKey.Should().Be("tax_id_name");
        NaturalPersonKycFields.OccupationFieldKey.Should().Be("occupation");
        NaturalPersonKycFields.EmployerNameFieldKey.Should().Be("employer_name");
        NaturalPersonKycFields.EmployerAddressFieldKey.Should().Be("employer_address");
        NaturalPersonKycFields.LanguageCodeFieldKey.Should().Be("language_code");
        NaturalPersonKycFields.IdTypeFieldKey.Should().Be("id_type");
        NaturalPersonKycFields.IdCountryCodeFieldKey.Should().Be("id_country_code");
        NaturalPersonKycFields.IdIssueDateFieldKey.Should().Be("id_issue_date");
        NaturalPersonKycFields.IdExpirationDateFieldKey.Should().Be("id_expiration_date");
        NaturalPersonKycFields.IdNumberFieldKey.Should().Be("id_number");
        NaturalPersonKycFields.IpAddressFieldKey.Should().Be("ip_address");
        NaturalPersonKycFields.SexFieldKey.Should().Be("sex");
        NaturalPersonKycFields.ReferralIdFieldKey.Should().Be("referral_id");
    }

    /// <summary>
    ///     Verifies that file key constants match SEP-9 specification.
    /// </summary>
    [TestMethod]
    public void FileKeyConstants_MatchSep9Specification()
    {
        // Assert
        NaturalPersonKycFields.PhotoIdFrontFileKey.Should().Be("photo_id_front");
        NaturalPersonKycFields.PhotoIdBackFileKey.Should().Be("photo_id_back");
        NaturalPersonKycFields.NotaryApprovalOfPhotoIdFileKey.Should().Be("notary_approval_of_photo_id");
        NaturalPersonKycFields.PhotoProofResidenceFileKey.Should().Be("photo_proof_residence");
        NaturalPersonKycFields.ProofOfIncomeFileKey.Should().Be("proof_of_income");
        NaturalPersonKycFields.ProofOfLivenessFileKey.Should().Be("proof_of_liveness");
    }
}