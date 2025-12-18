using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Soroban;
using xdrSDK = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for <see cref="SCVal" /> class and related Soroban value types.
/// </summary>
[TestClass]
public class ScValTest
{
    private const string WasmHash = "6416281094F721A3CC324DC5A119A71101E80F17B03D92FE528AFEC56238B882";

    /// <summary>
    ///     Verifies that SCBool values round-trip correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCBoolValues_RoundTripsCorrectly()
    {
        // Arrange
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

    /// <summary>
    ///     Verifies that SCContractError round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCContractError_RoundTripsCorrectly()
    {
        // Arrange
        var error = new SCContractError(1);

        // Act
        var contractErrorXdrBase64 = error.ToXdrBase64();
        var fromXdrBase64ContractError = (SCContractError)SCVal.FromXdrBase64(contractErrorXdrBase64);

        // Assert
        Assert.AreEqual(error.ContractCode, fromXdrBase64ContractError.ContractCode);
    }

    /// <summary>
    ///     Verifies that SCWasmVmError values round-trip correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCWasmVmErrorValues_RoundTripsCorrectly()
    {
        // Arrange
        var arithDomainError = new SCWasmVmError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN,
        };
        var internalError = new SCWasmVmError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR,
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

    /// <summary>
    ///     Verifies that SCContextError values round-trip correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCContextErrorValues_RoundTripsCorrectly()
    {
        // Arrange
        var arithDomainError = new SCContextError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN,
        };
        var internalError = new SCContextError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR,
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

    /// <summary>
    ///     Verifies that SCStorageError values round-trip correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCStorageErrorValues_RoundTripsCorrectly()
    {
        // Arrange
        var arithDomainError = new SCStorageError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN,
        };
        var internalError = new SCStorageError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR,
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

    /// <summary>
    ///     Verifies that SCObjectError values round-trip correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCObjectErrorValues_RoundTripsCorrectly()
    {
        // Arrange
        var arithDomainError = new SCObjectError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN,
        };
        var internalError = new SCObjectError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR,
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

    /// <summary>
    ///     Verifies that SCCryptoError values round-trip correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCCryptoErrorValues_RoundTripsCorrectly()
    {
        // Arrange
        var arithDomainError = new SCCryptoError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN,
        };
        var internalError = new SCCryptoError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR,
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

    /// <summary>
    ///     Verifies that SCEventsError values round-trip correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCEventsErrorValues_RoundTripsCorrectly()
    {
        // Arrange
        var arithDomainError = new SCEventsError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN,
        };
        var internalError = new SCEventsError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR,
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

    /// <summary>
    ///     Verifies that SCBudgetError values round-trip correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCBudgetErrorValues_RoundTripsCorrectly()
    {
        // Arrange
        var arithDomainError = new SCBudgetError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN,
        };
        var internalError = new SCBudgetError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR,
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

    /// <summary>
    ///     Verifies that SCValueError values round-trip correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCValueErrorValues_RoundTripsCorrectly()
    {
        // Arrange
        var arithDomainError = new SCValueError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN,
        };
        var internalError = new SCValueError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR,
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

    /// <summary>
    ///     Verifies that SCAuthError values round-trip correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCAuthErrorValues_RoundTripsCorrectly()
    {
        // Arrange
        var arithDomainError = new SCAuthError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_ARITH_DOMAIN,
        };
        var internalError = new SCAuthError
        {
            Code = xdrSDK.SCErrorCode.SCErrorCodeEnum.SCEC_INTERNAL_ERROR,
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

    /// <summary>
    ///     Verifies that SCUint32 round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCUint32_RoundTripsCorrectly()
    {
        // Arrange
        var scUint32 = new SCUint32(1319013123);

        // Act
        var scUint32XdrBase64 = scUint32.ToXdrBase64();
        var xdrScUint32 = (SCUint32)SCVal.FromXdrBase64(scUint32XdrBase64);

        // Assert
        Assert.AreEqual(scUint32.InnerValue, xdrScUint32.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCInt32 round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCInt32_RoundTripsCorrectly()
    {
        // Arrange
        var scInt32 = new SCInt32(-192049123);

        // Act
        var scInt32XdrBase64 = scInt32.ToXdrBase64();
        var fromXdrBase64ScInt32 = (SCInt32)SCVal.FromXdrBase64(scInt32XdrBase64);

        // Assert
        Assert.AreEqual(scInt32.InnerValue, fromXdrBase64ScInt32.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCUint64 round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCUint64_RoundTripsCorrectly()
    {
        // Arrange
        var scUint64 = new SCUint64(18446744073709551615);

        // Act
        var scUint64XdrBase64 = scUint64.ToXdrBase64();
        var fromXdrBase64ScUint64 = (SCUint64)SCVal.FromXdrBase64(scUint64XdrBase64);

        // Assert
        Assert.AreEqual(scUint64.InnerValue, fromXdrBase64ScUint64.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCInt64 round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCInt64_RoundTripsCorrectly()
    {
        // Arrange
        var scInt64 = new SCInt64(-9223372036854775807);

        // Act
        var scInt64XdrBase64 = scInt64.ToXdrBase64();
        var fromXdrBase64ScInt64 = (SCInt64)SCVal.FromXdrBase64(scInt64XdrBase64);

        // Assert
        Assert.AreEqual(scInt64.InnerValue, fromXdrBase64ScInt64.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCTimePoint round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCTimePoint_RoundTripsCorrectly()
    {
        // Arrange
        var scTimePoint = new SCTimePoint(18446744073709551615);

        // Act
        var scTimePointXdrBase64 = scTimePoint.ToXdrBase64();
        var fromXdrBase64ScTimePoint = (SCTimePoint)SCVal.FromXdrBase64(scTimePointXdrBase64);

        // Assert
        Assert.AreEqual(scTimePoint.InnerValue, fromXdrBase64ScTimePoint.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCDuration round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCDuration_RoundTripsCorrectly()
    {
        // Arrange
        var scDuration = new SCDuration(18446744073709551615);

        // Act
        var scDurationXdrBase64 = scDuration.ToXdrBase64();
        var fromXdrBase64ScDuration = (SCDuration)SCVal.FromXdrBase64(scDurationXdrBase64);

        // Assert
        Assert.AreEqual(scDuration.InnerValue, fromXdrBase64ScDuration.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCUint128 round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCUint128_RoundTripsCorrectly()
    {
        // Arrange
        var scUint128 = new SCUint128(1, 18446744073709551615);

        // Act
        var scUint128XdrBase64 = scUint128.ToXdrBase64();
        var fromXdrBase64ScUint128 = (SCUint128)SCVal.FromXdrBase64(scUint128XdrBase64);

        // Assert
        Assert.AreEqual(scUint128.Hi, fromXdrBase64ScUint128.Hi);
        Assert.AreEqual(scUint128.Lo, fromXdrBase64ScUint128.Lo);
    }

    /// <summary>
    ///     Verifies that SCInt128 created from parts round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCInt128FromParts_RoundTripsCorrectly()
    {
        // Arrange
        var scInt128 = new SCInt128(-9223372036854775807, 18446744073709551615);

        // Act
        var scInt128XdrBase64 = scInt128.ToXdrBase64();
        var fromXdrBase64ScInt128 = (SCInt128)SCVal.FromXdrBase64(scInt128XdrBase64);

        // Assert
        Assert.AreEqual(scInt128.Lo, fromXdrBase64ScInt128.Lo);
        Assert.AreEqual(scInt128.Hi, fromXdrBase64ScInt128.Hi);
    }

    /// <summary>
    ///     Verifies that SCInt128 constructed from valid string round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCInt128FromValidString_RoundTripsCorrectly()
    {
        // Arrange
        var scInt128FromString = new SCInt128("18446744073709551616");

        var scInt128FromParts = new SCInt128(1, 0);

        // Act
        var scInt128XdrBase64 = scInt128FromString.ToXdrBase64();
        var fromXdrBase64ScInt128 = (SCInt128)SCVal.FromXdrBase64(scInt128XdrBase64);

        // Assert
        Assert.AreEqual(scInt128FromString.Lo, fromXdrBase64ScInt128.Lo);
        Assert.AreEqual(scInt128FromString.Hi, fromXdrBase64ScInt128.Hi);

        Assert.AreEqual(scInt128FromString.Lo, scInt128FromParts.Lo);
        Assert.AreEqual(scInt128FromString.Hi, scInt128FromParts.Hi);
    }

    /// <summary>
    ///     Verifies that SCInt128 constructor throws ArgumentOutOfRangeException when value exceeds maximum allowed.
    /// </summary>
    [TestMethod]
    public void Constructor_WithTooBigNumericString_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            _ = new SCInt128("170141183460469231731687303715884105728");
        });
        Assert.IsTrue(ex.Message.Contains("Value must be between -2^127 and 2^127 - 1."));
    }

    /// <summary>
    ///     Verifies that SCInt128 constructor throws ArgumentException when given invalid numeric string format.
    /// </summary>
    [TestMethod]
    public void Constructor_WithInvalidNumericString_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = Assert.ThrowsException<ArgumentException>(() => { _ = new SCInt128("9,223,372,036,854,775,807"); });
        Assert.IsTrue(ex.Message.Contains("Invalid numeric string."));
    }

    /// <summary>
    ///     Verifies that SCUint256 round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCUint256_RoundTripsCorrectly()
    {
        // Arrange
        var scUint256 = new SCUint256
        {
            HiHi = 18446744073709551615,
            HiLo = 1,
            LoHi = 18446744073709551614,
            LoLo = 0,
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

    /// <summary>
    ///     Verifies that SCInt256 round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCInt256_RoundTripsCorrectly()
    {
        // Arrange
        var scInt256 = new SCInt256
        {
            HiHi = -9223372036854775807,
            HiLo = 18446744073709551614,
            LoHi = 18446744073709551615,
            LoLo = 18446744073709551613,
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

    /// <summary>
    ///     Verifies that SCBytes with valid byte array round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCBytesWithValidArgument_RoundTripsCorrectly()
    {
        // Arrange
        byte[] bytes = { 0x00, 0x01, 0x03, 0x03, 0x34, 0x45, 0x66, 0x46 };
        var scBytes = new SCBytes(bytes);

        // Act
        var scBytesXdrBase64 = scBytes.ToXdrBase64();
        var fromXdrBase64ScBytes = (SCBytes)SCVal.FromXdrBase64(scBytesXdrBase64);

        // Assert
        CollectionAssert.AreEqual(scBytes.InnerValue, fromXdrBase64ScBytes.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCBytes with empty byte array round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCBytesWithEmptyArgument_RoundTripsCorrectly()
    {
        // Arrange
        var bytes = Array.Empty<byte>();
        var scBytes = new SCBytes(bytes);

        // Act
        var scBytesXdrBase64 = scBytes.ToXdrBase64();
        var fromXdrBase64ScBytes = (SCBytes)SCVal.FromXdrBase64(scBytesXdrBase64);

        // Assert
        CollectionAssert.AreEqual(scBytes.InnerValue, fromXdrBase64ScBytes.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCString round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCString_RoundTripsCorrectly()
    {
        // Arrange
        var scString = new SCString("hello world");

        // Act
        var scStringXdrBase64 = scString.ToXdrBase64();
        var fromXdrBase64ScString = (SCString)SCVal.FromXdrBase64(scStringXdrBase64);

        // Assert
        Assert.AreEqual(scString.InnerValue, fromXdrBase64ScString.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCString with empty string round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCStringWithEmptyArgument_RoundTripsCorrectly()
    {
        // Arrange
        var scString = new SCString("");

        // Act
        var scStringXdrBase64 = scString.ToXdrBase64();
        var fromXdrBase64ScString = (SCString)SCVal.FromXdrBase64(scStringXdrBase64);

        // Assert
        Assert.AreEqual(scString.InnerValue, fromXdrBase64ScString.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCSymbol round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCSymbol_RoundTripsCorrectly()
    {
        // Arrange
        var scSymbol = new SCSymbol("Is this a symbol?");

        // Act
        var scSymbolXdrBase64 = scSymbol.ToXdrBase64();
        var fromXdrBase64ScSymbol = (SCSymbol)SCVal.FromXdrBase64(scSymbolXdrBase64);

        // Assert
        Assert.AreEqual(scSymbol.InnerValue, fromXdrBase64ScSymbol.InnerValue);
    }

    /// <summary>
    ///     Verifies that SCVec with valid entries round-trips correctly through XDR serialization.
    /// </summary>
    /// <remarks>
    ///     It's not necessary to check each of the scVec.InnerValue element for type and properties,
    ///     since there are already other tests in the <see cref="ScValTest" /> class that cover different scenarios for
    ///     <see cref="SCVal" />
    /// </remarks>
    [TestMethod]
    public void FromXdrBase64_SCVecWithValidEntries_RoundTripsCorrectly()
    {
        // Arrange
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
        {
            Assert.AreEqual(scVec.InnerValue[i].ToXdrBase64(), fromXdrBase64ScVec.InnerValue[i].ToXdrBase64());
        }
    }

    /// <summary>
    ///     Verifies that SCMap with valid entries round-trips correctly through XDR serialization.
    /// </summary>
    /// <remarks>
    ///     It's not necessary to check each of the scMap Key and Value for type and properties,
    ///     since there are already other tests in the <see cref="ScValTest" /> class that cover different scenarios for
    ///     <see cref="SCVal" />
    /// </remarks>
    [TestMethod]
    public void FromXdrBase64_SCMapWithValidEntries_RoundTripsCorrectly()
    {
        // Arrange
        var entry1 = new SCMapEntry(new SCString("key 1"), new SCBool(false));
        var entry2 = new SCMapEntry(new SCUint32(1), new SCString("this is value 2"));
        var entry3 = new SCMapEntry(new SCUint32(1), new SCSymbol("$$$"));
        var nestedScMap = new SCMap(new[] { entry1, entry2 });
        var entry4 = new SCMapEntry(new SCUint32(1), nestedScMap);

        var mapEntries = new[] { entry1, entry2, entry3, entry3, entry4 };
        var scMap = new SCMap(mapEntries);

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

    /// <summary>
    ///     Verifies that SCMap with no entries round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCMapWithNoEntries_RoundTripsCorrectly()
    {
        // Arrange
        var scMap = new SCMap();

        // Act
        var scMapXdrBase64 = scMap.ToXdrBase64();
        var fromXdrBase64ScMap = (SCMap)SCVal.FromXdrBase64(scMapXdrBase64);

        // Assert
        Assert.AreEqual(0, fromXdrBase64ScMap.Entries.Length);
    }

    /// <summary>
    ///     Verifies that SCContractInstance with ContractExecutableWasm and missing storage round-trips correctly through XDR
    ///     serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCContractExecutableWasmWithMissingStorage_RoundTripsCorrectly()
    {
        // Arrange
        var contractExecutable = new ContractExecutableWasm(WasmHash);

        var contractInstance = new SCContractInstance(contractExecutable, null);

        // Act
        var contractInstanceXdrBase64 = contractInstance.ToXdrBase64();
        var fromXdrBase64ContractInstance = (SCContractInstance)SCVal.FromXdrBase64(contractInstanceXdrBase64);

        var decodedContractExecutable = (ContractExecutableWasm)fromXdrBase64ContractInstance.Executable;

        // Assert
        Assert.IsNull(contractInstance.Storage);
        Assert.IsNull(fromXdrBase64ContractInstance.Storage);
        Assert.AreEqual(contractExecutable.WasmHash, decodedContractExecutable.WasmHash);
    }

    /// <summary>
    ///     Verifies that SCContractInstance with ContractExecutableWasm and storage round-trips correctly through XDR
    ///     serialization.
    /// </summary>
    /// <remarks>
    ///     It's not necessary to check each of the entries element Key and Value for type and properties,
    ///     since there are already other tests in the <see cref="ScValTest" /> class that cover different scenarios for
    ///     <see cref="SCVal" />
    /// </remarks>
    [TestMethod]
    public void FromXdrBase64_SCContractExecutableWasm_RoundTripsCorrectly()
    {
        // Arrange
        var contractExecutable = new ContractExecutableWasm(WasmHash);

        var entry1 = new SCMapEntry(new SCString("key 1"), new SCBool(false));
        var entry2 = new SCMapEntry(new SCUint32(111), new SCString("2nd value"));
        var entry3 = new SCMapEntry(new SCUint32(1), new SCSymbol("&"));

        SCMapEntry[] mapEntries = { entry1, entry2, entry3 };
        var scMap = new SCMap(mapEntries);

        var contractInstance = new SCContractInstance(contractExecutable, scMap);

        // Act
        var contractInstanceXdrBase64 = contractInstance.ToXdrBase64();
        var fromXdrBase64ContractInstance = (SCContractInstance)SCVal.FromXdrBase64(contractInstanceXdrBase64);

        var decodedContractExecutable = (ContractExecutableWasm)fromXdrBase64ContractInstance.Executable;

        // Assert
        Assert.IsNotNull(contractInstance.Storage);
        var entries = contractInstance.Storage.Entries;
        Assert.IsNotNull(fromXdrBase64ContractInstance.Storage);
        var decodedEntries = fromXdrBase64ContractInstance.Storage.Entries;

        Assert.AreEqual(contractExecutable.WasmHash, decodedContractExecutable.WasmHash);
        Assert.AreEqual(entries.Length, decodedEntries.Length);
        for (var i = 0; i < entries.Length; i++)
        {
            Assert.AreEqual(entries[i].Key.ToXdrBase64(), decodedEntries[i].Key.ToXdrBase64());
            Assert.AreEqual(entries[i].Value.ToXdrBase64(), decodedEntries[i].Value.ToXdrBase64());
        }
    }

    /// <summary>
    ///     Verifies that SCContractInstance with ContractExecutableStellarAsset round-trips correctly through XDR
    ///     serialization.
    /// </summary>
    /// <remarks>
    ///     It's not necessary to check each of the entries element Key and Value for type and properties,
    ///     since there are already other tests in the <see cref="ScValTest" /> class that cover different scenarios for
    ///     <see cref="SCVal" />
    /// </remarks>
    [TestMethod]
    public void FromXdrBase64_SCContractExecutableStellarAsset_RoundTripsCorrectly()
    {
        // Arrange
        var entry1 = new SCMapEntry(new SCString("key 1"), new SCBool(false));
        var entry2 = new SCMapEntry(new SCUint32(111), new SCString("2nd value"));
        var entry3 = new SCMapEntry(new SCUint32(1), new SCSymbol("&"));

        SCMapEntry[] mapEntries = { entry1, entry2, entry3 };
        var scMap = new SCMap(mapEntries);

        var contractInstance = new SCContractInstance(new ContractExecutableStellarAsset(), scMap);

        // Act
        var contractInstanceXdrBase64 = contractInstance.ToXdrBase64();
        var fromXdrBase64ContractInstance = (SCContractInstance)SCVal.FromXdrBase64(contractInstanceXdrBase64);

        // Assert
        Assert.IsNotNull(contractInstance.Storage);
        Assert.IsNotNull(fromXdrBase64ContractInstance.Storage);
        var entries = contractInstance.Storage.Entries;
        var decodedEntries = fromXdrBase64ContractInstance.Storage.Entries;

        Assert.AreEqual(entries.Length, decodedEntries.Length);
        for (var i = 0; i < entries.Length; i++)
        {
            Assert.AreEqual(entries[i].Key.ToXdrBase64(), decodedEntries[i].Key.ToXdrBase64());
            Assert.AreEqual(entries[i].Value.ToXdrBase64(), decodedEntries[i].Value.ToXdrBase64());
        }
    }

    /// <summary>
    ///     Verifies that SCNonceKey round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void FromXdrBase64_SCNonceKey_RoundTripsCorrectly()
    {
        // Arrange
        var scNonceKey = new SCNonceKey(-9223372036854775807);

        // Act
        var scNonceKeyXdrBase64 = scNonceKey.ToXdrBase64();
        var fromXdrBase64ScNonceKey = (SCNonceKey)SCVal.FromXdrBase64(scNonceKeyXdrBase64);

        // Assert
        Assert.AreEqual(scNonceKey.Nonce, fromXdrBase64ScNonceKey.Nonce);
    }
}