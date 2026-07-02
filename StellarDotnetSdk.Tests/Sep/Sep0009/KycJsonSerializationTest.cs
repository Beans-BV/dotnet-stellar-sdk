using System;
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

#if !TEST_SDK_NETSTANDARD21
    [TestMethod]
    public void NullableDateOnlyJsonConverter_RejectsInvalidDateFormat()
    {
        var json = """{"birthDate":"15-01-1990"}""";

        var act = () => JsonSerializer.Deserialize<NaturalPersonKycFields>(json, KycJsonOptions.Default);

        act.Should().Throw<JsonException>();
    }
#endif

#if TEST_SDK_NETSTANDARD21
    [TestMethod]
    public void IsoDateStringJsonConverter_RejectsInvalidDateFormat_OnRead()
    {
        var json = """{"birthDate":"15-01-1990"}""";

        var act = () => JsonSerializer.Deserialize<NaturalPersonKycFields>(json, KycJsonOptions.Default);

        act.Should().Throw<JsonException>();
    }

    [TestMethod]
    public void IsoDateStringJsonConverter_RejectsInvalidRegistrationDate_OnRead()
    {
        var json = """{"registrationDate":"June 1, 2010"}""";

        var act = () => JsonSerializer.Deserialize<OrganizationKycFields>(json, KycJsonOptions.Default);

        act.Should().Throw<JsonException>();
    }

    [TestMethod]
    public void IsoDateStringJsonConverter_RejectsInvalidDateFormat_OnWrite()
    {
        var fields = new NaturalPersonKycFields { BirthDate = "6/9/2026" };

        var act = () => JsonSerializer.Serialize(fields, KycJsonOptions.Default);

        act.Should().Throw<JsonException>();
    }

    [TestMethod]
    public void IsoDateStringJsonConverter_AllowsNullDates()
    {
        var fields = new NaturalPersonKycFields { FirstName = "John" };

        var json = JsonSerializer.Serialize(fields, KycJsonOptions.Default);
        var roundTrip = JsonSerializer.Deserialize<NaturalPersonKycFields>(json, KycJsonOptions.Default);

        roundTrip!.BirthDate.Should().BeNull();
    }
#endif
}
