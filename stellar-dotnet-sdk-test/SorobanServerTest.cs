using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.requests.sorobanrpc;
using stellar_dotnet_sdk.responses;
using stellar_dotnet_sdk.responses.operations;
using stellar_dotnet_sdk.responses.sorobanrpc;
using stellar_dotnet_sdk.xdr;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using LedgerFootprint = stellar_dotnet_sdk.LedgerFootprint;
using LedgerKey = stellar_dotnet_sdk.LedgerKey;
using SCSymbol = stellar_dotnet_sdk.SCSymbol;
using SCVal = stellar_dotnet_sdk.SCVal;
using SCVec = stellar_dotnet_sdk.SCVec;
using SorobanResources = stellar_dotnet_sdk.SorobanResources;
using SorobanTransactionData = stellar_dotnet_sdk.SorobanTransactionData;
using Transaction = stellar_dotnet_sdk.Transaction;
using TransactionResult = stellar_dotnet_sdk.responses.TransactionResult;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class SorobanServerTest
{
    private const string HelloContractWasmId = "waZQUG98IMj00Wquc/iU8wLNAR1+8zre9XLyCzT3ZT4=";
    private const string HelloContractId = "CDMTUCYPBMWUFESK2EZA6ZZMSEX3NNOMZEXZD2VVJGZ332DYTKCEBFI5";
    private readonly string _helloWasmPath = Path.GetFullPath("wasm/soroban_hello_world_contract.wasm");
    private readonly Server _server = new("https://horizon-testnet.stellar.org");
    private readonly SorobanServer _sorobanServer = new("https://soroban-testnet.stellar.org");
    private string _accountId = "GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA";
    private KeyPair _sourceAccount = KeyPair.FromSecretSeed("SDR4PTKMR5TAQQCL3RI2MLXXSXQDIR7DCAONQNQP6UCDZCD4OVRWXUHI");

    [TestInitialize]
    public async Task Setup()
    {
        Network.UseTestNetwork();
        // Generates a new _accountId in case the testnet has been reset
        try
        {
            await _sorobanServer.GetAccount(_accountId);
        }
        catch (AccountNotFoundException ex)
        {
            _sourceAccount = await CreateNewRandomAccountOnTestnet();
        }

        _accountId = _sourceAccount.AccountId;
    }

    [TestCleanup]
    public void Cleanup()
    {
        Network.Use(null);
        _server.Dispose();
        _sorobanServer.Dispose();
    }

    [TestMethod]
    [Order(0)]
    public async Task TestGetHealth()
    {
        var response = await _sorobanServer.GetHealth();
        Assert.AreEqual("healthy", response.Status);
    }

    [TestMethod]
    [Order(1)]
    public async Task TestGetNetwork()
    {
        var response = await _sorobanServer.GetNetwork();
        Assert.AreEqual("https://friendbot.stellar.org/", response.FriendbotUrl);
        Assert.AreEqual("Test SDF Network ; September 2015", response.Passphrase);
        Assert.AreEqual(20, response.ProtocolVersion);
    }

    [TestMethod]
    [Order(2)]
    public async Task TestUploadContract()
    {
        await UploadContract(_helloWasmPath);
    }

    [TestMethod]
    [Order(3)]
    public async Task TestExtendFootprint()
    {
        await ExtendFootprintTTL(HelloContractWasmId, 1000);
    }

    [TestMethod]
    [Order(4)]
    public async Task TestRestoreFootprint()
    {
        await RestoreFootprint(HelloContractWasmId);
    }

    [TestMethod]
    [Order(5)]
    public async Task TestCreateContract()
    {
        await CreateContract(HelloContractWasmId);
    }

    [TestMethod]
    [Order(6)]
    public async Task TestInvokeContract()
    {
        await InvokeContract(HelloContractId!);
    }

    [TestMethod]
    public async Task TestGetLatestLedger()
    {
        var response = await _sorobanServer.GetLatestLedger();
        Assert.AreEqual(20, response.ProtocolVersion);
        Assert.IsTrue(response.Sequence > 0);
        Assert.IsTrue(response.Id != null);
    }

    [TestMethod]
    public async Task TestGetAccountNotFound()
    {
        const string accountId = "GCZFMH32MF5EAWETZTKF3ZV5SEVJPI53UEMDNSW55WBR75GMZJU4U573";
        var ex = await Assert.ThrowsExceptionAsync<AccountNotFoundException>(() =>
            _sorobanServer.GetAccount(accountId));
        Assert.AreEqual($"Account ID {accountId} not found.", ex.Message);
    }

    [TestMethod]
    public async Task TestGetEvents()
    {
        var response = await _sorobanServer.GetLatestLedger();
        var latestLedger = response.Sequence;
        var eventFilter = new GetEventsRequest.EventFilter
        {
            Type = "contract"
        };
        var getEventsRequest = new GetEventsRequest
        {
            StartLedger = latestLedger - 1000,
            Filters = new[] { eventFilter }
        };

        var eventsResponse = await _sorobanServer.GetEvents(getEventsRequest);
        Assert.IsNotNull(eventsResponse.Events);
        Assert.IsTrue(eventsResponse.Events.Length > 0);
    }

    [TestMethod]
    public async Task TestGetAccount()
    {
        var testnetAccount = await _sorobanServer.GetAccount(_accountId);
        Assert.AreEqual(_accountId, testnetAccount.AccountId);
    }

    [TestMethod]
    [Order(7)]
    public async Task TestGetLedgerEntriesOfTypeContractData()
    {
        Assert.IsNotNull(HelloContractId);
        var ledgerKeyContractData = new LedgerKey[]
        {
            new LedgerKeyContractData(new SCContractId(HelloContractId!), new SCLedgerKeyContractInstance(),
                ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT))
        };
        var contractDataResponse = await _sorobanServer.GetLedgerEntries(ledgerKeyContractData);
        Assert.IsNotNull(contractDataResponse.Entries);
        Assert.AreEqual(1, contractDataResponse.Entries.Length);
        var contractData = LedgerEntryContractData.FromXdrBase64(contractDataResponse.Entries[0]!.Xdr);
        Assert.IsNotNull(contractData);
        Assert.AreEqual(HelloContractId, ((SCContractId)contractData.Contract).InnerValue);
    }

    private async Task GetEvents(long ledger, string contractId)
    {
        var eventFilter = new GetEventsRequest.EventFilter
        {
            Type = "diagnostic",
            ContractIds = new[] { contractId },
            Topics = new[] { new[] { "*", new SCSymbol("hello").ToXdrBase64() } }
        };
        var getEventsRequest = new GetEventsRequest
        {
            StartLedger = ledger,
            Filters = new[] { eventFilter }
        };

        var eventsResponse = await _sorobanServer.GetEvents(getEventsRequest);
        Assert.IsNotNull(eventsResponse.Events);
        Assert.AreEqual(1, eventsResponse.Events.Length);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeContractCode()
    {
        var ledgerKeyContractCodes = new LedgerKey[]
        {
            new LedgerKeyContractCode(HelloContractWasmId)
        };
        var contractCodes = await _sorobanServer.GetLedgerEntries(ledgerKeyContractCodes);
        Assert.IsNotNull(contractCodes.Entries);
        Assert.AreEqual(1, contractCodes.Entries.Length);
        var contractCode = LedgerEntryContractCode.FromXdrBase64(contractCodes.Entries[0].Xdr);
        Assert.IsNotNull(contractCode);
        Assert.AreEqual(HelloContractWasmId, Convert.ToBase64String(contractCode.Hash.InnerValue));
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeAccount()
    {
        var ledgerKeyAccounts = new LedgerKeyAccount[]
        {
            new("GBA2NHOV6A5OUEBLUVMU3GJRZ3TARTHMYEYDG7ENVNKUS3U7JW65OEVS"),
            new("GDAT5HWTGIU4TSSZ4752OUC4SABDLTLZFRPZUJ3D6LKBNEPA7V2CIG54")
        };
        var accounts = await _sorobanServer.GetLedgerEntries(ledgerKeyAccounts);
        Assert.IsNotNull(accounts.Entries);
        var account0 = LedgerEntryAccount.FromXdrBase64(accounts.Entries[0]!.Xdr);
        var account1 = LedgerEntryAccount.FromXdrBase64(accounts.Entries[1]!.Xdr);
        Assert.AreEqual(ledgerKeyAccounts[0].Account.AccountId, account0.Account.AccountId);
        Assert.AreEqual(ledgerKeyAccounts[1].Account.AccountId, account1.Account.AccountId);
    }

    private async Task<string> UploadContract(string wasmPath)
    {
        string? contractWasmId = null;
        var wasm = await File.ReadAllBytesAsync(wasmPath);

        // Load the account with the updated sequence number from Soroban server
        var account = await _sorobanServer.GetAccount(_accountId);
        var uploadOperation = new UploadContractOperation.Builder(wasm).SetSourceAccount(_sourceAccount).Build();
        var tx = new TransactionBuilder(account)
            .AddOperation(uploadOperation).Build();

        var simulateResponse = await SimulateAndUpdateTransaction(tx);
        AssertSimulateResponse(simulateResponse);

        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);
        var txHash = sendResponse.Hash;
        var getTransactionResponse = await PollTransaction(txHash);

        contractWasmId = getTransactionResponse.WasmId;

        Assert.IsNotNull(contractWasmId);

        await Task.Delay(5000);

        var operationResponse = await GetHorizonOperation(txHash, transactionEnvelopeXdrBase64);

        Assert.IsInstanceOfType(operationResponse, typeof(InvokeHostFunctionOperationResponse));

        return contractWasmId;
    }

    private async Task<OperationResponse> GetHorizonOperation(string txHash, string transactionEnvelopeXdrBase64)
    {
        // Get Transaction details from Horizon testnet
        var horizonTransactionResponse = await _server.Transactions.Transaction(txHash);
        // Check if the transaction is newly created
        Assert.IsTrue(DateTime.ParseExact(horizonTransactionResponse.CreatedAt, "yyyy-MM-ddTHH:mm:ssZ",
            CultureInfo.InvariantCulture) > DateTime.Now.AddMinutes(-15));
        Assert.AreEqual(1, horizonTransactionResponse.OperationCount);
        Assert.AreEqual(transactionEnvelopeXdrBase64, horizonTransactionResponse.EnvelopeXdr);

        var operations = await _server.Operations.ForTransaction(txHash).Execute();
        Assert.IsNotNull(operations.Records);
        Assert.IsTrue(operations.Records.Count > 0);

        var operation = operations.Records[0];
        return operation;
    }

    private async Task<SendTransactionResponse> SendTransaction(Transaction tx)
    {
        var sendResponse = await _sorobanServer.SendTransaction(tx);
        Assert.IsNull(sendResponse.ErrorResultXdr);
        Assert.IsNotNull(sendResponse.Hash);
        Assert.IsNotNull(sendResponse.Status);
        Assert.AreNotEqual(SendTransactionResponse.SendTransactionStatus.ERROR, sendResponse.Status);
        return sendResponse;
    }

    private async Task<SimulateTransactionResponse> SimulateAndUpdateTransaction(Transaction tx)
    {
        var simulateResponse = await _sorobanServer.SimulateTransaction(tx);

        Assert.IsNull(simulateResponse.Error);
        Assert.IsNotNull(simulateResponse.SorobanTransactionData);

        tx.SorobanTransactionData = simulateResponse.SorobanTransactionData;
        if (simulateResponse.SorobanAuthorization != null)
            tx.SetSorobanAuthorization(simulateResponse.SorobanAuthorization);
        tx.AddResourceFee(simulateResponse.MinResourceFee + 100000);
        tx.Sign(_sourceAccount);

        return simulateResponse;
    }

    private static void AssertSimulateResponse(SimulateTransactionResponse simulateResponse)
    {
        Assert.IsNull(simulateResponse.Error);
        Assert.IsNotNull(simulateResponse.Results);
        Assert.IsNotNull(simulateResponse.SorobanTransactionData);
        Assert.IsNotNull(simulateResponse.MinResourceFee);
        Assert.IsNotNull(simulateResponse.LatestLedger);
        Assert.IsNotNull(simulateResponse.Cost);
        Assert.IsNotNull(simulateResponse.Cost.CpuInstructions);
        Assert.IsNotNull(simulateResponse.Cost.MemoryBytes);
    }

    private async Task<Tuple<long, string>> CreateContract(string contractWasmId)
    {
        await Task.Delay(2000);
        var account = await _server.Accounts.Account(_accountId);
        var createContractOperation =
            new CreateContractOperation.Builder(account.AccountId, contractWasmId, null).Build();
        var tx = new TransactionBuilder(account).AddOperation(createContractOperation).Build();

        var simulateResponse = await SimulateAndUpdateTransaction(tx);
        AssertSimulateResponse(simulateResponse);

        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);

        var txHash = sendResponse.Hash;

        var getTransactionResponse = await PollTransaction(txHash);
        var contractId = getTransactionResponse.CreatedContractId;
        var ledger = getTransactionResponse.Ledger;
        Assert.IsNotNull(contractId);

        await Task.Delay(3000);

        var operationResponse = await GetHorizonOperation(txHash, transactionEnvelopeXdrBase64);
        Assert.IsInstanceOfType(operationResponse, typeof(InvokeHostFunctionOperationResponse));
        var hostFunctionOperationResponse = (InvokeHostFunctionOperationResponse)operationResponse;
        Assert.AreEqual("HostFunctionTypeHostFunctionTypeCreateContract", hostFunctionOperationResponse.Function);

        return new Tuple<long, string>(ledger, contractId);
    }

    [TestMethod]
    public async Task TestASeriesOfHostFunctionInvocation()
    {
        var contractWasmId = await UploadContract(_helloWasmPath);
        await ExtendFootprintTTL(contractWasmId, 10000);
        await RestoreFootprint(contractWasmId);
        var (ledger, createdContractId) = await CreateContract(contractWasmId);
        await InvokeContract(createdContractId);
        await GetEvents(ledger, createdContractId);
    }

    private async Task InvokeContract(string contractId)
    {
        Assert.IsNotNull(contractId);
        var account = await _server.Accounts.Account(_accountId);
        var address = new SCContractId(contractId);
        var arg = new SCSymbol("gents");
        var functionName = new SCSymbol("hello");
        var invokeContractOperation =
            new InvokeContractOperation.Builder(address, functionName, new SCVal[] { arg }).Build();
        var tx = new TransactionBuilder(account).AddOperation(invokeContractOperation).Build();

        var simulateResponse = await SimulateAndUpdateTransaction(tx);
        AssertSimulateResponse(simulateResponse);

        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);

        var txHash = sendResponse.Hash;
        Assert.IsNotNull(txHash);

        var getTransactionResponse = await PollTransaction(txHash);
        Assert.IsNotNull(getTransactionResponse);
        Assert.IsInstanceOfType(getTransactionResponse.ResultValue, typeof(SCVec));
        var vec = (SCVec)getTransactionResponse.ResultValue;
        Assert.AreEqual(2, vec.InnerValue.Length);
        Assert.AreEqual("Hello", ((SCSymbol)vec.InnerValue[0]).InnerValue);
        Assert.AreEqual("gents", ((SCSymbol)vec.InnerValue[1]).InnerValue);

        await Task.Delay(2000);

        var operationResponse = await GetHorizonOperation(txHash, transactionEnvelopeXdrBase64);
        Assert.IsInstanceOfType(operationResponse, typeof(InvokeHostFunctionOperationResponse));
        var hostFunctionOperationResponse =
            (InvokeHostFunctionOperationResponse)operationResponse;
        Assert.AreEqual("HostFunctionTypeHostFunctionTypeInvokeContract", hostFunctionOperationResponse.Function);
    }

    private async Task RestoreFootprint(string wasmId)
    {
        await Task.Delay(2000);
        var account = await _server.Accounts.Account(_accountId);
        var restoreOperation = new RestoreFootprintOperation.Builder().Build();
        var tx = new TransactionBuilder(account).AddOperation(restoreOperation).Build();
        var ledgerFootprint = new LedgerFootprint
        {
            ReadWrite = new LedgerKey[] { new LedgerKeyContractCode(wasmId) }
        };
        var resources = new SorobanResources(ledgerFootprint, 0, 0, 0);
        var transactionData = new SorobanTransactionData
        {
            Resources = resources,
            ResourceFee = 0
        };
        tx.SorobanTransactionData = transactionData;

        await SimulateAndUpdateTransaction(tx);

        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);
        var txHash = sendResponse.Hash;

        var getTransactionResponse = await PollTransaction(txHash);

        var operationResponse = await GetHorizonOperation(txHash, transactionEnvelopeXdrBase64);
        Assert.IsInstanceOfType(operationResponse, typeof(RestoreFootprintOperationResponse));
        Assert.AreEqual("restore_footprint", operationResponse.Type);
    }

    private async Task ExtendFootprintTTL(string wasmId, uint extentTo)
    {
        await Task.Delay(2000);
        var account = await _server.Accounts.Account(_accountId);

        var extendOperation = new ExtendFootprintOperation.Builder(extentTo).Build();
        var tx = new TransactionBuilder(account).AddOperation(extendOperation).Build();
        var ledgerFootprint = new LedgerFootprint
        {
            ReadOnly = new LedgerKey[] { new LedgerKeyContractCode(wasmId) }
        };

        var resources = new SorobanResources(ledgerFootprint, 0, 0, 0);
        var transactionData = new SorobanTransactionData
        {
            Resources = resources,
            ResourceFee = 0
        };

        tx.SorobanTransactionData = transactionData;

        await SimulateAndUpdateTransaction(tx);

        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);
        var txHash = sendResponse.Hash;

        var getTransactionResponse = await PollTransaction(txHash);

        await Task.Delay(2000);

        var operationResponse = await GetHorizonOperation(txHash, transactionEnvelopeXdrBase64);
        Assert.IsInstanceOfType(operationResponse, typeof(ExtendFootprintOperationResponse));
        Assert.AreEqual("extend_footprint_ttl", operationResponse.Type);
    }

    // Keep querying for the transaction until success or error
    private async Task<GetTransactionResponse> PollTransaction(string transactionHash)
    {
        var status = GetTransactionResponse.GetTransactionStatus.NOT_FOUND;
        GetTransactionResponse? transactionResponse = null;
        while (status == GetTransactionResponse.GetTransactionStatus.NOT_FOUND)
        {
            await Task.Delay(5000);
            transactionResponse = await _sorobanServer.GetTransaction(transactionHash);
            status = transactionResponse.Status;
            if (status == GetTransactionResponse.GetTransactionStatus.FAILED)
            {
                Assert.IsNotNull(transactionResponse.ResultMetaXdr);
                Assert.Fail();
            }
            else if (status == GetTransactionResponse.GetTransactionStatus.SUCCESS)
            {
                Assert.IsNotNull(transactionResponse.ResultXdr);
            }
        }

        return transactionResponse!;
    }

    private async Task<KeyPair> CreateNewRandomAccountOnTestnet()
    {
        var isSuccess = false;
        KeyPair account;
        do
        {
            account = KeyPair.Random();
            var fundResponse = await _server.TestNetFriendBot.FundAccount(account.AccountId).Execute();
            var result = TransactionResult.FromXdrBase64(fundResponse.ResultXdr);
            isSuccess = result.IsSuccess && result is TransactionResultSuccess;
        } while (!isSuccess);

        return account;
    }
}