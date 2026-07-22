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

    // The following run on every TFM (including netstandard2.1) — that is the point of referencing
    // System.Text.Json 10.x on all targets: KycJsonOptions.Default must enforce the same JSON guards
    // as JsonOptions.DefaultOptions everywhere, not just on net10.0.
    // (KycJsonOptions.Default is a public utility for consumers' own internal/persistence
    // serialization — SEP-0009 wire parsing goes through the typed field models, not these options.)
    [TestMethod]
    public void KycJsonOptions_Default_DisablesAllowDuplicateProperties()
    {
        KycJsonOptions.Default.AllowDuplicateProperties.Should().BeFalse();
    }

    [TestMethod]
    public void KycJsonOptions_Default_RespectNullableAnnotations_IsEnabled()
    {
        // The SEP-9 models can't exercise this behaviorally (every property on
        // NaturalPersonKycFields/OrganizationKycFields is nullable), so this asserts the flag and
        // KycJsonOptions_Deserialize_NullForNonNullableMember_ThrowsJsonException (below) asserts the
        // behavior via a synthetic non-nullable DTO.
        KycJsonOptions.Default.RespectNullableAnnotations.Should().BeTrue();
    }

    [TestMethod]
    public void KycJsonOptions_Deserialize_NullForNonNullableMember_ThrowsJsonException()
    {
        // Behavioral coverage for RespectNullableAnnotations = true through KycJsonOptions.Default:
        // an explicit null for a non-nullable member is rejected rather than silently written.
        var json = """{"value":null}""";

        var act = () => JsonSerializer.Deserialize<NonNullableProbe>(json, KycJsonOptions.Default);

        act.Should().Throw<JsonException>();
    }

#nullable enable
    private sealed class NonNullableProbe
    {
        public string Value { get; set; } = string.Empty;
    }
#nullable restore

    [TestMethod]
    public void KycJsonOptions_Deserialize_WithDuplicateProperties_ThrowsJsonException()
    {
        // A duplicated field must be rejected rather than silently taking the last value.
        var json = """{"firstName":"John","firstName":"Jane"}""";

        var act = () => JsonSerializer.Deserialize<NaturalPersonKycFields>(json, KycJsonOptions.Default);

        act.Should().Throw<JsonException>();
    }

    [TestMethod]
    public void KycJsonOptions_Deserialize_OrganizationWithDuplicateProperties_ThrowsJsonException()
    {
        // Same guard as the NaturalPersonKycFields test above, on the organization model.
        var json = """{"name":"Acme Corp","name":"Evil Corp"}""";

        var act = () => JsonSerializer.Deserialize<OrganizationKycFields>(json, KycJsonOptions.Default);

        act.Should().Throw<JsonException>();
    }

    [TestMethod]
    public void KycJsonOptions_Deserialize_WithCaseInsensitiveDuplicateProperties_ThrowsJsonException()
    {
        // PropertyNameCaseInsensitive = true makes these the same logical property, so the
        // duplicate guard must reject them despite the differing case (mirrors the equivalent
        // JsonOptions.DefaultOptions test).
        var json = """{"firstName":"John","FIRSTNAME":"Jane"}""";

        var act = () => JsonSerializer.Deserialize<NaturalPersonKycFields>(json, KycJsonOptions.Default);

        act.Should().Throw<JsonException>();
    }
}
