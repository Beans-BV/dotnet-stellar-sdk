# SEP-0009 (Standard KYC Fields) Compatibility Matrix

**Updated:** 2026-04-15
**SDK:** StellarDotnetSdk
**SDK Version:** 12.0.0
**SEP Version:** 1.17.0
**SEP Status:** Active
**SEP URL:** https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0009.md

## SEP Summary

This SEP defines a list of standard KYC, AML, and financial account-related
fields for use in Stellar ecosystem protocols. Applications on Stellar should
use these fields when sending or requesting KYC, AML, or financial
account-related information with other parties on Stellar. This is an evolving
list, so please suggest any missing fields that you use.

This is a list of possible fields that may be necessary to handle many
different use cases, there is no expectation that any particular fields be used
for a particular application. The best fields to use in a particular case is
determined by the needs of the application.

## Overall Coverage

**Total Coverage:** 100.0% (76/76 fields)

- ✅ **Implemented:** 76/76
- ❌ **Not Implemented:** 0/76

**Required Fields:** 0% (0/0)

**Optional Fields:** 100.0% (76/76)

## Implementation Status

✅ **Implemented**

### Implementation Files

- `StellarDotnetSdk/Sep/Sep0009/StandardKycFields.cs`
- `StellarDotnetSdk/Sep/Sep0009/NaturalPersonKycFields.cs`
- `StellarDotnetSdk/Sep/Sep0009/OrganizationKycFields.cs`
- `StellarDotnetSdk/Sep/Sep0009/FinancialAccountKycFields.cs`
- `StellarDotnetSdk/Sep/Sep0009/CardKycFields.cs`

### Key Classes

- **`StandardKycFields`**: Container for all standard KYC field types
- **`NaturalPersonKycFields`**: KYC fields for individuals (name, address, ID documents, etc.)
- **`FinancialAccountKycFields`**: KYC fields for financial accounts (bank name, account number, etc.)
- **`OrganizationKycFields`**: KYC fields for organizations (legal name, registration, address, etc.)
- **`CardKycFields`**: KYC fields for payment cards (card number, expiration, CVV, etc.)

## Coverage by Section

| Section | Coverage | Required Coverage | Implemented | Not Implemented | Total |
|---------|----------|-------------------|-------------|-----------------|-------|
| Card Fields | 100.0% | 100% | 11 | 0 | 11 |
| Financial Account Fields | 100.0% | 100% | 14 | 0 | 14 |
| Natural Person Fields | 100.0% | 100% | 34 | 0 | 34 |
| Organization Fields | 100.0% | 100% | 17 | 0 | 17 |

## Detailed Field Comparison

### Card Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `card.address` |  | ✅ | `Address` | Entire address (country, state, postal code, street address, etc.) as a multi-line string |
| `card.city` |  | ✅ | `City` | Name of city/town |
| `card.country_code` |  | ✅ | `CountryCode` | Billing address country code in ISO 3166-1 alpha-2 code (e.g., US) |
| `card.cvc` |  | ✅ | `Cvc` | CVC number (Digits on the back of the card) |
| `card.expiration_date` |  | ✅ | `ExpirationDate` | Expiration month and year in YY-MM format (e.g., 29-11, November 2029) |
| `card.holder_name` |  | ✅ | `HolderName` | Name of the card holder |
| `card.network` |  | ✅ | `Network` | Brand of the card/network it operates within (e.g., Visa, Mastercard, AmEx, etc.) |
| `card.number` |  | ✅ | `Number` | Card number |
| `card.postal_code` |  | ✅ | `PostalCode` | Billing address postal code |
| `card.state_or_province` |  | ✅ | `StateOrProvince` | Name of state/province/region/prefecture in ISO 3166-2 format |
| `card.token` |  | ✅ | `Token` | Token representation of the card in some external payment system (e.g., Stripe) |

### Financial Account Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `bank_account_number` |  | ✅ | `BankAccountNumber` | Number identifying bank account |
| `bank_account_type` |  | ✅ | `BankAccountType` | Type of bank account |
| `bank_branch_number` |  | ✅ | `BankBranchNumber` | Number identifying bank branch |
| `bank_name` |  | ✅ | `BankName` | Name of the bank |
| `bank_number` |  | ✅ | `BankNumber` | Number identifying bank in national banking system (routing number in US) |
| `bank_phone_number` |  | ✅ | `BankPhoneNumber` | Phone number with country code for bank |
| `cbu_alias` |  | ✅ | `CbuAlias` | The alias for a CBU or CVU |
| `cbu_number` |  | ✅ | `CbuNumber` | Clave Bancaria Uniforme (CBU) or Clave Virtual Uniforme (CVU) |
| `clabe_number` |  | ✅ | `ClabeNumber` | Bank account number for Mexico |
| `crypto_address` |  | ✅ | `CryptoAddress` | Address for a cryptocurrency account |
| `crypto_memo` |  | ✅ | `CryptoMemo` | A destination tag/memo used to identify a transaction |
| `external_transfer_memo` |  | ✅ | `ExternalTransferMemo` | A destination tag/memo used to identify a transaction |
| `mobile_money_number` |  | ✅ | `MobileMoneyNumber` | Mobile phone number in E.164 format with which a mobile money account is associated |
| `mobile_money_provider` |  | ✅ | `MobileMoneyProvider` | Name of the mobile money service provider |

### Natural Person Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `additional_name` |  | ✅ | `AdditionalName` | Middle name or other additional name |
| `address` |  | ✅ | `Address` | Entire address (country, state, postal code, street address, etc.) as a multi-line string |
| `address_country_code` |  | ✅ | `AddressCountryCode` | Country code for current address |
| `birth_country_code` |  | ✅ | `BirthCountryCode` | ISO Code of country of birth (ISO 3166-1 alpha-3) |
| `birth_date` |  | ✅ | `BirthDate` | Date of birth (e.g., 1976-07-04) |
| `birth_place` |  | ✅ | `BirthPlace` | Place of birth (city, state, country; as on passport) |
| `city` |  | ✅ | `City` | Name of city/town |
| `email_address` |  | ✅ | `EmailAddress` | Email address |
| `employer_address` |  | ✅ | `EmployerAddress` | Address of employer |
| `employer_name` |  | ✅ | `EmployerName` | Name of employer |
| `first_name` |  | ✅ | `FirstName` | Given or first name |
| `id_country_code` |  | ✅ | `IdCountryCode` | Country issuing passport or photo ID (ISO 3166-1 alpha-3) |
| `id_expiration_date` |  | ✅ | `IdExpirationDate` | ID expiration date |
| `id_issue_date` |  | ✅ | `IdIssueDate` | ID issue date |
| `id_number` |  | ✅ | `IdNumber` | Passport or ID number |
| `id_type` |  | ✅ | `IdType` | Type of ID (passport, drivers_license, id_card, etc.) |
| `ip_address` |  | ✅ | `IpAddress` | IP address of customer's computer |
| `language_code` |  | ✅ | `LanguageCode` | Primary language (ISO 639-1) |
| `last_name` |  | ✅ | `LastName` | Family or last name |
| `mobile_number` |  | ✅ | `MobileNumber` | Mobile phone number with country code, in E.164 format |
| `mobile_number_format` |  | ✅ | `MobileNumberFormat` | Expected format of the mobile_number field (E.164, hash, etc.) |
| `notary_approval_of_photo_id` |  | ✅ | `NotaryApprovalOfPhotoId` | Image of notary's approval of photo ID or passport |
| `occupation` |  | ✅ | `Occupation` | Occupation ISCO code |
| `photo_id_back` |  | ✅ | `PhotoIdBack` | Image of back of user's photo ID or passport |
| `photo_id_front` |  | ✅ | `PhotoIdFront` | Image of front of user's photo ID or passport |
| `photo_proof_residence` |  | ✅ | `PhotoProofResidence` | Image of a utility bill, bank statement or similar with the user's name and address |
| `postal_code` |  | ✅ | `PostalCode` | Postal or other code identifying user's locale |
| `proof_of_income` |  | ✅ | `ProofOfIncome` | Image of user's proof of income document |
| `proof_of_liveness` |  | ✅ | `ProofOfLiveness` | Video or image file of user as a liveness proof |
| `referral_id` |  | ✅ | `ReferralId` | User's origin (such as an id in another application) or a referral code |
| `sex` |  | ✅ | `Sex` | Gender (male, female, or other) |
| `state_or_province` |  | ✅ | `StateOrProvince` | Name of state/province/region/prefecture |
| `tax_id` |  | ✅ | `TaxId` | Tax identifier of user in their country (social security number in US) |
| `tax_id_name` |  | ✅ | `TaxIdName` | Name of the tax ID (SSN or ITIN in the US) |

### Organization Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `organization.VAT_number` |  | ✅ | `VatNumber` | Organization VAT number |
| `organization.address_country_code` |  | ✅ | `AddressCountryCode` | Country code for current address |
| `organization.city` |  | ✅ | `City` | Name of city/town |
| `organization.director_name` |  | ✅ | `DirectorName` | Organization registered managing director |
| `organization.email` |  | ✅ | `Email` | Organization contact email |
| `organization.name` |  | ✅ | `Name` | Full organization name as on the incorporation papers |
| `organization.number_of_shareholders` |  | ✅ | `NumberOfShareholders` | Organization shareholder number |
| `organization.phone` |  | ✅ | `Phone` | Organization contact phone |
| `organization.photo_incorporation_doc` |  | ✅ | `PhotoIncorporationDoc` | Image of incorporation documents |
| `organization.photo_proof_address` |  | ✅ | `PhotoProofAddress` | Image of a utility bill, bank statement with the organization's name and address |
| `organization.postal_code` |  | ✅ | `PostalCode` | Postal or other code identifying organization's locale |
| `organization.registered_address` |  | ✅ | `RegisteredAddress` | Organization registered address |
| `organization.registration_date` |  | ✅ | `RegistrationDate` | Date the organization was registered |
| `organization.registration_number` |  | ✅ | `RegistrationNumber` | Organization registration number |
| `organization.shareholder_name` |  | ✅ | `ShareholderName` | Name of shareholder (can be organization or person) |
| `organization.state_or_province` |  | ✅ | `StateOrProvince` | Name of state/province/region/prefecture |
| `organization.website` |  | ✅ | `Website` | Organization website |

## Implementation Gaps

🎉 **No gaps found!** All fields are implemented.

## Recommendations

✅ The SDK has full compatibility with SEP-0009!

## Legend

- ✅ **Implemented**: Field is implemented in SDK
- ❌ **Not Implemented**: Field is missing from SDK
- ⚙️ **Server**: Server-side only feature (not applicable to client SDKs)
- ✓ **Required**: Field is required by SEP specification
- (blank) **Optional**: Field is optional
