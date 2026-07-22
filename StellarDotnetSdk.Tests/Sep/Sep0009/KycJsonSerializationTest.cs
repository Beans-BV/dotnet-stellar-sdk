using System.Text.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Sep.Sep0009;
using StellarDotnetSdk.Tests.Sep.Sep0009.Fixtures;

namespace StellarDotnetSdk.Tests.Sep.Sep0009;

/// <summary>
///     JSON serialization tests for SEP-0009 KYC field types.
/// </summary>
[TestClass]
public class KycJsonSerializationTest
{
    [TestMethod]
    public void NaturalPersonKycFields_JsonRoundTrip_PreservesDateFields()
    {
        var json = """
                   {
                     "firstName": "John",
                     "birthDate": "1990-01-15",
                     "idIssueDate": "2020-05-10",
                     "idExpirationDate": "2030-05-10"
                   }
                   """;

        var fields = JsonSerializer.Deserialize<NaturalPersonKycFields>(json, KycJsonOptions.Default);

        fields.Should().NotBeNull();
        fields!.FirstName.Should().Be("John");
        fields.BirthDate.Should().Be(KycTestDates.BirthDate);
        fields.IdIssueDate.Should().Be(KycTestDates.IdIssueDate);
        fields.IdExpirationDate.Should().Be(KycTestDates.IdExpirationDate);

        var roundTrip = JsonSerializer.Serialize(fields, KycJsonOptions.Default);
        roundTrip.Should().Contain("\"birthDate\":\"1990-01-15\"");
        roundTrip.Should().Contain("\"idIssueDate\":\"2020-05-10\"");
        roundTrip.Should().Contain("\"idExpirationDate\":\"2030-05-10\"");
    }

    [TestMethod]
    public void OrganizationKycFields_JsonRoundTrip_PreservesRegistrationDate()
    {
        var json = """
                   {
                     "name": "Acme Corp",
                     "registrationDate": "2020-01-15"
                   }
                   """;

        var fields = JsonSerializer.Deserialize<OrganizationKycFields>(json, KycJsonOptions.Default);

        fields.Should().NotBeNull();
        fields!.Name.Should().Be("Acme Corp");
        fields.RegistrationDate.Should().Be(KycTestDates.RegistrationDateIso);

        var roundTrip = JsonSerializer.Serialize(fields, KycJsonOptions.Default);
        roundTrip.Should().Contain("\"registrationDate\":\"2020-01-15\"");
    }

    [TestMethod]
    public void StandardKycFields_JsonRoundTrip_PreservesNestedDates()
    {
        var json = """
                   {
                     "naturalPerson": {
                       "birthDate": "1985-12-25"
                     },
                     "organization": {
                       "registrationDate": "2010-06-01"
                     }
                   }
                   """;

        var fields = JsonSerializer.Deserialize<StandardKycFields>(json, KycJsonOptions.Default);

        fields.Should().NotBeNull();
        fields!.NaturalPerson!.BirthDate.Should().Be(KycTestDates.NestedBirthDate);
        fields.Organization!.RegistrationDate.Should().Be(KycTestDates.NestedRegistrationDate);
    }

    [TestMethod]
    public void JsonOptions_DefaultOptions_IsReadOnly()
    {
        JsonOptions.DefaultOptions.IsReadOnly.Should().BeTrue();
    }

    [TestMethod]
    public void KycJsonOptions_Default_IsReadOnly()
    {
        KycJsonOptions.Default.IsReadOnly.Should().BeTrue();
    }

    /// <summary>
    ///     Runs on every TFM leg and asserts the exact message so the wording stays identical across the
    ///     netstandard2.1 <c>IsoDateStringJsonConverter</c> and the net8.0/net10.0 DateOnly converters.
    /// </summary>
    [TestMethod]
    public void KycDateField_RejectsInvalidDateFormat_OnRead_WithSameMessageOnAllTfms()
    {
        var json = """{"birthDate":"15-01-1990"}""";

        var act = () => JsonSerializer.Deserialize<NaturalPersonKycFields>(json, KycJsonOptions.Default);

        act.Should().Throw<JsonException>()
            .WithMessage("Cannot convert JSON value '15-01-1990' to an ISO 8601 date. Expected format: yyyy-MM-dd.*");
    }

    [TestMethod]
    public void KycRegistrationDate_RejectsInvalidValue_OnRead_WithSameMessageOnAllTfms()
    {
        var json = """{"registrationDate":"June 1, 2010"}""";

        var act = () => JsonSerializer.Deserialize<OrganizationKycFields>(json, KycJsonOptions.Default);

        act.Should().Throw<JsonException>()
            .WithMessage("Cannot convert JSON value 'June 1, 2010' to an ISO 8601 date. Expected format: yyyy-MM-dd.*");
    }

    [TestMethod]
    public void KycDateFields_AllowNullDates()
    {
        var fields = new NaturalPersonKycFields { FirstName = "John" };

        var json = JsonSerializer.Serialize(fields, KycJsonOptions.Default);
        var roundTrip = JsonSerializer.Deserialize<NaturalPersonKycFields>(json, KycJsonOptions.Default);

        roundTrip!.BirthDate.Should().BeNull();
    }

#if TEST_SDK_NETSTANDARD21
    [TestMethod]
    public void IsoDateStringJsonConverter_RejectsInvalidDateFormat_OnWrite()
    {
        var fields = new NaturalPersonKycFields { BirthDate = "6/9/2026" };

        var act = () => JsonSerializer.Serialize(fields, KycJsonOptions.Default);

        act.Should().Throw<JsonException>()
            .WithMessage("Cannot convert JSON value '6/9/2026' to an ISO 8601 date. Expected format: yyyy-MM-dd.*");
    }
#endif
}
