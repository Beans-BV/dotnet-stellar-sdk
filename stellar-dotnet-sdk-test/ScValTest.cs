using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;
using xdrSDK = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class ScValTest
{
    private const string WasmHash = "ZBYoEJT3IaPMMk3FoRmnEQHoDxewPZL+Uor+xWI4uII=";

    [TestMethod]
    public void TestScBool()
    {
        var scBoolTrue = new SCBool(true);
        var scBoolFalse = new SCBool(false);

        // Act
        var scBoolTrueXdrBase64 = scBoolTrue.ToXdrBase64();
        var fromXdrBase64ScBoolTrue = (SCBool)SCVal.FromXdrBase64(scBoolTrueXdrBase64);

        var scBoolFalseXdrBase64 = scBoolFalse.ToXdrBase64();
        var fromXdrBase64ScBoolFalse = (SCBool)SCVal.FromXdrBase64(scBoolFalseXdrBase64);

        // Assert
        Assert.AreEqual(scBoolTrue.InnerValue, fromXdrBase64ScBoolTrue.InnerValue);
        Assert.AreEqual(scBoolFalse.InnerValue, fromXdrBase64ScBoolFalse.InnerValue);
    }

    [TestMethod]
    public void TestSCContractError()
    {
        var error = new SCContractError(1);

        // Act
        var contractErrorXdrBase64 = error.ToXdrBase64();
        var fromXdrBase64ContractError = (SCContractError)SCVal.FromXdrBase64(contractErrorXdrBase64);

        // Assert
        Assert.AreEqual(error.ContractCode, fromXdrBase64ContractError.ContractCode);
    }

    [TestMethod]
    public void TestSCWasmVmError()
    {
        var arithDomainError = new SCWasmVmError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN
        };
        var internalError = new SCWasmVmError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR
        };
        // Act
        var arithDomainErrorXdrBase64 = arithDomainError.ToXdrBase64();
        var fromXdrBase64ArithDomainError = (SCWasmVmError)SCVal.FromXdrBase64(arithDomainErrorXdrBase64);

        var internalErrorXdrBase64 = internalError.ToXdrBase64();
        var fromXdrBase64InternalError = (SCWasmVmError)SCVal.FromXdrBase64(internalErrorXdrBase64);

        // Assert
        Assert.AreEqual(arithDomainError.Code, fromXdrBase64ArithDomainError.Code);
        Assert.AreEqual(internalError.Code, fromXdrBase64InternalError.Code);
    }

    [TestMethod]
    public void TestSCContextError()
    {
        var arithDomainError = new SCContextError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN
        };
        var internalError = new SCContextError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR
        };
        // Act
        var arithDomainErrorXdrBase64 = arithDomainError.ToXdrBase64();
        var fromXdrBase64ArithDomainError = (SCContextError)SCVal.FromXdrBase64(arithDomainErrorXdrBase64);

        var xdrBase64InternalError = internalError.ToXdrBase64();
        var fromXdrBase64InternalError = (SCContextError)SCVal.FromXdrBase64(xdrBase64InternalError);

        // Assert
        Assert.AreEqual(arithDomainError.Code, fromXdrBase64ArithDomainError.Code);
        Assert.AreEqual(internalError.Code, fromXdrBase64InternalError.Code);
    }

    [TestMethod]
    public void TestSCStorageError()
    {
        var arithDomainError = new SCStorageError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN
        };
        var internalError = new SCStorageError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR
        };
        // Act
        var arithDomainErrorXdrBase64 = arithDomainError.ToXdrBase64();
        var fromXdrBase64ArithDomainError = (SCStorageError)SCVal.FromXdrBase64(arithDomainErrorXdrBase64);

        var xdrBase64InternalError = internalError.ToXdrBase64();
        var fromXdrBase64InternalError = (SCStorageError)SCVal.FromXdrBase64(xdrBase64InternalError);

        // Assert
        Assert.AreEqual(arithDomainError.Code, fromXdrBase64ArithDomainError.Code);
        Assert.AreEqual(internalError.Code, fromXdrBase64InternalError.Code);
    }

    [TestMethod]
    public void TestSCObjectError()
    {
        var arithDomainError = new SCObjectError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN
        };
        var internalError = new SCObjectError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR
        };
        // Act
        var arithDomainErrorXdrBase64 = arithDomainError.ToXdrBase64();
        var fromXdrBase64ArithDomainError = (SCObjectError)SCVal.FromXdrBase64(arithDomainErrorXdrBase64);

        var xdrBase64InternalError = internalError.ToXdrBase64();
        var fromXdrBase64InternalError = (SCObjectError)SCVal.FromXdrBase64(xdrBase64InternalError);

        // Assert
        Assert.AreEqual(arithDomainError.Code, fromXdrBase64ArithDomainError.Code);
        Assert.AreEqual(internalError.Code, fromXdrBase64InternalError.Code);
    }

    [TestMethod]
    public void TestSCCryptoError()
    {
        var arithDomainError = new SCCryptoError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN
        };
        var internalError = new SCCryptoError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR
        };
        // Act
        var arithDomainErrorXdrBase64 = arithDomainError.ToXdrBase64();
        var fromXdrBase64ArithDomainError = (SCCryptoError)SCVal.FromXdrBase64(arithDomainErrorXdrBase64);

        var xdrBase64InternalError = internalError.ToXdrBase64();
        var fromXdrBase64InternalError = (SCCryptoError)SCVal.FromXdrBase64(xdrBase64InternalError);

        // Assert
        Assert.AreEqual(arithDomainError.Code, fromXdrBase64ArithDomainError.Code);
        Assert.AreEqual(internalError.Code, fromXdrBase64InternalError.Code);
    }

    [TestMethod]
    public void TestSCEventsError()
    {
        var arithDomainError = new SCEventsError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN
        };
        var internalError = new SCEventsError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR
        };
        // Act
        var arithDomainErrorXdrBase64 = arithDomainError.ToXdrBase64();
        var fromXdrBase64ArithDomainError = (SCEventsError)SCVal.FromXdrBase64(arithDomainErrorXdrBase64);

        var xdrBase64InternalError = internalError.ToXdrBase64();
        var fromXdrBase64InternalError = (SCEventsError)SCVal.FromXdrBase64(xdrBase64InternalError);

        // Assert
        Assert.AreEqual(arithDomainError.Code, fromXdrBase64ArithDomainError.Code);
        Assert.AreEqual(internalError.Code, fromXdrBase64InternalError.Code);
    }

    [TestMethod]
    public void TestSCBudgetError()
    {
        var arithDomainError = new SCBudgetError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN
        };
        var internalError = new SCBudgetError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR
        };
        // Act
        var arithDomainErrorXdrBase64 = arithDomainError.ToXdrBase64();
        var fromXdrBase64ArithDomainError = (SCBudgetError)SCVal.FromXdrBase64(arithDomainErrorXdrBase64);

        var xdrBase64InternalError = internalError.ToXdrBase64();
        var fromXdrBase64InternalError = (SCBudgetError)SCVal.FromXdrBase64(xdrBase64InternalError);

        // Assert
        Assert.AreEqual(arithDomainError.Code, fromXdrBase64ArithDomainError.Code);
        Assert.AreEqual(internalError.Code, fromXdrBase64InternalError.Code);
    }

    [TestMethod]
    public void TestSCValueError()
    {
        var arithDomainError = new SCValueError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN
        };
        var internalError = new SCValueError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR
        };
        // Act
        var arithDomainErrorXdrBase64 = arithDomainError.ToXdrBase64();
        var fromXdrBase64ArithDomainError = (SCValueError)SCVal.FromXdrBase64(arithDomainErrorXdrBase64);

        var xdrBase64InternalError = internalError.ToXdrBase64();
        var fromXdrBase64InternalError = (SCValueError)SCVal.FromXdrBase64(xdrBase64InternalError);

        // Assert
        Assert.AreEqual(arithDomainError.Code, fromXdrBase64ArithDomainError.Code);
        Assert.AreEqual(internalError.Code, fromXdrBase64InternalError.Code);
    }

    [TestMethod]
    public void TestSCAuthError()
    {
        var arithDomainError = new SCAuthError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN
        };
        var internalError = new SCAuthError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR
        };
        // Act
        var arithDomainErrorXdrBase64 = arithDomainError.ToXdrBase64();
        var fromXdrBase64ArithDomainError = (SCAuthError)SCVal.FromXdrBase64(arithDomainErrorXdrBase64);

        var xdrBase64InternalError = internalError.ToXdrBase64();
        var fromXdrBase64InternalError = (SCAuthError)SCVal.FromXdrBase64(xdrBase64InternalError);

        // Assert
        Assert.AreEqual(arithDomainError.Code, fromXdrBase64ArithDomainError.Code);
        Assert.AreEqual(internalError.Code, fromXdrBase64InternalError.Code);
    }

    [TestMethod]
    public void TestScUint32()
    {
        var scUint32 = new SCUint32(1319013123);

        // Act
        var scUint32XdrBase64 = scUint32.ToXdrBase64();
        var xdrScUint32 = (SCUint32)SCVal.FromXdrBase64(scUint32XdrBase64);

        // Assert
        Assert.AreEqual(scUint32.InnerValue, xdrScUint32.InnerValue);
    }

    [TestMethod]
    public void TestScInt32()
    {
        var scInt32 = new SCInt32(-192049123);

        // Act
        var scInt32XdrBase64 = scInt32.ToXdrBase64();
        var fromXdrBase64ScInt32 = (SCInt32)SCVal.FromXdrBase64(scInt32XdrBase64);

        // Assert
        Assert.AreEqual(scInt32.InnerValue, fromXdrBase64ScInt32.InnerValue);
    }

    [TestMethod]
    public void TestScUint64()
    {
        var scUint64 = new SCUint64(18446744073709551615);

        // Act
        var scUint64XdrBase64 = scUint64.ToXdrBase64();
        var fromXdrBase64ScUint64 = (SCUint64)SCVal.FromXdrBase64(scUint64XdrBase64);

        // Assert
        Assert.AreEqual(scUint64.InnerValue, fromXdrBase64ScUint64.InnerValue);
    }

    [TestMethod]
    public void TestScInt64()
    {
        var scInt64 = new SCInt64(-9223372036854775807);

        // Act
        var scInt64XdrBase64 = scInt64.ToXdrBase64();
        var fromXdrBase64ScInt64 = (SCInt64)SCVal.FromXdrBase64(scInt64XdrBase64);

        // Assert
        Assert.AreEqual(scInt64.InnerValue, fromXdrBase64ScInt64.InnerValue);
    }

    [TestMethod]
    public void TestScTimePoint()
    {
        var scTimePoint = new SCTimePoint(18446744073709551615);

        // Act
        var scTimePointXdrBase64 = scTimePoint.ToXdrBase64();
        var fromXdrBase64ScTimePoint = (SCTimePoint)SCVal.FromXdrBase64(scTimePointXdrBase64);

        // Assert
        Assert.AreEqual(scTimePoint.InnerValue, fromXdrBase64ScTimePoint.InnerValue);
    }

    [TestMethod]
    public void TestScDuration()
    {
        var scDuration = new SCDuration(18446744073709551615);

        // Act
        var scDurationXdrBase64 = scDuration.ToXdrBase64();
        var fromXdrBase64ScDuration = (SCDuration)SCVal.FromXdrBase64(scDurationXdrBase64);

        // Assert
        Assert.AreEqual(scDuration.InnerValue, fromXdrBase64ScDuration.InnerValue);
    }

    [TestMethod]
    public void TestScUint128()
    {
        var scUint128 = new SCUint128(1, 18446744073709551615);

        // Act
        var scUint128XdrBase64 = scUint128.ToXdrBase64();
        var fromXdrBase64ScUint128 = (SCUint128)SCVal.FromXdrBase64(scUint128XdrBase64);

        // Assert
        Assert.AreEqual(scUint128.Hi, fromXdrBase64ScUint128.Hi);
        Assert.AreEqual(scUint128.Lo, fromXdrBase64ScUint128.Lo);
    }

    [TestMethod]
    public void TestScInt128()
    {
        var scInt128 = new SCInt128(18446744073709551615, -9223372036854775807);

        // Act
        var scInt128XdrBase64 = scInt128.ToXdrBase64();
        var fromXdrBase64ScInt128 = (SCInt128)SCVal.FromXdrBase64(scInt128XdrBase64);

        // Assert
        Assert.AreEqual(scInt128.Lo, fromXdrBase64ScInt128.Lo);
        Assert.AreEqual(scInt128.Hi, fromXdrBase64ScInt128.Hi);
    }

    [TestMethod]
    public void TestScUint256()
    {
        var scUint256 = new SCUint256
        {
            HiHi = 18446744073709551615,
            HiLo = 1,
            LoHi = 18446744073709551614,
            LoLo = 0
        };

        // Act
        var scUint256XdrBase64 = scUint256.ToXdrBase64();
        var fromXdrBase64ScUint256 = (SCUint256)SCVal.FromXdrBase64(scUint256XdrBase64);

        // Assert
        Assert.AreEqual(scUint256.HiHi, fromXdrBase64ScUint256.HiHi);
        Assert.AreEqual(scUint256.HiLo, fromXdrBase64ScUint256.HiLo);
        Assert.AreEqual(scUint256.LoHi, fromXdrBase64ScUint256.LoHi);
        Assert.AreEqual(scUint256.LoLo, fromXdrBase64ScUint256.LoLo);
    }

    [TestMethod]
    public void TestScInt256()
    {
        var scInt256 = new SCInt256
        {
            HiHi = -9223372036854775807,
            HiLo = 18446744073709551614,
            LoHi = 18446744073709551615,
            LoLo = 18446744073709551613
        };

        // Act
        var scInt256XdrBase64 = scInt256.ToXdrBase64();
        var fromXdrBase64ScInt256 = (SCInt256)SCVal.FromXdrBase64(scInt256XdrBase64);

        // Assert
        Assert.AreEqual(scInt256.HiHi, fromXdrBase64ScInt256.HiHi);
        Assert.AreEqual(scInt256.HiLo, fromXdrBase64ScInt256.HiLo);
        Assert.AreEqual(scInt256.LoHi, fromXdrBase64ScInt256.LoHi);
        Assert.AreEqual(scInt256.LoLo, fromXdrBase64ScInt256.LoLo);
    }

    [TestMethod]
    public void TestScBytesWithValidArgument()
    {
        byte[] bytes = { 0x00, 0x01, 0x03, 0x03, 0x34, 0x45, 0x66, 0x46 };
        var scBytes = new SCBytes(bytes);

        // Act
        var scBytesXdrBase64 = scBytes.ToXdrBase64();
        var fromXdrBase64ScBytes = (SCBytes)SCVal.FromXdrBase64(scBytesXdrBase64);

        // Assert
        CollectionAssert.AreEqual(scBytes.InnerValue, fromXdrBase64ScBytes.InnerValue);
    }

    [TestMethod]
    public void TestScBytesWithEmptyArgument()
    {
        var bytes = Array.Empty<byte>();
        var scBytes = new SCBytes(bytes);

        // Act
        var scBytesXdrBase64 = scBytes.ToXdrBase64();
        var fromXdrBase64ScBytes = (SCBytes)SCVal.FromXdrBase64(scBytesXdrBase64);

        // Assert
        CollectionAssert.AreEqual(scBytes.InnerValue, fromXdrBase64ScBytes.InnerValue);
    }

    [TestMethod]
    public void TestScString()
    {
        var scString = new SCString("hello world");

        // Act
        var scStringXdrBase64 = scString.ToXdrBase64();
        var fromXdrBase64ScString = (SCString)SCVal.FromXdrBase64(scStringXdrBase64);

        // Assert
        Assert.AreEqual(scString.InnerValue, fromXdrBase64ScString.InnerValue);
    }

    [TestMethod]
    public void TestScStringWithEmptyArgument()
    {
        var scString = new SCString("");

        // Act
        var scStringXdrBase64 = scString.ToXdrBase64();
        var fromXdrBase64ScString = (SCString)SCVal.FromXdrBase64(scStringXdrBase64);

        // Assert
        Assert.AreEqual(scString.InnerValue, fromXdrBase64ScString.InnerValue);
    }

    [TestMethod]
    public void TestScSymbol()
    {
        var scSymbol = new SCSymbol("Is this a symbol?");

        // Act
        var scSymbolXdrBase64 = scSymbol.ToXdrBase64();
        var fromXdrBase64ScSymbol = (SCSymbol)SCVal.FromXdrBase64(scSymbolXdrBase64);

        // Assert
        Assert.AreEqual(scSymbol.InnerValue, fromXdrBase64ScSymbol.InnerValue);
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check each of the scVec.InnerValue element for type and properties,
    ///     since there are already other tests in the <see cref="ScValTest" /> class that cover different scenarios for
    ///     <see cref="SCVal" />
    /// </remarks>
    [TestMethod]
    public void TestScVecWithValidEntries()
    {
        var scSymbol = new SCSymbol("Is this a symbol?");
        var scString = new SCString("hello world");
        var scVals = new SCVal[] { scString, scSymbol };
        var scVec = new SCVec(scVals);

        // Act
        var scVecXdrBase64 = scVec.ToXdrBase64();
        var fromXdrBase64ScVec = (SCVec)SCVal.FromXdrBase64(scVecXdrBase64);

        // Assert
        Assert.AreEqual(scVec.InnerValue.Length, fromXdrBase64ScVec.InnerValue.Length);
        for (var i = 0; i < scVec.InnerValue.Length; i++)
            Assert.AreEqual(scVec.InnerValue[i].ToXdrBase64(), fromXdrBase64ScVec.InnerValue[i].ToXdrBase64());
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check each of the scMap Key and Value for type and properties,
    ///     since there are already other tests in the <see cref="ScValTest" /> class that cover different scenarios for
    ///     <see cref="SCVal" />
    /// </remarks>
    [TestMethod]
    public void TestScMapWithValidEntries()
    {
        var entry1 = new SCMapEntry
        {
            Key = new SCString("key 1"),
            Value = new SCBool(false)
        };
        var entry2 = new SCMapEntry
        {
            Key = new SCUint32(1),
            Value = new SCString("this is value 2")
        };
        var entry3 = new SCMapEntry
        {
            Key = new SCUint32(1),
            Value = new SCSymbol("$$$")
        };
        var nestedScMap = new SCMap
        {
            Entries = new[] { entry1, entry2 }
        };
        var entry4 = new SCMapEntry
        {
            Key = new SCUint32(1),
            Value = nestedScMap
        };

        var mapEntries = new[] { entry1, entry2, entry3, entry3, entry4 };
        var scMap = new SCMap { Entries = mapEntries };

        // Act
        var scMapXdrBase64 = scMap.ToXdrBase64();
        var fromXdrBase64ScMap = (SCMap)SCVal.FromXdrBase64(scMapXdrBase64);

        // Assert
        Assert.AreEqual(scMap.Entries.Length, fromXdrBase64ScMap.Entries.Length);
        for (var i = 0; i < scMap.Entries.Length; i++)
        {
            Assert.AreEqual(scMap.Entries[i].Key.ToXdrBase64(), fromXdrBase64ScMap.Entries[i].Key.ToXdrBase64());
            Assert.AreEqual(scMap.Entries[i].Value.ToXdrBase64(), fromXdrBase64ScMap.Entries[i].Value.ToXdrBase64());
        }
    }

    [TestMethod]
    public void TestScMapWithNoEntries()
    {
        var scMap = new SCMap();

        // Act
        var scMapXdrBase64 = scMap.ToXdrBase64();
        var fromXdrBase64ScMap = (SCMap)SCVal.FromXdrBase64(scMapXdrBase64);

        // Assert
        Assert.AreEqual(0, fromXdrBase64ScMap.Entries.Length);
    }

    [TestMethod]
    public void TestSCContractExecutableWasmWithMissingStorage()
    {
        var contractExecutable = new ContractExecutableWasm(WasmHash);

        var contractInstance = new SCContractInstance
        {
            Executable = contractExecutable
        };

        // Act
        var contractInstanceXdrBase64 = contractInstance.ToXdrBase64();
        var fromXdrBase64ContractInstance = (SCContractInstance)SCVal.FromXdrBase64(contractInstanceXdrBase64);

        var decodedContractExecutable = (ContractExecutableWasm)fromXdrBase64ContractInstance.Executable;

        // Assert
        Assert.AreEqual(contractExecutable.WasmHash, decodedContractExecutable.WasmHash);
        Assert.AreEqual(contractInstance.Storage.Entries.Length, fromXdrBase64ContractInstance.Storage.Entries.Length);
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check each of the entries element Key and Value for type and properties,
    ///     since there are already other tests in the <see cref="ScValTest" /> class that cover different scenarios for
    ///     <see cref="SCVal" />
    /// </remarks>
    [TestMethod]
    public void TestSCContractExecutableWasm()
    {
        var contractExecutable = new ContractExecutableWasm(WasmHash);

        var entry1 = new SCMapEntry
        {
            Key = new SCString("key 1"),
            Value = new SCBool(false)
        };
        var entry2 = new SCMapEntry
        {
            Key = new SCUint32(111),
            Value = new SCString("2nd value")
        };
        var entry3 = new SCMapEntry
        {
            Key = new SCUint32(1),
            Value = new SCSymbol("&")
        };

        SCMapEntry[] mapEntries = { entry1, entry2, entry3 };
        var scMap = new SCMap { Entries = mapEntries };

        var contractInstance = new SCContractInstance
        {
            Executable = contractExecutable,
            Storage = scMap
        };

        // Act
        var contractInstanceXdrBase64 = contractInstance.ToXdrBase64();
        var fromXdrBase64ContractInstance = (SCContractInstance)SCVal.FromXdrBase64(contractInstanceXdrBase64);

        var decodedContractExecutable = (ContractExecutableWasm)fromXdrBase64ContractInstance.Executable;

        var entries = contractInstance.Storage.Entries;
        var decodedEntries = fromXdrBase64ContractInstance.Storage.Entries;

        // Assert
        Assert.AreEqual(contractExecutable.WasmHash, decodedContractExecutable.WasmHash);
        Assert.AreEqual(entries.Length, decodedEntries.Length);
        for (var i = 0; i < entries.Length; i++)
        {
            Assert.AreEqual(entries[i].Key.ToXdrBase64(), decodedEntries[i].Key.ToXdrBase64());
            Assert.AreEqual(entries[i].Value.ToXdrBase64(), decodedEntries[i].Value.ToXdrBase64());
        }
    }

    [TestMethod]
    public void TestSCContractExecutableStellarAssetWithMissingStorage()
    {
        var contractInstance = new SCContractInstance
        {
            Executable = new ContractExecutableStellarAsset()
        };

        var contractInstanceXdrBase64 = contractInstance.ToXdrBase64();
        var fromXdrBase64ContractInstance = (SCContractInstance)SCVal.FromXdrBase64(contractInstanceXdrBase64);

        Assert.AreEqual(0, fromXdrBase64ContractInstance.Storage.Entries.Length);
        // Nothing else to compare, as the XDR ContractExecutable of type CONTRACT_EXECUTABLE_STELLAR_ASSET doesn't have any property
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check each of the entries element Key and Value for type and properties,
    ///     since there are already other tests in the <see cref="ScValTest" /> class that cover different scenarios for
    ///     <see cref="SCVal" />
    /// </remarks>
    [TestMethod]
    public void TestSCContractExecutableStellarAsset()
    {
        var entry1 = new SCMapEntry
        {
            Key = new SCString("key 1"),
            Value = new SCBool(false)
        };
        var entry2 = new SCMapEntry
        {
            Key = new SCUint32(111),
            Value = new SCString("2nd value")
        };
        var entry3 = new SCMapEntry
        {
            Key = new SCUint32(1),
            Value = new SCSymbol("&")
        };

        SCMapEntry[] mapEntries = { entry1, entry2, entry3 };
        var scMap = new SCMap { Entries = mapEntries };

        var contractInstance = new SCContractInstance
        {
            Executable = new ContractExecutableStellarAsset(),
            Storage = scMap
        };

        var contractInstanceXdrBase64 = contractInstance.ToXdrBase64();
        var fromXdrBase64ContractInstance = (SCContractInstance)SCVal.FromXdrBase64(contractInstanceXdrBase64);

        var entries = contractInstance.Storage.Entries;
        var decodedEntries = fromXdrBase64ContractInstance.Storage.Entries;

        Assert.AreEqual(entries.Length, decodedEntries.Length);
        for (var i = 0; i < entries.Length; i++)
        {
            Assert.AreEqual(entries[i].Key.ToXdrBase64(), decodedEntries[i].Key.ToXdrBase64());
            Assert.AreEqual(entries[i].Value.ToXdrBase64(), decodedEntries[i].Value.ToXdrBase64());
        }
    }

    [TestMethod]
    public void TestScNonceKey()
    {
        var scNonceKey = new SCNonceKey(-9223372036854775807);

        // Act
        var scNonceKeyXdrBase64 = scNonceKey.ToXdrBase64();
        var fromXdrBase64ScNonceKey = (SCNonceKey)SCVal.FromXdrBase64(scNonceKeyXdrBase64);

        // Assert
        Assert.AreEqual(scNonceKey.Nonce, fromXdrBase64ScNonceKey.Nonce);
    }

    [TestMethod]
    public void TestOthers()
    {
        byte[] values = { 0, 10, 100 };
        var publicKey = KeyPair.FromAccountId("GCZFMH32MF5EAWETZTKF3ZV5SEVJPI53UEMDNSW55WBR75GMZJU4U573").XdrPublicKey;
        var thresholds = new xdrSDK.Thresholds();

        var v3 = new xdrSDK.AccountEntryExtensionV3
        {
            SeqTime = new xdrSDK.TimePoint(new xdrSDK.Uint64(1000)),
            Ext = new xdrSDK.ExtensionPoint { Discriminant = 0 },
            SeqLedger = new xdrSDK.Uint32(64)
        };
        var v2ext =
            new xdrSDK.AccountEntryExtensionV2.AccountEntryExtensionV2Ext
            {
                Discriminant = 3,
                V3 = v3
            };
        var v2 = new xdrSDK.AccountEntryExtensionV2
        {
            NumSponsored = new xdrSDK.Uint32(2),
            NumSponsoring = new xdrSDK.Uint32(1),
            SignerSponsoringIDs = new xdrSDK.SponsorshipDescriptor[]
                { new(new xdrSDK.AccountID(publicKey)) },
            Ext = v2ext
        };
        var v1ext =
            new xdrSDK.AccountEntryExtensionV1.AccountEntryExtensionV1Ext
            {
                Discriminant = 2,
                V2 = v2
            };
        var v1 = new xdrSDK.AccountEntryExtensionV1
        {
            Ext = v1ext,
            Liabilities = new xdrSDK.Liabilities
            {
                Buying = new xdrSDK.Int64(128),
                Selling = new xdrSDK.Int64(256)
            }
        };
        var ext = new xdrSDK.AccountEntry.AccountEntryExt
        {
            Discriminant = 1,
            V1 = new xdrSDK.AccountEntryExtensionV1
            {
                Liabilities = new xdrSDK.Liabilities
                {
                    Buying = new xdrSDK.Int64(128),
                    Selling = new xdrSDK.Int64(256)
                },
                Ext = new xdrSDK.AccountEntryExtensionV1.AccountEntryExtensionV1Ext
                {
                    Discriminant = 2,
                    V2 = new xdrSDK.AccountEntryExtensionV2
                    {
                        NumSponsored = new xdrSDK.Uint32(1),
                        NumSponsoring = new xdrSDK.Uint32(2),
                        SignerSponsoringIDs = new xdrSDK.SponsorshipDescriptor[] { },
                        Ext = new xdrSDK.AccountEntryExtensionV2.AccountEntryExtensionV2Ext
                        {
                            Discriminant = 0,
                            V3 = new xdrSDK.AccountEntryExtensionV3
                            {
                                Ext = new xdrSDK.ExtensionPoint()
                            }
                        }
                    }
                }
            }
        };

        var ledgerEntry = new xdrSDK.LedgerEntry
        {
            LastModifiedLedgerSeq = new xdrSDK.Uint32(1),
            Data = new xdrSDK.LedgerEntry.LedgerEntryData
            {
                Discriminant = new xdrSDK.LedgerEntryType
                {
                    InnerValue = xdrSDK.LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT
                },
                Account = new xdrSDK.AccountEntry
                {
                    AccountID = new xdrSDK.AccountID(KeyPair.Random().XdrPublicKey),
                    Balance = new xdrSDK.Int64(1),
                    SeqNum = new xdrSDK.SequenceNumber(new xdrSDK.Int64(2)),
                    NumSubEntries = new xdrSDK.Uint32(3),
                    InflationDest = null,
                    Flags = new xdrSDK.Uint32(4),
                    HomeDomain = new xdrSDK.String32("blabla"),
                    Thresholds = new xdrSDK.Thresholds(new byte[] { 1, 3, 5, 7 }),
                    Signers = new xdrSDK.Signer[] { },
                    // Ext must not be null
                    Ext = new xdrSDK.AccountEntry.AccountEntryExt
                    {
                        Discriminant = 0, // V1 is ignored if Discriminant is not 1 
                        V1 = new xdrSDK.AccountEntryExtensionV1()
                    }
                }
            },
            Ext = new xdrSDK.LedgerEntry.LedgerEntryExt
            {
                Discriminant = 0
            }
        };
        var os = new xdrSDK.XdrDataOutputStream();
        // xdrSDK.AccountEntry.AccountEntryExt.Encode(os, ext);
        xdrSDK.LedgerEntry.Encode(os, ledgerEntry);
        var xdr = Convert.ToBase64String(os.ToArray());
    }

    [TestMethod]
    public void Test2()
    {
        var dataEntry = new xdrSDK.DataEntry
        {
            AccountID = new xdrSDK.AccountID(KeyPair.Random().XdrPublicKey),
            DataName = new xdrSDK.String64("hehe"),
            DataValue = new xdrSDK.DataValue(new byte[64]
            {
                97, 226, 203, 28, 76, 241, 140, 193, 142, 125, 35, 14, 23, 90, 173, 48, 247, 117, 224, 252, 100, 84,
                204, 32, 229, 249, 159, 81, 191, 106, 78, 169, 107, 164, 134, 156, 77, 13, 88, 147, 162, 79, 189, 38,
                220, 178, 213, 41, 46, 119, 152, 176, 63, 18, 45, 55, 172, 94, 165, 80, 85, 25, 54, 24
            }),
            Ext = new xdrSDK.DataEntry.DataEntryExt
            {
                Discriminant = 0
            }
        };

        var os = new xdrSDK.XdrDataOutputStream();
        xdrSDK.DataEntry.Encode(os, dataEntry);
        var xdr = Convert.ToBase64String(os.ToArray());
    }

    [TestMethod]
    public void TestTransaction()
    {
        var network = new Network("Standalone Network ; February 2017");
        var source = KeyPair.FromSecretSeed(network.NetworkId);
        var txSource = new MuxedAccountMed25519(source, 0);
        var account = new Account(txSource, 7);
        var destination = KeyPair.FromAccountId("GDQERENWDDSQZS7R7WKHZI3BSOYMV3FSWR7TFUYFTKQ447PIX6NREOJM");
        var amount = "2000";
        var asset = new AssetTypeNative();
        var tx = new TransactionBuilder(account)
            .SetFee(100)
            .AddTimeBounds(new TimeBounds(0, 0))
            .AddOperation(
                new PaymentOperation.Builder(destination, asset, amount).Build())
            .AddMemo(new MemoText("Happy birthday!"))
            .Build();
        var tran = tx.ToXdrV1();
        tran.Ext = new xdrSDK.Transaction.TransactionExt
        {
            Discriminant = 1,
            SorobanData = new xdrSDK.SorobanTransactionData
            {
                Ext = new xdrSDK.ExtensionPoint(),
                ResourceFee = new xdrSDK.Int64(1),
                Resources = new xdrSDK.SorobanResources
                {
                    Footprint = new xdrSDK.LedgerFootprint
                    {
                        ReadOnly = new xdrSDK.LedgerKey[] { },
                        ReadWrite = new xdrSDK.LedgerKey[] { }
                    },
                    Instructions = new xdrSDK.Uint32(1),
                    ReadBytes = new xdrSDK.Uint32(1),
                    WriteBytes = new xdrSDK.Uint32(1)
                }
            }
        };
        var os = new xdrSDK.XdrDataOutputStream();
        xdrSDK.Transaction.Encode(os, tran);
        var xdr = Convert.ToBase64String(os.ToArray());
    }

    [TestMethod]
    public void TestTrustLineAsset()
    {
        var asset = new xdrSDK.TrustLineAsset
        {
            Discriminant = new xdrSDK.AssetType
            {
                InnerValue = xdrSDK.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4
            },
            AlphaNum4 = new xdrSDK.AlphaNum4
            {
                Issuer = new xdrSDK.AccountID(KeyPair.Random().XdrPublicKey),
                AssetCode = new xdrSDK.AssetCode4(Encoding.UTF8.GetBytes("XXX"))
            }
        };
        var os = new xdrSDK.XdrDataOutputStream();
        xdrSDK.TrustLineAsset.Encode(os, asset);
        var xdr = Convert.ToBase64String(os.ToArray());
    }

    [TestMethod]
    public void TestAsset()
    {
        var asset = new xdrSDK.Asset
        {
            Discriminant = new xdrSDK.AssetType
            {
                InnerValue = xdrSDK.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4
            },
            AlphaNum4 = new xdrSDK.AlphaNum4
            {
                Issuer = new xdrSDK.AccountID(KeyPair.Random().XdrPublicKey),
                AssetCode = new xdrSDK.AssetCode4(Encoding.UTF8.GetBytes("XXX"))
            }
        };
        var os = new xdrSDK.XdrDataOutputStream();
        xdrSDK.Asset.Encode(os, asset);
        var xdr = Convert.ToBase64String(os.ToArray());
    }

    [TestMethod]
    public void TestConfigSettingEntry()
    {
        var configSettingEntry = new xdrSDK.ConfigSettingEntry
        {
            Discriminant = new xdrSDK.ConfigSettingID
            {
                InnerValue = xdrSDK.ConfigSettingID.ConfigSettingIDEnum.CONFIG_SETTING_CONTRACT_MAX_SIZE_BYTES
            },
            ContractMaxSizeBytes = new xdrSDK.Uint32(111),
            BucketListSizeWindow = new[] { new xdrSDK.Uint64(1) }
        };
        var os = new xdrSDK.XdrDataOutputStream();
        // xdrSDK.ConfigSettingEntry.Encode(os, configSettingEntry);
        // var xdr = Convert.ToBase64String(os.ToArray());
        var ledgerEntry = new xdrSDK.LedgerEntry
        {
            Data = new xdrSDK.LedgerEntry.LedgerEntryData
            {
                Discriminant = new xdrSDK.LedgerEntryType
                {
                    InnerValue = xdrSDK.LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING
                },
                ConfigSetting = configSettingEntry
            },
            LastModifiedLedgerSeq = new xdrSDK.Uint32(1),
            Ext = new xdrSDK.LedgerEntry.LedgerEntryExt
            {
                Discriminant = 0
            }
        };
        xdrSDK.LedgerEntry.Encode(os, ledgerEntry);
        var xdr = Convert.ToBase64String(os.ToArray());
    }

    [TestMethod]
    public void TestOfferEntry()
    {
        var offerEntry = new xdrSDK.OfferEntry
        {
            OfferID = new xdrSDK.Int64(1),
            SellerID = new xdrSDK.AccountID(KeyPair.Random().XdrPublicKey),
            Selling = new xdrSDK.Asset
            {
                Discriminant = new xdrSDK.AssetType
                {
                    InnerValue = xdrSDK.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4
                },
                AlphaNum4 = new xdrSDK.AlphaNum4
                {
                    Issuer = new xdrSDK.AccountID(KeyPair.Random().XdrPublicKey),
                    AssetCode = new xdrSDK.AssetCode4
                    {
                        InnerValue = Encoding.UTF8.GetBytes("USDC")
                    }
                }
            },
            Buying = new xdrSDK.Asset
            {
                Discriminant = new xdrSDK.AssetType
                {
                    InnerValue = xdrSDK.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4
                },
                AlphaNum4 = new xdrSDK.AlphaNum4
                {
                    Issuer = new xdrSDK.AccountID(KeyPair.Random().XdrPublicKey),
                    AssetCode = new xdrSDK.AssetCode4
                    {
                        InnerValue = Encoding.UTF8.GetBytes("VNDC")
                    }
                }
            },
            Amount = new xdrSDK.Int64(10),
            Price = new xdrSDK.Price
            {
                N = new xdrSDK.Int32(1),
                D = new xdrSDK.Int32(10)
            },
            Flags = new xdrSDK.Uint32(3),
            Ext = new xdrSDK.OfferEntry.OfferEntryExt
            {
                Discriminant = 0
            }
        };
        var os = new xdrSDK.XdrDataOutputStream();
        xdrSDK.OfferEntry.Encode(os, offerEntry);
        var xdr = Convert.ToBase64String(os.ToArray());
    }

    [TestMethod]
    public void TestTrustlineEntry()
    {
        var trustLineAsset = new xdrSDK.TrustLineAsset
        {
            Discriminant = new xdrSDK.AssetType
            {
                InnerValue = xdrSDK.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4
            },
            AlphaNum4 = new xdrSDK.AlphaNum4
            {
                Issuer = new xdrSDK.AccountID(KeyPair.Random().XdrPublicKey),
                AssetCode = new xdrSDK.AssetCode4
                {
                    InnerValue = Encoding.UTF8.GetBytes("VNDT")
                }
            }
        };
        var trustLineEntry = new xdrSDK.TrustLineEntry
        {
            Asset = trustLineAsset,
            AccountID = new xdrSDK.AccountID(KeyPair.Random().XdrPublicKey),
            Balance = new xdrSDK.Int64(1000),
            Ext = new xdrSDK.TrustLineEntry.TrustLineEntryExt
            {
                Discriminant = 0
            },
            Flags = new xdrSDK.Uint32(1000),
            Limit = new xdrSDK.Int64(10)
        };
        var os = new xdrSDK.XdrDataOutputStream();
        xdrSDK.TrustLineEntry.Encode(os, trustLineEntry);
        var xdr = Convert.ToBase64String(os.ToArray());
    }

    [TestMethod]
    public void TestHash()
    {
        var ttl = new xdrSDK.LedgerKey.LedgerKeyTtl
        {
            KeyHash = new xdrSDK.Hash(new byte[]
                { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 })
        };
        var os = new xdrSDK.XdrDataOutputStream();
        xdrSDK.LedgerKey.LedgerKeyTtl.Encode(os, ttl);
        var xdr = Convert.ToBase64String(os.ToArray());
    }
}