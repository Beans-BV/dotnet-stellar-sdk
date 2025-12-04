using System;
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
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Requests.SorobanRpc;
using StellarDotnetSdk.Responses.SorobanRpc;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using CollectionAssert = NUnit.Framework.CollectionAssert;
using ConfigSettingContractLedgerCostExtV0 = StellarDotnetSdk.LedgerEntries.ConfigSettingContractLedgerCostExtV0;
using ConfigSettingContractParallelComputeV0 = StellarDotnetSdk.LedgerEntries.ConfigSettingContractParallelComputeV0;
using EvictionIterator = StellarDotnetSdk.LedgerEntries.EvictionIterator;
using FeeBumpTransaction = StellarDotnetSdk.Transactions.FeeBumpTransaction;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using SCContractInstance = StellarDotnetSdk.Soroban.SCContractInstance;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using SCVec = StellarDotnetSdk.Soroban.SCVec;
using StateArchivalSettings = StellarDotnetSdk.LedgerEntries.StateArchivalSettings;
using Transaction = StellarDotnetSdk.Transactions.Transaction;
using TransactionMetaV3 = StellarDotnetSdk.Soroban.TransactionMetaV3;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class SorobanServerTest
{
    private const string HelloContractWasmHash = "c1a650506f7c20c8f4d16aae73f894f302cd011d7ef33adef572f20b34f7653e";

    private readonly KeyPair _account =
        KeyPair.FromSecretSeed("SBQZZETKBHMRVNPEM7TMYAXORIRIDBBS6HD43C3PFH75SI54QAC6YTE2");

    // "GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS";
    private string AccountId => _account.AccountId;

    [TestMethod]
    public async Task TestGetHealth()
    {
        const string getNetworkResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "status": "healthy",
                "latestLedger": 453892,
                "oldestLedger": 436613,
                "ledgerRetentionWindow": 17280
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(getNetworkResponseJson);
        var response = await sorobanServer.GetHealth();

        Assert.AreEqual("healthy", response.Status);
        Assert.AreEqual(453892L, response.LatestLedger);
        Assert.AreEqual(436613L, response.OldestLedger);
        Assert.AreEqual(17280L, response.LedgerRetentionWindow);
    }

    [TestMethod]
    public async Task TestGetNetwork()
    {
        const string getNetworkResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "friendbotUrl": "https://friendbot.stellar.org/",
                "passphrase": "Test SDF Network ; September 2015",
                "protocolVersion": 21
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(getNetworkResponseJson);
        var response = await sorobanServer.GetNetwork();

        Assert.AreEqual("https://friendbot.stellar.org/", response.FriendbotUrl);
        Assert.AreEqual("Test SDF Network ; September 2015", response.Passphrase);
        Assert.AreEqual(21, response.ProtocolVersion);
    }

    [TestMethod]
    public async Task TestGetLatestLedger()
    {
        const string getLatestLedgerResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "id": "6bdb3e5cd5dcbf53df4b67dd56f892d0134c5abfb659234a83778af0b85620fe",
                "protocolVersion": 21,
                "sequence": 453871
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(getLatestLedgerResponseJson);
        var response = await sorobanServer.GetLatestLedger();
        Assert.AreEqual(21, response.ProtocolVersion);
        Assert.AreEqual(453871, response.Sequence);
        Assert.AreEqual("6bdb3e5cd5dcbf53df4b67dd56f892d0134c5abfb659234a83778af0b85620fe", response.Id);
    }

    [TestMethod]
    public async Task TestSendTransactionPending()
    {
        const string sendTransactionResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "d3fe7352-21c6-440f-b911-a7236100a41a",
              "result": {
                "status": "PENDING",
                "hash": "8b8c40fb49f4fb2880884ddeba30253e5b63e02a8da4bac40878bac66a08bbf0",
                "latestLedger": 453130,
                "latestLedgerCloseTime": "1728974496"
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(sendTransactionResponseJson);
        var response = await sorobanServer.SendTransaction(CreateDummyTransaction());

        Assert.IsNotNull(response);
        Assert.IsNull(response.ErrorResultXdr);
        Assert.AreEqual(SendTransactionResponse.SendTransactionStatus.PENDING, response.Status);
        Assert.AreEqual("8b8c40fb49f4fb2880884ddeba30253e5b63e02a8da4bac40878bac66a08bbf0", response.Hash);
        Assert.AreEqual(453130L, response.LatestLedger);
        Assert.AreEqual(1728974496L, response.LatestLedgerCloseTime);
    }

    [TestMethod]
    public async Task TestSendTransactionTryAgainLater()
    {
        const string sendTransactionResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "status": "TRY_AGAIN_LATER",
                "hash": "1744683ce7f874586990b3b70c4beab249a714b2679cf1dd76d80ade60e46a6e",
                "latestLedger": 453745,
                "latestLedgerCloseTime": "1728977723"
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(sendTransactionResponseJson);
        var response = await sorobanServer.SendTransaction(CreateDummyTransaction());

        Assert.IsNotNull(response);
        Assert.IsNull(response.ErrorResultXdr);
        Assert.AreEqual(SendTransactionResponse.SendTransactionStatus.TRY_AGAIN_LATER, response.Status);
        Assert.AreEqual("1744683ce7f874586990b3b70c4beab249a714b2679cf1dd76d80ade60e46a6e", response.Hash);
        Assert.AreEqual(453745L, response.LatestLedger);
        Assert.AreEqual(1728977723L, response.LatestLedgerCloseTime);
    }

    [TestMethod]
    public async Task TestSendTransactionError()
    {
        const string sendTransactionResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "errorResultXdr": "AAAAAAAAAGT////7AAAAAA==",
                "status": "ERROR",
                "hash": "1744683ce7f874586990b3b70c4beab249a714b2679cf1dd76d80ade60e46a6e",
                "latestLedger": 453756,
                "latestLedgerCloseTime": "1728977779"
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(sendTransactionResponseJson);
        var response = await sorobanServer.SendTransaction(CreateDummyTransaction());

        Assert.IsNotNull(response);
        Assert.AreEqual(SendTransactionResponse.SendTransactionStatus.ERROR, response.Status);
        Assert.AreEqual("AAAAAAAAAGT////7AAAAAA==", response.ErrorResultXdr);
        Assert.AreEqual("1744683ce7f874586990b3b70c4beab249a714b2679cf1dd76d80ade60e46a6e", response.Hash);
        Assert.AreEqual(453756L, response.LatestLedger);
        Assert.AreEqual(1728977779L, response.LatestLedgerCloseTime);
    }

    [TestMethod]
    public async Task TestSendTransactionDuplicate()
    {
        const string sendTransactionResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "status": "DUPLICATE",
                "hash": "198d834b53c4f0d44119f400c092bf3b3225ddce302cb060c7917445f57e237e",
                "latestLedger": 453756,
                "latestLedgerCloseTime": "1728978088"
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(sendTransactionResponseJson);
        var response = await sorobanServer.SendTransaction(CreateDummyTransaction());

        Assert.IsNotNull(response);
        Assert.AreEqual(SendTransactionResponse.SendTransactionStatus.DUPLICATE, response.Status);
        Assert.IsNull(response.ErrorResultXdr);
        Assert.AreEqual("198d834b53c4f0d44119f400c092bf3b3225ddce302cb060c7917445f57e237e", response.Hash);
        Assert.AreEqual(453756L, response.LatestLedger);
        Assert.AreEqual(1728978088L, response.LatestLedgerCloseTime);
    }

    [TestMethod]
    public async Task TestSendFeeBumpTransactionPending()
    {
        const string sendTransactionResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "d3fe7352-21c6-440f-b911-a7236100a41a",
              "result": {
                "status": "PENDING",
                "hash": "8b8c40fb49f4fb2880884ddeba30253e5b63e02a8da4bac40878bac66a08bbf0",
                "latestLedger": 453130,
                "latestLedgerCloseTime": "1728974496"
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(sendTransactionResponseJson);
        var response = await sorobanServer.SendTransaction(CreateDummyFeeBumpTransaction());

        Assert.IsNotNull(response);
        Assert.IsNull(response.ErrorResultXdr);
        Assert.AreEqual(SendTransactionResponse.SendTransactionStatus.PENDING, response.Status);
        Assert.AreEqual("8b8c40fb49f4fb2880884ddeba30253e5b63e02a8da4bac40878bac66a08bbf0", response.Hash);
        Assert.AreEqual(453130L, response.LatestLedger);
        Assert.AreEqual(1728974496L, response.LatestLedgerCloseTime);
    }

    [TestMethod]
    public async Task TestSendFeeBumpTransactionTryAgainLater()
    {
        const string sendTransactionResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "status": "TRY_AGAIN_LATER",
                "hash": "1744683ce7f874586990b3b70c4beab249a714b2679cf1dd76d80ade60e46a6e",
                "latestLedger": 453745,
                "latestLedgerCloseTime": "1728977723"
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(sendTransactionResponseJson);
        var response = await sorobanServer.SendTransaction(CreateDummyFeeBumpTransaction());

        Assert.IsNotNull(response);
        Assert.IsNull(response.ErrorResultXdr);
        Assert.AreEqual(SendTransactionResponse.SendTransactionStatus.TRY_AGAIN_LATER, response.Status);
        Assert.AreEqual("1744683ce7f874586990b3b70c4beab249a714b2679cf1dd76d80ade60e46a6e", response.Hash);
        Assert.AreEqual(453745L, response.LatestLedger);
        Assert.AreEqual(1728977723L, response.LatestLedgerCloseTime);
    }

    [TestMethod]
    public async Task TestSendFeeBumpTransactionError()
    {
        const string sendTransactionResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "errorResultXdr": "AAAAAAAAAGT////7AAAAAA==",
                "status": "ERROR",
                "hash": "1744683ce7f874586990b3b70c4beab249a714b2679cf1dd76d80ade60e46a6e",
                "latestLedger": 453756,
                "latestLedgerCloseTime": "1728977779"
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(sendTransactionResponseJson);
        var response = await sorobanServer.SendTransaction(CreateDummyFeeBumpTransaction());

        Assert.IsNotNull(response);
        Assert.AreEqual(SendTransactionResponse.SendTransactionStatus.ERROR, response.Status);
        Assert.AreEqual("AAAAAAAAAGT////7AAAAAA==", response.ErrorResultXdr);
        Assert.AreEqual("1744683ce7f874586990b3b70c4beab249a714b2679cf1dd76d80ade60e46a6e", response.Hash);
        Assert.AreEqual(453756L, response.LatestLedger);
        Assert.AreEqual(1728977779L, response.LatestLedgerCloseTime);
    }

    [TestMethod]
    public async Task TestSendFeeBumpTransactionDuplicate()
    {
        const string sendTransactionResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "status": "DUPLICATE",
                "hash": "198d834b53c4f0d44119f400c092bf3b3225ddce302cb060c7917445f57e237e",
                "latestLedger": 453756,
                "latestLedgerCloseTime": "1728978088"
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(sendTransactionResponseJson);
        var response = await sorobanServer.SendTransaction(CreateDummyFeeBumpTransaction());

        Assert.IsNotNull(response);
        Assert.AreEqual(SendTransactionResponse.SendTransactionStatus.DUPLICATE, response.Status);
        Assert.IsNull(response.ErrorResultXdr);
        Assert.AreEqual("198d834b53c4f0d44119f400c092bf3b3225ddce302cb060c7917445f57e237e", response.Hash);
        Assert.AreEqual(453756L, response.LatestLedger);
        Assert.AreEqual(1728978088L, response.LatestLedgerCloseTime);
    }

    [TestMethod]
    public async Task TestSimulateSignedTransaction()
    {
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent("");
        await Assert.ThrowsExceptionAsync<TooManySignaturesException>(() =>
            sorobanServer.SimulateTransaction(CreateDummyTransaction(true)));
    }

    [TestMethod]
    public async Task TestSimulateTransactionFailed()
    {
        const string simulateTransactionResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "a6f290c3-109c-4e91-a2f2-6dbeb77d0436",
              "result": {
                "error": "HostError: Error(Storage, MissingValue)\n\nEvent log (newest first):\n   0: [Diagnostic Event] topics:[error, Error(Storage, MissingValue)], data:\"trying to get non-existing value for contract instance\"\n   1: [Diagnostic Event] topics:[fn_call, Bytes(d984dfd95d19b413c6472426347d2d35ba2dc876c96721cabdaec078e67913dd), hello1], data:gents\n\nBacktrace (newest first):\n   0: soroban_env_host::storage::Storage::get_with_host\n   1: soroban_env_host::host::data_helper::\u003cimpl soroban_env_host::host::Host\u003e::retrieve_contract_instance_from_storage\n   2: soroban_env_host::host::frame::\u003cimpl soroban_env_host::host::Host\u003e::call_n_internal\n   3: soroban_env_host::host::frame::\u003cimpl soroban_env_host::host::Host\u003e::invoke_function_and_return_val::{{closure}}\n   4: soroban_env_host::host::frame::\u003cimpl soroban_env_host::host::Host\u003e::invoke_function\n   5: soroban_env_host::e2e_invoke::invoke_host_function_in_recording_mode\n   6: soroban_simulation::simulation::simulate_invoke_host_function_op\n   7: preflight::preflight_invoke_hf_op::{{closure}}\n   8: core::ops::function::FnOnce::call_once{{vtable.shim}}\n   9: preflight::catch_preflight_panic\n  10: preflight_invoke_hf_op\n  11: _cgo_4dfa88039e17_Cfunc_preflight_invoke_hf_op\n             at tmp/go-build/cgo-gcc-prolog:105:11\n  12: runtime.asmcgocall\n             at ./runtime/asm_amd64.s:918\n\n",
                "events": [
                  "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAADAAAADwAAAAdmbl9jYWxsAAAAAA0AAAAg2YTf2V0ZtBPGRyQmNH0tNbotyHbJZyHKva7AeOZ5E90AAAAPAAAABmhlbGxvMQAAAAAADwAAAAVnZW50cwAAAA\u003d\u003d",
                  "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAVlcnJvcgAAAAAAAAIAAAADAAAAAwAAAA4AAAA2dHJ5aW5nIHRvIGdldCBub24tZXhpc3RpbmcgdmFsdWUgZm9yIGNvbnRyYWN0IGluc3RhbmNlAAA\u003d"
                ],
                "latestLedger": 453979
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(simulateTransactionResponseJson);
        var response = await sorobanServer.SimulateTransaction(CreateDummyTransaction(false));

        Assert.AreEqual(453979L, response.LatestLedger);
        Assert.IsNull(response.Results);
        Assert.IsNotNull(response.Error);

        var diagnosticEventStrings = response.Events;

        Assert.IsNotNull(diagnosticEventStrings);
        Assert.AreEqual(2, diagnosticEventStrings.Length);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAADAAAADwAAAAdmbl9jYWxsAAAAAA0AAAAg2YTf2V0ZtBPGRyQmNH0tNbotyHbJZyHKva7AeOZ5E90AAAAPAAAABmhlbGxvMQAAAAAADwAAAAVnZW50cwAAAA==",
            diagnosticEventStrings[0]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAVlcnJvcgAAAAAAAAIAAAADAAAAAwAAAA4AAAA2dHJ5aW5nIHRvIGdldCBub24tZXhpc3RpbmcgdmFsdWUgZm9yIGNvbnRyYWN0IGluc3RhbmNlAAA=",
            diagnosticEventStrings[1]);
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
                    "topic": [
                      "AAAADwAAAAhTVFJfQ0VSVA\u003d\u003d",
                      "AAAADwAAAAhzdHJfY2VydA\u003d\u003d"
                    ],
                    "value": "AAAAEQAAAAEAAAADAAAADwAAAARoYXNoAAAADgAAAEAzYjdkODUxYjVjNmMyMTNmNzUyYzdmNzJhNDA0Yjg5NGFiZGU2NDY2NDhjMzU0MmQ3MDRlMjI4OTMwNmU1MTFhAAAADwAAAAZwYXJlbnQAAAAAAA4AAAAAAAAADwAAAAd2ZXJzaW9uAAAAAAMAAAAA",
                    "inSuccessfulContractCall": true,
                    "operationIndex": 1,
                    "transactionIndex": 2,
                    "txHash": "9f6cf2cf2d1dd41af039325503ba98daf3cfa10d0079cd2c50a28355fb1b4af2"
                  },
                  {
                    "type": "contract",
                    "ledger": 912723,
                    "ledgerClosedAt": "2024-08-06T10:10:47Z",
                    "contractId": "CDTJALOV4KLSPEMNFHKYSG4WOTN7FCN4A2JOKRPVCQYEHLUEH2YUJF5R",
                    "id": "0003920115435319296-0000000001",
                    "topic": [
                      "AAAADwAAAARtaW50",
                      "AAAAEgAAAAAAAAAAdWnjmUMJ9zn2dIq1d6OhQ7XzqNT2ppF+9OgDmID0yhQ\u003d",
                      "AAAAEgAAAAAAAAAAoM2uDbnOXB4fd/4Y3ZRlbiis4zPp1sdQNyQHWQuvzq4\u003d"
                    ],
                    "value": "AAAACgAAAAAAAAAAAAAJGE5yoAA\u003d",
                    "inSuccessfulContractCall": true,
                    "operationIndex": 3,
                    "transactionIndex": 4,
                    "txHash": "318915004f904a36fddcefa8d4935ab21db7848a5a5e528815672968125a79a8"
                  },
                  {
                    "type": "contract",
                    "ledger": 912725,
                    "ledgerClosedAt": "2024-08-06T10:10:58Z",
                    "contractId": "CDYTK2FLRHT3KJ6RVAAABI4A7Y2XERCWN3FT5II7FHONDKPQVEAZ33YI",
                    "id": "0003920124025257984-0000000001",
                    "topic": [
                      "AAAADwAAAARtaW50",
                      "AAAAEgAAAAAAAAAAdWnjmUMJ9zn2dIq1d6OhQ7XzqNT2ppF+9OgDmID0yhQ\u003d",
                      "AAAAEgAAAAAAAAAAdT55ljEK4qTy1Y7Fw9KtdAAZIBkrd2p7IX0ZByhSO2k\u003d"
                    ],
                    "value": "AAAACgAAAAAAAAAAAAAJGE5yoAA\u003d",
                    "inSuccessfulContractCall": true,
                    "operationIndex": 5,
                    "transactionIndex": 6,
                    "txHash": "944d99fd572b8541f5c0ce95881b844d79144fe9a5cef2aea1cf94fb7c91f1f7"
                  },
                  {
                    "type": "contract",
                    "ledger": 913037,
                    "ledgerClosedAt": "2024-08-06T10:38:23Z",
                    "contractId": "CDV6IIE2DFFGB3GKAG7YYSKBO4PFDAZ76GRHXKSOBBIG64NNHMMRCRXH",
                    "id": "0003921464055050240-0000000001",
                    "topic": [
                      "AAAADwAAAAdhcHByb3ZlAA\u003d\u003d",
                      "AAAAEgAAAAAAAAAAWoM+w+i/0MLTSydKU896zcL4/EhYLVMHlSxIzY+ucJs\u003d",
                      "AAAAEgAAAAGPj5dVA0lfIe7VhCeJwcQ68vhwUcgQjj7XNcpMoUn7Hw\u003d\u003d"
                    ],
                    "value": "AAAAEAAAAAEAAAACAAAACgAAAAAAAAAAAAQdH3d3yKkAAAADABD7yA\u003d\u003d",
                    "inSuccessfulContractCall": true,
                    "operationIndex": 7,
                    "transactionIndex": 8,
                    "txHash": "64bd9d003dbc4c9f766206dee34d57285322eeee6c5acb6d2a31d2668d88c2fd"
                  }
                ],
                "latestLedger": 913609,
                "oldestLedger": 2175664,
                "latestLedgerCloseTime": "1751862824",
                "oldestLedgerCloseTime": "1751853705",
                "cursor": "0003920046715838464-0000000001"
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);

        var eventsResponse = await sorobanServer.GetEvents(new GetEventsRequest());

        Assert.IsNotNull(eventsResponse);
        Assert.IsNotNull(eventsResponse.Events);
        Assert.AreEqual(913609L, eventsResponse.LatestLedger);
        Assert.AreEqual(2175664L, eventsResponse.OldestLedger);
        Assert.AreEqual(1751862824L, eventsResponse.LatestLedgerCloseTime);
        Assert.AreEqual(1751853705L, eventsResponse.OldestLedgerCloseTime);
        Assert.AreEqual("0003920046715838464-0000000001", eventsResponse.Cursor);
        Assert.AreEqual(4, eventsResponse.Events.Length);
        var event1 = eventsResponse.Events[0];

        Assert.IsNotNull(event1);
        Assert.AreEqual("contract", event1.Type);
        Assert.AreEqual(912707, event1.Ledger);
        Assert.AreEqual(new DateTimeOffset(2024, 8, 6, 10, 9, 22, TimeSpan.Zero), event1.LedgerClosedAt);
        Assert.AreEqual("CASCLAHV7E7H3BOGQIW5HIC3H6WVDOTOQRTRMXYSTKJHXOORP3DNATY2", event1.ContractId);
        Assert.AreEqual("0003920046715838464-0000000001", event1.Id);
        Assert.AreEqual(2, event1.Topics.Length);
        Assert.AreEqual("AAAADwAAAAhTVFJfQ0VSVA==", event1.Topics[0]);
        Assert.AreEqual("AAAADwAAAAhzdHJfY2VydA==", event1.Topics[1]);
        Assert.AreEqual(
            "AAAAEQAAAAEAAAADAAAADwAAAARoYXNoAAAADgAAAEAzYjdkODUxYjVjNmMyMTNmNzUyYzdmNzJhNDA0Yjg5NGFiZGU2NDY2NDhjMzU0MmQ3MDRlMjI4OTMwNmU1MTFhAAAADwAAAAZwYXJlbnQAAAAAAA4AAAAAAAAADwAAAAd2ZXJzaW9uAAAAAAMAAAAA",
            event1.Value);
        Assert.IsTrue(event1.InSuccessfulContractCall);
        Assert.AreEqual(2U, event1.TransactionIndex);
        Assert.AreEqual(1U, event1.OperationIndex);
        Assert.AreEqual("9f6cf2cf2d1dd41af039325503ba98daf3cfa10d0079cd2c50a28355fb1b4af2", event1.TransactionHash);

        var event2 = eventsResponse.Events[1];
        Assert.IsNotNull(event2);
        Assert.AreEqual("contract", event2.Type);
        Assert.AreEqual(912723, event2.Ledger);
        Assert.AreEqual(new DateTimeOffset(2024, 8, 6, 10, 10, 47, TimeSpan.Zero), event2.LedgerClosedAt);
        Assert.AreEqual("CDTJALOV4KLSPEMNFHKYSG4WOTN7FCN4A2JOKRPVCQYEHLUEH2YUJF5R", event2.ContractId);
        Assert.AreEqual("0003920115435319296-0000000001", event2.Id);
        Assert.AreEqual(3, event2.Topics.Length);
        Assert.AreEqual("AAAADwAAAARtaW50", event2.Topics[0]);
        Assert.AreEqual("AAAAEgAAAAAAAAAAdWnjmUMJ9zn2dIq1d6OhQ7XzqNT2ppF+9OgDmID0yhQ=", event2.Topics[1]);
        Assert.AreEqual("AAAAEgAAAAAAAAAAoM2uDbnOXB4fd/4Y3ZRlbiis4zPp1sdQNyQHWQuvzq4=", event2.Topics[2]);
        Assert.AreEqual("AAAACgAAAAAAAAAAAAAJGE5yoAA=", event2.Value);
        Assert.IsTrue(event2.InSuccessfulContractCall);
        Assert.AreEqual(4U, event2.TransactionIndex);
        Assert.AreEqual(3U, event2.OperationIndex);
        Assert.AreEqual("318915004f904a36fddcefa8d4935ab21db7848a5a5e528815672968125a79a8", event2.TransactionHash);

        var event3 = eventsResponse.Events[2];
        Assert.IsNotNull(event3);
        Assert.AreEqual("contract", event3.Type);
        Assert.AreEqual(912725, event3.Ledger);
        Assert.AreEqual(new DateTimeOffset(2024, 8, 6, 10, 10, 58, TimeSpan.Zero), event3.LedgerClosedAt);
        Assert.AreEqual("CDYTK2FLRHT3KJ6RVAAABI4A7Y2XERCWN3FT5II7FHONDKPQVEAZ33YI", event3.ContractId);
        Assert.AreEqual("0003920124025257984-0000000001", event3.Id);
        Assert.AreEqual(3, event3.Topics.Length);
        Assert.AreEqual("AAAADwAAAARtaW50", event3.Topics[0]);
        Assert.AreEqual("AAAAEgAAAAAAAAAAdWnjmUMJ9zn2dIq1d6OhQ7XzqNT2ppF+9OgDmID0yhQ=", event3.Topics[1]);
        Assert.AreEqual("AAAAEgAAAAAAAAAAdT55ljEK4qTy1Y7Fw9KtdAAZIBkrd2p7IX0ZByhSO2k=", event3.Topics[2]);
        Assert.AreEqual("AAAACgAAAAAAAAAAAAAJGE5yoAA=", event3.Value);
        Assert.IsTrue(event3.InSuccessfulContractCall);
        Assert.AreEqual(6U, event3.TransactionIndex);
        Assert.AreEqual(5U, event3.OperationIndex);
        Assert.AreEqual("944d99fd572b8541f5c0ce95881b844d79144fe9a5cef2aea1cf94fb7c91f1f7", event3.TransactionHash);

        var event4 = eventsResponse.Events[3];
        Assert.IsNotNull(event4);
        Assert.AreEqual("contract", event4.Type);
        Assert.AreEqual(913037, event4.Ledger);
        Assert.AreEqual(new DateTimeOffset(2024, 8, 6, 10, 38, 23, TimeSpan.Zero), event4.LedgerClosedAt);
        Assert.AreEqual("CDV6IIE2DFFGB3GKAG7YYSKBO4PFDAZ76GRHXKSOBBIG64NNHMMRCRXH", event4.ContractId);
        Assert.AreEqual("0003921464055050240-0000000001", event4.Id);
        Assert.AreEqual(3, event4.Topics.Length);
        Assert.AreEqual("AAAADwAAAAdhcHByb3ZlAA==", event4.Topics[0]);
        Assert.AreEqual("AAAAEgAAAAAAAAAAWoM+w+i/0MLTSydKU896zcL4/EhYLVMHlSxIzY+ucJs=", event4.Topics[1]);
        Assert.AreEqual("AAAAEgAAAAGPj5dVA0lfIe7VhCeJwcQ68vhwUcgQjj7XNcpMoUn7Hw==", event4.Topics[2]);
        Assert.AreEqual("AAAAEAAAAAEAAAACAAAACgAAAAAAAAAAAAQdH3d3yKkAAAADABD7yA==", event4.Value);
        Assert.IsTrue(event4.InSuccessfulContractCall);
        Assert.AreEqual(8U, event4.TransactionIndex);
        Assert.AreEqual(7U, event4.OperationIndex);

        Assert.AreEqual("64bd9d003dbc4c9f766206dee34d57285322eeee6c5acb6d2a31d2668d88c2fd", event4.TransactionHash);
    }

    [TestMethod]
    public async Task TestGetAccountNotFound()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "entries": [],
                "latestLedger": 458234
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        await Assert.ThrowsExceptionAsync<AccountNotFoundException>(() =>
            sorobanServer.GetAccount("GDPNJ4YFMQYSNWMSF6XZEDXS4M4ECTQHMVXBISQA4U7DEHRGUY3EGDSB"));
    }

    [TestMethod]
    public async Task TestGetAccount()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "entries": [
                  {
                    "key": "AAAAAAAAAADe1PMFZDEm2ZIvr5IO8uM4QU4HZW4USgDlPjIeJqY2Qw==",
                    "xdr": "AAAAAAAAAADe1PMFZDEm2ZIvr5IO8uM4QU4HZW4USgDlPjIeJqY2QwAAABc7WfOIAABhSwAAAAwAAAAGAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAO5rKAAAAAAIAAAAAAAAAAQAAAAAAAAADAAAAAAAG/PgAAAAAZw5yKA==",
                    "lastModifiedLedgerSeq": 457976
                  }
                ],
                "latestLedger": 458186
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        var account = await sorobanServer.GetAccount("GDPNJ4YFMQYSNWMSF6XZEDXS4M4ECTQHMVXBISQA4U7DEHRGUY3EGDSB");
        Assert.AreEqual("GDPNJ4YFMQYSNWMSF6XZEDXS4M4ECTQHMVXBISQA4U7DEHRGUY3EGDSB", account.AccountId);
        Assert.AreEqual(106974750441484L, account.SequenceNumber);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeContractData()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "entries": [
                  {
                    "key": "AAAABgAAAAHlSOklVKjopXQGiolb7/AWOctPGrwMQz83BSx22ZdhWQAAABQAAAAB",
                    "xdr": "AAAABgAAAAAAAAAB5UjpJVSo6KV0BoqJW+/wFjnLTxq8DEM/NwUsdtmXYVkAAAAUAAAAAQAAABMAAAAAwaZQUG98IMj00Wquc/iU8wLNAR1+8zre9XLyCzT3ZT4AAAAA",
                    "lastModifiedLedgerSeq": 458285,
                    "liveUntilLedgerSeq": 2531884
                  }
                ],
                "latestLedger": 458386
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);

        var response = await sorobanServer.GetLedgerEntries([]);

        Assert.AreEqual(458386U, response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntry = response.LedgerEntries[0] as LedgerEntryContractData;
        var ledgerKey = response.LedgerKeys[0] as LedgerKeyContractData;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);
        Assert.AreEqual(458285U, ledgerEntry.LastModifiedLedgerSeq);
        Assert.AreEqual(2531884U, ledgerEntry.LiveUntilLedger);

        Assert.IsInstanceOfType(ledgerEntry.Key, typeof(SCLedgerKeyContractInstance));
        Assert.AreEqual("CDSUR2JFKSUORJLUA2FISW7P6ALDTS2PDK6AYQZ7G4CSY5WZS5QVSM47",
            ((ScContractId)ledgerKey.Contract).InnerValue);
        Assert.AreEqual("CDSUR2JFKSUORJLUA2FISW7P6ALDTS2PDK6AYQZ7G4CSY5WZS5QVSM47",
            ((ScContractId)ledgerEntry.Contract).InnerValue);
        Assert.IsNull(ledgerEntry.LedgerExtensionV1);
        Assert.AreEqual(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT, ledgerKey.Durability.InnerValue);
        Assert.AreEqual(ContractDataDurability.ContractDataDurabilityEnum.PERSISTENT,
            ledgerEntry.Durability.InnerValue);
        Assert.IsInstanceOfType(ledgerEntry.Value, typeof(SCContractInstance));
        var ledgerValue = (SCContractInstance)ledgerEntry.Value;
        Assert.IsInstanceOfType(ledgerValue.Executable, typeof(ContractExecutableWasm));
        var ledgerExecutable = (ContractExecutableWasm)ledgerValue.Executable;
        Assert.AreEqual(HelloContractWasmHash.ToLower(), ledgerExecutable.WasmHash.ToLower());
        Assert.IsNull(ledgerValue.Storage);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeContractCode()
    {
        const string getLedgerEntriesResponseJson =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "entries": [
                  {
                    "key": "AAAAB8GmUFBvfCDI9NFqrnP4lPMCzQEdfvM63vVy8gs092U+",
                    "xdr": "AAAABwAAAAEAAAAAAAAAAAAAAF8AAAACAAAAAwAAAAAAAAADAAAAAAAAAAAAAAABAAAABQAAAADBplBQb3wgyPTRaq5z+JTzAs0BHX7zOt71cvILNPdlPgAAAhsAYXNtAQAAAAEPA2ACfn4BfmABfgF+YAAAAgcBAXYBZwAAAwMCAQIFAwEAEAYZA38BQYCAwAALfwBBgIDAAAt/AEGAgMAACwcxBQZtZW1vcnkCAAVoZWxsbwABAV8AAgpfX2RhdGFfZW5kAwELX19oZWFwX2Jhc2UDAgrIAQLCAQECfyOAgICAAEEgayIBJICAgIAAAkACQCAAp0H/AXEiAkEORg0AIAJBygBHDQELIAEgADcDCCABQo7o8di6AjcDAEEAIQIDQAJAIAJBEEcNAEEAIQICQANAIAJBEEYNASABQRBqIAJqIAEgAmopAwA3AwAgAkEIaiECDAALCyABQRBqrUIghkIEhEKEgICAIBCAgICAACEAIAFBIGokgICAgAAgAA8LIAFBEGogAmpCAjcDACACQQhqIQIMAAsLAAALAgALAEMOY29udHJhY3RzcGVjdjAAAAAAAAAAAAAAAAVoZWxsbwAAAAAAAAEAAAAAAAAAAnRvAAAAAAARAAAAAQAAA+oAAAARAB4RY29udHJhY3RlbnZtZXRhdjAAAAAAAAAAFAAAAAAAbw5jb250cmFjdG1ldGF2MAAAAAAAAAAFcnN2ZXIAAAAAAAAGMS43NC4xAAAAAAAAAAAACHJzc2RrdmVyAAAALzIwLjAuMCM4MjJjZTZjYzNlNDYxY2NjOTI1Mjc1YjQ3MmQ3N2I2Y2EzNWIyY2Q5AAA=",
                    "lastModifiedLedgerSeq": 274826,
                    "liveUntilLedgerSeq": 2348425
                  }
                ],
                "latestLedger": 454092
              }
            }
            """;

        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(getLedgerEntriesResponseJson);
        var response = await sorobanServer.GetLedgerEntries([]);

        Assert.AreEqual(454092U, response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntry = response.LedgerEntries[0] as LedgerEntryContractCode;
        var ledgerKey = response.LedgerKeys[0] as LedgerKeyContractCode;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);
        Assert.AreEqual(HelloContractWasmHash.ToLower(), Convert.ToHexString(ledgerKey.Hash).ToLower());

        Assert.AreEqual(2348425U, ledgerEntry.LiveUntilLedger);
        Assert.AreEqual(274826U, ledgerEntry.LastModifiedLedgerSeq);
        Assert.AreEqual(HelloContractWasmHash.ToLower(), Convert.ToHexString(ledgerEntry.Hash).ToLower());
        Assert.AreEqual(
            "0061736d01000000010f0360027e7e017e60017e017e600000020701017601670000030302010205030100100619037f01418080c0000b7f00418080c0000b7f00418080c0000b073105066d656d6f727902000568656c6c6f0001015f00020a5f5f646174615f656e6403010b5f5f686561705f6261736503020ac80102c20101027f23808080800041206b2201248080808000024002402000a741ff01712202410e460d00200241ca00470d010b200120003703082001428ee8f1d8ba02370300410021020340024020024110470d00410021020240034020024110460d01200141106a20026a200120026a290300370300200241086a21020c000b0b200141106aad4220864204844284808080201080808080002100200141206a24808080800020000f0b200141106a20026a4202370300200241086a21020c000b0b00000b02000b00430e636f6e747261637473706563763000000000000000000000000568656c6c6f000000000000010000000000000002746f00000000001100000001000003ea00000011001e11636f6e7472616374656e766d6574617630000000000000001400000000006f0e636f6e74726163746d65746176300000000000000005727376657200000000000006312e37342e3100000000000000000008727373646b7665720000002f32302e302e30233832326365366363336534363163636339323532373562343732643737623663613335623263643900",
            Convert.ToHexString(ledgerEntry.Code).ToLower());

        var ext = ledgerEntry.ContractCodeExtensionV1;
        Assert.IsNotNull(ext);

        var costInputs = ext.CostInputs;
        Assert.IsNotNull(costInputs);
        Assert.IsInstanceOfType(costInputs.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(95U, costInputs.NInstructions);
        Assert.AreEqual(2U, costInputs.NFunctions);
        Assert.AreEqual(3U, costInputs.NGlobals);
        Assert.AreEqual(0U, costInputs.NTableEntries);
        Assert.AreEqual(3U, costInputs.NTypes);
        Assert.AreEqual(0U, costInputs.NDataSegments);
        Assert.AreEqual(0U, costInputs.NElemSegments);
        Assert.AreEqual(1U, costInputs.NImports);
        Assert.AreEqual(5U, costInputs.NExports);
        Assert.AreEqual(0U, costInputs.NDataSegmentBytes);
        Assert.IsNull(ledgerEntry.LedgerExtensionV1);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeAccount()
    {
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
        var response = await sorobanServer.GetLedgerEntries([]);

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

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeData()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "entries": [
                  {
                    "key": "AAAAAwAAAADe1PMFZDEm2ZIvr5IO8uM4QU4HZW4USgDlPjIeJqY2QwAAAARUZXN0",
                    "xdr": "AAAAAwAAAADe1PMFZDEm2ZIvr5IO8uM4QU4HZW4USgDlPjIeJqY2QwAAAARUZXN0AAAABEhvaG8AAAAA",
                    "lastModifiedLedgerSeq": 457882
                  }
                ],
                "latestLedger": 457887
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);

        var response = await sorobanServer.GetLedgerEntries([]);

        Assert.AreEqual(457887U, response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntry = response.LedgerEntries[0] as LedgerEntryData;
        var ledgerKey = response.LedgerKeys[0] as LedgerKeyData;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);
        Assert.AreEqual(457882U, ledgerEntry.LastModifiedLedgerSeq);
        Assert.AreEqual("Test", ledgerKey.DataName);
        Assert.AreEqual("GDPNJ4YFMQYSNWMSF6XZEDXS4M4ECTQHMVXBISQA4U7DEHRGUY3EGDSB", ledgerKey.Account.AccountId);

        Assert.AreEqual("GDPNJ4YFMQYSNWMSF6XZEDXS4M4ECTQHMVXBISQA4U7DEHRGUY3EGDSB", ledgerEntry.Account.AccountId);
        Assert.AreEqual("Test", ledgerEntry.DataName);
        Assert.AreEqual("Hoho", Encoding.UTF8.GetString(ledgerEntry.DataValue));
        Assert.IsNull(ledgerEntry.LedgerExtensionV1);
        Assert.IsNull(ledgerEntry.DataExtension);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeOffer()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "entries": [
                  {
                    "key": "AAAAAgAAAADe1PMFZDEm2ZIvr5IO8uM4QU4HZW4USgDlPjIeJqY2QwAAAAAAABxc",
                    "xdr": "AAAAAgAAAADe1PMFZDEm2ZIvr5IO8uM4QU4HZW4USgDlPjIeJqY2QwAAAAAAABxcAAAAAAAAAAFFVVJDAAAAAMIyI22v9Xk6N/jbhhoxbcFy+1F8zGqS7NRXdFEUp0bcAAAAADuaygAAAAADAAAAAgAAAAAAAAAA",
                    "lastModifiedLedgerSeq": 457704
                  }
                ],
                "latestLedger": 457750
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);

        var response = await sorobanServer.GetLedgerEntries([]);

        Assert.AreEqual(457750U, response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntry = response.LedgerEntries[0] as LedgerEntryOffer;
        var ledgerKey = response.LedgerKeys[0] as LedgerKeyOffer;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);
        Assert.AreEqual(457704U, ledgerEntry.LastModifiedLedgerSeq);

        Assert.AreEqual(7260L, ledgerKey.OfferId);
        Assert.AreEqual(1000000000L, ledgerEntry.Amount);
        Assert.IsInstanceOfType(ledgerEntry.Buying, typeof(AssetTypeCreditAlphaNum4));
        Assert.IsInstanceOfType(ledgerEntry.Selling, typeof(AssetTypeNative));
        var buyingAsset = (AssetTypeCreditAlphaNum4)ledgerEntry.Buying;
        Assert.AreEqual("EURC", buyingAsset.Code);
        Assert.AreEqual("GDBDEI3NV72XSORX7DNYMGRRNXAXF62RPTGGVEXM2RLXIUIUU5DNZWWH", buyingAsset.Issuer);
        Assert.AreEqual("GDPNJ4YFMQYSNWMSF6XZEDXS4M4ECTQHMVXBISQA4U7DEHRGUY3EGDSB", ledgerEntry.SellerId.AccountId);
        Assert.AreEqual(Price.FromString("1.5"), ledgerEntry.Price);
        Assert.AreEqual(0U, ledgerEntry.Flags);
        Assert.IsNull(ledgerEntry.LedgerExtensionV1);
        Assert.IsNull(ledgerEntry.OfferExtension);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeClaimableBalance()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "entries": [
                  {
                    "key": "AAAABAAAAAApmjIQYjjzsthNQUJ4P+MgJTvNp3XRv7eszbUzAh3czw==",
                    "xdr": "AAAABAAAAAApmjIQYjjzsthNQUJ4P+MgJTvNp3XRv7eszbUzAh3czwAAAAEAAAAAAAAAALtJgdGXASRLp/M5ZpckEa10nJPtYvrgX6M5wTPacDUYAAAAAAAAAAAAAAAAC+vCAAAAAAA=",
                    "lastModifiedLedgerSeq": 457593
                  }
                ],
                "latestLedger": 457624
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);

        var response = await sorobanServer.GetLedgerEntries([]);

        Assert.AreEqual(457624U, response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntry = response.LedgerEntries[0] as LedgerEntryClaimableBalance;
        var ledgerKey = response.LedgerKeys[0] as LedgerKeyClaimableBalance;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);
        Assert.AreEqual("00000000299A32106238F3B2D84D4142783FE320253BCDA775D1BFB7ACCDB533021DDCCF",
            ledgerKey.BalanceId.ToUpper());
        Assert.AreEqual(457593U, ledgerEntry.LastModifiedLedgerSeq);
        Assert.AreEqual("native", ledgerEntry.Asset.Type);
        Assert.AreEqual(200000000L, ledgerEntry.Amount);
        Assert.IsNull(ledgerEntry.ClaimableBalanceEntryExtensionV1);
        Assert.AreEqual(1, ledgerEntry.Claimants.Length);
        var claimant = ledgerEntry.Claimants[0];
        Assert.AreEqual("GC5UTAORS4ASIS5H6M4WNFZECGWXJHET5VRPVYC7UM44CM62OA2RQEPS", claimant.Destination.AccountId);
        Assert.IsInstanceOfType(claimant.Predicate, typeof(ClaimPredicateUnconditional));
    }

    // TODO Test liquidity pool with some deposits/withdrawals
    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeLiquidityPool()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "entries": [
                  {
                    "key": "AAAABf1JjDlZINVxVuhrLVKs0ru/wWSDaiBMc9O2S+wzh004",
                    "xdr": "AAAABf1JjDlZINVxVuhrLVKs0ru/wWSDaiBMc9O2S+wzh004AAAAAAAAAAAAAAABRVVSQwAAAADCMiNtr/V5Ojf424YaMW3BcvtRfMxqkuzUV3RRFKdG3AAAAB4AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAg==",
                    "lastModifiedLedgerSeq": 457976
                  }
                ],
                "latestLedger": 457992
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);

        var response = await sorobanServer.GetLedgerEntries([]);

        Assert.AreEqual(457992U, response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntry = response.LedgerEntries[0] as LedgerEntryLiquidityPool;
        var ledgerKey = response.LedgerKeys[0] as LedgerKeyLiquidityPool;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);
        Assert.AreEqual(457976U, ledgerEntry.LastModifiedLedgerSeq);
        Assert.AreEqual("fd498c395920d57156e86b2d52acd2bbbfc164836a204c73d3b64bec33874d38",
            ledgerKey.LiquidityPoolId.ToString());
        Assert.AreEqual("fd498c395920d57156e86b2d52acd2bbbfc164836a204c73d3b64bec33874d38",
            ledgerEntry.LiquidityPoolId.ToString());
        Assert.IsInstanceOfType(ledgerEntry.LiquidityPoolBody, typeof(LiquidityPoolConstantProduct));
        var constantProduct = (LiquidityPoolConstantProduct)ledgerEntry.LiquidityPoolBody;
        Assert.AreEqual(2, constantProduct.PoolSharesTrustLineCount);
        Assert.AreEqual(0L, constantProduct.ReserveA);
        Assert.AreEqual(0L, constantProduct.ReserveB);
        Assert.AreEqual(0L, constantProduct.TotalPoolShares);
        var parameters = constantProduct.Parameters;
        Assert.AreEqual(30, parameters.Fee);
        Assert.IsInstanceOfType(parameters.AssetA, typeof(AssetTypeNative));
        Assert.IsInstanceOfType(parameters.AssetB, typeof(AssetTypeCreditAlphaNum4));
        Assert.AreEqual("EURC", ((AssetTypeCreditAlphaNum4)parameters.AssetB).Code);
        Assert.AreEqual("GDBDEI3NV72XSORX7DNYMGRRNXAXF62RPTGGVEXM2RLXIUIUU5DNZWWH",
            ((AssetTypeCreditAlphaNum4)parameters.AssetB).Issuer);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeTrustline()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "entries": [
                  {
                    "key": "AAAAAQAAAADe1PMFZDEm2ZIvr5IO8uM4QU4HZW4USgDlPjIeJqY2QwAAAAFFVVJDAAAAAMIyI22v9Xk6N/jbhhoxbcFy+1F8zGqS7NRXdFEUp0bc",
                    "xdr": "AAAAAQAAAADe1PMFZDEm2ZIvr5IO8uM4QU4HZW4USgDlPjIeJqY2QwAAAAFFVVJDAAAAAMIyI22v9Xk6N/jbhhoxbcFy+1F8zGqS7NRXdFEUp0bcAAAAAlEQ84B//////////wAAAAEAAAAA",
                    "lastModifiedLedgerSeq": 139305
                  }
                ],
                "latestLedger": 457171
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);

        var response = await sorobanServer.GetLedgerEntries([]);

        Assert.AreEqual(457171U, response.LatestLedger);
        Assert.IsNotNull(response.LedgerEntries);
        Assert.IsNotNull(response.LedgerKeys);
        Assert.AreEqual(1, response.LedgerEntries.Length);
        Assert.AreEqual(1, response.LedgerKeys.Length);
        var ledgerEntry = response.LedgerEntries[0] as LedgerEntryTrustline;
        var ledgerKey = response.LedgerKeys[0] as LedgerKeyTrustline;
        Assert.IsNotNull(ledgerEntry);
        Assert.IsNotNull(ledgerKey);
        Assert.AreEqual(139305U, ledgerEntry.LastModifiedLedgerSeq);
        Assert.AreEqual(9950000000L, ledgerEntry.Balance);

        var asset = (AssetTypeCreditAlphaNum4)((TrustlineAsset.Wrapper)ledgerEntry.Asset).Asset;
        Assert.AreEqual("GDBDEI3NV72XSORX7DNYMGRRNXAXF62RPTGGVEXM2RLXIUIUU5DNZWWH", asset.Issuer);
        Assert.AreEqual("EURC", asset.Code);
        Assert.AreEqual("GDPNJ4YFMQYSNWMSF6XZEDXS4M4ECTQHMVXBISQA4U7DEHRGUY3EGDSB", ledgerEntry.Account.AccountId);
        Assert.AreEqual(1U, ledgerEntry.Flags);
        Assert.AreEqual(long.MaxValue, ledgerEntry.Limit);
        Assert.IsNull(ledgerEntry.TrustlineExtensionV1);
    }

    [TestMethod]
    public async Task TestGetTransactionNotFound()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "status": "NOT_FOUND",
                "latestLedger": 456220,
                "latestLedgerCloseTime": "1728990739",
                "oldestLedger": 438941,
                "oldestLedgerCloseTime": "1728899977"
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        var response = await sorobanServer.GetTransaction("");
        Assert.AreEqual(TransactionInfo.TransactionStatus.NOT_FOUND, response.Status);
        Assert.AreEqual(456220L, response.LatestLedger);
        Assert.AreEqual(438941L, response.OldestLedger);
        Assert.AreEqual(1728990739L, response.LatestLedgerCloseTime);
        Assert.AreEqual(1728899977, response.OldestLedgerCloseTime);
    }

    [TestMethod]
    public async Task TestSimulateTransactionSuccess()
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

        var response = await sorobanServer.SimulateTransaction(
            CreateDummyTransaction(false),
            null,
            AuthMode.RECORD_ALLOW_NONROOT
        );

        Assert.IsNotNull(response);

        // SorobanTransactionData
        var sorobanData = response.SorobanTransactionData;

        Assert.IsNotNull(sorobanData);
        Assert.IsNull(sorobanData.Extension);
        Assert.AreEqual(2L, sorobanData.ResourceFee);
        var sorobanResources = sorobanData.Resources;
        Assert.IsNotNull(sorobanResources);
        Assert.AreEqual(1976262U, sorobanResources.Instructions);
        Assert.AreEqual(1428U, sorobanResources.DiskReadBytes);
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
                Contract: ScContractId contractId,
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

        if (readWrite is LedgerKeyContractData { Contract: ScContractId contractId1, Key: SCVec vec } contractData1)
        {
            Assert.AreEqual("CDU3PZ4LXVETIFVLS33RDXLD63JZ5GXS7PCV2DJ7BBT6EBPA2AB7YR5H", contractId1.InnerValue);
            Assert.AreEqual(2, vec.InnerValue.Length);
            Assert.IsTrue(vec.InnerValue[0] is SCSymbol { InnerValue: "Counter" });
            Assert.IsTrue(vec.InnerValue[1] is ScAccountId
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
    // This test only focuses on testing the SorobanTransactionDataExt
    public async Task TestSimulateTransactionProtocol23Success()
    {
        const string json =
            """
            {
                "jsonrpc": "2.0",
                "id": "7a469b9d6ed4444893491be530862ce3",
                "result": {
                    "transactionData": "AAAAAQAAAAIAAACGAAACKgAAAAIAAAAGAAAAAem354u9STQWq5b3Ed1j9tOemvL7xV0NPwhn4gXg0AP8AAAAFAAAAAEAAAAH8dTe2OoI0BnhlDbH0fWvXmvprkBvBAgKIcL9busuuMEAAAABAAAABgAAAAHpt+eLvUk0FquW9xHdY/bTnpry+8VdDT8IZ+IF4NAD/AAAABAAAAABAAAAAgAAAA8AAAAHQ291bnRlcgAAAAASAAAAAAAAAABYt8SiyPKXqo89JHEoH9/M7K/kjlZjMT7BjhKnPsqYoQAAAAEAHifGAAAFlAAABZQAAAAAAAAAbw==",
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

        var response = await sorobanServer.SimulateTransaction(
            CreateDummyTransaction(false)
        );

        Assert.IsNotNull(response);

        // SorobanTransactionData
        var sorobanData = response.SorobanTransactionData;

        Assert.IsNotNull(sorobanData);

        var extension = sorobanData.Extension;
        Assert.IsNotNull(extension);
        Assert.AreEqual(2, extension.ArchivedSorobanEntries.Length);
        Assert.AreEqual(134U, extension.ArchivedSorobanEntries[0]);
        Assert.AreEqual(554U, extension.ArchivedSorobanEntries[1]);
    }

    [TestMethod]
    public async Task TestGetFeeStats()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
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
        Assert.AreEqual("195", inclusionFee.P95);
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
        Assert.AreEqual("290", sorobanInclusionFee.P95);
        Assert.AreEqual("299", sorobanInclusionFee.P99);
        Assert.AreEqual("7", sorobanInclusionFee.TransactionCount);
        Assert.AreEqual(10, sorobanInclusionFee.LedgerCount);
        Assert.IsNotNull(response.SorobanInclusionFee);
        Assert.AreEqual(4519945L, response.LatestLedger);
    }

    [TestMethod]
    public async Task TestGetVersionInfoProtocol22()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                  "version": "22.1.0-c7e9737b54913f5a33f29decdb0c9f6101dc1cfc",
                  "commitHash": "c7e9737b54913f5a33f29decdb0c9f6101dc1cfc",
                  "buildTimestamp": "2024-11-13T21:20:45",
                  "captiveCoreVersion": "stellar-core 22.0.0 (721fd0a654d5e82d38c748a91053e530a475193d)",
                  "protocolVersion": 22,
                  "commit_hash": "c7e9737b54913f5a33f29decdb0c9f6101dc1cfc",
                  "build_time_stamp": "2024-11-13T21:20:45",
                  "captive_core_version": "stellar-core 22.0.0 (721fd0a654d5e82d38c748a91053e530a475193d)",
                  "protocol_version": 22
                }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        var response = await sorobanServer.GetVersionInfo();

        Assert.IsNotNull(response);
        Assert.AreEqual("22.1.0-c7e9737b54913f5a33f29decdb0c9f6101dc1cfc", response.Version);
        Assert.AreEqual("c7e9737b54913f5a33f29decdb0c9f6101dc1cfc", response.CommitHash);
        Assert.AreEqual("2024-11-13T21:20:45", response.BuildTimeStamp);
        Assert.AreEqual("stellar-core 22.0.0 (721fd0a654d5e82d38c748a91053e530a475193d)",
            response.CaptiveCoreVersion);
        Assert.AreEqual(22, response.ProtocolVersion);
    }

    [TestMethod]
    public async Task TestGetVersionInfoPriorToProtocol22()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
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
    public async Task TestGetTransactionSuccess()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "status": "SUCCESS",
                "txHash": "89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db0",
                "latestLedger": 2540076,
                "latestLedgerCloseTime": "1700086333",
                "oldestLedger": 2538637,
                "oldestLedgerCloseTime": "1700078796",
                "applicationOrder": 1,
                "envelopeXdr": "AAAAAgAAAADGFY14/R1KD0VGtTbi5Yp4d7LuMW0iQbLM/AUiGKj5owCpsoQAJY3OAAAjqgAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAGAAAAAAAAAABhhOwI+RL18Zpk7cqI5pRRf0L96jE8i+0x3ekhuBh2cUAAAARc2V0X2N1cnJlbmN5X3JhdGUAAAAAAAACAAAADwAAAANldXIAAAAACQAAAAAAAAAAAAAAAAARCz4AAAABAAAAAAAAAAAAAAABhhOwI+RL18Zpk7cqI5pRRf0L96jE8i+0x3ekhuBh2cUAAAARc2V0X2N1cnJlbmN5X3JhdGUAAAAAAAACAAAADwAAAANldXIAAAAACQAAAAAAAAAAAAAAAAARCz4AAAAAAAAAAQAAAAAAAAABAAAAB4408vVXuLU3mry897TfPpYjjsSN7n42REos241RddYdAAAAAQAAAAYAAAABhhOwI+RL18Zpk7cqI5pRRf0L96jE8i+0x3ekhuBh2cUAAAAUAAAAAQFvcYAAAImAAAAHxAAAAAAAAAACAAAAARio+aMAAABATbFMyom/TUz87wHex0LoYZA8jbNJkXbaDSgmOdk+wSBFJuMuta+/vSlro0e0vK2+1FqD/zWHZeYig4pKmM3rDA==",
                "resultXdr": "AAAAAAARFy8AAAAAAAAAAQAAAAAAAAAYAAAAAMu8SHUN67hTUJOz3q+IrH9M/4dCVXaljeK6x1Ss20YWAAAAAA==",
                "resultMetaXdr": "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",
                "ledger": 2540064,
                "createdAt": 1700086268,
                "events": {
                    "diagnosticEventsXdr": [
                        "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAACGNwdV9pbnNuAAAABQAAAAAAM2BC",
                        "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAD21heF9yd19rZXlfYnl0ZQAAAAAFAAAAAAAAAFQ=",
                        "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEG1heF9yd19jb2RlX2J5dGUAAAAFAAAAAAAAQEw="
                    ],
                    "transactionEventsXdr": [
                        "AAAAAAAAAAAAAAAB15KLcsJwPM/q9+uf9O9NUEpVqLl5/JtFDqLIQrTRzmEAAAABAAAAAAAAAAIAAAAPAAAAA2ZlZQAAAAASAAAAAAAAAADte5nJrehJq/pu3qlV/bASRSOiJVXdNC+gQW/nxVNWuQAAAAoAAAAAAAAAAAAAAAAAWOuO",
                        "AAAAAQAAAAAAAAAB15KLcsJwPM/q9+uf9O9NUEpVqLl5/JtFDqLIQrTRzmEAAAABAAAAAAAAAAIAAAAPAAAAA2ZlZQAAAAASAAAAAAAAAADte5nJrehJq/pu3qlV/bASRSOiJVXdNC+gQW/nxVNWuQAAAAr/////////////////8/qT"
                    ],
                    "contractEventsXdr": [
                        [
                            "AAAAAAAAAAHf65G24dyt1q+Xu3xFX5fzdHcKf3j2lXO5n11b+EnOfAAAAAEAAAAAAAAAAQAAAA8AAAAEaW5pdAAAABAAAAABAAAABQAAAAUAAAAAAAaRmQAAABIAAAAAAAAAACcMY2GvjF3igK326WyiU8hv107p9YxvAS29gt1fml2WAAAAEgAAAAAAAAAAyewwXk7lqpxiQNYP3VlZ1EEprNK+dSBV4KQ9iluwbx8AAAASAAAAAAAAAAAY2Rm1IXXndEI0rYg2bt1/rw2mi1SYOUT2qeKPvf56cgAAABIAAAABusKzizgXRsUWKJQRrpWHAWG/yujQ6LBT/pMDljEiAeg="
                        ]
                    ]
                }
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        var response = await sorobanServer.GetTransaction("");

        Assert.IsNotNull(response);
        Assert.AreEqual(TransactionInfo.TransactionStatus.SUCCESS, response.Status);
        Assert.AreEqual("89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db0", response.TxHash);
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
        Assert.AreEqual(1700086268, response.CreatedAt);

        var meta = response.TransactionMeta;
        Assert.IsNotNull(meta);
        Assert.IsInstanceOfType(meta, typeof(TransactionMetaV3));

        #region Events

        var events = response.Events;
        Assert.IsNotNull(events);
        var transactionEventsXdr = events.TransactionEventsXdr;
        Assert.IsNotNull(transactionEventsXdr);
        Assert.AreEqual(2, transactionEventsXdr.Length);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAB15KLcsJwPM/q9+uf9O9NUEpVqLl5/JtFDqLIQrTRzmEAAAABAAAAAAAAAAIAAAAPAAAAA2ZlZQAAAAASAAAAAAAAAADte5nJrehJq/pu3qlV/bASRSOiJVXdNC+gQW/nxVNWuQAAAAoAAAAAAAAAAAAAAAAAWOuO",
            transactionEventsXdr[0]);
        Assert.AreEqual(
            "AAAAAQAAAAAAAAAB15KLcsJwPM/q9+uf9O9NUEpVqLl5/JtFDqLIQrTRzmEAAAABAAAAAAAAAAIAAAAPAAAAA2ZlZQAAAAASAAAAAAAAAADte5nJrehJq/pu3qlV/bASRSOiJVXdNC+gQW/nxVNWuQAAAAr/////////////////8/qT",
            transactionEventsXdr[1]);

        var diagnosticEventsXdr = events.DiagnosticEventsXdr;
        Assert.IsNotNull(diagnosticEventsXdr);
        Assert.AreEqual(3, diagnosticEventsXdr.Length);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAACGNwdV9pbnNuAAAABQAAAAAAM2BC",
            diagnosticEventsXdr[0]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAD21heF9yd19rZXlfYnl0ZQAAAAAFAAAAAAAAAFQ=",
            diagnosticEventsXdr[1]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEG1heF9yd19jb2RlX2J5dGUAAAAFAAAAAAAAQEw=",
            diagnosticEventsXdr[2]);

        var contractEventsXdr = events.ContractEventsXdr;
        Assert.IsNotNull(contractEventsXdr);
        Assert.AreEqual(1, contractEventsXdr.Length);
        Assert.AreEqual(1, contractEventsXdr[0].Length);
        Assert.AreEqual(
            "AAAAAAAAAAHf65G24dyt1q+Xu3xFX5fzdHcKf3j2lXO5n11b+EnOfAAAAAEAAAAAAAAAAQAAAA8AAAAEaW5pdAAAABAAAAABAAAABQAAAAUAAAAAAAaRmQAAABIAAAAAAAAAACcMY2GvjF3igK326WyiU8hv107p9YxvAS29gt1fml2WAAAAEgAAAAAAAAAAyewwXk7lqpxiQNYP3VlZ1EEprNK+dSBV4KQ9iluwbx8AAAASAAAAAAAAAAAY2Rm1IXXndEI0rYg2bt1/rw2mi1SYOUT2qeKPvf56cgAAABIAAAABusKzizgXRsUWKJQRrpWHAWG/yujQ6LBT/pMDljEiAeg=",
            contractEventsXdr[0][0]);

        #endregion
    }

    [TestMethod]
    public async Task TestGetTransactions()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "8675309",
              "result": {
                "transactions": [
                  {
                    "status": "FAILED",
                    "txHash": "89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db1",
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
                    "txHash": "89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db2",
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
                    "txHash": "89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db3",
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
                    "txHash": "89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db4",
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
                    "createdAt": 1717166042,
                    "events": {
                        "diagnosticEventsXdr": [
                            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEG1heF9yd19kYXRhX2J5dGUAAAAFAAAAAAAAAbg=",
                            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEG1heF9yd19jb2RlX2J5dGUAAAAFAAAAAAAAQEw=",
                            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAE21heF9lbWl0X2V2ZW50X2J5dGUAAAAABQAAAAAAAAEE"
                        ],
                        "transactionEventsXdr": [
                            "AAAAAAAAAAAAAAAB15KLcsJwPM/q9+uf9O9NUEpVqLl5/JtFDqLIQrTRzmEAAAABAAAAAAAAAAIAAAAPAAAAA2ZlZQAAAAASAAAAAAAAAADte5nJrehJq/pu3qlV/bASRSOiJVXdNC+gQW/nxVNWuQAAAAoAAAAAAAAAAAAAAAAAWOuO",
                            "AAAAAQAAAAAAAAAB15KLcsJwPM/q9+uf9O9NUEpVqLl5/JtFDqLIQrTRzmEAAAABAAAAAAAAAAIAAAAPAAAAA2ZlZQAAAAASAAAAAAAAAADte5nJrehJq/pu3qlV/bASRSOiJVXdNC+gQW/nxVNWuQAAAAr/////////////////8/qT"
                        ],
                        "contractEventsXdr": [
                            [
                                "AAAAAAAAAAHf65G24dyt1q+Xu3xFX5fzdHcKf3j2lXO5n11b+EnOfAAAAAEAAAAAAAAAAQAAAA8AAAAEaW5pdAAAABAAAAABAAAABQAAAAUAAAAAAAaRmQAAABIAAAAAAAAAACcMY2GvjF3igK326WyiU8hv107p9YxvAS29gt1fml2WAAAAEgAAAAAAAAAAyewwXk7lqpxiQNYP3VlZ1EEprNK+dSBV4KQ9iluwbx8AAAASAAAAAAAAAAAY2Rm1IXXndEI0rYg2bt1/rw2mi1SYOUT2qeKPvf56cgAAABIAAAABusKzizgXRsUWKJQRrpWHAWG/yujQ6LBT/pMDljEiAeg="
                            ]
                        ]
                    }
                  },
                  {
                    "status": "FAILED",
                    "txHash": "89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db5",
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
        var response = await sorobanServer.GetTransactions(new GetTransactionsRequest());

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
        Assert.AreEqual("89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db1", tx1.TxHash);
        Assert.AreEqual(1888539L, tx1.Ledger);
        Assert.AreEqual(1717166042, tx1.CreatedAt);
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
        Assert.AreEqual("89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db2", tx2.TxHash);
        Assert.AreEqual(1888539L, tx2.Ledger);
        Assert.AreEqual(1717166042, tx2.CreatedAt);
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
        Assert.AreEqual("89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db3", tx3.TxHash);
        Assert.AreEqual(1888539L, tx3.Ledger);
        Assert.AreEqual(1717166042, tx3.CreatedAt);
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
        Assert.AreEqual("89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db4", tx4.TxHash);
        Assert.AreEqual(1888539L, tx4.Ledger);
        Assert.AreEqual(1717166042, tx4.CreatedAt);
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

        #region Tx4DiagnosticEvents

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

        #endregion

        #region Tx4Events

        var tx4Events = tx4.Events;
        Assert.IsNotNull(tx4Events);
        var tx4TransactionEvents = tx4Events.TransactionEventsXdr;
        Assert.IsNotNull(tx4TransactionEvents);
        Assert.AreEqual(2, tx4TransactionEvents.Length);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAB15KLcsJwPM/q9+uf9O9NUEpVqLl5/JtFDqLIQrTRzmEAAAABAAAAAAAAAAIAAAAPAAAAA2ZlZQAAAAASAAAAAAAAAADte5nJrehJq/pu3qlV/bASRSOiJVXdNC+gQW/nxVNWuQAAAAoAAAAAAAAAAAAAAAAAWOuO",
            tx4TransactionEvents[0]);
        Assert.AreEqual(
            "AAAAAQAAAAAAAAAB15KLcsJwPM/q9+uf9O9NUEpVqLl5/JtFDqLIQrTRzmEAAAABAAAAAAAAAAIAAAAPAAAAA2ZlZQAAAAASAAAAAAAAAADte5nJrehJq/pu3qlV/bASRSOiJVXdNC+gQW/nxVNWuQAAAAr/////////////////8/qT",
            tx4TransactionEvents[1]);

        var tx4DiagnosticEvents = tx4Events.DiagnosticEventsXdr;
        Assert.IsNotNull(tx4DiagnosticEvents);
        Assert.AreEqual(3, tx4DiagnosticEvents.Length);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEG1heF9yd19kYXRhX2J5dGUAAAAFAAAAAAAAAbg=",
            tx4DiagnosticEvents[0]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAEG1heF9yd19jb2RlX2J5dGUAAAAFAAAAAAAAQEw=",
            tx4DiagnosticEvents[1]);
        Assert.AreEqual(
            "AAAAAAAAAAAAAAAAAAAAAgAAAAAAAAACAAAADwAAAAxjb3JlX21ldHJpY3MAAAAPAAAAE21heF9lbWl0X2V2ZW50X2J5dGUAAAAABQAAAAAAAAEE",
            tx4DiagnosticEvents[2]);

        var tx4ContractEvents = tx4Events.ContractEventsXdr;
        Assert.IsNotNull(tx4ContractEvents);
        Assert.AreEqual(1, tx4ContractEvents.Length);
        Assert.AreEqual(1, tx4ContractEvents[0].Length);
        Assert.AreEqual(
            "AAAAAAAAAAHf65G24dyt1q+Xu3xFX5fzdHcKf3j2lXO5n11b+EnOfAAAAAEAAAAAAAAAAQAAAA8AAAAEaW5pdAAAABAAAAABAAAABQAAAAUAAAAAAAaRmQAAABIAAAAAAAAAACcMY2GvjF3igK326WyiU8hv107p9YxvAS29gt1fml2WAAAAEgAAAAAAAAAAyewwXk7lqpxiQNYP3VlZ1EEprNK+dSBV4KQ9iluwbx8AAAASAAAAAAAAAAAY2Rm1IXXndEI0rYg2bt1/rw2mi1SYOUT2qeKPvf56cgAAABIAAAABusKzizgXRsUWKJQRrpWHAWG/yujQ6LBT/pMDljEiAeg=",
            tx4ContractEvents[0][0]);

        #endregion

        var tx5 = transactions[4];
        Assert.AreEqual(TransactionInfo.TransactionStatus.FAILED, tx5.Status);
        Assert.AreEqual("89ed109b74a65e28f6771b78ca70c6aa937792eea16506eb359cb58cc94e5db5", tx5.TxHash);
        Assert.AreEqual(1888540L, tx5.Ledger);
        Assert.AreEqual(1717166047, tx5.CreatedAt);
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

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeConfigSetting()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "cee63f05-e7ad-42c8-bc79-ceb9374043a4",
              "result": {
                "entries": [
                  {
                    "key": "AAAACAAAAAA=",
                    "xdr": "AAAACAAAAAAAAQAA",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAE=",
                    "xdr": "AAAACAAAAAEAAAAAHc1lAAAAAAAF9eEAAAAAAAAAABkCgAAA",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAI=",
                    "xdr": "AAAACAAAAAIAAADIAAehIAAAAH0AARFwAAAAKAADDUAAAAAZAAEEAAAAAAAAABhqAAAAAAAAJxAAAAAAAAAG+gAAAAAR4aMAAAAAAAAAJmwAAAAAAAAvVAAAE4g=",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAM=",
                    "xdr": "AAAACAAAAAMAAAAAAAA/aw==",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAQ=",
                    "xdr": "AAAACAAAAAQAACAGAAAAAAAAJxA=",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAU=",
                    "xdr": "AAAACAAAAAUAARgAAAEYAAAAAAAAAAZY",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAY=",
                    "xdr": "AAAACAAAAAYAAAAtAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAbIAAAAAAAAAEAAAAAAAAAAAAAAAKgAAAAAAAAAQAAAAAAAAAAAAAAAsAAAAAAAAABAAAAAAAAAAAAAAAScAAAAAAAAAAAAAAAAAAAAAAAAAPAAAAAAAAAAAAAAAAAAAAAAAAADdAAAAAAAAABoAAAAAAAAAAAAAAUsAAAAAAAAREQAAAAAAAAAAAAAONAAAAAAAABtlAAAAAAAAAAAAAJ1AAAAAAAAAAAAAAAAAAAAAAAAFws8AAAAAAAAP2wAAAAAAAAAAAAZeygAAAAAAALKQAAAAAAAAAAAAAKC2AAAAAAAAAnoAAAAAAAAAAAAAB5kAAAAAAAAAAAAAAAAAAAAAAAAZUQAAAAAAABc3AAAAAAAAAAAAAALHAAAAAAAAAAAAAAAAAAAAAAAjUjQAAAAAAAAAAAAAAAAAAAAAAAAQUAAAAAAAAAAAAAAAAAAAAAAAABJsAAAAAAAAAAAAAAAAAAAAAAAAEkgAAAAAAAAAAAAAAAAAAAAAAAAQoAAAAAAAAAAAAAAAAAAAAAAAAAN0AAAAAAAAAAAAAAAAAAAAAAAABCMAAAAAAAAB9gAAAAAAAAAAAAEddQAAAAAAAGNCAAAAAAAAAAAAAAAAAAAAAAAIQFAAAAAAAAAAAAAAAAAAAAAAAAKw6wAAAAAAAAAAAAAAAAAAAAAAAHUlAAAAAAAAAAAAAAAAAAAAAAAQMkkAAAAAAAAAAAAAAAAAAAAAAAOfGAAAAAAAAAAAAAAAAAAAAAAABQMcAAAAAAAAAAAAAAAAAAAAAAAKtZUAAAAAAAAAAAAAAAAAAAAAAAaNRwAAAAAAAAAAAAAAAAAAAAAAAAAcAAAAAAAAAAAAAKgWAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAdhAAAAAAAAAAAAAAAAAAAAAAAACnXAAAAAAAAAAAAAAAAAAAAAAAADOQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFn+AAAAAAAAAAAAAAAAAAAAAAAApfgAAAAAAAAAAAAAAAAAAAAAAAymLgAAAAAAAAAAAAAAAAAAAAAABIiMAAAAAAAAAAAAAAAAAAAAAAAAAA4AAAAAAAAAAAAAB1oAAAAAAAAAAAAAAAAAAAAAAC3KSgAAAAAAAAAA",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAc=",
                    "xdr": "AAAACAAAAAcAAAAtAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADyAAAAAAAAAYAAAAAAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIGpQAAAAAAABMnAAAAAAAAAAAAAQ9gAAAAAAAABMEAAAAAAAAAAAAAAA4AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALUAAAAAAAAAAAAAAAAAAAAAAAAAYwAAAAAAAAAAAAAAAAAAAAAAAABjAAAAAAAAAAAAAAAAAAAAAAAAAGMAAAAAAAAAAAAAAAAAAAAAAAAAYwAAAAAAAAAAAAAAAAAAAAAAAABjAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABEnAAAAAAAABk5AAAAAAAAAAAAAAAAAAAAAAAAuWgAAAAAAAAAAAAAAAAAAAAAAAA0bAAAAAAAAAAAAAAAAAAAAAAAABiNAAAAAAAAAAAAAAAAAAAAAAAA/J4AAAAAAAAAAAAAAAAAAAAAAABxkgAAAAAAAAAAAAAAAAAAAAAAALvfAAAAAAAAAAAAAAAAAAAAAAABkz0AAAAAAAAAAAAAAAAAAAAAAACOKgAAAAAAAAAAAAAAAAAAAAAAAAEBAAAAAAAAAAAAARQwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA5FQAAAAAAAAAAAAAAAAAAAAAAABqxAAAAAAAAAAAAAAAAAAAAAAAABAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAfpgAAAAAAAAAAAAAAAAAAAAAAAANWEAAAAAAAAAAAAAAAAAAAAAAAF9ZQAAAAAAAAAAAAAAAAAAAAAAACPYAAAAAAAAAAAAAAAAAAAAAAAAAH4AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAg=",
                    "xdr": "AAAACAAAAAgAAAD6",
                    "lastModifiedLedgerSeq": 276293
                  },
                  {
                    "key": "AAAACAAAAAk=",
                    "xdr": "AAAACAAAAAkAAQAA",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAo=",
                    "xdr": "AAAACAAAAAoAL3YAAABDgAAfpAAAAAAAAAAINwAAAAAAABBuAAAD6AAAAB4AAABAAAGGoAAAAAc=",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAs=",
                    "xdr": "AAAACAAAAAsAAABk",
                    "lastModifiedLedgerSeq": 1157
                  },
                  {
                    "key": "AAAACAAAAAw=",
                    "xdr": "AAAACAAAAAwAAAAeAAAAAAAAAGQAAAAAAAAAZAAAAAAAAABkAAAAAAAAAGQAAAAAAAAAZAAAAAAAAABkAAAAAAAAAGQAAAAAAAAAZAAAAAAAAABkAAAAAAAAAGQAAAAAAAAAZAAAAAAAAABkAAAAAAAAAGQAAAAAAAAAZAAAAAAAAABkAAAAAAAAAGQAAAAAAAAAZAAAAAAAAABkAAAAAAAAAGQAAAAAAAAAZAAAAAAAAABkAAAAAAAAAGQAAAAAAAALxAAAAAAAAA0IAAAAAAAADkwAAAAAAAAOTAAAAAAAAA+QAAAAAAAAD5AAAAAAAAAPkAAAAAAAAA+Q",
                    "lastModifiedLedgerSeq": 512
                  },
                  {
                    "key": "AAAACAAAAA0=",
                    "xdr": "AAAACAAAAA0AAAAGAAAAAQAAAAAAAAAA",
                    "lastModifiedLedgerSeq": 575
                  },
                  {
                    "key": "AAAACAAAAA4=",
                    "xdr": "AAAACAAAAA4AAAAL",
                    "lastModifiedLedgerSeq": 575
                  },
                  {
                    "key": "AAAACAAAAA8=",
                    "xdr": "AAAACAAAAA8AAAAMAAAAAAAAEBs=",
                    "lastModifiedLedgerSeq": 175
                  },
                  {
                    "key": "AAAACAAAABA=",
                    "xdr": "AAAACAAAABAAAAG8AAABuQAAADcAAAB7AAAADQ==",
                    "lastModifiedLedgerSeq": 175
                  }
                ],
                "latestLedger": 1172233
              }
            }
            """;
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);
        var response = await sorobanServer.GetLedgerEntries([]);

        Assert.IsNotNull(response);
        var entries = response.LedgerEntries;
        Assert.IsNotNull(entries);
        Assert.AreEqual(1172233U, response.LatestLedger);
        Assert.AreEqual(17, entries.Length);

        var entry0 = entries[0] as ConfigSettingContractMaxSizeBytes;
        Assert.IsNotNull(entry0);
        Assert.AreEqual(65536U, entry0.InnerValue);

        var entry1 = entries[1] as ConfigSettingContractCompute;
        Assert.IsNotNull(entry1);
        Assert.AreEqual(500000000L, entry1.LedgerMaxInstructions);
        Assert.AreEqual(100000000L, entry1.TxMaxInstructions);
        Assert.AreEqual(25L, entry1.FeeRatePerInstructionsIncrement);
        Assert.AreEqual(41943040U, entry1.TxMemoryLimit);

        var entry2 = entries[2] as ConfigSettingContractLedgerCost;
        Assert.IsNotNull(entry2);
        Assert.AreEqual(200U, entry2.LedgerMaxDiskReadEntries);
        Assert.AreEqual(500000U, entry2.LedgerMaxDiskReadBytes);
        Assert.AreEqual(125U, entry2.LedgerMaxWriteLedgerEntries);
        Assert.AreEqual(70000U, entry2.LedgerMaxWriteBytes);
        Assert.AreEqual(40U, entry2.TxMaxDiskReadEntries);
        Assert.AreEqual(200000U, entry2.TxMaxDiskReadBytes);
        Assert.AreEqual(25U, entry2.TxMaxWriteLedgerEntries);
        Assert.AreEqual(66560U, entry2.TxMaxWriteBytes);
        Assert.AreEqual(6250L, entry2.FeeDiskReadLedgerEntry);
        Assert.AreEqual(10000L, entry2.FeeWriteLedgerEntry);
        Assert.AreEqual(1786L, entry2.FeeDiskRead1Kb);
        Assert.AreEqual(300000000L, entry2.SorobanStateTargetSizeBytes);
        Assert.AreEqual(9836L, entry2.RentFee1KbSorobanStateSizeLow);
        Assert.AreEqual(12116L, entry2.RentFee1KbSorobanStateSizeHigh);
        Assert.AreEqual(5000U, entry2.SorobanStateRentFeeGrowthFactor);

        var entry3 = entries[3] as ConfigSettingContractHistoricalData;
        Assert.IsNotNull(entry3);
        Assert.AreEqual(16235L, entry3.FeeHistorical1Kb);

        var entry4 = entries[4] as ConfigSettingContractEvents;
        Assert.IsNotNull(entry4);
        Assert.AreEqual(8198U, entry4.TxMaxContractEventsSizeBytes);
        Assert.AreEqual(10000L, entry4.FeeContractEvents1Kb);

        var entry5 = entries[5] as ConfigSettingContractBandwidth;
        Assert.IsNotNull(entry5);
        Assert.AreEqual(71680U, entry5.LedgerMaxTxsSizeBytes);
        Assert.AreEqual(71680U, entry5.TxMaxSizeBytes);
        Assert.AreEqual(1624L, entry5.FeeTxSize1Kb);

        var entry6 = entries[6] as ConfigSettingContractCostParamsCpuInstructions;
        Assert.IsNotNull(entry6);
        var paramEntries = entry6.ParamEntries;
        Assert.IsNotNull(paramEntries);
        Assert.AreEqual(45, paramEntries.Length);

        var pEntry0 = paramEntries[0];
        Assert.IsNotNull(pEntry0);
        Assert.IsInstanceOfType(pEntry0.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(4L, pEntry0.ConstTerm);
        Assert.AreEqual(0L, pEntry0.LinearTerm);

        var pEntry1 = paramEntries[1];
        Assert.IsNotNull(pEntry1);
        Assert.IsInstanceOfType(pEntry1.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(434L, pEntry1.ConstTerm);
        Assert.AreEqual(16L, pEntry1.LinearTerm);

        var pEntry33 = paramEntries[33];
        Assert.IsNotNull(pEntry33);
        Assert.IsInstanceOfType(pEntry33.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(43030L, pEntry33.ConstTerm);
        Assert.AreEqual(0L, pEntry33.LinearTerm);

        var pEntry44 = paramEntries[44];
        Assert.IsNotNull(pEntry44);
        Assert.IsInstanceOfType(pEntry44.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(3000906L, pEntry44.ConstTerm);
        Assert.AreEqual(0L, pEntry44.LinearTerm);

        var entry7 = entries[7] as ConfigSettingContractCostParamsMemoryBytes;
        Assert.IsNotNull(entry7);
        paramEntries = entry7.ParamEntries;
        Assert.IsNotNull(paramEntries);
        Assert.AreEqual(45, paramEntries.Length);

        var pEntry3 = paramEntries[3];
        Assert.IsNotNull(pEntry3);
        Assert.IsInstanceOfType(pEntry3.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(0L, pEntry3.ConstTerm);
        Assert.AreEqual(0L, pEntry3.LinearTerm);

        var pEntry6 = paramEntries[6];
        Assert.IsNotNull(pEntry6);
        Assert.IsInstanceOfType(pEntry6.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(242L, pEntry6.ConstTerm);
        Assert.AreEqual(384L, pEntry6.LinearTerm);

        var pEntry12 = paramEntries[12];
        Assert.IsNotNull(pEntry12);
        Assert.IsInstanceOfType(pEntry12.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(69472L, pEntry12.ConstTerm);
        Assert.AreEqual(1217L, pEntry12.LinearTerm);

        var pEntry30 = paramEntries[30];
        Assert.IsNotNull(pEntry30);
        Assert.IsInstanceOfType(pEntry30.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(0L, pEntry30.ConstTerm);
        Assert.AreEqual(103229L, pEntry30.LinearTerm);

        var entry8 = entries[8] as ConfigSettingContractDataKeySizeBytes;
        Assert.IsNotNull(entry8);
        Assert.AreEqual(250U, entry8.InnerValue);

        var entry9 = entries[9] as ConfigSettingContractDataEntrySizeBytes;
        Assert.IsNotNull(entry9);
        Assert.AreEqual(65536U, entry9.InnerValue);

        var entry10 = entries[10] as StateArchivalSettings;
        Assert.IsNotNull(entry10);
        Assert.AreEqual(3110400U, entry10.MaxEntryTtl);
        Assert.AreEqual(17280U, entry10.MinTemporaryTtl);
        Assert.AreEqual(2073600U, entry10.MinPersistentTtl);
        Assert.AreEqual(2103L, entry10.PersistentRentRateDenominator);
        Assert.AreEqual(4206L, entry10.TempRentRateDenominator);
        Assert.AreEqual(1000U, entry10.MaxEntriesToArchive);
        Assert.AreEqual(30U, entry10.LiveSorobanStateSizeWindowSampleSize);
        Assert.AreEqual(64U, entry10.LiveSorobanStateSizeWindowSamplePeriod);
        Assert.AreEqual(100000U, entry10.EvictionScanSize);
        Assert.AreEqual(7U, entry10.StartingEvictionScanLevel);

        var entry11 = entries[11] as ConfigSettingContractExecutionLanes;
        Assert.IsNotNull(entry11);
        Assert.AreEqual(100U, entry11.LedgerMaxTxCount);

        var entry12 = entries[12] as ConfigSettingLiveSorobanStateSizeWindow;
        Assert.IsNotNull(entry12);
        Assert.AreEqual(30, entry12.InnerValue.Length);
        Assert.AreEqual(100UL, entry12.InnerValue[0]);
        Assert.AreEqual(3012UL, entry12.InnerValue[22]);
        Assert.AreEqual(3984UL, entry12.InnerValue[29]);

        var entry13 = entries[13] as EvictionIterator;
        Assert.IsNotNull(entry13);
        Assert.AreEqual(6U, entry13.BucketListLevel);
        Assert.AreEqual(true, entry13.IsCurrBucket);
        Assert.AreEqual(0UL, entry13.BucketFileOffset);

        var entry14 = entries[14] as ConfigSettingContractParallelComputeV0;
        Assert.IsNotNull(entry14);
        Assert.AreEqual(11U, entry14.LedgerMaxDependentTxClusters);

        var entry15 = entries[15] as ConfigSettingContractLedgerCostExtV0;
        Assert.IsNotNull(entry15);
        Assert.AreEqual(4123L, entry15.FeeWrite1Kb);
        Assert.AreEqual(12U, entry15.TxMaxFootprintEntries);

        var entry16 = entries[16] as ConfigSettingScpTiming;
        Assert.IsNotNull(entry16);
        Assert.AreEqual(444U, entry16.LedgerTargetCloseTimeMilliseconds);
        Assert.AreEqual(441U, entry16.NominationTimeoutInitialMilliseconds);
        Assert.AreEqual(55U, entry16.NominationTimeoutIncrementMilliseconds);
        Assert.AreEqual(123U, entry16.BallotTimeoutInitialMilliseconds);
        Assert.AreEqual(13U, entry16.BallotTimeoutIncrementMilliseconds);
    }

    [TestMethod]
    public async Task TestGetLedgerEntriesOfTypeTtl()
    {
        const string json =
            """
            {
              "jsonrpc": "2.0",
              "id": "fd6785fe-3535-4106-b979-b0f01e2ab541",
              "error": {
                "code": -32602,
                "message": "ledger ttl entries cannot be queried directly"
              }
            }
            """;

        var ledgerKeyTtl = new LedgerKey[]
        {
            new LedgerKeyTtl("fsh67B45yRcC/gKPI2ky8EPbFMtc8y0fnjUDaI36OKc="),
        };
        using var sorobanServer = Utils.CreateTestSorobanServerWithContent(json);

        var response = await sorobanServer.GetLedgerEntries(ledgerKeyTtl);
        Assert.IsNull(response);
    }

    private Transaction CreateDummyTransaction(bool sign = true)
    {
        var account = new Account(AccountId, 0);
        var transaction = new TransactionBuilder(account)
            .AddOperation(new BeginSponsoringFutureReservesOperation(_account))
            .Build();
        if (sign)
        {
            transaction.Sign(_account);
        }
        return transaction;
    }

    private FeeBumpTransaction CreateDummyFeeBumpTransaction(bool sign = true)
    {
        var innerTransaction = CreateDummyTransaction();
        const long maxFee = 10000;

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
            _account,
            innerTransaction,
            maxFee
        );

        if (sign)
        {
            feeBumpTransaction.Sign(_account);
        }

        return feeBumpTransaction;
    }

    [TestMethod]
    public void Constructor_WithHttpResilienceOptions_CreatesSorobanServerWithResilience()
    {
        // Arrange
        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            MaxDelay = TimeSpan.FromSeconds(5),
        };

        // Act
        using var server = new SorobanServer("https://soroban-testnet.stellar.org", resilienceOptions);

        // Assert - Server should be created successfully
        Assert.IsNotNull(server);
    }

    [TestMethod]
    public void Constructor_WithBearerTokenAndHttpResilienceOptions_CreatesSorobanServerWithBoth()
    {
        // Arrange
        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 5,
            BaseDelay = TimeSpan.FromMilliseconds(500),
            MaxDelay = TimeSpan.FromSeconds(15),
        };

        // Act
        using var server = new SorobanServer("https://soroban-testnet.stellar.org", resilienceOptions, "test-token");

        // Assert - Server should be created successfully
        Assert.IsNotNull(server);
    }

    [TestMethod]
    public void Constructor_WithNullHttpResilienceOptions_CreatesSorobanServerWithDefaultResilience()
    {
        // Act
        using var server = new SorobanServer("https://soroban-testnet.stellar.org", null, null);

        // Assert - Server should be created successfully with default resilience (no retries)
        Assert.IsNotNull(server);
    }

    [TestMethod]
    public void Constructor_WithHttpResilienceOptions_UsesResilienceForRequests()
    {
        // Arrange
        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 1,
            BaseDelay = TimeSpan.FromMilliseconds(10),
            UseJitter = false,
        };

        // Act
        using var sorobanServer = new SorobanServer("https://soroban-testnet.stellar.org", resilienceOptions);
        // Note: This test verifies the constructor accepts the parameter
        // Actual resilience behavior is tested in DefaultStellarSdkHttpClientTests

        // Assert - Server should be created successfully
        Assert.IsNotNull(sorobanServer);
    }
}