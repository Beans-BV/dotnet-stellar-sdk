namespace StellarDotnetSdk.Responses;

/// <summary>
///     Ledger closeTime before minTime.
/// </summary>
public class TransactionResultTooEarly : TransactionResult
{
    public override bool IsSuccess => false;
}