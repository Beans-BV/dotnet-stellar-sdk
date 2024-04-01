namespace StellarDotnetSdk.Responses;

/// <summary>
///     Too few valid signatures or invalid network.
/// </summary>
public class TransactionResultBadAuth : TransactionResult
{
    public override bool IsSuccess => false;
}