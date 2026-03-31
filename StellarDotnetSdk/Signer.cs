using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk;

/// <summary>
///     Represents an account signer with a signing key and associated weight.
///     Signers are used to authorize transactions for Stellar accounts, where
///     the weight contributes toward meeting the required signing thresholds.
/// </summary>
public class Signer
{
    /// <summary>
    ///     Initializes a new <see cref="Signer" /> with the given signing key and weight.
    /// </summary>
    /// <param name="key">The XDR signer key.</param>
    /// <param name="weight">The signer weight (0-255). A weight of 0 removes the signer.</param>
    public Signer(SignerKey key, uint weight)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key), "signer key cannot be null");
        if (weight > 255)
        {
            throw new ArgumentException("weight must be an integer between 0 and 255 (inclusive).", nameof(weight));
        }
        Key = key;

        Weight = weight;
    }

    /// <summary>Gets the XDR signer key.</summary>
    public SignerKey Key { get; }

    /// <summary>Gets the signer weight (0-255) that contributes toward meeting threshold requirements.</summary>
    public uint Weight { get; }

    /// <summary>
    ///     Converts this signer to its XDR representation.
    /// </summary>
    /// <returns>An XDR <see cref="Xdr.Signer" /> object.</returns>
    public Xdr.Signer ToXdr()
    {
        return new Xdr.Signer
        {
            Key = Key,
            Weight = new Uint32(Weight),
        };
    }

    /// <summary>
    ///     Creates a <see cref="Signer" /> from an XDR signer object.
    /// </summary>
    /// <param name="xdrSigner">The XDR signer to convert.</param>
    /// <returns>A new <see cref="Signer" /> instance.</returns>
    public static Signer FromXdr(Xdr.Signer xdrSigner)
    {
        return new Signer(xdrSigner.Key, xdrSigner.Weight.InnerValue);
    }
}