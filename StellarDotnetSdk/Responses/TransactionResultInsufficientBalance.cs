namespace StellarDotnetSdk.Responses;

/// <summary>
///     Fee would bring account below reserve.
/// </summary>
public class TransactionResultInsufficientBalance : TransactionResult
{
    public override bool IsSuccess => false;
}