namespace stellar_dotnet_sdk.responses.results;

/// <summary>
///     Sponsorship cannot be removed from this ledgerEntry. This error will happen if the user tries to remove the
///     sponsorship from a ClaimableBalanceEntry.
/// </summary>
public class RevokeSponsorshipOnlyTransferable : RevokeSponsorshipResult
{
}