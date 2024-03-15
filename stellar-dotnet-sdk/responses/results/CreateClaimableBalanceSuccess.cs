using System;

namespace stellar_dotnet_sdk.responses.results;

/// <summary>
///     Create claimable balance success.
/// </summary>
public class CreateClaimableBalanceSuccess : CreateClaimableBalanceResult
{
    public CreateClaimableBalanceSuccess(byte[] balanceId)
    {
        BalanceId = Convert.ToBase64String(balanceId);
    } 
    
    public override bool IsSuccess => true;
    public string BalanceId { get; }
}