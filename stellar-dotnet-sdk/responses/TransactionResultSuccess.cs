using System.Collections.Generic;
using stellar_dotnet_sdk.responses.results;

namespace stellar_dotnet_sdk.responses
{
    public class TransactionResultSuccess : TransactionResult
    {
        public override bool IsSuccess => true;

        public ICollection<OperationResult> Results { get; set; }
    }
}