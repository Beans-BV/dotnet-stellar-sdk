namespace stellar_dotnet_sdk.responses;

/// <summary>
///     The transaction type is not supported.
/// </summary>
public class TransactionResultNotSupported : TransactionResult
{
    public override bool IsSuccess => false;
}