using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Examples.Soroban.Helpers;
using SCString = StellarDotnetSdk.Soroban.SCString;
using SCVal = StellarDotnetSdk.Soroban.SCVal;

namespace StellarDotnetSdk.Examples.Soroban.Examples;

/// <summary>
///     Demonstrates atomic swap between two parties:
///     - Deploy atomic swap contract
///     - Deploy two token contracts (Token A and Token B)
///     - Mint tokens to both parties
///     - Execute the atomic swap
///     - The swap happens atomically (both succeed or both fail)
/// </summary>
internal static class AtomicSwapExample
{
    private const long AmountA = 1000000L; // 0.1 tokens with 7 decimals
    private const long AmountB = 2000000L; // 0.2 tokens with 7 decimals

    public static async Task Run(IAccountId partyA, IAccountId partyB, IAccountId tokenIssuer)
    {
        Console.WriteLine("=== Atomic Swap Example ===");

        var server = SorobanHelpers.CreateServer();

        Console.WriteLine("\n--- Step 1: Deploy Token A (Party A will swap this) ---");
        var tokenAContractId = await DeployTokenContract(
            partyA,
            "Token A",
            "TKNA",
            7);
        Console.WriteLine($"Token A contract ID: {tokenAContractId}");

        Console.WriteLine("\n--- Step 2: Deploy Token B (Party B will swap this) ---");
        var tokenBContractId = await DeployTokenContract(
            partyB,
            "Token B",
            "TKNB",
            7);
        Console.WriteLine($"Token B contract ID: {tokenBContractId}");

        Console.WriteLine("\n--- Step 3: Mint Token A to Party A ---");
        await MintTokens(server, partyA, partyA, tokenAContractId, AmountA);

        Console.WriteLine("\n--- Step 4: Mint Token B to Party B ---");
        await MintTokens(server, partyB, partyB, tokenBContractId, AmountB);

        Console.WriteLine("\n--- Step 5: Deploy Atomic Swap Contract ---");
        var swapWasmId = await UploadContractExample.Run(partyA, SorobanWasms.AtomicSwapWasmPath);
        var swapContractId = await CreateContractExample.Run(partyA, swapWasmId);
        Console.WriteLine($"Atomic swap contract ID: {swapContractId}");

        Console.WriteLine("\n--- Step 6: Party A Approves Swap Contract to Spend Token A ---");
        await ApproveSwapContract(server, partyA, tokenAContractId, swapContractId, AmountA);

        Console.WriteLine("\n--- Step 7: Party B Approves Swap Contract to Spend Token B ---");
        await ApproveSwapContract(server, partyB, tokenBContractId, swapContractId, AmountB);

        Console.WriteLine("\n--- Step 8: Execute Atomic Swap ---");
        await ExecuteSwap(server, partyA, partyB, tokenAContractId, tokenBContractId, swapContractId);

        Console.WriteLine("\n--- Step 9: Verify Swap Results ---");
        await VerifySwapResults(server, partyA, partyB, tokenAContractId, tokenBContractId);

        Console.WriteLine("\n✓ Atomic swap completed successfully!");
        Console.WriteLine("Key takeaways:");
        Console.WriteLine("  • Both transfers succeeded together (atomicity)");
        Console.WriteLine("  • No intermediary could steal funds");
        Console.WriteLine("  • No trust required between parties");
        Console.WriteLine("  • Smart contract enforced fair exchange");
    }

    private static async Task<string> DeployTokenContract(
        IAccountId admin,
        string name,
        string symbol,
        uint decimals)
    {
        var tokenWasmId = await UploadContractExample.Run(admin, SorobanWasms.TokenWasmPath);

        var constructorArgs = new SCVal[]
        {
            new ScAccountId(admin.AccountId),
            new SCUint32(decimals),
            new SCString(name),
            new SCString(symbol),
        };

        var contractId = await CreateContractExample.Run(admin, tokenWasmId, constructorArgs);
        return contractId;
    }

    private static async Task MintTokens(
        SorobanServer server,
        IAccountId minter,
        IAccountId recipient,
        string tokenContractId,
        long amount)
    {
        var minterAccount = await server.GetAccount(minter.AccountId);

        var mintArgs = new SCVal[]
        {
            new ScAccountId(recipient.AccountId),
            new SCInt128(amount.ToString()),
        };

        var mintOp = new InvokeContractOperation(tokenContractId, "mint", mintArgs, minter);
        var mintTx = new TransactionBuilder(minterAccount).AddOperation(mintOp).Build();
        await SorobanHelpers.SimulateAndUpdateTransaction(mintTx, minter);

        var mintResponse = await server.SendTransaction(mintTx);
        ArgumentNullException.ThrowIfNull(mintResponse.Hash);
        await SorobanHelpers.PollTransaction(mintResponse.Hash);
        Console.WriteLine($"Minted {amount} tokens to {recipient.AccountId}");
    }

    private static async Task ApproveSwapContract(
        SorobanServer server,
        IAccountId owner,
        string tokenContractId,
        string swapContractId,
        long amount)
    {
        var ownerAccount = await server.GetAccount(owner.AccountId);
        var latestLedger = (await server.GetLatestLedger()).Sequence;

        var approveArgs = new SCVal[]
        {
            new ScAccountId(owner.AccountId),
            new ScContractId(swapContractId),
            new SCInt128(amount.ToString()),
            new SCUint32((uint)latestLedger + 200000),
        };

        var approveOp = new InvokeContractOperation(tokenContractId, "approve", approveArgs, owner);
        var approveTx = new TransactionBuilder(ownerAccount).AddOperation(approveOp).Build();
        await SorobanHelpers.SimulateAndUpdateTransaction(approveTx, owner);

        var approveResponse = await server.SendTransaction(approveTx);
        ArgumentNullException.ThrowIfNull(approveResponse.Hash);
        await SorobanHelpers.PollTransaction(approveResponse.Hash);
        Console.WriteLine($"Approved swap contract to spend {amount} tokens");
    }

    private static async Task ExecuteSwap(
        SorobanServer server,
        IAccountId partyA,
        IAccountId partyB,
        string tokenAContractId,
        string tokenBContractId,
        string swapContractId)
    {
        Console.WriteLine("Party A initiates the swap:");
        Console.WriteLine($"  • Party A gives {AmountA} of Token A");
        Console.WriteLine($"  • Party A receives {AmountB} of Token B");
        Console.WriteLine($"  • Party B gives {AmountB} of Token B");
        Console.WriteLine($"  • Party B receives {AmountA} of Token A");

        var partyAAccount = await server.GetAccount(partyA.AccountId);

        var swapArgs = new SCVal[]
        {
            new ScAccountId(partyA.AccountId),
            new ScAccountId(partyB.AccountId),
            new ScContractId(tokenAContractId),
            new ScContractId(tokenBContractId),
            new SCInt128(AmountA.ToString()),
            new SCInt128(AmountB.ToString()),
            new SCInt128(AmountB.ToString()),
            new SCInt128(AmountA.ToString()),
        };

        var swapOp = new InvokeContractOperation(swapContractId, "swap", swapArgs, partyA);
        var swapTx = new TransactionBuilder(partyAAccount).AddOperation(swapOp).Build();
        await SorobanHelpers.SimulateAndUpdateTransaction(swapTx, partyA);

        Console.WriteLine("Sending atomic swap transaction...");
        var swapResponse = await server.SendTransaction(swapTx);
        ArgumentNullException.ThrowIfNull(swapResponse.Hash);
        await SorobanHelpers.PollTransaction(swapResponse.Hash);
    }

    private static async Task VerifySwapResults(
        SorobanServer server,
        IAccountId partyA,
        IAccountId partyB,
        string tokenAContractId,
        string tokenBContractId)
    {
        // Check Party A's Token B balance
        var partyAAccount = await server.GetAccount(partyA.AccountId);
        var balanceABArgs = new SCVal[] { new ScAccountId(partyA.AccountId) };
        var balanceABOp = new InvokeContractOperation(tokenBContractId, "balance", balanceABArgs, partyA);
        var balanceABTx = new TransactionBuilder(partyAAccount).AddOperation(balanceABOp).Build();
        var balanceABSim = await server.SimulateTransaction(balanceABTx);

        if (balanceABSim.Results != null && balanceABSim.Results.Length > 0)
        {
            var balanceXdr = balanceABSim.Results[0].Xdr;
            ArgumentNullException.ThrowIfNull(balanceXdr);
            var balance = (SCInt128)SCVal.FromXdrBase64(balanceXdr);
            Console.WriteLine($"Party A now has {balance.Lo} of Token B (expected {AmountB})");
        }

        // Check Party B's Token A balance
        var partyBAccount = await server.GetAccount(partyB.AccountId);
        var balanceBAArgs = new SCVal[] { new ScAccountId(partyB.AccountId) };
        var balanceBAOp = new InvokeContractOperation(tokenAContractId, "balance", balanceBAArgs, partyB);
        var balanceBATx = new TransactionBuilder(partyBAccount).AddOperation(balanceBAOp).Build();
        var balanceBASim = await server.SimulateTransaction(balanceBATx);

        if (balanceBASim.Results != null && balanceBASim.Results.Length > 0)
        {
            var balanceXdr = balanceBASim.Results[0].Xdr;
            ArgumentNullException.ThrowIfNull(balanceXdr);
            var balance = (SCInt128)SCVal.FromXdrBase64(balanceXdr);
            Console.WriteLine($"Party B now has {balance.Lo} of Token A (expected {AmountA})");
        }
    }
}

