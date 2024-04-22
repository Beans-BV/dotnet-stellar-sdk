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
        ExtensionPoint = extensionPoint ?? new ExtensionPointZero();
    }

    public uint ExtendTo { get; }

    public ExtensionPoint ExtensionPoint { get; }

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
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL),
            ExtendFootprintTTLOp = ToExtendFootprintTTLOperationXdr()
        };
        return body;
    }

    public class Builder
    {
        private readonly uint _extendTo;
        private ExtensionPoint? _extensionPoint;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Construct a new <see cref="ExtendFootprintOperation" /> builder.
        /// </summary>
        /// <param name="extendTo"></param>
        /// <param name="extensionPoint">Defaults to <see cref="ExtensionPointZero" />.</param>
        public Builder(uint extendTo, ExtensionPoint? extensionPoint = null)
        {
            _extendTo = extendTo;
            _extensionPoint = extensionPoint;
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
            var operation = new ExtendFootprintOperation(_extendTo, _extensionPoint);
            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}