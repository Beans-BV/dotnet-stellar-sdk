using System;
using System.Text.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Exceptions;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     Helpers shared by the hand-written asset converters (<see cref="AssetJsonConverter" />,
///     <see cref="AssetAmountJsonConverter" />, <see cref="ReserveJsonConverter" />,
///     <see cref="LiquidityPoolClaimableAssetAmountJsonConverter" />) that build an <see cref="Asset" />
///     from JSON values.
/// </summary>
/// <remarks>
///     <see cref="Asset.Create(string)" /> and <see cref="Asset.CreateNonNativeAsset(string, string)" />
///     validate the asset string/code and throw <see cref="ArgumentException" /> or
///     <see cref="AssetCodeLengthInvalidException" /> on invalid input. A converter must instead surface
///     deserialization failures as <see cref="JsonException" /> (the documented System.Text.Json failure
///     mode) so a caller can handle every malformed-response failure with a single
///     <c>catch (JsonException)</c>, so these helpers translate the factory's domain exceptions while
///     preserving the original as <see cref="Exception.InnerException" />.
/// </remarks>
internal static class AssetJsonReadHelper
{
    /// <summary>
    ///     Builds an <see cref="Asset" /> from its canonical string form, translating an invalid value
    ///     into a <see cref="JsonException" />.
    /// </summary>
    internal static Asset CreateAsset(string canonicalName)
    {
        try
        {
            return Asset.Create(canonicalName);
        }
        catch (Exception exception) when (exception is ArgumentException or AssetCodeLengthInvalidException)
        {
            throw new JsonException($"Invalid asset value '{canonicalName}'.", exception);
        }
    }

    /// <summary>
    ///     Builds a non-native <see cref="Asset" /> from a code/issuer pair, translating an invalid code
    ///     length into a <see cref="JsonException" />.
    /// </summary>
    internal static Asset CreateNonNativeAsset(string code, string issuer)
    {
        try
        {
            return Asset.CreateNonNativeAsset(code, issuer);
        }
        catch (Exception exception) when (exception is ArgumentException or AssetCodeLengthInvalidException)
        {
            throw new JsonException($"Invalid asset code '{code}' or issuer '{issuer}'.", exception);
        }
    }
}
