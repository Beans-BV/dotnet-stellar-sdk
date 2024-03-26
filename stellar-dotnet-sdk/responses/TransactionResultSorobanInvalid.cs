namespace stellar_dotnet_sdk.responses;

/// <summary>
///     TODO Summary not available on stellar.org
/// </summary>
public class TransactionResultSorobanInvalid : TransactionResult
{
    public override bool IsSuccess => false;
}