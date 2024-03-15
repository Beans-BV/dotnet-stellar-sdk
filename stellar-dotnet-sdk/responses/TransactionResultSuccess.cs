using System.Collections.Generic;
using stellar_dotnet_sdk.responses.results;

namespace stellar_dotnet_sdk.responses;

/// <summary>
///     All operations succeeded.
/// </summary>
public class TransactionResultSuccess : TransactionResult
{
    public TransactionResultSuccess(string feeCharged, ICollection<OperationResult> results)
    {
        FeeCharged = feeCharged;
        Results = results;
    }

    public override bool IsSuccess => true;

    public ICollection<OperationResult> Results { get; }
}