﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnetcore_sdk;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace stellar_dotnetcore_unittest
{
    [TestClass]
    public class TransactionTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Network.UseTestNetwork();
        }

        [TestMethod]
        public void TestBuilderSuccessTestnet()
        {
            // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
            KeyPair source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
            KeyPair destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

            long sequenceNumber = 2908908335136768L;
            Account account = new Account(source, sequenceNumber);
            Transaction transaction = new Transaction.Builder(account)
                    .AddOperation(new CreateAccountOperation.Builder(destination, "2000").Build())
                    .Build();

            transaction.Sign(source);

            Assert.AreEqual(
                    "AAAAAF7FIiDToW1fOYUFBC0dmyufJbFTOa2GQESGz+S2h5ViAAAAZAAKVaMAAAABAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAAEqBfIAAAAAAAAAAABtoeVYgAAAEDLki9Oi700N60Lo8gUmEFHbKvYG4QSqXiLIt9T0ru2O5BphVl/jR9tYtHAD+UeDYhgXNgwUxqTEu1WukvEyYcD",
                    transaction.ToEnvelopeXdrBase64());

            Assert.AreEqual(transaction.SourceAccount, source);
            Assert.AreEqual(transaction.SequenceNumber, sequenceNumber + 1);
            Assert.AreEqual(transaction.Fee, 100);
        }

        [TestMethod]
        public void TestBuilderMemoText()
        {            
            // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
            KeyPair source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
            KeyPair destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

            Account account = new Account(source, 2908908335136768);
            Transaction transaction = new Transaction.Builder(account)
                    .AddOperation(new CreateAccountOperation.Builder(destination, "2000").Build())
                    .AddMemo(Memo.Text("Hello world!"))
                    .Build();

            transaction.Sign(source);

            Assert.AreEqual(
                    "AAAAAF7FIiDToW1fOYUFBC0dmyufJbFTOa2GQESGz+S2h5ViAAAAZAAKVaMAAAABAAAAAAAAAAEAAAAMSGVsbG8gd29ybGQhAAAAAQAAAAAAAAAAAAAAAO3gUmG83C+VCqO6FztuMtXJF/l7grZA7MjRzqdZ9W8QAAAABKgXyAAAAAAAAAAAAbaHlWIAAABAxzofBhoayuUnz8t0T1UNWrTgmJ+lCh9KaeOGu2ppNOz9UGw0abGLhv+9oWQsstaHx6YjwWxL+8GBvwBUVWRlBQ==",
                    transaction.ToEnvelopeXdrBase64());
        }

        //[TestMethod]
        //public void TestBuilderTimeBounds()
        //{
        //    //TODO: Fix me!
        //    Assert.Fail();
        //    // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
        //    KeyPair source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        //    KeyPair destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        //    Account account = new Account(source, 2908908335136768L);
        //    Transaction transaction = new Transaction.Builder(account)
        //            .AddOperation(new CreateAccountOperation.Builder(destination, "2000").build())
        //            .AddTimeBounds(new TimeBounds(42, 1337))
        //            .Build();

        //    transaction.Sign(source);

        //    // Convert transaction to binary XDR and back again to make sure timebounds are correctly de/serialized.
        //    XdrDataInputStream is = new XdrDataInputStream(

        //            new ByteArrayInputStream(
        //                    javax.xml.bind.DatatypeConverter.parseBase64Binary(transaction.toEnvelopeXdrBase64())
        //            )
        //    );
        //    org.stellar.sdk.xdr.Transaction decodedTransaction = org.stellar.sdk.xdr.Transaction.decode(is);

        //    Assert.AreEqual(decodedTransaction.getTimeBounds().getMinTime().getUint64().longValue(), 42);
        //    Assert.AreEqual(decodedTransaction.getTimeBounds().getMaxTime().getUint64().longValue(), 1337);
        //}

        [TestMethod]
        public void TestBuilderSuccessPublic()
        {
            Network.UsePublicNetwork();

            // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
            KeyPair source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
            KeyPair destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

            Account account = new Account(source, 2908908335136768L);
            Transaction transaction = new Transaction.Builder(account)
                    .AddOperation(new CreateAccountOperation.Builder(destination, "2000").Build())
                    .Build();

            transaction.Sign(source);

            Assert.AreEqual(
                    "AAAAAF7FIiDToW1fOYUFBC0dmyufJbFTOa2GQESGz+S2h5ViAAAAZAAKVaMAAAABAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAA7eBSYbzcL5UKo7oXO24y1ckX+XuCtkDsyNHOp1n1bxAAAAAEqBfIAAAAAAAAAAABtoeVYgAAAEDzfR5PgRFim5Wdvq9ImdZNWGBxBWwYkQPa9l5iiBdtPLzAZv6qj+iOfSrqinsoF0XrLkwdIcZQVtp3VRHhRoUE",
                    transaction.ToEnvelopeXdrBase64());
        }

        [TestMethod]
        public void TestSha256HashSigning()
        {
            Network.UsePublicNetwork();

            KeyPair source = KeyPair.FromAccountId("GBBM6BKZPEHWYO3E3YKREDPQXMS4VK35YLNU7NFBRI26RAN7GI5POFBB");
            KeyPair destination = KeyPair.FromAccountId("GDJJRRMBK4IWLEPJGIE6SXD2LP7REGZODU7WDC3I2D6MR37F4XSHBKX2");

            Account account = new Account(source, 0L);
            Transaction transaction = new Transaction.Builder(account)
                    .AddOperation(new PaymentOperation.Builder(destination, new AssetTypeNative(), "2000").Build())
                    .Build();

            byte[] preimage = new byte[64];

            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

            rngCsp.GetBytes(preimage);
            byte[] hash = Util.Hash(preimage);

            transaction.Sign(preimage);

            Assert.IsTrue(transaction.Signatures[0].Signature.InnerValue.Equals(preimage));


            var length = hash.Length;
            var rangeHashCopy = hash.Skip(length - 4).Take(4).ToArray();

            Assert.IsTrue(transaction.Signatures[0].Hint.InnerValue.SequenceEqual(rangeHashCopy));
        }

        [TestMethod]
        public void TestToBase64EnvelopeXdrBuilderNoSignatures()
        {
            // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
            KeyPair source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
            KeyPair destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

            Account account = new Account(source, 2908908335136768L);
            Transaction transaction = new Transaction.Builder(account)
                    .AddOperation(new CreateAccountOperation.Builder(destination, "2000").Build())
                    .Build();

            try
            {
                transaction.ToEnvelopeXdrBase64();
                Assert.Fail();
            }
            catch (Exception exception)
            {
                Assert.IsTrue(exception.Message.Contains("Transaction must be signed by at least one signer."));
            }
        }

        [TestMethod]
        public void TestNoOperations()
        {
            // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
            KeyPair source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");

            Account account = new Account(source, 2908908335136768L);
            try
            {
                Transaction transaction = new Transaction.Builder(account).Build();
                Assert.Fail();
            }
            catch (Exception exception)
            {
                Assert.IsTrue(exception.Message.Contains("At least one operation required"));
                Assert.AreEqual(2908908335136768L, account.SequenceNumber);
            }
        }

        [TestMethod]
        public void TestTryingToAddMemoTwice()
        {
            // GBPMKIRA2OQW2XZZQUCQILI5TMVZ6JNRKM423BSAISDM7ZFWQ6KWEBC4
            KeyPair source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
            KeyPair destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

            try
            {
                Account account = new Account(source, 2908908335136768L);
                new Transaction.Builder(account)
                              .AddOperation(new CreateAccountOperation.Builder(destination, "2000").Build())
                              .AddMemo(Memo.None())
                              .AddMemo(Memo.None());
                Assert.Fail();
            }
            catch (Exception exception)
            {
                Assert.IsTrue(exception.Message.Contains("Memo has been already added."));
            }
        }
    }
}
