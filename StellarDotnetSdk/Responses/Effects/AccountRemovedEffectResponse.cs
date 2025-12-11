namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the account_removed effect response.
///     This effect occurs when an account is merged into another account.
/// </summary>
public sealed class AccountRemovedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 1;
}