namespace stellar_dotnet_sdk.responses;

/// <summary>
///     TODO Summary not available on stellar.org
/// </summary>
public class TransactionResultMalformed : TransactionResult
{
    public override bool IsSuccess => false;
}