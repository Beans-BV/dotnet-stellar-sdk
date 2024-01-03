using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
/// Operation that extends footprint TTL
/// </summary>
public class ExtendFootprintOperation : Operation
{
    public uint ExtendTo { get; set; }
    
    public ExtensionPoint ExtensionPoint { get; set; }

    /// <summary>
    /// Creates a new ExtendFootprintOperation object from the given base64-encoded XDR Operation.
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

    public static ExtendFootprintOperation FromExtendFootprintTTLOperationXdr(ExtendFootprintTTLOp xdrExtendFootprintTTLOp)
    {
        return new ExtendFootprintOperation
        {
            ExtendTo = xdrExtendFootprintTTLOp.ExtendTo.InnerValue,
            ExtensionPoint = ExtensionPoint.FromXdr(xdrExtendFootprintTTLOp.Ext)
        };
    }

    public ExtendFootprintTTLOp ToExtendFootprintTTLOperationXdr()
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
            Discriminant = new xdr.OperationType
            {
                InnerValue = xdr.OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL
            },
            ExtendFootprintTTLOp = ToExtendFootprintTTLOperationXdr()
        };
        return body;
    }

    public class Builder
    {
        private uint? _extendTo;
        private ExtensionPoint _extensionPoint;
      
        private KeyPair? _sourceAccount;

        public Builder()
        {
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
        
        public Builder SetExtendTo(uint extendTo)
        {
            _extendTo = extendTo;
            return this;
        }
        
        public ExtendFootprintOperation Build()
        {
            if (_extensionPoint == null)
                throw new InvalidOperationException("Extension point cannot be null");
            if (_extendTo == null)
                throw new InvalidOperationException("Extend to cannot be null");
            var operation = new ExtendFootprintOperation()
            {
                ExtensionPoint = _extensionPoint,
                ExtendTo = _extendTo.Value
            };
            if (_sourceAccount != null)
            {
                operation.SourceAccount = _sourceAccount;
            }
            return operation;
        }
    }
}
