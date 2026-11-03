using System;

namespace StellarDotnetSdk.Assets;

/// <summary>
///     Assets are uniquely identified by the asset code and the issuer. Ultimately, it’s up to the issuer to set the asset
///     code. By convention, however, currencies should be represented by
///     the appropriate ISO 4217 code. For stocks and bonds, use the appropriate ISIN number.
/// </summary>
public abstract class AssetTypeCreditAlphaNum : Asset
{
    /// <summary>
    ///     Creates an AssetTypeCreditAlphaNum based on the code and KeyPair
    /// </summary>
    /// <param name="code">The asset code.</param>
    /// <param name="issuer">The KeyPair of the issuer.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public AssetTypeCreditAlphaNum(string code, string issuer)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code), "code cannot be null");

        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer), "issuer cannot be null");
    }

    /// <summary>
    ///     Return the asset code
    /// </summary>
    public string Code { get; }

    /// <summary>
    ///     Return the asset issuer
    /// </summary>
    public string Issuer { get; }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        //see: https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        unchecked
        {
            // Choose large primes to avoid hashing collisions
            const int hashingBase = (int)2166136261;
            const int hashingMultiplier = 16777619;

            var hash = hashingBase;
            hash = (hash * hashingMultiplier) ^ (!ReferenceEquals(null, Code) ? Code.GetHashCode() : 0);
            hash = (hash * hashingMultiplier) ^ (!ReferenceEquals(null, Issuer) ? Issuer.GetHashCode() : 0);
            return hash;
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not AssetTypeCreditAlphaNum numObj)
        {
            return false;
        }

        return Code.Equals(numObj.Code) &&
               Issuer.Equals(numObj.Issuer);
    }

    /// <inheritdoc />
    public override string CanonicalName()
    {
        return $"{Code}:{Issuer}";
    }

    /// <summary>
    ///     Compares the code, then the issuer, in the order stellar-core uses for assets of the same type.
    /// </summary>
    /// <param name="other">An asset of the same type as this one.</param>
    private protected int CompareCodeAndIssuer(AssetTypeCreditAlphaNum other)
    {
        if (Code != other.Code)
        {
            // stellar-core compares the zero-padded code arrays. For valid (ASCII alphanumeric) codes that is the
            // same as ordinal order, since a shorter code's padding byte sorts before any character.
            return string.Compare(Code, other.Code, StringComparison.Ordinal);
        }

        if (Issuer == other.Issuer)
        {
            return 0;
        }

        // stellar-core compares the raw 32-byte keys. The strkey text sorts differently: base32 puts the digits
        // 2-7 after 'Z', but in ASCII they come before 'A'.
        return StrKey.DecodeEd25519PublicKey(Issuer).AsSpan()
            .SequenceCompareTo(StrKey.DecodeEd25519PublicKey(other.Issuer));
    }
}