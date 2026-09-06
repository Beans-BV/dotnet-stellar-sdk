using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using Transaction = StellarDotnetSdk.Transactions.Transaction;

namespace StellarDotnetSdk.Examples.Soroban.Helpers;

/// <summary>
///     Shared constants and helper methods for Soroban examples.
///     Centralizes common functionality to maintain DRY principles.
/// </summary>
internal static class SorobanHelpers
{
    public const string TestNetSorobanUrl = "https://soroban-testnet.stellar.org";

    /// <summary>
    ///     Creates a new StellarRpcServer instance configured for testnet.
    /// </summary>
    public static StellarRpcServer CreateServer()
    {
        return new StellarRpcServer(TestNetSorobanUrl);
    }

    /// <summary>
    ///     Polls for transaction completion using the GetTransaction endpoint.
    ///     Continues polling until the transaction succeeds, fails, or is not found.
    /// </summary>
    /// <param name="transactionHash">The hash of the transaction to poll.</param>
    /// <returns>The final transaction response.</returns>
    public static async Task<GetTransactionResponse> PollTransaction(string transactionHash)
    {
        var server = CreateServer();
        var status = TransactionInfo.TransactionStatus.NOT_FOUND;
        GetTransactionResponse? transactionResponse = null;

        while (status == TransactionInfo.TransactionStatus.NOT_FOUND)
        {
            await Task.Delay(3000);
            Console.WriteLine($"Fetching details for transaction {transactionHash}");
            transactionResponse = await server.GetTransaction(transactionHash);

            status = transactionResponse.Status;

            switch (status)
            {
                case TransactionInfo.TransactionStatus.FAILED:
                    Console.WriteLine($"Transaction {transactionHash} failed");
                    break;
                case TransactionInfo.TransactionStatus.SUCCESS:
                    ArgumentNullException.ThrowIfNull(transactionResponse.ResultXdr);
                    Console.WriteLine($"Transaction {transactionHash} was successful");
                    return transactionResponse;
                default:
                    Console.WriteLine("Transaction not updated on-chain. Waiting for a moment before retrying..");
                    break;
            }
        }

        ArgumentNullException.ThrowIfNull(transactionResponse);
        return transactionResponse;
    }

    /// <summary>
    ///     Simulates a transaction and updates it with the simulation results.
    ///     Sets transaction data, authorization, resource fee, and signs the transaction.
    /// </summary>
    /// <param name="tx">The transaction to simulate and update.</param>
    /// <param name="signer">The key pair to sign the transaction with.</param>
    /// <returns>The simulation response.</returns>
    public static async Task<SimulateTransactionResponse> SimulateAndUpdateTransaction(
        Transaction tx,
        IAccountId signer)
    {
        var server = CreateServer();
        var simulateResponse = await server.SimulateTransaction(tx);

        // Read each property ONCE into a local: both decode the server's base64 XDR on every access, so
        // `if (x.SorobanTransactionData != null) { use(x.SorobanTransactionData); }` decodes twice, and the
        // `!= null` test itself throws InvalidDataException on a malformed blob rather than yielding null.
        var transactionData = simulateResponse.SorobanTransactionData;
        if (transactionData != null)
        {
            tx.SetSorobanTransactionData(transactionData);
        }

        // Probe presence on Results[0].Auth, not on SorobanAuthorization. Reading that property decodes every
        // entry's base64 XDR, so on a malformed blob it throws InvalidDataException rather than answering
        // null — which is what makes it unusable as a guard. Going through Results keeps the decode to the
        // one place the value is used, so a caller can wrap that line to handle a malformed entry.
        // `Results[0]` is null-checked because the array is not copied on assignment: code still holding the
        // reference can write a null into it after deserialization, which is why SorobanAuthorization's own
        // getter keeps the same check.
        //
        // This still cannot tell "no authorization required" apart from "the reply carried no results":
        // SorobanAuthorization answers null for both, and an empty `results` is legitimate when simulating an
        // operation other than InvokeHostFunction, so both are treated here as "nothing to attach".
        var result = simulateResponse.Results is { Length: > 0 } results ? results[0] : null;
        if (result?.Auth != null)
        {
            // Non-null by construction: derived from exactly the entries just checked.
            tx.SetSorobanAuthorization(simulateResponse.SorobanAuthorization!);
        }

        tx.AddResourceFee((simulateResponse.MinResourceFee ?? 0) + 100000);
        tx.Sign(signer);

        return simulateResponse;
    }

    /// <summary>
    ///     Creates a ledger key for contract data (instance) lookup.
    /// </summary>
    /// <param name="contractId">The contract ID.</param>
    /// <returns>A ledger key for the contract data.</returns>
    public static LedgerKey CreateLedgerKeyContractData(string contractId)
    {
        var scContractId = new ScContractId(contractId);

        var contractDataDurability =
            ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT);

        var result = new LedgerKeyContractData(
            scContractId,
            new SCLedgerKeyContractInstance(),
            contractDataDurability
        );

        return result;
    }
}