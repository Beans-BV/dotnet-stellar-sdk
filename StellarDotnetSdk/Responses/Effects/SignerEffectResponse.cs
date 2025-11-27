using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Base class for signer-related effect responses.
/// </summary>
public abstract class SignerEffectResponse : EffectResponse
{
    /// <summary>
    ///     The weight of the signer (0-255).
    /// </summary>
    [JsonPropertyName("weight")]
    public int Weight { get; init; }

    /// <summary>
    ///     The public key of the signer.
    /// </summary>
    [JsonPropertyName("public_key")]
    public string? PublicKey { get; init; }
}

/// <summary>
///     Represents the signer_removed effect response.
///     This effect occurs when a signer is removed from an account.
/// </summary>
public sealed class SignerRemovedEffectResponse : SignerEffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 11;
}

/// <summary>
///     Represents the signer_updated effect response.
///     This effect occurs when a signer's weight is changed.
/// </summary>
public sealed class SignerUpdatedEffectResponse : SignerEffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 12;
}

/// <summary>
///     Represents the signer_created effect response.
///     This effect occurs when a signer is added to an account.
/// </summary>
public sealed class SignerCreatedEffectResponse : SignerEffectResponse
{
    /// <inheritdoc />
    public override int TypeId => 10;
}