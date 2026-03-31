using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0009;

/// <summary>
///     Payment card information for KYC verification.
///     Contains credit or debit card details for payment processing.
///     All field keys are prefixed with "card."
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0009.md">SEP-0009</a>
/// </summary>
public sealed record CardKycFields
{
    /// <summary>
    ///     Key prefix for all card fields (<c>"card."</c>) as defined in SEP-0009.
    /// </summary>
    public const string KeyPrefix = "card.";

    /// <summary>
    ///     Field key for the <c>card.number</c> field as defined in SEP-0009.
    /// </summary>
    public const string NumberFieldKey = KeyPrefix + "number";

    /// <summary>
    ///     Field key for the <c>card.expiration_date</c> field (YY-MM format) as defined in SEP-0009.
    /// </summary>
    public const string ExpirationDateFieldKey = KeyPrefix + "expiration_date";

    /// <summary>
    ///     Field key for the <c>card.cvc</c> field (digits on the back of the card) as defined in SEP-0009.
    /// </summary>
    public const string CvcFieldKey = KeyPrefix + "cvc";

    /// <summary>
    ///     Field key for the <c>card.holder_name</c> field as defined in SEP-0009.
    /// </summary>
    public const string HolderNameFieldKey = KeyPrefix + "holder_name";

    /// <summary>
    ///     Field key for the <c>card.network</c> field (e.g., Visa, Mastercard) as defined in SEP-0009.
    /// </summary>
    public const string NetworkFieldKey = KeyPrefix + "network";

    /// <summary>
    ///     Field key for the <c>card.postal_code</c> field (billing address postal code) as defined in SEP-0009.
    /// </summary>
    public const string PostalCodeFieldKey = KeyPrefix + "postal_code";

    /// <summary>
    ///     Field key for the <c>card.country_code</c> field (ISO 3166-1 alpha-2 code) as defined in SEP-0009.
    /// </summary>
    public const string CountryCodeFieldKey = KeyPrefix + "country_code";

    /// <summary>
    ///     Field key for the <c>card.state_or_province</c> field (ISO 3166-2 format) as defined in SEP-0009.
    /// </summary>
    public const string StateOrProvinceFieldKey = KeyPrefix + "state_or_province";

    /// <summary>
    ///     Field key for the <c>card.city</c> field as defined in SEP-0009.
    /// </summary>
    public const string CityFieldKey = KeyPrefix + "city";

    /// <summary>
    ///     Field key for the <c>card.address</c> field (full billing address) as defined in SEP-0009.
    /// </summary>
    public const string AddressFieldKey = KeyPrefix + "address";

    /// <summary>
    ///     Field key for the <c>card.token</c> field (external payment system token) as defined in SEP-0009.
    /// </summary>
    public const string TokenFieldKey = KeyPrefix + "token";

    /// <summary>
    ///     Card number
    /// </summary>
    public string? Number { get; init; }

    /// <summary>
    ///     Expiration month and year in YY-MM format (e.g. 29-11, November 2029)
    /// </summary>
    public string? ExpirationDate { get; init; }

    /// <summary>
    ///     CVC number (Digits on the back of the card)
    /// </summary>
    public string? Cvc { get; init; }

    /// <summary>
    ///     Name of the card holder
    /// </summary>
    public string? HolderName { get; init; }

    /// <summary>
    ///     Brand of the card/network it operates within (e.g. Visa, Mastercard, AmEx, etc.)
    /// </summary>
    public string? Network { get; init; }

    /// <summary>
    ///     Billing address postal code
    /// </summary>
    public string? PostalCode { get; init; }

    /// <summary>
    ///     Billing address country code in ISO 3166-1 alpha-2 code (e.g. US)
    /// </summary>
    public string? CountryCode { get; init; }

    /// <summary>
    ///     Name of state/province/region/prefecture in ISO 3166-2 format
    /// </summary>
    public string? StateOrProvince { get; init; }

    /// <summary>
    ///     Name of city/town
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    ///     Entire address (country, state, postal code, street address, etc...) as a multi-line string
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    ///     Token representation of the card in some external payment system (e.g. Stripe)
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    ///     Converts all card KYC fields to a map for SEP-9 submission.
    /// </summary>
    /// <returns>Dictionary containing all non-null field values</returns>
    internal IReadOnlyDictionary<string, string> GetFields()
    {
        var result = new Dictionary<string, string>();
        if (Number is not null)
        {
            result[NumberFieldKey] = Number;
        }
        if (ExpirationDate is not null)
        {
            result[ExpirationDateFieldKey] = ExpirationDate;
        }
        if (Cvc is not null)
        {
            result[CvcFieldKey] = Cvc;
        }
        if (HolderName is not null)
        {
            result[HolderNameFieldKey] = HolderName;
        }
        if (Network is not null)
        {
            result[NetworkFieldKey] = Network;
        }
        if (PostalCode is not null)
        {
            result[PostalCodeFieldKey] = PostalCode;
        }
        if (CountryCode is not null)
        {
            result[CountryCodeFieldKey] = CountryCode;
        }
        if (StateOrProvince is not null)
        {
            result[StateOrProvinceFieldKey] = StateOrProvince;
        }
        if (City is not null)
        {
            result[CityFieldKey] = City;
        }
        if (Address is not null)
        {
            result[AddressFieldKey] = Address;
        }
        if (Token is not null)
        {
            result[TokenFieldKey] = Token;
        }
        return result;
    }
}