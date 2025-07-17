using System;
using StellarDotnetSdk.Xdr;
using ClaimableVersion = StellarDotnetSdk.Xdr.ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum;

namespace StellarDotnetSdk;

public static class ClaimableBalanceIdUtils
{
    internal static ClaimableBalanceID ToXdr(string claimableBalanceId)
    {
        var decoded = StrKey.DecodeClaimableBalanceId(claimableBalanceId);
        // The first byte is the version
        var version = decoded[0];

        // So, the raw ID should not contain the version byte
        var rawId = decoded[1..];
        return version switch
        {
            0 => new ClaimableBalanceID
            {
                Discriminant = ClaimableBalanceIDType.Create(
                    ClaimableVersion.CLAIMABLE_BALANCE_ID_TYPE_V0),
                V0 = new Hash(rawId),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(version),
                $"Claimable balance ID version {version} is not supported."),
        };
    }

    /// <summary>
    ///     Converts from a <see cref="Xdr.ClaimableBalanceID">Xdr.ClaimableBalanceID</see>
    ///     to claimable balance ID (B...).
    /// </summary>
    /// <param name="xdr">An <see cref="Xdr.ClaimableBalanceID">Xdr.ClaimableBalanceID</see> object.</param>
    /// <returns>A base32-encoded claimable balance ID (B...).</returns>
    public static string FromXdr(ClaimableBalanceID xdr)
    {
        var version = xdr.Discriminant.InnerValue;
        var rawId = version switch
        {
            ClaimableVersion.CLAIMABLE_BALANCE_ID_TYPE_V0 => xdr.V0.InnerValue,
            _ => throw new ArgumentOutOfRangeException(nameof(version),
                $"Claimable balance ID version {version} is not supported."),
        };
        // The actual ID contains the version as the first byte
        byte[] fullId = [(byte)version, .. rawId];

        return StrKey.EncodeClaimableBalanceId(fullId);
    }

    /// <summary>
    ///     Converts a base32 encoded claimable balance ID (B...) to hex format (0000...).
    ///     The hex ID can then be used in Horizon or RPC endpoints.
    /// </summary>
    /// <param name="base32Id">A base32-encoded claimable balance ID (B...).</param>
    /// <returns>A hex-encoded claimable balance ID.</returns>
    public static string ToHexString(string base32Id)
    {
        var xdr = ToXdr(base32Id);
        return ToHexString(xdr);
    }

    /// <summary>
    ///     Converts a hex-encoded claimable balance ID (0000...) to base32 format (B...).
    /// </summary>
    /// <param name="hex">A hex-encoded claimable balance ID (0000...).</param>
    /// <returns>A base32-encoded claimable balance ID.</returns>
    public static string ToBase32String(string hex)
    {
        var xdr = FromHexString(hex);
        return FromXdr(xdr);
    }

    internal static string ToHexString(ClaimableBalanceID xdr)
    {
        var os = new XdrDataOutputStream();
        ClaimableBalanceID.Encode(os, xdr);
        return Convert.ToHexString(os.ToArray());
    }

    internal static ClaimableBalanceID FromHexString(string hex)
    {
        try
        {
            var inputStream = new XdrDataInputStream(Convert.FromHexString(hex));
            return ClaimableBalanceID.Decode(inputStream);
        }
        catch
        {
            throw new ArgumentException($"Invalid claimable balance ID {hex}.");
        }
    }
}