using System;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Shared helpers for the Soroban authorization tests. Compares addresses through the SDK-internal
///     <see cref="ScAddressExtensions.ToXdrByteArray" /> (visible via InternalsVisibleTo), so the tests
///     do not re-implement the canonical XDR-byte ordering they verify.
/// </summary>
internal static class SorobanAuthTestHelpers
{
    /// <summary>
    ///     Compares two addresses by their canonical XDR byte encoding, the order CAP-71 delegate arrays
    ///     must follow. Negative if <paramref name="a" /> sorts before <paramref name="b" />.
    /// </summary>
    public static int CompareAddressXdr(ScAddress a, ScAddress b)
    {
        return a.ToXdrByteArray().AsSpan().SequenceCompareTo(b.ToXdrByteArray());
    }
}

/// <summary>
///     A <see cref="SorobanCredentials" /> subtype the SDK does not recognize, used to exercise the
///     "unknown credential type" fallbacks in the signing-policy switches.
/// </summary>
internal sealed class UnknownCredentials : SorobanCredentials
{
    public override StellarDotnetSdk.Xdr.SorobanCredentials ToXdr()
    {
        throw new NotImplementedException();
    }
}
