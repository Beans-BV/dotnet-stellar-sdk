using System.Linq;
using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

public class SimulateTransactionResponse
{
    public SimulateTransactionCost Cost;
    public string? Error;

    public string[]? Events;
    public long LatestLedger;

    public uint MinResourceFee;

    public RestorePreamble RestorePreambleInfo;

    // An array of the individual host function call results.
    // This will only contain a single element if present, because only a single
    // invokeHostFunctionOperation is supported per transaction.
    public SimulateInvokeHostFunctionResult[]? Results;

    public string? TransactionData;

    public SorobanTransactionData? SorobanTransactionData
    {
        get
        {
            if (TransactionData != null) return SorobanTransactionData.FromXdrBase64(TransactionData);

            return null;
        }
    }

    public SorobanAuthorizationEntry[]? SorobanAuthorization
    {
        get
        {
            if (Results is { Length: > 0 } && Results[0].Auth != null)
                return Results[0].Auth.Select(SorobanAuthorizationEntry.FromXdrBase64).ToArray();
            return null;
        }
    }

    public class RestorePreamble
    {
        public long MinResourceFee;
        public string TransactionData;
    }

    public class SimulateTransactionCost
    {
        [JsonProperty(PropertyName = "cpuInsns")]
        public long CpuInstructions;

        [JsonProperty(PropertyName = "memBytes")]
        public long MemoryBytes;
    }

    public class SimulateInvokeHostFunctionResult
    {
        public string[]? Auth;
        public string? Xdr;
    }
}