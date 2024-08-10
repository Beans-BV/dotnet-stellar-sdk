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
    private const string HelloContractWasmHash = "c1a650506f7c20c8f4d16aae73f894f302cd011d7ef33adef572f20b34f7653e";
    private const string HelloContractId = "CDMYJX6ZLUM3IE6GI4SCMND5FU23ULOIO3EWOIOKXWXMA6HGPEJ53RQZ";
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
        await ExtendFootprintTtl(HelloContractWasmHash, 1000);
    }

    [TestMethod]
    public async Task TestRestoreFootprint()
    {
        await RestoreFootprint(HelloContractWasmHash);
    }

    [TestMethod]
    public async Task TestCreateContract()
    {
        await CreateContract(HelloContractWasmHash);
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
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "6b82a39c-ad6b-4699-9d26-f41432cbb9e1",
              "result": {
                "events": [
                  {
                    "type": "contract",
                    "ledger": 912707,
                    "ledgerClosedAt": "2024-08-06T10:09:22Z",
                    "contractId": "CASCLAHV7E7H3BOGQIW5HIC3H6WVDOTOQRTRMXYSTKJHXOORP3DNATY2",
                    "id": "0003920046715838464-0000000001",
                    "pagingToken": "0003920046715838464-0000000001",
                    "topic": [
                      "AAAADwAAAAhTVFJfQ0VSVA\u003d\u003d",
                      "AAAADwAAAAhzdHJfY2VydA\u003d\u003d"
                    ],
                    "value": "AAAAEQAAAAEAAAADAAAADwAAAARoYXNoAAAADgAAAEAzYjdkODUxYjVjNmMyMTNmNzUyYzdmNzJhNDA0Yjg5NGFiZGU2NDY2NDhjMzU0MmQ3MDRlMjI4OTMwNmU1MTFhAAAADwAAAAZwYXJlbnQAAAAAAA4AAAAAAAAADwAAAAd2ZXJzaW9uAAAAAAMAAAAA",
                    "inSuccessfulContractCall": true,
                    "txHash": "9f6cf2cf2d1dd41af039325503ba98daf3cfa10d0079cd2c50a28355fb1b4af2"
                  },
                  {
                    "type": "contract",
                    "ledger": 912723,
                    "ledgerClosedAt": "2024-08-06T10:10:47Z",
                    "contractId": "CDTJALOV4KLSPEMNFHKYSG4WOTN7FCN4A2JOKRPVCQYEHLUEH2YUJF5R",
                    "id": "0003920115435319296-0000000001",
                    "pagingToken": "0003920115435319296-0000000001",
                    "topic": [
                      "AAAADwAAAARtaW50",
                      "AAAAEgAAAAAAAAAAdWnjmUMJ9zn2dIq1d6OhQ7XzqNT2ppF+9OgDmID0yhQ\u003d",
                      "AAAAEgAAAAAAAAAAoM2uDbnOXB4fd/4Y3ZRlbiis4zPp1sdQNyQHWQuvzq4\u003d"
                    ],
                    "value": "AAAACgAAAAAAAAAAAAAJGE5yoAA\u003d",
                    "inSuccessfulContractCall": true,
                    "txHash": "318915004f904a36fddcefa8d4935ab21db7848a5a5e528815672968125a79a8"
                  },
                  {
                    "type": "contract",
                    "ledger": 912725,
                    "ledgerClosedAt": "2024-08-06T10:10:58Z",
                    "contractId": "CDYTK2FLRHT3KJ6RVAAABI4A7Y2XERCWN3FT5II7FHONDKPQVEAZ33YI",
                    "id": "0003920124025257984-0000000001",
                    "pagingToken": "0003920124025257984-0000000001",
                    "topic": [
                      "AAAADwAAAARtaW50",
                      "AAAAEgAAAAAAAAAAdWnjmUMJ9zn2dIq1d6OhQ7XzqNT2ppF+9OgDmID0yhQ\u003d",
                      "AAAAEgAAAAAAAAAAdT55ljEK4qTy1Y7Fw9KtdAAZIBkrd2p7IX0ZByhSO2k\u003d"
                    ],
                    "value": "AAAACgAAAAAAAAAAAAAJGE5yoAA\u003d",
                    "inSuccessfulContractCall": true,
                    "txHash": "944d99fd572b8541f5c0ce95881b844d79144fe9a5cef2aea1cf94fb7c91f1f7"
                  },
                  {
                    "type": "contract",
                    "ledger": 913037,
                    "ledgerClosedAt": "2024-08-06T10:38:23Z",
                    "contractId": "CDV6IIE2DFFGB3GKAG7YYSKBO4PFDAZ76GRHXKSOBBIG64NNHMMRCRXH",
                    "id": "0003921464055050240-0000000001",
                    "pagingToken": "0003921464055050240-0000000001",
                    "topic": [
                      "AAAADwAAAAdhcHByb3ZlAA\u003d\u003d",
                      "AAAAEgAAAAAAAAAAWoM+w+i/0MLTSydKU896zcL4/EhYLVMHlSxIzY+ucJs\u003d",
                      "AAAAEgAAAAGPj5dVA0lfIe7VhCeJwcQ68vhwUcgQjj7XNcpMoUn7Hw\u003d\u003d"
                    ],
                    "value": "AAAAEAAAAAEAAAACAAAACgAAAAAAAAAAAAQdH3d3yKkAAAADABD7yA\u003d\u003d",
                    "inSuccessfulContractCall": true,
                    "txHash": "64bd9d003dbc4c9f766206dee34d57285322eeee6c5acb6d2a31d2668d88c2fd"
                  }
                ],
                "latestLedger": 913609
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);

        var eventsResponse = await sorobanServer.GetEvents(new GetEventsRequest());

        Assert.IsNotNull(eventsResponse);
        Assert.IsNotNull(eventsResponse.Events);
        Assert.AreEqual(913609L, eventsResponse.LatestLedger);
        Assert.AreEqual(4, eventsResponse.Events.Length);
        var event1 = eventsResponse.Events[0];

        Assert.IsNotNull(event1);
        Assert.AreEqual("contract", event1.Type);
        Assert.AreEqual(912707, event1.Ledger);
        Assert.AreEqual("2024-08-06T10:09:22Z", event1.LedgerClosedAt);
        Assert.AreEqual("CASCLAHV7E7H3BOGQIW5HIC3H6WVDOTOQRTRMXYSTKJHXOORP3DNATY2", event1.ContractId);
        Assert.AreEqual("0003920046715838464-0000000001", event1.Id);
        Assert.AreEqual("0003920046715838464-0000000001", event1.PagingToken);
        Assert.AreEqual(2, event1.Topics.Length);
        Assert.AreEqual("AAAADwAAAAhTVFJfQ0VSVA==", event1.Topics[0]);
        Assert.AreEqual("AAAADwAAAAhzdHJfY2VydA==", event1.Topics[1]);
        Assert.AreEqual(
            "AAAAEQAAAAEAAAADAAAADwAAAARoYXNoAAAADgAAAEAzYjdkODUxYjVjNmMyMTNmNzUyYzdmNzJhNDA0Yjg5NGFiZGU2NDY2NDhjMzU0MmQ3MDRlMjI4OTMwNmU1MTFhAAAADwAAAAZwYXJlbnQAAAAAAA4AAAAAAAAADwAAAAd2ZXJzaW9uAAAAAAMAAAAA",
            event1.Value);
        Assert.IsTrue(event1.InSuccessfulContractCall);
        Assert.AreEqual("9f6cf2cf2d1dd41af039325503ba98daf3cfa10d0079cd2c50a28355fb1b4af2", event1.TransactionHash);

        var event2 = eventsResponse.Events[1];
        Assert.IsNotNull(event2);
        Assert.AreEqual("contract", event2.Type);
        Assert.AreEqual(912723, event2.Ledger);
        Assert.AreEqual("2024-08-06T10:10:47Z", event2.LedgerClosedAt);
        Assert.AreEqual("CDTJALOV4KLSPEMNFHKYSG4WOTN7FCN4A2JOKRPVCQYEHLUEH2YUJF5R", event2.ContractId);
        Assert.AreEqual("0003920115435319296-0000000001", event2.Id);
        Assert.AreEqual("0003920115435319296-0000000001", event2.PagingToken);
        Assert.AreEqual(3, event2.Topics.Length);
        Assert.AreEqual("AAAADwAAAARtaW50", event2.Topics[0]);
        Assert.AreEqual("AAAAEgAAAAAAAAAAdWnjmUMJ9zn2dIq1d6OhQ7XzqNT2ppF+9OgDmID0yhQ=", event2.Topics[1]);
        Assert.AreEqual("AAAAEgAAAAAAAAAAoM2uDbnOXB4fd/4Y3ZRlbiis4zPp1sdQNyQHWQuvzq4=", event2.Topics[2]);
        Assert.AreEqual("AAAACgAAAAAAAAAAAAAJGE5yoAA=", event2.Value);
        Assert.IsTrue(event2.InSuccessfulContractCall);
        Assert.AreEqual("318915004f904a36fddcefa8d4935ab21db7848a5a5e528815672968125a79a8", event2.TransactionHash);

        var event3 = eventsResponse.Events[2];
        Assert.IsNotNull(event3);
        Assert.AreEqual("contract", event3.Type);
        Assert.AreEqual(912725, event3.Ledger);
        Assert.AreEqual("2024-08-06T10:10:58Z", event3.LedgerClosedAt);
        Assert.AreEqual("CDYTK2FLRHT3KJ6RVAAABI4A7Y2XERCWN3FT5II7FHONDKPQVEAZ33YI", event3.ContractId);
        Assert.AreEqual("0003920124025257984-0000000001", event3.Id);
        Assert.AreEqual("0003920124025257984-0000000001", event3.PagingToken);
        Assert.AreEqual(3, event3.Topics.Length);
        Assert.AreEqual("AAAADwAAAARtaW50", event3.Topics[0]);
        Assert.AreEqual("AAAAEgAAAAAAAAAAdWnjmUMJ9zn2dIq1d6OhQ7XzqNT2ppF+9OgDmID0yhQ=", event3.Topics[1]);
        Assert.AreEqual("AAAAEgAAAAAAAAAAdT55ljEK4qTy1Y7Fw9KtdAAZIBkrd2p7IX0ZByhSO2k=", event3.Topics[2]);
        Assert.AreEqual("AAAACgAAAAAAAAAAAAAJGE5yoAA=", event3.Value);
        Assert.IsTrue(event3.InSuccessfulContractCall);
        Assert.AreEqual("944d99fd572b8541f5c0ce95881b844d79144fe9a5cef2aea1cf94fb7c91f1f7", event3.TransactionHash);

        var event4 = eventsResponse.Events[3];
        Assert.IsNotNull(event4);
        Assert.AreEqual("contract", event4.Type);
        Assert.AreEqual(913037, event4.Ledger);
        Assert.AreEqual("2024-08-06T10:38:23Z", event4.LedgerClosedAt);
        Assert.AreEqual("CDV6IIE2DFFGB3GKAG7YYSKBO4PFDAZ76GRHXKSOBBIG64NNHMMRCRXH", event4.ContractId);
        Assert.AreEqual("0003921464055050240-0000000001", event4.Id);
        Assert.AreEqual("0003921464055050240-0000000001", event4.PagingToken);
        Assert.AreEqual(3, event4.Topics.Length);
        Assert.AreEqual("AAAADwAAAAdhcHByb3ZlAA==", event4.Topics[0]);
        Assert.AreEqual("AAAAEgAAAAAAAAAAWoM+w+i/0MLTSydKU896zcL4/EhYLVMHlSxIzY+ucJs=", event4.Topics[1]);
        Assert.AreEqual("AAAAEgAAAAGPj5dVA0lfIe7VhCeJwcQ68vhwUcgQjj7XNcpMoUn7Hw==", event4.Topics[2]);
        Assert.AreEqual("AAAAEAAAAAEAAAACAAAACgAAAAAAAAAAAAQdH3d3yKkAAAADABD7yA==", event4.Value);
        Assert.IsTrue(event4.InSuccessfulContractCall);
        Assert.AreEqual("64bd9d003dbc4c9f766206dee34d57285322eeee6c5acb6d2a31d2668d88c2fd", event4.TransactionHash);
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
                ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT)),
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
        Assert.AreEqual(HelloContractWasmHash.ToLower(), ledgerExecutable.WasmHash.ToLower());
    }

    private async Task GetEvents(long? ledger, string contractId)
    {
        var eventFilter = new GetEventsRequest.EventFilter
        {
            Type = "diagnostic",
            ContractIds = [contractId],
            Topics = [["*", new SCSymbol("hello").ToXdrBase64()]],
        };
        var getEventsRequest = new GetEventsRequest
        {
            StartLedger = ledger,
            Filters = [eventFilter],
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
            new LedgerKeyContractCode(HelloContractWasmHash),
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
        Assert.AreEqual(HelloContractWasmHash.ToLower(), Convert.ToHexString(ledgerEntry.Hash).ToLower());
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
            new LedgerKeyAccount(accountId2),
        };

        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "918681d5-6e9b-44a0-912a-db599b5fb27f",
              "result": {
                "entries": [
                  {
                    "key": "AAAAAAAAAADBPp7TMinJylnn+6dQXJACNc15LF+aJ2Py1BaR4P10JA==",
                    "xdr": "AAAAAAAAAADBPp7TMinJylnn+6dQXJACNc15LF+aJ2Py1BaR4P10JAAAABazqg5CAAADrQAAAAoAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAABZnkAAAAAZnAVKA==",
                    "lastModifiedLedgerSeq": 91769
                  }
                ],
                "latestLedger": 910556
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        var response = await sorobanServer.GetLedgerEntries(ledgerKeyAccounts);

        Assert.IsNotNull(response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntryA = response.LedgerEntries[0] as LedgerEntryAccount;
        var ledgerKeyA = response.LedgerKeys[0] as LedgerKeyAccount;
        Assert.IsNotNull(ledgerEntryA);
        Assert.IsNotNull(ledgerKeyA);

        Assert.AreEqual("GDAT5HWTGIU4TSSZ4752OUC4SABDLTLZFRPZUJ3D6LKBNEPA7V2CIG54", ledgerEntryA.Account.AccountId);
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
        var accountExtensionV1 = ledgerEntryA.AccountExtensionV1;
        Assert.IsNotNull(accountExtensionV1);
        Assert.AreEqual(0L, accountExtensionV1.Liabilities.Buying);
        Assert.AreEqual(0L, accountExtensionV1.Liabilities.Selling);
        var accountExtensionV2 = accountExtensionV1.ExtensionV2;
        Assert.IsNotNull(accountExtensionV2);
        Assert.AreEqual(0U, accountExtensionV2.NumberSponsored);
        Assert.AreEqual(0U, accountExtensionV2.NumberSponsoring);
        Assert.AreEqual(0, accountExtensionV2.SignerSponsoringIDs.Length);
        var accountExtensionV3 = accountExtensionV2.ExtensionV3;
        Assert.IsNotNull(accountExtensionV3);
        Assert.IsInstanceOfType(accountExtensionV3.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(91769U, accountExtensionV3.SequenceLedger);
        Assert.AreEqual(1718621480UL, accountExtensionV3.SequenceTime);
        Assert.AreEqual("GDAT5HWTGIU4TSSZ4752OUC4SABDLTLZFRPZUJ3D6LKBNEPA7V2CIG54", ledgerKeyA.Account.AccountId);
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
        Assert.AreEqual(HelloContractWasmHash.ToLower(), Convert.ToHexString(result.InnerValue).ToLower());

        var transactionEnvelopeXdrBase64 = tx.ToEnvelopeXdrBase64();

        var sendResponse = await SendTransaction(tx);
        var txHash = sendResponse.Hash;
        Assert.IsNotNull(txHash);
        var getTransactionResponse = await PollTransaction(txHash);

        var wasmHash = getTransactionResponse.WasmHash;

        Assert.IsNotNull(wasmHash);

        await Task.Delay(2000);

        var operationResponse = await GetHorizonOperation(txHash, transactionEnvelopeXdrBase64);

        Assert.IsInstanceOfType(operationResponse, typeof(InvokeHostFunctionOperationResponse));

        return wasmHash;
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

        tx.SetSorobanTransactionData(simulateResponse.SorobanTransactionData);
        if (simulateResponse.SorobanAuthorization != null)
        {
            tx.SetSorobanAuthorization(simulateResponse.SorobanAuthorization);
        }
        Assert.IsNotNull(simulateResponse.MinResourceFee);
        tx.AddResourceFee(simulateResponse.MinResourceFee.Value);
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

    private async Task<Tuple<long?, string>> CreateContract(string wasmHash)
    {
        await Task.Delay(2000);
        var account = await _server.Accounts.Account(SourceAccountId);
        var createContractOperation = CreateContractOperation.FromAddress(wasmHash, account.AccountId);
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

        return new Tuple<long?, string>(ledger, contractId);
    }

    [TestMethod]
    public async Task TestASeriesOfHostFunctionInvocation()
    {
        var wasmHash = await UploadContract(_helloWasmPath);
        await ExtendFootprintTtl(wasmHash, 10000);
        await RestoreFootprint(wasmHash);
        var (ledger, createdContractId) = await CreateContract(wasmHash);
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
    ///     Restores the contract entry, if a Wasm hash is specified, the footprint should be a LedgerKeyContractCode,
    ///     otherwise
    ///     it should be a LedgerKeyContractData
    /// </summary>
    /// <param name="id">could either be a Wasm hash or a contractId.</param>
    private async Task RestoreFootprint(string id)
    {
        var account = await _server.Accounts.Account(SourceAccountId);
        var restoreOperation = new RestoreFootprintOperation();
        var tx = new TransactionBuilder(account).AddOperation(restoreOperation).Build();
        LedgerKey key;
        if (StrKey.IsValidContractId(id))
        {
            key = new LedgerKeyContractData(new SCContractId(id), new SCLedgerKeyContractInstance(),
                ContractDataDurability.Create(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT));
        }
        else
        {
            key = new LedgerKeyContractCode(id);
        }

        tx.SetSorobanTransactionData(new SorobanTransactionData(key, false));

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

    private async Task ExtendFootprintTtl(string wasmHash, uint extentTo)
    {
        var account = await _server.Accounts.Account(SourceAccountId);

        var extendOperation = new ExtendFootprintOperation(extentTo);
        var tx = new TransactionBuilder(account).AddOperation(extendOperation).Build();
        var ledgerFootprint = new LedgerFootprint
        {
            ReadOnly = new LedgerKey[] { new LedgerKeyContractCode(wasmHash) },
        };

        var resources = new SorobanResources(ledgerFootprint, 0, 0, 0);
        var transactionData = new SorobanTransactionData(resources, 0);

        tx.SetSorobanTransactionData(transactionData);

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
        var status = TransactionInfo.TransactionStatus.NOT_FOUND;
        GetTransactionResponse? transactionResponse = null;
        while (status == TransactionInfo.TransactionStatus.NOT_FOUND)
        {
            transactionResponse = await _sorobanServer.GetTransaction(transactionHash);

            status = transactionResponse.Status;
            if (status == TransactionInfo.TransactionStatus.FAILED)
            {
                Assert.IsNotNull(transactionResponse.ResultMetaXdr);
                Assert.IsNotNull(transactionResponse.TransactionMeta);
                Assert.Fail();
            }
            else if (status == TransactionInfo.TransactionStatus.SUCCESS)
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
                new(SourceAccountId, new ClaimPredicateUnconditional()),
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
            new LedgerKeyData(TargetAccountId, "passkey"),
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
            new LedgerKeyOffer(SourceAccountId, offerId),
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
            new LedgerKeyClaimableBalance(claimableBalanceId),
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
            new LedgerKeyLiquidityPool(nativeAsset, _asset, 30),
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
            new LedgerKeyTrustline(TargetAccountId, _asset),
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
        Assert.AreEqual(TransactionInfo.TransactionStatus.NOT_FOUND, getTransactionResponse.Status);
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
                Key: SCLedgerKeyContractInstance,
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
        {
            Assert.AreEqual("8dTe2OoI0BnhlDbH0fWvXmvprkBvBAgKIcL9busuuME=", Convert.ToBase64String(contractCode.Hash));
        }
        else
        {
            Assert.Fail();
        }

        if (readWrite is LedgerKeyContractData { Contract: SCContractId contractId1, Key: SCVec vec } contractData1)
        {
            Assert.AreEqual("CDU3PZ4LXVETIFVLS33RDXLD63JZ5GXS7PCV2DJ7BBT6EBPA2AB7YR5H", contractId1.InnerValue);
            Assert.AreEqual(2, vec.InnerValue.Length);
            Assert.IsTrue(vec.InnerValue[0] is SCSymbol { InnerValue: "Counter" });
            Assert.IsTrue(vec.InnerValue[1] is SCAccountId
            {
                InnerValue: "GBMLPRFCZDZJPKUPHUSHCKA737GOZL7ERZLGGMJ6YGHBFJZ6ZKMKCZTM",
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

    [TestMethod]
    public async Task TestGetFeeStats()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": 8675309,
              "result": {
                "inclusionFee": {
                  "max": "210",
                  "min": "10",
                  "mode": "100",
                  "p10": "110",
                  "p20": "120",
                  "p30": "130",
                  "p40": "140",
                  "p50": "150",
                  "p60": "160",
                  "p70": "170",
                  "p80": "180",
                  "p90": "190",
                  "p95": "195",
                  "p99": "199",
                  "transactionCount": "10",
                  "ledgerCount": 50
                },
                "sorobanInclusionFee": {
                  "max": "300",
                  "min": "200",
                  "mode": "100",
                  "p10": "200",
                  "p20": "210",
                  "p30": "220",
                  "p40": "230",
                  "p50": "240",
                  "p60": "250",
                  "p70": "260",
                  "p80": "270",
                  "p90": "280",
                  "p95": "290",
                  "p99": "299",
                  "transactionCount": "7",
                  "ledgerCount": 10
                },
                "latestLedger": 4519945
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        var response = await sorobanServer.GetFeeStats();

        Assert.IsNotNull(response);

        var inclusionFee = response.InclusionFee;
        Assert.IsNotNull(inclusionFee);
        Assert.AreEqual("210", inclusionFee.Max);
        Assert.AreEqual("10", inclusionFee.Min);
        Assert.AreEqual("100", inclusionFee.Mode);
        Assert.AreEqual("110", inclusionFee.P10);
        Assert.AreEqual("120", inclusionFee.P20);
        Assert.AreEqual("130", inclusionFee.P30);
        Assert.AreEqual("140", inclusionFee.P40);
        Assert.AreEqual("150", inclusionFee.P50);
        Assert.AreEqual("160", inclusionFee.P60);
        Assert.AreEqual("170", inclusionFee.P70);
        Assert.AreEqual("180", inclusionFee.P80);
        Assert.AreEqual("190", inclusionFee.P90);
        Assert.AreEqual("199", inclusionFee.P99);
        Assert.AreEqual("10", inclusionFee.TransactionCount);
        Assert.AreEqual(50, inclusionFee.LedgerCount);

        var sorobanInclusionFee = response.SorobanInclusionFee;
        Assert.IsNotNull(sorobanInclusionFee);
        Assert.AreEqual("300", sorobanInclusionFee.Max);
        Assert.AreEqual("200", sorobanInclusionFee.Min);
        Assert.AreEqual("100", sorobanInclusionFee.Mode);
        Assert.AreEqual("200", sorobanInclusionFee.P10);
        Assert.AreEqual("210", sorobanInclusionFee.P20);
        Assert.AreEqual("220", sorobanInclusionFee.P30);
        Assert.AreEqual("230", sorobanInclusionFee.P40);
        Assert.AreEqual("240", sorobanInclusionFee.P50);
        Assert.AreEqual("250", sorobanInclusionFee.P60);
        Assert.AreEqual("260", sorobanInclusionFee.P70);
        Assert.AreEqual("270", sorobanInclusionFee.P80);
        Assert.AreEqual("280", sorobanInclusionFee.P90);
        Assert.AreEqual("299", sorobanInclusionFee.P99);
        Assert.AreEqual("7", sorobanInclusionFee.TransactionCount);
        Assert.AreEqual(10, sorobanInclusionFee.LedgerCount);
        Assert.IsNotNull(response.SorobanInclusionFee);
        Assert.AreEqual(4519945L, response.LatestLedger);
    }

    [TestMethod]
    public async Task TestGetVersionInfo()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": 8675309,
              "result": {
                "version": "21.1.0",
                "commit_hash": "fcd2f0523f04279bae4502f3e3fa00ca627e6f6a",
                "build_time_stamp": "2024-05-10T11:18:38",
                "captive_core_version": "stellar-core 21.0.0.rc2 (c6f474133738ae5f6d11b07963ca841909210273)",
                "protocol_version": 21
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        var response = await sorobanServer.GetVersionInfo();

        Assert.IsNotNull(response);
        Assert.AreEqual("21.1.0", response.Version);
        Assert.AreEqual("fcd2f0523f04279bae4502f3e3fa00ca627e6f6a", response.CommitHash);
        Assert.AreEqual("2024-05-10T11:18:38", response.BuildTimeStamp);
        Assert.AreEqual("stellar-core 21.0.0.rc2 (c6f474133738ae5f6d11b07963ca841909210273)",
            response.CaptiveCoreVersion);
        Assert.AreEqual(21, response.ProtocolVersion);
    }

    [TestMethod]
    public async Task TestGetTransaction()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": 8675309,
              "result": {
                "status": "SUCCESS",
                "latestLedger": 2540076,
                "latestLedgerCloseTime": "1700086333",
                "oldestLedger": 2538637,
                "oldestLedgerCloseTime": "1700078796",
                "applicationOrder": 1,
                "envelopeXdr": "AAAAAgAAAADGFY14/R1KD0VGtTbi5Yp4d7LuMW0iQbLM/AUiGKj5owCpsoQAJY3OAAAjqgAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAGAAAAAAAAAABhhOwI+RL18Zpk7cqI5pRRf0L96jE8i+0x3ekhuBh2cUAAAARc2V0X2N1cnJlbmN5X3JhdGUAAAAAAAACAAAADwAAAANldXIAAAAACQAAAAAAAAAAAAAAAAARCz4AAAABAAAAAAAAAAAAAAABhhOwI+RL18Zpk7cqI5pRRf0L96jE8i+0x3ekhuBh2cUAAAARc2V0X2N1cnJlbmN5X3JhdGUAAAAAAAACAAAADwAAAANldXIAAAAACQAAAAAAAAAAAAAAAAARCz4AAAAAAAAAAQAAAAAAAAABAAAAB4408vVXuLU3mry897TfPpYjjsSN7n42REos241RddYdAAAAAQAAAAYAAAABhhOwI+RL18Zpk7cqI5pRRf0L96jE8i+0x3ekhuBh2cUAAAAUAAAAAQFvcYAAAImAAAAHxAAAAAAAAAACAAAAARio+aMAAABATbFMyom/TUz87wHex0LoYZA8jbNJkXbaDSgmOdk+wSBFJuMuta+/vSlro0e0vK2+1FqD/zWHZeYig4pKmM3rDA==",
                "resultXdr": "AAAAAAARFy8AAAAAAAAAAQAAAAAAAAAYAAAAAMu8SHUN67hTUJOz3q+IrH9M/4dCVXaljeK6x1Ss20YWAAAAAA==",
                "resultMetaXdr": "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",
                "ledger": 2540064,
                "createdAt": "1700086268"
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        const string txHash = "6bc97bddc21811c626839baf4ab574f4f9f7ddbebb44d286ae504396d4e752da";
        var response = await sorobanServer.GetTransaction(txHash);

        Assert.IsNotNull(response);
        Assert.AreEqual(TransactionInfo.TransactionStatus.SUCCESS, response.Status);
        Assert.AreEqual(2540076L, response.LatestLedger);
        Assert.AreEqual(1700086333L, response.LatestLedgerCloseTime);
        Assert.AreEqual(2538637L, response.OldestLedger);
        Assert.AreEqual(1700078796L, response.OldestLedgerCloseTime);
        Assert.AreEqual(1, response.ApplicationOrder);
        Assert.AreEqual(
            "AAAAAgAAAADGFY14/R1KD0VGtTbi5Yp4d7LuMW0iQbLM/AUiGKj5owCpsoQAJY3OAAAjqgAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAGAAAAAAAAAABhhOwI+RL18Zpk7cqI5pRRf0L96jE8i+0x3ekhuBh2cUAAAARc2V0X2N1cnJlbmN5X3JhdGUAAAAAAAACAAAADwAAAANldXIAAAAACQAAAAAAAAAAAAAAAAARCz4AAAABAAAAAAAAAAAAAAABhhOwI+RL18Zpk7cqI5pRRf0L96jE8i+0x3ekhuBh2cUAAAARc2V0X2N1cnJlbmN5X3JhdGUAAAAAAAACAAAADwAAAANldXIAAAAACQAAAAAAAAAAAAAAAAARCz4AAAAAAAAAAQAAAAAAAAABAAAAB4408vVXuLU3mry897TfPpYjjsSN7n42REos241RddYdAAAAAQAAAAYAAAABhhOwI+RL18Zpk7cqI5pRRf0L96jE8i+0x3ekhuBh2cUAAAAUAAAAAQFvcYAAAImAAAAHxAAAAAAAAAACAAAAARio+aMAAABATbFMyom/TUz87wHex0LoYZA8jbNJkXbaDSgmOdk+wSBFJuMuta+/vSlro0e0vK2+1FqD/zWHZeYig4pKmM3rDA==",
            response.EnvelopeXdr);
        Assert.AreEqual("AAAAAAARFy8AAAAAAAAAAQAAAAAAAAAYAAAAAMu8SHUN67hTUJOz3q+IrH9M/4dCVXaljeK6x1Ss20YWAAAAAA==",
            response.ResultXdr);
        Assert.AreEqual(
            "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",
            response.ResultMetaXdr);
        Assert.AreEqual(2540064L, response.Ledger);
        Assert.AreEqual(1700086268L, response.CreatedAt);
    }

    [TestMethod]
    public async Task TestGetTransactions()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": 8675309,
              "result": {
                "transactions": [
                  {
                    "status": "FAILED",
                    "applicationOrder": 1,
                    "feeBump": false,
                    "envelopeXdr": "AAAAAgAAAACDz21Q3CTITlGqRus3/96/05EDivbtfJncNQKt64BTbAAAASwAAKkyAAXlMwAAAAEAAAAAAAAAAAAAAABmWeASAAAAAQAAABR3YWxsZXQ6MTcxMjkwNjMzNjUxMAAAAAEAAAABAAAAAIPPbVDcJMhOUapG6zf/3r/TkQOK9u18mdw1Aq3rgFNsAAAAAQAAAABwOSvou8mtwTtCkysVioO35TSgyRir2+WGqO8FShG/GAAAAAFVQUgAAAAAAO371tlrHUfK+AvmQvHje1jSUrvJb3y3wrJ7EplQeqTkAAAAAAX14QAAAAAAAAAAAeuAU2wAAABAn+6A+xXvMasptAm9BEJwf5Y9CLLQtV44TsNqS8ocPmn4n8Rtyb09SBiFoMv8isYgeQU5nAHsIwBNbEKCerusAQ==",
                    "resultXdr": "AAAAAAAAAGT/////AAAAAQAAAAAAAAAB////+gAAAAA=",
                    "resultMetaXdr": "AAAAAwAAAAAAAAACAAAAAwAc0RsAAAAAAAAAAIPPbVDcJMhOUapG6zf/3r/TkQOK9u18mdw1Aq3rgFNsAAAAF0YpYBQAAKkyAAXlMgAAAAsAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAMAAAAAABzRGgAAAABmWd/VAAAAAAAAAAEAHNEbAAAAAAAAAACDz21Q3CTITlGqRus3/96/05EDivbtfJncNQKt64BTbAAAABdGKWAUAACpMgAF5TMAAAALAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAc0RsAAAAAZlnf2gAAAAAAAAAAAAAAAAAAAAA=",
                    "ledger": 1888539,
                    "createdAt": 1717166042
                  },
                  {
                    "status": "SUCCESS",
                    "applicationOrder": 2,
                    "feeBump": false,
                    "envelopeXdr": "AAAAAgAAAAC4EZup+ewCs/doS3hKbeAa4EviBHqAFYM09oHuLtqrGAAPQkAAGgQZAAAANgAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAABAAAAABB90WssODNIgi6BHveqzxTRmIpvAFRyVNM+Hm2GVuCcAAAAAAAAAAAq6aHAHZ2sd9aPbRsskrlXMLWIwqs4Sv2Bk+VwuIR+9wAAABdIdugAAAAAAAAAAAIu2qsYAAAAQERzKOqYYiPXNwsiL8ADAG/f45RBssmf3umGzw4qKkLGlObuPdX0buWmTGrhI13SG38F2V8Mp9DI+eDkcCjMSAOGVuCcAAAAQHnm0o/r+Gsl+6oqBgSbqoSY37gflvQB3zZRghuir0N75UVerd0Q50yG5Zfu08i2crhx6uk+5HYTl8/Sa7uZ+Qc=",
                    "resultXdr": "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAA=",
                    "resultMetaXdr": "AAAAAwAAAAAAAAACAAAAAwAc0RsAAAAAAAAAALgRm6n57AKz92hLeEpt4BrgS+IEeoAVgzT2ge4u2qsYAAAAADwzS2gAGgQZAAAANQAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAMAAAAAABzPVAAAAABmWdZ2AAAAAAAAAAEAHNEbAAAAAAAAAAC4EZup+ewCs/doS3hKbeAa4EviBHqAFYM09oHuLtqrGAAAAAA8M0toABoEGQAAADYAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAc0RsAAAAAZlnf2gAAAAAAAAABAAAAAwAAAAMAHNEaAAAAAAAAAAAQfdFrLDgzSIIugR73qs8U0ZiKbwBUclTTPh5thlbgnABZJUSd0V2hAAAAawAAAlEAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAaBGEAAAAAZkspCwAAAAAAAAABABzRGwAAAAAAAAAAEH3Rayw4M0iCLoEe96rPFNGYim8AVHJU0z4ebYZW4JwAWSUtVVp1oQAAAGsAAAJRAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAwAAAAAAGgRhAAAAAGZLKQsAAAAAAAAAAAAc0RsAAAAAAAAAACrpocAdnax31o9tGyySuVcwtYjCqzhK/YGT5XC4hH73AAAAF0h26AAAHNEbAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
                    "ledger": 1888539,
                    "createdAt": 1717166042
                  },
                  {
                    "status": "SUCCESS",
                    "applicationOrder": 3,
                    "feeBump": false,
                    "envelopeXdr": "AAAAAgAAAACwtG/IRC5DZE1UdekijEsoQEPM/uOwZ3iY/Y8UZ3b9xAAPQkAAGgRHAAAANgAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAABAAAAABB90WssODNIgi6BHveqzxTRmIpvAFRyVNM+Hm2GVuCcAAAAAAAAAADgdupKeB04lazKXCOb+E1JfxaM3tI4Xsb/qDa1MWOvXgAAABdIdugAAAAAAAAAAAJndv3EAAAAQKcTimw6KKcM0AeCMxXJcEK/hS9ROoj/qpMFppGNAr4W3ifSOSTGAFbA+cIVHmaV4p7xGcR+9JnUN1YjamvJZwSGVuCcAAAAQK9Cp775JbnYA793SXkkWWbmvnEFTiDPiFyTHxTphCwBDB1zqkXqGG6Q5O3dAyqkNJvj1XNRDsmY4pKV41qijQU=",
                    "resultXdr": "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAA=",
                    "resultMetaXdr": "AAAAAwAAAAAAAAACAAAAAwAc0RsAAAAAAAAAALC0b8hELkNkTVR16SKMSyhAQ8z+47BneJj9jxRndv3EAAAAADwzS2gAGgRHAAAANQAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAMAAAAAABzPVAAAAABmWdZ2AAAAAAAAAAEAHNEbAAAAAAAAAACwtG/IRC5DZE1UdekijEsoQEPM/uOwZ3iY/Y8UZ3b9xAAAAAA8M0toABoERwAAADYAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAc0RsAAAAAZlnf2gAAAAAAAAABAAAAAwAAAAMAHNEbAAAAAAAAAAAQfdFrLDgzSIIugR73qs8U0ZiKbwBUclTTPh5thlbgnABZJS1VWnWhAAAAawAAAlEAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAaBGEAAAAAZkspCwAAAAAAAAABABzRGwAAAAAAAAAAEH3Rayw4M0iCLoEe96rPFNGYim8AVHJU0z4ebYZW4JwAWSUWDOONoQAAAGsAAAJRAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAwAAAAAAGgRhAAAAAGZLKQsAAAAAAAAAAAAc0RsAAAAAAAAAAOB26kp4HTiVrMpcI5v4TUl/Foze0jhexv+oNrUxY69eAAAAF0h26AAAHNEbAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
                    "ledger": 1888539,
                    "createdAt": 1717166042
                  },
                  {
                    "status": "SUCCESS",
                    "applicationOrder": 4,
                    "feeBump": false,
                    "envelopeXdr": "AAAAAgAAAACxMt2gKYOehEoVbmh9vfvZ4mVzXFSNTbAU5S4a8zorrAA4wrwAHLqRAAAADAAAAAAAAAAAAAAAAQAAAAAAAAAYAAAAAQAAAAAAAAAAAAAAALEy3aApg56EShVuaH29+9niZXNcVI1NsBTlLhrzOiusz3K+BVgRzXig/Bhz1TL5Qy+Ibv6cDvCfdaAtBMMFPcYAAAAAHXUVmJM11pdJSKKV52UJrVYlvxaPLmmg17nMe0HGy0MAAAABAAAAAAAAAAEAAAAAAAAAAAAAAACxMt2gKYOehEoVbmh9vfvZ4mVzXFSNTbAU5S4a8zorrM9yvgVYEc14oPwYc9Uy+UMviG7+nA7wn3WgLQTDBT3GAAAAAB11FZiTNdaXSUiiledlCa1WJb8Wjy5poNe5zHtBxstDAAAAAAAAAAEAAAAAAAAAAQAAAAcddRWYkzXWl0lIopXnZQmtViW/Fo8uaaDXucx7QcbLQwAAAAEAAAAGAAAAAbolCtTsMrJvK0M2SaskFsaMajj3iAZbXxELZHwDyE5dAAAAFAAAAAEABf2jAAAd1AAAAGgAAAAAADjCWAAAAAHzOiusAAAAQM+qaiMKxMoCVNjdRIh3X9CSxkjAm0BpXYDB9Fd+DS0guYKiY3TMaVe243UB008iBn5ynQv724rReXlg7iFqXQA=",
                    "resultXdr": "AAAAAAAw3cUAAAAAAAAAAQAAAAAAAAAYAAAAAKg/pGuhtOG27rIpG8xhUIp46CStGWOcsGlNsTQv44UOAAAAAA==",
                    "resultMetaXdr": "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",
                    "diagnosticEventsXdr": [
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAACnJlYWRfZW50cnkAAAAAAAUAAAAAAAAAAg==",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAC3dyaXRlX2VudHJ5AAAAAAUAAAAAAAAAAQ==",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEGxlZGdlcl9yZWFkX2J5dGUAAAAFAAAAAAAAHdQ=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEWxlZGdlcl93cml0ZV9ieXRlAAAAAAAABQAAAAAAAABo",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAADXJlYWRfa2V5X2J5dGUAAAAAAAAFAAAAAAAAAFQ=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAADndyaXRlX2tleV9ieXRlAAAAAAAFAAAAAAAAAAA=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAADnJlYWRfZGF0YV9ieXRlAAAAAAAFAAAAAAAAAAA=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAD3dyaXRlX2RhdGFfYnl0ZQAAAAAFAAAAAAAAAGg=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAADnJlYWRfY29kZV9ieXRlAAAAAAAFAAAAAAAAHdQ=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAD3dyaXRlX2NvZGVfYnl0ZQAAAAAFAAAAAAAAAAA=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAACmVtaXRfZXZlbnQAAAAAAAUAAAAAAAAAAA==",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAD2VtaXRfZXZlbnRfYnl0ZQAAAAAFAAAAAAAAAAA=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAACGNwdV9pbnNuAAAABQAAAAAABTO4",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAACG1lbV9ieXRlAAAABQAAAAAAAPkD",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEWludm9rZV90aW1lX25zZWNzAAAAAAAABQAAAAAAAmiz",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAD21heF9yd19rZXlfYnl0ZQAAAAAFAAAAAAAAADA=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEG1heF9yd19kYXRhX2J5dGUAAAAFAAAAAAAAAGg=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEG1heF9yd19jb2RlX2J5dGUAAAAFAAAAAAAAHdQ=",
                      "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAE21heF9lbWl0X2V2ZW50X2J5dGUAAAAABQAAAAAAAAAA"
                    ],
                    "ledger": 1888539,
                    "createdAt": 1717166042
                  },
                  {
                    "status": "FAILED",
                    "applicationOrder": 1,
                    "feeBump": false,
                    "envelopeXdr": "AAAAAgAAAAAxLMEcxmfUgNzL687Js4sX/jmFQDqTo1Lj4KDoC1PeSQAehIAAAAIJAAtMUQAAAAEAAAAAAAAAAAAAAABmWeAVAAAAAQAAAAlwc3BiOjMyMTcAAAAAAAACAAAAAQAAAACKlutUN5GT3UOoE2BUkNtJEwoipGOinBFsQtXgpIZMxQAAAAEAAAAA433o+yremWU3t88cKpfpHR+JMFR44JHzmBGni6hqCEYAAAACQVRVQUgAAAAAAAAAAAAAAGfK1mN4mg51jbX6by6TWghGynQ463doEDgzriqZo9bzAAAAAAaOd4AAAAABAAAAAIqW61Q3kZPdQ6gTYFSQ20kTCiKkY6KcEWxC1eCkhkzFAAAAAQAAAADjfej7Kt6ZZTe3zxwql+kdH4kwVHjgkfOYEaeLqGoIRgAAAAJBVFVTRAAAAAAAAAAAAAAAZ8rWY3iaDnWNtfpvLpNaCEbKdDjrd2gQODOuKpmj1vMAAAAAADh1IAAAAAAAAAACC1PeSQAAAEBoad/kqj/4Sqq5tC6HyeMm5LJKM1VqKRGZc3e4uvA3ITThwn2nNMRJRegdQrLrPBTSgw51nY8npilXVIds7I0OpIZMxQAAAEDTZNaLjIDMWPDdCxa1ZB28vUxTcS/0xykOFTI/JAz096vX6Y7wI0QvnbPM7KCoL0cJAciD+pJxNqXQ2Aff1hoO",
                    "resultXdr": "AAAAAAAAAMj/////AAAAAgAAAAAAAAAB////+wAAAAAAAAAB////+wAAAAA=",
                    "resultMetaXdr": "AAAAAwAAAAAAAAACAAAAAwAc0RwAAAAAAAAAADEswRzGZ9SA3Mvrzsmzixf+OYVAOpOjUuPgoOgLU95JAAAAFxzxIbUAAAIJAAtMUAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAMAAAAAABzRGgAAAABmWd/VAAAAAAAAAAEAHNEcAAAAAAAAAAAxLMEcxmfUgNzL687Js4sX/jmFQDqTo1Lj4KDoC1PeSQAAABcc8SG1AAACCQALTFEAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAc0RwAAAAAZlnf3wAAAAAAAAAAAAAAAAAAAAA=",
                    "ledger": 1888540,
                    "createdAt": 1717166047
                  }
                ],
                "latestLedger": 1888542,
                "latestLedgerCloseTimestamp": 1717166057,
                "oldestLedger": 1871263,
                "oldestLedgerCloseTimestamp": 1717075350,
                "cursor": "8111217537191937"
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        var response = await sorobanServer.GetTransactions(new GetTransactionsRequest { StartLedger = 1888539 });

        Assert.IsNotNull(response);
        var transactions = response.Transactions;
        Assert.IsNotNull(transactions);
        Assert.AreEqual(1888542L, response.LatestLedger);
        Assert.AreEqual(1717166057L, response.LatestLedgerCloseTimestamp);
        Assert.AreEqual(1871263L, response.OldestLedger);
        Assert.AreEqual(1717075350L, response.OldestLedgerCloseTimestamp);
        Assert.AreEqual(5, transactions.Length);

        var tx1 = transactions[0];
        Assert.AreEqual(TransactionInfo.TransactionStatus.FAILED, tx1.Status);
        Assert.AreEqual(1888539L, tx1.Ledger);
        Assert.AreEqual(1717166042L, tx1.CreatedAt);
        Assert.AreEqual(1, tx1.ApplicationOrder);
        Assert.IsFalse(tx1.FeeBump);
        Assert.AreEqual(
            "AAAAAgAAAACDz21Q3CTITlGqRus3/96/05EDivbtfJncNQKt64BTbAAAASwAAKkyAAXlMwAAAAEAAAAAAAAAAAAAAABmWeASAAAAAQAAABR3YWxsZXQ6MTcxMjkwNjMzNjUxMAAAAAEAAAABAAAAAIPPbVDcJMhOUapG6zf/3r/TkQOK9u18mdw1Aq3rgFNsAAAAAQAAAABwOSvou8mtwTtCkysVioO35TSgyRir2+WGqO8FShG/GAAAAAFVQUgAAAAAAO371tlrHUfK+AvmQvHje1jSUrvJb3y3wrJ7EplQeqTkAAAAAAX14QAAAAAAAAAAAeuAU2wAAABAn+6A+xXvMasptAm9BEJwf5Y9CLLQtV44TsNqS8ocPmn4n8Rtyb09SBiFoMv8isYgeQU5nAHsIwBNbEKCerusAQ==",
            tx1.EnvelopeXdr);
        Assert.AreEqual("AAAAAAAAAGT/////AAAAAQAAAAAAAAAB////+gAAAAA=", tx1.ResultXdr);
        Assert.AreEqual(
            "AAAAAwAAAAAAAAACAAAAAwAc0RsAAAAAAAAAAIPPbVDcJMhOUapG6zf/3r/TkQOK9u18mdw1Aq3rgFNsAAAAF0YpYBQAAKkyAAXlMgAAAAsAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAMAAAAAABzRGgAAAABmWd/VAAAAAAAAAAEAHNEbAAAAAAAAAACDz21Q3CTITlGqRus3/96/05EDivbtfJncNQKt64BTbAAAABdGKWAUAACpMgAF5TMAAAALAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAc0RsAAAAAZlnf2gAAAAAAAAAAAAAAAAAAAAA=",
            tx1.ResultMetaXdr);

        var tx2 = transactions[1];
        Assert.AreEqual(TransactionInfo.TransactionStatus.SUCCESS, tx2.Status);
        Assert.AreEqual(1888539L, tx2.Ledger);
        Assert.AreEqual(1717166042L, tx2.CreatedAt);
        Assert.AreEqual(2, tx2.ApplicationOrder);
        Assert.IsFalse(tx2.FeeBump);
        Assert.AreEqual(
            "AAAAAgAAAAC4EZup+ewCs/doS3hKbeAa4EviBHqAFYM09oHuLtqrGAAPQkAAGgQZAAAANgAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAABAAAAABB90WssODNIgi6BHveqzxTRmIpvAFRyVNM+Hm2GVuCcAAAAAAAAAAAq6aHAHZ2sd9aPbRsskrlXMLWIwqs4Sv2Bk+VwuIR+9wAAABdIdugAAAAAAAAAAAIu2qsYAAAAQERzKOqYYiPXNwsiL8ADAG/f45RBssmf3umGzw4qKkLGlObuPdX0buWmTGrhI13SG38F2V8Mp9DI+eDkcCjMSAOGVuCcAAAAQHnm0o/r+Gsl+6oqBgSbqoSY37gflvQB3zZRghuir0N75UVerd0Q50yG5Zfu08i2crhx6uk+5HYTl8/Sa7uZ+Qc=",
            tx2.EnvelopeXdr);
        Assert.AreEqual("AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAA=", tx2.ResultXdr);
        Assert.AreEqual(
            "AAAAAwAAAAAAAAACAAAAAwAc0RsAAAAAAAAAALgRm6n57AKz92hLeEpt4BrgS+IEeoAVgzT2ge4u2qsYAAAAADwzS2gAGgQZAAAANQAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAMAAAAAABzPVAAAAABmWdZ2AAAAAAAAAAEAHNEbAAAAAAAAAAC4EZup+ewCs/doS3hKbeAa4EviBHqAFYM09oHuLtqrGAAAAAA8M0toABoEGQAAADYAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAc0RsAAAAAZlnf2gAAAAAAAAABAAAAAwAAAAMAHNEaAAAAAAAAAAAQfdFrLDgzSIIugR73qs8U0ZiKbwBUclTTPh5thlbgnABZJUSd0V2hAAAAawAAAlEAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAaBGEAAAAAZkspCwAAAAAAAAABABzRGwAAAAAAAAAAEH3Rayw4M0iCLoEe96rPFNGYim8AVHJU0z4ebYZW4JwAWSUtVVp1oQAAAGsAAAJRAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAwAAAAAAGgRhAAAAAGZLKQsAAAAAAAAAAAAc0RsAAAAAAAAAACrpocAdnax31o9tGyySuVcwtYjCqzhK/YGT5XC4hH73AAAAF0h26AAAHNEbAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
            tx2.ResultMetaXdr);

        var tx3 = transactions[2];
        Assert.AreEqual(TransactionInfo.TransactionStatus.SUCCESS, tx3.Status);
        Assert.AreEqual(1888539L, tx3.Ledger);
        Assert.AreEqual(1717166042L, tx3.CreatedAt);
        Assert.AreEqual(3, tx3.ApplicationOrder);
        Assert.IsFalse(tx3.FeeBump);
        Assert.AreEqual(
            "AAAAAgAAAACwtG/IRC5DZE1UdekijEsoQEPM/uOwZ3iY/Y8UZ3b9xAAPQkAAGgRHAAAANgAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAABAAAAABB90WssODNIgi6BHveqzxTRmIpvAFRyVNM+Hm2GVuCcAAAAAAAAAADgdupKeB04lazKXCOb+E1JfxaM3tI4Xsb/qDa1MWOvXgAAABdIdugAAAAAAAAAAAJndv3EAAAAQKcTimw6KKcM0AeCMxXJcEK/hS9ROoj/qpMFppGNAr4W3ifSOSTGAFbA+cIVHmaV4p7xGcR+9JnUN1YjamvJZwSGVuCcAAAAQK9Cp775JbnYA793SXkkWWbmvnEFTiDPiFyTHxTphCwBDB1zqkXqGG6Q5O3dAyqkNJvj1XNRDsmY4pKV41qijQU=",
            tx3.EnvelopeXdr);
        Assert.AreEqual("AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAA=", tx3.ResultXdr);
        Assert.AreEqual(
            "AAAAAwAAAAAAAAACAAAAAwAc0RsAAAAAAAAAALC0b8hELkNkTVR16SKMSyhAQ8z+47BneJj9jxRndv3EAAAAADwzS2gAGgRHAAAANQAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAMAAAAAABzPVAAAAABmWdZ2AAAAAAAAAAEAHNEbAAAAAAAAAACwtG/IRC5DZE1UdekijEsoQEPM/uOwZ3iY/Y8UZ3b9xAAAAAA8M0toABoERwAAADYAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAc0RsAAAAAZlnf2gAAAAAAAAABAAAAAwAAAAMAHNEbAAAAAAAAAAAQfdFrLDgzSIIugR73qs8U0ZiKbwBUclTTPh5thlbgnABZJS1VWnWhAAAAawAAAlEAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAaBGEAAAAAZkspCwAAAAAAAAABABzRGwAAAAAAAAAAEH3Rayw4M0iCLoEe96rPFNGYim8AVHJU0z4ebYZW4JwAWSUWDOONoQAAAGsAAAJRAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAAAAAAAAAAAAwAAAAAAGgRhAAAAAGZLKQsAAAAAAAAAAAAc0RsAAAAAAAAAAOB26kp4HTiVrMpcI5v4TUl/Foze0jhexv+oNrUxY69eAAAAF0h26AAAHNEbAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
            tx3.ResultMetaXdr);

        var tx4 = transactions[3];
        Assert.AreEqual(TransactionInfo.TransactionStatus.SUCCESS, tx4.Status);
        Assert.AreEqual(1888539L, tx4.Ledger);
        Assert.AreEqual(1717166042L, tx4.CreatedAt);
        Assert.AreEqual(4, tx4.ApplicationOrder);
        Assert.IsFalse(tx4.FeeBump);
        Assert.AreEqual(
            "AAAAAgAAAACxMt2gKYOehEoVbmh9vfvZ4mVzXFSNTbAU5S4a8zorrAA4wrwAHLqRAAAADAAAAAAAAAAAAAAAAQAAAAAAAAAYAAAAAQAAAAAAAAAAAAAAALEy3aApg56EShVuaH29+9niZXNcVI1NsBTlLhrzOiusz3K+BVgRzXig/Bhz1TL5Qy+Ibv6cDvCfdaAtBMMFPcYAAAAAHXUVmJM11pdJSKKV52UJrVYlvxaPLmmg17nMe0HGy0MAAAABAAAAAAAAAAEAAAAAAAAAAAAAAACxMt2gKYOehEoVbmh9vfvZ4mVzXFSNTbAU5S4a8zorrM9yvgVYEc14oPwYc9Uy+UMviG7+nA7wn3WgLQTDBT3GAAAAAB11FZiTNdaXSUiiledlCa1WJb8Wjy5poNe5zHtBxstDAAAAAAAAAAEAAAAAAAAAAQAAAAcddRWYkzXWl0lIopXnZQmtViW/Fo8uaaDXucx7QcbLQwAAAAEAAAAGAAAAAbolCtTsMrJvK0M2SaskFsaMajj3iAZbXxELZHwDyE5dAAAAFAAAAAEABf2jAAAd1AAAAGgAAAAAADjCWAAAAAHzOiusAAAAQM+qaiMKxMoCVNjdRIh3X9CSxkjAm0BpXYDB9Fd+DS0guYKiY3TMaVe243UB008iBn5ynQv724rReXlg7iFqXQA=",
            tx4.EnvelopeXdr);
        Assert.AreEqual("AAAAAAAw3cUAAAAAAAAAAQAAAAAAAAAYAAAAAKg/pGuhtOG27rIpG8xhUIp46CStGWOcsGlNsTQv44UOAAAAAA==",
            tx4.ResultXdr);
        Assert.AreEqual(
            "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",
            tx4.ResultMetaXdr);
        var tx4DiagnosticEventsXdr = tx4.DiagnosticEventsXdr;
        Assert.IsNotNull(tx4DiagnosticEventsXdr);
        Assert.AreEqual(19, tx4DiagnosticEventsXdr.Length);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAACnJlYWRfZW50cnkAAAAAAAUAAAAAAAAAAg==",
            tx4DiagnosticEventsXdr[0]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAC3dyaXRlX2VudHJ5AAAAAAUAAAAAAAAAAQ==",
            tx4DiagnosticEventsXdr[1]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEGxlZGdlcl9yZWFkX2J5dGUAAAAFAAAAAAAAHdQ=",
            tx4DiagnosticEventsXdr[2]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEWxlZGdlcl93cml0ZV9ieXRlAAAAAAAABQAAAAAAAABo",
            tx4DiagnosticEventsXdr[3]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAADXJlYWRfa2V5X2J5dGUAAAAAAAAFAAAAAAAAAFQ=",
            tx4DiagnosticEventsXdr[4]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAADndyaXRlX2tleV9ieXRlAAAAAAAFAAAAAAAAAAA=",
            tx4DiagnosticEventsXdr[5]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAADnJlYWRfZGF0YV9ieXRlAAAAAAAFAAAAAAAAAAA=",
            tx4DiagnosticEventsXdr[6]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAD3dyaXRlX2RhdGFfYnl0ZQAAAAAFAAAAAAAAAGg=",
            tx4DiagnosticEventsXdr[7]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAADnJlYWRfY29kZV9ieXRlAAAAAAAFAAAAAAAAHdQ=",
            tx4DiagnosticEventsXdr[8]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAD3dyaXRlX2NvZGVfYnl0ZQAAAAAFAAAAAAAAAAA=",
            tx4DiagnosticEventsXdr[9]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAACmVtaXRfZXZlbnQAAAAAAAUAAAAAAAAAAA==",
            tx4DiagnosticEventsXdr[10]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAD2VtaXRfZXZlbnRfYnl0ZQAAAAAFAAAAAAAAAAA=",
            tx4DiagnosticEventsXdr[11]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAACGNwdV9pbnNuAAAABQAAAAAABTO4",
            tx4DiagnosticEventsXdr[12]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAACG1lbV9ieXRlAAAABQAAAAAAAPkD",
            tx4DiagnosticEventsXdr[13]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEWludm9rZV90aW1lX25zZWNzAAAAAAAABQAAAAAAAmiz",
            tx4DiagnosticEventsXdr[14]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAD21heF9yd19rZXlfYnl0ZQAAAAAFAAAAAAAAADA=",
            tx4DiagnosticEventsXdr[15]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEG1heF9yd19kYXRhX2J5dGUAAAAFAAAAAAAAAGg=",
            tx4DiagnosticEventsXdr[16]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEG1heF9yd19jb2RlX2J5dGUAAAAFAAAAAAAAHdQ=",
            tx4DiagnosticEventsXdr[17]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAE21heF9lbWl0X2V2ZW50X2J5dGUAAAAABQAAAAAAAAAA",
            tx4DiagnosticEventsXdr[18]);

        var tx5 = transactions[4];
        Assert.AreEqual(TransactionInfo.TransactionStatus.FAILED, tx5.Status);
        Assert.AreEqual(1888540L, tx5.Ledger);
        Assert.AreEqual(1717166047L, tx5.CreatedAt);
        Assert.AreEqual(1, tx5.ApplicationOrder);
        Assert.IsFalse(tx5.FeeBump);
        Assert.AreEqual(
            "AAAAAgAAAAAxLMEcxmfUgNzL687Js4sX/jmFQDqTo1Lj4KDoC1PeSQAehIAAAAIJAAtMUQAAAAEAAAAAAAAAAAAAAABmWeAVAAAAAQAAAAlwc3BiOjMyMTcAAAAAAAACAAAAAQAAAACKlutUN5GT3UOoE2BUkNtJEwoipGOinBFsQtXgpIZMxQAAAAEAAAAA433o+yremWU3t88cKpfpHR+JMFR44JHzmBGni6hqCEYAAAACQVRVQUgAAAAAAAAAAAAAAGfK1mN4mg51jbX6by6TWghGynQ463doEDgzriqZo9bzAAAAAAaOd4AAAAABAAAAAIqW61Q3kZPdQ6gTYFSQ20kTCiKkY6KcEWxC1eCkhkzFAAAAAQAAAADjfej7Kt6ZZTe3zxwql+kdH4kwVHjgkfOYEaeLqGoIRgAAAAJBVFVTRAAAAAAAAAAAAAAAZ8rWY3iaDnWNtfpvLpNaCEbKdDjrd2gQODOuKpmj1vMAAAAAADh1IAAAAAAAAAACC1PeSQAAAEBoad/kqj/4Sqq5tC6HyeMm5LJKM1VqKRGZc3e4uvA3ITThwn2nNMRJRegdQrLrPBTSgw51nY8npilXVIds7I0OpIZMxQAAAEDTZNaLjIDMWPDdCxa1ZB28vUxTcS/0xykOFTI/JAz096vX6Y7wI0QvnbPM7KCoL0cJAciD+pJxNqXQ2Aff1hoO",
            tx5.EnvelopeXdr);
        Assert.AreEqual("AAAAAAAAAMj/////AAAAAgAAAAAAAAAB////+wAAAAAAAAAB////+wAAAAA=", tx5.ResultXdr);
        Assert.AreEqual(
            "AAAAAwAAAAAAAAACAAAAAwAc0RwAAAAAAAAAADEswRzGZ9SA3Mvrzsmzixf+OYVAOpOjUuPgoOgLU95JAAAAFxzxIbUAAAIJAAtMUAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAMAAAAAABzRGgAAAABmWd/VAAAAAAAAAAEAHNEcAAAAAAAAAAAxLMEcxmfUgNzL687Js4sX/jmFQDqTo1Lj4KDoC1PeSQAAABcc8SG1AAACCQALTFEAAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAADAAAAAAAc0RwAAAAAZlnf3wAAAAAAAAAAAAAAAAAAAAA=",
            tx5.ResultMetaXdr);
    }
    // TODO TestGetLedgerEntriesOfTypeTTL()
    // TODO TestGetLedgerEntriesOfTypeConfigSetting()
}