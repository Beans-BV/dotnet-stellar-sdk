using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
/// Operation that extends footprint TTL
/// </summary>
public class ExtendFootprintOperation : Operation
{
    public SCUint32 ExtendTo { get; set; }
    
    public ExtensionPoint Ext { get; set; }

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
            ExtendTo = SCUint32.FromXdr(xdrExtendFootprintTTLOp.ExtendTo),
            Ext = ExtensionPoint.FromXdr(xdrExtendFootprintTTLOp.Ext)
        };
    }

    public ExtendFootprintTTLOp ToExtendFootprintTTLOperationXdr()
    {
        return new ExtendFootprintTTLOp
        {
            Ext = Ext.ToXdr(),
            ExtendTo = ExtendTo.ToXdr()
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
        private SCUint32 _extendTo;
        private ExtensionPoint _ext;
      
        private KeyPair? _sourceAccount;

        public Builder()
        {
        }

        public Builder(ExtendFootprintTTLOp operationXdr)
        {
            _extendTo = SCUint32.FromXdr(operationXdr.ExtendTo);
            _ext = ExtensionPoint.FromXdr(operationXdr.Ext);
        }

        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }
    
        public Builder SetExt(ExtensionPoint ext)
        {
            _ext = ext;
            return this;
        }
        
        public Builder SetExtendTo(SCUint32 extendTo)
        {
            _extendTo = extendTo;
            return this;
        }
        
        public ExtendFootprintOperation Build()
        {
            if (_ext == null)
                throw new InvalidOperationException("Ext cannot be null");
            if (_extendTo == null)
                throw new InvalidOperationException("Extend to cannot be null");
            var operation = new ExtendFootprintOperation()
            {
                Ext = _ext,
                ExtendTo = _extendTo
            };
            if (_sourceAccount != null)
            {
                operation.SourceAccount = _sourceAccount;
            }
            return operation;
        }
    }
}
