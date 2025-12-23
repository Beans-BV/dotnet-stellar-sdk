using System;
using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0009;

/// <summary>
///     KYC fields for natural persons (individuals).
///     Contains personal identification information for individual customers.
///     Fields follow international standards (ISO 3166, ISO 639, E.164) where applicable.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0009.md">SEP-0009</a>
/// </summary>
/// <remarks>
///     <para>
///         This record contains <c>byte[]</c> properties for file attachments (e.g., <see cref="PhotoIdFront" />,
///         <see cref="PhotoIdBack" />, etc.). Note that record equality for <c>byte[]</c> properties compares
///         by reference, not by content. Two instances with identical file content but different array references
///         will not be considered equal. This is standard C# behavior for array equality.
///     </para>
///     <para>
///         If content-based equality is required for file attachments, consider overriding <c>Equals</c> and
///         <c>GetHashCode</c>, or use <c>ImmutableArray&lt;byte&gt;</c> instead of <c>byte[]</c>.
///     </para>
/// </remarks>
public sealed record NaturalPersonKycFields
{
    // Field keys
    public const string LastNameFieldKey = "last_name";
    public const string FamilyNameFieldKey = "family_name";
    public const string FirstNameFieldKey = "first_name";
    public const string GivenNameFieldKey = "given_name";
    public const string AdditionalNameFieldKey = "additional_name";
    public const string AddressCountryCodeFieldKey = "address_country_code";
    public const string StateOrProvinceFieldKey = "state_or_province";
    public const string CityFieldKey = "city";
    public const string PostalCodeFieldKey = "postal_code";
    public const string AddressFieldKey = "address";
    public const string MobileNumberFieldKey = "mobile_number";
    public const string MobileNumberFormatFieldKey = "mobile_number_format";
    public const string EmailAddressFieldKey = "email_address";
    public const string BirthDateFieldKey = "birth_date";
    public const string BirthPlaceFieldKey = "birth_place";
    public const string BirthCountryCodeFieldKey = "birth_country_code";
    public const string TaxIdFieldKey = "tax_id";
    public const string TaxIdNameFieldKey = "tax_id_name";
    public const string OccupationFieldKey = "occupation";
    public const string EmployerNameFieldKey = "employer_name";
    public const string EmployerAddressFieldKey = "employer_address";
    public const string LanguageCodeFieldKey = "language_code";
    public const string IdTypeFieldKey = "id_type";
    public const string IdCountryCodeFieldKey = "id_country_code";
    public const string IdIssueDateFieldKey = "id_issue_date";
    public const string IdExpirationDateFieldKey = "id_expiration_date";
    public const string IdNumberFieldKey = "id_number";
    public const string IpAddressFieldKey = "ip_address";
    public const string SexFieldKey = "sex";
    public const string ReferralIdFieldKey = "referral_id";

    // File keys
    public const string PhotoIdFrontFileKey = "photo_id_front";
    public const string PhotoIdBackFileKey = "photo_id_back";
    public const string NotaryApprovalOfPhotoIdFileKey = "notary_approval_of_photo_id";
    public const string PhotoProofResidenceFileKey = "photo_proof_residence";
    public const string ProofOfIncomeFileKey = "proof_of_income";
    public const string ProofOfLivenessFileKey = "proof_of_liveness";

    /// <summary>
    ///     Family or last name
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    ///     Given or first name
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    ///     Middle name or other additional name
    /// </summary>
    public string? AdditionalName { get; init; }

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
    ///     Postal or other code identifying user's locale
    /// </summary>
    public string? PostalCode { get; init; }

    /// <summary>
    ///     Entire address (country, state, postal code, street address, etc...) as a multi-line string
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    ///     Mobile phone number with country code, in E.164 format
    /// </summary>
    public string? MobileNumber { get; init; }

    /// <summary>
    ///     Expected format of the mobile_number field. E.g.: E.164, hash, etc...
    ///     In case this field is not specified, receiver will assume it's in E.164 format
    /// </summary>
    public string? MobileNumberFormat { get; init; }

    /// <summary>
    ///     Email address
    /// </summary>
    public string? EmailAddress { get; init; }

    /// <summary>
    ///     Date of birth, e.g. 1976-07-04
    /// </summary>
    public DateOnly? BirthDate { get; init; }

    /// <summary>
    ///     Place of birth (city, state, country; as on passport)
    /// </summary>
    public string? BirthPlace { get; init; }

    /// <summary>
    ///     ISO Code of country of birth ISO 3166-1 alpha-3
    /// </summary>
    public string? BirthCountryCode { get; init; }

    /// <summary>
    ///     Tax identifier of user in their country (social security number in US)
    /// </summary>
    public string? TaxId { get; init; }

    /// <summary>
    ///     Name of the tax ID (SSN or ITIN in the US)
    /// </summary>
    public string? TaxIdName { get; init; }

    /// <summary>
    ///     Occupation ISCO code
    /// </summary>
    public int? Occupation { get; init; }

    /// <summary>
    ///     Name of employer
    /// </summary>
    public string? EmployerName { get; init; }

    /// <summary>
    ///     Address of employer
    /// </summary>
    public string? EmployerAddress { get; init; }

    /// <summary>
    ///     Primary language ISO 639-1
    /// </summary>
    public string? LanguageCode { get; init; }

    /// <summary>
    ///     passport, drivers_license, id_card, etc...
    /// </summary>
    public string? IdType { get; init; }

    /// <summary>
    ///     Country issuing passport or photo ID as ISO 3166-1 alpha-3 code
    /// </summary>
    public string? IdCountryCode { get; init; }

    /// <summary>
    ///     ID issue date
    /// </summary>
    public DateOnly? IdIssueDate { get; init; }

    /// <summary>
    ///     ID expiration date
    /// </summary>
    public DateOnly? IdExpirationDate { get; init; }

    /// <summary>
    ///     Passport or ID number
    /// </summary>
    public string? IdNumber { get; init; }

    /// <summary>
    ///     Image of front of user's photo ID or passport
    /// </summary>
    public byte[]? PhotoIdFront { get; init; }

    /// <summary>
    ///     Image of back of user's photo ID or passport
    /// </summary>
    public byte[]? PhotoIdBack { get; init; }

    /// <summary>
    ///     Image of notary's approval of photo ID or passport
    /// </summary>
    public byte[]? NotaryApprovalOfPhotoId { get; init; }

    /// <summary>
    ///     IP address of customer's computer
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    ///     Image of a utility bill, bank statement or similar with the user's name and address
    /// </summary>
    public byte[]? PhotoProofResidence { get; init; }

    /// <summary>
    ///     male, female, or other
    /// </summary>
    public string? Sex { get; init; }

    /// <summary>
    ///     Image of user's proof of income document
    /// </summary>
    public byte[]? ProofOfIncome { get; init; }

    /// <summary>
    ///     Video or image file of user as a liveness proof
    /// </summary>
    public byte[]? ProofOfLiveness { get; init; }

    /// <summary>
    ///     User's origin (such as an id in another application) or a referral code
    /// </summary>
    public string? ReferralId { get; init; }

    /// <summary>
    ///     Financial Account Fields
    /// </summary>
    public FinancialAccountKycFields? FinancialAccount { get; init; }

    /// <summary>
    ///     Card Fields
    /// </summary>
    public CardKycFields? Card { get; init; }

    /// <summary>
    ///     Converts all natural person KYC fields to a map of field names to values for SEP-9 submission.
    /// </summary>
    /// <returns>Dictionary containing all non-null field values</returns>
    public IReadOnlyDictionary<string, string> GetFields()
    {
        var result = new Dictionary<string, string>();
        if (LastName is not null)
        {
            result[LastNameFieldKey] = LastName;
        }
        if (FirstName is not null)
        {
            result[FirstNameFieldKey] = FirstName;
        }
        if (AdditionalName is not null)
        {
            result[AdditionalNameFieldKey] = AdditionalName;
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
        if (Address is not null)
        {
            result[AddressFieldKey] = Address;
        }
        if (MobileNumber is not null)
        {
            result[MobileNumberFieldKey] = MobileNumber;
        }
        if (MobileNumberFormat is not null)
        {
            result[MobileNumberFormatFieldKey] = MobileNumberFormat;
        }
        if (EmailAddress is not null)
        {
            result[EmailAddressFieldKey] = EmailAddress;
        }
        if (BirthDate.HasValue)
        {
            result[BirthDateFieldKey] = BirthDate.Value.ToString("yyyy-MM-dd");
        }
        if (BirthPlace is not null)
        {
            result[BirthPlaceFieldKey] = BirthPlace;
        }
        if (BirthCountryCode is not null)
        {
            result[BirthCountryCodeFieldKey] = BirthCountryCode;
        }
        if (TaxId is not null)
        {
            result[TaxIdFieldKey] = TaxId;
        }
        if (TaxIdName is not null)
        {
            result[TaxIdNameFieldKey] = TaxIdName;
        }
        if (Occupation.HasValue)
        {
            result[OccupationFieldKey] = Occupation.Value.ToString();
        }
        if (EmployerName is not null)
        {
            result[EmployerNameFieldKey] = EmployerName;
        }
        if (EmployerAddress is not null)
        {
            result[EmployerAddressFieldKey] = EmployerAddress;
        }
        if (LanguageCode is not null)
        {
            result[LanguageCodeFieldKey] = LanguageCode;
        }
        if (IdType is not null)
        {
            result[IdTypeFieldKey] = IdType;
        }
        if (IdCountryCode is not null)
        {
            result[IdCountryCodeFieldKey] = IdCountryCode;
        }
        if (IdIssueDate.HasValue)
        {
            result[IdIssueDateFieldKey] = IdIssueDate.Value.ToString("yyyy-MM-dd");
        }
        if (IdExpirationDate.HasValue)
        {
            result[IdExpirationDateFieldKey] = IdExpirationDate.Value.ToString("yyyy-MM-dd");
        }
        if (IdNumber is not null)
        {
            result[IdNumberFieldKey] = IdNumber;
        }
        if (IpAddress is not null)
        {
            result[IpAddressFieldKey] = IpAddress;
        }
        if (Sex is not null)
        {
            result[SexFieldKey] = Sex;
        }
        if (ReferralId is not null)
        {
            result[ReferralIdFieldKey] = ReferralId;
        }
        if (FinancialAccount is not null)
        {
            foreach (var kvp in FinancialAccount.GetFields())
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
    ///     Converts all natural person KYC file attachments to a map for SEP-9 submission.
    /// </summary>
    /// <returns>Dictionary containing all non-null file values</returns>
    public IReadOnlyDictionary<string, byte[]> GetFiles()
    {
        var result = new Dictionary<string, byte[]>();
        if (PhotoIdFront is not null)
        {
            result[PhotoIdFrontFileKey] = PhotoIdFront;
        }
        if (PhotoIdBack is not null)
        {
            result[PhotoIdBackFileKey] = PhotoIdBack;
        }
        if (NotaryApprovalOfPhotoId is not null)
        {
            result[NotaryApprovalOfPhotoIdFileKey] = NotaryApprovalOfPhotoId;
        }
        if (PhotoProofResidence is not null)
        {
            result[PhotoProofResidenceFileKey] = PhotoProofResidence;
        }
        if (ProofOfIncome is not null)
        {
            result[ProofOfIncomeFileKey] = ProofOfIncome;
        }
        if (ProofOfLiveness is not null)
        {
            result[ProofOfLivenessFileKey] = ProofOfLiveness;
        }
        return result;
    }
}

