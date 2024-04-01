using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Claimant;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests.SorobanRpc;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Asset = StellarDotnetSdk.Assets.Asset;
using ChangeTrustAsset = StellarDotnetSdk.Assets.ChangeTrustAsset;
using CollectionAssert = NUnit.Framework.CollectionAssert;
using DiagnosticEvent = StellarDotnetSdk.Soroban.DiagnosticEvent;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using SorobanResources = StellarDotnetSdk.Soroban.SorobanResources;
using SorobanTransactionData = StellarDotnetSdk.Soroban.SorobanTransactionData;
using Transaction = StellarDotnetSdk.Transactions.Transaction;
using TransactionResult = StellarDotnetSdk.Responses.TransactionResult;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class SorobanServerTest
{
    private const string HelloContractWasmId = "waZQUG98IMj00Wquc/iU8wLNAR1+8zre9XLyCzT3ZT4=";
    private const string HelloContractId = "CDMTUCYPBMWUFESK2EZA6ZZMSEX3NNOMZEXZD2VVJGZ332DYTKCEBFI5";
    private readonly string _helloWasmPath = Path.GetFullPath("wasm/soroban_hello_world_contract.wasm");
    private readonly Server _server = new("https://horizon-testnet.stellar.org");

    private readonly SorobanServer _sorobanServer = new("https://soroban-testnet.stellar.org");

    private readonly KeyPair _sourceAccount =
        KeyPair.FromSecretSeed("SDR4PTKMR5TAQQCL3RI2MLXXSXQDIR7DCAONQNQP6UCDZCD4OVRWXUHI");

    private readonly KeyPair _targetAccount =
        KeyPair.FromSecretSeed("SDBNUIC2JMIYKGLJUFI743AQDWPBOWKG42GADHEY3FQDTQLJADYPQZTP");

    private Asset _asset =
        new AssetTypeCreditAlphaNum4("BBB", "GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA");

    // "GC3TDMFTMYZY2G4C77AKAVC3BR4KL6WMQ6K2MHISKDH2OHRFS7CVVEAF"
    private string TargetAccountId => _targetAccount.AccountId;

    // "GARRDNS77ZSI6PPXRBWTHIVX4RS2ULVBKNJXFRV77AZUNLDUNV2NAHJA";
    private string SourceAccountId => _sourceAccount.AccountId;

    [TestInitialize]
    public async Task Setup()
    {
        Network.UseTestNetwork();

        await TestNetUtil.CheckAndCreateAccountOnTestnet(SourceAccountId);
        await TestNetUtil.CheckAndCreateAccountOnTestnet(TargetAccountId);

        _asset = new AssetTypeCreditAlphaNum4("AAA", SourceAccountId);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Network.Use(null);
        _server.Dispose();
        _sorobanServer.Dispose();
    }

    [TestMethod]
    public async Task TestGetHealth()
    {
        var response = await _sorobanServer.GetHealth();
        Assert.AreEqual("healthy", response.Status);
    }

    [TestMethod]
    public async Task TestGetNetwork()
    {
        var response = await _sorobanServer.GetNetwork();
        Assert.AreEqual("https://friendbot.stellar.org/", response.FriendbotUrl);
        Assert.AreEqual("Test SDF Network ; September 2015", response.Passphrase);
        Assert.AreEqual(20, response.ProtocolVersion);
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
    public async Task TestUploadContract()
    {
        await UploadContract(_helloWasmPath);
    }

    [TestMethod]
    public async Task TestExtendFootprint()
    {
        await ExtendFootprintTTL(HelloContractWasmId, 1000);
    }

    [TestMethod]
    public async Task TestRestoreFootprint()
    {
        await RestoreFootprint(HelloContractWasmId);
    }

    [TestMethod]
    public async Task TestCreateContract()
    {
        await CreateContract(HelloContractWasmId);
    }

    [TestMethod]
    public async Task TestInvokeContract()
    {
        await RestoreFootprint(HelloContractId);
        await InvokeContract(HelloContractId);
    }

    [TestMethod]
    public async Task TestSimulateTransactionFails()
    {
        var account = await _server.Accounts.Account(SourceAccountId);
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
        var testnetAccount = await _sorobanServer.GetAccount(SourceAccountId);
        Assert.AreEqual(SourceAccountId, testnetAccount.AccountId);
    }

    [TestMethod]
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
        var account = await _sorobanServer.GetAccount(SourceAccountId);
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
        var account = await _server.Accounts.Account(SourceAccountId);
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
        var account = await _server.Accounts.Account(SourceAccountId);
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
        var account = await _server.Accounts.Account(SourceAccountId);
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
        Assert.IsTrue(getTransactionResponse.ApplicationOrder > 0);
        Assert.IsTrue(getTransactionResponse.CreatedAt > 0);
        Assert.IsNull(getTransactionResponse.CreatedContractId);
        Assert.IsFalse(getTransactionResponse.FeeBump);
        Assert.IsTrue(getTransactionResponse.LatestLedger > 0);
        Assert.IsTrue(getTransactionResponse.LatestLedgerCloseTime > 0);
        Assert.IsTrue(getTransactionResponse.Ledger > 0);
        Assert.IsTrue(getTransactionResponse.OldestLedger > 0);
        Assert.IsTrue(getTransactionResponse.OldestLedgerCloseTime > 0);
        Assert.IsNotNull(getTransactionResponse.EnvelopeXdr);
        Assert.IsInstanceOfType(getTransactionResponse.ResultValue, typeof(SCBool));
        var operationResponse = await GetHorizonOperation(txHash, transactionEnvelopeXdrBase64);
        Assert.IsInstanceOfType(operationResponse, typeof(RestoreFootprintOperationResponse));
        Assert.AreEqual("restore_footprint", operationResponse.Type);
    }

    private async Task ExtendFootprintTTL(string wasmId, uint extentTo)
    {
        await Task.Delay(2000);
        var account = await _server.Accounts.Account(SourceAccountId);

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
                Assert.IsNotNull(transactionResponse.TransactionMeta);
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

    private async Task<string> CreateClaimableBalance()
    {
        var account = await _sorobanServer.GetAccount(SourceAccountId);
        var operation = new CreateClaimableBalanceOperation.Builder(new AssetTypeNative(), "100",
            new Claimant.Claimant[]
            {
                new(SourceAccountId, new ClaimPredicateUnconditional())
            }).Build();
        var tx = new TransactionBuilder(account).AddOperation(operation).Build();
        tx.Sign(_sourceAccount);
        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(1, results.Count);
        var operationResult = results.First();
        Assert.IsInstanceOfType(operationResult, typeof(CreateClaimableBalanceSuccess));
        var balanceId = ((CreateClaimableBalanceSuccess)operationResult).BalanceId;
        Assert.IsNotNull(balanceId);
        return balanceId;
    }

    private async Task CreateLiquidityPoolShare(Asset assetA, Asset assetB)
    {
        var account = await _sorobanServer.GetAccount(SourceAccountId);

        var operation = new ChangeTrustOperation.Builder(assetA, assetB).Build();

        var tx = new TransactionBuilder(account).AddOperation(operation).Build();
        tx.Sign(_sourceAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(1, results.Count);
        var operationResult = results.First();
        Assert.IsInstanceOfType(operationResult, typeof(ChangeTrustSuccess));
    }

    private async Task CreateTrustline(Asset asset)
    {
        var account = await _server.Accounts.Account(TargetAccountId);

        var trustOperation = new ChangeTrustOperation.Builder(asset).Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(trustOperation)
            .Build();
        tx.Sign(_targetAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsNotNull(transactionResult.FeeCharged);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(1, results.Count);
        Assert.IsInstanceOfType(results[0], typeof(ChangeTrustSuccess));
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeData()
    {
        var account = await _sorobanServer.GetAccount(TargetAccountId);

        var manageDataOperation = new ManageDataOperation.Builder("passkey", "it's a secret").Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(manageDataOperation)
            .Build();
        tx.Sign(_targetAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());

        var ledgerKeyData = new LedgerKey[]
        {
            new LedgerKeyData(SourceAccountId, "passkey")
        };
        var dataResponse = await _sorobanServer.GetLedgerEntries(ledgerKeyData);

        Assert.IsNotNull(dataResponse.LatestLedger);
        Assert.IsNotNull(dataResponse.LedgerEntries);
        Assert.IsNotNull(dataResponse.LedgerKeys);
        Assert.AreEqual(1, dataResponse.LedgerEntries.Length);
        Assert.AreEqual(1, dataResponse.LedgerKeys.Length);
        var ledgerEntry = dataResponse.LedgerEntries[0] as LedgerEntryData;
        var ledgerKey = dataResponse.LedgerKeys[0] as LedgerKeyData;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);

        Assert.AreEqual(0U, ledgerEntry.LiveUntilLedger);
        Assert.IsTrue(ledgerEntry.LastModifiedLedgerSeq > 0);
        Assert.AreEqual("it's a secret", Encoding.UTF8.GetString(ledgerEntry.DataValue));
        Assert.AreEqual(SourceAccountId, ledgerKey.Account.AccountId);
        Assert.IsNull(ledgerEntry.LedgerExtensionV1);
        Assert.IsNull(ledgerEntry.DataExtension);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeOffer()
    {
        var account = await _sorobanServer.GetAccount(SourceAccountId);

        var nativeAsset = new AssetTypeNative();
        const string price = "1.5";
        var manageSellOfferOperation = new ManageSellOfferOperation.Builder(nativeAsset, _asset, "1", "1.5").Build();

        var tx = new TransactionBuilder(account)
            .AddOperation(manageSellOfferOperation)
            .Build();
        tx.Sign(_sourceAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());

        Assert.IsNotNull(txResponse.ResultXdr);
        var transactionResult = TransactionResult.FromXdrBase64(txResponse.ResultXdr);
        Assert.IsTrue(transactionResult.IsSuccess);
        Assert.IsInstanceOfType(transactionResult, typeof(TransactionResultSuccess));
        var results = ((TransactionResultSuccess)transactionResult).Results;
        Assert.AreEqual(1, results.Count);
        var operationResult = results.First();
        Assert.IsInstanceOfType(operationResult, typeof(ManageSellOfferCreated));
        var createdOffer = (ManageSellOfferCreated)operationResult;
        Assert.AreEqual(0, createdOffer.OffersClaimed.Length);
        var offerId = createdOffer.Offer.OfferId;
        Assert.IsNotNull(offerId);

        var ledgerKeyData = new LedgerKey[]
        {
            new LedgerKeyOffer(SourceAccountId, offerId)
        };
        var dataResponse = await _sorobanServer.GetLedgerEntries(ledgerKeyData);

        Assert.IsNotNull(dataResponse.LatestLedger);
        Assert.IsNotNull(dataResponse.LedgerEntries);
        Assert.IsNotNull(dataResponse.LedgerKeys);
        Assert.AreEqual(1, dataResponse.LedgerEntries.Length);
        Assert.AreEqual(1, dataResponse.LedgerKeys.Length);
        var ledgerEntry = dataResponse.LedgerEntries[0] as LedgerEntryOffer;
        var ledgerKey = dataResponse.LedgerKeys[0] as LedgerKeyOffer;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);

        Assert.AreEqual(0U, ledgerEntry.LiveUntilLedger);
        Assert.IsTrue(ledgerEntry.LastModifiedLedgerSeq > 0);
        Assert.AreEqual(10000000L, ledgerEntry.Amount);
        Assert.IsInstanceOfType(ledgerEntry.Buying, typeof(AssetTypeCreditAlphaNum4));
        Assert.IsInstanceOfType(ledgerEntry.Selling, typeof(AssetTypeNative));
        var buyingAsset = (AssetTypeCreditAlphaNum4)ledgerEntry.Buying;
        Assert.AreEqual(((AssetTypeCreditAlphaNum4)_asset).Code, buyingAsset.Code);
        Assert.AreEqual(((AssetTypeCreditAlphaNum4)_asset).Issuer, buyingAsset.Issuer);
        Assert.AreEqual(offerId, ledgerEntry.OfferID);
        Assert.AreEqual(SourceAccountId, ledgerEntry.SellerID.AccountId);
        Assert.AreEqual(Price.FromString(price), ledgerEntry.Price);
        Assert.AreEqual(0U, ledgerEntry.Flags);
        Assert.IsNull(ledgerEntry.LedgerExtensionV1);
        Assert.IsNull(ledgerEntry.OfferExtension);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeClaimableBalance()
    {
        var claimableBalanceId = await CreateClaimableBalance();

        var ledgerKeys = new LedgerKey[]
        {
            new LedgerKeyClaimableBalance(claimableBalanceId)
        };
        var response = await _sorobanServer.GetLedgerEntries(ledgerKeys);

        Assert.IsNotNull(response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntry = response.LedgerEntries[0] as LedgerEntryClaimableBalance;
        var ledgerKey = response.LedgerKeys[0] as LedgerKeyClaimableBalance;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);

        Assert.AreEqual(0U, ledgerEntry.LiveUntilLedger);
        Assert.IsTrue(ledgerEntry.LastModifiedLedgerSeq > 0);
        Assert.AreEqual(1000000000L, ledgerEntry.Amount);
        Assert.AreEqual("native", ledgerEntry.Asset.Type);
        Assert.IsNull(ledgerEntry.ClaimableBalanceEntryExtensionV1);
        Assert.AreEqual(1, ledgerEntry.Claimants.Length);
        var claimant = ledgerEntry.Claimants[0];
        Assert.AreEqual(SourceAccountId, claimant.Destination.AccountId);
        Assert.IsInstanceOfType(claimant.Predicate, typeof(ClaimPredicateUnconditional));
        CollectionAssert.AreEqual(Convert.FromHexString(claimableBalanceId), ledgerKey.BalanceId);

        var claimClaimableBalanceOperation = new ClaimClaimableBalanceOperation.Builder(claimableBalanceId).Build();
        var account = await _sorobanServer.GetAccount(SourceAccountId);
        var tx = new TransactionBuilder(account).AddOperation(claimClaimableBalanceOperation).Build();
        tx.Sign(_sourceAccount);
        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess());
    }

    // TODO Test liquidity pool with some deposits/withdrawals
    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeLiquidityPool()
    {
        var nativeAsset = new AssetTypeNative();

        await CreateLiquidityPoolShare(nativeAsset, _asset);

        var ledgerKeys = new LedgerKey[]
        {
            new LedgerKeyLiquidityPool(nativeAsset, _asset, 30)
        };
        var response = await _sorobanServer.GetLedgerEntries(ledgerKeys);

        Assert.IsNotNull(response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntry = response.LedgerEntries[0] as LedgerEntryLiquidityPool;
        var ledgerKey = response.LedgerKeys[0] as LedgerKeyLiquidityPool;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);

        Assert.AreEqual(0U, ledgerEntry.LiveUntilLedger);
        Assert.IsInstanceOfType(ledgerEntry.LiquidityPoolBody, typeof(LiquidityPoolConstantProduct));
        var constantProduct = (LiquidityPoolConstantProduct)ledgerEntry.LiquidityPoolBody;
        Assert.AreEqual(1, constantProduct.PoolSharesTrustLineCount);
        Assert.AreEqual(0L, constantProduct.ReserveA);
        Assert.AreEqual(0L, constantProduct.ReserveB);
        Assert.AreEqual(0L, constantProduct.TotalPoolShares);
        var parameters = constantProduct.Parameters;
        Assert.IsInstanceOfType(parameters.AssetA, typeof(AssetTypeNative));
        Assert.IsInstanceOfType(parameters.AssetB, typeof(AssetTypeCreditAlphaNum4));
        Assert.AreEqual("AAA", ((AssetTypeCreditAlphaNum4)parameters.AssetB).Code);
        Assert.AreEqual(SourceAccountId, ((AssetTypeCreditAlphaNum4)parameters.AssetB).Issuer);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeTrustline()
    {
        await CreateTrustline(_asset);

        var ledgerKeys = new LedgerKey[]
        {
            new LedgerKeyTrustline(TargetAccountId, _asset)
        };
        var response = await _sorobanServer.GetLedgerEntries(ledgerKeys);

        Assert.IsNotNull(response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntry = response.LedgerEntries[0] as LedgerEntryTrustline;
        var ledgerKey = response.LedgerKeys[0] as LedgerKeyTrustline;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);

        Assert.AreEqual(0U, ledgerEntry.LiveUntilLedger);
        Assert.IsTrue(ledgerEntry.LastModifiedLedgerSeq > 0);
        Assert.AreEqual(0L, ledgerEntry.Balance);
        Assert.AreEqual(TargetAccountId, ledgerEntry.Account.AccountId);
        Assert.AreEqual(1U, ledgerEntry.Flags);
        Assert.AreEqual(long.MaxValue, ledgerEntry.Limit);
        Assert.IsNull(ledgerEntry.TrustlineExtensionV1);
    }

    [TestMethod]
    public async Task TestGetTransactionFails()
    {
        const string randomInvalidHash = "b9d0b229fc4e09e8eb22d036171491e87b8d2086bf8b265874c8d182cb9c9020";
        var getTransactionResponse = await _sorobanServer.GetTransaction(randomInvalidHash);
        Assert.AreEqual(GetTransactionResponse.GetTransactionStatus.NOT_FOUND, getTransactionResponse.Status);
    }

    // TODO TestGetLedgerEntriesOfTypeTTL()
    // TODO TestGetLedgerEntriesOfTypeConfigSetting()
}