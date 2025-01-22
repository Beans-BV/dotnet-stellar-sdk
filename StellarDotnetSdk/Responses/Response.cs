using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses
{
    public abstract class Response
    {
        private const string XRateLimitLimit = "X-Ratelimit-Limit";
        private const string XRateLimitRemaining = "X-Ratelimit-Remaining";
        private const string XRateLimitReset = "X-Ratelimit-Reset";

        [JsonIgnore] protected int RateLimitLimit { get; private set; }

        [JsonIgnore] protected int RateLimitRemaining { get; private set; }

        [JsonIgnore] protected int RateLimitReset { get; private set; }

        public void SetHeaders(HttpResponseHeaders headers)
        {
            if (headers.Contains(XRateLimitLimit))
            {
                RateLimitLimit = int.Parse(headers.GetValues(XRateLimitLimit).First());
            }

            if (headers.Contains(XRateLimitRemaining))
            {
                RateLimitRemaining = int.Parse(headers.GetValues(XRateLimitRemaining).First());
            }

            if (headers.Contains(XRateLimitReset))
            {
                RateLimitReset = int.Parse(headers.GetValues(XRateLimitReset).First());
            }
        }
    }
}