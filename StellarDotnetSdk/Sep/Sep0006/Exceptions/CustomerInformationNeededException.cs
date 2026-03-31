using StellarDotnetSdk.Sep.Sep0006.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Exceptions;

/// <summary>
///     Exception thrown when the anchor requires additional customer information
///     before processing a transaction. The required fields can be submitted via SEP-12.
/// </summary>
public class CustomerInformationNeededException : TransferServerException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomerInformationNeededException" /> class
    ///     with the anchor's response detailing the required fields.
    /// </summary>
    /// <param name="response">The response containing the list of required customer information fields.</param>
    public CustomerInformationNeededException(CustomerInformationNeededResponse response)
        : base(BuildMessage(response))
    {
        Response = response;
    }

    /// <summary>
    ///     The anchor's response indicating which customer information fields are required.
    /// </summary>
    public CustomerInformationNeededResponse Response { get; }

    private static string BuildMessage(CustomerInformationNeededResponse response)
    {
        if (response.Fields == null || response.Fields.Count == 0)
        {
            return
                "The anchor needs more information about the customer. All information can be received non-interactively via SEP-12.";
        }

        var fieldsList = string.Join(", ", response.Fields);
        return
            $"The anchor needs more information about the customer and all the information can be received non-interactively via SEP-12. Fields: {fieldsList}";
    }
}