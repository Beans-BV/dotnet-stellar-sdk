using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0001;

/// <summary>
///     General information from the stellar.toml file.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0001.md">SEP-0001</a>
/// </summary>
public class GeneralInformation
{
    /// <summary>
    ///     The version of SEP-1 your stellar.toml adheres to. This helps parsers know which fields to expect.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    ///     The passphrase for the specific Stellar network this infrastructure operates on.
    /// </summary>
    public string? NetworkPassphrase { get; set; }

    /// <summary>
    ///     The endpoint for clients to resolve stellar addresses for users on your domain via SEP-2 Federation Protocol.
    /// </summary>
    public string? FederationServer { get; set; }

    /// <summary>
    ///     The endpoint used for SEP-3 Compliance Protocol (deprecated).
    /// </summary>
    public string? AuthServer { get; set; }

    /// <summary>
    ///     The server used for SEP-6 Anchor/Client interoperability.
    /// </summary>
    public string? TransferServer { get; set; }

    /// <summary>
    ///     The server used for SEP-24 Anchor/Client interoperability.
    /// </summary>
    public string? TransferServerSep24 { get; set; }

    /// <summary>
    ///     The server used for SEP-12 Anchor/Client customer info transfer.
    /// </summary>
    public string? KycServer { get; set; }

    /// <summary>
    ///     The endpoint used for SEP-10 Web Authentication.
    /// </summary>
    public string? WebAuthEndpoint { get; set; }

    /// <summary>
    ///     The signing key is used for SEP-3 Compliance Protocol (deprecated) and SEP-10/SEP-45 Authentication Protocols.
    /// </summary>
    public string? SigningKey { get; set; }

    /// <summary>
    ///     Location of public-facing Horizon instance (if one is offered).
    /// </summary>
    public string? HorizonUrl { get; set; }

    /// <summary>
    ///     A list of Stellar accounts that are controlled by this domain.
    /// </summary>
    public List<string> Accounts { get; } = [];

    /// <summary>
    ///     The signing key is used for SEP-7 delegated signing.
    /// </summary>
    public string? UriRequestSigningKey { get; set; }

    /// <summary>
    ///     The server used for receiving SEP-31 direct fiat-to-fiat payments. Requires SEP-12 and hence a KYC_SERVER TOML attribute.
    /// </summary>
    public string? DirectPaymentServer { get; set; }

    /// <summary>
    ///     The server used for receiving SEP-38 requests.
    /// </summary>
    public string? AnchorQuoteServer { get; set; }

    /// <summary>
    ///     The endpoint used for SEP-45 Web Authentication (contract-based auth).
    /// </summary>
    public string? WebAuthForContractsEndpoint { get; set; }

    /// <summary>
    ///     The web authentication contract ID for SEP-45 Web Authentication.
    /// </summary>
    public string? WebAuthContractId { get; set; }
}

