# Stellar .NET SDK Examples

This section contains working examples demonstrating how to use the Stellar .NET SDK for both Horizon API and Soroban RPC. Each example is taken directly from our test code and thoroughly documented to help you understand how to implement similar functionality in your own applications.

If you are missing one example or guide, just write an issue in the [GitHub](https://github.com/Beans-BV/dotnet-stellar-sdk/issues) repository.

## Example Categories

### Horizon API Examples

Examples for working with Stellar's core network functionality:

- [**Account Management**](examples/horizon/account-management.md) - Creating, funding accounts and checking balances
- [**Assets**](examples/horizon/assets.md) - Managing trust lines and transferring assets
- [**XDR Encoding**](examples/horizon/xdr-encoding.md) - Encoding to and from base-64 encoded XDR
- [**Transactions**](examples/horizon/transactions.md) - Building, submitting, and managing transactions
- [**Sponsored Reserves**](examples/horizon/sponsored-reserves.md) - Working with Stellar's sponsorship features
- [**Fee-bump Transactions**](examples/horizon/fee-bump.md) - Working with Stellar's fee-bump transactions

### Soroban RPC Examples

Examples for working with Stellar's smart contract platform:

- [**Server Information**](examples/soroban/soroban-server-info.md) - Connecting to and querying Soroban server information
- [**Smart Contracts**](examples/soroban/smart-contracts.md) - Uploading, creating and invoking contract instances
- [**Ledger Entries**](examples/soroban/ledger-entries.md) - Retrieving data from the Stellar ledger

### Configuration Examples

Examples for configuring SDK behavior:

- [**HTTP Retry Configuration**](examples/http-retry.md) - Configuring automatic retry behavior for transient failures

## How to Use These Examples

Each example page follows a consistent format:

1. **Overview** - A brief explanation of the functionality
2. **Prerequisites** - Any required setup or knowledge
3. **Code Examples** - Complete, working code snippets
4. **Step-by-Step Explanation** - Detailed commentary on how the code works
5. **Variations** - Common modifications for different use cases
6. **Best Practices** - Recommendations for production use

All code examples are extracted directly from our testing suite, ensuring they work with the current version of the SDK.

## Running the Examples

To run these examples yourself:

1. Clone the repository:
   ```
   git clone https://github.com/Beans-BV/dotnet-stellar-sdk.git
   ```

2. Navigate to a specific example project directory:
   ```
   cd stellar-dotnet-sdk/Examples/Horizon
   ```

3. Build the examples:
   ```
   dotnet build
   ```

4. Run a specific example:
   ```
   dotnet run
   ```

## A Note on Testnet vs Mainnet

All examples use the Stellar Testnet by default. For production applications, you'll need to:

1. Change `Network.UseTestNetwork()` to `Network.UsePublicNetwork()`
2. Use real funding mechanisms instead of Friendbot
3. Carefully manage secret keys and security

Always thoroughly test your applications on the testnet before deploying to production.

**Happy coding!**