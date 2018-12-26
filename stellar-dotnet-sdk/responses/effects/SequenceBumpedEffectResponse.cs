﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace stellar_dotnet_sdk.responses.effects
{
    /// <summary>
    ///     Represents account_home_domain_updated effect response.
    ///     See: https://www.stellar.org/developers/horizon/reference/resources/effect.html
    ///     <seealso cref="requests.EffectsRequestBuilder" />
    ///     <seealso cref="Server" />
    /// </summary>
    public class SequenceBumpedEffectResponse : EffectResponse
    {
        public override int TypeId => 43;

        [JsonProperty(PropertyName = "new_seq")]
        public long NewSequence { get; private set; }

        public SequenceBumpedEffectResponse()
        {

        }

        public SequenceBumpedEffectResponse(long newSequence)
        {
            this.NewSequence = newSequence;
        }
    }
}