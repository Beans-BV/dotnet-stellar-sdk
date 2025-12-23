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
}

