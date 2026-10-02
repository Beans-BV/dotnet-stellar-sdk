using System;

namespace StellarDotnetSdk.Sep.Sep0007;

/// <summary>
///     One entry of a SEP-7 <c>replace</c> parameter: a SEP-11 Txrep field of the transaction that the wallet
///     should fill in, the reference identifier that groups fields taking the same value, and the hint shown to
///     the user for that identifier.
/// </summary>
public sealed class Sep7Replacement : IEquatable<Sep7Replacement>
{
    /// <summary>Initializes a new instance of the <see cref="Sep7Replacement" /> class.</summary>
    /// <param name="id">The reference identifier, e.g. <c>X</c>.</param>
    /// <param name="path">
    ///     The Txrep path of the field without the <c>tx.</c> prefix, e.g. <c>sourceAccount</c> or
    ///     <c>operations[0].sourceAccount</c>.
    /// </param>
    /// <param name="hint">The hint shown to the user for <paramref name="id" />.</param>
    public Sep7Replacement(string id, string path, string hint)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Hint = hint ?? throw new ArgumentNullException(nameof(hint));
    }

    /// <summary>The reference identifier, e.g. <c>X</c>.</summary>
    public string Id { get; }

    /// <summary>The Txrep path of the field without the <c>tx.</c> prefix.</summary>
    public string Path { get; }

    /// <summary>The hint shown to the user for <see cref="Id" />.</summary>
    public string Hint { get; }

    /// <inheritdoc />
    public bool Equals(Sep7Replacement? other)
    {
        return other is not null && Id == other.Id && Path == other.Path && Hint == other.Hint;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as Sep7Replacement);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Id.GetHashCode();
            hash = hash * 397 ^ Path.GetHashCode();
            return hash * 397 ^ Hint.GetHashCode();
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Path}:{Id} ({Hint})";
    }
}
