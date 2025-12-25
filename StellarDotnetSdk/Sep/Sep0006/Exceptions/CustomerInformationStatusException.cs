using StellarDotnetSdk.Sep.Sep0006.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Exceptions;

/// <summary>
///     Exception thrown when customer information has been submitted but is either
///     still being processed (status: pending) or was not accepted (status: denied).
/// </summary>
public class CustomerInformationStatusException : TransferServerException
{
    public CustomerInformationStatusException(CustomerInformationStatusResponse response)
        : base(BuildMessage(response))
    {
        Response = response;
    }

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