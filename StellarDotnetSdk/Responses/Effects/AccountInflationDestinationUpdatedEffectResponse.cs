namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the account_inflation_destination_updated effect response.
///     This effect occurs when an account's inflation destination is changed.
///     Note: Inflation is no longer active on the Stellar network.
/// </summary>
public sealed class AccountInflationDestinationUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 7;
}