using System;
using System.Linq;
using System.Text;

namespace StellarDotnetSdk.Exceptions;

/// <summary>
///     Thrown when the <see href="https://github.com/stellar/stellar-protocol/blob/master/core/cap-0085.md">CAP-85</see>
///     executable tag entry that an external executable reference points at cannot be used: it does not exist, or it
///     has been archived and must be restored before the reference resolves again.
/// </summary>
public class ExternalRefNotFoundException : Exception
{
    // Tags are unbounded byte strings, and the constructor accepts any owner string; keep the message readable and
    // bounded.
    private const int MaxBytesInMessage = 64;

    /// <summary>
    ///     Initializes a new instance for the tag entry of <paramref name="ownerContractId" /> keyed by
    ///     <paramref name="tag" />.
    /// </summary>
    /// <param name="ownerContractId">
    ///     The contract (C...) that was expected to hold the tag entry. The SDK always passes one; it may be
    ///     <see langword="null" /> only when the exception is constructed elsewhere.
    /// </param>
    /// <param name="tag">
    ///     The raw tag bytes that were looked up. They are copied. The SDK always passes them; they may be
    ///     <see langword="null" /> only when the exception is constructed elsewhere.
    /// </param>
    /// <param name="isArchived">
    ///     <see langword="true" /> when the entry exists but has been archived; <see langword="false" /> when it does
    ///     not exist.
    /// </param>
    public ExternalRefNotFoundException(string? ownerContractId, byte[]? tag, bool isArchived)
        : base(BuildMessage(ownerContractId, tag, isArchived))
    {
        OwnerContractId = ownerContractId;
        Tag = (byte[]?)tag?.Clone();
        IsArchived = isArchived;
    }

    /// <summary>
    ///     The contract (C...) that was expected to hold the tag entry, or <see langword="null" /> when the exception
    ///     was constructed without one. Never null when thrown by the SDK.
    /// </summary>
    public string? OwnerContractId { get; }

    /// <summary>
    ///     The raw tag bytes that were looked up, or <see langword="null" /> when the exception was constructed
    ///     without them. Never null when thrown by the SDK. The array is this instance's own copy.
    /// </summary>
    public byte[]? Tag { get; }

    /// <summary>
    ///     <see langword="true" /> when the tag entry exists but has been archived, so restoring its footprint makes the
    ///     reference resolvable again; <see langword="false" /> when the entry does not exist.
    /// </summary>
    public bool IsArchived { get; }

    private static string BuildMessage(string? ownerContractId, byte[]? tag, bool isArchived)
    {
        var state = isArchived
            ? "has been archived; restore its footprint to resolve the reference"
            : "was not found";
        return $"External executable tag entry {state}. Owner: {DescribeOwner(ownerContractId)}, tag: {DescribeTag(tag)}.";
    }

    // A tag need not be valid UTF-8, so a binary tag is shown as hex rather than as a lossy decode that would no
    // longer identify it. Only a tag made entirely of printable ASCII other than '"' and '\' is shown as text, in
    // quotes: anything else (control, format or invisible characters, look-alike quotes and letters, combining
    // marks) could disguise where the tag ends or what the message says, so it is shown as hex. The exact bytes are
    // always available from Tag.
    private static string DescribeTag(byte[]? tag)
    {
        return tag == null ? "null" : Describe(tag);
    }

    // The SDK passes a validated StrKey, but the constructor is public, so the owner gets the same treatment as a
    // tag: its UTF-8 bytes (an unpaired surrogate as EF BF BD) are shown as quoted text or as hex. The quotes keep an
    // owner such as "null", "" or "0x61" distinct from a null owner and from hex; the exact string is always
    // available from OwnerContractId.
    private static string DescribeOwner(string? ownerContractId)
    {
        return ownerContractId == null ? "null" : Describe(Encoding.UTF8.GetBytes(ownerContractId));
    }

    // Renders at most MaxBytesInMessage bytes, as quoted text when every byte (not only the shown ones) is plain
    // ASCII and as lowercase hex otherwise, followed by the full length when they were truncated.
    private static string Describe(byte[] bytes)
    {
        var shown = bytes.Length <= MaxBytesInMessage ? bytes : bytes.Take(MaxBytesInMessage).ToArray();
        var rendered = bytes.All(IsPlainAscii)
            ? $"\"{Encoding.ASCII.GetString(shown)}\""
            : "0x" + Util.BytesToHex(shown).ToLowerInvariant();
        return bytes.Length <= MaxBytesInMessage ? rendered : $"{rendered}... ({bytes.Length} bytes)";
    }

    private static bool IsPlainAscii(byte b)
    {
        return b is >= 0x20 and <= 0x7e && b != '"' && b != '\\';
    }
}
