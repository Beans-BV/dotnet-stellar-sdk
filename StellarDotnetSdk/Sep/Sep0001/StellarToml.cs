using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Nett;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0001.Exceptions;

namespace StellarDotnetSdk.Sep.Sep0001;

/// <summary>
///     Parses and provides access to stellar.toml files as defined in SEP-0001.
///     The stellar.toml file is a standardized configuration file that organizations
///     publish at `https://DOMAIN/.well-known/stellar.toml` to provide information
///     about their Stellar integration, including service endpoints, validators,
///     currencies, and organizational details.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0001.md">SEP-0001</a>
/// </summary>
public class StellarToml
{
    /// <summary>
    ///     Gets or creates an HttpClient instance.
    ///     Uses DefaultStellarSdkHttpClient without retries by default, matching the Server class pattern.
    /// </summary>
    private static HttpClient GetOrCreateHttpClient(
        string? bearerToken = null,
        HttpResilienceOptions? resilienceOptions = null)
    {
        return new DefaultStellarSdkHttpClient(
            bearerToken,
            resilienceOptions: resilienceOptions);
    }

    /// <summary>
    ///     General information from the stellar.toml file.
    /// </summary>
    public GeneralInformation GeneralInformation { get; }

    /// <summary>
    ///     Organization documentation from the DOCUMENTATION table.
    /// </summary>
    public Documentation? Documentation { get; }

    /// <summary>
    ///     Points of contact from the PRINCIPALS list.
    /// </summary>
    public List<PointOfContact>? PointsOfContact { get; }

    /// <summary>
    ///     Currencies from the CURRENCIES list.
    /// </summary>
    public List<Currency>? Currencies { get; }

    /// <summary>
    ///     Validators from the VALIDATORS list.
    /// </summary>
    public List<Validator>? Validators { get; }

    /// <summary>
    ///     Constructs a StellarToml instance by parsing raw TOML content.
    /// </summary>
    /// <param name="toml">Raw TOML content string to parse</param>
    /// <exception cref="StellarTomlException">Thrown when TOML parsing fails</exception>
    public StellarToml(string toml)
    {
        try
        {
            var safeToml = SafeguardTomlContent(toml);
            var document = Toml.ReadString(safeToml);

            GeneralInformation = ParseGeneralInformation(document);
            Documentation = ParseDocumentation(document);
            PointsOfContact = ParsePointsOfContact(document);
            Currencies = ParseCurrencies(document);
            Validators = ParseValidators(document);
        }
        catch (Exception ex)
        {
            throw new StellarTomlException("Failed to parse stellar.toml content", ex);
        }
    }

    /// <summary>
    ///     Fetches and parses stellar.toml from a domain's well-known location.
    ///     Automatically constructs the standard stellar.toml URL for the given domain
    ///     and fetches the content via HTTPS. The standard location is always:
    ///     `https://DOMAIN/.well-known/stellar.toml`
    /// </summary>
    /// <param name="domain">The domain name (without protocol). E.g., "example.com"</param>
    /// <param name="httpClient">Optional custom HTTP client for testing or proxy configuration</param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers to include in the request</param>
    /// <returns>StellarToml instance containing the parsed stellar.toml data</returns>
    /// <exception cref="StellarTomlException">Thrown when the stellar.toml file is not found or cannot be parsed</exception>
    public static async Task<StellarToml> FromDomainAsync(
        string domain,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null)
    {
        return await FromDomainAsync(domain, null, null, httpClient, httpRequestHeaders);
    }

    /// <summary>
    ///     Fetches and parses stellar.toml from a domain's well-known location.
    ///     Automatically constructs the standard stellar.toml URL for the given domain
    ///     and fetches the content via HTTPS. The standard location is always:
    ///     `https://DOMAIN/.well-known/stellar.toml`
    /// </summary>
    /// <param name="domain">The domain name (without protocol). E.g., "example.com"</param>
    /// <param name="resilienceOptions">Resilience options for HTTP requests. If null, default retry configuration is used.</param>
    /// <param name="bearerToken">(Optional) Bearer token in case the server requires it.</param>
    /// <param name="httpClient">Optional custom HTTP client for testing or proxy configuration. If provided, resilienceOptions and bearerToken are ignored.</param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers to include in the request</param>
    /// <returns>StellarToml instance containing the parsed stellar.toml data</returns>
    /// <exception cref="StellarTomlException">Thrown when the stellar.toml file is not found or cannot be parsed</exception>
    public static async Task<StellarToml> FromDomainAsync(
        string domain,
        HttpResilienceOptions? resilienceOptions,
        string? bearerToken,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new ArgumentException("Domain cannot be null or empty", nameof(domain));
        }

        var stellarTomlUri = new Uri($"https://{domain}/.well-known/stellar.toml");

        var client = httpClient ?? GetOrCreateHttpClient(bearerToken, resilienceOptions);
        var internalHttpClient = httpClient == null;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, stellarTomlUri);
            if (httpRequestHeaders != null)
            {
                foreach (var header in httpRequestHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var response = await client.SendAsync(request).ConfigureAwait(false);

            if ((int)response.StatusCode >= 300)
            {
                throw new StellarTomlException(
                    $"Stellar toml not found, response status code {(int)response.StatusCode}");
            }

            var responseToml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new StellarToml(responseToml);
        }
        catch (HttpRequestException ex)
        {
            throw new StellarTomlException($"Failed to fetch stellar.toml from {stellarTomlUri}", ex);
        }
        catch (StellarTomlException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new StellarTomlException($"Unexpected error fetching stellar.toml from {stellarTomlUri}", ex);
        }
        finally
        {
            if (internalHttpClient)
            {
                client.Dispose();
            }
        }
    }

    /// <summary>
    ///     Loads detailed currency information from an external TOML file.
    ///     Instead of embedding complete currency information directly in stellar.toml,
    ///     organizations can link to separate TOML files for each currency.
    /// </summary>
    /// <param name="tomlUrl">The full URL to the currency TOML file</param>
    /// <param name="httpClient">Optional custom HTTP client for testing or proxy configuration</param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers to include in the request</param>
    /// <returns>Currency instance containing the complete currency information</returns>
    /// <exception cref="StellarTomlException">Thrown when the currency TOML file is not found or cannot be parsed</exception>
    public static async Task<Currency> CurrencyFromUrlAsync(
        string tomlUrl,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null)
    {
        return await CurrencyFromUrlAsync(tomlUrl, null, null, httpClient, httpRequestHeaders);
    }

    /// <summary>
    ///     Loads detailed currency information from an external TOML file.
    ///     Instead of embedding complete currency information directly in stellar.toml,
    ///     organizations can link to separate TOML files for each currency.
    /// </summary>
    /// <param name="tomlUrl">The full URL to the currency TOML file</param>
    /// <param name="resilienceOptions">Resilience options for HTTP requests. If null, default retry configuration is used.</param>
    /// <param name="bearerToken">(Optional) Bearer token in case the server requires it.</param>
    /// <param name="httpClient">Optional custom HTTP client for testing or proxy configuration. If provided, resilienceOptions and bearerToken are ignored.</param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers to include in the request</param>
    /// <returns>Currency instance containing the complete currency information</returns>
    /// <exception cref="StellarTomlException">Thrown when the currency TOML file is not found or cannot be parsed</exception>
    public static async Task<Currency> CurrencyFromUrlAsync(
        string tomlUrl,
        HttpResilienceOptions? resilienceOptions,
        string? bearerToken,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null)
    {
        if (string.IsNullOrWhiteSpace(tomlUrl))
        {
            throw new ArgumentException("TOML URL cannot be null or empty", nameof(tomlUrl));
        }

        if (!Uri.TryCreate(tomlUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid TOML URL: {tomlUrl}", nameof(tomlUrl));
        }

        var client = httpClient ?? GetOrCreateHttpClient(bearerToken, resilienceOptions);
        var internalHttpClient = httpClient == null;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (httpRequestHeaders != null)
            {
                foreach (var header in httpRequestHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var response = await client.SendAsync(request).ConfigureAwait(false);

            if ((int)response.StatusCode >= 300)
            {
                throw new StellarTomlException(
                    $"Currency toml not found, response status code {(int)response.StatusCode}");
            }

            var responseToml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var document = Toml.ReadString(responseToml);
            return ParseCurrency(document);
        }
        catch (HttpRequestException ex)
        {
            throw new StellarTomlException($"Failed to fetch currency TOML from {uri}", ex);
        }
        catch (StellarTomlException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new StellarTomlException($"Unexpected error fetching currency TOML from {uri}", ex);
        }
        finally
        {
            if (internalHttpClient)
            {
                client.Dispose();
            }
        }
    }

    /// <summary>
    ///     Corrects common formatting errors in stellar.toml content.
    ///     Some stellar.toml files in the wild contain invalid TOML syntax, particularly
    ///     with array table declarations. This method automatically corrects these issues.
    /// </summary>
    /// <param name="input">The raw TOML content string</param>
    /// <returns>Corrected TOML content string</returns>
    private static string SafeguardTomlContent(string input)
    {
        var lines = input.Split('\n');
        var correctedLines = new List<string>();

        for (var i = 0; i < lines.Length; i++)
        {
            var trimmedLine = lines[i].TrimStart();
            var line = lines[i];

            // Fix [[DOCUMENTATION]] -> [DOCUMENTATION] (should be single table)
            if (trimmedLine.StartsWith("[[DOCUMENTATION]]"))
            {
                line = line.Replace("[[DOCUMENTATION]]", "[DOCUMENTATION]");
            }
            // Fix [PRINCIPALS] -> [[PRINCIPALS]] (should be array of tables)
            else if (trimmedLine.StartsWith("[PRINCIPALS]"))
            {
                line = line.Replace("[PRINCIPALS]", "[[PRINCIPALS]]");
            }
            // Fix [CURRENCIES] -> [[CURRENCIES]] (should be array of tables)
            else if (trimmedLine.StartsWith("[CURRENCIES]"))
            {
                line = line.Replace("[CURRENCIES]", "[[CURRENCIES]]");
            }
            // Fix [VALIDATORS] -> [[VALIDATORS]] (should be array of tables)
            else if (trimmedLine.StartsWith("[VALIDATORS]"))
            {
                line = line.Replace("[VALIDATORS]", "[[VALIDATORS]]");
            }

            correctedLines.Add(line);
        }

        return string.Join("\n", correctedLines);
    }

    private static GeneralInformation ParseGeneralInformation(TomlTable document)
    {
        var info = new GeneralInformation();

        if (document.TryGetValue("VERSION", out var version))
        {
            info.Version = version.Get<string>();
        }

        if (document.TryGetValue("NETWORK_PASSPHRASE", out var networkPassphrase))
        {
            info.NetworkPassphrase = networkPassphrase.Get<string>();
        }

        if (document.TryGetValue("FEDERATION_SERVER", out var federationServer))
        {
            info.FederationServer = federationServer.Get<string>();
        }

        if (document.TryGetValue("AUTH_SERVER", out var authServer))
        {
            info.AuthServer = authServer.Get<string>();
        }

        if (document.TryGetValue("TRANSFER_SERVER", out var transferServer))
        {
            info.TransferServer = transferServer.Get<string>();
        }

        if (document.TryGetValue("TRANSFER_SERVER_SEP0024", out var transferServerSep24))
        {
            info.TransferServerSep24 = transferServerSep24.Get<string>();
        }

        if (document.TryGetValue("KYC_SERVER", out var kycServer))
        {
            info.KycServer = kycServer.Get<string>();
        }

        if (document.TryGetValue("WEB_AUTH_ENDPOINT", out var webAuthEndpoint))
        {
            info.WebAuthEndpoint = webAuthEndpoint.Get<string>();
        }

        if (document.TryGetValue("SIGNING_KEY", out var signingKey))
        {
            info.SigningKey = signingKey.Get<string>();
        }

        if (document.TryGetValue("HORIZON_URL", out var horizonUrl))
        {
            info.HorizonUrl = horizonUrl.Get<string>();
        }

        if (document.TryGetValue("ACCOUNTS", out var accounts))
        {
            var result = new List<string>();
            var accountsArray = accounts.Get<TomlArray>();
            foreach (var item in accountsArray.Items)
            {
                var accountValue = item.Get<string>();
                if (!string.IsNullOrWhiteSpace(accountValue))
                {
                    result.Add(accountValue);
                }
            }
            info.Accounts = result.AsReadOnly();
        }

        if (document.TryGetValue("URI_REQUEST_SIGNING_KEY", out var uriRequestSigningKey))
        {
            info.UriRequestSigningKey = uriRequestSigningKey.Get<string>();
        }

        if (document.TryGetValue("DIRECT_PAYMENT_SERVER", out var directPaymentServer))
        {
            info.DirectPaymentServer = directPaymentServer.Get<string>();
        }

        if (document.TryGetValue("ANCHOR_QUOTE_SERVER", out var anchorQuoteServer))
        {
            info.AnchorQuoteServer = anchorQuoteServer.Get<string>();
        }

        if (document.TryGetValue("WEB_AUTH_FOR_CONTRACTS_ENDPOINT", out var webAuthForContractsEndpoint))
        {
            info.WebAuthForContractsEndpoint = webAuthForContractsEndpoint.Get<string>();
        }

        if (document.TryGetValue("WEB_AUTH_CONTRACT_ID", out var webAuthContractId))
        {
            info.WebAuthContractId = webAuthContractId.Get<string>();
        }

        return info;
    }

    private static Documentation? ParseDocumentation(TomlTable document)
    {
        if (!document.TryGetValue("DOCUMENTATION", out var docTable))
        {
            return null;
        }

        var doc = docTable.Get<TomlTable>();
        var documentation = new Documentation();

        if (doc.TryGetValue("ORG_NAME", out var orgName))
        {
            documentation.OrgName = orgName.Get<string>();
        }

        if (doc.TryGetValue("ORG_DBA", out var orgDba))
        {
            documentation.OrgDba = orgDba.Get<string>();
        }

        if (doc.TryGetValue("ORG_URL", out var orgUrl))
        {
            documentation.OrgUrl = orgUrl.Get<string>();
        }

        if (doc.TryGetValue("ORG_LOGO", out var orgLogo))
        {
            documentation.OrgLogo = orgLogo.Get<string>();
        }

        if (doc.TryGetValue("ORG_DESCRIPTION", out var orgDescription))
        {
            documentation.OrgDescription = orgDescription.Get<string>();
        }

        if (doc.TryGetValue("ORG_PHYSICAL_ADDRESS", out var orgPhysicalAddress))
        {
            documentation.OrgPhysicalAddress = orgPhysicalAddress.Get<string>();
        }

        if (doc.TryGetValue("ORG_PHYSICAL_ADDRESS_ATTESTATION", out var orgPhysicalAddressAttestation))
        {
            documentation.OrgPhysicalAddressAttestation = orgPhysicalAddressAttestation.Get<string>();
        }

        if (doc.TryGetValue("ORG_PHONE_NUMBER", out var orgPhoneNumber))
        {
            documentation.OrgPhoneNumber = orgPhoneNumber.Get<string>();
        }

        if (doc.TryGetValue("ORG_PHONE_NUMBER_ATTESTATION", out var orgPhoneNumberAttestation))
        {
            documentation.OrgPhoneNumberAttestation = orgPhoneNumberAttestation.Get<string>();
        }

        if (doc.TryGetValue("ORG_KEYBASE", out var orgKeybase))
        {
            documentation.OrgKeybase = orgKeybase.Get<string>();
        }

        if (doc.TryGetValue("ORG_TWITTER", out var orgTwitter))
        {
            documentation.OrgTwitter = orgTwitter.Get<string>();
        }

        if (doc.TryGetValue("ORG_GITHUB", out var orgGithub))
        {
            documentation.OrgGithub = orgGithub.Get<string>();
        }

        if (doc.TryGetValue("ORG_OFFICIAL_EMAIL", out var orgOfficialEmail))
        {
            documentation.OrgOfficialEmail = orgOfficialEmail.Get<string>();
        }

        if (doc.TryGetValue("ORG_SUPPORT_EMAIL", out var orgSupportEmail))
        {
            documentation.OrgSupportEmail = orgSupportEmail.Get<string>();
        }

        if (doc.TryGetValue("ORG_LICENSING_AUTHORITY", out var orgLicensingAuthority))
        {
            documentation.OrgLicensingAuthority = orgLicensingAuthority.Get<string>();
        }

        if (doc.TryGetValue("ORG_LICENSE_TYPE", out var orgLicenseType))
        {
            documentation.OrgLicenseType = orgLicenseType.Get<string>();
        }

        if (doc.TryGetValue("ORG_LICENSE_NUMBER", out var orgLicenseNumber))
        {
            documentation.OrgLicenseNumber = orgLicenseNumber.Get<string>();
        }

        return documentation;
    }

    private static List<PointOfContact>? ParsePointsOfContact(TomlTable document)
    {
        if (!document.TryGetValue("PRINCIPALS", out var principals))
        {
            return null;
        }

        var principalsArray = principals.Get<TomlTableArray>();
        var pointsOfContact = new List<PointOfContact>();

        foreach (var principalTable in principalsArray.Items)
        {
            var pointOfContact = new PointOfContact();

            if (principalTable.TryGetValue("name", out var name))
            {
                pointOfContact.Name = name.Get<string>();
            }

            if (principalTable.TryGetValue("email", out var email))
            {
                pointOfContact.Email = email.Get<string>();
            }

            if (principalTable.TryGetValue("keybase", out var keybase))
            {
                pointOfContact.Keybase = keybase.Get<string>();
            }

            if (principalTable.TryGetValue("telegram", out var telegram))
            {
                pointOfContact.Telegram = telegram.Get<string>();
            }

            if (principalTable.TryGetValue("twitter", out var twitter))
            {
                pointOfContact.Twitter = twitter.Get<string>();
            }

            if (principalTable.TryGetValue("github", out var github))
            {
                pointOfContact.Github = github.Get<string>();
            }

            if (principalTable.TryGetValue("id_photo_hash", out var idPhotoHash))
            {
                pointOfContact.IdPhotoHash = idPhotoHash.Get<string>();
            }

            if (principalTable.TryGetValue("verification_photo_hash", out var verificationPhotoHash))
            {
                pointOfContact.VerificationPhotoHash = verificationPhotoHash.Get<string>();
            }

            pointsOfContact.Add(pointOfContact);
        }

        return pointsOfContact;
    }

    private static List<Currency>? ParseCurrencies(TomlTable document)
    {
        if (!document.TryGetValue("CURRENCIES", out var currencies))
        {
            return null;
        }

        var currenciesArray = currencies.Get<TomlTableArray>();
        var currencyList = new List<Currency>();

        foreach (var currencyTable in currenciesArray.Items)
        {
            var currency = ParseCurrency(currencyTable);
            currencyList.Add(currency);
        }

        return currencyList;
    }

    private static Currency ParseCurrency(TomlTable currencyTable)
    {
        var currency = new Currency();

        if (currencyTable.TryGetValue("toml", out var toml))
        {
            currency.Toml = toml.Get<string>();
        }

        if (currencyTable.TryGetValue("code", out var code))
        {
            currency.Code = code.Get<string>();
        }

        if (currencyTable.TryGetValue("code_template", out var codeTemplate))
        {
            currency.CodeTemplate = codeTemplate.Get<string>();
        }

        if (currencyTable.TryGetValue("issuer", out var issuer))
        {
            currency.Issuer = issuer.Get<string>();
        }

        if (currencyTable.TryGetValue("contract", out var contract))
        {
            currency.Contract = contract.Get<string>();
        }

        if (currencyTable.TryGetValue("status", out var status))
        {
            currency.Status = status.Get<string>();
        }

        if (currencyTable.TryGetValue("display_decimals", out var displayDecimals))
        {
            currency.DisplayDecimals = displayDecimals.Get<int>();
        }

        if (currencyTable.TryGetValue("name", out var name))
        {
            currency.Name = name.Get<string>();
        }

        if (currencyTable.TryGetValue("desc", out var desc))
        {
            currency.Desc = desc.Get<string>();
        }

        if (currencyTable.TryGetValue("conditions", out var conditions))
        {
            currency.Conditions = conditions.Get<string>();
        }

        if (currencyTable.TryGetValue("image", out var image))
        {
            currency.Image = image.Get<string>();
        }

        if (currencyTable.TryGetValue("fixed_number", out var fixedNumber))
        {
            currency.FixedNumber = fixedNumber.Get<int>();
        }

        if (currencyTable.TryGetValue("max_number", out var maxNumber))
        {
            currency.MaxNumber = maxNumber.Get<int>();
        }

        if (currencyTable.TryGetValue("is_unlimited", out var isUnlimited))
        {
            currency.IsUnlimited = isUnlimited.Get<bool>();
        }

        if (currencyTable.TryGetValue("is_asset_anchored", out var isAssetAnchored))
        {
            currency.IsAssetAnchored = isAssetAnchored.Get<bool>();
        }

        if (currencyTable.TryGetValue("anchor_asset_type", out var anchorAssetType))
        {
            currency.AnchorAssetType = anchorAssetType.Get<string>();
        }

        if (currencyTable.TryGetValue("anchor_asset", out var anchorAsset))
        {
            currency.AnchorAsset = anchorAsset.Get<string>();
        }

        if (currencyTable.TryGetValue("attestation_of_reserve", out var attestationOfReserve))
        {
            currency.AttestationOfReserve = attestationOfReserve.Get<string>();
        }

        if (currencyTable.TryGetValue("redemption_instructions", out var redemptionInstructions))
        {
            currency.RedemptionInstructions = redemptionInstructions.Get<string>();
        }

        if (currencyTable.TryGetValue("collateral_addresses", out var collateralAddresses))
        {
            var result = new List<string>();
            var addressesArray = collateralAddresses.Get<TomlArray>();
            foreach (var item in addressesArray.Items)
            {
                var address = item.Get<string>();
                if (!string.IsNullOrWhiteSpace(address))
                {
                    result.Add(address);
                }
            }
            currency.CollateralAddresses = result.AsReadOnly();
        }

        if (currencyTable.TryGetValue("collateral_address_messages", out var collateralAddressMessages))
        {
            var result = new List<string>();
            var messagesArray = collateralAddressMessages.Get<TomlArray>();
            foreach (var item in messagesArray.Items)
            {
                var message = item.Get<string>();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    result.Add(message);
                }
            }
            currency.CollateralAddressMessages = result.AsReadOnly();
        }

        if (currencyTable.TryGetValue("collateral_address_signatures", out var collateralAddressSignatures))
        {
            var result = new List<string>();
            var signaturesArray = collateralAddressSignatures.Get<TomlArray>();
            foreach (var item in signaturesArray.Items)
            {
                var signature = item.Get<string>();
                if (!string.IsNullOrWhiteSpace(signature))
                {
                    result.Add(signature);
                }
            }
            currency.CollateralAddressSignatures = result.AsReadOnly();
        }

        if (currencyTable.TryGetValue("regulated", out var regulated))
        {
            currency.Regulated = regulated.Get<bool>();
        }

        if (currencyTable.TryGetValue("approval_server", out var approvalServer))
        {
            currency.ApprovalServer = approvalServer.Get<string>();
        }

        if (currencyTable.TryGetValue("approval_criteria", out var approvalCriteria))
        {
            currency.ApprovalCriteria = approvalCriteria.Get<string>();
        }

        return currency;
    }

    private static List<Validator>? ParseValidators(TomlTable document)
    {
        if (!document.TryGetValue("VALIDATORS", out var validators))
        {
            return null;
        }

        var validatorsArray = validators.Get<TomlTableArray>();
        var validatorList = new List<Validator>();

        foreach (var validatorTable in validatorsArray.Items)
        {
            var validator = new Validator();

            if (validatorTable.TryGetValue("ALIAS", out var alias))
            {
                validator.Alias = alias.Get<string>();
            }

            if (validatorTable.TryGetValue("DISPLAY_NAME", out var displayName))
            {
                validator.DisplayName = displayName.Get<string>();
            }

            if (validatorTable.TryGetValue("PUBLIC_KEY", out var publicKey))
            {
                validator.PublicKey = publicKey.Get<string>();
            }

            if (validatorTable.TryGetValue("HOST", out var host))
            {
                validator.Host = host.Get<string>();
            }

            if (validatorTable.TryGetValue("HISTORY", out var history))
            {
                validator.History = history.Get<string>();
            }

            validatorList.Add(validator);
        }

        return validatorList;
    }
}

