using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
/// Operation that restores a footprint
/// </summary>
public class RestoreFootprintOperation : Operation
{
    public ExtensionPoint Ext { get; set; }

    /// <summary>
    /// Creates a new RestoreFootprintOperation object from the given base64-encoded XDR Operation.
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

    public static RestoreFootprintOperation FromRestoreFootprintOperationXdr(RestoreFootprintOp xdrRestoreFootprintOp)
    {
        return new RestoreFootprintOperation
        {
            Ext = ExtensionPoint.FromXdr(xdrRestoreFootprintOp.Ext),
        };
    }

    public RestoreFootprintOp ToRestoreFootprintOperationXdr()
    {
        return new RestoreFootprintOp
        {
            Ext = Ext.ToXdr()
        };
    }
    
    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant = new xdr.OperationType
            {
                InnerValue = xdr.OperationType.OperationTypeEnum.RESTORE_FOOTPRINT
            },
            RestoreFootprintOp = ToRestoreFootprintOperationXdr()
        };
        return body;
    }

    public class Builder
    {
        private ExtensionPoint _ext;
      
        private KeyPair? _sourceAccount;

        public Builder()
        {
        }

        public Builder(RestoreFootprintOp operationXdr)
        {
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
     
        public RestoreFootprintOperation Build()
        {
            if (_ext == null)
                throw new InvalidOperationException("Ext cannot be null");
            var operation = new RestoreFootprintOperation()
            {
                Ext = _ext
            };
            if (_sourceAccount != null)
            {
                operation.SourceAccount = _sourceAccount;
            }
            return operation;
        }
    }
}
