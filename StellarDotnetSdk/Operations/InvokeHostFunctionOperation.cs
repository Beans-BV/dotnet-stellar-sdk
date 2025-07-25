﻿using System;
using System.Linq;
using System.Security.Cryptography;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using ContractExecutable = StellarDotnetSdk.Soroban.ContractExecutable;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using FunctionType = StellarDotnetSdk.Xdr.SorobanAuthorizedFunctionType.SorobanAuthorizedFunctionTypeEnum;
using PreimageType = StellarDotnetSdk.Xdr.ContractIDPreimageType.ContractIDPreimageTypeEnum;
using CredentialsType = StellarDotnetSdk.Xdr.SorobanCredentialsType.SorobanCredentialsTypeEnum;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Base class for operations that invoke host functions.
///     See:
///     <a
///         href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#invoke-host-function">
///         Invoke
///         host function
///     </a>
/// </summary>
public abstract class InvokeHostFunctionOperation : Operation
{
    protected InvokeHostFunctionOperation(IAccountId? sourceAccount = null) : base(sourceAccount)
    {
    }

    /// <summary>
    ///     Contains authorization data for the contract invocations.
    /// </summary>
    public SorobanAuthorizationEntry[] Auth { get; set; } = [];
}

/// <summary>
///     Operation that invokes a Soroban host function to invoke a contract.
/// </summary>
public class InvokeContractOperation : InvokeHostFunctionOperation
{
    /// <summary>
    ///     Constructs a new <c>InvokeContractOperation</c>.
    /// </summary>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public InvokeContractOperation(
        ScAddress contractAddress,
        SCSymbol functionName,
        SCVal[]? args,
        IAccountId? sourceAccount = null
    ) : base(sourceAccount)
    {
        if (contractAddress == null)
        {
            throw new ArgumentException("Contract address cannot be null", nameof(contractAddress));
        }
        if (functionName == null)
        {
            throw new ArgumentException("Function name cannot be null", nameof(functionName));
        }
        args ??= [];
        HostFunction = new InvokeContractHostFunction(contractAddress, functionName, args);
    }

    /// <summary>
    ///     Constructs a new <c>InvokeContractOperation</c>.
    /// </summary>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public InvokeContractOperation(
        string contractAddress,
        string functionName,
        SCVal[]? args,
        IAccountId? sourceAccount = null
    ) : base(sourceAccount)
    {
        if (contractAddress == null)
        {
            throw new ArgumentException("Contract address cannot be null", nameof(contractAddress));
        }
        if (functionName == null)
        {
            throw new ArgumentException("Function name cannot be null", nameof(functionName));
        }
        args ??= [];
        HostFunction = new InvokeContractHostFunction(
            new ScContractId(contractAddress), new SCSymbol(functionName), args);
    }

    public InvokeContractHostFunction HostFunction { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION),
            InvokeHostFunctionOp = new InvokeHostFunctionOp
            {
                HostFunction = new Xdr.HostFunction
                {
                    Discriminant = new HostFunctionType
                    {
                        InnerValue = HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_INVOKE_CONTRACT,
                    },
                    InvokeContract = HostFunction.ToXdr(),
                },
                Auth = Auth.Select(a => a.ToXdr()).ToArray(),
            },
        };
    }

    public static InvokeContractOperation FromXdr(InvokeHostFunctionOp invokeHostFunctionOp)
    {
        var invokeContractArgs = invokeHostFunctionOp.HostFunction.InvokeContract;
        return new InvokeContractOperation(
            ScAddress.FromXdr(invokeContractArgs.ContractAddress),
            SCSymbol.FromXdr(invokeContractArgs.FunctionName),
            invokeContractArgs.Args.Select(SCVal.FromXdr).ToArray()
        )
        {
            Auth = invokeHostFunctionOp.Auth.Select(SorobanAuthorizationEntry.FromXdr).ToArray(),
        };
    }
}

public class CreateContractOperation : InvokeHostFunctionOperation
{
    private CreateContractOperation(
        CreateContractV2HostFunction hostFunction,
        IAccountId? sourceAccount = null
    ) : base(sourceAccount)
    {
        HostFunction = hostFunction;
    }

    /// <summary>
    ///     The host function to invoke.
    /// </summary>
    public CreateContractV2HostFunction HostFunction { get; }

    /// <summary>
    ///     Creates a new <c>CreateContractOperation</c> using the provided address and salt.
    /// </summary>
    /// <param name="wasmHash">
    ///     A hex-encoded string of the Wasm bytes of a compiled smart contract.
    /// </param>
    /// <param name="accountId">The address to use to derive the contract ID.</param>
    /// <param name="arguments">The optional parameters to pass to the constructor of this contract.</param>
    /// <param name="salt">(Optional) Custom salt 32-byte salt for the token ID. It will be randomly generated if omitted.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public static CreateContractOperation FromAddress(
        string wasmHash,
        string accountId,
        SCVal[]? arguments = null,
        byte[]? salt = null,
        IAccountId? sourceAccount = null
    )
    {
        return new CreateContractOperation(
            new CreateContractV2HostFunction(
                new ContractIdAddressPreimage(accountId, salt),
                new ContractExecutableWasm(wasmHash),
                arguments ?? []
            ),
            sourceAccount);
    }

    /// <summary>
    ///     Creates a new <c>CreateContractOperation</c> to deploy builtin Soroban Asset Contract using the Stellar asset.
    /// </summary>
    /// <param name="asset">The contract will be created using this Stellar asset.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    /// <remarks>
    ///     Note, that the asset doesn't need to exist when this is applied, however the issuer of the asset will be the
    ///     initial token administrator. Anyone can deploy asset contracts.
    /// </remarks>
    public static CreateContractOperation FromAsset(
        Asset asset,
        SCVal[]? arguments = null,
        IAccountId? sourceAccount = null
    )
    {
        return new CreateContractOperation(
            new CreateContractV2HostFunction(
                new ContractIdAssetPreimage(asset),
                new ContractExecutableStellarAsset(),
                arguments ?? []
            ),
            sourceAccount);
    }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION),
            InvokeHostFunctionOp = new InvokeHostFunctionOp
            {
                HostFunction = new Xdr.HostFunction
                {
                    Discriminant = new HostFunctionType
                    {
                        InnerValue = HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_CREATE_CONTRACT_V2,
                    },
                    CreateContractV2 = HostFunction.ToXdr(),
                },
                Auth = Auth.Select(a => a.ToXdr()).ToArray(),
            },
        };
    }

    public static CreateContractOperation FromXdr(InvokeHostFunctionOp invokeHostFunctionOp)
    {
        var createContractArgs = invokeHostFunctionOp.HostFunction.CreateContractV2;
        return new CreateContractOperation(
            new CreateContractV2HostFunction(
                ContractIdPreimage.FromXdr(createContractArgs.ContractIDPreimage),
                ContractExecutable.FromXdr(createContractArgs.Executable),
                createContractArgs.ConstructorArgs.Select(SCVal.FromXdr).ToArray()
            )
        )
        {
            Auth = invokeHostFunctionOp.Auth.Select(SorobanAuthorizationEntry.FromXdr).ToArray(),
        };
    }
}

public class UploadContractOperation : InvokeHostFunctionOperation
{
    /// <summary>
    ///     Constructs a new <c>UploadContractOperation</c>.
    /// </summary>
    /// <param name="wasm">The contract Wasm.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public UploadContractOperation(byte[] wasm, IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        HostFunction = new UploadContractHostFunction(wasm);
    }

    public UploadContractHostFunction HostFunction { get; }

    /// <summary>
    ///     The contract Wasm to be uploaded.
    /// </summary>
    public byte[] Wasm => HostFunction.Wasm;

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION),
            InvokeHostFunctionOp = new InvokeHostFunctionOp
            {
                HostFunction = new Xdr.HostFunction
                {
                    Discriminant = new HostFunctionType
                    {
                        InnerValue = HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_UPLOAD_CONTRACT_WASM,
                    },
                    Wasm = HostFunction.Wasm,
                },
                Auth = Auth.Select(a => a.ToXdr()).ToArray(),
            },
        };
    }

    public static UploadContractOperation FromXdr(InvokeHostFunctionOp invokeHostFunctionOp)
    {
        return new UploadContractOperation(invokeHostFunctionOp.HostFunction.Wasm)
        {
            Auth = invokeHostFunctionOp.Auth.Select(SorobanAuthorizationEntry.FromXdr).ToArray(),
        };
    }
}

public abstract class HostFunction;

public class InvokeContractHostFunction : HostFunction
{
    public InvokeContractHostFunction(
        ScAddress contractAddress,
        SCSymbol functionName,
        SCVal[] args
    )
    {
        if (contractAddress is ScClaimableBalanceId or ScLiquidityPoolId)
        {
            throw new InvalidOperationException(
                "Claimable balances and liquidity pools cannot be arguments to invokeHostFunction.");
        }
        ContractAddress = contractAddress;
        FunctionName = functionName;
        Args = args;
    }

    public ScAddress ContractAddress { get; }
    public SCSymbol FunctionName { get; }
    public SCVal[] Args { get; }

    public static InvokeContractHostFunction FromXdr(InvokeContractArgs xdrInvokeContractArgs)
    {
        return new InvokeContractHostFunction(
            ScAddress.FromXdr(xdrInvokeContractArgs.ContractAddress),
            SCSymbol.FromXdr(xdrInvokeContractArgs.FunctionName),
            xdrInvokeContractArgs.Args.Select(SCVal.FromXdr).ToArray()
        );
    }

    public InvokeContractArgs ToXdr()
    {
        return new InvokeContractArgs
        {
            ContractAddress = ContractAddress.ToXdr(),
            FunctionName = FunctionName.ToXdr(),
            Args = Args.Select(a => a.ToXdr()).ToArray(),
        };
    }
}

/// <summary>
///     Represents a create contract host function.
/// </summary>
public class CreateContractHostFunction : HostFunction
{
    public CreateContractHostFunction(
        ContractIdPreimage contractIdPreimage,
        ContractExecutable executable
    )
    {
        ContractIdPreimage = contractIdPreimage;
        Executable = executable;
    }

    /// <summary>
    ///     Constructs a create contract host function.
    /// </summary>
    /// <param name="wasmHash">A hex-encoded string of previously uploaded Wasm bytes of a compiled smart contract.</param>
    /// <param name="address">An account address.</param>
    /// <param name="salt">(Optional) A salt. Will be randomly generated if not provided.</param>
    public CreateContractHostFunction(string wasmHash, string address, byte[]? salt = null)
    {
        ContractIdPreimage = new ContractIdAddressPreimage(address, salt);
        Executable = new ContractExecutableWasm(wasmHash);
    }

    public ContractIdPreimage ContractIdPreimage { get; }
    public ContractExecutable Executable { get; }

    public static CreateContractHostFunction FromXdr(CreateContractArgs xdrCreateContractArgs)
    {
        return new CreateContractHostFunction(
            ContractIdPreimage.FromXdr(xdrCreateContractArgs.ContractIDPreimage),
            ContractExecutable.FromXdr(xdrCreateContractArgs.Executable)
        );
    }

    public CreateContractArgs ToXdr()
    {
        return new CreateContractArgs
        {
            ContractIDPreimage = ContractIdPreimage.ToXdr(),
            Executable = Executable.ToXdr(),
        };
    }
}

/// <summary>
///     Represents a create contract V2 host function.
/// </summary>
public class CreateContractV2HostFunction : HostFunction
{
    public CreateContractV2HostFunction(
        ContractIdPreimage contractIdPreimage,
        ContractExecutable executable,
        SCVal[] arguments
    )
    {
        ContractIdPreimage = contractIdPreimage;
        Executable = executable;
        Arguments = arguments;
    }

    /// <summary>
    ///     Constructs a create contract V2 host function.
    /// </summary>
    /// <param name="wasmHash">A hex-encoded string of previously uploaded Wasm bytes of a compiled smart contract.</param>
    /// <param name="address">An account address.</param>
    /// <param name="salt">(Optional) A salt. Will be randomly generated if not provided.</param>
    public CreateContractV2HostFunction(
        string wasmHash,
        string address,
        SCVal[] arguments,
        byte[]? salt = null
    )
    {
        ContractIdPreimage = new ContractIdAddressPreimage(address, salt);
        Executable = new ContractExecutableWasm(wasmHash);
        Arguments = arguments;
    }

    public ContractIdPreimage ContractIdPreimage { get; }
    public ContractExecutable Executable { get; }
    public SCVal[] Arguments { get; }

    public static CreateContractV2HostFunction FromXdr(CreateContractArgsV2 xdrCreateContractArgs)
    {
        return new CreateContractV2HostFunction(
            ContractIdPreimage.FromXdr(xdrCreateContractArgs.ContractIDPreimage),
            ContractExecutable.FromXdr(xdrCreateContractArgs.Executable),
            xdrCreateContractArgs.ConstructorArgs.Select(SCVal.FromXdr).ToArray()
        );
    }

    public CreateContractArgsV2 ToXdr()
    {
        return new CreateContractArgsV2
        {
            ContractIDPreimage = ContractIdPreimage.ToXdr(),
            Executable = Executable.ToXdr(),
            ConstructorArgs = Arguments.Select(a => a.ToXdr()).ToArray(),
        };
    }
}

public class UploadContractHostFunction : HostFunction
{
    public UploadContractHostFunction(byte[] wasm)
    {
        Wasm = wasm;
    }

    public byte[] Wasm { get; }
}

/// <summary>
///     Contains a tree of invocations with <c>rootInvocation</c> as a root.
///     Building SorobanAuthorizedInvocation trees may be simplified by using the recording auth mode in Soroban's
///     <see cref="SorobanServer.SimulateTransaction" /> mechanism.
/// </summary>
public class SorobanAuthorizationEntry
{
    public SorobanAuthorizationEntry(SorobanCredentials credentials, SorobanAuthorizedInvocation rootInvocation)
    {
        Credentials = credentials;
        RootInvocation = rootInvocation;
    }

    /// <summary>
    ///     The user that authorizes the invocations.
    /// </summary>
    public SorobanCredentials Credentials { get; }

    public SorobanAuthorizedInvocation RootInvocation { get; }

    public Xdr.SorobanAuthorizationEntry ToXdr()
    {
        return new Xdr.SorobanAuthorizationEntry
        {
            Credentials = Credentials.ToXdr(),
            RootInvocation = RootInvocation.ToXdr(),
        };
    }

    public static SorobanAuthorizationEntry FromXdr(Xdr.SorobanAuthorizationEntry xdr)
    {
        return new SorobanAuthorizationEntry(
            SorobanCredentials.FromXdr(xdr.Credentials),
            SorobanAuthorizedInvocation.FromXdr(xdr.RootInvocation)
        );
    }

    /// <summary>
    ///     Creates a new <c>SorobanAuthorizationEntry</c> object from the given SorobanAuthorizationEntry XDR base64 string.
    /// </summary>
    public static SorobanAuthorizationEntry FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = Xdr.SorobanAuthorizationEntry.Decode(reader);
        return FromXdr(thisXdr);
    }
}

public abstract class SorobanCredentials
{
    public Xdr.SorobanCredentials ToXdr()
    {
        return this switch
        {
            SorobanSourceAccountCredentials sourceAccount => sourceAccount.ToSorobanCredentialsXdr(),
            SorobanAddressCredentials address => address.ToSorobanCredentialsXdr(),
            _ => throw new InvalidOperationException("Unknown SorobanCredentials type"),
        };
    }

    public static SorobanCredentials FromXdr(Xdr.SorobanCredentials xdrSorobanCredentials)
    {
        return xdrSorobanCredentials.Discriminant.InnerValue switch
        {
            CredentialsType.SOROBAN_CREDENTIALS_SOURCE_ACCOUNT =>
                new SorobanSourceAccountCredentials(),
            CredentialsType.SOROBAN_CREDENTIALS_ADDRESS =>
                SorobanAddressCredentials.FromSorobanCredentialsXdr(xdrSorobanCredentials),
            _ => throw new InvalidOperationException("Unknown SorobanCredentials type"),
        };
    }
}

/// <summary>
///     This simply uses the signature of the transaction (or operation, if any) source account and hence doesn't require
///     any additional payload.
/// </summary>
public class SorobanSourceAccountCredentials : SorobanCredentials
{
    public Xdr.SorobanCredentials ToSorobanCredentialsXdr()
    {
        return new Xdr.SorobanCredentials
        {
            Discriminant = new SorobanCredentialsType
            {
                InnerValue = CredentialsType.SOROBAN_CREDENTIALS_SOURCE_ACCOUNT,
            },
        };
    }
}

public class SorobanAddressCredentials : SorobanCredentials
{
    public SorobanAddressCredentials(ScAddress address, long nonce, uint signatureExpirationLedger, SCVal signature)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address), "Address cannot be null.");
        Nonce = nonce;
        SignatureExpirationLedger = signatureExpirationLedger;
        Signature = signature ?? throw new ArgumentNullException(nameof(signature), "Signature cannot be null.");
    }

    /// <summary>
    ///     The address that authorizes invocation.
    /// </summary>
    public ScAddress Address { get; }

    /// <summary>
    ///     Is an arbitrary value that is unique for all the signatures performed by <c>address</c> until
    ///     <c>signatureExpirationLedger</c>. A good approach to generating this is to just use a random value.
    /// </summary>
    public long Nonce { get; }

    /// <summary>
    ///     The ledger sequence number on which the signature expires.
    /// </summary>
    public uint SignatureExpirationLedger { get; }

    public SCVal Signature { get; }

    public static SorobanAddressCredentials FromSorobanCredentialsXdr(Xdr.SorobanCredentials xdrSorobanCredentials)
    {
        if (xdrSorobanCredentials.Discriminant.InnerValue != CredentialsType.SOROBAN_CREDENTIALS_ADDRESS)
        {
            throw new InvalidOperationException("Invalid SorobanCredentials type");
        }

        return new SorobanAddressCredentials(
            ScAddress.FromXdr(xdrSorobanCredentials.Address.Address),
            xdrSorobanCredentials.Address.Nonce.InnerValue,
            xdrSorobanCredentials.Address.SignatureExpirationLedger.InnerValue,
            SCVal.FromXdr(xdrSorobanCredentials.Address.Signature)
        );
    }

    public Xdr.SorobanCredentials ToSorobanCredentialsXdr()
    {
        return new Xdr.SorobanCredentials
        {
            Discriminant = new SorobanCredentialsType
            {
                InnerValue = CredentialsType.SOROBAN_CREDENTIALS_ADDRESS,
            },
            Address = new Xdr.SorobanAddressCredentials
            {
                Address = Address.ToXdr(),
                Nonce = new Int64(Nonce),
                SignatureExpirationLedger = new Uint32(SignatureExpirationLedger),
                Signature = Signature.ToXdr(),
            },
        };
    }
}

public class SorobanAuthorizedInvocation
{
    public SorobanAuthorizedInvocation(
        SorobanAuthorizedFunction function,
        SorobanAuthorizedInvocation[] subInvocations
    )
    {
        Function = function;
        SubInvocations = subInvocations;
    }

    public SorobanAuthorizedFunction Function { get; }
    public SorobanAuthorizedInvocation[] SubInvocations { get; }

    public Xdr.SorobanAuthorizedInvocation ToXdr()
    {
        return new Xdr.SorobanAuthorizedInvocation
        {
            Function = Function.ToXdr(),
            SubInvocations = SubInvocations.Select(i => i.ToXdr()).ToArray(),
        };
    }

    public static SorobanAuthorizedInvocation FromXdr(Xdr.SorobanAuthorizedInvocation xdr)
    {
        return new SorobanAuthorizedInvocation(
            SorobanAuthorizedFunction.FromXdr(xdr.Function),
            xdr.SubInvocations.Select(FromXdr).ToArray()
        );
    }
}

public abstract class SorobanAuthorizedFunction
{
    public Xdr.SorobanAuthorizedFunction ToXdr()
    {
        return this switch
        {
            SorobanAuthorizedContractFunction contractFn
                => contractFn.ToSorobanAuthorizedFunctionXdr(),
            SorobanAuthorizedCreateContractFunction createContractHostFn
                => createContractHostFn.ToSorobanAuthorizedFunctionXdr(),
            SorobanAuthorizedCreateContractV2Function createContractHostV2Fn
                => createContractHostV2Fn.ToSorobanAuthorizedFunctionXdr(),
            _ => throw new InvalidOperationException("Unknown SorobanAuthorizedFunction type"),
        };
    }

    public static SorobanAuthorizedFunction FromXdr(Xdr.SorobanAuthorizedFunction xdrSorobanAuthorizedFunction)
    {
        return xdrSorobanAuthorizedFunction.Discriminant.InnerValue switch
        {
            FunctionType.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN
                => SorobanAuthorizedContractFunction
                    .FromSorobanAuthorizedFunctionXdr(xdrSorobanAuthorizedFunction),
            FunctionType.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_HOST_FN
                => SorobanAuthorizedCreateContractFunction
                    .FromSorobanAuthorizedFunctionXdr(xdrSorobanAuthorizedFunction),
            FunctionType.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_V2_HOST_FN
                => SorobanAuthorizedCreateContractV2Function
                    .FromSorobanAuthorizedFunctionXdr(xdrSorobanAuthorizedFunction),
            _ => throw new InvalidOperationException("Unknown SorobanAuthorizedFunction type"),
        };
    }
}

public class SorobanAuthorizedContractFunction : SorobanAuthorizedFunction
{
    public SorobanAuthorizedContractFunction(InvokeContractHostFunction hostFunction)
    {
        HostFunction = hostFunction;
    }

    public InvokeContractHostFunction HostFunction { get; }

    public static SorobanAuthorizedFunction FromSorobanAuthorizedFunctionXdr(
        Xdr.SorobanAuthorizedFunction xdrSorobanAuthorizedFunction
    )
    {
        if (xdrSorobanAuthorizedFunction.Discriminant.InnerValue !=
            FunctionType.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN)
        {
            throw new InvalidOperationException("Invalid SorobanAuthorizedFunction type");
        }

        return new SorobanAuthorizedContractFunction(
            InvokeContractHostFunction.FromXdr(xdrSorobanAuthorizedFunction.ContractFn)
        );
    }

    public Xdr.SorobanAuthorizedFunction ToSorobanAuthorizedFunctionXdr()
    {
        return new Xdr.SorobanAuthorizedFunction
        {
            Discriminant = new SorobanAuthorizedFunctionType
            {
                InnerValue = FunctionType.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN,
            },
            ContractFn = HostFunction.ToXdr(),
        };
    }
}

public class SorobanAuthorizedCreateContractFunction : SorobanAuthorizedFunction
{
    public SorobanAuthorizedCreateContractFunction(CreateContractHostFunction hostFunction)
    {
        HostFunction = hostFunction;
    }

    public CreateContractHostFunction HostFunction { get; }

    public static SorobanAuthorizedFunction FromSorobanAuthorizedFunctionXdr(
        Xdr.SorobanAuthorizedFunction xdrSorobanAuthorizedFunction
    )
    {
        if (xdrSorobanAuthorizedFunction.Discriminant.InnerValue !=
            FunctionType.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_HOST_FN)
        {
            throw new InvalidOperationException("Invalid SorobanAuthorizedFunction type");
        }

        return new SorobanAuthorizedCreateContractFunction(
            CreateContractHostFunction.FromXdr(xdrSorobanAuthorizedFunction.CreateContractHostFn)
        );
    }

    public Xdr.SorobanAuthorizedFunction ToSorobanAuthorizedFunctionXdr()
    {
        return new Xdr.SorobanAuthorizedFunction
        {
            Discriminant = new SorobanAuthorizedFunctionType
            {
                InnerValue = FunctionType.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_HOST_FN,
            },
            CreateContractHostFn = HostFunction.ToXdr(),
        };
    }
}

public class SorobanAuthorizedCreateContractV2Function : SorobanAuthorizedFunction
{
    public SorobanAuthorizedCreateContractV2Function(CreateContractV2HostFunction hostFunction)
    {
        HostFunction = hostFunction;
    }

    public CreateContractV2HostFunction HostFunction { get; }

    public static SorobanAuthorizedFunction FromSorobanAuthorizedFunctionXdr(
        Xdr.SorobanAuthorizedFunction xdrSorobanAuthorizedFunction
    )
    {
        if (xdrSorobanAuthorizedFunction.Discriminant.InnerValue !=
            FunctionType.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_V2_HOST_FN)
        {
            throw new InvalidOperationException("Invalid SorobanAuthorizedFunction type");
        }

        return new SorobanAuthorizedCreateContractV2Function(
            CreateContractV2HostFunction.FromXdr(xdrSorobanAuthorizedFunction.CreateContractV2HostFn)
        );
    }

    public Xdr.SorobanAuthorizedFunction ToSorobanAuthorizedFunctionXdr()
    {
        return new Xdr.SorobanAuthorizedFunction
        {
            Discriminant = new SorobanAuthorizedFunctionType
            {
                InnerValue = FunctionType.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_V2_HOST_FN,
            },
            CreateContractV2HostFn = HostFunction.ToXdr(),
        };
    }
}

public abstract class ContractIdPreimage
{
    public ContractIDPreimage ToXdr()
    {
        return this switch
        {
            ContractIdAddressPreimage fromAddress => fromAddress.ToContractIdPreimageXdr(),
            ContractIdAssetPreimage fromAsset => fromAsset.ToContractIdPreimageXdr(),
            _ => throw new InvalidOperationException("Unknown ContractIDPreimage type"),
        };
    }

    public static ContractIdPreimage FromXdr(ContractIDPreimage xdrContractIdPreimage)
    {
        return xdrContractIdPreimage.Discriminant.InnerValue switch
        {
            PreimageType.CONTRACT_ID_PREIMAGE_FROM_ADDRESS =>
                ContractIdAddressPreimage.FromContractIdPreimageXdr(xdrContractIdPreimage),
            PreimageType.CONTRACT_ID_PREIMAGE_FROM_ASSET =>
                ContractIdAssetPreimage.FromContractIdPreimageXdr(xdrContractIdPreimage),
            _ => throw new InvalidOperationException("Unknown ContractIdPreimage type"),
        };
    }
}

public class ContractIdAddressPreimage : ContractIdPreimage
{
    public ContractIdAddressPreimage(string address, byte[]? salt = null)
    {
        Address = new ScAccountId(address);

        if (salt != null)
        {
            if (salt.Length != 32)
            {
                throw new ArgumentException("Salt must have exactly 32 bytes.", nameof(salt));
            }
        }
        else
        {
            salt = new byte[32];
            RandomNumberGenerator.Create().GetBytes(salt);
        }

        Salt = salt;
    }

    public ScAddress Address { get; }
    public byte[] Salt { get; }

    public static ContractIdPreimage FromContractIdPreimageXdr(ContractIDPreimage xdrContractIdPreimage)
    {
        if (xdrContractIdPreimage.Discriminant.InnerValue != PreimageType.CONTRACT_ID_PREIMAGE_FROM_ADDRESS)
        {
            throw new InvalidOperationException("Invalid ContractIdPreimage type");
        }

        return new ContractIdAddressPreimage(
            KeyPair.FromXdrPublicKey(xdrContractIdPreimage.FromAddress.Address.AccountId.InnerValue).AccountId,
            xdrContractIdPreimage.FromAddress.Salt.InnerValue
        );
    }

    public ContractIDPreimage ToContractIdPreimageXdr()
    {
        return new ContractIDPreimage
        {
            Discriminant = new ContractIDPreimageType
            {
                InnerValue = PreimageType.CONTRACT_ID_PREIMAGE_FROM_ADDRESS,
            },
            FromAddress = new ContractIDPreimage.ContractIDPreimageFromAddress
            {
                Address = Address.ToXdr(),
                Salt = new Uint256(Salt),
            },
        };
    }
}

public class ContractIdAssetPreimage : ContractIdPreimage
{
    public ContractIdAssetPreimage(Asset asset)
    {
        Asset = asset;
    }

    public Asset Asset { get; }

    public static ContractIdPreimage FromContractIdPreimageXdr(ContractIDPreimage xdrContractIdPreimage)
    {
        if (xdrContractIdPreimage.Discriminant.InnerValue != PreimageType.CONTRACT_ID_PREIMAGE_FROM_ASSET)
        {
            throw new InvalidOperationException("Invalid ContractIdPreimage type");
        }

        return new ContractIdAssetPreimage(Asset.FromXdr(xdrContractIdPreimage.FromAsset));
    }

    public ContractIDPreimage ToContractIdPreimageXdr()
    {
        return new ContractIDPreimage
        {
            Discriminant = new ContractIDPreimageType
            {
                InnerValue = PreimageType.CONTRACT_ID_PREIMAGE_FROM_ASSET,
            },
            FromAsset = Asset.ToXdr(),
        };
    }
}