namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the data_updated effect response.
///     This effect occurs when a data entry is updated on an account.
/// </summary>
public sealed class DataUpdatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 42;
}