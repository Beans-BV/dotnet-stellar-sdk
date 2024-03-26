namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Operation successful.
/// </summary>
public class PaymentSuccess : PaymentResult
{
    public override bool IsSuccess => true;
}