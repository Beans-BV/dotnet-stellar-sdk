namespace StellarDotnetSdk.Tests.Sep.Sep0007.Fixtures;

/// <summary>
///     Known-answer vectors for SEP-7. Sources: the SEP-7 v2.1.0 spec text (stellar-protocol,
///     ecosystem/sep-0007.md), the stellar/typescript-wallet-sdk test suite (test/sep7.test.ts), and the Soneso
///     Flutter/iOS SDK test suites.
/// </summary>
internal static class Sep7TestVectors
{
    // ---- SEP-7 spec ----

    /// <summary>Spec "Request Signing" example key.</summary>
    public const string SpecSigningSeed = "SBPOVRVKTTV7W3IOX2FJPSMPCJ5L2WU2YKTP3HCLYPXNI5MDIGREVNYC";

    public const string SpecSigningAccountId = "GD7ACHBPHSC5OJMJZZBXA7Z5IAUFTH6E6XVLNBPASDQYJ7LO5UIYBDQW";

    public const string SpecUnsignedPayUri =
        "web+stellar:pay?destination=GCALNQQBXAPZ2WIRSDDBMSTAKCUH5SG6U76YBFLQLIXJTF7FE5AX7AOO&amount=120.1234567" +
        "&memo=skdjfasf&memo_type=MEMO_TEXT&msg=pay%20me%20with%20lumens&origin_domain=someDomain.com";

    public const string SpecSignature =
        "tbsLtlK/fouvRWk2UWFP47yHYeI1g1NEC/fEQvuXG6V8P+beLxplYbOVtTk1g94Wp97cHZ3pVJy/tZNYobl3Cw==";

    public const string SpecSignedPayUri = SpecUnsignedPayUri +
                                           "&signature=tbsLtlK%2FfouvRWk2UWFP47yHYeI1g1NEC%2FfEQvuXG6V8P%2BbeLxplYbOVtTk1g94Wp97cHZ3pVJy%2FtZNYobl3Cw%3D%3D";

    /// <summary>The (decoded) transaction envelope of the spec's tx examples: a CHANGE_TRUST for HUG.</summary>
    public const string SpecTxXdr =
        "AAAAAP+yw+ZEuNg533pUmwlYxfrq6/BoMJqiJ8vuQhf6rHWmAAAAZAB8NHAAAAABAAAAAAAAAAAAAAABAAAAAAAAAAYAAAABSFVHAAAAAABAH0wIyY3BJBS2qHdRPAV80M8hF7NBpxRjXyjuT9kEbH//////////AAAAAAAAAAA=";

    public const string SpecTxExample1 =
        "web+stellar:tx?xdr=AAAAAP%2Byw%2BZEuNg533pUmwlYxfrq6%2FBoMJqiJ8vuQhf6rHWmAAAAZAB8NHAAAAABAAAAAAAAAAAAAAABAAAAAAAAAAYAAAABSFVHAAAAAABAH0wIyY3BJBS2qHdRPAV80M8hF7NBpxRjXyjuT9kEbH%2F%2F%2F%2F%2F%2F%2F%2F%2F%2FAAAAAAAAAAA%3D" +
        "&callback=url%3Ahttps%3A%2F%2FsomeSigningService.com%2Fa8f7asdfkjha" +
        "&pubkey=GAU2ZSYYEYO5S5ZQSMMUENJ2TANY4FPXYGGIMU6GMGKTNVDG5QYFW6JS&msg=order%20number%2024";

    public const string SpecTxExample2 =
        "web+stellar:tx?xdr=AAAAAP%2Byw%2BZEuNg533pUmwlYxfrq6%2FBoMJqiJ8vuQhf6rHWmAAAAZAB8NHAAAAABAAAAAAAAAAAAAAABAAAAAAAAAAYAAAABSFVHAAAAAABAH0wIyY3BJBS2qHdRPAV80M8hF7NBpxRjXyjuT9kEbH%2F%2F%2F%2F%2F%2F%2F%2F%2F%2FAAAAAAAAAAA%3D" +
        "&replace=sourceAccount%3AX%3BX%3Aaccount%20on%20which%20to%20create%20the%20trustline";

    public const string SpecPayExample1 =
        "web+stellar:pay?destination=GCALNQQBXAPZ2WIRSDDBMSTAKCUH5SG6U76YBFLQLIXJTF7FE5AX7AOO&amount=120.1234567" +
        "&memo=skdjfasf&memo_type=MEMO_TEXT&msg=pay%20me%20with%20lumens";

    public const string SpecPayExample2 =
        "web+stellar:pay?destination=GCALNQQBXAPZ2WIRSDDBMSTAKCUH5SG6U76YBFLQLIXJTF7FE5AX7AOO&amount=120.123" +
        "&asset_code=USD&asset_issuer=GCRCUE2C5TBNIPYHMEP7NK5RWTT2WBSZ75CMARH7GDOHDDCQH3XANFOB" +
        "&memo=hasysda987fs&memo_type=MEMO_TEXT" +
        "&callback=url%3Ahttps%3A%2F%2FsomeSigningService.com%2Fhasysda987fs%3Fasset%3DUSD";

    /// <summary>The spec's replace example, URL-decoded.</summary>
    public const string SpecReplace =
        "sourceAccount:X,operations[0].sourceAccount:Y,operations[1].destination:Y;" +
        "X:account from where you want to pay fees," +
        "Y:account that needs the trustline and which will receive the new tokens";

    /// <summary>The spec's replace example, URL-encoded as the spec prints it.</summary>
    public const string SpecReplaceEncoded =
        "sourceAccount%3AX%2Coperations%5B0%5D.sourceAccount%3AY%2Coperations%5B1%5D.destination%3AY%3BX%3Aaccount%20from%20where%20you%20want%20to%20pay%20fees%2CY%3Aaccount%20that%20needs%20the%20trustline%20and%20which%20will%20receive%20the%20new%20tokens";

    public const string SpecReplaceHintX = "account from where you want to pay fees";

    public const string SpecReplaceHintY = "account that needs the trustline and which will receive the new tokens";

    // ---- typescript-wallet-sdk (test/sep7.test.ts) ----

    public const string TsSigningSeed = "SBKQDF56C5VY2YQTNQFGY7HM6R3V6QKDUEDXZQUCPQOP2EBZWG2QJ2JL";

    public const string TsSigningAccountId = "GCTPTXLPGWHV35RJZKTGQPDI2E3T34YBOBKPOJMGYX6NXV5QJS5QOVMN";

    public const string TsUnsignedPayUri =
        "web+stellar:pay?destination=GCALNQQBXAPZ2WIRSDDBMSTAKCUH5SG6U76YBFLQLIXJTF7FE5AX7AOO&amount=120.1234567" +
        "&memo=skdjfasf&msg=pay%20me%20with%20lumens&origin_domain=someDomain.com";

    public const string TsSignedPayUri = TsUnsignedPayUri +
                                         "&signature=juY2Pi1%2FIubcbIDds2CbnL%2BImr7dbpJYMW1nLAesOmyh5v%2FuTVvJwI06RgCGBtHh5%2B5DWOhJUlEfOSGXPtqgAA%3D%3D";

    public const string TsMuxedDestination = "MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAABUTGI4";

    // ---- Soneso Flutter / iOS SDKs ----

    public const string SonesoAccountId = "GDGUF4SCNINRDCRUIVOMDYGIMXOWVP3ZLMTL2OGQIWMFDDSECZSFQMQV";

    public const string SonesoSeed = "SBA2XQ5SRUW5H3FUQARMC6QYEPUYNSVCMM4PGESGVB2UIFHLM73TPXXF";

    /// <summary>The key the Soneso "signature mismatch" stellar.toml mock publishes.</summary>
    public const string SonesoMismatchAccountId = "GCCHBLJOZUFBVAUZP55N7ZU6ZB5VGEDHSXT23QC6UIVDQNGI6QDQTOOR";

    public const string SonesoAssetIssuer = "GC4HC3AXQDNAMURMHVGMLFGLQELEQBCE4GI7IOKEAWAKBXY7SXXWBTLV";

    public const string SonesoExpectedPayUri =
        "web+stellar:pay?destination=GDGUF4SCNINRDCRUIVOMDYGIMXOWVP3ZLMTL2OGQIWMFDDSECZSFQMQV&amount=123.21" +
        "&asset_code=ANA&asset_issuer=GC4HC3AXQDNAMURMHVGMLFGLQELEQBCE4GI7IOKEAWAKBXY7SXXWBTLV";
}
