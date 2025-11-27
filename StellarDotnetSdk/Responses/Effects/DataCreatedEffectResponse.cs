namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the data_created effect response.
///     This effect occurs when a data entry is created on an account.
/// </summary>
public sealed class DataCreatedEffectResponse : EffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 40;
}