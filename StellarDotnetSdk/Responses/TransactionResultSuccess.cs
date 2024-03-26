using System.Collections.Generic;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     All operations succeeded.
/// </summary>
public class TransactionResultSuccess : TransactionResult
{
    public TransactionResultSuccess(string feeCharged, List<OperationResult> results)
    {
        FeeCharged = feeCharged;
        Results = results;
    }

    public override bool IsSuccess => true;

    public List<OperationResult> Results { get; }
}