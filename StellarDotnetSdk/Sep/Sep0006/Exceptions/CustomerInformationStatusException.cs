using StellarDotnetSdk.Sep.Sep0006.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Exceptions;

/// <summary>
///     Exception thrown when customer information has been submitted but is either
///     still being processed (status: pending) or was not accepted (status: denied).
/// </summary>
public class CustomerInformationStatusException : TransferServerException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomerInformationStatusException" /> class
    ///     with the anchor's status response.
    /// </summary>
    /// <param name="response">The response containing the customer information processing status.</param>
    public CustomerInformationStatusException(CustomerInformationStatusResponse response)
        : base(BuildMessage(response))
    {
        Response = response;
    }

    /// <summary>
    ///     The anchor's response containing the customer information processing status (pending or denied).
    /// </summary>
    public CustomerInformationStatusResponse Response { get; }

    private static string BuildMessage(CustomerInformationStatusResponse response)
    {
        var status = response.Status ?? "unknown";
        var moreInfoUrl = response.MoreInfoUrl ?? "none";
        var eta = response.Eta.HasValue ? response.Eta.Value.ToString() : "unknown";

        return
            $"Customer information was submitted for the account, but the information is either still being processed or was not accepted. Status: {status} - More info url: {moreInfoUrl} - Eta: {eta}";
    }
}