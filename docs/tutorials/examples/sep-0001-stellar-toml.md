# SEP-0001: stellar.toml

This guide demonstrates how to work with stellar.toml files using the Stellar .NET SDK. The stellar.toml file is a standardized configuration file that organizations publish at `https://DOMAIN/.well-known/stellar.toml` to provide information about their Stellar integration.

## Understanding stellar.toml

The stellar.toml file serves several critical purposes:

- **Service Endpoints**: Declares service endpoints for SEP implementations (WebAuth, Transfer, KYC, etc.)
- **Organization Information**: Publishes organization information and contact details for transparency
- **Currencies**: Lists supported currencies/assets with their properties
- **Validators**: Declares validator nodes and their configuration
- **Account Linking**: Links Stellar accounts to a domain for identity verification

## Fetching stellar.toml from a Domain

The primary way to discover a domain's Stellar integration information is to fetch their stellar.toml file:

```csharp
using StellarDotnetSdk.Sep.Sep0001;

// Fetch and parse stellar.toml from a domain
var stellarToml = await StellarToml.FromDomainAsync("example.com");

// Access general information
Console.WriteLine($"WebAuth endpoint: {stellarToml.GeneralInformation.WebAuthEndpoint}");
Console.WriteLine($"Transfer server: {stellarToml.GeneralInformation.TransferServer}");
Console.WriteLine($"Federation server: {stellarToml.GeneralInformation.FederationServer}");
```

## Parsing stellar.toml from a String

You can also parse stellar.toml content directly from a string:

```csharp
var tomlContent = @"
VERSION=""2.0.0""
NETWORK_PASSPHRASE=""Public Global Stellar Network ; September 2015""
WEB_AUTH_ENDPOINT=""https://example.com/auth""
SIGNING_KEY=""GBWMCCC3NHSKLAOJDBKKYW7SSH2PFTTNVFKWSGLWGDLEBKLOVP5JLBBP""

[DOCUMENTATION]
ORG_NAME=""Example Organization""
ORG_URL=""https://example.com""
";

var stellarToml = new StellarToml(tomlContent);
Console.WriteLine(stellarToml.GeneralInformation.WebAuthEndpoint);
```

## Accessing Organization Documentation

The `Documentation` section contains information about the organization:

```csharp
var stellarToml = await StellarToml.FromDomainAsync("example.com");

if (stellarToml.Documentation != null)
{
    var doc = stellarToml.Documentation;
    Console.WriteLine($"Organization: {doc.OrgName}");
    Console.WriteLine($"DBA: {doc.OrgDba}");
    Console.WriteLine($"URL: {doc.OrgUrl}");
    Console.WriteLine($"Support email: {doc.OrgSupportEmail}");
    Console.WriteLine($"Official email: {doc.OrgOfficialEmail}");
    Console.WriteLine($"Description: {doc.OrgDescription}");
}
```

## Working with Currencies

The `Currencies` list contains information about all supported assets:

```csharp
var stellarToml = await StellarToml.FromDomainAsync("example.com");

if (stellarToml.Currencies != null)
{
    foreach (var currency in stellarToml.Currencies)
    {
        Console.WriteLine($"Currency: {currency.Code}");
        Console.WriteLine($"Issuer: {currency.Issuer}");
        Console.WriteLine($"Display decimals: {currency.DisplayDecimals}");
        
        if (currency.IsAssetAnchored == true)
        {
            Console.WriteLine($"Anchored to: {currency.AnchorAsset}");
            Console.WriteLine($"Anchor type: {currency.AnchorAssetType}");
        }
        
        if (!string.IsNullOrEmpty(currency.Desc))
        {
            Console.WriteLine($"Description: {currency.Desc}");
        }
    }
}
```

## Loading Linked Currency Information

Some currencies may link to external TOML files for more detailed information:

```csharp
var stellarToml = await StellarToml.FromDomainAsync("example.com");

if (stellarToml.Currencies != null)
{
    foreach (var currency in stellarToml.Currencies)
    {
        // Check if this currency links to an external TOML file
        if (!string.IsNullOrEmpty(currency.Toml))
        {
            // Load the full currency details from the external file
            var fullCurrency = await StellarToml.CurrencyFromUrlAsync(currency.Toml);
            
            Console.WriteLine($"Full currency details for {fullCurrency.Code}:");
            Console.WriteLine($"Description: {fullCurrency.Desc}");
            Console.WriteLine($"Issuer: {fullCurrency.Issuer}");
            
            if (fullCurrency.IsAssetAnchored == true)
            {
                Console.WriteLine($"Anchored to: {fullCurrency.AnchorAsset}");
            }
        }
    }
}
```

## Accessing Validator Information

The `Validators` list contains information about validator nodes:

```csharp
var stellarToml = await StellarToml.FromDomainAsync("example.com");

if (stellarToml.Validators != null)
{
    foreach (var validator in stellarToml.Validators)
    {
        Console.WriteLine($"Validator: {validator.DisplayName}");
        Console.WriteLine($"Alias: {validator.Alias}");
        Console.WriteLine($"Public Key: {validator.PublicKey}");
        Console.WriteLine($"Host: {validator.Host}");
        Console.WriteLine($"History: {validator.History}");
    }
}
```

## Accessing Points of Contact

The `PointsOfContact` list contains information about principals:

```csharp
var stellarToml = await StellarToml.FromDomainAsync("example.com");

if (stellarToml.PointsOfContact != null)
{
    foreach (var contact in stellarToml.PointsOfContact)
    {
        Console.WriteLine($"Name: {contact.Name}");
        Console.WriteLine($"Email: {contact.Email}");
        Console.WriteLine($"Keybase: {contact.Keybase}");
        Console.WriteLine($"Twitter: {contact.Twitter}");
        Console.WriteLine($"Github: {contact.Github}");
    }
}
```

## Using Custom HttpClient

For testing or when you need custom HTTP configuration, you can provide your own `HttpClient`:

```csharp
using System.Net.Http;

// Create a custom HttpClient with timeout
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};

// Use it when fetching stellar.toml
var stellarToml = await StellarToml.FromDomainAsync(
    "example.com",
    httpClient: httpClient
);
```

## Error Handling

The SDK throws `StellarTomlException` when there are issues fetching or parsing stellar.toml files:

```csharp
try
{
    var stellarToml = await StellarToml.FromDomainAsync("example.com");
    // Use stellar.toml...
}
catch (StellarTomlException ex)
{
    Console.WriteLine($"Failed to fetch or parse stellar.toml: {ex.Message}");
    // Handle error...
}
```

## Complete Example

Here's a complete example that fetches and displays all available information:

```csharp
using StellarDotnetSdk.Sep.Sep0001;

try
{
    var stellarToml = await StellarToml.FromDomainAsync("example.com");
    
    // General Information
    var info = stellarToml.GeneralInformation;
    Console.WriteLine($"Version: {info.Version}");
    Console.WriteLine($"Network: {info.NetworkPassphrase}");
    Console.WriteLine($"WebAuth: {info.WebAuthEndpoint}");
    Console.WriteLine($"Transfer Server: {info.TransferServer}");
    Console.WriteLine($"Federation Server: {info.FederationServer}");
    
    if (info.Accounts.Count > 0)
    {
        Console.WriteLine("Accounts:");
        foreach (var account in info.Accounts)
        {
            Console.WriteLine($"  - {account}");
        }
    }
    
    // Organization Documentation
    if (stellarToml.Documentation != null)
    {
        var doc = stellarToml.Documentation;
        Console.WriteLine($"\nOrganization: {doc.OrgName}");
        Console.WriteLine($"URL: {doc.OrgUrl}");
        Console.WriteLine($"Support: {doc.OrgSupportEmail}");
    }
    
    // Currencies
    if (stellarToml.Currencies != null && stellarToml.Currencies.Count > 0)
    {
        Console.WriteLine("\nCurrencies:");
        foreach (var currency in stellarToml.Currencies)
        {
            Console.WriteLine($"  - {currency.Code} ({currency.Issuer})");
        }
    }
    
    // Validators
    if (stellarToml.Validators != null && stellarToml.Validators.Count > 0)
    {
        Console.WriteLine("\nValidators:");
        foreach (var validator in stellarToml.Validators)
        {
            Console.WriteLine($"  - {validator.DisplayName} ({validator.Alias})");
        }
    }
}
catch (StellarTomlException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Additional Resources

- [SEP-0001 Specification](https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0001.md)
- [Stellar Anchor Validator](https://anchor-tests.stellar.org/) - Tool to validate your stellar.toml file
- [stellar.toml Checker](https://stellar.sui.li) - Community-supported checker

