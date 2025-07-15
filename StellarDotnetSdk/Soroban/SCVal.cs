using System;
using System.Linq;
using System.Numerics;
using StellarDotnetSdk.Accounts;
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
            SCAddress scAddress => scAddress.ToSCValXdr(),
            SCContractInstance scContractInstance => scContractInstance.ToSCValXdr(),
            SCLedgerKeyContractInstance scLedgerKeyContractInstance => scLedgerKeyContractInstance.ToSCValXdr(),
            SCNonceKey scNonceKey => scNonceKey.ToSCValXdr(),
            _ => throw new InvalidOperationException("Unknown SCVal type"),
        };
    }

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
            SCValType.SCValTypeEnum.SCV_ADDRESS => SCAddress.FromSCValXdr(xdrVal),
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

public class SCBool : SCVal
{
    public SCBool(bool value)
    {
        InnerValue = value;
    }

    public bool InnerValue { get; set; }

    public new bool ToXdr()
    {
        return InnerValue;
    }

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

    public static SCBool FromXdr(bool xdrBool)
    {
        return new SCBool(xdrBool);
    }

    public static SCBool FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_BOOL)
        {
            throw new ArgumentException("Not an SCBool.", nameof(xdrVal));
        }

        return FromXdr(xdrVal.B);
    }
}

public class SCVoid : SCVal
{
    public new void ToXdr()
    {
    }

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

    public static SCVoid FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_VOID)
        {
            throw new ArgumentException("Not an SCVoid", nameof(xdrVal));
        }

        return FromXdr();
    }

    public static SCVoid FromXdr()
    {
        return new SCVoid();
    }
}

public abstract class SCError : SCVal
{
    public SCErrorCode.SCErrorCodeEnum Code { get; set; }

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

    public static SCError FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_ERROR)
        {
            throw new ArgumentException("Not an SCError", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Error);
    }

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

public class SCContractError : SCError
{
    public SCContractError(uint value)
    {
        ContractCode = value;
    }

    public uint ContractCode { get; set; }

    public static SCContractError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCContractError(xdrSCError.ContractCode.InnerValue);
    }

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

public class SCWasmVmError : SCError
{
    public static SCWasmVmError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCWasmVmError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

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

public class SCContextError : SCError
{
    public static SCContextError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCContextError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

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

public class SCStorageError : SCError
{
    public static SCStorageError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCStorageError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

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

public class SCObjectError : SCError
{
    public static SCObjectError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCObjectError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

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

public class SCCryptoError : SCError
{
    public static SCCryptoError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCCryptoError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

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

public class SCEventsError : SCError
{
    public static SCEventsError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCEventsError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

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

public class SCBudgetError : SCError
{
    public static SCBudgetError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCBudgetError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

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

public class SCValueError : SCError
{
    public static SCValueError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCValueError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

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

public class SCAuthError : SCError
{
    public static SCAuthError FromSCErrorXdr(Xdr.SCError xdrSCError)
    {
        return new SCAuthError
        {
            Code = xdrSCError.Code.InnerValue,
        };
    }

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

public class SCUint32 : SCVal
{
    public SCUint32(uint value)
    {
        InnerValue = value;
    }

    public uint InnerValue { get; set; }

    public Uint32 ToXdr()
    {
        return new Uint32(InnerValue);
    }

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

    public static SCUint32 FromXdr(Uint32 xdrUint32)
    {
        return new SCUint32(xdrUint32.InnerValue);
    }

    public static SCUint32 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_U32)
        {
            throw new ArgumentException("Not an SCUint32", nameof(xdrVal));
        }

        return FromXdr(xdrVal.U32);
    }
}

public class SCInt32 : SCVal
{
    public SCInt32(int value)
    {
        InnerValue = value;
    }

    public int InnerValue { get; set; }

    public Int32 ToXdr()
    {
        return new Int32(InnerValue);
    }

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

    public static SCInt32 FromXdr(Int32 xdrInt32)
    {
        return new SCInt32(xdrInt32.InnerValue);
    }

    public static SCInt32 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_I32)
        {
            throw new ArgumentException("Not an SCInt32", nameof(xdrVal));
        }

        return FromXdr(xdrVal.I32);
    }
}

public class SCUint64 : SCVal
{
    public SCUint64(ulong value)
    {
        InnerValue = value;
    }

    public ulong InnerValue { get; set; }

    public Uint64 ToXdr()
    {
        return new Uint64(InnerValue);
    }

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

    public static SCUint64 FromXdr(Uint64 xdrUint64)
    {
        return new SCUint64(xdrUint64.InnerValue);
    }

    public static SCUint64 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_U64)
        {
            throw new ArgumentException("Not an SCUint64", nameof(xdrVal));
        }

        return FromXdr(xdrVal.U64);
    }
}

public class SCInt64 : SCVal
{
    public SCInt64(long value)
    {
        InnerValue = value;
    }

    public long InnerValue { get; set; }

    public Int64 ToXdr()
    {
        return new Int64(InnerValue);
    }

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

    public static SCInt64 FromXdr(Int64 xdrInt64)
    {
        return new SCInt64(xdrInt64.InnerValue);
    }

    public static SCInt64 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_I64)
        {
            throw new ArgumentException("Not an SCInt64", nameof(xdrVal));
        }

        return FromXdr(xdrVal.I64);
    }
}

public class SCTimePoint : SCVal
{
    public SCTimePoint(ulong value)
    {
        InnerValue = value;
    }

    public ulong InnerValue { get; set; }

    public TimePoint ToXdr()
    {
        return new TimePoint(new Uint64(InnerValue));
    }

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

    public static SCTimePoint FromXdr(TimePoint xdrTimePoint)
    {
        return new SCTimePoint(xdrTimePoint.InnerValue.InnerValue);
    }

    public static SCTimePoint FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_TIMEPOINT)
        {
            throw new ArgumentException("Not an SCTimePoint", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Timepoint);
    }
}

public class SCDuration : SCVal
{
    public SCDuration(ulong value)
    {
        InnerValue = value;
    }

    public ulong InnerValue { get; set; }

    public Duration ToXdr()
    {
        return new Duration(new Uint64(InnerValue));
    }

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

    public static SCDuration FromXdr(Duration xdrDuration)
    {
        return new SCDuration(xdrDuration.InnerValue.InnerValue);
    }

    public static SCDuration FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_DURATION)
        {
            throw new ArgumentException("Not an SCDuration", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Duration);
    }
}

public class SCUint128 : SCVal
{
    public SCUint128(ulong lo, ulong hi)
    {
        Hi = hi;
        Lo = lo;
    }

    public ulong Lo { get; set; }
    public ulong Hi { get; set; }

    public UInt128Parts ToXdr()
    {
        return new UInt128Parts
        {
            Lo = new Uint64(Lo),
            Hi = new Uint64(Hi),
        };
    }

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

    public static SCUint128 FromXdr(UInt128Parts xdrUInt128Parts)
    {
        return new SCUint128(xdrUInt128Parts.Lo.InnerValue, xdrUInt128Parts.Hi.InnerValue);
    }

    public static SCUint128 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_U128)
        {
            throw new ArgumentException("Not an SCUint128", nameof(xdrVal));
        }

        return FromXdr(xdrVal.U128);
    }
}

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

    public ulong Lo { get; set; }
    public long Hi { get; set; }

    public Int128Parts ToXdr()
    {
        return new Int128Parts
        {
            Lo = new Uint64(Lo),
            Hi = new Int64(Hi),
        };
    }

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

    public static SCInt128 FromXdr(Int128Parts xdrInt128Parts)
    {
        return new SCInt128(xdrInt128Parts.Hi.InnerValue, xdrInt128Parts.Lo.InnerValue);
    }

    public static SCInt128 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_I128)
        {
            throw new ArgumentException("Not an SCInt128", nameof(xdrVal));
        }

        return new SCInt128(xdrVal.I128.Hi.InnerValue, xdrVal.I128.Lo.InnerValue);
    }
}

public class SCUint256 : SCVal
{
    public ulong HiHi { get; set; }
    public ulong HiLo { get; set; }
    public ulong LoHi { get; set; }
    public ulong LoLo { get; set; }

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

    public static SCUint256 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_U256)
        {
            throw new ArgumentException("Not an SCUint256", nameof(xdrVal));
        }

        return FromXdr(xdrVal.U256);
    }
}

public class SCInt256 : SCVal
{
    public long HiHi { get; set; }
    public ulong HiLo { get; set; }
    public ulong LoHi { get; set; }
    public ulong LoLo { get; set; }

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

    public static SCInt256 FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_I256)
        {
            throw new ArgumentException("Not an SCInt256", nameof(xdrVal));
        }

        return FromXdr(xdrVal.I256);
    }
}

public class SCBytes : SCVal
{
    public SCBytes(byte[] value)
    {
        InnerValue = value;
    }

    public byte[] InnerValue { get; set; }

    public Xdr.SCBytes ToXdr()
    {
        return new Xdr.SCBytes(InnerValue);
    }

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

    public static SCBytes FromXdr(Xdr.SCBytes xdrSCBytes)
    {
        return new SCBytes(xdrSCBytes.InnerValue);
    }

    public static SCBytes FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_BYTES)
        {
            throw new ArgumentException("Not an SCBytes", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Bytes);
    }
}

public class SCString : SCVal
{
    public SCString(string value)
    {
        InnerValue = value;
    }

    public string InnerValue { get; set; }

    public Xdr.SCString ToXdr()
    {
        return new Xdr.SCString(InnerValue);
    }

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

    public static SCString FromXdr(Xdr.SCString xdrSCString)
    {
        return new SCString(xdrSCString.InnerValue);
    }

    public static SCString FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_STRING)
        {
            throw new ArgumentException("Not an SCString", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Str);
    }
}

public class SCSymbol : SCVal
{
    public SCSymbol(string innerValue)
    {
        InnerValue = innerValue;
    }

    public string InnerValue { get; }

    public Xdr.SCSymbol ToXdr()
    {
        return new Xdr.SCSymbol(InnerValue);
    }

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

    public static SCSymbol FromXdr(Xdr.SCSymbol xdrSCSymbol)
    {
        return new SCSymbol(xdrSCSymbol.InnerValue);
    }

    public static SCSymbol FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_SYMBOL)
        {
            throw new ArgumentException("Not an SCSymbol", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Sym);
    }
}

public class SCVec : SCVal
{
    public SCVec(SCVal[] value)
    {
        InnerValue = value;
    }

    public SCVal[] InnerValue { get; }

    public Xdr.SCVec ToXdr()
    {
        return new Xdr.SCVec(InnerValue.Select(a => a.ToXdr()).ToArray());
    }

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

    public static SCVec FromXdr(Xdr.SCVec xdrSCVec)
    {
        return new SCVec(xdrSCVec.InnerValue.Select(FromXdr).ToArray());
    }

    public static SCVec FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_VEC)
        {
            throw new ArgumentException("Not an SCVec", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Vec);
    }
}

public class SCMap : SCVal
{
    public SCMap(SCMapEntry[]? entries = null)
    {
        entries ??= Array.Empty<SCMapEntry>();
        Entries = entries;
    }

    public SCMapEntry[] Entries { get; }

    public Xdr.SCMap ToXdr()
    {
        return Entries.Length == 0
            ? new Xdr.SCMap { InnerValue = Array.Empty<Xdr.SCMapEntry>() }
            : new Xdr.SCMap(Entries.Select(a => a.ToXdr()).ToArray());
    }

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

    public static SCMap FromXdr(Xdr.SCMap xdrSCMap)
    {
        return new SCMap(xdrSCMap.InnerValue.Select(SCMapEntry.FromXdr).ToArray());
    }

    public static SCMap FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_MAP)
        {
            throw new ArgumentException("Not an SCMap", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Map);
    }
}

public class SCMapEntry
{
    public SCMapEntry(SCVal key, SCVal value)
    {
        Key = key;
        Value = value;
    }

    public SCVal Key { get; init; }
    public SCVal Value { get; init; }

    public static SCMapEntry FromXdr(Xdr.SCMapEntry xdr)
    {
        return new SCMapEntry(SCVal.FromXdr(xdr.Key), SCVal.FromXdr(xdr.Val));
    }

    public Xdr.SCMapEntry ToXdr()
    {
        return new Xdr.SCMapEntry
        {
            Key = Key.ToXdr(),
            Val = Value.ToXdr(),
        };
    }
}

public abstract class SCAddress : SCVal
{
    public static SCAddress FromXdr(Xdr.SCAddress xdrSCAddress)
    {
        return xdrSCAddress.Discriminant.InnerValue switch
        {
            SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_ACCOUNT => SCAccountId.FromXdr(xdrSCAddress),
            SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_CONTRACT => SCContractId.FromXdr(xdrSCAddress),
            _ => throw new ArgumentOutOfRangeException(nameof(xdrSCAddress), "Invalid address type."),
        };
    }

    public static SCAddress FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_ADDRESS)
        {
            throw new ArgumentException("Not an SCAddress", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Address);
    }

    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_ADDRESS,
            },
            Address = ToXdr(),
        };
    }

    public abstract Xdr.SCAddress ToXdr();
}

public class SCAccountId : SCAddress
{
    public SCAccountId(string value)
    {
        if (!StrKey.IsValidEd25519PublicKey(value))
        {
            throw new ArgumentException("Invalid account ID.", nameof(value));
        }

        InnerValue = value;
    }

    public string InnerValue { get; set; }

    public static SCAccountId FromXdr(Xdr.SCAddress xdr)
    {
        return new SCAccountId(
            KeyPair.FromXdrPublicKey(xdr.AccountId.InnerValue).AccountId);
    }

    public override Xdr.SCAddress ToXdr()
    {
        return new Xdr.SCAddress
        {
            Discriminant = new SCAddressType
            {
                InnerValue = SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_ACCOUNT,
            },
            AccountId = new AccountID(KeyPair.FromAccountId(InnerValue).XdrPublicKey),
        };
    }
}

public class SCContractId : SCAddress
{
    public SCContractId(string value)
    {
        if (!StrKey.IsValidContractId(value))
        {
            throw new ArgumentException("Invalid contract id", nameof(value));
        }

        InnerValue = value;
    }

    public string InnerValue { get; }

    public static SCContractId FromXdr(Xdr.SCAddress xdr)
    {
        var value = StrKey.EncodeContractId(xdr.ContractId.InnerValue.InnerValue);

        if (!StrKey.IsValidContractId(value))
        {
            throw new InvalidOperationException("Invalid contract id");
        }

        return new SCContractId(value);
    }

    public override Xdr.SCAddress ToXdr()
    {
        if (!StrKey.IsValidContractId(InnerValue))
        {
            throw new InvalidOperationException("Invalid contract id");
        }

        return new Xdr.SCAddress
        {
            Discriminant = new SCAddressType
            {
                InnerValue = SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_CONTRACT,
            },
            ContractId = new ContractID { InnerValue = new Hash(StrKey.DecodeContractId(InnerValue)) },
        };
    }
}

public class SCLedgerKeyContractInstance : SCVal
{
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

    public static SCLedgerKeyContractInstance FromXdr()
    {
        return new SCLedgerKeyContractInstance();
    }

    public static SCLedgerKeyContractInstance FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_LEDGER_KEY_CONTRACT_INSTANCE)
        {
            throw new ArgumentException("Not an SCLedgerKeyContractInstance", nameof(xdrVal));
        }

        return FromXdr();
    }
}

public class SCContractInstance : SCVal
{
    public SCContractInstance(ContractExecutable executable, SCMap? storage)
    {
        Executable = executable;
        Storage = storage;
    }

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


    public static SCContractInstance FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_CONTRACT_INSTANCE)
        {
            throw new ArgumentException("Not an SCContractInstance.", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Instance);
    }

    public Xdr.SCContractInstance ToXdr()
    {
        return new Xdr.SCContractInstance
        {
            Executable = Executable.ToXdr(),
            Storage = Storage?.ToXdr(),
        };
    }

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

    public abstract Xdr.ContractExecutable ToXdr();
}

public class ContractExecutableWasm : ContractExecutable
{
    public ContractExecutableWasm(string hash)
    {
        WasmHash = hash;
    }

    /// <summary>
    ///     A hex-encoded string of the Wasm bytes of a compiled smart contract.
    /// </summary>
    public string WasmHash { get; }

    public static ContractExecutableWasm FromXdr(Xdr.ContractExecutable xdr)
    {
        return new ContractExecutableWasm(Convert.ToHexString(xdr.WasmHash.InnerValue));
    }

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

public class ContractExecutableStellarAsset : ContractExecutable
{
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

public class SCNonceKey : SCVal
{
    public SCNonceKey(long value)
    {
        Nonce = value;
    }

    public long Nonce { get; set; }

    public Xdr.SCNonceKey ToXdr()
    {
        return new Xdr.SCNonceKey
        {
            Nonce = new Int64(Nonce),
        };
    }

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

    public static SCNonceKey FromXdr(Xdr.SCNonceKey xdr)
    {
        return new SCNonceKey(xdr.Nonce.InnerValue);
    }

    public static SCNonceKey FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_LEDGER_KEY_NONCE)
        {
            throw new ArgumentException("Not an SCNonceKey", nameof(xdrVal));
        }

        return FromXdr(xdrVal.NonceKey);
    }
}