using System.Linq;
using FsCheck;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Tests.Generators;

public static class KeyPairGenerator
{
    public static Arbitrary<KeyPair> Generate()
    {
        return Gen.ListOf(32, Arb.Default.Byte().Generator)
            .Select(seed => KeyPair.FromSecretSeed(seed.ToArray())).ToArbitrary();
    }
}