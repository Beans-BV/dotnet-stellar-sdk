namespace StellarDotnetSdk.Responses;

/// <summary>
///     Fee is too small.
/// </summary>
public class TransactionResultInsufficientFee : TransactionResult
{
    public override bool IsSuccess => false;
}