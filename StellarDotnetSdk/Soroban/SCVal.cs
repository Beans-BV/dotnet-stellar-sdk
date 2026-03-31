using System;
using System.Linq;
using System.Numerics;
using StellarDotnetSdk.Xdr;
using Int32 = StellarDotnetSdk.Xdr.Int32;
using Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Custom types for smart contracts.
///     <seealso href="https://developers.stellar.org/docs/learn/smart-contract-internals/types/custom-types" />
/// </summary>
public abstract class SCVal
{
    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object.</returns>
    public Xdr.SCVal ToXdr()
    {
        return this switch
        {
            SCBool scBool => scBool.ToSCValXdr(),
            SCVoid scVoid => scVoid.ToSCValXdr(),
            SCError scError => scError.ToSCValXdr(),
            SCUint32 scUint32 => scUint32.ToSCValXdr(),
            SCInt32 scInt32 => scInt32.ToSCValXdr(),
            SCUint64 scUint64 => scUint64.ToSCValXdr(),
            SCInt64 scInt64 => scInt64.ToSCValXdr(),
            SCTimePoint scTimePoint => scTimePoint.ToSCValXdr(),
            SCDuration scDuration => scDuration.ToSCValXdr(),
            SCUint128 scUint128 => scUint128.ToSCValXdr(),
            SCInt128 scInt128 => scInt128.ToSCValXdr(),
            SCUint256 scUint256 => scUint256.ToSCValXdr(),
            SCInt256 scInt256 => scInt256.ToSCValXdr(),
            SCBytes scBytes => scBytes.ToSCValXdr(),
            SCString scString => scString.ToSCValXdr(),
            SCSymbol scSymbol => scSymbol.ToSCValXdr(),
            SCVec scVec => scVec.ToSCValXdr(),
            SCMap scMap => scMap.ToSCValXdr(),
            ScAddress scAddress => scAddress.ToSCValXdr(),
            SCContractInstance scContractInstance => scContractInstance.ToSCValXdr(),
            SCLedgerKeyContractInstance scLedgerKeyContractInstance => scLedgerKeyContractInstance.ToSCValXdr(),
            SCNonceKey scNonceKey => scNonceKey.ToSCValXdr(),
            _ => throw new InvalidOperationException("Unknown SCVal type"),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCVal" /> subclass from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>The appropriate <see cref="SCVal" /> subclass instance.</returns>
    public static SCVal FromXdr(Xdr.SCVal xdrVal)
    {
        return xdrVal.Discriminant.InnerValue switch
        {
            SCValType.SCValTypeEnum.SCV_BOOL => SCBool.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_VOID => SCVoid.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_ERROR => SCError.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_U32 => SCUint32.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_I32 => SCInt32.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_U64 => SCUint64.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_I64 => SCInt64.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_TIMEPOINT => SCTimePoint.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_DURATION => SCDuration.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_U128 => SCUint128.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_I128 => SCInt128.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_U256 => SCUint256.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_I256 => SCInt256.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_BYTES => SCBytes.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_STRING => SCString.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_SYMBOL => SCSymbol.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_VEC => SCVec.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_MAP => SCMap.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_ADDRESS => ScAddress.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_CONTRACT_INSTANCE => SCContractInstance.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_LEDGER_KEY_CONTRACT_INSTANCE =>
                SCLedgerKeyContractInstance.FromSCValXdr(xdrVal),
            SCValType.SCValTypeEnum.SCV_LEDGER_KEY_NONCE => SCNonceKey.FromSCValXdr(xdrVal),
            _ => throw new InvalidOperationException("Unknown SCVal type"),
        };
    }

    /// <summary>
    ///     Creates a new SCVal object from the given SCVal XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>SCVal object</returns>
    public static SCVal FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = Xdr.SCVal.Decode(reader);
        return FromXdr(thisXdr);
    }

    /// <summary>
    ///     Returns base64-encoded SCVal XDR object.
    /// </summary>
    public string ToXdrBase64()
    {
        var xdrValue = ToXdr();
        var writer = new XdrDataOutputStream();
        Xdr.SCVal.Encode(writer, xdrValue);
        return Convert.ToBase64String(writer.ToArray());
    }
}

/// <summary>
///     Represents a Soroban boolean value.
/// </summary>
public class SCBool : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCBool" /> with the specified boolean value.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public SCBool(bool value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The boolean value.
    /// </summary>
    public bool InnerValue { get; set; }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>The boolean value.</returns>
    public new bool ToXdr()
    {
        return InnerValue;
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_BOOL</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_BOOL,
            },
            B = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCBool" /> from a boolean value.
    /// </summary>
    /// <param name="xdrBool">The boolean value.</param>
    /// <returns>An <see cref="SCBool" /> instance.</returns>
    public static SCBool FromXdr(bool xdrBool)
    {
        return new SCBool(xdrBool);
    }

    /// <summary>
    ///     Creates a new <see cref="SCBool" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCBool" /> instance.</returns>
    public static SCBool FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_BOOL)
        {
            throw new ArgumentException("Not an SCBool.", nameof(xdrVal));
        }

        return FromXdr(xdrVal.B);
    }
}

/// <summary>
///     Represents a Soroban void value, carrying no data.
/// </summary>
public class SCVoid : SCVal
{
    /// <summary>
    ///     No-op conversion; void values carry no data.
    /// </summary>
    public new void ToXdr()
    {
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_VOID</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_VOID,
            },
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCVoid" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCVoid" /> instance.</returns>
    public static SCVoid FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_VOID)
        {
            throw new ArgumentException("Not an SCVoid", nameof(xdrVal));
        }

        return FromXdr();
    }

    /// <summary>
    ///     Creates a new <see cref="SCVoid" /> instance.
    /// </summary>
    /// <returns>An <see cref="SCVoid" /> instance.</returns>
    public static SCVoid FromXdr()
    {
        return new SCVoid();
    }
}

/// <summary>
///     Base class for Soroban runtime errors, discriminated by <see cref="SCErrorType" />.
/// </summary>
public abstract class SCError : SCVal
{
    /// <summary>
    ///     The error code within the specific error type.
    /// </summary>
    public SCErrorCode.SCErrorCodeEnum Code { get; set; }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToXdr()
    {
        return this switch
        {
            SCContractError scContractError => scContractError.ToSCErrorXdr(),
            SCWasmVmError scWasmVmError => scWasmVmError.ToSCErrorXdr(),
            SCContextError scContextError => scContextError.ToSCErrorXdr(),
            SCStorageError scStorageError => scStorageError.ToSCErrorXdr(),
            SCObjectError scObjectError => scObjectError.ToSCErrorXdr(),
            SCCryptoError scCryptoError => scCryptoError.ToSCErrorXdr(),
            SCEventsError scEventsError => scEventsError.ToSCErrorXdr(),
            SCBudgetError scBudgetError => scBudgetError.ToSCErrorXdr(),
            SCValueError scValueError => scValueError.ToSCErrorXdr(),
            SCAuthError scAuthError => scAuthError.ToSCErrorXdr(),
            _ => throw new InvalidOperationException("Unknown SCVal type"),
        };
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_ERROR</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_ERROR,
            },
            Error = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCError" /> subclass from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>The appropriate <see cref="SCError" /> subclass instance.</returns>
    public static SCError FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_ERROR)
        {
            throw new ArgumentException("Not an SCError", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Error);
    }

    /// <summary>
    ///     Creates a new <see cref="SCError" /> subclass from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>The appropriate <see cref="SCError" /> subclass instance.</returns>
    public static SCError FromXdr(Xdr.SCError xdrSCError)
    {
        return xdrSCError.Discriminant.InnerValue switch
        {
            SCErrorType.SCErrorTypeEnum.SCE_CONTRACT => SCContractError.FromSCErrorXdr(xdrSCError),
            SCErrorType.SCErrorTypeEnum.SCE_WASM_VM => SCWasmVmError.FromSCErrorXdr(xdrSCError),
            SCErrorType.SCErrorTypeEnum.SCE_CONTEXT => SCContextError.FromSCErrorXdr(xdrSCError),
            SCErrorType.SCErrorTypeEnum.SCE_STORAGE => SCStorageError.FromSCErrorXdr(xdrSCError),
            SCErrorType.SCErrorTypeEnum.SCE_OBJECT => SCObjectError.FromSCErrorXdr(xdrSCError),
            SCErrorType.SCErrorTypeEnum.SCE_CRYPTO => SCCryptoError.FromSCErrorXdr(xdrSCError),
            SCErrorType.SCErrorTypeEnum.SCE_EVENTS => SCEventsError.FromSCErrorXdr(xdrSCError),
            SCErrorType.SCErrorTypeEnum.SCE_BUDGET => SCBudgetError.FromSCErrorXdr(xdrSCError),
            SCErrorType.SCErrorTypeEnum.SCE_VALUE => SCValueError.FromSCErrorXdr(xdrSCError),
            SCErrorType.SCErrorTypeEnum.SCE_AUTH => SCAuthError.FromSCErrorXdr(xdrSCError),
            _ => throw new InvalidOperationException("Unknown SCError type"),
        };
    }
}

/// <summary>
///     Represents a Soroban contract error, containing a contract-defined error code.
/// </summary>
public class SCContractError : SCError
{
    /// <summary>
    ///     Initializes a new <see cref="SCContractError" /> with the specified contract-defined error code.
    /// </summary>
    /// <param name="value">The contract-defined error code.</param>
    public SCContractError(uint value)
    {
        ContractCode = value;
    }

    /// <summary>
    ///     The contract-defined error code.
    /// </summary>
    public uint ContractCode { get; set; }

    /// <summary>
    ///     Creates a new <see cref="SCContractError" /> from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>An <see cref="SCContractError" /> instance.</returns>
    public static SCContractError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCContractError(xdrSCError.ContractCode.InnerValue);
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToSCErrorXdr()
    {
        return new Xdr.SCError
        {
            Discriminant = new SCErrorType
            {
                InnerValue = SCErrorType.SCErrorTypeEnum.SCE_CONTRACT,
            },
            ContractCode = new Uint32(ContractCode),
        };
    }
}

/// <summary>
///     Represents a Soroban WebAssembly VM error.
/// </summary>
public class SCWasmVmError : SCError
{
    /// <summary>
    ///     Creates a new <see cref="SCWasmVmError" /> from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>An <see cref="SCWasmVmError" /> instance.</returns>
    public static SCWasmVmError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCWasmVmError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToSCErrorXdr()
    {
        return new Xdr.SCError
        {
            Discriminant = new SCErrorType
            {
                InnerValue = SCErrorType.SCErrorTypeEnum.SCE_WASM_VM,
            },
            Code = SCErrorCode.Create(Code),
        };
    }
}

/// <summary>
///     Represents a Soroban context error.
/// </summary>
public class SCContextError : SCError
{
    /// <summary>
    ///     Creates a new <see cref="SCContextError" /> from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>An <see cref="SCContextError" /> instance.</returns>
    public static SCContextError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCContextError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToSCErrorXdr()
    {
        return new Xdr.SCError
        {
            Discriminant = new SCErrorType
            {
                InnerValue = SCErrorType.SCErrorTypeEnum.SCE_CONTEXT,
            },
            Code = SCErrorCode.Create(Code),
        };
    }
}

/// <summary>
///     Represents a Soroban storage error.
/// </summary>
public class SCStorageError : SCError
{
    /// <summary>
    ///     Creates a new <see cref="SCStorageError" /> from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>An <see cref="SCStorageError" /> instance.</returns>
    public static SCStorageError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCStorageError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToSCErrorXdr()
    {
        return new Xdr.SCError
        {
            Discriminant = new SCErrorType
            {
                InnerValue = SCErrorType.SCErrorTypeEnum.SCE_STORAGE,
            },
            Code = SCErrorCode.Create(Code),
        };
    }
}

/// <summary>
///     Represents a Soroban object error.
/// </summary>
public class SCObjectError : SCError
{
    /// <summary>
    ///     Creates a new <see cref="SCObjectError" /> from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>An <see cref="SCObjectError" /> instance.</returns>
    public static SCObjectError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCObjectError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToSCErrorXdr()
    {
        return new Xdr.SCError
        {
            Discriminant = new SCErrorType
            {
                InnerValue = SCErrorType.SCErrorTypeEnum.SCE_OBJECT,
            },
            Code = SCErrorCode.Create(Code),
        };
    }
}

/// <summary>
///     Represents a Soroban cryptography error.
/// </summary>
public class SCCryptoError : SCError
{
    /// <summary>
    ///     Creates a new <see cref="SCCryptoError" /> from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>An <see cref="SCCryptoError" /> instance.</returns>
    public static SCCryptoError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCCryptoError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToSCErrorXdr()
    {
        return new Xdr.SCError
        {
            Discriminant = new SCErrorType
            {
                InnerValue = SCErrorType.SCErrorTypeEnum.SCE_CRYPTO,
            },
            Code = SCErrorCode.Create(Code),
        };
    }
}

/// <summary>
///     Represents a Soroban events error.
/// </summary>
public class SCEventsError : SCError
{
    /// <summary>
    ///     Creates a new <see cref="SCEventsError" /> from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>An <see cref="SCEventsError" /> instance.</returns>
    public static SCEventsError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCEventsError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToSCErrorXdr()
    {
        return new Xdr.SCError
        {
            Discriminant = new SCErrorType
            {
                InnerValue = SCErrorType.SCErrorTypeEnum.SCE_EVENTS,
            },
            Code = SCErrorCode.Create(Code),
        };
    }
}

/// <summary>
///     Represents a Soroban budget error.
/// </summary>
public class SCBudgetError : SCError
{
    /// <summary>
    ///     Creates a new <see cref="SCBudgetError" /> from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>An <see cref="SCBudgetError" /> instance.</returns>
    public static SCBudgetError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCBudgetError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToSCErrorXdr()
    {
        return new Xdr.SCError
        {
            Discriminant = new SCErrorType
            {
                InnerValue = SCErrorType.SCErrorTypeEnum.SCE_BUDGET,
            },
            Code = SCErrorCode.Create(Code),
        };
    }
}

/// <summary>
///     Represents a Soroban value error.
/// </summary>
public class SCValueError : SCError
{
    /// <summary>
    ///     Creates a new <see cref="SCValueError" /> from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>An <see cref="SCValueError" /> instance.</returns>
    public static SCValueError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCValueError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToSCErrorXdr()
    {
        return new Xdr.SCError
        {
            Discriminant = new SCErrorType
            {
                InnerValue = SCErrorType.SCErrorTypeEnum.SCE_VALUE,
            },
            Code = SCErrorCode.Create(Code),
        };
    }
}

/// <summary>
///     Represents a Soroban authentication error.
/// </summary>
public class SCAuthError : SCError
{
    /// <summary>
    ///     Creates a new <see cref="SCAuthError" /> from an XDR <see cref="Xdr.SCError" /> object.
    /// </summary>
    /// <param name="xdrSCError">The XDR error to convert.</param>
    /// <returns>An <see cref="SCAuthError" /> instance.</returns>
    public static SCAuthError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCAuthError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCError" /> XDR object.</returns>
    public Xdr.SCError ToSCErrorXdr()
    {
        return new Xdr.SCError
        {
            Discriminant = new SCErrorType
            {
                InnerValue = SCErrorType.SCErrorTypeEnum.SCE_AUTH,
            },
            Code = SCErrorCode.Create(Code),
        };
    }
}

/// <summary>
///     Represents a Soroban unsigned 32-bit integer value.
/// </summary>
public class SCUint32 : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCUint32" /> with the specified value.
    /// </summary>
    /// <param name="value">The unsigned 32-bit integer value.</param>
    public SCUint32(uint value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The unsigned 32-bit integer value.
    /// </summary>
    public uint InnerValue { get; set; }

    /// <summary>
    ///     Converts this value to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="Uint32" /> XDR object.</returns>
    public Uint32 ToXdr()
    {
        return new Uint32(InnerValue);
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_U32</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_U32,
            },
            U32 = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCUint32" /> from an XDR <see cref="Uint32" /> object.
    /// </summary>
    /// <param name="xdrUint32">The XDR value to convert.</param>
    /// <returns>An <see cref="SCUint32" /> instance.</returns>
    public static SCUint32 FromXdr(Uint32 xdrUint32)
    {
        return new SCUint32(xdrUint32.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCUint32" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCUint32" /> instance.</returns>
    public static SCUint32 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_U32)
        {
            throw new ArgumentException("Not an SCUint32", nameof(xdrVal));
        }

        return FromXdr(xdrVal.U32);
    }
}

/// <summary>
///     Represents a Soroban signed 32-bit integer value.
/// </summary>
public class SCInt32 : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCInt32" /> with the specified value.
    /// </summary>
    /// <param name="value">The signed 32-bit integer value.</param>
    public SCInt32(int value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The signed 32-bit integer value.
    /// </summary>
    public int InnerValue { get; set; }

    /// <summary>
    ///     Converts this value to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Int32" /> XDR object.</returns>
    public Int32 ToXdr()
    {
        return new Int32(InnerValue);
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_I32</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_I32,
            },
            I32 = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCInt32" /> from an XDR <see cref="Int32" /> object.
    /// </summary>
    /// <param name="xdrInt32">The XDR value to convert.</param>
    /// <returns>An <see cref="SCInt32" /> instance.</returns>
    public static SCInt32 FromXdr(Int32 xdrInt32)
    {
        return new SCInt32(xdrInt32.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCInt32" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCInt32" /> instance.</returns>
    public static SCInt32 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_I32)
        {
            throw new ArgumentException("Not an SCInt32", nameof(xdrVal));
        }

        return FromXdr(xdrVal.I32);
    }
}

/// <summary>
///     Represents a Soroban unsigned 64-bit integer value.
/// </summary>
public class SCUint64 : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCUint64" /> with the specified value.
    /// </summary>
    /// <param name="value">The unsigned 64-bit integer value.</param>
    public SCUint64(ulong value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The unsigned 64-bit integer value.
    /// </summary>
    public ulong InnerValue { get; set; }

    /// <summary>
    ///     Converts this value to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="Uint64" /> XDR object.</returns>
    public Uint64 ToXdr()
    {
        return new Uint64(InnerValue);
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_U64</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_U64,
            },
            U64 = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCUint64" /> from an XDR <see cref="Uint64" /> object.
    /// </summary>
    /// <param name="xdrUint64">The XDR value to convert.</param>
    /// <returns>An <see cref="SCUint64" /> instance.</returns>
    public static SCUint64 FromXdr(Uint64 xdrUint64)
    {
        return new SCUint64(xdrUint64.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCUint64" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCUint64" /> instance.</returns>
    public static SCUint64 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_U64)
        {
            throw new ArgumentException("Not an SCUint64", nameof(xdrVal));
        }

        return FromXdr(xdrVal.U64);
    }
}

/// <summary>
///     Represents a Soroban signed 64-bit integer value.
/// </summary>
public class SCInt64 : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCInt64" /> with the specified value.
    /// </summary>
    /// <param name="value">The signed 64-bit integer value.</param>
    public SCInt64(long value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The signed 64-bit integer value.
    /// </summary>
    public long InnerValue { get; set; }

    /// <summary>
    ///     Converts this value to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Int64" /> XDR object.</returns>
    public Int64 ToXdr()
    {
        return new Int64(InnerValue);
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_I64</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_I64,
            },
            I64 = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCInt64" /> from an XDR <see cref="Int64" /> object.
    /// </summary>
    /// <param name="xdrInt64">The XDR value to convert.</param>
    /// <returns>An <see cref="SCInt64" /> instance.</returns>
    public static SCInt64 FromXdr(Int64 xdrInt64)
    {
        return new SCInt64(xdrInt64.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCInt64" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCInt64" /> instance.</returns>
    public static SCInt64 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_I64)
        {
            throw new ArgumentException("Not an SCInt64", nameof(xdrVal));
        }

        return FromXdr(xdrVal.I64);
    }
}

/// <summary>
///     Represents a Soroban time point value (a UNIX timestamp as an unsigned 64-bit integer).
/// </summary>
public class SCTimePoint : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCTimePoint" /> with the specified UNIX timestamp.
    /// </summary>
    /// <param name="value">A UNIX timestamp as an unsigned 64-bit integer.</param>
    public SCTimePoint(ulong value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The UNIX timestamp as an unsigned 64-bit integer.
    /// </summary>
    public ulong InnerValue { get; set; }

    /// <summary>
    ///     Converts this value to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="TimePoint" /> XDR object.</returns>
    public TimePoint ToXdr()
    {
        return new TimePoint(new Uint64(InnerValue));
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_TIMEPOINT</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_TIMEPOINT,
            },
            Timepoint = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCTimePoint" /> from an XDR <see cref="TimePoint" /> object.
    /// </summary>
    /// <param name="xdrTimePoint">The XDR value to convert.</param>
    /// <returns>An <see cref="SCTimePoint" /> instance.</returns>
    public static SCTimePoint FromXdr(TimePoint xdrTimePoint)
    {
        return new SCTimePoint(xdrTimePoint.InnerValue.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCTimePoint" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCTimePoint" /> instance.</returns>
    public static SCTimePoint FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_TIMEPOINT)
        {
            throw new ArgumentException("Not an SCTimePoint", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Timepoint);
    }
}

/// <summary>
///     Represents a Soroban duration value (a time span as an unsigned 64-bit integer).
/// </summary>
public class SCDuration : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCDuration" /> with the specified duration.
    /// </summary>
    /// <param name="value">A time span as an unsigned 64-bit integer.</param>
    public SCDuration(ulong value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The duration as an unsigned 64-bit integer.
    /// </summary>
    public ulong InnerValue { get; set; }

    /// <summary>
    ///     Converts this value to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="Duration" /> XDR object.</returns>
    public Duration ToXdr()
    {
        return new Duration(new Uint64(InnerValue));
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_DURATION</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_DURATION,
            },
            Duration = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCDuration" /> from an XDR <see cref="Duration" /> object.
    /// </summary>
    /// <param name="xdrDuration">The XDR value to convert.</param>
    /// <returns>An <see cref="SCDuration" /> instance.</returns>
    public static SCDuration FromXdr(Duration xdrDuration)
    {
        return new SCDuration(xdrDuration.InnerValue.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCDuration" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCDuration" /> instance.</returns>
    public static SCDuration FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_DURATION)
        {
            throw new ArgumentException("Not an SCDuration", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Duration);
    }
}

/// <summary>
///     Represents a Soroban unsigned 128-bit integer value, stored as high and low 64-bit parts.
/// </summary>
public class SCUint128 : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCUint128" /> from low and high 64-bit parts.
    /// </summary>
    /// <param name="lo">The low 64 bits.</param>
    /// <param name="hi">The high 64 bits.</param>
    public SCUint128(ulong lo, ulong hi)
    {
        Hi = hi;
        Lo = lo;
    }

    /// <summary>
    ///     The low 64 bits of the 128-bit value.
    /// </summary>
    public ulong Lo { get; set; }

    /// <summary>
    ///     The high 64 bits of the 128-bit value.
    /// </summary>
    public ulong Hi { get; set; }

    /// <summary>
    ///     Converts this value to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="UInt128Parts" /> XDR object.</returns>
    public UInt128Parts ToXdr()
    {
        return new UInt128Parts
        {
            Lo = new Uint64(Lo),
            Hi = new Uint64(Hi),
        };
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_U128</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_U128,
            },
            U128 = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCUint128" /> from an XDR <see cref="UInt128Parts" /> object.
    /// </summary>
    /// <param name="xdrUInt128Parts">The XDR value to convert.</param>
    /// <returns>An <see cref="SCUint128" /> instance.</returns>
    public static SCUint128 FromXdr(UInt128Parts xdrUInt128Parts)
    {
        return new SCUint128(xdrUInt128Parts.Lo.InnerValue, xdrUInt128Parts.Hi.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCUint128" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCUint128" /> instance.</returns>
    public static SCUint128 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_U128)
        {
            throw new ArgumentException("Not an SCUint128", nameof(xdrVal));
        }

        return FromXdr(xdrVal.U128);
    }
}

/// <summary>
///     Represents a Soroban signed 128-bit integer value, stored as high and low 64-bit parts.
/// </summary>
public class SCInt128 : SCVal
{
    /// <summary>
    ///     Constructs a new SCInt128 object from high and low parts.
    /// </summary>
    /// <param name="hi">High parts.</param>
    /// <param name="lo">Low parts.</param>
    public SCInt128(long hi, ulong lo)
    {
        Hi = hi;
        Lo = lo;
    }

    /// <summary>
    ///     Constructs a new SCInt128 object from a numeric string.
    /// </summary>
    /// <param name="input">A string represents a 128-bit signed integer.</param>
    public SCInt128(string input)
    {
        if (!BigInteger.TryParse(input, out var bigInt))
        {
            throw new ArgumentException("Invalid numeric string.", nameof(input));
        }
        if (bigInt < BigInteger.MinusOne << 127 || bigInt > (BigInteger.One << 127) - 1)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Value must be between -2^127 and 2^127 - 1.");
        }
        var low = (ulong)(bigInt & ulong.MaxValue);
        var high = (long)(bigInt >> 64);
        Hi = high;
        Lo = low;
    }

    /// <summary>
    ///     The low 64 bits of the 128-bit value.
    /// </summary>
    public ulong Lo { get; set; }

    /// <summary>
    ///     The high 64 bits of the 128-bit value (signed).
    /// </summary>
    public long Hi { get; set; }

    /// <summary>
    ///     Converts this value to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Int128Parts" /> XDR object.</returns>
    public Int128Parts ToXdr()
    {
        return new Int128Parts
        {
            Lo = new Uint64(Lo),
            Hi = new Int64(Hi),
        };
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_I128</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_I128,
            },
            I128 = new Int128Parts
            {
                Hi = new Int64(Hi),
                Lo = new Uint64(Lo),
            },
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCInt128" /> from an XDR <see cref="Int128Parts" /> object.
    /// </summary>
    /// <param name="xdrInt128Parts">The XDR value to convert.</param>
    /// <returns>An <see cref="SCInt128" /> instance.</returns>
    public static SCInt128 FromXdr(Int128Parts xdrInt128Parts)
    {
        return new SCInt128(xdrInt128Parts.Hi.InnerValue, xdrInt128Parts.Lo.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCInt128" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCInt128" /> instance.</returns>
    public static SCInt128 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_I128)
        {
            throw new ArgumentException("Not an SCInt128", nameof(xdrVal));
        }

        return new SCInt128(xdrVal.I128.Hi.InnerValue, xdrVal.I128.Lo.InnerValue);
    }
}

/// <summary>
///     Represents a Soroban unsigned 256-bit integer value, stored as four 64-bit parts.
/// </summary>
public class SCUint256 : SCVal
{
    /// <summary>
    ///     The most significant 64 bits (bits 192-255).
    /// </summary>
    public ulong HiHi { get; set; }

    /// <summary>
    ///     The second most significant 64 bits (bits 128-191).
    /// </summary>
    public ulong HiLo { get; set; }

    /// <summary>
    ///     The second least significant 64 bits (bits 64-127).
    /// </summary>
    public ulong LoHi { get; set; }

    /// <summary>
    ///     The least significant 64 bits (bits 0-63).
    /// </summary>
    public ulong LoLo { get; set; }

    /// <summary>
    ///     Converts this value to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="UInt256Parts" /> XDR object.</returns>
    public UInt256Parts ToXdr()
    {
        return new UInt256Parts
        {
            HiHi = new Uint64(HiHi),
            HiLo = new Uint64(HiLo),
            LoHi = new Uint64(LoHi),
            LoLo = new Uint64(LoLo),
        };
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_U256</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_U256,
            },
            U256 = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCUint256" /> from an XDR <see cref="UInt256Parts" /> object.
    /// </summary>
    /// <param name="xdrUInt256Parts">The XDR value to convert.</param>
    /// <returns>An <see cref="SCUint256" /> instance.</returns>
    public static SCUint256 FromXdr(UInt256Parts xdrUInt256Parts)
    {
        return new SCUint256
        {
            HiHi = xdrUInt256Parts.HiHi.InnerValue,
            HiLo = xdrUInt256Parts.HiLo.InnerValue,
            LoHi = xdrUInt256Parts.LoHi.InnerValue,
            LoLo = xdrUInt256Parts.LoLo.InnerValue,
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCUint256" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCUint256" /> instance.</returns>
    public static SCUint256 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_U256)
        {
            throw new ArgumentException("Not an SCUint256", nameof(xdrVal));
        }

        return FromXdr(xdrVal.U256);
    }
}

/// <summary>
///     Represents a Soroban signed 256-bit integer value, stored as four 64-bit parts.
/// </summary>
public class SCInt256 : SCVal
{
    /// <summary>
    ///     The most significant 64 bits (bits 192-255, signed).
    /// </summary>
    public long HiHi { get; set; }

    /// <summary>
    ///     The second most significant 64 bits (bits 128-191).
    /// </summary>
    public ulong HiLo { get; set; }

    /// <summary>
    ///     The second least significant 64 bits (bits 64-127).
    /// </summary>
    public ulong LoHi { get; set; }

    /// <summary>
    ///     The least significant 64 bits (bits 0-63).
    /// </summary>
    public ulong LoLo { get; set; }

    /// <summary>
    ///     Converts this value to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Int256Parts" /> XDR object.</returns>
    public Int256Parts ToXdr()
    {
        return new Int256Parts
        {
            HiHi = new Int64(HiHi),
            HiLo = new Uint64(HiLo),
            LoHi = new Uint64(LoHi),
            LoLo = new Uint64(LoLo),
        };
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_I256</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_I256,
            },
            I256 = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCInt256" /> from an XDR <see cref="Int256Parts" /> object.
    /// </summary>
    /// <param name="xdrInt256Parts">The XDR value to convert.</param>
    /// <returns>An <see cref="SCInt256" /> instance.</returns>
    public static SCInt256 FromXdr(Int256Parts xdrInt256Parts)
    {
        return new SCInt256
        {
            HiHi = xdrInt256Parts.HiHi.InnerValue,
            HiLo = xdrInt256Parts.HiLo.InnerValue,
            LoHi = xdrInt256Parts.LoHi.InnerValue,
            LoLo = xdrInt256Parts.LoLo.InnerValue,
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCInt256" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCInt256" /> instance.</returns>
    public static SCInt256 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_I256)
        {
            throw new ArgumentException("Not an SCInt256", nameof(xdrVal));
        }

        return FromXdr(xdrVal.I256);
    }
}

/// <summary>
///     Represents a Soroban byte array value.
/// </summary>
public class SCBytes : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCBytes" /> with the specified byte array.
    /// </summary>
    /// <param name="value">The byte array.</param>
    public SCBytes(byte[] value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The byte array value.
    /// </summary>
    public byte[] InnerValue { get; set; }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCBytes" /> XDR object.</returns>
    public Xdr.SCBytes ToXdr()
    {
        return new Xdr.SCBytes(InnerValue);
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_BYTES</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_BYTES,
            },
            Bytes = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCBytes" /> from an XDR <see cref="Xdr.SCBytes" /> object.
    /// </summary>
    /// <param name="xdrSCBytes">The XDR value to convert.</param>
    /// <returns>An <see cref="SCBytes" /> instance.</returns>
    public static SCBytes FromXdr(Xdr.SCBytes xdrSCBytes)
    {
        return new SCBytes(xdrSCBytes.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCBytes" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCBytes" /> instance.</returns>
    public static SCBytes FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_BYTES)
        {
            throw new ArgumentException("Not an SCBytes", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Bytes);
    }
}

/// <summary>
///     Represents a Soroban string value.
/// </summary>
public class SCString : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCString" /> with the specified string value.
    /// </summary>
    /// <param name="value">The string value.</param>
    public SCString(string value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The string value.
    /// </summary>
    public string InnerValue { get; set; }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCString" /> XDR object.</returns>
    public Xdr.SCString ToXdr()
    {
        return new Xdr.SCString(InnerValue);
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_STRING</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_STRING,
            },
            Str = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCString" /> from an XDR <see cref="Xdr.SCString" /> object.
    /// </summary>
    /// <param name="xdrSCString">The XDR value to convert.</param>
    /// <returns>An <see cref="SCString" /> instance.</returns>
    public static SCString FromXdr(Xdr.SCString xdrSCString)
    {
        return new SCString(xdrSCString.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCString" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCString" /> instance.</returns>
    public static SCString FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_STRING)
        {
            throw new ArgumentException("Not an SCString", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Str);
    }
}

/// <summary>
///     Represents a Soroban symbol value, a short identifier string used for function names and map keys.
/// </summary>
public class SCSymbol : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCSymbol" /> with the specified symbol string.
    /// </summary>
    /// <param name="innerValue">The symbol string (up to 32 characters of [a-zA-Z0-9_]).</param>
    public SCSymbol(string innerValue)
    {
        InnerValue = innerValue;
    }

    /// <summary>
    ///     The symbol string value.
    /// </summary>
    public string InnerValue { get; }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCSymbol" /> XDR object.</returns>
    public Xdr.SCSymbol ToXdr()
    {
        return new Xdr.SCSymbol(InnerValue);
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_SYMBOL</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_SYMBOL,
            },
            Sym = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCSymbol" /> from an XDR <see cref="Xdr.SCSymbol" /> object.
    /// </summary>
    /// <param name="xdrSCSymbol">The XDR value to convert.</param>
    /// <returns>An <see cref="SCSymbol" /> instance.</returns>
    public static SCSymbol FromXdr(Xdr.SCSymbol xdrSCSymbol)
    {
        return new SCSymbol(xdrSCSymbol.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCSymbol" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCSymbol" /> instance.</returns>
    public static SCSymbol FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_SYMBOL)
        {
            throw new ArgumentException("Not an SCSymbol", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Sym);
    }
}

/// <summary>
///     Represents a Soroban vector (ordered collection) of <see cref="SCVal" /> elements.
/// </summary>
public class SCVec : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCVec" /> with the specified array of elements.
    /// </summary>
    /// <param name="value">The array of <see cref="SCVal" /> elements.</param>
    public SCVec(SCVal[] value)
    {
        InnerValue = value;
    }

    /// <summary>
    ///     The array of <see cref="SCVal" /> elements in this vector.
    /// </summary>
    public SCVal[] InnerValue { get; }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVec" /> XDR object.</returns>
    public Xdr.SCVec ToXdr()
    {
        return new Xdr.SCVec(InnerValue.Select(a => a.ToXdr()).ToArray());
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_VEC</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_VEC,
            },
            Vec = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCVec" /> from an XDR <see cref="Xdr.SCVec" /> object.
    /// </summary>
    /// <param name="xdrSCVec">The XDR value to convert.</param>
    /// <returns>An <see cref="SCVec" /> instance.</returns>
    public static SCVec FromXdr(Xdr.SCVec xdrSCVec)
    {
        return new SCVec(xdrSCVec.InnerValue.Select(FromXdr).ToArray());
    }

    /// <summary>
    ///     Creates a new <see cref="SCVec" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCVec" /> instance.</returns>
    public static SCVec FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_VEC)
        {
            throw new ArgumentException("Not an SCVec", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Vec);
    }
}

/// <summary>
///     Represents a Soroban map (key-value collection) of <see cref="SCMapEntry" /> elements.
/// </summary>
public class SCMap : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCMap" /> with the specified entries.
    /// </summary>
    /// <param name="entries">The array of key-value entries, or <c>null</c> for an empty map.</param>
    public SCMap(SCMapEntry[]? entries = null)
    {
        entries ??= Array.Empty<SCMapEntry>();
        Entries = entries;
    }

    /// <summary>
    ///     The array of key-value entries in this map.
    /// </summary>
    public SCMapEntry[] Entries { get; }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCMap" /> XDR object.</returns>
    public Xdr.SCMap ToXdr()
    {
        return Entries.Length == 0
            ? new Xdr.SCMap { InnerValue = Array.Empty<Xdr.SCMapEntry>() }
            : new Xdr.SCMap(Entries.Select(a => a.ToXdr()).ToArray());
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_MAP</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_MAP,
            },
            Map = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCMap" /> from an XDR <see cref="Xdr.SCMap" /> object.
    /// </summary>
    /// <param name="xdrSCMap">The XDR value to convert.</param>
    /// <returns>An <see cref="SCMap" /> instance.</returns>
    public static SCMap FromXdr(Xdr.SCMap xdrSCMap)
    {
        return new SCMap(xdrSCMap.InnerValue.Select(SCMapEntry.FromXdr).ToArray());
    }

    /// <summary>
    ///     Creates a new <see cref="SCMap" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCMap" /> instance.</returns>
    public static SCMap FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_MAP)
        {
            throw new ArgumentException("Not an SCMap", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Map);
    }
}

/// <summary>
///     Represents a single key-value entry in a Soroban map.
/// </summary>
public class SCMapEntry
{
    /// <summary>
    ///     Initializes a new <see cref="SCMapEntry" /> with the specified key and value.
    /// </summary>
    /// <param name="key">The map entry key.</param>
    /// <param name="value">The map entry value.</param>
    public SCMapEntry(SCVal key, SCVal value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    ///     The key of this map entry.
    /// </summary>
    public SCVal Key { get; init; }

    /// <summary>
    ///     The value of this map entry.
    /// </summary>
    public SCVal Value { get; init; }

    /// <summary>
    ///     Creates a new <see cref="SCMapEntry" /> from an XDR <see cref="Xdr.SCMapEntry" /> object.
    /// </summary>
    /// <param name="xdr">The XDR map entry to convert.</param>
    /// <returns>An <see cref="SCMapEntry" /> instance.</returns>
    public static SCMapEntry FromXdr(Xdr.SCMapEntry xdr)
    {
        return new SCMapEntry(SCVal.FromXdr(xdr.Key), SCVal.FromXdr(xdr.Val));
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCMapEntry" /> XDR object.</returns>
    public Xdr.SCMapEntry ToXdr()
    {
        return new Xdr.SCMapEntry
        {
            Key = Key.ToXdr(),
            Val = Value.ToXdr(),
        };
    }
}

/// <summary>
///     Represents a Soroban ledger key that refers to a contract instance entry.
/// </summary>
public class SCLedgerKeyContractInstance : SCVal
{
    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_LEDGER_KEY_CONTRACT_INSTANCE</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_LEDGER_KEY_CONTRACT_INSTANCE,
            },
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCLedgerKeyContractInstance" /> instance.
    /// </summary>
    /// <returns>An <see cref="SCLedgerKeyContractInstance" /> instance.</returns>
    public static SCLedgerKeyContractInstance FromXdr()
    {
        return new SCLedgerKeyContractInstance();
    }

    /// <summary>
    ///     Creates a new <see cref="SCLedgerKeyContractInstance" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCLedgerKeyContractInstance" /> instance.</returns>
    public static SCLedgerKeyContractInstance FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_LEDGER_KEY_CONTRACT_INSTANCE)
        {
            throw new ArgumentException("Not an SCLedgerKeyContractInstance", nameof(xdrVal));
        }

        return FromXdr();
    }
}

/// <summary>
///     Represents a Soroban contract instance, containing the executable and optional instance storage.
/// </summary>
public class SCContractInstance : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCContractInstance" /> with the specified executable and optional storage.
    /// </summary>
    /// <param name="executable">The contract executable (Wasm or Stellar Asset Contract).</param>
    /// <param name="storage">Optional instance storage as a map.</param>
    public SCContractInstance(ContractExecutable executable, SCMap? storage)
    {
        Executable = executable;
        Storage = storage;
    }

    /// <summary>
    ///     The contract executable (Wasm or Stellar Asset Contract).
    /// </summary>
    public ContractExecutable Executable { get; }

    /// <summary>
    ///     <see cref="Xdr.SCContractInstance.Storage" /> can be null.
    /// </summary>
    public SCMap? Storage { get; }

    private static SCContractInstance FromXdr(Xdr.SCContractInstance xdr)
    {
        return new SCContractInstance(ContractExecutable.FromXdr(xdr.Executable),
            xdr.Storage != null ? SCMap.FromXdr(xdr.Storage) : null);
    }


    /// <summary>
    ///     Creates a new <see cref="SCContractInstance" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCContractInstance" /> instance.</returns>
    public static SCContractInstance FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_CONTRACT_INSTANCE)
        {
            throw new ArgumentException("Not an SCContractInstance.", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Instance);
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCContractInstance" /> XDR object.</returns>
    public Xdr.SCContractInstance ToXdr()
    {
        return new Xdr.SCContractInstance
        {
            Executable = Executable.ToXdr(),
            Storage = Storage?.ToXdr(),
        };
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_CONTRACT_INSTANCE</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_CONTRACT_INSTANCE,
            },
            Instance = ToXdr(),
        };
    }
}

/// <summary>
///     Base class for contract executables.
/// </summary>
public abstract class ContractExecutable
{
    /// <summary>
    ///     Creates a new <see cref="ContractExecutable" /> subclass from an XDR
    ///     <see cref="Xdr.ContractExecutable" /> object.
    /// </summary>
    /// <param name="xdrContractExecutable">The XDR contract executable to convert.</param>
    /// <returns>The appropriate <see cref="ContractExecutable" /> subclass instance.</returns>
    public static ContractExecutable FromXdr(Xdr.ContractExecutable xdrContractExecutable)
    {
        return xdrContractExecutable.Discriminant.InnerValue switch
        {
            ContractExecutableType.ContractExecutableTypeEnum.CONTRACT_EXECUTABLE_WASM =>
                ContractExecutableWasm.FromXdr(xdrContractExecutable),
            ContractExecutableType.ContractExecutableTypeEnum.CONTRACT_EXECUTABLE_STELLAR_ASSET =>
                new ContractExecutableStellarAsset(),
            _ => throw new ArgumentOutOfRangeException(nameof(xdrContractExecutable),
                "Not a valid contract executable type."),
        };
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.ContractExecutable" /> XDR object.</returns>
    public abstract Xdr.ContractExecutable ToXdr();
}

/// <summary>
///     Represents a contract executable backed by a WebAssembly (Wasm) module, identified by its hash.
/// </summary>
public class ContractExecutableWasm : ContractExecutable
{
    /// <summary>
    ///     Initializes a new <see cref="ContractExecutableWasm" /> with the specified Wasm hash.
    /// </summary>
    /// <param name="hash">A hex-encoded hash of the compiled Wasm module.</param>
    public ContractExecutableWasm(string hash)
    {
        WasmHash = hash;
    }

    /// <summary>
    ///     A hex-encoded string of the Wasm bytes of a compiled smart contract.
    /// </summary>
    public string WasmHash { get; }

    /// <summary>
    ///     Creates a new <see cref="ContractExecutableWasm" /> from an XDR <see cref="Xdr.ContractExecutable" /> object.
    /// </summary>
    /// <param name="xdr">The XDR contract executable to convert.</param>
    /// <returns>A <see cref="ContractExecutableWasm" /> instance.</returns>
    public static ContractExecutableWasm FromXdr(Xdr.ContractExecutable xdr)
    {
        return new ContractExecutableWasm(Convert.ToHexString(xdr.WasmHash.InnerValue));
    }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.ContractExecutable" /> XDR object.</returns>
    public override Xdr.ContractExecutable ToXdr()
    {
        return new Xdr.ContractExecutable
        {
            Discriminant = new ContractExecutableType
            {
                InnerValue = ContractExecutableType.ContractExecutableTypeEnum.CONTRACT_EXECUTABLE_WASM,
            },
            WasmHash = new Hash(Convert.FromHexString(WasmHash)),
        };
    }
}

/// <summary>
///     Represents a contract executable for a built-in Stellar Asset Contract (SAC).
/// </summary>
public class ContractExecutableStellarAsset : ContractExecutable
{
    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.ContractExecutable" /> XDR object.</returns>
    public override Xdr.ContractExecutable ToXdr()
    {
        return new Xdr.ContractExecutable
        {
            Discriminant = new ContractExecutableType
            {
                InnerValue = ContractExecutableType.ContractExecutableTypeEnum.CONTRACT_EXECUTABLE_STELLAR_ASSET,
            },
        };
    }
}

/// <summary>
///     Represents a Soroban nonce key used for authorization nonce tracking.
/// </summary>
public class SCNonceKey : SCVal
{
    /// <summary>
    ///     Initializes a new <see cref="SCNonceKey" /> with the specified nonce value.
    /// </summary>
    /// <param name="value">The nonce value used for authorization tracking.</param>
    public SCNonceKey(long value)
    {
        Nonce = value;
    }

    /// <summary>
    ///     The nonce value used for authorization tracking.
    /// </summary>
    public long Nonce { get; set; }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCNonceKey" /> XDR object.</returns>
    public Xdr.SCNonceKey ToXdr()
    {
        return new Xdr.SCNonceKey
        {
            Nonce = new Int64(Nonce),
        };
    }

    /// <summary>
    ///     Converts this instance to an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <returns>An <see cref="Xdr.SCVal" /> XDR object of type <c>SCV_LEDGER_KEY_NONCE</c>.</returns>
    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_LEDGER_KEY_NONCE,
            },
            NonceKey = ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SCNonceKey" /> from an XDR <see cref="Xdr.SCNonceKey" /> object.
    /// </summary>
    /// <param name="xdr">The XDR nonce key to convert.</param>
    /// <returns>An <see cref="SCNonceKey" /> instance.</returns>
    public static SCNonceKey FromXdr(Xdr.SCNonceKey xdr)
    {
        return new SCNonceKey(xdr.Nonce.InnerValue);
    }

    /// <summary>
    ///     Creates a new <see cref="SCNonceKey" /> from an XDR <see cref="Xdr.SCVal" /> object.
    /// </summary>
    /// <param name="xdrVal">The XDR value to convert.</param>
    /// <returns>An <see cref="SCNonceKey" /> instance.</returns>
    public static SCNonceKey FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_LEDGER_KEY_NONCE)
        {
            throw new ArgumentException("Not an SCNonceKey", nameof(xdrVal));
        }

        return FromXdr(xdrVal.NonceKey);
    }
}