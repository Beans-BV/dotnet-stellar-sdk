using StellarDotnetSdk.Sep.Sep0006.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Exceptions;

/// <summary>
///     Exception thrown when the anchor requires additional customer information
///     before processing a transaction. The required fields can be submitted via SEP-12.
/// </summary>
public class CustomerInformationNeededException : TransferServerException
{
    public CustomerInformationNeededException(CustomerInformationNeededResponse response)
        : base(BuildMessage(response))
    {
        Response = response;
    }

    public CustomerInformationNeededResponse Response { get; }

    private static string BuildMessage(CustomerInformationNeededResponse response)
    {
        if (response.Fields == null || response.Fields.Count == 0)
        {
            return "The anchor needs more information about the customer. All information can be received non-interactively via SEP-12.";
        }

        var fieldsList = string.Join(", ", response.Fields);
        return $"The anchor needs more information about the customer and all the information can be received non-interactively via SEP-12. Fields: {fieldsList}";
    }
}

