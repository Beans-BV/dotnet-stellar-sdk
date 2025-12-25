using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Sep.Sep0009;

namespace StellarDotnetSdk.Tests.Sep.Sep0009;

/// <summary>
///     Tests for StandardKycFields class functionality.
/// </summary>
[TestClass]
public class StandardKycFieldsTest
{
    /// <summary>
    ///     Verifies that NaturalPerson property can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void NaturalPerson_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var naturalPerson = new NaturalPersonKycFields
        {
            FirstName = "John",
            LastName = "Doe"
        };
        var fields = new StandardKycFields
        {
            NaturalPerson = naturalPerson
        };

        // Act & Assert
        fields.NaturalPerson.Should().NotBeNull();
        fields.NaturalPerson!.FirstName.Should().Be("John");
        fields.NaturalPerson.LastName.Should().Be("Doe");
    }

    /// <summary>
    ///     Verifies that Organization property can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void Organization_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var organization = new OrganizationKycFields
        {
            Name = "Acme Corp",
            VatNumber = "VAT123456"
        };
        var fields = new StandardKycFields
        {
            Organization = organization
        };

        // Act & Assert
        fields.Organization.Should().NotBeNull();
        fields.Organization!.Name.Should().Be("Acme Corp");
        fields.Organization.VatNumber.Should().Be("VAT123456");
    }

    /// <summary>
    ///     Verifies that both NaturalPerson and Organization can be set simultaneously.
    /// </summary>
    [TestMethod]
    public void BothProperties_SetSimultaneously_WorkCorrectly()
    {
        // Arrange
        var naturalPerson = new NaturalPersonKycFields
        {
            FirstName = "John",
            LastName = "Doe"
        };
        var organization = new OrganizationKycFields
        {
            Name = "Acme Corp"
        };
        var fields = new StandardKycFields
        {
            NaturalPerson = naturalPerson,
            Organization = organization
        };

        // Act & Assert
        fields.NaturalPerson.Should().NotBeNull();
        fields.Organization.Should().NotBeNull();
        fields.NaturalPerson!.FirstName.Should().Be("John");
        fields.Organization!.Name.Should().Be("Acme Corp");
    }

    /// <summary>
    ///     Verifies that properties can be null.
    /// </summary>
    [TestMethod]
    public void Properties_CanBeNull()
    {
        // Arrange
        var fields = new StandardKycFields();

        // Act & Assert
        fields.NaturalPerson.Should().BeNull();
        fields.Organization.Should().BeNull();
    }

    /// <summary>
    ///     Verifies that NaturalPerson can be set to null using with expression.
    /// </summary>
    [TestMethod]
    public void NaturalPerson_CanBeSetToNull()
    {
        // Arrange
        var fields = new StandardKycFields
        {
            NaturalPerson = new NaturalPersonKycFields { FirstName = "John" }
        };

        // Act - Use with expression to create a new instance with null NaturalPerson
        var fieldsWithNull = fields with { NaturalPerson = null };

        // Assert
        fields.NaturalPerson.Should().NotBeNull(); // Original unchanged
        fieldsWithNull.NaturalPerson.Should().BeNull(); // New instance has null
    }

    /// <summary>
    ///     Verifies that Organization can be set to null using with expression.
    /// </summary>
    [TestMethod]
    public void Organization_CanBeSetToNull()
    {
        // Arrange
        var fields = new StandardKycFields
        {
            Organization = new OrganizationKycFields { Name = "Acme Corp" }
        };

        // Act - Use with expression to create a new instance with null Organization
        var fieldsWithNull = fields with { Organization = null };

        // Assert
        fields.Organization.Should().NotBeNull(); // Original unchanged
        fieldsWithNull.Organization.Should().BeNull(); // New instance has null
    }

    /// <summary>
    ///     Verifies that GetFields returns empty dictionary when both properties are null.
    /// </summary>
    [TestMethod]
    public void GetFields_WithBothPropertiesNull_ReturnsEmptyDictionary()
    {
        // Arrange
        var fields = new StandardKycFields();

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetFields includes fields from NaturalPerson when set.
    /// </summary>
    [TestMethod]
    public void GetFields_WithNaturalPersonSet_ReturnsNaturalPersonFields()
    {
        // Arrange
        var fields = new StandardKycFields
        {
            NaturalPerson = new NaturalPersonKycFields
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com"
            }
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey(NaturalPersonKycFields.FirstNameFieldKey).WhoseValue.Should().Be("John");
        result.Should().ContainKey(NaturalPersonKycFields.LastNameFieldKey).WhoseValue.Should().Be("Doe");
        result.Should().ContainKey(NaturalPersonKycFields.EmailAddressFieldKey).WhoseValue.Should().Be("john@example.com");
    }

    /// <summary>
    ///     Verifies that GetFields includes fields from Organization when set.
    /// </summary>
    [TestMethod]
    public void GetFields_WithOrganizationSet_ReturnsOrganizationFields()
    {
        // Arrange
        var fields = new StandardKycFields
        {
            Organization = new OrganizationKycFields
            {
                Name = "Acme Corp",
                VatNumber = "VAT123456",
                Email = "contact@acme.com"
            }
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey(OrganizationKycFields.NameFieldKey).WhoseValue.Should().Be("Acme Corp");
        result.Should().ContainKey(OrganizationKycFields.VatNumberFieldKey).WhoseValue.Should().Be("VAT123456");
        result.Should().ContainKey(OrganizationKycFields.EmailFieldKey).WhoseValue.Should().Be("contact@acme.com");
    }

    /// <summary>
    ///     Verifies that GetFields combines fields from both NaturalPerson and Organization when both are set.
    /// </summary>
    [TestMethod]
    public void GetFields_WithBothPropertiesSet_ReturnsCombinedFields()
    {
        // Arrange
        var fields = new StandardKycFields
        {
            NaturalPerson = new NaturalPersonKycFields
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john@example.com"
            },
            Organization = new OrganizationKycFields
            {
                Name = "Acme Corp",
                VatNumber = "VAT123456",
                Email = "contact@acme.com"
            }
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().NotBeNull();
        // Verify NaturalPerson fields are present
        result.Should().ContainKey(NaturalPersonKycFields.FirstNameFieldKey).WhoseValue.Should().Be("John");
        result.Should().ContainKey(NaturalPersonKycFields.LastNameFieldKey).WhoseValue.Should().Be("Doe");
        result.Should().ContainKey(NaturalPersonKycFields.EmailAddressFieldKey).WhoseValue.Should().Be("john@example.com");
        // Verify Organization fields are present
        result.Should().ContainKey(OrganizationKycFields.NameFieldKey).WhoseValue.Should().Be("Acme Corp");
        result.Should().ContainKey(OrganizationKycFields.VatNumberFieldKey).WhoseValue.Should().Be("VAT123456");
        result.Should().ContainKey(OrganizationKycFields.EmailFieldKey).WhoseValue.Should().Be("contact@acme.com");
        // Verify all fields are present
        result.Should().HaveCount(6);
    }
}

