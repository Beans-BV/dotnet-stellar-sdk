using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Xdr;
using FormatException = System.FormatException;
using Memo = StellarDotnetSdk.Memos.Memo;

namespace StellarDotnetSdk.Tests;

/// <summary>
/// Unit tests for <see cref="Memo"/> class.
/// </summary>
[TestClass]
public class MemoTest
{
    /// <summary>
    /// Verifies that Memo.None creates a memo with MEMO_NONE type.
    /// </summary>
    [TestMethod]
    public void None_CreatesMemo_WithMemoNoneType()
    {
        // Act
        var memo = Memo.None();

        // Assert
        Assert.AreEqual(MemoType.MemoTypeEnum.MEMO_NONE, memo.ToXdr().Discriminant.InnerValue);
    }

    /// <summary>
    /// Verifies that Memo.Text creates a memo with MEMO_TEXT type and correct value.
    /// </summary>
    [TestMethod]
    public void Text_WithValidString_CreatesMemoWithTextType()
    {
        // Act
        var memo = Memo.Text("test");

        // Assert
        Assert.AreEqual(MemoType.MemoTypeEnum.MEMO_TEXT, memo.ToXdr().Discriminant.InnerValue);
        Assert.AreEqual("test", memo.MemoTextValue);
    }

    /// <summary>
    /// Verifies that Memo.Text correctly handles UTF-8 characters.
    /// </summary>
    [TestMethod]
    public void Text_WithUtf8Character_CreatesMemoWithCorrectValue()
    {
        // Act
        var memo = Memo.Text("三");

        // Assert
        Assert.AreEqual(MemoType.MemoTypeEnum.MEMO_TEXT, memo.ToXdr().Discriminant.InnerValue);
        Assert.AreEqual("三", memo.MemoTextValue);
    }

    /// <summary>
    /// Verifies that Memo.Text throws ArgumentNullException when string is null.
    /// </summary>
    [TestMethod]
    public void Text_WithNullString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => { Memo.Text((string)null); });
    }

    /// <summary>
    /// Verifies that Memo.Text throws exception when string exceeds 28 bytes.
    /// </summary>
    [TestMethod]
    public void Text_WithStringTooLong_ThrowsException()
    {
        // Arrange
        const string tooLongString = "12345678901234567890123456789";

        // Act & Assert
        try
        {
            Memo.Text(tooLongString);
            Assert.Fail();
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.Message.Contains("text must be <= 28 bytes."));
        }
    }

    /// <summary>
    /// Verifies that Memo.Text throws ArgumentNullException when byte array is null.
    /// </summary>
    [TestMethod]
    public void Text_WithNullByteArray_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => { Memo.Text((byte[])null); });
    }

    /// <summary>
    /// Verifies that Memo.Text throws MemoTooLongException when byte array exceeds 28 bytes.
    /// </summary>
    [TestMethod]
    public void Text_WithByteArrayTooLong_ThrowsMemoTooLongException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<MemoTooLongException>(() =>
        {
            var bytes = Encoding.UTF8.GetBytes("12345678901234567890123456789");
            Memo.Text(bytes);
        });
    }

    /// <summary>
    /// Verifies that Memo.Text handles UTF-8 characters correctly even when they result in longer byte representation.
    /// </summary>
    [TestMethod]
    public void Text_WithUtf8Characters_HandlesCorrectly()
    {
        Memo.Text("\"<r~=Zp8yi");
    }

    /// <summary>
    /// Verifies that Memo.Id creates a memo with MEMO_ID type and correct ID value.
    /// </summary>
    [TestMethod]
    public void Id_WithValidId_CreatesMemoWithIdType()
    {
        // Act
        var memo = Memo.Id(9223372036854775807L);

        // Assert
        Assert.AreEqual(9223372036854775807UL, memo.IdValue);
        Assert.AreEqual(MemoType.MemoTypeEnum.MEMO_ID, memo.ToXdr().Discriminant.InnerValue);
        Assert.AreEqual(9223372036854775807UL, memo.ToXdr().Id.InnerValue);
    }

    /// <summary>
    /// Verifies that Memo.Hash creates a memo with MEMO_HASH type and correct hash value from hex string.
    /// </summary>
    [TestMethod]
    public void Hash_WithValidHexString_CreatesMemoWithHashType()
    {
        // Act
        var memo = Memo.Hash("4142434445464748494a4b4c");

        // Assert
        Assert.AreEqual(MemoType.MemoTypeEnum.MEMO_HASH, memo.ToXdr().Discriminant.InnerValue);
        var test = "ABCDEFGHIJKL";
        Assert.AreEqual(test, Util.PaddedByteArrayToString(memo.MemoBytes));
        Assert.AreEqual("4142434445464748494a4b4c", memo.GetTrimmedHexValue());
    }

    /// <summary>
    /// Verifies that Memo.Hash creates a memo with MEMO_HASH type and correct hash value from byte array.
    /// </summary>
    [TestMethod]
    public void Hash_WithValidByteArray_CreatesMemoWithHashType()
    {
        // Arrange
        var bytes = Enumerable.Repeat((byte)'A', 10).ToArray();

        // Act
        var memo = Memo.Hash(bytes);

        // Assert
        Assert.AreEqual(MemoType.MemoTypeEnum.MEMO_HASH, memo.ToXdr().Discriminant.InnerValue);
        Assert.AreEqual("AAAAAAAAAA", Util.PaddedByteArrayToString(memo.MemoBytes));
        Assert.AreEqual("4141414141414141414100000000000000000000000000000000000000000000", memo.GetHexValue());
        Assert.AreEqual("41414141414141414141", memo.GetTrimmedHexValue());
    }

    /// <summary>
    /// Verifies that Memo.Hash throws MemoTooLongException when byte array exceeds 32 bytes.
    /// </summary>
    [TestMethod]
    public void Hash_WithByteArrayTooLong_ThrowsMemoTooLongException()
    {
        // Arrange
        var longer = Enumerable.Repeat((byte)0, 33).ToArray();

        // Act & Assert
        try
        {
            Memo.Hash(longer);
            Assert.Fail("Expected MemoTooLongException was not thrown.");
        }
        catch (MemoTooLongException exception)
        {
            Assert.IsTrue(exception.Message.Contains("MEMO_HASH can contain 32 bytes at max."));
        }
    }

    /// <summary>
    /// Verifies that Memo.Hash throws FormatException when hex string is invalid.
    /// </summary>
    [TestMethod]
    public void Hash_WithInvalidHexString_ThrowsFormatException()
    {
        // Arrange
        const string invalidHex = "test";

        // Act & Assert
        try
        {
            Memo.Hash(invalidHex);
            Assert.Fail("Expected FormatException was not thrown.");
        }
        catch (FormatException)
        {
            // Expected exception
        }
    }

    /// <summary>
    /// Verifies that Memo.ReturnHash creates a memo with MEMO_RETURN type and correct return hash value.
    /// </summary>
    [TestMethod]
    public void ReturnHash_WithValidHexString_CreatesMemoWithReturnHashType()
    {
        // Act
        var memo = Memo.ReturnHash("4142434445464748494a4b4c");
        var memoXdr = memo.ToXdr();

        // Assert
        Assert.AreEqual(MemoType.MemoTypeEnum.MEMO_RETURN, memoXdr.Discriminant.InnerValue);
        Assert.IsNull(memoXdr.Hash);
        Assert.AreEqual("4142434445464748494a4b4c0000000000000000000000000000000000000000",
            BitConverter.ToString(memoXdr.RetHash.InnerValue).Replace("-", "").ToLower());
        Assert.AreEqual("4142434445464748494a4b4c", memo.GetTrimmedHexValue());
    }

    /// <summary>
    /// Verifies that Memo.Id instances with same ID value are equal and have same hash code.
    /// </summary>
    [TestMethod]
    public void Id_WithSameIdValue_AreEqualAndHaveSameHashCode()
    {
        // Arrange
        var memo = Memo.Id(9223372036854775807L);
        var memo2 = Memo.Id(9223372036854775807L);

        // Act & Assert
        Assert.AreEqual(memo.GetHashCode(), memo2.GetHashCode());
        Assert.AreEqual(memo, memo2);
    }

    /// <summary>
    /// Verifies that Memo.ReturnHash instances with same hash value are equal and have same hash code.
    /// </summary>
    [TestMethod]
    public void ReturnHash_WithSameHashValue_AreEqualAndHaveSameHashCode()
    {
        // Arrange
        var memo = Memo.ReturnHash("4142434445464748494a4b4c");
        var memo2 = Memo.ReturnHash("4142434445464748494a4b4c");

        // Act & Assert
        Assert.AreEqual(memo.GetHashCode(), memo2.GetHashCode());
        Assert.AreEqual(memo, memo2);

        memo = Memo.ReturnHash(Encoding.UTF8.GetBytes("4142434445464748494a4b4c"));
        memo2 = Memo.ReturnHash(Encoding.UTF8.GetBytes("4142434445464748494a4b4c"));

        Assert.AreEqual(memo.GetHashCode(), memo2.GetHashCode());
        Assert.AreEqual(memo, memo2);
    }

    /// <summary>
    /// Verifies that Memo.Hash instances with same hash value are equal and have same hash code.
    /// </summary>
    [TestMethod]
    public void Hash_WithSameHashValue_AreEqualAndHaveSameHashCode()
    {
        // Arrange
        var memo = Memo.Hash("4142434445464748494a4b4c");
        var memo2 = Memo.Hash("4142434445464748494a4b4c");

        // Act & Assert
        Assert.AreEqual(memo.GetHashCode(), memo2.GetHashCode());
        Assert.AreEqual(memo, memo2);
    }

    /// <summary>
    /// Verifies that Memo.Text instances with same text value are equal and have same hash code.
    /// </summary>
    [TestMethod]
    public void Text_WithSameTextValue_AreEqualAndHaveSameHashCode()
    {
        // Arrange
        var memo = Memo.Text("test");
        var memo2 = Memo.Text("test");

        // Act & Assert
        Assert.AreEqual(memo.GetHashCode(), memo2.GetHashCode());
        Assert.AreEqual(memo, memo2);
    }

    /// <summary>
    /// Verifies that Memo.None instances are equal and have same hash code.
    /// </summary>
    [TestMethod]
    public void None_Instances_AreEqualAndHaveSameHashCode()
    {
        // Arrange
        var memo = Memo.None();
        var memo2 = Memo.None();

        // Act & Assert
        Assert.AreEqual(memo.GetHashCode(), memo2.GetHashCode());
        Assert.AreEqual(memo, memo2);
    }
}