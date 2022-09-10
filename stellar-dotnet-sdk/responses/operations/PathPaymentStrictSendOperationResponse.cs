﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace stellar_dotnet_sdk.responses.operations
{
    /// <inheritdoc />
    /// <summary>
    /// A path payment strict send operation represents a payment from one account to another through a path. This type of payment starts as one type of asset and ends as another type of asset.
    /// See: https://www.stellar.org/developers/horizon/reference/resources/operation.html#path-payment-strict-send
    /// <seealso cref="T:stellar_dotnetcore_sdk.requests.OperationsRequestBuilder" />
    /// <seealso cref="T:stellar_dotnetcore_sdk.Server" />
    /// </summary>
    public class PathPaymentStrictSendOperationResponse : OperationResponse
    {
        public PathPaymentStrictSendOperationResponse()
        {

        }

        /// <summary>
        /// Sends an amount in a specific asset to a destination account through a path of offers. This allows the asset sent (e.g., 450 XLM) to be different from the asset received (e.g, 6 BTC).
        /// </summary>
        /// <param name="from">Account address that is sending the payment.</param>
        /// <param name="to">Account address that receives the payment.</param>
        /// <param name="assetType">Destination asset type. (Alpha4, Alpha12, etc.)</param>
        /// <param name="assetCode">Destination asset code.</param>
        /// <param name="assetIssuer">Destination asset issuer account</param>
        /// <param name="amount">The amount of destination asset the destination account receives.</param>
        /// <param name="sourceAssetType">Source asset type. (Alpha4, Alpha12, etc.)</param>
        /// <param name="sourceAssetCode">Source asset code.</param>
        /// <param name="sourceAssetIssuer">Source asset issuer account.</param>
        /// <param name="sourceAmount">The amount of source asset deducted from senders account.</param>
        /// <param name="destinationMin">The minimum amount of destination asset expected to be received.</param>
        /// <param name="path">Additional hops the operation went through to get to the destination asset.</param>
        public PathPaymentStrictSendOperationResponse(string from, string to, string assetType, string assetCode, string assetIssuer, string amount, string sourceAssetType, string sourceAssetCode,
            string sourceAssetIssuer, string sourceAmount, string destinationMin, IEnumerable<Asset> path)
        {
            From = from;
            To = to;
            AssetType = assetType;
            AssetCode = assetCode;
            AssetIssuer = assetIssuer;
            Amount = amount;
            SourceAssetType = sourceAssetType;
            SourceAssetCode = sourceAssetCode;
            SourceAssetIssuer = sourceAssetIssuer;
            SourceAmount = sourceAmount;
            DestinationMin = destinationMin;
            Path = path;
        }

        public override int TypeId => 13;

        /// <summary>
        /// Account address that is sending the payment.
        /// </summary>
        [JsonProperty(PropertyName = "from")]
        public string From { get; private set; }

        /// <summary>
        /// Account address that receives the payment.
        /// </summary>
        [JsonProperty(PropertyName = "to")]
        public string To { get; private set; }

        /// <summary>
        /// The destination asset type. (Alpha4, Alpha12, etc.)
        /// </summary>
        [JsonProperty(PropertyName = "asset_type")]
        public string AssetType { get; private set; }

        /// <summary>
        /// The destination asset code (Alpha4, Alpha12, etc.)
        /// </summary>
        [JsonProperty(PropertyName = "asset_code")]
        public string AssetCode { get; private set; }

        /// <summary>
        /// The destination asset issuer account.
        /// </summary>
        [JsonProperty(PropertyName = "asset_issuer")]
        public string AssetIssuer { get; private set; }

        /// <summary>
        /// The amount of destination asset the destination account receives.
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; private set; }

        /// <summary>
        /// The source asset type. (Alpha4, Alpha12, etc.)
        /// </summary>
        [JsonProperty(PropertyName = "source_asset_type")]
        public string SourceAssetType { get; private set; }

        /// <summary>
        /// The source asset code.
        /// </summary>
        [JsonProperty(PropertyName = "source_asset_code")]
        public string SourceAssetCode { get; private set; }

        /// <summary>
        /// The source asset issuer account.
        /// </summary>
        [JsonProperty(PropertyName = "source_asset_issuer")]
        public string SourceAssetIssuer { get; private set; }

        /// <summary>
        /// The amount sent.
        /// </summary>
        [JsonProperty(PropertyName = "source_amount")]
        public string SourceAmount { get; private set; }

        /// <summary>
        /// The maximum send amount.
        /// </summary>
        [JsonProperty(PropertyName = "destination_min")]
        public string DestinationMin { get; private set; }

        /// <summary>
        /// Additional hops the operation went through to get to the destination asset
        /// </summary>
        [JsonProperty(PropertyName = "path")]
        public IEnumerable<Asset> Path { get; private set; }

        /// <summary>
        /// Destination Asset
        /// </summary>
        public Asset DestinationAsset => Asset.Create(AssetType, AssetCode, AssetIssuer);

        /// <summary>
        /// Source Asset
        /// </summary>
        public Asset SourceAsset => Asset.Create(SourceAssetType, SourceAssetCode, SourceAssetIssuer);
    }
}
