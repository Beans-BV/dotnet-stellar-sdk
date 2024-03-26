using System.Linq;
using FsCheck;
using stellar_dotnet_sdk;

namespace stellar_dotnet_sdk_test.Generators;

public static class KeyPairGenerator
{
    public static Arbitrary<KeyPair> Generate()
    {
        return Gen.ListOf(32, Arb.Default.Byte().Generator)
            .Select(seed => KeyPair.FromSecretSeed(seed.ToArray())).ToArbitrary();
    }
}