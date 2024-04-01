namespace StellarDotnetSdk.Responses;

/// <summary>
///     An unknown error occured.
/// </summary>
public class TransactionResultInternalError : TransactionResult
{
    public override bool IsSuccess => false;
}