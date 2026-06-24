# SEP-0001 (Stellar Info File) Compatibility Matrix

**Updated:** 2026-06-17  
**SDK:** StellarDotnetSdk  
**SDK Version:** 12.0.0  
**SEP Version:** 2.7.0  
**SEP Status:** Active  
**SEP URL:** https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0001.md

## SEP Summary

The `stellar.toml` file is used to provide a common place where the Internet
can find information about your organization’s Stellar integration. By setting
the home_domain of your Stellar account to the domain that hosts your
`stellar.toml`, you can create a definitive link between this information and
that account. Any website can publish Stellar network information, and the
`stellar.toml` is designed to be readable by both humans and machines.

If you are an anchor or issuer, the `stellar.toml` file serves a very important
purpose: it allows you to publish information about your organization and
token(s) that help to legitimize your offerings. Clients and exchanges can use
this information to decide whether a token should be listed. Fully and
truthfully disclosing contact and business information is an essential step in
responsible token issuance.

If you are a validator, the `stellar.toml` file allows you to declare your
node(s) to other network participants, which improves discoverability, and
contributes to the health and decentralization of the network as a whole.

## Overall Coverage

**Total Coverage:** 100.0% (70/70 fields)

- ✅ **Implemented:** 70/70
- ❌ **Not Implemented:** 0/70

**Required Fields:** 100.0% (3/3)

**Optional Fields:** 100.0% (67/67)

## Implementation Status

✅ **Implemented**

### Implementation Files

- `StellarDotnetSdk/Sep/Sep0001/StellarToml.cs`
- `StellarDotnetSdk/Sep/Sep0001/GeneralInformation.cs`
- `StellarDotnetSdk/Sep/Sep0001/Documentation.cs`
- `StellarDotnetSdk/Sep/Sep0001/PointOfContact.cs`
- `StellarDotnetSdk/Sep/Sep0001/Currency.cs`
- `StellarDotnetSdk/Sep/Sep0001/Validator.cs`

### Key Classes

- **`StellarToml`**: Main class for fetching and parsing stellar.toml files from Stellar domains
- **`GeneralInformation`**: Represents the general information section (VERSION, NETWORK_PASSPHRASE, etc.)
- **`Documentation`**: Represents organization documentation (ORG_NAME, ORG_URL, ORG_LOGO, etc.)
- **`PointOfContact`**: Represents point of contact information (name, email, keybase, etc.)
- **`Currency`**: Represents currency/asset documentation (code, issuer, status, etc.)
- **`Validator`**: Represents validator node information (ALIAS, DISPLAY_NAME, PUBLIC_KEY, etc.)

## Coverage by Section

| Section | Coverage | Required Coverage | Implemented | Not Implemented | Total |
|---------|----------|-------------------|-------------|-----------------|-------|
| Currency Documentation | 100.0% | 100.0% | 24 | 0 | 24 |
| General Information | 100.0% | 100.0% | 16 | 0 | 16 |
| Organization Documentation | 100.0% | 100.0% | 17 | 0 | 17 |
| Point of Contact Documentation | 100.0% | 100.0% | 8 | 0 | 8 |
| Validator Information | 100.0% | 100.0% | 5 | 0 | 5 |

## Detailed Field Comparison

### Currency Documentation

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `anchor_asset` |  | ✅ | `AnchorAsset` | If anchored token, code / symbol for asset that token is anchored to. E.g. USD, BTC, SBUX, Address of real-estate investment property. |
| `anchor_asset_type` |  | ✅ | `AnchorAssetType` | Type of asset anchored. Can be `fiat`, `crypto`, `nft`, `stock`, `bond`, `commodity`, `realestate`, or `other`. |
| `approval_criteria` |  | ✅ | `ApprovalCriteria` | a human readable string that explains the issuer's requirements for approving transactions. |
| `approval_server` |  | ✅ | `ApprovalServer` | url of a [sep0008](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0008.md) compliant approval service that signs validated transactions. |
| `attestation_of_reserve` |  | ✅ | `AttestationOfReserve` | URL to attestation or other proof, evidence, or verification of reserves, such as third-party audits. |
| `code` | ✓ | ✅ | `Code` | Token code. Required. |
| `code_template` |  | ✅ | `CodeTemplate` | A pattern with `?` as a single character wildcard. Allows a `[[CURRENCIES]]` entry to apply to multiple assets that share the same info. An example is futures, where the only difference between issues... |
| `collateral_address_messages` |  | ✅ | `CollateralAddressMessages` | Messages stating that funds in the `collateral_addresses` list are reserved to back the issued asset. See below for details. |
| `collateral_address_signatures` |  | ✅ | `CollateralAddressSignatures` | These prove you control the `collateral_addresses`. For each address you list, sign the entry in `collateral_address_messages` with the address's private key and add the resulting string to this list ... |
| `collateral_addresses` |  | ✅ | `CollateralAddresses` | If this is an anchored crypto token, list of one or more public addresses that hold the assets for which you are issuing tokens. |
| `conditions` |  | ✅ | `Conditions` | Conditions on token |
| `contract` | ✓ | ✅ | `Contract` | Contract ID of the token contract. The token must be compatible with the [SEP-41 Token Interface](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0041.md) to be defined here. Required for tokens that are not Stellar Assets. Omitted if the token... |
| `desc` |  | ✅ | `Desc` | Description of token and what it represents |
| `display_decimals` |  | ✅ | `DisplayDecimals` | Preference for number of decimals to show when a client displays currency balance |
| `fixed_number` |  | ✅ | `FixedNumber` | Fixed number of tokens, if the number of tokens issued will never change |
| `image` |  | ✅ | `Image` | URL to a PNG image on a transparent background representing token |
| `is_asset_anchored` |  | ✅ | `IsAssetAnchored` | `true` if token can be redeemed for underlying asset, otherwise `false` |
| `is_unlimited` |  | ✅ | `IsUnlimited` | The number of tokens is dilutable at the issuer's discretion |
| `issuer` | ✓ | ✅ | `Issuer` | Stellar public key of the issuing account. Required for tokens that are Stellar Assets. Omitted if the token is not a Stellar asset. |
| `max_number` |  | ✅ | `MaxNumber` | Max number of tokens, if there will never be more than `max_number` tokens |
| `name` |  | ✅ | `Name` | A short name for the token |
| `redemption_instructions` |  | ✅ | `RedemptionInstructions` | If anchored token, these are instructions to redeem the underlying asset from tokens. |
| `regulated` |  | ✅ | `Regulated` | indicates whether or not this is a [sep0008](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0008.md) regulated asset. If missing, `false` is assumed. |
| `status` |  | ✅ | `Status` | Status of token. One of `live`, `dead`, `test`, or `private`. Allows issuer to mark whether token is dead/for testing/for private use or is live and should be listed in live exchanges. |

### General Information

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `ACCOUNTS` |  | ✅ | `Accounts` | A list of Stellar accounts that are controlled by this domain |
| `ANCHOR_QUOTE_SERVER` |  | ✅ | `AnchorQuoteServer` | The server used for receiving [SEP-38](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0038.md) requests. |
| `AUTH_SERVER` |  | ✅ | `AuthServer` | (deprecated) The endpoint used for [SEP-3](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0003.md) Compliance Protocol |
| `DIRECT_PAYMENT_SERVER` |  | ✅ | `DirectPaymentServer` | The server used for receiving [SEP-31](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0031.md) direct fiat-to-fiat payments. Requires [SEP-12](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0012.md) and hence a `KYC_SERVER` TOML attribute. |
| `FEDERATION_SERVER` |  | ✅ | `FederationServer` | The endpoint for clients to resolve stellar addresses for users on your domain via [SEP-2](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0002.md) Federation Protocol |
| `HORIZON_URL` |  | ✅ | `HorizonUrl` | Location of public-facing Horizon instance (if you offer one) |
| `KYC_SERVER` |  | ✅ | `KycServer` | The server used for [SEP-12](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0012.md) Anchor/Client customer info transfer |
| `NETWORK_PASSPHRASE` |  | ✅ | `NetworkPassphrase` | The passphrase for the specific [Stellar network](https://developers.stellar.org/docs/networks) this infrastructure operates on |
| `SIGNING_KEY` |  | ✅ | `SigningKey` | The signing key is used for [SEP-3](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0003.md) Compliance Protocol (deprecated) and [SEP-10](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0010.md)/[SEP-45](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0045.md) Authentication Protocols |
| `TRANSFER_SERVER` |  | ✅ | `TransferServer` | The server used for [SEP-6](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0006.md) Anchor/Client interoperability |
| `TRANSFER_SERVER_SEP0024` |  | ✅ | `TransferServerSep24` | The server used for [SEP-24](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0024.md) Anchor/Client interoperability |
| `URI_REQUEST_SIGNING_KEY` |  | ✅ | `UriRequestSigningKey` | The signing key is used for [SEP-7](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0007.md) delegated signing |
| `VERSION` |  | ✅ | `Version` | The version of SEP-1 your `stellar.toml` adheres to. This helps parsers know which fields to expect. |
| `WEB_AUTH_CONTRACT_ID` |  | ✅ | `WebAuthContractId` | The web authentication contract ID for [SEP-45 Web Authentication](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0045.md) |
| `WEB_AUTH_ENDPOINT` |  | ✅ | `WebAuthEndpoint` | The endpoint used for [SEP-10 Web Authentication](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0010.md) |
| `WEB_AUTH_FOR_CONTRACTS_ENDPOINT` |  | ✅ | `WebAuthForContractsEndpoint` | The endpoint used for [SEP-45 Web Authentication](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0045.md) |

### Organization Documentation

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `ORG_DBA` |  | ✅ | `OrgDba` | (may not apply) [DBA](https://www.entrepreneur.com/encyclopedia/doing-business-as-dba) of your organization |
| `ORG_DESCRIPTION` |  | ✅ | `OrgDescription` | Short description of your organization |
| `ORG_GITHUB` |  | ✅ | `OrgGithub` | Your organization's GitHub account |
| `ORG_KEYBASE` |  | ✅ | `OrgKeybase` | A [Keybase](https://keybase.io/) account name for your organization. Should contain proof of ownership of any public online accounts you list here, including your organization's domain. |
| `ORG_LICENSE_NUMBER` |  | ✅ | `OrgLicenseNumber` | Official license, registration, or authorization number of your organization, if applicable |
| `ORG_LICENSE_TYPE` |  | ✅ | `OrgLicenseType` | Type of financial or other license, registration, or authorization your organization holds, if applicable |
| `ORG_LICENSING_AUTHORITY` |  | ✅ | `OrgLicensingAuthority` | Name of the authority or agency that issued a license, registration, or authorization to your organization, if applicable |
| `ORG_LOGO` |  | ✅ | `OrgLogo` | A PNG image of your organization's logo on a transparent background |
| `ORG_NAME` |  | ✅ | `OrgName` | Legal name of your organization |
| `ORG_OFFICIAL_EMAIL` |  | ✅ | `OrgOfficialEmail` | An email that business partners such as wallets, exchanges, or anchors can use to contact your organization. Must be hosted at your `ORG_URL` domain. |
| `ORG_PHONE_NUMBER` |  | ✅ | `OrgPhoneNumber` | Your organization's phone number in [E.164 format](https://en.wikipedia.org/wiki/E.164), e.g. `+14155552671`. See also [this guide](https://support.twilio.com/hc/en-us/articles/223183008-Formatting-International-Phone-Numbers). |
| `ORG_PHONE_NUMBER_ATTESTATION` |  | ✅ | `OrgPhoneNumberAttestation` | URL on the same domain as your `ORG_URL` that contains an image or pdf of a phone bill showing both the phone number and your organization's name. |
| `ORG_PHYSICAL_ADDRESS` |  | ✅ | `OrgPhysicalAddress` | Physical address for your organization |
| `ORG_PHYSICAL_ADDRESS_ATTESTATION` |  | ✅ | `OrgPhysicalAddressAttestation` | URL on the same domain as your `ORG_URL` that contains an image or pdf official document attesting to your physical address. It must list your `ORG_NAME` or `ORG_DBA` as the party at the address. Only... |
| `ORG_SUPPORT_EMAIL` |  | ✅ | `OrgSupportEmail` | An email that users can use to request support regarding your Stellar assets or applications. |
| `ORG_TWITTER` |  | ✅ | `OrgTwitter` | Your organization's Twitter account |
| `ORG_URL` |  | ✅ | `OrgUrl` | Your organization's official URL. Your `stellar.toml` must be hosted on the same domain. |

### Point of Contact Documentation

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `email` |  | ✅ | `Email` | Business email address for the principal |
| `github` |  | ✅ | `Github` | Personal GitHub account |
| `id_photo_hash` |  | ✅ | `IdPhotoHash` | SHA-256 hash of a photo of the principal's government-issued photo ID |
| `keybase` |  | ✅ | `Keybase` | Personal Keybase account. Should include proof of ownership for other online accounts, as well as the organization's domain. |
| `name` |  | ✅ | `Name` | Full legal name |
| `telegram` |  | ✅ | `Telegram` | Personal Telegram account |
| `twitter` |  | ✅ | `Twitter` | Personal Twitter account |
| `verification_photo_hash` |  | ✅ | `VerificationPhotoHash` | SHA-256 hash of a verification photo of principal. Should be well-lit and contain: principal holding ID card and signed, dated, hand-written message stating `I, $NAME, am a principal of $ORG_NAME, a S... |

### Validator Information

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `ALIAS` |  | ✅ | `Alias` | A name for display in stellar-core configs that conforms to `^[a-z0-9-]{2,16}$` |
| `DISPLAY_NAME` |  | ✅ | `DisplayName` | A human-readable name for display in quorum explorers and other interfaces |
| `HISTORY` |  | ✅ | `History` | The location of the history archive published by this validator |
| `HOST` |  | ✅ | `Host` | The IP:port or domain:port peers can use to connect to the node |
| `PUBLIC_KEY` |  | ✅ | `PublicKey` | The Stellar account associated with the node |

## Implementation Gaps

🎉 **No gaps found!** All fields are implemented.

## Recommendations

✅ The SDK has full compatibility with SEP-0001!

## Legend

- ✅ **Implemented**: Field is implemented in SDK
- ❌ **Not Implemented**: Field is missing from SDK
- ⚙️ **Server**: Server-side only feature (not applicable to client SDKs)
- ✓ **Required**: Field is required by SEP specification
- (blank) **Optional**: Field is optional
