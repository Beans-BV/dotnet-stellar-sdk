using System.Net.Http;
using System.Threading;

namespace StellarDotnetSdk.Tests.Requests;

internal class FakeResponseWithIOError : FakeResponse
{
    public override HttpResponseMessage MakeResponse(CancellationToken cancellationToken)
    {
        throw new HttpRequestException("Unit Test Exception Message");
    }
}