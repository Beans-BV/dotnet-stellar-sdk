using System.Collections.Generic;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Responses;

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