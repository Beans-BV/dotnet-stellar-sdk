using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using Soroban_ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Make archived Soroban smart contract entries accessible again by restoring them with this restore footprint
///     operation.
///     This operation restores the archived entries specified in the readWrite footprint.
///     <p>Note that Soroban transactions can only contain one operation per transaction.</p>
/// </summary>
public class RestoreFootprintOperation : Operation
{
    public RestoreFootprintOperation(Soroban_ExtensionPoint extensionPoint)
    {
        ExtensionPoint = extensionPoint;
    }

    public Soroban_ExtensionPoint ExtensionPoint { get; }

    /// <summary>
    ///     Creates a new RestoreFootprintOperation object from the given base64-encoded XDR Operation.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>RestoreFootprintOperation object</returns>
    /// <exception cref="InvalidOperationException">Thrown when the base64-encoded XDR value is invalid.</exception>
    public static RestoreFootprintOperation FromOperationXdrBase64(string xdrBase64)
    {
        var operation = FromXdrBase64(xdrBase64);

        if (operation == null)
            throw new InvalidOperationException("Operation XDR is invalid");
        if (operation is not RestoreFootprintOperation restoreFootprintOperation)
            throw new InvalidOperationException("Operation is not RestoreFootprintOperation");

        return restoreFootprintOperation;
    }

    private RestoreFootprintOp ToRestoreFootprintOperationXdr()
    {
        return new RestoreFootprintOp
        {
            Ext = ExtensionPoint.ToXdr()
        };
    }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.RESTORE_FOOTPRINT),
            RestoreFootprintOp = ToRestoreFootprintOperationXdr()
        };
        return body;
    }

    public class Builder
    {
        private Soroban_ExtensionPoint? _extensionPoint;

        private KeyPair? _sourceAccount;

        public Builder(Soroban_ExtensionPoint? extensionPoint = null)
        {
            _extensionPoint = extensionPoint;
        }

        public Builder(RestoreFootprintOp operationXdr)
        {
            _extensionPoint = Soroban_ExtensionPoint.FromXdr(operationXdr.Ext);
        }

        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }

        public Builder SetExtensionPoint(Soroban_ExtensionPoint ext)
        {
            _extensionPoint = ext;
            return this;
        }

        public RestoreFootprintOperation Build()
        {
            var operation = new RestoreFootprintOperation(_extensionPoint ?? new ExtensionPointZero());
            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}