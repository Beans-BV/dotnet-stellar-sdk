﻿using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.operations
{
    /// <inheritdoc />
    /// <summary>
    /// Sends an amount in a specific asset to a destination account through a path of offers. This allows the asset sent (e.g., 450 XLM) to be different from the asset received (e.g, 6 BTC).
    /// See: https://www.stellar.org/developers/horizon/reference/resources/operation.html
    /// <seealso cref="T:stellar_dotnetcore_sdk.requests.OperationsRequestBuilder" />
    /// <seealso cref="T:stellar_dotnetcore_sdk.Server" />
    /// </summary>
    public class PathPaymentOperationResponse : OperationResponse
    {
        public PathPaymentOperationResponse()
        {

        }

        /// <summary>
        /// Sends an amount in a specific asset to a destination account through a path of offers. This allows the asset sent (e.g., 450 XLM) to be different from the asset received (e.g, 6 BTC).
        /// </summary>
        /// <param name="amount">The amount of destination asset the destination account receives.</param>
        /// <param name="sourceMax">The amount of source asset deducted from senders account.</param>
        /// <param name="from">Account address that is sending the payment.</param>
        /// <param name="to">Account address that receives the payment.</param>
        /// <param name="assetType">Account address that receives the payment.</param>
        /// <param name="assetCode">The asset code (Alpha4, Alpha12, etc.)</param>
        /// <param name="assetIssuer">The account that created the asset</param>
        /// <param name="sendAssetType">The asset type (USD, BTC, etc.) to be sent.</param>
        /// <param name="sendAssetCode">The asset code (Alpha4, Alpha12, etc.) to be sent</param>
        /// <param name="sendAssetIssuer">The account that created the asset to be sent.</param>
        public PathPaymentOperationResponse(string amount, string sourceAmount, string sourceMax, string from, string to, string assetType, string assetCode,
            string assetIssuer, string sendAssetType, string sendAssetCode, string sendAssetIssuer)
        {
            Amount = amount;
            SourceAmount = sourceAmount;
            SourceMax = sourceMax;
            From = from;
            To = to;
            AssetType = assetType;
            AssetCode = assetCode;
            AssetIssuer = assetIssuer;
            SendAssetType = sendAssetType;
            SendAssetCode = sendAssetCode;
            SendAssetIssuer = sendAssetIssuer;
        }

        public override int TypeId => 2;

        /// <summary>
        /// The amount of destination asset the destination account receives.
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; private set; }

        /// <summary>
        /// The amount of source asset deducted from senders account.
        /// </summary>
        [JsonProperty(PropertyName = "source_amount")]
        public string SourceAmount { get; private set; }

        /// <summary>
        /// The amount of source asset deducted from senders account.
        /// </summary>
        [JsonProperty(PropertyName = "source_max")]
        public string SourceMax { get; private set; }

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
        /// Account address that receives the payment.
        /// </summary>
        [JsonProperty(PropertyName = "asset_type")]
        public string AssetType { get; private set; }

        /// <summary>
        /// The asset code (Alpha4, Alpha12, etc.)
        /// </summary>
        [JsonProperty(PropertyName = "asset_code")]
        public string AssetCode { get; private set; }

        /// <summary>
        /// The account that created the asset
        /// </summary>
        [JsonProperty(PropertyName = "asset_issuer")]
        public string AssetIssuer { get; private set; }

        /// <summary>
        /// The asset type (USD, BTC, etc.) to be sent.
        /// </summary>
        [JsonProperty(PropertyName = "send_asset_type")]
        public string SendAssetType { get; private set; }

        /// <summary>
        /// The asset code (Alpha4, Alpha12, etc.) to be sent
        /// </summary>
        [JsonProperty(PropertyName = "send_asset_code")]
        public string SendAssetCode { get; private set; }

        /// <summary>
        /// The account that created the asset to be sent.
        /// </summary>
        [JsonProperty(PropertyName = "send_asset_issuer")]
        public string SendAssetIssuer { get; private set; }

        /// <summary>
        /// Asset from source to send.
        /// </summary>
        public Asset Asset => Asset.CreateNonNativeAsset(AssetType, AssetIssuer, AssetCode);

        /// <summary>
        /// Asset to destination.
        /// </summary>
        public Asset SendAsset => Asset.CreateNonNativeAsset(SendAssetType, SendAssetIssuer, SendAssetCode);
    }
}
