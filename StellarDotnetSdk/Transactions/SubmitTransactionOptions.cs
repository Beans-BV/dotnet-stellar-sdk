namespace StellarDotnetSdk.Transactions;

/// <summary>
///     Options for controlling transaction submission behavior, such as skipping SEP-0029 memo checks or ensuring success
///     status codes.
/// </summary>
public class SubmitTransactionOptions
{
    public bool SkipMemoRequiredCheck { get; set; }
    public bool FeeBumpTransaction { get; set; }
    public bool EnsureSuccess { get; set; }
}