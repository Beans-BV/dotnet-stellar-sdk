using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StellarDotnetSdk.MauiValidation.Tests;

/// <summary>
///     Unit tests for <see cref="StreamSearch" />.
/// </summary>
[TestClass]
public class StreamSearchTest
{
    // The start of an SSE event's id line, as the ledger stream check looks for it.
    private const string Needle = "\nid: ";

    /// <summary>
    ///     Verifies that a needle in the stream is found.
    /// </summary>
    [TestMethod]
    public async Task ReadUntilAsync_NeedleInStream_ReturnsTrue()
    {
        // Arrange
        using var stream = new ChunkedStream("event: ledger\nid: 42\ndata: {}\n\n");

        // Act
        var isFound = await StreamSearch.ReadUntilAsync(stream, Needle, null, CancellationToken.None);

        // Assert
        Assert.IsTrue(isFound);
    }

    /// <summary>
    ///     Verifies that a stream that ends without the needle is reported as such.
    /// </summary>
    [TestMethod]
    public async Task ReadUntilAsync_StreamEndsWithoutNeedle_ReturnsFalse()
    {
        // Arrange
        using var stream = new ChunkedStream("event: ledger\n", "data: {}\n\n");

        // Act
        var isFound = await StreamSearch.ReadUntilAsync(stream, Needle, null, CancellationToken.None);

        // Assert
        Assert.IsFalse(isFound);
    }

    /// <summary>
    ///     Verifies that a needle spread over several reads is found. Every read here returns one byte.
    /// </summary>
    [TestMethod]
    public async Task ReadUntilAsync_NeedleSpreadOverReads_ReturnsTrue()
    {
        // Arrange
        using var stream = new ChunkedStream("data: {}\nid: 42\n".Select(c => c.ToString()).ToArray());

        // Act
        var isFound = await StreamSearch.ReadUntilAsync(stream, Needle, null, CancellationToken.None);

        // Assert
        Assert.IsTrue(isFound);
    }

    /// <summary>
    ///     Verifies that reading stops at the read that brings the needle: an SSE stream does not end on its own.
    /// </summary>
    [TestMethod]
    public async Task ReadUntilAsync_NeedleInFirstRead_ReadsNoFurther()
    {
        // Arrange
        using var stream = new ChunkedStream("data: {}\nid: 42\n", "data: {}\n");

        // Act
        await StreamSearch.ReadUntilAsync(stream, Needle, null, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, stream.ReadCount);
    }

    /// <summary>
    ///     Verifies that the first-bytes callback runs once, right after the first read.
    /// </summary>
    [TestMethod]
    public async Task ReadUntilAsync_SeveralReads_CallsOnFirstBytesOnceAfterFirstRead()
    {
        // Arrange
        using var stream = new ChunkedStream("event: ", "ledger\n", "data: {}\n\n");
        var readCountsAtCalls = new List<int>();

        // Act
        await StreamSearch.ReadUntilAsync(stream, Needle, () => readCountsAtCalls.Add(stream.ReadCount),
            CancellationToken.None);

        // Assert
        CollectionAssert.AreEqual(new[] { 1 }, readCountsAtCalls);
    }

    /// <summary>
    ///     Verifies that the first-bytes callback does not run for a stream that ends without any bytes.
    /// </summary>
    [TestMethod]
    public async Task ReadUntilAsync_EmptyStream_DoesNotCallOnFirstBytes()
    {
        // Arrange
        using var stream = new ChunkedStream();
        var calls = 0;

        // Act
        await StreamSearch.ReadUntilAsync(stream, Needle, () => calls++, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, calls);
    }

    /// <summary>A read-only stream that returns one chunk of text per read, then the end of the stream.</summary>
    private sealed class ChunkedStream(params string[] chunks) : Stream
    {
        private readonly Queue<byte[]> _chunks = new(chunks.Select(chunk => Encoding.UTF8.GetBytes(chunk)));

        /// <summary>The number of reads, including one that found the end of the stream.</summary>
        public int ReadCount { get; private set; }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsSpan(offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            ReadCount++;
            if (!_chunks.TryDequeue(out var chunk))
            {
                return 0;
            }
            chunk.CopyTo(buffer);
            return chunk.Length;
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(Read(buffer.Span));
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
