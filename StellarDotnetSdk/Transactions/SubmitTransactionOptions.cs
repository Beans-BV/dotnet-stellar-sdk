namespace StellarDotnetSdk.Transactions;

public class SubmitTransactionOptions
{
    public bool SkipMemoRequiredCheck { get; set; }
    public bool FeeBumpTransaction { get; set; }
    public bool EnsureSuccess { get; set; }
}