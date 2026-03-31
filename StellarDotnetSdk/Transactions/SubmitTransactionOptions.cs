namespace StellarDotnetSdk.Transactions;

/// <summary>
///     Options for controlling transaction submission behavior, such as skipping SEP-0029 memo checks or ensuring success
///     status codes.
/// </summary>
public class SubmitTransactionOptions
{
    /// <summary>
    ///     Gets or sets whether to skip the SEP-0029 memo required check before submitting.
    /// </summary>
    public bool SkipMemoRequiredCheck { get; set; }

    /// <summary>
    ///     Gets or sets whether the transaction being submitted is a fee bump transaction.
    /// </summary>
    public bool FeeBumpTransaction { get; set; }

    /// <summary>
    ///     Gets or sets whether to throw an exception if the transaction submission does not succeed.
    /// </summary>
    public bool EnsureSuccess { get; set; }
}