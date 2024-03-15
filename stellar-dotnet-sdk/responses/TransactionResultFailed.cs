using System.Collections.Generic;
using stellar_dotnet_sdk.responses.results;

namespace stellar_dotnet_sdk.responses;

/// <summary>
///     One of the operations failed (none were applied).
/// </summary>
public class TransactionResultFailed : TransactionResult
{
    public TransactionResultFailed(string feeCharged, IList<OperationResult> results)
    {
        FeeCharged = feeCharged;
        Results = results;
    }

    public override bool IsSuccess => false;

    public IList<OperationResult> Results { get; }
}