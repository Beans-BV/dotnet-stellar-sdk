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
    ///     Creates a new SorobanServer instance configured for testnet.
    /// </summary>
    public static SorobanServer CreateServer()
    {
        return new SorobanServer(TestNetSorobanUrl);
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

        if (simulateResponse.SorobanTransactionData != null)
        {
            tx.SetSorobanTransactionData(simulateResponse.SorobanTransactionData);
        }

        if (simulateResponse.SorobanAuthorization != null)
        {
            tx.SetSorobanAuthorization(simulateResponse.SorobanAuthorization);
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

