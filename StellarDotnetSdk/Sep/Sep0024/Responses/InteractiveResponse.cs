using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024.Responses;

/// <summary>
///     Response from deposit or withdraw endpoints containing interactive flow details.
///     This response provides the URL for the interactive web interface where the user
///     completes KYC, provides additional details, and receives instructions.
///     The URL should be displayed in a popup window or webview. The client should
///     poll the /transaction endpoint using the provided ID to monitor status changes.
/// </summary>
public class InteractiveResponse : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InteractiveResponse" /> class.
    /// </summary>
    /// <param name="type">Response type (always 'interactive_customer_info_needed').</param>
    /// <param name="url">URL for the interactive flow to display to user.</param>
    /// <param name="id">Anchor's internal ID for this transaction.</param>
    [JsonConstructor]
    public InteractiveResponse(string type, string url, string id)
    {
        Type = type;
        Url = url;
        Id = id;
    }

    /// <summary>
    ///     Gets the response type. Always set to 'interactive_customer_info_needed'.
    ///     Indicates that user interaction is required via the provided URL.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>
    ///     Gets the URL hosted by the anchor for the interactive flow.
    ///     Display this URL to the user in a popup window or webview.
    ///     The user will complete KYC and provide necessary details here.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; }

    /// <summary>
    ///     Gets the anchor's internal ID for this deposit or withdrawal request.
    ///     Use this ID to query the /transaction endpoint to check the status.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; }
}

