﻿using System;
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

    public TradesAggregationRequestBuilder StartTime(long startTime)
    {
        UriBuilder.SetQueryParam("start_time", startTime.ToString());
        return this;
    }

    public TradesAggregationRequestBuilder EndTime(long endTime)
    {
        UriBuilder.SetQueryParam("end_time", endTime.ToString());
        return this;
    }

    public TradesAggregationRequestBuilder Resolution(long resolution)
    {
        UriBuilder.SetQueryParam("resolution", resolution.ToString());
        return this;
    }

    public TradesAggregationRequestBuilder Offset(long offset)
    {
        UriBuilder.SetQueryParam("offset", offset.ToString());
        return this;
    }

    public TradesAggregationRequestBuilder BaseAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("base_asset_type", asset.Type);
        if (asset is not AssetTypeCreditAlphaNum creditAlphaNumAsset) return this;
        UriBuilder.SetQueryParam("base_asset_code", creditAlphaNumAsset.Code);
        UriBuilder.SetQueryParam("base_asset_issuer", creditAlphaNumAsset.Issuer);

        return this;
    }

    public TradesAggregationRequestBuilder CounterAsset(Asset asset)
    {
        UriBuilder.SetQueryParam("counter_asset_type", asset.Type);
        if (asset is not AssetTypeCreditAlphaNum creditAlphaNumAsset) return this;
        UriBuilder.SetQueryParam("counter_asset_code", creditAlphaNumAsset.Code);
        UriBuilder.SetQueryParam("counter_asset_issuer", creditAlphaNumAsset.Issuer);

        return this;
    }
}