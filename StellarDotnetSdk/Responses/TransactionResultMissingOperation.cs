namespace StellarDotnetSdk.Responses;

/// <summary>
///     No operation was specified.
/// </summary>
public class TransactionResultMissingOperation : TransactionResult
{
    public override bool IsSuccess => false;
}