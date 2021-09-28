﻿using System;
using Newtonsoft.Json;
using stellar_dotnet_sdk.converters;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk
{
    /// <summary>
    /// Class to have Asset and Amount in the same place.
    /// </summary>

    [JsonConverter(typeof(AssetAmountConverter))]
    public class AssetAmount
    {
        public Asset Asset { get; set; }

        public string Amount { get; set; }

        public AssetAmount() { }

        public AssetAmount(Asset asset, string amount)
        {
            Asset = asset;
            Amount = amount;
        }

        public override int GetHashCode()
        {
            return HashCode.Hash(Asset.GetHashCode(), Amount);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AssetAmount))
            {
                return false;
            }

            AssetAmount other = (AssetAmount)obj;
            return Equals(Asset, other.Asset) && Equals(Amount, other.Amount);
        }
    }
}