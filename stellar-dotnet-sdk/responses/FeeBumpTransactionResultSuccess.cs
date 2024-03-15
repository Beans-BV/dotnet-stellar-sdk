namespace stellar_dotnet_sdk.responses;

/// <summary>
///     All operations in the inner transaction succeeded.
/// </summary>
public class FeeBumpTransactionResultSuccess : TransactionResult
{
    public FeeBumpTransactionResultSuccess(string feeCharged, InnerTransactionResultPair innerResultPair)
    {
        FeeCharged = feeCharged;
        InnerResultPair = innerResultPair;
    }

    public override bool IsSuccess => true;

    // TODO: Unit test
    public InnerTransactionResultPair InnerResultPair { get; }
}