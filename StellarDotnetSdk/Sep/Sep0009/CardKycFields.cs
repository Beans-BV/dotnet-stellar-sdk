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
    // Field keys
    public const string KeyPrefix = "card.";
    public const string NumberFieldKey = KeyPrefix + "number";
    public const string ExpirationDateFieldKey = KeyPrefix + "expiration_date";
    public const string CvcFieldKey = KeyPrefix + "cvc";
    public const string HolderNameFieldKey = KeyPrefix + "holder_name";
    public const string NetworkFieldKey = KeyPrefix + "network";
    public const string PostalCodeFieldKey = KeyPrefix + "postal_code";
    public const string CountryCodeFieldKey = KeyPrefix + "country_code";
    public const string StateOrProvinceFieldKey = KeyPrefix + "state_or_province";
    public const string CityFieldKey = KeyPrefix + "city";
    public const string AddressFieldKey = KeyPrefix + "address";
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