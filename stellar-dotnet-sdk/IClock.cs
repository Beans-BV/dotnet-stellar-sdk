using System;

namespace stellar_dotnet_sdk;

public interface IClock
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
