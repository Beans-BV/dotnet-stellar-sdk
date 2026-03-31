using System;
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
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvokeHostFunctionOperation" /> class.
    /// </summary>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
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

    /// <summary>
    ///     The host function containing the contract address, function name, and arguments for the invocation.
    /// </summary>
    public InvokeContractHostFunction HostFunction { get; }

    /// <inheritdoc />
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

    /// <summary>
    ///     Creates a new <see cref="InvokeContractOperation" /> from the given XDR <see cref="InvokeHostFunctionOp" />.
    /// </summary>
    /// <param name="invokeHostFunctionOp">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="InvokeContractOperation" /> instance.</returns>
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

/// <summary>
///     Operation that invokes a Soroban host function to deploy a new smart contract on the Stellar network.
///     Supports creating contracts from a Wasm hash with an address-derived contract ID, or deploying
///     the builtin Soroban Asset Contract from a Stellar asset.
/// </summary>
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

    /// <inheritdoc />
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

    /// <summary>
    ///     Creates a new <see cref="CreateContractOperation" /> from the given XDR <see cref="InvokeHostFunctionOp" />.
    /// </summary>
    /// <param name="invokeHostFunctionOp">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="CreateContractOperation" /> instance.</returns>
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

/// <summary>
///     Operation that invokes a Soroban host function to upload compiled smart contract Wasm bytecode
///     to the Stellar network. The uploaded Wasm can subsequently be used to create contract instances
///     via <see cref="CreateContractOperation" />.
/// </summary>
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

    /// <summary>
    ///     The host function that holds the compiled Wasm bytecode to be uploaded.
    /// </summary>
    public UploadContractHostFunction HostFunction { get; }

    /// <summary>
    ///     The contract Wasm to be uploaded.
    /// </summary>
    public byte[] Wasm => HostFunction.Wasm;

    /// <inheritdoc />
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

    /// <summary>
    ///     Creates a new <see cref="UploadContractOperation" /> from the given XDR <see cref="InvokeHostFunctionOp" />.
    /// </summary>
    /// <param name="invokeHostFunctionOp">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="UploadContractOperation" /> instance.</returns>
    public static UploadContractOperation FromXdr(InvokeHostFunctionOp invokeHostFunctionOp)
    {
        return new UploadContractOperation(invokeHostFunctionOp.HostFunction.Wasm)
        {
            Auth = invokeHostFunctionOp.Auth.Select(SorobanAuthorizationEntry.FromXdr).ToArray(),
        };
    }
}

/// <summary>
///     Abstract base class for Soroban host functions that can be invoked via an
///     <see cref="InvokeHostFunctionOperation" />.
/// </summary>
public abstract class HostFunction;

/// <summary>
///     Represents a host function that invokes a method on an existing Soroban smart contract.
///     Encapsulates the contract address, function name, and arguments required for the invocation.
/// </summary>
public class InvokeContractHostFunction : HostFunction
{
    /// <summary>
    ///     Constructs a new <see cref="InvokeContractHostFunction" />.
    /// </summary>
    /// <param name="contractAddress">The address of the contract to invoke.</param>
    /// <param name="functionName">The name of the contract function to invoke.</param>
    /// <param name="args">The arguments to pass to the contract function.</param>
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

    /// <summary>
    ///     The address of the contract to invoke.
    /// </summary>
    public ScAddress ContractAddress { get; }

    /// <summary>
    ///     The name of the contract function to invoke.
    /// </summary>
    public SCSymbol FunctionName { get; }

    /// <summary>
    ///     The arguments to pass to the contract function.
    /// </summary>
    public SCVal[] Args { get; }

    /// <summary>
    ///     Creates a new <see cref="InvokeContractHostFunction" /> from the given XDR
    ///     <see cref="InvokeContractArgs" />.
    /// </summary>
    /// <param name="xdrInvokeContractArgs">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="InvokeContractHostFunction" /> instance.</returns>
    public static InvokeContractHostFunction FromXdr(InvokeContractArgs xdrInvokeContractArgs)
    {
        return new InvokeContractHostFunction(
            ScAddress.FromXdr(xdrInvokeContractArgs.ContractAddress),
            SCSymbol.FromXdr(xdrInvokeContractArgs.FunctionName),
            xdrInvokeContractArgs.Args.Select(SCVal.FromXdr).ToArray()
        );
    }

    /// <summary>
    ///     Converts this <see cref="InvokeContractHostFunction" /> to its XDR <see cref="InvokeContractArgs" />
    ///     representation.
    /// </summary>
    /// <returns>An <see cref="InvokeContractArgs" /> XDR object.</returns>
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
    /// <summary>
    ///     Constructs a new <see cref="CreateContractHostFunction" />.
    /// </summary>
    /// <param name="contractIdPreimage">The preimage used to derive the contract ID.</param>
    /// <param name="executable">The contract executable (Wasm reference or builtin Stellar asset contract).</param>
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

    /// <summary>
    ///     The preimage used to derive the contract ID.
    /// </summary>
    public ContractIdPreimage ContractIdPreimage { get; }

    /// <summary>
    ///     The contract executable (Wasm reference or builtin Stellar asset contract).
    /// </summary>
    public ContractExecutable Executable { get; }

    /// <summary>
    ///     Creates a new <see cref="CreateContractHostFunction" /> from the given XDR
    ///     <see cref="CreateContractArgs" />.
    /// </summary>
    /// <param name="xdrCreateContractArgs">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="CreateContractHostFunction" /> instance.</returns>
    public static CreateContractHostFunction FromXdr(CreateContractArgs xdrCreateContractArgs)
    {
        return new CreateContractHostFunction(
            ContractIdPreimage.FromXdr(xdrCreateContractArgs.ContractIDPreimage),
            ContractExecutable.FromXdr(xdrCreateContractArgs.Executable)
        );
    }

    /// <summary>
    ///     Converts this <see cref="CreateContractHostFunction" /> to its XDR <see cref="CreateContractArgs" />
    ///     representation.
    /// </summary>
    /// <returns>A <see cref="CreateContractArgs" /> XDR object.</returns>
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
    /// <summary>
    ///     Constructs a new <see cref="CreateContractV2HostFunction" />.
    /// </summary>
    /// <param name="contractIdPreimage">The preimage used to derive the contract ID.</param>
    /// <param name="executable">The contract executable (Wasm reference or builtin Stellar asset contract).</param>
    /// <param name="arguments">The arguments to pass to the contract constructor during deployment.</param>
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

    /// <summary>
    ///     The preimage used to derive the contract ID.
    /// </summary>
    public ContractIdPreimage ContractIdPreimage { get; }

    /// <summary>
    ///     The contract executable (Wasm reference or builtin Stellar asset contract).
    /// </summary>
    public ContractExecutable Executable { get; }

    /// <summary>
    ///     The arguments to pass to the contract constructor during deployment.
    /// </summary>
    public SCVal[] Arguments { get; }

    /// <summary>
    ///     Creates a new <see cref="CreateContractV2HostFunction" /> from the given XDR
    ///     <see cref="CreateContractArgsV2" />.
    /// </summary>
    /// <param name="xdrCreateContractArgs">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="CreateContractV2HostFunction" /> instance.</returns>
    public static CreateContractV2HostFunction FromXdr(CreateContractArgsV2 xdrCreateContractArgs)
    {
        return new CreateContractV2HostFunction(
            ContractIdPreimage.FromXdr(xdrCreateContractArgs.ContractIDPreimage),
            ContractExecutable.FromXdr(xdrCreateContractArgs.Executable),
            xdrCreateContractArgs.ConstructorArgs.Select(SCVal.FromXdr).ToArray()
        );
    }

    /// <summary>
    ///     Converts this <see cref="CreateContractV2HostFunction" /> to its XDR <see cref="CreateContractArgsV2" />
    ///     representation.
    /// </summary>
    /// <returns>A <see cref="CreateContractArgsV2" /> XDR object.</returns>
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

/// <summary>
///     Represents a host function that uploads compiled smart contract Wasm bytecode to the Stellar network.
///     The uploaded Wasm is stored on-chain and can be referenced by its hash when creating new contract instances.
/// </summary>
public class UploadContractHostFunction : HostFunction
{
    /// <summary>
    ///     Constructs a new <see cref="UploadContractHostFunction" />.
    /// </summary>
    /// <param name="wasm">The compiled smart contract Wasm bytecode.</param>
    public UploadContractHostFunction(byte[] wasm)
    {
        Wasm = wasm;
    }

    /// <summary>
    ///     The compiled smart contract Wasm bytecode to be uploaded.
    /// </summary>
    public byte[] Wasm { get; }
}

/// <summary>
///     Contains a tree of invocations with <c>rootInvocation</c> as a root.
///     Building SorobanAuthorizedInvocation trees may be simplified by using the recording auth mode in Soroban's
///     <see cref="SorobanServer.SimulateTransaction" /> mechanism.
/// </summary>
public class SorobanAuthorizationEntry
{
    /// <summary>
    ///     Constructs a new <see cref="SorobanAuthorizationEntry" />.
    /// </summary>
    /// <param name="credentials">The credentials identifying and authenticating the authorizing entity.</param>
    /// <param name="rootInvocation">The root of the authorized invocation tree.</param>
    public SorobanAuthorizationEntry(SorobanCredentials credentials, SorobanAuthorizedInvocation rootInvocation)
    {
        Credentials = credentials;
        RootInvocation = rootInvocation;
    }

    /// <summary>
    ///     The user that authorizes the invocations.
    /// </summary>
    public SorobanCredentials Credentials { get; }

    /// <summary>
    ///     The root of the authorized invocation tree.
    /// </summary>
    public SorobanAuthorizedInvocation RootInvocation { get; }

    /// <summary>
    ///     Converts this <see cref="SorobanAuthorizationEntry" /> to its XDR
    ///     <see cref="Xdr.SorobanAuthorizationEntry" /> representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanAuthorizationEntry" /> XDR object.</returns>
    public Xdr.SorobanAuthorizationEntry ToXdr()
    {
        return new Xdr.SorobanAuthorizationEntry
        {
            Credentials = Credentials.ToXdr(),
            RootInvocation = RootInvocation.ToXdr(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SorobanAuthorizationEntry" /> from the given XDR
    ///     <see cref="Xdr.SorobanAuthorizationEntry" />.
    /// </summary>
    /// <param name="xdr">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="SorobanAuthorizationEntry" /> instance.</returns>
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

/// <summary>
///     Abstract base class for Soroban authorization credentials.
///     Credentials identify and authenticate the entity authorizing a contract invocation.
/// </summary>
/// <seealso cref="SorobanSourceAccountCredentials" />
/// <seealso cref="SorobanAddressCredentials" />
public abstract class SorobanCredentials
{
    /// <summary>
    ///     Converts this <see cref="SorobanCredentials" /> to its XDR <see cref="Xdr.SorobanCredentials" /> representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanCredentials" /> XDR object.</returns>
    public Xdr.SorobanCredentials ToXdr()
    {
        return this switch
        {
            SorobanSourceAccountCredentials sourceAccount => sourceAccount.ToSorobanCredentialsXdr(),
            SorobanAddressCredentials address => address.ToSorobanCredentialsXdr(),
            _ => throw new InvalidOperationException("Unknown SorobanCredentials type"),
        };
    }

    /// <summary>
    ///     Creates a <see cref="SorobanCredentials" /> subclass instance from the given XDR
    ///     <see cref="Xdr.SorobanCredentials" />.
    /// </summary>
    /// <param name="xdrSorobanCredentials">The XDR object to deserialize.</param>
    /// <returns>
    ///     A <see cref="SorobanSourceAccountCredentials" /> or <see cref="SorobanAddressCredentials" /> instance.
    /// </returns>
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
    /// <summary>
    ///     Converts this <see cref="SorobanSourceAccountCredentials" /> to its XDR
    ///     <see cref="Xdr.SorobanCredentials" /> representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanCredentials" /> XDR object.</returns>
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

/// <summary>
///     Represents Soroban credentials that authenticate a specific address for contract invocation authorization.
///     Contains the authorizing address, a unique nonce, a signature expiration ledger, and the cryptographic signature.
/// </summary>
public class SorobanAddressCredentials : SorobanCredentials
{
    /// <summary>
    ///     Constructs a new <see cref="SorobanAddressCredentials" />.
    /// </summary>
    /// <param name="address">The address that authorizes the invocation.</param>
    /// <param name="nonce">
    ///     An arbitrary value that must be unique for all signatures performed by <paramref name="address" />
    ///     until <paramref name="signatureExpirationLedger" />.
    /// </param>
    /// <param name="signatureExpirationLedger">The ledger sequence number on which the signature expires.</param>
    /// <param name="signature">The cryptographic signature authenticating the authorization.</param>
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

    /// <summary>
    ///     The cryptographic signature authenticating the authorization.
    /// </summary>
    public SCVal Signature { get; }

    /// <summary>
    ///     Creates a new <see cref="SorobanAddressCredentials" /> from the given XDR
    ///     <see cref="Xdr.SorobanCredentials" />.
    /// </summary>
    /// <param name="xdrSorobanCredentials">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="SorobanAddressCredentials" /> instance.</returns>
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

    /// <summary>
    ///     Converts this <see cref="SorobanAddressCredentials" /> to its XDR <see cref="Xdr.SorobanCredentials" />
    ///     representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanCredentials" /> XDR object.</returns>
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

/// <summary>
///     Represents a single authorized Soroban invocation within an authorization tree.
///     Each invocation consists of the authorized function being called and an array of sub-invocations
///     that represent nested contract calls requiring the same authorization.
/// </summary>
public class SorobanAuthorizedInvocation
{
    /// <summary>
    ///     Constructs a new <see cref="SorobanAuthorizedInvocation" />.
    /// </summary>
    /// <param name="function">The authorized function being invoked.</param>
    /// <param name="subInvocations">Nested contract calls requiring the same authorization.</param>
    public SorobanAuthorizedInvocation(
        SorobanAuthorizedFunction function,
        SorobanAuthorizedInvocation[] subInvocations
    )
    {
        Function = function;
        SubInvocations = subInvocations;
    }

    /// <summary>
    ///     The authorized function being invoked at this node of the invocation tree.
    /// </summary>
    public SorobanAuthorizedFunction Function { get; }

    /// <summary>
    ///     Nested sub-invocations that are authorized under the same credentials as this invocation.
    /// </summary>
    public SorobanAuthorizedInvocation[] SubInvocations { get; }

    /// <summary>
    ///     Converts this <see cref="SorobanAuthorizedInvocation" /> to its XDR
    ///     <see cref="Xdr.SorobanAuthorizedInvocation" /> representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanAuthorizedInvocation" /> XDR object.</returns>
    public Xdr.SorobanAuthorizedInvocation ToXdr()
    {
        return new Xdr.SorobanAuthorizedInvocation
        {
            Function = Function.ToXdr(),
            SubInvocations = SubInvocations.Select(i => i.ToXdr()).ToArray(),
        };
    }

    /// <summary>
    ///     Creates a new <see cref="SorobanAuthorizedInvocation" /> from the given XDR
    ///     <see cref="Xdr.SorobanAuthorizedInvocation" />.
    /// </summary>
    /// <param name="xdr">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="SorobanAuthorizedInvocation" /> instance.</returns>
    public static SorobanAuthorizedInvocation FromXdr(Xdr.SorobanAuthorizedInvocation xdr)
    {
        return new SorobanAuthorizedInvocation(
            SorobanAuthorizedFunction.FromXdr(xdr.Function),
            xdr.SubInvocations.Select(FromXdr).ToArray()
        );
    }
}

/// <summary>
///     Abstract base class for authorized Soroban functions used within a
///     <see cref="SorobanAuthorizedInvocation" /> tree.
/// </summary>
/// <seealso cref="SorobanAuthorizedContractFunction" />
/// <seealso cref="SorobanAuthorizedCreateContractFunction" />
/// <seealso cref="SorobanAuthorizedCreateContractV2Function" />
public abstract class SorobanAuthorizedFunction
{
    /// <summary>
    ///     Converts this <see cref="SorobanAuthorizedFunction" /> to its XDR
    ///     <see cref="Xdr.SorobanAuthorizedFunction" /> representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanAuthorizedFunction" /> XDR object.</returns>
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

    /// <summary>
    ///     Creates a <see cref="SorobanAuthorizedFunction" /> subclass instance from the given XDR
    ///     <see cref="Xdr.SorobanAuthorizedFunction" />.
    /// </summary>
    /// <param name="xdrSorobanAuthorizedFunction">The XDR object to deserialize.</param>
    /// <returns>A concrete <see cref="SorobanAuthorizedFunction" /> subclass instance.</returns>
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

/// <summary>
///     Represents an authorized function that invokes a method on an existing Soroban smart contract.
///     Wraps an <see cref="InvokeContractHostFunction" /> for use within a <see cref="SorobanAuthorizedInvocation" />
///     tree.
/// </summary>
public class SorobanAuthorizedContractFunction : SorobanAuthorizedFunction
{
    /// <summary>
    ///     Constructs a new <see cref="SorobanAuthorizedContractFunction" />.
    /// </summary>
    /// <param name="hostFunction">The contract invocation host function being authorized.</param>
    public SorobanAuthorizedContractFunction(InvokeContractHostFunction hostFunction)
    {
        HostFunction = hostFunction;
    }

    /// <summary>
    ///     The contract invocation host function being authorized.
    /// </summary>
    public InvokeContractHostFunction HostFunction { get; }

    /// <summary>
    ///     Creates a new <see cref="SorobanAuthorizedContractFunction" /> from the given XDR
    ///     <see cref="Xdr.SorobanAuthorizedFunction" />.
    /// </summary>
    /// <param name="xdrSorobanAuthorizedFunction">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="SorobanAuthorizedContractFunction" /> instance.</returns>
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

    /// <summary>
    ///     Converts this <see cref="SorobanAuthorizedContractFunction" /> to its XDR
    ///     <see cref="Xdr.SorobanAuthorizedFunction" /> representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanAuthorizedFunction" /> XDR object.</returns>
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

/// <summary>
///     Represents an authorized function that creates a new Soroban smart contract.
///     Wraps a <see cref="CreateContractHostFunction" /> for use within a <see cref="SorobanAuthorizedInvocation" /> tree.
/// </summary>
public class SorobanAuthorizedCreateContractFunction : SorobanAuthorizedFunction
{
    /// <summary>
    ///     Constructs a new <see cref="SorobanAuthorizedCreateContractFunction" />.
    /// </summary>
    /// <param name="hostFunction">The create contract host function being authorized.</param>
    public SorobanAuthorizedCreateContractFunction(CreateContractHostFunction hostFunction)
    {
        HostFunction = hostFunction;
    }

    /// <summary>
    ///     The create contract host function being authorized.
    /// </summary>
    public CreateContractHostFunction HostFunction { get; }

    /// <summary>
    ///     Creates a new <see cref="SorobanAuthorizedCreateContractFunction" /> from the given XDR
    ///     <see cref="Xdr.SorobanAuthorizedFunction" />.
    /// </summary>
    /// <param name="xdrSorobanAuthorizedFunction">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="SorobanAuthorizedCreateContractFunction" /> instance.</returns>
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

    /// <summary>
    ///     Converts this <see cref="SorobanAuthorizedCreateContractFunction" /> to its XDR
    ///     <see cref="Xdr.SorobanAuthorizedFunction" /> representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanAuthorizedFunction" /> XDR object.</returns>
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

/// <summary>
///     Represents an authorized function that creates a new Soroban smart contract using the V2 protocol,
///     which supports passing constructor arguments during contract deployment.
///     Wraps a <see cref="CreateContractV2HostFunction" /> for use within a <see cref="SorobanAuthorizedInvocation" />
///     tree.
/// </summary>
public class SorobanAuthorizedCreateContractV2Function : SorobanAuthorizedFunction
{
    /// <summary>
    ///     Constructs a new <see cref="SorobanAuthorizedCreateContractV2Function" />.
    /// </summary>
    /// <param name="hostFunction">The create contract V2 host function being authorized.</param>
    public SorobanAuthorizedCreateContractV2Function(CreateContractV2HostFunction hostFunction)
    {
        HostFunction = hostFunction;
    }

    /// <summary>
    ///     The create contract V2 host function being authorized.
    /// </summary>
    public CreateContractV2HostFunction HostFunction { get; }

    /// <summary>
    ///     Creates a new <see cref="SorobanAuthorizedCreateContractV2Function" /> from the given XDR
    ///     <see cref="Xdr.SorobanAuthorizedFunction" />.
    /// </summary>
    /// <param name="xdrSorobanAuthorizedFunction">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="SorobanAuthorizedCreateContractV2Function" /> instance.</returns>
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

    /// <summary>
    ///     Converts this <see cref="SorobanAuthorizedCreateContractV2Function" /> to its XDR
    ///     <see cref="Xdr.SorobanAuthorizedFunction" /> representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanAuthorizedFunction" /> XDR object.</returns>
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

/// <summary>
///     Abstract base class for contract ID preimages used to deterministically derive a contract ID
///     when deploying a new Soroban smart contract.
/// </summary>
/// <seealso cref="ContractIdAddressPreimage" />
/// <seealso cref="ContractIdAssetPreimage" />
public abstract class ContractIdPreimage
{
    /// <summary>
    ///     Converts this <see cref="ContractIdPreimage" /> to its XDR <see cref="ContractIDPreimage" /> representation.
    /// </summary>
    /// <returns>A <see cref="ContractIDPreimage" /> XDR object.</returns>
    public ContractIDPreimage ToXdr()
    {
        return this switch
        {
            ContractIdAddressPreimage fromAddress => fromAddress.ToContractIdPreimageXdr(),
            ContractIdAssetPreimage fromAsset => fromAsset.ToContractIdPreimageXdr(),
            _ => throw new InvalidOperationException("Unknown ContractIDPreimage type"),
        };
    }

    /// <summary>
    ///     Creates a <see cref="ContractIdPreimage" /> subclass instance from the given XDR
    ///     <see cref="ContractIDPreimage" />.
    /// </summary>
    /// <param name="xdrContractIdPreimage">The XDR object to deserialize.</param>
    /// <returns>
    ///     A <see cref="ContractIdAddressPreimage" /> or <see cref="ContractIdAssetPreimage" /> instance.
    /// </returns>
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

/// <summary>
///     Represents a contract ID preimage derived from an account address and a 32-byte salt.
///     The contract ID is deterministically computed from the address and salt combination.
/// </summary>
public class ContractIdAddressPreimage : ContractIdPreimage
{
    /// <summary>
    ///     Constructs a new <see cref="ContractIdAddressPreimage" />.
    /// </summary>
    /// <param name="address">The account address used to derive the contract ID.</param>
    /// <param name="salt">
    ///     (Optional) A 32-byte salt. If not provided, a cryptographically random salt is generated.
    /// </param>
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

    /// <summary>
    ///     The account address used to derive the contract ID.
    /// </summary>
    public ScAddress Address { get; }

    /// <summary>
    ///     The 32-byte salt used together with <see cref="Address" /> to derive the contract ID.
    /// </summary>
    public byte[] Salt { get; }

    /// <summary>
    ///     Creates a new <see cref="ContractIdAddressPreimage" /> from the given XDR
    ///     <see cref="ContractIDPreimage" />.
    /// </summary>
    /// <param name="xdrContractIdPreimage">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="ContractIdAddressPreimage" /> instance.</returns>
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

    /// <summary>
    ///     Converts this <see cref="ContractIdAddressPreimage" /> to its XDR <see cref="ContractIDPreimage" /> representation
    ///     using the <c>CONTRACT_ID_PREIMAGE_FROM_ADDRESS</c> variant.
    /// </summary>
    /// <returns>A <see cref="ContractIDPreimage" /> XDR object.</returns>
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

/// <summary>
///     Represents a contract ID preimage derived from a Stellar asset.
///     Used when deploying the builtin Soroban Asset Contract for a given Stellar asset.
/// </summary>
public class ContractIdAssetPreimage : ContractIdPreimage
{
    /// <summary>
    ///     Constructs a new <see cref="ContractIdAssetPreimage" />.
    /// </summary>
    /// <param name="asset">The Stellar asset from which the contract ID is derived.</param>
    public ContractIdAssetPreimage(Asset asset)
    {
        Asset = asset;
    }

    /// <summary>
    ///     The Stellar asset from which the contract ID is derived.
    /// </summary>
    public Asset Asset { get; }

    /// <summary>
    ///     Creates a new <see cref="ContractIdAssetPreimage" /> from the given XDR
    ///     <see cref="ContractIDPreimage" />.
    /// </summary>
    /// <param name="xdrContractIdPreimage">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="ContractIdAssetPreimage" /> instance.</returns>
    public static ContractIdPreimage FromContractIdPreimageXdr(ContractIDPreimage xdrContractIdPreimage)
    {
        if (xdrContractIdPreimage.Discriminant.InnerValue != PreimageType.CONTRACT_ID_PREIMAGE_FROM_ASSET)
        {
            throw new InvalidOperationException("Invalid ContractIdPreimage type");
        }

        return new ContractIdAssetPreimage(Asset.FromXdr(xdrContractIdPreimage.FromAsset));
    }

    /// <summary>
    ///     Converts this <see cref="ContractIdAssetPreimage" /> to its XDR <see cref="ContractIDPreimage" /> representation
    ///     using the <c>CONTRACT_ID_PREIMAGE_FROM_ASSET</c> variant.
    /// </summary>
    /// <returns>A <see cref="ContractIDPreimage" /> XDR object.</returns>
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