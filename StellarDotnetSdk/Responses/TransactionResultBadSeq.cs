namespace StellarDotnetSdk.Responses;

/// <summary>
///     Sequence number does not match source account.
/// </summary>
public class TransactionResultBadSeq : TransactionResult
{
    public override bool IsSuccess => false;
}