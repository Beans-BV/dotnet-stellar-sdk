using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;

namespace stellar_dotnet_sdk_test.operations
{
    [TestClass]
    public class FootprintOperationTest
    {
        private readonly KeyPair _sourceAccount =
            KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

        [TestMethod]
        public void TestEmptyExtExtendFootprintOperation()
        {
            var extendTo = new SCUint32(10000);
            var builder = new ExtendFootprintOperation.Builder();
            builder.SetSourceAccount(_sourceAccount);
            builder.SetExtendTo(extendTo);
            
            var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
            Assert.AreEqual("Ext cannot be null", ex.Message);
        }
        
        [TestMethod]
        public void TestEmptyExtendToExtendFootprintOperation()
        {
            var zeroExt = new ExtensionPointZero();
            var builder = new ExtendFootprintOperation.Builder();
            builder.SetSourceAccount(_sourceAccount);
            builder.SetExt(zeroExt);
            
            var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
            Assert.AreEqual("Extend to cannot be null", ex.Message);
        }
        
        [TestMethod]
        public void TestEmptySourceExtendFootprintOperation()
        {
            var zeroExt = new ExtensionPointZero();
            var extendTo = new SCUint32(10000);
            var builder = new ExtendFootprintOperation.Builder();
            builder.SetExtendTo(extendTo);
            builder.SetExt(zeroExt);

            var operation = builder.Build();
            
            // Act
            var operationXdrBase64 = operation.ToXdrBase64();

            var decodedOperation = ExtendFootprintOperation.FromOperationXdrBase64(operationXdrBase64);
            
            // Assert
            Assert.AreEqual(operation.ExtendTo.InnerValue, decodedOperation.ExtendTo.InnerValue);
            Assert.AreEqual(operation.Ext.ToXdrBase64(), decodedOperation.Ext.ToXdrBase64());
            Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
        }

        [TestMethod]
        public void TestExtendFootprintOperation()
        {
            var zeroExt = new ExtensionPointZero();
            var extendTo = new SCUint32(10000);
            var builder = new ExtendFootprintOperation.Builder();
            builder.SetExtendTo(extendTo);
            builder.SetExt(zeroExt);
            builder.SetSourceAccount(_sourceAccount);

            var operation = builder.Build();
            
            // Act
            var operationXdrBase64 = operation.ToXdrBase64();

            var decodedOperation = ExtendFootprintOperation.FromOperationXdrBase64(operationXdrBase64);
            
            // Assert
            Assert.AreEqual(operation.ExtendTo.InnerValue, decodedOperation.ExtendTo.InnerValue);
            Assert.AreEqual(operation.Ext.ToXdrBase64(), decodedOperation.Ext.ToXdrBase64());
            Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
        }
        
        [TestMethod]
        public void TestEmptyExtRestoreFootprintOperation()
        {
            var builder = new RestoreFootprintOperation.Builder();
            builder.SetSourceAccount(_sourceAccount);
            
            var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
            Assert.AreEqual("Ext cannot be null", ex.Message);
        }
        
        [TestMethod]
        public void TestEmptySourceRestoreFootprintOperation()
        {
            var zeroExt = new ExtensionPointZero();
            var builder = new RestoreFootprintOperation.Builder();
            builder.SetExt(zeroExt);

            var operation = builder.Build();
            
            // Act
            var operationXdrBase64 = operation.ToXdrBase64();

            var decodedOperation = RestoreFootprintOperation.FromOperationXdrBase64(operationXdrBase64);
            
            // Assert
            Assert.AreEqual(operation.Ext.ToXdrBase64(), decodedOperation.Ext.ToXdrBase64());
            Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
        }

        [TestMethod]
        public void TestRestoreFootprintOperation()
        {
            var zeroExt = new ExtensionPointZero();
            var builder = new RestoreFootprintOperation.Builder();
            builder.SetExt(zeroExt);
            builder.SetSourceAccount(_sourceAccount);

            var operation = builder.Build();
            
            // Act
            var operationXdrBase64 = operation.ToXdrBase64();

            var decodedOperation = RestoreFootprintOperation.FromOperationXdrBase64(operationXdrBase64);
            
            // Assert
            Assert.AreEqual(operation.Ext.ToXdrBase64(), decodedOperation.Ext.ToXdrBase64());
            Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
        }
    }
}