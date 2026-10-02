namespace StellarDotnetSdk.Sep.Sep0007;

/// <summary>The values SEP-7 allows for the <c>memo_type</c> parameter of a <c>pay</c> request.</summary>
public static class Sep7MemoType
{
    /// <summary>A UTF-8 text memo of at most 28 bytes, sent URL-encoded.</summary>
    public const string MemoText = "MEMO_TEXT";

    /// <summary>An unsigned 64-bit integer memo, sent as its decimal representation.</summary>
    public const string MemoId = "MEMO_ID";

    /// <summary>A 32-byte hash memo, sent base64-encoded and then URL-encoded.</summary>
    public const string MemoHash = "MEMO_HASH";

    /// <summary>A 32-byte return-hash memo, sent base64-encoded and then URL-encoded.</summary>
    public const string MemoReturn = "MEMO_RETURN";
}
