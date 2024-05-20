using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Claimants;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.LedgerEntries;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests.SorobanRpc;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Asset = StellarDotnetSdk.Assets.Asset;
using Claimant = StellarDotnetSdk.Claimants.Claimant;
using CollectionAssert = NUnit.Framework.CollectionAssert;
using DiagnosticEvent = StellarDotnetSdk.Soroban.DiagnosticEvent;
using LedgerFootprint = StellarDotnetSdk.Soroban.LedgerFootprint;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using SCBytes = StellarDotnetSdk.Soroban.SCBytes;
using SCContractInstance = StellarDotnetSdk.Soroban.SCContractInstance;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using SCVec = StellarDotnetSdk.Soroban.SCVec;
using SorobanResources = StellarDotnetSdk.Soroban.SorobanResources;
using SorobanTransactionData = StellarDotnetSdk.Soroban.SorobanTransactionData;
using Transaction = StellarDotnetSdk.Transactions.Transaction;
using TransactionResult = StellarDotnetSdk.Responses.Results.TransactionResult;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class SorobanServerTest
{
    private const string HelloContractWasmId = "waZQUG98IMj00Wquc/iU8wLNAR1+8zre9XLyCzT3ZT4=";
    private const string HelloContractId = "CDMTUCYPBMWUFESK2EZA6ZZMSEX3NNOMZEXZD2VVJGZ332DYTKCEBFI5";
    private readonly string _helloWasmPath = Path.GetFullPath("TestData/Wasm/soroban_hello_world_contract.wasm");
    private readonly Server _server = new("https://horizon-testnet.stellar.org");

    private readonly SorobanServer _sorobanServer = new("https://soroban-testnet.stellar.org");

    private readonly KeyPair _sourceAccount =
        KeyPair.FromSecretSeed("SBQZZETKBHMRVNPEM7TMYAXORIRIDBBS6HD43C3PFH75SI54QAC6YTE2");

    private readonly KeyPair _targetAccount =
        KeyPair.FromSecretSeed("SBV33ITENGZRQ3UEUY5XD3NOBHHSGZY2ADF2OQ7JC2FR2S3BV3DSHEGC");

    private Asset _asset =
        new AssetTypeCreditAlphaNum4("XXA", "GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS");

    // "GDUFELVZEZ3CX5PLYJAGPZ7CIM3HTVAD2JRHKXTGK4N5B2ADCALW7NGW"
    private string TargetAccountId => _targetAccount.AccountId;

    // "GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS";
    private string SourceAccountId => _sourceAccount.AccountId;

    [TestInitialize]
    public async Task Setup()
    {
        Network.UseTestNetwork();

        await Utils.CheckAndCreateAccountOnTestnet(SourceAccountId);
        await Utils.CheckAndCreateAccountOnTestnet(TargetAccountId);

        _asset = new AssetTypeCreditAlphaNum4("XXA", SourceAccountId);
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
        Assert.IsTrue(response.LatestLedger > 0);
        Assert.IsTrue(response.OldestLedger > 0);
        Assert.IsTrue(response.LedgerRetentionWindow > 0);
    }

    [TestMethod]
    public async Task TestGetNetwork()
    {
        var response = await _sorobanServer.GetNetwork();
        Assert.AreEqual("https://friendbot.stellar.org/", response.FriendbotUrl);
        Assert.AreEqual("Test SDF Network ; September 2015", response.Passphrase);
        Assert.AreEqual(21, response.ProtocolVersion);
    }

    [TestMethod]
    public async Task TestGetLatestLedger()
    {
        var response = await _sorobanServer.GetLatestLedger();
        Assert.AreEqual(21, response.ProtocolVersion);
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
        await ExtendFootprintTtl(HelloContractWasmId, 1000);
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
            new InvokeContractOperation(address, functionName, new SCVal[] { arg });
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
        var eventFilter = new GetEventsRequest.EventFilter
        {
            Type = "contract"
        };
        var getEventsRequest = new GetEventsRequest
        {
            StartLedger = latestLedger - 1000,
            Filters = [eventFilter], Pagination = new GetEventsRequest.PaginationOptions
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
        Assert.IsNotNull(ledgerEntry.ContractCodeExtensionV1);
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
        var uploadOperation = new UploadContractOperation(wasm, _sourceAccount);
        var tx = new TransactionBuilder(account).AddOperation(uploadOperation).Build();

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

        await Task.Delay(2000);

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
        var createContractOperation = CreateContractOperation.FromAddress(contractWasmId, account.AccountId);
        var tx = new TransactionBuilder(account).AddOperation(createContractOperation).Build();

        var simulateResponse = await SimulateAndUpdateTransaction(tx);
        AssertSimulateResponse(simulateResponse);
        var stateChanges = simulateResponse.StateChanges;
        Assert.IsNotNull(stateChanges);
        Assert.AreEqual(1, stateChanges.Length);
        Assert.AreEqual("created", stateChanges[0].Type);
        Assert.IsNotNull(stateChanges[0].Key);
        Assert.IsNull(stateChanges[0].Before);
        Assert.IsNotNull(stateChanges[0].After);
        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);

        var txHash = sendResponse.Hash;
        Assert.IsNotNull(txHash);
        var getTransactionResponse = await PollTransaction(txHash);
        var contractId = getTransactionResponse.CreatedContractId;
        var ledger = getTransactionResponse.Ledger;
        Assert.IsNotNull(contractId);

        await Task.Delay(1000);

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
        await ExtendFootprintTtl(contractWasmId, 10000);
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

        var changeTrustOperation = new ChangeTrustOperation(asset, ChangeTrustOperation.MaxLimit, _sourceAccount);

        var paymentOperation = new PaymentOperation(_sourceAccount, asset, "200");

        var tx = new TransactionBuilder(randomAccount).AddOperation(changeTrustOperation).AddOperation(paymentOperation)
            .Build();
        tx.Sign(randomKeyPair);
        tx.Sign(_sourceAccount);
        var submitResponse = await _server.SubmitTransaction(tx);

        Assert.IsNotNull(submitResponse);
        Assert.IsTrue(submitResponse.IsSuccess);

        randomAccount = await _sorobanServer.GetAccount(randomAccountId);

        var createContractOperation = CreateContractOperation.FromAsset(asset);

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
            new InvokeContractOperation(address, functionName, new SCVal[] { arg });
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
        var account = await _server.Accounts.Account(SourceAccountId);
        var restoreOperation = new RestoreFootprintOperation();
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

    private async Task ExtendFootprintTtl(string wasmId, uint extentTo)
    {
        var account = await _server.Accounts.Account(SourceAccountId);

        var extendOperation = new ExtendFootprintOperation(extentTo);
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

        await Task.Delay(1000);

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
            else
            {
                await Task.Delay(500);
            }
        }

        Assert.IsNotNull(transactionResponse);
        return transactionResponse;
    }

    private async Task<string> CreateClaimableBalance()
    {
        var account = await _sorobanServer.GetAccount(SourceAccountId);
        var operation = new CreateClaimableBalanceOperation(new AssetTypeNative(), "100",
            new Claimant[]
            {
                new(SourceAccountId, new ClaimPredicateUnconditional())
            });
        var tx = new TransactionBuilder(account).AddOperation(operation).Build();
        tx.Sign(_sourceAccount);
        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
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

        var operation = new ChangeTrustOperation(assetA, assetB);

        var tx = new TransactionBuilder(account).AddOperation(operation).Build();
        tx.Sign(_sourceAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
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

        var trustOperation = new ChangeTrustOperation(asset);

        var tx = new TransactionBuilder(account)
            .AddOperation(trustOperation)
            .Build();
        tx.Sign(_targetAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
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

        var manageDataOperation = new ManageDataOperation("passkey", "it's a secret");

        var tx = new TransactionBuilder(account)
            .AddOperation(manageDataOperation)
            .Build();
        tx.Sign(_targetAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);

        var ledgerKeyData = new LedgerKey[]
        {
            new LedgerKeyData(TargetAccountId, "passkey")
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
        Assert.AreEqual(TargetAccountId, ledgerKey.Account.AccountId);
        Assert.IsNull(ledgerEntry.LedgerExtensionV1);
        Assert.IsNull(ledgerEntry.DataExtension);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeOffer()
    {
        var account = await _sorobanServer.GetAccount(SourceAccountId);

        const string price = "1.5";
        var manageSellOfferOperation = new ManageSellOfferOperation(new AssetTypeNative(), _asset, "1", "1.5", 0);

        var tx = new TransactionBuilder(account)
            .AddOperation(manageSellOfferOperation)
            .Build();
        tx.Sign(_sourceAccount);

        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);

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
        Assert.AreEqual(offerId, ledgerEntry.OfferId);
        Assert.AreEqual(SourceAccountId, ledgerEntry.SellerId.AccountId);
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

        var claimClaimableBalanceOperation = new ClaimClaimableBalanceOperation(claimableBalanceId);
        var account = await _sorobanServer.GetAccount(SourceAccountId);
        var tx = new TransactionBuilder(account).AddOperation(claimClaimableBalanceOperation).Build();
        tx.Sign(_sourceAccount);
        var txResponse = await _server.SubmitTransaction(tx);
        Assert.IsNotNull(txResponse);
        Assert.IsTrue(txResponse.IsSuccess);
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
        Assert.AreEqual("XXA", ((AssetTypeCreditAlphaNum4)parameters.AssetB).Code);
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

    [TestMethod]
    public async Task TestSimulateTransaction()
    {
        const string json =
            """
            {
                "jsonrpc": "2.0",
                "id": "7a469b9d6ed4444893491be530862ce3",
                "result": {
                    "transactionData": "AAAAAAAAAAIAAAAGAAAAAem354u9STQWq5b3Ed1j9tOemvL7xV0NPwhn4gXg0AP8AAAAFAAAAAEAAAAH8dTe2OoI0BnhlDbH0fWvXmvprkBvBAgKIcL9busuuMEAAAABAAAABgAAAAHpt+eLvUk0FquW9xHdY/bTnpry+8VdDT8IZ+IF4NAD/AAAABAAAAABAAAAAgAAAA8AAAAHQ291bnRlcgAAAAASAAAAAAAAAABYt8SiyPKXqo89JHEoH9/M7K/kjlZjMT7BjhKnPsqYoQAAAAEAHifGAAAFlAAAAIgAAAAAAAAAAg==",
                    "minResourceFee": "58181",
                    "events": [
                        "AAAAAQAAAAAAAAAAAAAAAgAAAAAAAAADAAAADwAAAAdmbl9jYWxsAAAAAA0AAAAg6bfni71JNBarlvcR3WP2056a8vvFXQ0/CGfiBeDQA/wAAAAPAAAACWluY3JlbWVudAAAAAAAABAAAAABAAAAAgAAABIAAAAAAAAAAFi3xKLI8peqjz0kcSgf38zsr+SOVmMxPsGOEqc+ypihAAAAAwAAAAo=",
                        "AAAAAQAAAAAAAAAB6bfni71JNBarlvcR3WP2056a8vvFXQ0/CGfiBeDQA/wAAAACAAAAAAAAAAIAAAAPAAAACWZuX3JldHVybgAAAAAAAA8AAAAJaW5jcmVtZW50AAAAAAAAAwAAABQ="
                    ],
                    "results": [
                        {
                            "auth": [
                                "AAAAAAAAAAAAAAAB6bfni71JNBarlvcR3WP2056a8vvFXQ0/CGfiBeDQA/wAAAAJaW5jcmVtZW50AAAAAAAAAgAAABIAAAAAAAAAAFi3xKLI8peqjz0kcSgf38zsr+SOVmMxPsGOEqc+ypihAAAAAwAAAAoAAAAA"
                            ],
                            "xdr": "AAAAAwAAABQ="
                        }
                    ],
                    "cost": { "cpuInsns": "1646885", "memBytes": "1296481" },
                    "stateChanges": [
                        {
                            "type": "created",
                            "key": "AAAAAAAAAABuaCbVXZ2DlXWarV6UxwbW3GNJgpn3ASChIFp5bxSIWg==",
                            "before": null,
                            "after": "AAAAZAAAAAAAAAAAbmgm1V2dg5V1mq1elMcG1txjSYKZ9wEgoSBaeW8UiFoAAAAAAAAAZAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="
                        }
                    ],
                    "latestLedger": "14245"
                }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        var sourceAccount = new Account(_sourceAccount, 1L);
        var paymentOperation = new PaymentOperation(_sourceAccount, new AssetTypeNative(), "10");
        var transaction = new TransactionBuilder(sourceAccount).AddOperation(paymentOperation).Build();
        var response = await sorobanServer.SimulateTransaction(transaction);

        Assert.IsNotNull(response);

        // SorobanTransactionData
        var sorobanData = response.SorobanTransactionData;

        Assert.IsNotNull(sorobanData);
        Assert.IsInstanceOfType(sorobanData.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(2L, sorobanData.ResourceFee);
        var sorobanResources = sorobanData.Resources;
        Assert.IsNotNull(sorobanResources);
        Assert.AreEqual(1976262U, sorobanResources.Instructions);
        Assert.AreEqual(1428U, sorobanResources.ReadBytes);
        Assert.AreEqual(136U, sorobanResources.WriteBytes);
        var footprint = sorobanResources.Footprint;
        Assert.IsNotNull(footprint);
        Assert.AreEqual(2, footprint.ReadOnly.Length);
        var readOnly0 = footprint.ReadOnly[0];
        var readOnly1 = footprint.ReadOnly[1];
        var readWrite = footprint.ReadWrite[0];
        if (readOnly0
            is LedgerKeyContractData
            {
                Contract: SCContractId contractId,
                Key: SCLedgerKeyContractInstance
            } contractData)
        {
            Assert.AreEqual("CDU3PZ4LXVETIFVLS33RDXLD63JZ5GXS7PCV2DJ7BBT6EBPA2AB7YR5H", contractId.InnerValue);
            Assert.AreEqual(
                ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT,
                contractData.Durability.InnerValue);
        }
        else
        {
            Assert.Fail();
        }

        if (readOnly1 is LedgerKeyContractCode contractCode)
            Assert.AreEqual("8dTe2OoI0BnhlDbH0fWvXmvprkBvBAgKIcL9busuuME=", Convert.ToBase64String(contractCode.Hash));
        else
            Assert.Fail();

        if (readWrite is LedgerKeyContractData { Contract: SCContractId contractId1, Key: SCVec vec } contractData1)
        {
            Assert.AreEqual("CDU3PZ4LXVETIFVLS33RDXLD63JZ5GXS7PCV2DJ7BBT6EBPA2AB7YR5H", contractId1.InnerValue);
            Assert.AreEqual(2, vec.InnerValue.Length);
            Assert.IsTrue(vec.InnerValue[0] is SCSymbol { InnerValue: "Counter" });
            Assert.IsTrue(vec.InnerValue[1] is SCAccountId
            {
                InnerValue: "GBMLPRFCZDZJPKUPHUSHCKA737GOZL7ERZLGGMJ6YGHBFJZ6ZKMKCZTM"
            });
            Assert.AreEqual(
                ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT,
                contractData1.Durability.InnerValue);
        }

        Assert.AreEqual(1, footprint.ReadWrite.Length);
        Assert.AreEqual(58181U, response.MinResourceFee);
        var events = response.Events;
        Assert.IsNotNull(events);
        Assert.AreEqual(2, events.Length);
        Assert.AreEqual(
            "AAAAAQAAAAAAAAAAAAAAAgAAAAAAAAADAAAADwAAAAdmbl9jYWxsAAAAAA0AAAAg6bfni71JNBarlvcR3WP2056a8vvFXQ0/CGfiBeDQA/wAAAAPAAAACWluY3JlbWVudAAAAAAAABAAAAABAAAAAgAAABIAAAAAAAAAAFi3xKLI8peqjz0kcSgf38zsr+SOVmMxPsGOEqc+ypihAAAAAwAAAAo=",
            events[0]);
        Assert.AreEqual(
            "AAAAAQAAAAAAAAAB6bfni71JNBarlvcR3WP2056a8vvFXQ0/CGfiBeDQA/wAAAACAAAAAAAAAAIAAAAPAAAACWZuX3JldHVybgAAAAAAAA8AAAAJaW5jcmVtZW50AAAAAAAAAwAAABQ=",
            events[1]);
        Assert.AreEqual(14245L, response.LatestLedger);

        var results = response.Results;
        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Length);
        Assert.AreEqual("AAAAAwAAABQ=", results[0].Xdr);
        var auths = results[0].Auth;
        Assert.IsNotNull(auths);
        Assert.AreEqual(1, auths.Length);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAB6bfni71JNBarlvcR3WP2056a8vvFXQ0/CGfiBeDQA/wAAAAJaW5jcmVtZW50AAAAAAAAAgAAABIAAAAAAAAAAFi3xKLI8peqjz0kcSgf38zsr+SOVmMxPsGOEqc+ypihAAAAAwAAAAoAAAAA",
            auths[0]);

        // LedgerEntryChanges
        Assert.IsNotNull(response.StateChanges);
        Assert.AreEqual(1, response.StateChanges.Length);
        var stateChanges = response.StateChanges[0];
        Assert.IsNotNull(stateChanges);
        Assert.AreEqual("created", stateChanges.Type);
        Assert.AreEqual("AAAAAAAAAABuaCbVXZ2DlXWarV6UxwbW3GNJgpn3ASChIFp5bxSIWg==", stateChanges.Key);
        Assert.IsNull(stateChanges.Before);
        Assert.AreEqual(
            "AAAAZAAAAAAAAAAAbmgm1V2dg5V1mq1elMcG1txjSYKZ9wEgoSBaeW8UiFoAAAAAAAAAZAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
            stateChanges.After);
    }
    // TODO TestGetLedgerEntriesOfTypeTTL()
    // TODO TestGetLedgerEntriesOfTypeConfigSetting()
}