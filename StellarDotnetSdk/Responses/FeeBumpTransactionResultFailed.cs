namespace StellarDotnetSdk.Responses;

/// <summary>
///     One of the operations in the inner transaction failed (none were applied).
/// </summary>
public class FeeBumpTransactionResultFailed : TransactionResult
{
    public FeeBumpTransactionResultFailed(string feeCharged, InnerTransactionResultPair innerResultPair)
    {
        FeeCharged = feeCharged;
        InnerResultPair = innerResultPair;
    }

    public override bool IsSuccess => false;

    public InnerTransactionResultPair InnerResultPair { get; }
}