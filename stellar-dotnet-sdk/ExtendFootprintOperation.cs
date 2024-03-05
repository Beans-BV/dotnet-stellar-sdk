using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Operation that extends footprint TTL.
/// </summary>
public class ExtendFootprintOperation : Operation
{
    private ExtendFootprintOperation(uint extendTo, ExtensionPoint? extensionPoint = null)
    {
        ExtendTo = extendTo;
        if (extensionPoint != null) ExtensionPoint = extensionPoint;
    }

    public uint ExtendTo { get; }

    public ExtensionPoint ExtensionPoint { get; } = new ExtensionPointZero();

    /// <summary>
    ///     Creates a new ExtendFootprintOperation object from the given base64-encoded XDR Operation.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>ExtendFootprintOperation object</returns>
    /// <exception cref="InvalidOperationException">Thrown when the base64-encoded XDR value is invalid.</exception>
    public static ExtendFootprintOperation FromOperationXdrBase64(string xdrBase64)
    {
        var operation = FromXdrBase64(xdrBase64);
        if (operation == null)
            throw new InvalidOperationException("Operation XDR is invalid");

        if (operation is not ExtendFootprintOperation extendFootprintOperation)
            throw new InvalidOperationException("Operation is not ExtendFootprintOperation");
        return extendFootprintOperation;
    }

    private ExtendFootprintTTLOp ToExtendFootprintTTLOperationXdr()
    {
        return new ExtendFootprintTTLOp
        {
            Ext = ExtensionPoint.ToXdr(),
            ExtendTo = new Uint32(ExtendTo)
        };
    }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant = new OperationType
            {
                InnerValue = OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL
            },
            ExtendFootprintTTLOp = ToExtendFootprintTTLOperationXdr()
        };
        return body;
    }

    public class Builder
    {
        private readonly uint _extendTo;
        private ExtensionPoint _extensionPoint = new ExtensionPointZero();
        private KeyPair? _sourceAccount;

        public Builder(uint extendTo)
        {
            _extendTo = extendTo;
        }

        public Builder(ExtendFootprintTTLOp operationXdr)
        {
            _extendTo = operationXdr.ExtendTo.InnerValue;
            _extensionPoint = ExtensionPoint.FromXdr(operationXdr.Ext);
        }

        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }

        public Builder SetExtensionPoint(ExtensionPoint ext)
        {
            _extensionPoint = ext;
            return this;
        }

        public ExtendFootprintOperation Build()
        {
            if (_extensionPoint == null)
                throw new InvalidOperationException("Extension point cannot be null.");
            var operation = new ExtendFootprintOperation(_extendTo, _extensionPoint);
            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}