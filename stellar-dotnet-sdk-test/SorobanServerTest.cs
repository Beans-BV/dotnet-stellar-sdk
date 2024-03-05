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
using Asset = stellar_dotnet_sdk.Asset;
using ChangeTrustAsset = stellar_dotnet_sdk.ChangeTrustAsset;
using CollectionAssert = NUnit.Framework.CollectionAssert;
using DiagnosticEvent = stellar_dotnet_sdk.DiagnosticEvent;
using LedgerFootprint = stellar_dotnet_sdk.LedgerFootprint;
using LedgerKey = stellar_dotnet_sdk.LedgerKey;
using SCBytes = stellar_dotnet_sdk.SCBytes;
using SCContractInstance = stellar_dotnet_sdk.SCContractInstance;
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
        catch (AccountNotFoundException)
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
    public async Task TestGetLatestLedger()
    {
        var response = await _sorobanServer.GetLatestLedger();
        Assert.AreEqual(20, response.ProtocolVersion);
        Assert.IsTrue(response.Sequence > 0);
        Assert.IsTrue(response.Id != null);
    }

    [TestMethod]
    [Order(3)]
    public async Task TestUploadContract()
    {
        await UploadContract(_helloWasmPath);
    }

    [TestMethod]
    [Order(4)]
    public async Task TestExtendFootprint()
    {
        await ExtendFootprintTTL(HelloContractWasmId, 1000);
    }

    [TestMethod]
    [Order(5)]
    public async Task TestRestoreFootprint()
    {
        await RestoreFootprint(HelloContractWasmId);
    }

    [TestMethod]
    [Order(6)]
    public async Task TestCreateContract()
    {
        await CreateContract(HelloContractWasmId);
    }

    [TestMethod]
    [Order(7)]
    public async Task TestInvokeContract()
    {
        // await RestoreFootprint(HelloContractId);
        await InvokeContract(HelloContractId);
    }

    [TestMethod]
    public async Task TestSimulateTransactionFails()
    {
        var account = await _server.Accounts.Account(_accountId);
        var address = new SCContractId(HelloContractId);
        var arg = new SCSymbol("gents");
        var functionName = new SCSymbol("hello1");
        var invokeContractOperation =
            new InvokeContractOperation.Builder(address, functionName, new SCVal[] { arg }).Build();
        var tx = new TransactionBuilder(account).AddOperation(invokeContractOperation).Build();

        var simulateResponse = await _sorobanServer.SimulateTransaction(tx);

        Assert.IsNull(simulateResponse.Results);
        Assert.IsNotNull(simulateResponse.Error);

        var diagnosticEventStrings = simulateResponse.Events;

        Assert.IsNotNull(diagnosticEventStrings);
        Assert.IsTrue(diagnosticEventStrings.Length > 0);
        foreach (var eventString in diagnosticEventStrings)
        {
            var @event = DiagnosticEvent.FromXdrBase64(eventString);
            Assert.IsNotNull(@event);
        }
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
        var eventFilter = new GetEventsRequest.EventFilter();
        var getEventsRequest = new GetEventsRequest
        {
            StartLedger = latestLedger - 1000,
            Filters = new[] { eventFilter }, Pagination = new GetEventsRequest.PaginationOptions
            {
                Limit = 10
            }
        };

        var eventsResponse = await _sorobanServer.GetEvents(getEventsRequest);
        Assert.IsNotNull(eventsResponse.Events);
        Assert.IsNotNull(eventsResponse.LatestLedger);
        Assert.AreEqual(10, eventsResponse.Events.Length);
        foreach (var @event in eventsResponse.Events)
        {
            Assert.IsNotNull(@event);
            Assert.IsNotNull(@event.Id);
            Assert.IsNotNull(@event.PagingToken);
            Assert.IsNotNull(@event.Type);
            Assert.IsNotNull(@event.LedgerClosedAt);
            Assert.IsNotNull(@event.Value);
            var value = SCVal.FromXdrBase64(@event.Value);
            Assert.IsNotNull(value);
            Assert.IsNotNull(@event.Topics);
            if (@event.Topics.Length <= 0) continue;
            foreach (var topic in @event.Topics)
            {
                var scVal = SCVal.FromXdrBase64(topic);
                Assert.IsNotNull(scVal);
            }
        }
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
            new LedgerKeyContractData(new SCContractId(HelloContractId), new SCLedgerKeyContractInstance(),
                ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT))
        };
        var contractDataResponse = await _sorobanServer.GetLedgerEntries(ledgerKeyContractData);

        Assert.IsNotNull(contractDataResponse.LatestLedger);
        Assert.IsNotNull(contractDataResponse.LedgerEntries);
        Assert.IsNotNull(contractDataResponse.LedgerKeys);
        Assert.AreEqual(1, contractDataResponse.LedgerEntries.Length);
        Assert.AreEqual(1, contractDataResponse.LedgerKeys.Length);
        var ledgerEntry = contractDataResponse.LedgerEntries[0] as LedgerEntryContractData;
        var ledgerKey = contractDataResponse.LedgerKeys[0] as LedgerKeyContractData;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);

        Assert.IsTrue(ledgerEntry.LiveUntilLedger > 0);
        Assert.IsTrue(ledgerEntry.LastModifiedLedgerSeq > 0);
        Assert.IsInstanceOfType(ledgerEntry.Key, typeof(SCLedgerKeyContractInstance));
        Assert.AreEqual(HelloContractId, ((SCContractId)ledgerKey.Contract).InnerValue);
        Assert.AreEqual(HelloContractId, ((SCContractId)ledgerEntry.Contract).InnerValue);
        Assert.IsNull(ledgerEntry.LedgerExtensionV1);
        Assert.AreEqual(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT, ledgerKey.Durability.InnerValue);
        Assert.AreEqual(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT,
            ledgerEntry.Durability.InnerValue);
        Assert.IsInstanceOfType(ledgerEntry.Value, typeof(SCContractInstance));
        var ledgerValue = (SCContractInstance)ledgerEntry.Value;
        Assert.IsInstanceOfType(ledgerValue.Executable, typeof(ContractExecutableWasm));
        var ledgerExecutable = (ContractExecutableWasm)ledgerValue.Executable;
        Assert.AreEqual(HelloContractWasmId, ledgerExecutable.WasmHash);
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
        var contractCodeResponse = await _sorobanServer.GetLedgerEntries(ledgerKeyContractCodes);
        Assert.IsNotNull(contractCodeResponse.LatestLedger);
        Assert.IsNotNull(contractCodeResponse.LedgerEntries);
        Assert.IsNotNull(contractCodeResponse.LedgerKeys);
        Assert.AreEqual(1, contractCodeResponse.LedgerEntries.Length);
        Assert.AreEqual(1, contractCodeResponse.LedgerKeys.Length);
        var ledgerEntry = contractCodeResponse.LedgerEntries[0] as LedgerEntryContractCode;
        var ledgerKey = contractCodeResponse.LedgerKeys[0] as LedgerKeyContractCode;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);

        Assert.IsTrue(ledgerEntry.LiveUntilLedger > 0);
        Assert.IsInstanceOfType(ledgerEntry.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(HelloContractWasmId, Convert.ToBase64String(ledgerEntry.Hash));
        Assert.IsNotNull(ledgerEntry.Code);
        Assert.IsTrue(ledgerEntry.Code.Length > 1);
        Assert.IsNull(ledgerEntry.LedgerExtensionV1);
        Assert.IsTrue(ledgerEntry.LastModifiedLedgerSeq > 0);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeAccount()
    {
        const string accountId1 = "GBA2NHOV6A5OUEBLUVMU3GJRZ3TARTHMYEYDG7ENVNKUS3U7JW65OEVS";
        const string accountId2 = "GDAT5HWTGIU4TSSZ4752OUC4SABDLTLZFRPZUJ3D6LKBNEPA7V2CIG54";
        var ledgerKeyAccounts = new LedgerKey[]
        {
            new LedgerKeyAccount(accountId1),
            new LedgerKeyAccount(accountId2)
        };
        var accountsResponse = await _sorobanServer.GetLedgerEntries(ledgerKeyAccounts);

        Assert.IsNotNull(accountsResponse.LatestLedger);
        Assert.IsNotNull(accountsResponse.LedgerEntries);
        Assert.IsNotNull(accountsResponse.LedgerKeys);
        Assert.AreEqual(2, accountsResponse.LedgerEntries.Length);
        Assert.AreEqual(2, accountsResponse.LedgerKeys.Length);
        var ledgerEntryA = accountsResponse.LedgerEntries[0] as LedgerEntryAccount;
        var ledgerKeyA = accountsResponse.LedgerKeys[0] as LedgerKeyAccount;
        var ledgerEntryB = accountsResponse.LedgerEntries[1] as LedgerEntryAccount;
        var ledgerKeyB = accountsResponse.LedgerKeys[1] as LedgerKeyAccount;
        Assert.IsNotNull(ledgerEntryA);
        Assert.IsNotNull(ledgerKeyA);
        Assert.IsNotNull(ledgerEntryB);
        Assert.IsNotNull(ledgerKeyB);

        Assert.AreEqual("GBA2NHOV6A5OUEBLUVMU3GJRZ3TARTHMYEYDG7ENVNKUS3U7JW65OEVS", ledgerEntryA.Account.AccountId);
        Assert.IsTrue(ledgerEntryA.SequenceNumber > 0);
        Assert.AreEqual(0U, ledgerEntryA.Flags);
        Assert.IsTrue(ledgerEntryA.Balance > 0);
        Assert.AreEqual(0, ledgerEntryA.Signers.Length);
        CollectionAssert.AreEqual(new byte[] { 1, 0, 0, 0 }, ledgerEntryA.Thresholds);
        Assert.IsNull(ledgerEntryA.InflationDest);
        Assert.AreEqual("", ledgerEntryA.HomeDomain);
        Assert.AreEqual(0U, ledgerEntryA.NumberSubEntries);
        Assert.IsTrue(ledgerEntryA.LastModifiedLedgerSeq > 0);
        Assert.IsNull(ledgerEntryA.LedgerExtensionV1);
        Assert.IsNull(ledgerEntryA.AccountExtensionV1);

        Assert.AreEqual("GDAT5HWTGIU4TSSZ4752OUC4SABDLTLZFRPZUJ3D6LKBNEPA7V2CIG54", ledgerEntryB.Account.AccountId);
        Assert.IsTrue(ledgerEntryB.SequenceNumber > 0);
        Assert.AreEqual(0U, ledgerEntryB.Flags);
        Assert.IsTrue(ledgerEntryB.Balance > 0);
        Assert.AreEqual(0, ledgerEntryB.Signers.Length);
        CollectionAssert.AreEqual(new byte[] { 1, 0, 0, 0 }, ledgerEntryB.Thresholds);
        Assert.IsNull(ledgerEntryB.InflationDest);
        Assert.AreEqual("", ledgerEntryB.HomeDomain);
        Assert.AreEqual(0U, ledgerEntryB.NumberSubEntries);
        Assert.IsTrue(ledgerEntryB.LastModifiedLedgerSeq > 0);
        var extensionV1B = ledgerEntryB.AccountExtensionV1;
        Assert.IsNotNull(extensionV1B);
        Assert.AreEqual(0, extensionV1B.Liabilities.Buying);
        Assert.AreEqual(0, extensionV1B.Liabilities.Selling);
        var extensionV2B = extensionV1B.ExtensionV2;
        Assert.IsNotNull(extensionV2B);
        Assert.AreEqual(0U, extensionV2B.NumberSponsored);
        Assert.AreEqual(0U, extensionV2B.NumberSponsoring);
        Assert.AreEqual(0U, extensionV2B.NumberSponsoring);
        var extensionV3B = extensionV2B.ExtensionV3;
        Assert.IsNotNull(extensionV3B);
        Assert.IsTrue(extensionV3B.SequenceLedger > 0);
        Assert.IsTrue(extensionV3B.SequenceTime > 0);

        Assert.AreEqual("GBA2NHOV6A5OUEBLUVMU3GJRZ3TARTHMYEYDG7ENVNKUS3U7JW65OEVS", ledgerKeyA.Account.AccountId);
        Assert.AreEqual("GDAT5HWTGIU4TSSZ4752OUC4SABDLTLZFRPZUJ3D6LKBNEPA7V2CIG54", ledgerEntryB.Account.AccountId);
    }

    private async Task<string> UploadContract(string wasmPath)
    {
        var wasm = await File.ReadAllBytesAsync(wasmPath);

        // Load the account with the updated sequence number from Soroban server
        var account = await _sorobanServer.GetAccount(_accountId);
        var uploadOperation = new UploadContractOperation.Builder(wasm).SetSourceAccount(_sourceAccount).Build();
        var tx = new TransactionBuilder(account)
            .AddOperation(uploadOperation).Build();

        var simulateResponse = await SimulateAndUpdateTransaction(tx);
        AssertSimulateResponse(simulateResponse);
        Assert.IsNotNull(simulateResponse.Results);
        Assert.AreEqual(1, simulateResponse.Results.Length);
        var xdrBase64 = simulateResponse.Results[0].Xdr;
        Assert.IsNotNull(xdrBase64);
        var result = (SCBytes)SCVal.FromXdrBase64(xdrBase64);
        Assert.AreEqual(HelloContractWasmId, Convert.ToBase64String(result.InnerValue));

        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);
        var txHash = sendResponse.Hash;
        Assert.IsNotNull(txHash);
        var getTransactionResponse = await PollTransaction(txHash);

        var contractWasmId = getTransactionResponse.WasmId;

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
        Assert.IsNotNull(sendResponse.LatestLedger);
        Assert.IsNotNull(sendResponse.LatestLedgerCloseTime);
        Assert.IsNotNull(sendResponse.Status);
        Assert.AreNotEqual(SendTransactionResponse.SendTransactionStatus.ERROR, sendResponse.Status);
        return sendResponse;
    }

    private async Task<SimulateTransactionResponse> SimulateAndUpdateTransaction(Transaction tx, KeyPair? signer = null)
    {
        var simulateResponse = await _sorobanServer.SimulateTransaction(tx);

        Assert.IsNotNull(simulateResponse.SorobanTransactionData);

        tx.SorobanTransactionData = simulateResponse.SorobanTransactionData;
        if (simulateResponse.SorobanAuthorization != null)
            tx.SetSorobanAuthorization(simulateResponse.SorobanAuthorization);
        Assert.IsNotNull(simulateResponse.MinResourceFee);
        tx.AddResourceFee(simulateResponse.MinResourceFee.Value + 100000);
        tx.Sign(signer ?? _sourceAccount);

        return simulateResponse;
    }

    private static void AssertSimulateResponse(SimulateTransactionResponse simulateResponse)
    {
        Assert.IsNotNull(simulateResponse.Results);
        Assert.IsNotNull(simulateResponse.SorobanTransactionData);
        Assert.IsNotNull(simulateResponse.MinResourceFee);
        Assert.IsNotNull(simulateResponse.LatestLedger);
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
        Assert.IsNotNull(txHash);
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

    [TestMethod]
    public async Task TestDeploySacWithAsset()
    {
        // Load the account with the updated sequence number from Soroban server
        var randomKeyPair = KeyPair.Random();
        var randomAccountId = randomKeyPair.AccountId;
        await _server.TestNetFriendBot.FundAccount(randomAccountId).Execute();
        await Task.Delay(3000);

        var randomAccount = await _sorobanServer.GetAccount(randomAccountId);

        var asset = Asset.CreateNonNativeAsset("VNDT", randomAccountId);
        var changeTrustAsset = ChangeTrustAsset.Create(asset);

        var changeTrustOperation = new ChangeTrustOperation.Builder(changeTrustAsset, ChangeTrustOperation.MaxLimit)
            .SetSourceAccount(_sourceAccount).Build();

        var paymentOperation = new PaymentOperation.Builder(_sourceAccount, asset, "200").Build();

        var tx = new TransactionBuilder(randomAccount).AddOperation(changeTrustOperation).AddOperation(paymentOperation)
            .Build();
        tx.Sign(randomKeyPair);
        tx.Sign(_sourceAccount);
        var submitResponse = await _server.SubmitTransaction(tx);

        Assert.IsNotNull(submitResponse);
        Assert.IsTrue(submitResponse.IsSuccess());

        randomAccount = await _sorobanServer.GetAccount(randomAccountId);

        var createContractOperation = new CreateContractOperation.Builder(asset).Build();

        tx = new TransactionBuilder(randomAccount).AddOperation(createContractOperation).Build();

        var simulateResponse = await SimulateAndUpdateTransaction(tx, randomKeyPair);
        AssertSimulateResponse(simulateResponse);

        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);

        var txHash = sendResponse.Hash;
        Assert.IsNotNull(txHash);
        await PollTransaction(txHash);

        await Task.Delay(3000);

        var operationResponse = await GetHorizonOperation(txHash, transactionEnvelopeXdrBase64);

        Assert.IsInstanceOfType(operationResponse, typeof(InvokeHostFunctionOperationResponse));
        var hostFunctionOperationResponse = (InvokeHostFunctionOperationResponse)operationResponse;
        Assert.AreEqual("HostFunctionTypeHostFunctionTypeCreateContract", hostFunctionOperationResponse.Function);
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
        Assert.IsNotNull(getTransactionResponse.ResultValue);
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

    /// <summary>
    ///     Restores the contract entry, if a wasmId is specified, the footprint should be a LedgerKeyContractCode, otherwise
    ///     it should be a LedgerKeyContractData
    /// </summary>
    /// <param name="id">could either be a wasmId or a contractId.</param>
    private async Task RestoreFootprint(string id)
    {
        await Task.Delay(2000);
        var account = await _server.Accounts.Account(_accountId);
        var restoreOperation = new RestoreFootprintOperation.Builder().Build();
        var tx = new TransactionBuilder(account).AddOperation(restoreOperation).Build();
        LedgerKey key;
        if (StrKey.IsValidContractId(id))
            key = new LedgerKeyContractData(new SCContractId(id), new SCLedgerKeyContractInstance(),
                ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT));
        else
            key = new LedgerKeyContractCode(id);

        tx.SorobanTransactionData = new SorobanTransactionData(key, false);

        await SimulateAndUpdateTransaction(tx);

        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);
        var txHash = sendResponse.Hash;
        Assert.IsNotNull(txHash);
        var getTransactionResponse = await PollTransaction(txHash);

        Assert.IsNotNull(getTransactionResponse);
        Assert.IsInstanceOfType(getTransactionResponse.ResultValue, typeof(SCBool));
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
        var transactionData = new SorobanTransactionData(resources, 0);

        tx.SorobanTransactionData = transactionData;

        await SimulateAndUpdateTransaction(tx);

        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);
        var txHash = sendResponse.Hash;
        Assert.IsNotNull(txHash);
        await PollTransaction(txHash);

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

        Assert.IsNotNull(transactionResponse);
        return transactionResponse;
    }

    private async Task<KeyPair> CreateNewRandomAccountOnTestnet()
    {
        bool isSuccess;
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