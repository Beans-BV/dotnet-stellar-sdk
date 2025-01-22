﻿using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable

public class InvokeHostFunctionOperationResponse : OperationResponse
{
    public override int TypeId => 24;

    [JsonPropertyName("address")]
    public string Address { get; init; }

    [JsonPropertyName("asset_balance_changes")]
    public AssetContractBalanceChange[] AssetBalanceChanges { get; init; }

    [JsonPropertyName("function")]
    public string Function { get; init; }

    [JsonPropertyName("parameters")]
    public HostFunctionParameter[] Parameters { get; init; }

    [JsonPropertyName("salt")] public string Salt { get; init; }

    public class HostFunctionParameter
    {
        [JsonPropertyName("type")] public string Type { get; init; }

        [JsonPropertyName("value")] public string Value { get; init; }
    }

    public class AssetContractBalanceChange
    {
        [JsonPropertyName("amount")]
        public string Amount { get; init; }

        [JsonPropertyName("asset_code")]
        public string AssetCode { get; init; }

        [JsonPropertyName("asset_issuer")]
        public string AssetIssuer { get; init; }

        [JsonPropertyName("asset_type")]
        public string AssetType { get; init; }

        [JsonPropertyName("from")] public string From { get; init; }

        [JsonPropertyName("to")] public string To { get; init; }

        [JsonPropertyName("type")] public string Type { get; init; }
    }
}