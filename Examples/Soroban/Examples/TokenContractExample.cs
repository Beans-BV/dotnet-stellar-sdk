using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Examples.Horizon;
using StellarDotnetSdk.Examples.Soroban.Helpers;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using SCString = StellarDotnetSdk.Soroban.SCString;
using SCVal = StellarDotnetSdk.Soroban.SCVal;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates a complete token contract lifecycle:
///     - Deploy a CAP-46-6 compliant token contract with constructor arguments
///     - Mint tokens to an account
///     - Transfer tokens between accounts
///     - Check balances
///     - Approve spending allowances
///     - Burn tokens
///     Note: This token contract uses a two-phase initialization:
///     1. Constructor (called during deployment): Sets the admin
///     2. Token metadata is set via constructor arguments
/// </summary>
internal static class TokenContractExample
{
    public static async Task Run(IAccountId admin, IAccountId recipient)
    {
        Console.WriteLine("=== Token Contract Example ===");

        var server = SorobanHelpers.CreateServer();

        Console.WriteLine("\n--- Upload and Deploy Token Contract ---");
        var tokenWasmId = await UploadContractExample.Run(admin, SorobanWasms.TokenWasmPath);

        Console.WriteLine("\n--- Deploy Token Contract with Constructor ---");
        var constructorArgs = new SCVal[]
        {
            new ScAccountId(admin.AccountId),
            new SCUint32(10),
            new SCString("test name"),
            new SCString("a symbol"),
        };

        var tokenContractId = await CreateContractExample.Run(admin, tokenWasmId, constructorArgs);
        Console.WriteLine($"Token contract ID: {tokenContractId}");

        Console.WriteLine("\n--- Step 2: Mint Tokens ---");
        await MintTokens(server, admin, recipient, tokenContractId);

        Console.WriteLine("\n--- Step 3: Check Balance ---");
        await CheckBalance(server, admin, recipient, tokenContractId);

        Console.WriteLine("\n--- Step 4: Transfer Tokens ---");
        await TransferTokens(server, admin, recipient, tokenContractId);

        Console.WriteLine("\n--- Step 5: Approve Allowance ---");
        await ApproveAllowance(server, admin, recipient, tokenContractId);

        Console.WriteLine("\n--- Step 6: Burn Tokens ---");
        await BurnTokens(server, admin, tokenContractId);

        Console.WriteLine("\n✓ Token contract example completed successfully!");
    }

    private static async Task MintTokens(
        SorobanServer server,
        IAccountId admin,
        IAccountId recipient,
        string tokenContractId)
    {
        var adminAccount = await server.GetAccount(admin.AccountId);
        var mintAmount = 1000000L;

        var mintArgs = new SCVal[]
        {
            new ScAccountId(recipient.AccountId),
            new SCInt128(mintAmount.ToString()),
        };

        var mintOp = new InvokeContractOperation(tokenContractId, "mint", mintArgs, admin);
        var mintTx = new TransactionBuilder(adminAccount).AddOperation(mintOp).Build();
        await SorobanHelpers.SimulateAndUpdateTransaction(mintTx, admin);

        var mintResponse = await server.SendTransaction(mintTx);
        ArgumentNullException.ThrowIfNull(mintResponse.Hash);
        await SorobanHelpers.PollTransaction(mintResponse.Hash);
        Console.WriteLine($"Minted {mintAmount} tokens to {recipient.AccountId}");
    }

    private static async Task CheckBalance(
        SorobanServer server,
        IAccountId admin,
        IAccountId recipient,
        string tokenContractId)
    {
        var adminAccount = await server.GetAccount(admin.AccountId);

        var balanceArgs = new SCVal[]
        {
            new ScAccountId(recipient.AccountId),
        };

        var balanceOp = new InvokeContractOperation(tokenContractId, "balance", balanceArgs, admin);
        var balanceTx = new TransactionBuilder(adminAccount).AddOperation(balanceOp).Build();
        var balanceSimulation = await server.SimulateTransaction(balanceTx);

        if (balanceSimulation.Results != null && balanceSimulation.Results.Length > 0)
        {
            var balanceXdr = balanceSimulation.Results[0].Xdr;
            ArgumentNullException.ThrowIfNull(balanceXdr);
            var balanceVal = (SCInt128)SCVal.FromXdrBase64(balanceXdr);
            Console.WriteLine($"Balance of {recipient.AccountId}: {balanceVal.Lo}");
        }
    }

    private static async Task TransferTokens(
        SorobanServer server,
        IAccountId admin,
        IAccountId recipient,
        string tokenContractId)
    {
        // Recipient transfers half their tokens back to admin
        // First we need to fund recipient account
        await HorizonExamples.FundAccountUsingFriendBot(recipient.AccountId);

        var recipientAccount = await server.GetAccount(recipient.AccountId);
        var transferAmount = 500000L; // Half of minted amount

        var transferArgs = new SCVal[]
        {
            new ScAccountId(recipient.AccountId),
            new ScAccountId(admin.AccountId),
            new SCInt128(transferAmount.ToString()),
        };

        var transferOp = new InvokeContractOperation(tokenContractId, "transfer", transferArgs, recipient);
        var transferTx = new TransactionBuilder(recipientAccount).AddOperation(transferOp).Build();
        await SorobanHelpers.SimulateAndUpdateTransaction(transferTx, recipient);

        Console.WriteLine("Sending 'Transfer token' transaction");
        var transferResponse = await server.SendTransaction(transferTx);
        ArgumentNullException.ThrowIfNull(transferResponse.Hash);
        await SorobanHelpers.PollTransaction(transferResponse.Hash);
        Console.WriteLine($"Transferred {transferAmount} tokens from recipient to admin");
    }

    private static async Task ApproveAllowance(
        SorobanServer server,
        IAccountId admin,
        IAccountId recipient,
        string tokenContractId)
    {
        var recipientAccount = await server.GetAccount(recipient.AccountId);
        var latestLedgerResponse = await server.GetLatestLedger();
        var latestLedger = latestLedgerResponse.Sequence;
        var approveAmount = 100000L;

        var approveArgs = new SCVal[]
        {
            new ScAccountId(recipient.AccountId),
            new ScAccountId(admin.AccountId),
            new SCInt128(approveAmount.ToString()),
            new SCUint32((uint)latestLedger + 120000),
        };

        var approveOp = new InvokeContractOperation(tokenContractId, "approve", approveArgs, recipient);
        var approveTx = new TransactionBuilder(recipientAccount).AddOperation(approveOp).Build();
        await SorobanHelpers.SimulateAndUpdateTransaction(approveTx, recipient);

        Console.WriteLine("Sending 'Approve allowance' transaction");
        var approveResponse = await server.SendTransaction(approveTx);
        ArgumentNullException.ThrowIfNull(approveResponse.Hash);
        await SorobanHelpers.PollTransaction(approveResponse.Hash);
        Console.WriteLine($"Approved {approveAmount} tokens for admin to spend from recipient's account");
    }

    private static async Task BurnTokens(SorobanServer server, IAccountId admin, string tokenContractId)
    {
        var adminAccount = await server.GetAccount(admin.AccountId);
        var burnAmount = 50000L;

        var burnArgs = new SCVal[]
        {
            new ScAccountId(admin.AccountId),
            new SCInt128(burnAmount.ToString()),
        };

        var burnOp = new InvokeContractOperation(tokenContractId, "burn", burnArgs, admin);
        var burnTx = new TransactionBuilder(adminAccount).AddOperation(burnOp).Build();
        await SorobanHelpers.SimulateAndUpdateTransaction(burnTx, admin);

        var burnResponse = await server.SendTransaction(burnTx);
        ArgumentNullException.ThrowIfNull(burnResponse.Hash);
        await SorobanHelpers.PollTransaction(burnResponse.Hash);
        Console.WriteLine($"Burned {burnAmount} tokens from admin account");
    }
}