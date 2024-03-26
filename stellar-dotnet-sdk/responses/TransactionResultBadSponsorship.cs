namespace stellar_dotnet_sdk.responses;

/// <summary>
///     The sponsorship is not confirmed.
/// </summary>
public class TransactionResultBadSponsorship : TransactionResult
{
    public override bool IsSuccess => false;
}