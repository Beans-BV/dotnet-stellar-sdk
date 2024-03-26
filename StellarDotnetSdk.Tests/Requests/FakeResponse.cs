using System.Net.Http;
using System.Threading;

namespace StellarDotnetSdk.Tests.Requests;

public abstract class FakeResponse
{
    public abstract HttpResponseMessage MakeResponse(CancellationToken cancellationToken);

    public static FakeResponse StartsStream(params StreamAction[] actions)
    {
        return new FakeResponseWithStream(actions);
    }

    public static FakeResponse WithIOError()
    {
        return new FakeResponseWithIOError();
    }
}