using System;
using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0009;

/// <summary>
///     KYC fields for organizations (businesses).
///     Contains business entity identification information for corporate customers.
///     All field keys are prefixed with "organization."
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0009.md">SEP-0009</a>
/// </summary>
/// <remarks>
///     <para>
///         This record contains <c>byte[]</c> properties for file attachments (e.g., <see cref="PhotoIncorporationDoc" />,
///         <see cref="PhotoProofAddress" />). Note that record equality for <c>byte[]</c> properties compares
///         by reference, not by content. Two instances with identical file content but different array references
///         will not be considered equal. This is standard C# behavior for array equality.
///     </para>
///     <para>
///         If content-based equality is required for file attachments, consider overriding <c>Equals</c> and
///         <c>GetHashCode</c>, or use <c>ImmutableArray&lt;byte&gt;</c> instead of <c>byte[]</c>.
///     </para>
/// </remarks>
public sealed record OrganizationKycFields
{
    // Field keys
    public const string KeyPrefix = "organization.";
    public const string NameFieldKey = KeyPrefix + "name";
    public const string VatNumberFieldKey = KeyPrefix + "VAT_number";
    public const string RegistrationNumberFieldKey = KeyPrefix + "registration_number";
    public const string RegistrationDateFieldKey = KeyPrefix + "registration_date";
    public const string RegisteredAddressFieldKey = KeyPrefix + "registered_address";
    public const string NumberOfShareholdersFieldKey = KeyPrefix + "number_of_shareholders";
    public const string ShareholderNameFieldKey = KeyPrefix + "shareholder_name";
    public const string AddressCountryCodeFieldKey = KeyPrefix + "address_country_code";
    public const string StateOrProvinceFieldKey = KeyPrefix + "state_or_province";
    public const string CityFieldKey = KeyPrefix + "city";
    public const string PostalCodeFieldKey = KeyPrefix + "postal_code";
    public const string DirectorNameFieldKey = KeyPrefix + "director_name";
    public const string WebsiteFieldKey = KeyPrefix + "website";
    public const string EmailFieldKey = KeyPrefix + "email";
    public const string PhoneFieldKey = KeyPrefix + "phone";

    // File keys
    public const string PhotoIncorporationDocFileKey = KeyPrefix + "photo_incorporation_doc";
    public const string PhotoProofAddressFileKey = KeyPrefix + "photo_proof_address";

    /// <summary>
    ///     Full organization name as on the incorporation papers
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     Organization VAT number
    /// </summary>
    public string? VatNumber { get; init; }

    /// <summary>
    ///     Organization registration number
    /// </summary>
    public string? RegistrationNumber { get; init; }

    /// <summary>
    ///     Date the organization was registered
    /// </summary>
    public DateOnly? RegistrationDate { get; init; }

    /// <summary>
    ///     Organization registered address
    /// </summary>
    public string? RegisteredAddress { get; init; }

    /// <summary>
    ///     Organization shareholder number
    /// </summary>
    public int? NumberOfShareholders { get; init; }

    /// <summary>
    ///     Can be an organization or a person and should be queried recursively up to the ultimate beneficial owners
    ///     (with KYC information for natural persons such as above)
    /// </summary>
    public string? ShareholderName { get; init; }

    /// <summary>
    ///     Image of incorporation documents
    /// </summary>
    public byte[]? PhotoIncorporationDoc { get; init; }

    /// <summary>
    ///     Image of a utility bill, bank statement with the organization's name and address
    /// </summary>
    public byte[]? PhotoProofAddress { get; init; }

    /// <summary>
    ///     Country code for current address
    /// </summary>
    public string? AddressCountryCode { get; init; }

    /// <summary>
    ///     Name of state/province/region/prefecture
    /// </summary>
    public string? StateOrProvince { get; init; }

    /// <summary>
    ///     Name of city/town
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    ///     Postal or other code identifying organization's locale
    /// </summary>
    public string? PostalCode { get; init; }

    /// <summary>
    ///     Organization registered managing director
    /// </summary>
    public string? DirectorName { get; init; }

    /// <summary>
    ///     Organization website
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    ///     Organization contact email
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    ///     Organization contact phone
    /// </summary>
    public string? Phone { get; init; }

    /// <summary>
    ///     Financial Account Fields
    /// </summary>
    public FinancialAccountKycFields? FinancialAccount { get; init; }

    /// <summary>
    ///     Card Fields
    /// </summary>
    public CardKycFields? Card { get; init; }

    /// <summary>
    ///     Converts all organization KYC fields to a map of field names to values for SEP-9 submission.
    /// </summary>
    /// <returns>Dictionary containing all non-null field values</returns>
    internal IReadOnlyDictionary<string, string> GetFields()
    {
        var result = new Dictionary<string, string>();
        if (Name is not null)
        {
            result[NameFieldKey] = Name;
        }
        if (VatNumber is not null)
        {
            result[VatNumberFieldKey] = VatNumber;
        }
        if (RegistrationNumber is not null)
        {
            result[RegistrationNumberFieldKey] = RegistrationNumber;
        }
        if (RegistrationDate.HasValue)
        {
            result[RegistrationDateFieldKey] = RegistrationDate.Value.ToString("yyyy-MM-dd");
        }
        if (RegisteredAddress is not null)
        {
            result[RegisteredAddressFieldKey] = RegisteredAddress;
        }
        if (NumberOfShareholders.HasValue)
        {
            result[NumberOfShareholdersFieldKey] = NumberOfShareholders.Value.ToString();
        }
        if (ShareholderName is not null)
        {
            result[ShareholderNameFieldKey] = ShareholderName;
        }
        if (AddressCountryCode is not null)
        {
            result[AddressCountryCodeFieldKey] = AddressCountryCode;
        }
        if (StateOrProvince is not null)
        {
            result[StateOrProvinceFieldKey] = StateOrProvince;
        }
        if (City is not null)
        {
            result[CityFieldKey] = City;
        }
        if (PostalCode is not null)
        {
            result[PostalCodeFieldKey] = PostalCode;
        }
        if (DirectorName is not null)
        {
            result[DirectorNameFieldKey] = DirectorName;
        }
        if (Website is not null)
        {
            result[WebsiteFieldKey] = Website;
        }
        if (Email is not null)
        {
            result[EmailFieldKey] = Email;
        }
        if (Phone is not null)
        {
            result[PhoneFieldKey] = Phone;
        }
        if (FinancialAccount is not null)
        {
            foreach (var kvp in FinancialAccount.GetFields(KeyPrefix))
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        if (Card is not null)
        {
            foreach (var kvp in Card.GetFields())
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        return result;
    }

    /// <summary>
    ///     Converts all organization KYC file attachments to a map for SEP-9 submission.
    /// </summary>
    /// <returns>Dictionary containing all non-null file values</returns>
    public IReadOnlyDictionary<string, byte[]> GetFiles()
    {
        var result = new Dictionary<string, byte[]>();
        if (PhotoIncorporationDoc is not null)
        {
            result[PhotoIncorporationDocFileKey] = PhotoIncorporationDoc;
        }
        if (PhotoProofAddress is not null)
        {
            result[PhotoProofAddressFileKey] = PhotoProofAddress;
        }
        return result;
    }
}