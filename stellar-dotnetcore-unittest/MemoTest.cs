﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnetcore_sdk;
using FormatException = System.FormatException;
using sdkxdr = stellar_dotnetcore_sdk.xdr;

namespace stellar_dotnetcore_unittest
{
    [TestClass]
    public class MemoTest
    {
        [TestMethod]
        public void TestMemoNone()
        {
            var memo = Memo.None();
            Assert.AreEqual(sdkxdr.MemoType.MemoTypeEnum.MEMO_NONE, memo.ToXdr().Discriminant.InnerValue);
        }

        [TestMethod]
        public void TestMemoTextSuccess()
        {
            var memo = Memo.Text("test");
            Assert.AreEqual(sdkxdr.MemoType.MemoTypeEnum.MEMO_TEXT, memo.ToXdr().Discriminant.InnerValue);
            Assert.AreEqual("test", memo.MemoTextValue);
        }

        [TestMethod]
        public void TestMemoTextUtf8()
        {
            var memo = Memo.Text("三");
            Assert.AreEqual(sdkxdr.MemoType.MemoTypeEnum.MEMO_TEXT, memo.ToXdr().Discriminant.InnerValue);
            Assert.AreEqual("三", memo.MemoTextValue);
        }

        [TestMethod]
        public void TestMemoTextTooLong()
        {
            try
            {
                Memo.Text("12345678901234567890123456789");
                Assert.Fail();
            }
            catch (Exception exception)
            {
                Assert.IsTrue(exception.Message.Contains("text must be <= 28 bytes."));
            }
        }

        [TestMethod]
        public void TestMemoTextTooLongUtf8()
        {
            try
            {
                Memo.Text("价值交易的开源协议!!");
                Assert.Fail();
            }
            catch (Exception exception)
            {
                Assert.IsTrue(exception.Message.Contains("text must be <= 28 bytes."));
            }
        }

        [TestMethod]
        public void TestMemoId()
        {
            var memo = Memo.Id(9223372036854775807L);
            Assert.AreEqual(9223372036854775807L, memo.Id);
            Assert.AreEqual(sdkxdr.MemoType.MemoTypeEnum.MEMO_ID, memo.ToXdr().Discriminant.InnerValue);
            Assert.AreEqual(9223372036854775807L, memo.ToXdr().Id.InnerValue);
        }

        [TestMethod]
        public void TestMemoHashSuccess()
        {
            var memo = Memo.Hash("4142434445464748494a4b4c");
            Assert.AreEqual(sdkxdr.MemoType.MemoTypeEnum.MEMO_HASH, memo.ToXdr().Discriminant.InnerValue);
            var test = "ABCDEFGHIJKL";
            Assert.AreEqual(test, Util.PaddedByteArrayToString(memo.MemoBytes));
            Assert.AreEqual("4142434445464748494a4b4c", memo.GetTrimmedHexValue());
        }

        [TestMethod]
        public void TestMemoHashBytesSuccess()
        {
            var bytes = Enumerable.Repeat((byte) 'A', 10).ToArray();
            var memo = Memo.Hash(bytes);
            Assert.AreEqual(sdkxdr.MemoType.MemoTypeEnum.MEMO_HASH, memo.ToXdr().Discriminant.InnerValue);
            Assert.AreEqual("AAAAAAAAAA", Util.PaddedByteArrayToString(memo.MemoBytes));
            Assert.AreEqual("4141414141414141414100000000000000000000000000000000000000000000", memo.GetHexValue());
            Assert.AreEqual("41414141414141414141", memo.GetTrimmedHexValue());
        }

        [TestMethod]
        public void TestMemoHashTooLong()
        {
            var longer = Enumerable.Repeat((byte) 0, 33).ToArray();
            try
            {
                Memo.Hash(longer);
                Assert.Fail();
            }
            catch (MemoTooLongException exception)
            {
                Assert.IsTrue(exception.Message.Contains("MEMO_HASH can contain 32 bytes at max."));
            }
        }

        [TestMethod]
        public void TestMemoHashInvalidHex()
        {
            try
            {
                Memo.Hash("test");
                Assert.Fail();
            }
            catch (FormatException)
            {
            }
        }

        [TestMethod]
        public void TestMemoReturnHashSuccess()
        {
            var memo = Memo.returnHash("4142434445464748494a4b4c");
            Assert.AreEqual(sdkxdr.MemoType.MemoTypeEnum.MEMO_RETURN, memo.ToXdr().Discriminant.InnerValue);
            Assert.AreEqual("4142434445464748494a4b4c", memo.GetTrimmedHexValue());
        }
    }
}