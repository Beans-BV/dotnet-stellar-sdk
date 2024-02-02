using System;
using System.Linq;
using System.Security.Cryptography;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

/// <summary>
///     Base class for operations that invoke host functions.
/// </summary>
public abstract class InvokeHostFunctionOperation : Operation
{
    public SorobanAuthorizationEntry[] Auth { get; set; } = Array.Empty<SorobanAuthorizationEntry>();
}

/// <summary>
///     Operation that invokes a Soroban host function to invoke a contract
/// </summary>
public class InvokeContractOperation : InvokeHostFunctionOperation
{
    public InvokeContractOperation(InvokeContractHostFunction hostFunction)
    {
        HostFunction = hostFunction;
    }

    public InvokeContractHostFunction HostFunction { get; }

    /// <summary>
    ///     Creates a new InvokeContractOperation object from the given base64-encoded XDR Operation.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>InvokeContractOperation object</returns>
    /// <exception cref="InvalidOperationException">Thrown when the base64-encoded XDR value is invalid.</exception>
    public static InvokeContractOperation FromOperationXdrBase64(string xdrBase64)
    {
        var operation = FromXdrBase64(xdrBase64);
        if (operation == null)
            throw new InvalidOperationException("Operation XDR is invalid");

        if (operation is not InvokeContractOperation invokeHostFunctionOperation)
            throw new InvalidOperationException("Operation is not InvokeHostFunctionOperation");
        return invokeHostFunctionOperation;
    }

    public static InvokeContractOperation FromInvokeHostFunctionOperationXdr(
        InvokeHostFunctionOp xdrInvokeHostFunctionOp)
    {
        if (xdrInvokeHostFunctionOp.HostFunction.Discriminant.InnerValue !=
            HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_INVOKE_CONTRACT)
            throw new InvalidOperationException("Invalid HostFunction type");

        return new InvokeContractOperation(
            InvokeContractHostFunction.FromHostFunctionXdr(xdrInvokeHostFunctionOp.HostFunction)
        )
        {
            Auth = xdrInvokeHostFunctionOp.Auth.Select(SorobanAuthorizationEntry.FromXdr).ToArray()
        };
    }

    public InvokeHostFunctionOp ToInvokeHostFunctionOperationXdr()
    {
        return new InvokeHostFunctionOp
        {
            HostFunction = new xdr.HostFunction
            {
                Discriminant = new HostFunctionType
                {
                    InnerValue = HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_INVOKE_CONTRACT
                },
                InvokeContract = HostFunction.ToXdr()
            },
            Auth = Auth.Select(a => a.ToXdr()).ToArray()
        };
    }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant = new OperationType
            {
                InnerValue = OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION
            },
            InvokeHostFunctionOp = ToInvokeHostFunctionOperationXdr()
        };
        return body;
    }

    public class Builder
    {
        private SCVal[]? _args;
        private SorobanAuthorizationEntry[]? _auth;
        private SCAddress? _contractAddress;
        private SCSymbol? _functionName;

        private KeyPair? _sourceAccount;

        public Builder()
        {
        }

        public Builder(InvokeHostFunctionOp operationXdr)
        {
            _contractAddress = SCAddress.FromXdr(operationXdr.HostFunction.InvokeContract.ContractAddress);
            _functionName = SCSymbol.FromXdr(operationXdr.HostFunction.InvokeContract.FunctionName);
            _args = operationXdr.HostFunction.InvokeContract.Args.Select(SCVal.FromXdr).ToArray();
            _auth = operationXdr.Auth.Select(SorobanAuthorizationEntry.FromXdr).ToArray();
        }

        public Builder(
            InvokeContractHostFunction hostFunction,
            SorobanAuthorizationEntry[]? auth = null)
        {
            _contractAddress = hostFunction.ContractAddress;
            _functionName = hostFunction.FunctionName;
            _args = hostFunction.Args;
            _auth = auth;
        }

        public Builder(
            SCAddress contractAddress,
            SCSymbol functionName,
            SCVal[] args,
            SorobanAuthorizationEntry[]? auth = null)
        {
            _contractAddress = contractAddress;
            _functionName = functionName;
            _args = args;
            _auth = auth;
        }

        public Builder SetContractAddress(SCAddress contractAddress)
        {
            _contractAddress = contractAddress;
            return this;
        }

        public Builder SetFunctionName(SCSymbol functionName)
        {
            _functionName = functionName;
            return this;
        }

        public Builder SetArgs(SCVal[] args)
        {
            _args = args;
            return this;
        }

        public Builder SetAuth(SorobanAuthorizationEntry[] auth)
        {
            _auth = auth;
            return this;
        }

        public Builder AddAuth(SorobanAuthorizationEntry auth)
        {
            _auth ??= Array.Empty<SorobanAuthorizationEntry>();
            _auth = _auth.Append(auth).ToArray();
            return this;
        }

        public Builder RemoveAuth(SorobanAuthorizationEntry auth)
        {
            if (_auth == null)
                return this;

            _auth = _auth.Where(a => !a.Equals(auth)).ToArray();
            return this;
        }

        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }

        public InvokeContractOperation Build()
        {
            if (_contractAddress == null)
                throw new InvalidOperationException("Contract address cannot be null");
            if (_functionName == null)
                throw new InvalidOperationException("Function name cannot be null");

            var operation = new InvokeContractOperation(
                new InvokeContractHostFunction(
                    _contractAddress,
                    _functionName,
                    _args ?? Array.Empty<SCVal>()))
            {
                Auth = _auth ?? Array.Empty<SorobanAuthorizationEntry>()
            };
            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;

            return operation;
        }
    }
}

public class CreateContractOperation : InvokeHostFunctionOperation
{
    public CreateContractOperation(CreateContractHostFunction hostFunction)
    {
        HostFunction = hostFunction;
    }

    public CreateContractHostFunction HostFunction { get; }

    /// <summary>
    ///     Creates a new CreateContractOperation object from the given base64-encoded XDR Operation.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>CreateContractOperation object</returns>
    /// <exception cref="InvalidOperationException">Thrown when the base64-encoded XDR value is invalid.</exception>
    public static CreateContractOperation FromOperationXdrBase64(string xdrBase64)
    {
        var operation = FromXdrBase64(xdrBase64);
        if (operation == null)
            throw new InvalidOperationException("Operation XDR is invalid");

        if (operation is not CreateContractOperation invokeHostFunctionOperation)
            throw new InvalidOperationException("Operation is not InvokeHostFunctionOperation");

        return invokeHostFunctionOperation;
    }

    public static CreateContractOperation FromInvokeHostFunctionOperationXdr(
        InvokeHostFunctionOp xdrInvokeHostFunctionOp)
    {
        if (xdrInvokeHostFunctionOp.HostFunction.Discriminant.InnerValue !=
            HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_CREATE_CONTRACT)
            throw new InvalidOperationException("Invalid HostFunction type");

        return new CreateContractOperation(
            CreateContractHostFunction.FromHostFunctionXdr(xdrInvokeHostFunctionOp.HostFunction)
        )
        {
            Auth = xdrInvokeHostFunctionOp.Auth.Select(SorobanAuthorizationEntry.FromXdr).ToArray()
        };
    }

    public InvokeHostFunctionOp ToInvokeHostFunctionOperationXdr()
    {
        return new InvokeHostFunctionOp
        {
            HostFunction = new xdr.HostFunction
            {
                Discriminant = new HostFunctionType
                {
                    InnerValue = HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_CREATE_CONTRACT
                },
                CreateContract = HostFunction.ToXdr()
            },
            Auth = Auth.Select(a => a.ToXdr()).ToArray()
        };
    }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant = new OperationType
            {
                InnerValue = OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION
            },
            InvokeHostFunctionOp = ToInvokeHostFunctionOperationXdr()
        };
        return body;
    }

    public class Builder
    {
        private SorobanAuthorizationEntry[]? _auth;
        private ContractIDPreimage? _contractIDPreimage;
        private ContractExecutable? _executable;

        private KeyPair? _sourceAccount;

        public Builder()
        {
        }

        public Builder(string accountId, string wasmId, byte[]? salt)
        {
            _contractIDPreimage = new ContractIDAddressPreimage(accountId, salt);
            _executable = new ContractExecutableWasm(wasmId);
        }

        public Builder(InvokeHostFunctionOp operationXdr)
        {
            _contractIDPreimage =
                ContractIDPreimage.FromXdr(operationXdr.HostFunction.CreateContract.ContractIDPreimage);
            _executable = ContractExecutable.FromXdr(operationXdr.HostFunction.CreateContract.Executable);
            _auth = operationXdr.Auth.Select(SorobanAuthorizationEntry.FromXdr).ToArray();
        }

        public Builder(
            CreateContractHostFunction hostFunction,
            SorobanAuthorizationEntry[]? auth = null)
        {
            _contractIDPreimage = hostFunction.ContractIDPreimage;
            _executable = hostFunction.Executable;
            _auth = auth;
        }

        public Builder(
            ContractIDPreimage contractIDPreimage,
            ContractExecutable executable,
            SorobanAuthorizationEntry[]? auth = null)
        {
            _contractIDPreimage = contractIDPreimage;
            _executable = executable;
            _auth = auth;
        }

        public Builder SetContractIDPreimage(ContractIDPreimage contractIDPreimage)
        {
            _contractIDPreimage = contractIDPreimage;
            return this;
        }

        public Builder SetExecutable(ContractExecutable executable)
        {
            _executable = executable;
            return this;
        }

        public Builder SetAuth(SorobanAuthorizationEntry[] auth)
        {
            _auth = auth;
            return this;
        }

        public Builder AddAuth(SorobanAuthorizationEntry auth)
        {
            _auth ??= Array.Empty<SorobanAuthorizationEntry>();
            _auth = _auth.Append(auth).ToArray();
            return this;
        }

        public Builder RemoveAuth(SorobanAuthorizationEntry auth)
        {
            if (_auth == null)
                return this;

            _auth = _auth.Where(a => !a.Equals(auth)).ToArray();
            return this;
        }

        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }

        public CreateContractOperation Build()
        {
            if (_contractIDPreimage == null)
                throw new InvalidOperationException("Contract ID preimage cannot be null");
            if (_executable == null)
                throw new InvalidOperationException("Executable cannot be null");

            var operation = new CreateContractOperation(
                new CreateContractHostFunction(
                    _contractIDPreimage,
                    _executable))
            {
                Auth = _auth ?? Array.Empty<SorobanAuthorizationEntry>()
            };
            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;

            return operation;
        }
    }
}

public class UploadContractOperation : InvokeHostFunctionOperation
{
    public UploadContractOperation(UploadContractHostFunction hostFunction)
    {
        HostFunction = hostFunction;
    }

    public UploadContractHostFunction HostFunction { get; }

    /// <summary>
    ///     Creates a new UploadContractOperation object from the given base64-encoded XDR Operation.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>UploadContractOperation object</returns>
    /// <exception cref="InvalidOperationException">Thrown when the base64-encoded XDR value is invalid.</exception>
    public static UploadContractOperation FromOperationXdrBase64(string xdrBase64)
    {
        var operation = FromXdrBase64(xdrBase64);
        if (operation == null)
            throw new InvalidOperationException("Operation XDR is invalid");

        if (operation is not UploadContractOperation invokeHostFunctionOperation)
            throw new InvalidOperationException("Operation is not InvokeHostFunctionOperation");

        return invokeHostFunctionOperation;
    }

    public static UploadContractOperation FromInvokeHostFunctionOperationXdr(
        InvokeHostFunctionOp xdrInvokeHostFunctionOp)
    {
        if (xdrInvokeHostFunctionOp.HostFunction.Discriminant.InnerValue !=
            HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_UPLOAD_CONTRACT_WASM)
            throw new InvalidOperationException("Invalid HostFunction type");

        return new UploadContractOperation(
            UploadContractHostFunction.FromHostFunctionXdr(xdrInvokeHostFunctionOp.HostFunction)
        )
        {
            Auth = xdrInvokeHostFunctionOp.Auth.Select(SorobanAuthorizationEntry.FromXdr).ToArray()
        };
    }

    public InvokeHostFunctionOp ToInvokeHostFunctionOperationXdr()
    {
        return new InvokeHostFunctionOp
        {
            HostFunction = new xdr.HostFunction
            {
                Discriminant = new HostFunctionType
                {
                    InnerValue = HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_UPLOAD_CONTRACT_WASM
                },
                Wasm = HostFunction.ToXdr()
            },
            Auth = Auth.Select(a => a.ToXdr()).ToArray()
        };
    }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant = new OperationType
            {
                InnerValue = OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION
            },
            InvokeHostFunctionOp = ToInvokeHostFunctionOperationXdr()
        };
        return body;
    }

    public class Builder
    {
        private SorobanAuthorizationEntry[]? _auth;

        private KeyPair? _sourceAccount;
        private byte[]? _wasm;

        public Builder()
        {
        }

        public Builder(InvokeHostFunctionOp operationXdr)
        {
            _wasm = operationXdr.HostFunction.Wasm;
            _auth = operationXdr.Auth.Select(SorobanAuthorizationEntry.FromXdr).ToArray();
        }

        public Builder(
            UploadContractHostFunction hostFunction,
            SorobanAuthorizationEntry[]? auth = null)
        {
            _wasm = hostFunction.Wasm;
            _auth = auth;
        }

        public Builder(
            byte[] wasm,
            SorobanAuthorizationEntry[]? auth = null)
        {
            _wasm = wasm;
            _auth = auth;
        }

        public Builder SetWasm(byte[] wasm)
        {
            _wasm = wasm;
            return this;
        }

        public Builder SetAuth(SorobanAuthorizationEntry[] auth)
        {
            _auth = auth;
            return this;
        }

        public Builder AddAuth(SorobanAuthorizationEntry auth)
        {
            _auth ??= Array.Empty<SorobanAuthorizationEntry>();
            _auth = _auth.Append(auth).ToArray();
            return this;
        }

        public Builder RemoveAuth(SorobanAuthorizationEntry auth)
        {
            if (_auth == null)
                return this;

            _auth = _auth.Where(a => !a.Equals(auth)).ToArray();
            return this;
        }

        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }

        public UploadContractOperation Build()
        {
            if (_wasm == null)
                throw new InvalidOperationException("Wasm cannot be null");

            var operation = new UploadContractOperation(
                new UploadContractHostFunction(
                    _wasm))
            {
                Auth = _auth ?? Array.Empty<SorobanAuthorizationEntry>()
            };
            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;

            return operation;
        }
    }
}

public abstract class HostFunction
{
    public xdr.HostFunction ToXdr()
    {
        return this switch
        {
            InvokeContractHostFunction invokeContractArgs => invokeContractArgs.ToHostFunctionXdr(),
            CreateContractHostFunction createContractArgs => createContractArgs.ToHostFunctionXdr(),
            UploadContractHostFunction uploadContractArgs => uploadContractArgs.ToHostFunctionXdr(),
            _ => throw new InvalidOperationException("Unknown HostFunction type")
        };
    }

    public static HostFunction FromXdr(xdr.HostFunction xdrHostFunction)
    {
        return xdrHostFunction.Discriminant.InnerValue switch
        {
            HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_INVOKE_CONTRACT => InvokeContractHostFunction
                .FromHostFunctionXdr(xdrHostFunction),
            HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_CREATE_CONTRACT => CreateContractHostFunction
                .FromHostFunctionXdr(xdrHostFunction),
            HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_UPLOAD_CONTRACT_WASM =>
                UploadContractHostFunction.FromHostFunctionXdr(xdrHostFunction),
            _ => throw new InvalidOperationException("Unknown HostFunction type")
        };
    }
}

public class InvokeContractHostFunction : HostFunction
{
    public InvokeContractHostFunction(
        SCAddress contractAddress,
        SCSymbol functionName,
        SCVal[] args)
    {
        ContractAddress = contractAddress;
        FunctionName = functionName;
        Args = args;
    }

    public InvokeContractHostFunction(string contractAddress, string functionName, SCVal[] args)
    {
        throw new NotImplementedException();
    }

    public SCAddress ContractAddress { get; }
    public SCSymbol FunctionName { get; }
    public SCVal[] Args { get; }

    public static InvokeContractHostFunction FromXdr(InvokeContractArgs xdrInvokeContractArgs)
    {
        return new InvokeContractHostFunction(
            SCAddress.FromXdr(xdrInvokeContractArgs.ContractAddress),
            SCSymbol.FromXdr(xdrInvokeContractArgs.FunctionName),
            xdrInvokeContractArgs.Args.Select(SCVal.FromXdr).ToArray()
        );
    }

    public static InvokeContractHostFunction FromHostFunctionXdr(xdr.HostFunction xdrHostFunction)
    {
        if (xdrHostFunction.Discriminant.InnerValue !=
            HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_INVOKE_CONTRACT)
            throw new InvalidOperationException("Invalid HostFunction type");

        return FromXdr(xdrHostFunction.InvokeContract);
    }

    public InvokeContractArgs ToXdr()
    {
        return new InvokeContractArgs
        {
            ContractAddress = ContractAddress.ToXdr(),
            FunctionName = FunctionName.ToXdr(),
            Args = Args.Select(a => a.ToXdr()).ToArray()
        };
    }

    public xdr.HostFunction ToHostFunctionXdr()
    {
        return new xdr.HostFunction
        {
            Discriminant = new HostFunctionType
            {
                InnerValue = HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_INVOKE_CONTRACT
            },
            InvokeContract = ToXdr()
        };
    }

    /// <summary>
    ///     Returns base64-encoded InvokeContractArgs XDR object.
    /// </summary>
    public string ToXdrBase64()
    {
        var xdrValue = ToXdr();
        var writer = new XdrDataOutputStream();
        InvokeContractArgs.Encode(writer, xdrValue);
        return Convert.ToBase64String(writer.ToArray());
    }
}

public class CreateContractHostFunction : HostFunction
{
    public CreateContractHostFunction(
        ContractIDPreimage contractIDPreimage,
        ContractExecutable executable)
    {
        ContractIDPreimage = contractIDPreimage;
        Executable = executable;
    }

    public ContractIDPreimage ContractIDPreimage { get; }
    public ContractExecutable Executable { get; }

    public static CreateContractHostFunction FromXdr(CreateContractArgs xdrCreateContractArgs)
    {
        return new CreateContractHostFunction(
            ContractIDPreimage.FromXdr(xdrCreateContractArgs.ContractIDPreimage),
            ContractExecutable.FromXdr(xdrCreateContractArgs.Executable)
        );
    }

    public static CreateContractHostFunction FromHostFunctionXdr(xdr.HostFunction xdrHostFunction)
    {
        if (xdrHostFunction.Discriminant.InnerValue !=
            HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_CREATE_CONTRACT)
            throw new InvalidOperationException("Invalid HostFunction type");

        return new CreateContractHostFunction(
            ContractIDPreimage.FromXdr(xdrHostFunction.CreateContract.ContractIDPreimage),
            ContractExecutable.FromXdr(xdrHostFunction.CreateContract.Executable)
        );
    }

    public CreateContractArgs ToXdr()
    {
        return new CreateContractArgs
        {
            ContractIDPreimage = ContractIDPreimage.ToXdr(),
            Executable = Executable.ToXdr()
        };
    }

    public xdr.HostFunction ToHostFunctionXdr()
    {
        return new xdr.HostFunction
        {
            Discriminant = new HostFunctionType
            {
                InnerValue = HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_CREATE_CONTRACT
            },
            CreateContract = ToXdr()
        };
    }

    /// <summary>
    ///     Returns base64-encoded CreateContractArgs XDR object.
    /// </summary>
    public string ToXdrBase64()
    {
        var xdrValue = ToXdr();
        var writer = new XdrDataOutputStream();
        CreateContractArgs.Encode(writer, xdrValue);
        return Convert.ToBase64String(writer.ToArray());
    }
}

public class UploadContractHostFunction : HostFunction
{
    public UploadContractHostFunction(byte[] wasm)
    {
        Wasm = wasm;
    }

    public byte[] Wasm { get; }

    public static UploadContractHostFunction FromHostFunctionXdr(xdr.HostFunction xdrHostFunction)
    {
        if (xdrHostFunction.Discriminant.InnerValue !=
            HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_UPLOAD_CONTRACT_WASM)
            throw new InvalidOperationException("Invalid HostFunction type");

        return new UploadContractHostFunction(
            xdrHostFunction.Wasm
        );
    }

    public byte[] ToXdr()
    {
        return Wasm;
    }

    public xdr.HostFunction ToHostFunctionXdr()
    {
        return new xdr.HostFunction
        {
            Discriminant = new HostFunctionType
            {
                InnerValue = HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_UPLOAD_CONTRACT_WASM
            },
            Wasm = ToXdr()
        };
    }
}

public class SorobanAuthorizationEntry
{
    public SorobanCredentials Credentials { get; set; }
    public SorobanAuthorizedInvocation RootInvocation { get; set; }

    public xdr.SorobanAuthorizationEntry ToXdr()
    {
        return new xdr.SorobanAuthorizationEntry
        {
            Credentials = Credentials.ToXdr(),
            RootInvocation = RootInvocation.ToXdr()
        };
    }

    public static SorobanAuthorizationEntry FromXdr(xdr.SorobanAuthorizationEntry xdr)
    {
        return new SorobanAuthorizationEntry
        {
            Credentials = SorobanCredentials.FromXdr(xdr.Credentials),
            RootInvocation = SorobanAuthorizedInvocation.FromXdr(xdr.RootInvocation)
        };
    }

    /// <summary>
    ///     Creates a new SorobanAuthorizationEntry object from the given SorobanAuthorizationEntry XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>SorobanAuthorizationEntry object</returns>
    public static SorobanAuthorizationEntry FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = xdr.SorobanAuthorizationEntry.Decode(reader);
        return FromXdr(thisXdr);
    }

    /// <summary>
    ///     Returns base64-encoded SorobanAuthorizationEntry XDR object.
    /// </summary>
    public string ToXdrBase64()
    {
        var xdrValue = ToXdr();
        var writer = new XdrDataOutputStream();
        xdr.SorobanAuthorizationEntry.Encode(writer, xdrValue);
        return Convert.ToBase64String(writer.ToArray());
    }
}

public abstract class SorobanCredentials
{
    public xdr.SorobanCredentials ToXdr()
    {
        return this switch
        {
            SorobanSourceAccountCredentials sourceAccount => sourceAccount.ToSorobanCredentialsXdr(),
            SorobanAddressCredentials address => address.ToSorobanCredentialsXdr(),
            _ => throw new InvalidOperationException("Unknown SorobanCredentials type")
        };
    }

    public static SorobanCredentials FromXdr(xdr.SorobanCredentials xdrSorobanCredentials)
    {
        return xdrSorobanCredentials.Discriminant.InnerValue switch
        {
            SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_SOURCE_ACCOUNT =>
                SorobanSourceAccountCredentials.FromSorobanCredentialsXdr(xdrSorobanCredentials),
            SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS =>
                SorobanAddressCredentials.FromSorobanCredentialsXdr(xdrSorobanCredentials),
            _ => throw new InvalidOperationException("Unknown SorobanCredentials type")
        };
    }

    /// <summary>
    ///     Creates a new SorobanCredentials object from the given SorobanCredentials XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>SorobanCredentials object</returns>
    public static SorobanCredentials FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = xdr.SorobanCredentials.Decode(reader);
        return FromXdr(thisXdr);
    }

    /// <summary>
    ///     Returns base64-encoded SorobanCredentials XDR object.
    /// </summary>
    public string ToXdrBase64()
    {
        var xdrValue = ToXdr();
        var writer = new XdrDataOutputStream();
        xdr.SorobanCredentials.Encode(writer, xdrValue);
        return Convert.ToBase64String(writer.ToArray());
    }
}

public class SorobanSourceAccountCredentials : SorobanCredentials
{
    public static SorobanSourceAccountCredentials FromSorobanCredentialsXdr(
        xdr.SorobanCredentials xdrSorobanCredentials)
    {
        if (xdrSorobanCredentials.Discriminant.InnerValue != SorobanCredentialsType.SorobanCredentialsTypeEnum
                .SOROBAN_CREDENTIALS_SOURCE_ACCOUNT)
            throw new InvalidOperationException("Invalid SorobanCredentials type");

        return new SorobanSourceAccountCredentials();
    }

    public xdr.SorobanCredentials ToSorobanCredentialsXdr()
    {
        return new xdr.SorobanCredentials
        {
            Discriminant = new SorobanCredentialsType
            {
                InnerValue = SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_SOURCE_ACCOUNT
            }
        };
    }
}

public class SorobanAddressCredentials : SorobanCredentials
{
    public SCAddress Address { get; set; }
    public long Nonce { get; set; }
    public uint SignatureExpirationLedger { get; set; }
    public SCVal Signature { get; set; }

    public static SorobanAddressCredentials FromSorobanCredentialsXdr(xdr.SorobanCredentials xdrSorobanCredentials)
    {
        if (xdrSorobanCredentials.Discriminant.InnerValue !=
            SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS)
            throw new InvalidOperationException("Invalid SorobanCredentials type");

        return new SorobanAddressCredentials
        {
            Address = SCAddress.FromXdr(xdrSorobanCredentials.Address.Address),
            Nonce = xdrSorobanCredentials.Address.Nonce.InnerValue,
            SignatureExpirationLedger = xdrSorobanCredentials.Address.SignatureExpirationLedger.InnerValue,
            Signature = SCVal.FromXdr(xdrSorobanCredentials.Address.Signature)
        };
    }

    public xdr.SorobanCredentials ToSorobanCredentialsXdr()
    {
        if (Address == null) throw new InvalidOperationException("Address cannot be null");

        if (Nonce == null) throw new InvalidOperationException("Nonce cannot be null");

        if (SignatureExpirationLedger == null)
            throw new InvalidOperationException("SignatureExpirationLedger cannot be null");

        if (Signature == null) throw new InvalidOperationException("Signature cannot be null");

        return new xdr.SorobanCredentials
        {
            Discriminant = new SorobanCredentialsType
            {
                InnerValue = SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS
            },
            Address = new xdr.SorobanAddressCredentials
            {
                Address = Address.ToXdr(),
                Nonce = new Int64
                {
                    InnerValue = Nonce
                },
                SignatureExpirationLedger = new Uint32
                {
                    InnerValue = SignatureExpirationLedger
                },
                Signature = Signature.ToXdr()
            }
        };
    }
}

public class SorobanAuthorizedInvocation
{
    public SorobanAuthorizedFunction Function { get; set; }
    public SorobanAuthorizedInvocation[] SubInvocations { get; set; }

    public xdr.SorobanAuthorizedInvocation ToXdr()
    {
        return new xdr.SorobanAuthorizedInvocation
        {
            Function = Function.ToXdr(),
            SubInvocations = SubInvocations.Select(i => i.ToXdr()).ToArray()
        };
    }

    public static SorobanAuthorizedInvocation FromXdr(xdr.SorobanAuthorizedInvocation xdr)
    {
        return new SorobanAuthorizedInvocation
        {
            Function = SorobanAuthorizedFunction.FromXdr(xdr.Function),
            SubInvocations = xdr.SubInvocations.Select(FromXdr).ToArray()
        };
    }
}

public abstract class SorobanAuthorizedFunction
{
    public xdr.SorobanAuthorizedFunction ToXdr()
    {
        return this switch
        {
            SorobanAuthorizedContractFunction contractFn => contractFn.ToSorobanAuthorizedFunctionXdr(),
            SorobanAuthorizedCreateContractFunction createContractHostFn => createContractHostFn
                .ToSorobanAuthorizedFunctionXdr(),
            _ => throw new InvalidOperationException("Unknown SorobanAuthorizedFunction type")
        };
    }

    public static SorobanAuthorizedFunction FromXdr(xdr.SorobanAuthorizedFunction xdrSorobanAuthorizedFunction)
    {
        return xdrSorobanAuthorizedFunction.Discriminant.InnerValue switch
        {
            SorobanAuthorizedFunctionType.SorobanAuthorizedFunctionTypeEnum
                .SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN => SorobanAuthorizedContractFunction
                .FromSorobanAuthorizedFunctionXdr(xdrSorobanAuthorizedFunction),
            SorobanAuthorizedFunctionType.SorobanAuthorizedFunctionTypeEnum
                .SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_HOST_FN => SorobanAuthorizedCreateContractFunction
                .FromSorobanAuthorizedFunctionXdr(xdrSorobanAuthorizedFunction),
            _ => throw new InvalidOperationException("Unknown SorobanAuthorizedFunction type")
        };
    }
}

public class SorobanAuthorizedContractFunction : SorobanAuthorizedFunction
{
    public InvokeContractHostFunction HostFunction { get; set; }

    public static SorobanAuthorizedFunction FromSorobanAuthorizedFunctionXdr(
        xdr.SorobanAuthorizedFunction xdrSorobanAuthorizedFunction)
    {
        if (xdrSorobanAuthorizedFunction.Discriminant.InnerValue != SorobanAuthorizedFunctionType
                .SorobanAuthorizedFunctionTypeEnum.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN)
            throw new InvalidOperationException("Invalid SorobanAuthorizedFunction type");

        return new SorobanAuthorizedContractFunction
        {
            HostFunction = InvokeContractHostFunction.FromXdr(xdrSorobanAuthorizedFunction.ContractFn)
        };
    }

    public xdr.SorobanAuthorizedFunction ToSorobanAuthorizedFunctionXdr()
    {
        return new xdr.SorobanAuthorizedFunction
        {
            Discriminant = new SorobanAuthorizedFunctionType
            {
                InnerValue = SorobanAuthorizedFunctionType.SorobanAuthorizedFunctionTypeEnum
                    .SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN
            },
            ContractFn = HostFunction.ToXdr()
        };
    }
}

public class SorobanAuthorizedCreateContractFunction : SorobanAuthorizedFunction
{
    public CreateContractHostFunction HostFunction { get; set; }

    public static SorobanAuthorizedFunction FromSorobanAuthorizedFunctionXdr(
        xdr.SorobanAuthorizedFunction xdrSorobanAuthorizedFunction)
    {
        if (xdrSorobanAuthorizedFunction.Discriminant.InnerValue != SorobanAuthorizedFunctionType
                .SorobanAuthorizedFunctionTypeEnum.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_HOST_FN)
            throw new InvalidOperationException("Invalid SorobanAuthorizedFunction type");

        return new SorobanAuthorizedCreateContractFunction
        {
            HostFunction = CreateContractHostFunction.FromXdr(xdrSorobanAuthorizedFunction.CreateContractHostFn)
        };
    }

    public xdr.SorobanAuthorizedFunction ToSorobanAuthorizedFunctionXdr()
    {
        return new xdr.SorobanAuthorizedFunction
        {
            Discriminant = new SorobanAuthorizedFunctionType
            {
                InnerValue = SorobanAuthorizedFunctionType.SorobanAuthorizedFunctionTypeEnum
                    .SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_HOST_FN
            },
            CreateContractHostFn = HostFunction.ToXdr()
        };
    }
}

public abstract class ContractIDPreimage
{
    public xdr.ContractIDPreimage ToXdr()
    {
        return this switch
        {
            ContractIDAddressPreimage fromAddress => fromAddress.ToContractIDPreimageXdr(),
            ContractIDAssetPreimage fromAsset => fromAsset.ToContractIDPreimageXdr(),
            _ => throw new InvalidOperationException("Unknown ContractIDPreimage type")
        };
    }

    public static ContractIDPreimage FromXdr(xdr.ContractIDPreimage xdrContractIDPreimage)
    {
        return xdrContractIDPreimage.Discriminant.InnerValue switch
        {
            ContractIDPreimageType.ContractIDPreimageTypeEnum.CONTRACT_ID_PREIMAGE_FROM_ADDRESS =>
                ContractIDAddressPreimage.FromContractIDPreimageXdr(xdrContractIDPreimage),
            ContractIDPreimageType.ContractIDPreimageTypeEnum.CONTRACT_ID_PREIMAGE_FROM_ASSET =>
                ContractIDAssetPreimage.FromContractIDPreimageXdr(xdrContractIDPreimage),
            _ => throw new InvalidOperationException("Unknown ContractIDPreimage type")
        };
    }
}

public class ContractIDAddressPreimage : ContractIDPreimage
{
    public ContractIDAddressPreimage(string address, byte[]? salt = null)
    {
        Address = new SCAccountId(address);

        if (salt != null)
        {
            if (salt.Length != 32)
                throw new ArgumentException("Salt must have exactly 32 bytes.", nameof(salt));
        }
        else
        {
            salt = new byte[32];
            RandomNumberGenerator.Create().GetBytes(salt);
        }

        Salt = salt;
    }

    public SCAddress Address { get; set; }
    public byte[] Salt { get; set; }

    public static ContractIDPreimage FromContractIDPreimageXdr(xdr.ContractIDPreimage xdrContractIDPreimage)
    {
        if (xdrContractIDPreimage.Discriminant.InnerValue !=
            ContractIDPreimageType.ContractIDPreimageTypeEnum.CONTRACT_ID_PREIMAGE_FROM_ADDRESS)
            throw new InvalidOperationException("Invalid ContractIDPreimage type");

        return new ContractIDAddressPreimage(
            KeyPair.FromXdrPublicKey(xdrContractIDPreimage.FromAddress.Address.AccountId.InnerValue).AccountId,
            xdrContractIDPreimage.FromAddress.Salt.InnerValue);
    }

    public xdr.ContractIDPreimage ToContractIDPreimageXdr()
    {
        return new xdr.ContractIDPreimage
        {
            Discriminant = new ContractIDPreimageType
            {
                InnerValue = ContractIDPreimageType.ContractIDPreimageTypeEnum.CONTRACT_ID_PREIMAGE_FROM_ADDRESS
            },
            FromAddress = new xdr.ContractIDPreimage.ContractIDPreimageFromAddress
            {
                Address = Address.ToXdr(),
                Salt = new Uint256(Salt)
            }
        };
    }
}

public class ContractIDAssetPreimage : ContractIDPreimage
{
    public Asset Asset { get; set; }

    public static ContractIDPreimage FromContractIDPreimageXdr(xdr.ContractIDPreimage xdrContractIDPreimage)
    {
        if (xdrContractIDPreimage.Discriminant.InnerValue !=
            ContractIDPreimageType.ContractIDPreimageTypeEnum.CONTRACT_ID_PREIMAGE_FROM_ASSET)
            throw new InvalidOperationException("Invalid ContractIDPreimage type");

        return new ContractIDAssetPreimage
        {
            Asset = Asset.FromXdr(xdrContractIDPreimage.FromAsset)
        };
    }

    public xdr.ContractIDPreimage ToContractIDPreimageXdr()
    {
        return new xdr.ContractIDPreimage
        {
            Discriminant = new ContractIDPreimageType
            {
                InnerValue = ContractIDPreimageType.ContractIDPreimageTypeEnum.CONTRACT_ID_PREIMAGE_FROM_ASSET
            },
            FromAsset = Asset.ToXdr()
        };
    }
}