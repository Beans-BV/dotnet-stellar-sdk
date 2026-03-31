using System;
using System.Net.Http;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

public class
    TradesAggregationRequestBuilder : RequestBuilderExecutePageable<TradesAggregationRequestBuilder,
    TradeAggregationResponse>
{
    public TradesAggregationRequestBuilder(Uri serverUri, HttpClient httpClient)
        : base(serverUri, "trade_aggregations", httpClient)
    {
    }

    /// <summary>
    ///     Sets the lower time boundary for the aggregation range (inclusive), as a Unix timestamp in milliseconds.
    /// </summary>
    /// <param name="startTime">The start time in milliseconds since epoch.</param>
    /// <returns>The current <see cref="TradesAggregationRequestBuilder" /> instance for chaining.</returns>
    public TradesAggregationRequestBuilder StartTime(long startTime)
    {
        UriBuilder.SetQueryParam("start_time", startTime.ToString());
        return this;
    }

    /// <summary>
    ///     Sets the upper time boundary for the aggregation range (exclusive), as a Unix timestamp in milliseconds.
    /// </summary>
    /// <param name="endTime">The end time in milliseconds since epoch.</param>
    /// <returns>The current <see cref="TradesAggregationRequestBuilder" /> instance for chaining.</returns>
    public TradesAggregationRequestBuilder EndTime(long endTime)
    {
        UriBuilder.SetQueryParam("end_time", endTime.ToString());
        return this;
    }

    /// <summary>
    ///     Sets the segment duration for each aggregation bucket, in milliseconds.
    /// </summary>
    /// <param name="resolution">The resolution in milliseconds (e.g. 300000 for 5 minutes).</param>
    /// <returns>The current <see cref="TradesAggregationRequestBuilder" /> instance for chaining.</returns>
    public TradesAggregationRequestBuilder Resolution(long resolution)
    {
        UriBuilder.SetQueryParam("resolution", resolution.ToString());
        return this;
    }

    /// <summary>
    ///     Sets the offset for each aggregation bucket, in milliseconds. Must be less than the resolution.
    /// </summary>
    /// <param name="offset">The offset in milliseconds applied to each aggregation bucket.</param>
    /// <returns>The current <see cref="TradesAggregationRequestBuilder" /> instance for chaining.</returns>
    public TradesAggregationRequestBuilder Offset(long offset)
    {
        UriBuilder.SetQueryParam("offset", offset.ToString());
        return this;
    }

    /// <summary>
    ///     Sets the base asset for the trade aggregation query.
    /// </summary>
    /// <param name="asset">The base asset.</param>
    /// <returns>The current <see cref="TradesAggregationRequestBuilder" /> instance for chaining.</returns>
    public TradesAggregationRequestBuilder BaseAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("base_asset_type", asset.Type);
        if (asset is not AssetTypeCreditAlphaNum creditAlphaNumAsset)
        {
            return this;
        }
        UriBuilder.SetQueryParam("base_asset_code", creditAlphaNumAsset.Code);
        UriBuilder.SetQueryParam("base_asset_issuer", creditAlphaNumAsset.Issuer);

        return this;
    }

    /// <summary>
    ///     Sets the counter asset for the trade aggregation query.
    /// </summary>
    /// <param name="asset">The counter asset.</param>
    /// <returns>The current <see cref="TradesAggregationRequestBuilder" /> instance for chaining.</returns>
    public TradesAggregationRequestBuilder CounterAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("counter_asset_type", asset.Type);
        if (asset is not AssetTypeCreditAlphaNum creditAlphaNumAsset)
        {
            return this;
        }
        UriBuilder.SetQueryParam("counter_asset_code", creditAlphaNumAsset.Code);
        UriBuilder.SetQueryParam("counter_asset_issuer", creditAlphaNumAsset.Issuer);

        return this;
    }
}