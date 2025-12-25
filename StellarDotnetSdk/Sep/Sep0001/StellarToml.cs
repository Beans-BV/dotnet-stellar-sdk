using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
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
///     <para>
///         <strong>HttpClient Usage:</strong> For production code, it is strongly recommended to pass a shared
///         <see cref="HttpClient" /> instance to the static methods (<see cref="FromDomainAsync" /> and
///         <see cref="CurrencyFromUrlAsync" />). If no HttpClient is provided, a new instance will be created
///         and disposed for each call, which is inefficient and can lead to socket exhaustion under load.
///         Create and reuse a single HttpClient instance (or use <see cref="System.Net.Http.IHttpClientFactory" />)
///         for multiple calls to these methods.
///     </para>
/// </summary>
public class StellarToml
{
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
    public IReadOnlyList<PointOfContact>? PointsOfContact { get; }

    /// <summary>
    ///     Currencies from the CURRENCIES list.
    /// </summary>
    public IReadOnlyList<Currency>? Currencies { get; }

    /// <summary>
    ///     Validators from the VALIDATORS list.
    /// </summary>
    public IReadOnlyList<Validator>? Validators { get; }

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
    ///     Fetches and parses stellar.toml from a domain's well-known location.
    ///     Automatically constructs the standard stellar.toml URL for the given domain
    ///     and fetches the content via HTTPS. The standard location is always:
    ///     `https://DOMAIN/.well-known/stellar.toml`
    /// </summary>
    /// <param name="domain">The domain name (without protocol). E.g., "example.com"</param>
    /// <param name="httpClient">
    ///     Optional HTTP client instance. <strong>Recommended:</strong> Pass a shared HttpClient instance
    ///     for production use to avoid creating a new client per call. If null, a new HttpClient will be
    ///     created and disposed after the request completes.
    /// </param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers to include in the request</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>StellarToml instance containing the parsed stellar.toml data</returns>
    /// <exception cref="StellarTomlException">Thrown when the stellar.toml file is not found or cannot be parsed</exception>
    public static async Task<StellarToml> FromDomainAsync(
        string domain,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        CancellationToken cancellationToken = default)
    {
        return await FromDomainAsync(domain, null, null, httpClient, httpRequestHeaders, cancellationToken)
            .ConfigureAwait(false);
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
    /// <param name="httpClient">
    ///     Optional HTTP client instance. <strong>Recommended:</strong> Pass a shared HttpClient instance
    ///     for production use to avoid creating a new client per call. If null, a new HttpClient will be
    ///     created and disposed after the request completes. If provided, resilienceOptions and bearerToken are ignored.
    /// </param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers to include in the request</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>StellarToml instance containing the parsed stellar.toml data</returns>
    /// <exception cref="StellarTomlException">Thrown when the stellar.toml file is not found or cannot be parsed</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token</exception>
    public static async Task<StellarToml> FromDomainAsync(
        string domain,
        HttpResilienceOptions? resilienceOptions,
        string? bearerToken,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        CancellationToken cancellationToken = default)
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

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new StellarTomlException(
                    $"Stellar toml not found, response status code {(int)response.StatusCode}");
            }

            var responseToml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return new StellarToml(responseToml);
        }
        catch (HttpRequestException ex)
        {
            throw new StellarTomlException($"Failed to fetch stellar.toml from {stellarTomlUri}", ex);
        }
        catch (OperationCanceledException)
        {
            throw;
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
    /// <param name="httpClient">
    ///     Optional HTTP client instance. <strong>Recommended:</strong> Pass a shared HttpClient instance
    ///     for production use to avoid creating a new client per call. If null, a new HttpClient will be
    ///     created and disposed after the request completes.
    /// </param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers to include in the request</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Currency instance containing the complete currency information</returns>
    /// <exception cref="StellarTomlException">Thrown when the currency TOML file is not found or cannot be parsed</exception>
    public static async Task<Currency> CurrencyFromUrlAsync(
        string tomlUrl,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        CancellationToken cancellationToken = default)
    {
        return await CurrencyFromUrlAsync(tomlUrl, null, null, httpClient, httpRequestHeaders, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Loads detailed currency information from an external TOML file.
    ///     Instead of embedding complete currency information directly in stellar.toml,
    ///     organizations can link to separate TOML files for each currency.
    /// </summary>
    /// <param name="tomlUrl">The full URL to the currency TOML file</param>
    /// <param name="resilienceOptions">Resilience options for HTTP requests. If null, default retry configuration is used.</param>
    /// <param name="bearerToken">(Optional) Bearer token in case the server requires it.</param>
    /// <param name="httpClient">
    ///     Optional HTTP client instance. <strong>Recommended:</strong> Pass a shared HttpClient instance
    ///     for production use to avoid creating a new client per call. If null, a new HttpClient will be
    ///     created and disposed after the request completes. If provided, resilienceOptions and bearerToken are ignored.
    /// </param>
    /// <param name="httpRequestHeaders">Optional custom HTTP headers to include in the request</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Currency instance containing the complete currency information</returns>
    /// <exception cref="StellarTomlException">Thrown when the currency TOML file is not found or cannot be parsed</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token</exception>
    public static async Task<Currency> CurrencyFromUrlAsync(
        string tomlUrl,
        HttpResilienceOptions? resilienceOptions,
        string? bearerToken,
        HttpClient? httpClient = null,
        Dictionary<string, string>? httpRequestHeaders = null,
        CancellationToken cancellationToken = default)
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

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new StellarTomlException(
                    $"Currency toml not found, response status code {(int)response.StatusCode}");
            }

            var responseToml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var document = Toml.ReadString(responseToml);
            return ParseCurrency(document);
        }
        catch (HttpRequestException ex)
        {
            throw new StellarTomlException($"Failed to fetch currency TOML from {uri}", ex);
        }
        catch (OperationCanceledException)
        {
            throw;
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
        IReadOnlyCollection<string>? accounts = null;
        if (document.TryGetValue("ACCOUNTS", out var accountsValue))
        {
            var result = new List<string>();
            var accountsArray = accountsValue.Get<TomlArray>();
            foreach (var item in accountsArray.Items)
            {
                var accountValue = item.Get<string>();
                if (!string.IsNullOrWhiteSpace(accountValue))
                {
                    result.Add(accountValue);
                }
            }
            accounts = result.AsReadOnly();
        }

        return new GeneralInformation
        {
            Version = document.TryGetValue("VERSION", out var version)
                ? version.Get<string>()
                : null,
            NetworkPassphrase = document.TryGetValue("NETWORK_PASSPHRASE", out var networkPassphrase)
                ? networkPassphrase.Get<string>()
                : null,
            FederationServer = document.TryGetValue("FEDERATION_SERVER", out var federationServer)
                ? federationServer.Get<string>()
                : null,
            AuthServer = document.TryGetValue("AUTH_SERVER", out var authServer)
                ? authServer.Get<string>()
                : null,
            TransferServer = document.TryGetValue("TRANSFER_SERVER", out var transferServer)
                ? transferServer.Get<string>()
                : null,
            TransferServerSep24 = document.TryGetValue("TRANSFER_SERVER_SEP0024", out var transferServerSep24)
                ? transferServerSep24.Get<string>()
                : null,
            KycServer = document.TryGetValue("KYC_SERVER", out var kycServer)
                ? kycServer.Get<string>()
                : null,
            WebAuthEndpoint = document.TryGetValue("WEB_AUTH_ENDPOINT", out var webAuthEndpoint)
                ? webAuthEndpoint.Get<string>()
                : null,
            SigningKey = document.TryGetValue("SIGNING_KEY", out var signingKey)
                ? signingKey.Get<string>()
                : null,
            HorizonUrl = document.TryGetValue("HORIZON_URL", out var horizonUrl)
                ? horizonUrl.Get<string>()
                : null,
            Accounts = accounts,
            UriRequestSigningKey = document.TryGetValue("URI_REQUEST_SIGNING_KEY", out var uriRequestSigningKey)
                ? uriRequestSigningKey.Get<string>()
                : null,
            DirectPaymentServer = document.TryGetValue("DIRECT_PAYMENT_SERVER", out var directPaymentServer)
                ? directPaymentServer.Get<string>()
                : null,
            AnchorQuoteServer = document.TryGetValue("ANCHOR_QUOTE_SERVER", out var anchorQuoteServer)
                ? anchorQuoteServer.Get<string>()
                : null,
            WebAuthForContractsEndpoint =
                document.TryGetValue("WEB_AUTH_FOR_CONTRACTS_ENDPOINT", out var webAuthForContractsEndpoint)
                    ? webAuthForContractsEndpoint.Get<string>()
                    : null,
            WebAuthContractId = document.TryGetValue("WEB_AUTH_CONTRACT_ID", out var webAuthContractId)
                ? webAuthContractId.Get<string>()
                : null,
        };
    }

    private static Documentation? ParseDocumentation(TomlTable document)
    {
        if (!document.TryGetValue("DOCUMENTATION", out var docTable))
        {
            return null;
        }

        var doc = docTable.Get<TomlTable>();

        return new Documentation
        {
            OrgName = doc.TryGetValue("ORG_NAME", out var orgName)
                ? orgName.Get<string>()
                : null,
            OrgDba = doc.TryGetValue("ORG_DBA", out var orgDba)
                ? orgDba.Get<string>()
                : null,
            OrgUrl = doc.TryGetValue("ORG_URL", out var orgUrl)
                ? orgUrl.Get<string>()
                : null,
            OrgLogo = doc.TryGetValue("ORG_LOGO", out var orgLogo)
                ? orgLogo.Get<string>()
                : null,
            OrgDescription = doc.TryGetValue("ORG_DESCRIPTION", out var orgDescription)
                ? orgDescription.Get<string>()
                : null,
            OrgPhysicalAddress = doc.TryGetValue("ORG_PHYSICAL_ADDRESS", out var orgPhysicalAddress)
                ? orgPhysicalAddress.Get<string>()
                : null,
            OrgPhysicalAddressAttestation =
                doc.TryGetValue("ORG_PHYSICAL_ADDRESS_ATTESTATION", out var orgPhysicalAddressAttestation)
                    ? orgPhysicalAddressAttestation.Get<string>()
                    : null,
            OrgPhoneNumber = doc.TryGetValue("ORG_PHONE_NUMBER", out var orgPhoneNumber)
                ? orgPhoneNumber.Get<string>()
                : null,
            OrgPhoneNumberAttestation =
                doc.TryGetValue("ORG_PHONE_NUMBER_ATTESTATION", out var orgPhoneNumberAttestation)
                    ? orgPhoneNumberAttestation.Get<string>()
                    : null,
            OrgKeybase = doc.TryGetValue("ORG_KEYBASE", out var orgKeybase)
                ? orgKeybase.Get<string>()
                : null,
            OrgTwitter = doc.TryGetValue("ORG_TWITTER", out var orgTwitter)
                ? orgTwitter.Get<string>()
                : null,
            OrgGithub = doc.TryGetValue("ORG_GITHUB", out var orgGithub)
                ? orgGithub.Get<string>()
                : null,
            OrgOfficialEmail = doc.TryGetValue("ORG_OFFICIAL_EMAIL", out var orgOfficialEmail)
                ? orgOfficialEmail.Get<string>()
                : null,
            OrgSupportEmail = doc.TryGetValue("ORG_SUPPORT_EMAIL", out var orgSupportEmail)
                ? orgSupportEmail.Get<string>()
                : null,
            OrgLicensingAuthority = doc.TryGetValue("ORG_LICENSING_AUTHORITY", out var orgLicensingAuthority)
                ? orgLicensingAuthority.Get<string>()
                : null,
            OrgLicenseType = doc.TryGetValue("ORG_LICENSE_TYPE", out var orgLicenseType)
                ? orgLicenseType.Get<string>()
                : null,
            OrgLicenseNumber = doc.TryGetValue("ORG_LICENSE_NUMBER", out var orgLicenseNumber)
                ? orgLicenseNumber.Get<string>()
                : null,
        };
    }

    private static IReadOnlyList<PointOfContact>? ParsePointsOfContact(TomlTable document)
    {
        if (!document.TryGetValue("PRINCIPALS", out var principals))
        {
            return null;
        }

        var principalsArray = principals.Get<TomlTableArray>();
        var pointsOfContact = new List<PointOfContact>();

        foreach (var principalTable in principalsArray.Items)
        {
            var pointOfContact = new PointOfContact
            {
                Name = principalTable.TryGetValue("name", out var name)
                    ? name.Get<string>()
                    : null,
                Email = principalTable.TryGetValue("email", out var email)
                    ? email.Get<string>()
                    : null,
                Keybase = principalTable.TryGetValue("keybase", out var keybase)
                    ? keybase.Get<string>()
                    : null,
                Telegram = principalTable.TryGetValue("telegram", out var telegram)
                    ? telegram.Get<string>()
                    : null,
                Twitter = principalTable.TryGetValue("twitter", out var twitter)
                    ? twitter.Get<string>()
                    : null,
                Github = principalTable.TryGetValue("github", out var github)
                    ? github.Get<string>()
                    : null,
                IdPhotoHash = principalTable.TryGetValue("id_photo_hash", out var idPhotoHash)
                    ? idPhotoHash.Get<string>()
                    : null,
                VerificationPhotoHash =
                    principalTable.TryGetValue("verification_photo_hash", out var verificationPhotoHash)
                        ? verificationPhotoHash.Get<string>()
                        : null,
            };

            pointsOfContact.Add(pointOfContact);
        }

        return pointsOfContact.AsReadOnly();
    }

    private static IReadOnlyList<Currency>? ParseCurrencies(TomlTable document)
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

        return currencyList.AsReadOnly();
    }

    private static Currency ParseCurrency(TomlTable currencyTable)
    {
        IReadOnlyCollection<string>? collateralAddresses = null;
        if (currencyTable.TryGetValue("collateral_addresses", out var collateralAddressesValue))
        {
            var result = new List<string>();
            var addressesArray = collateralAddressesValue.Get<TomlArray>();
            foreach (var item in addressesArray.Items)
            {
                var address = item.Get<string>();
                if (!string.IsNullOrWhiteSpace(address))
                {
                    result.Add(address);
                }
            }
            collateralAddresses = result.AsReadOnly();
        }

        IReadOnlyCollection<string>? collateralAddressMessages = null;
        if (currencyTable.TryGetValue("collateral_address_messages", out var collateralAddressMessagesValue))
        {
            var result = new List<string>();
            var messagesArray = collateralAddressMessagesValue.Get<TomlArray>();
            foreach (var item in messagesArray.Items)
            {
                var message = item.Get<string>();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    result.Add(message);
                }
            }
            collateralAddressMessages = result.AsReadOnly();
        }

        IReadOnlyCollection<string>? collateralAddressSignatures = null;
        if (currencyTable.TryGetValue("collateral_address_signatures", out var collateralAddressSignaturesValue))
        {
            var result = new List<string>();
            var signaturesArray = collateralAddressSignaturesValue.Get<TomlArray>();
            foreach (var item in signaturesArray.Items)
            {
                var signature = item.Get<string>();
                if (!string.IsNullOrWhiteSpace(signature))
                {
                    result.Add(signature);
                }
            }
            collateralAddressSignatures = result.AsReadOnly();
        }

        return new Currency
        {
            Toml = currencyTable.TryGetValue("toml", out var toml)
                ? toml.Get<string>()
                : null,
            Code = currencyTable.TryGetValue("code", out var code)
                ? code.Get<string>()
                : null,
            CodeTemplate = currencyTable.TryGetValue("code_template", out var codeTemplate)
                ? codeTemplate.Get<string>()
                : null,
            Issuer = currencyTable.TryGetValue("issuer", out var issuer)
                ? issuer.Get<string>()
                : null,
            Contract = currencyTable.TryGetValue("contract", out var contract)
                ? contract.Get<string>()
                : null,
            Status = currencyTable.TryGetValue("status", out var status)
                ? status.Get<string>()
                : null,
            DisplayDecimals = currencyTable.TryGetValue("display_decimals", out var displayDecimals)
                ? displayDecimals.Get<int>()
                : null,
            Name = currencyTable.TryGetValue("name", out var name)
                ? name.Get<string>()
                : null,
            Desc = currencyTable.TryGetValue("desc", out var desc)
                ? desc.Get<string>()
                : null,
            Conditions = currencyTable.TryGetValue("conditions", out var conditions)
                ? conditions.Get<string>()
                : null,
            Image = currencyTable.TryGetValue("image", out var image)
                ? image.Get<string>()
                : null,
            FixedNumber = currencyTable.TryGetValue("fixed_number", out var fixedNumber)
                ? fixedNumber.Get<int>()
                : null,
            MaxNumber = currencyTable.TryGetValue("max_number", out var maxNumber)
                ? maxNumber.Get<int>()
                : null,
            IsUnlimited = currencyTable.TryGetValue("is_unlimited", out var isUnlimited)
                ? isUnlimited.Get<bool>()
                : null,
            IsAssetAnchored = currencyTable.TryGetValue("is_asset_anchored", out var isAssetAnchored)
                ? isAssetAnchored.Get<bool>()
                : null,
            AnchorAssetType = currencyTable.TryGetValue("anchor_asset_type", out var anchorAssetType)
                ? anchorAssetType.Get<string>()
                : null,
            AnchorAsset = currencyTable.TryGetValue("anchor_asset", out var anchorAsset)
                ? anchorAsset.Get<string>()
                : null,
            AttestationOfReserve = currencyTable.TryGetValue("attestation_of_reserve", out var attestationOfReserve)
                ? attestationOfReserve.Get<string>()
                : null,
            RedemptionInstructions =
                currencyTable.TryGetValue("redemption_instructions", out var redemptionInstructions)
                    ? redemptionInstructions.Get<string>()
                    : null,
            CollateralAddresses = collateralAddresses,
            CollateralAddressMessages = collateralAddressMessages,
            CollateralAddressSignatures = collateralAddressSignatures,
            Regulated = currencyTable.TryGetValue("regulated", out var regulated)
                ? regulated.Get<bool>()
                : null,
            ApprovalServer = currencyTable.TryGetValue("approval_server", out var approvalServer)
                ? approvalServer.Get<string>()
                : null,
            ApprovalCriteria = currencyTable.TryGetValue("approval_criteria", out var approvalCriteria)
                ? approvalCriteria.Get<string>()
                : null,
        };
    }

    private static IReadOnlyList<Validator>? ParseValidators(TomlTable document)
    {
        if (!document.TryGetValue("VALIDATORS", out var validators))
        {
            return null;
        }

        var validatorsArray = validators.Get<TomlTableArray>();
        var validatorList = new List<Validator>();

        foreach (var validatorTable in validatorsArray.Items)
        {
            var validator = new Validator
            {
                Alias = validatorTable.TryGetValue("ALIAS", out var alias)
                    ? alias.Get<string>()
                    : null,
                DisplayName = validatorTable.TryGetValue("DISPLAY_NAME", out var displayName)
                    ? displayName.Get<string>()
                    : null,
                PublicKey = validatorTable.TryGetValue("PUBLIC_KEY", out var publicKey)
                    ? publicKey.Get<string>()
                    : null,
                Host = validatorTable.TryGetValue("HOST", out var host)
                    ? host.Get<string>()
                    : null,
                History = validatorTable.TryGetValue("HISTORY", out var history)
                    ? history.Get<string>()
                    : null,
            };

            validatorList.Add(validator);
        }

        return validatorList.AsReadOnly();
    }
}