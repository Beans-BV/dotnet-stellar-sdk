using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
/// Operation that restores a footprint
/// </summary>
public class RestoreFootprintOperation : Operation
{
    public ExtensionPoint ExtensionPoint { get; set; }

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
            ExtensionPoint = ExtensionPoint.FromXdr(xdrRestoreFootprintOp.Ext),
        };
    }

    public RestoreFootprintOp ToRestoreFootprintOperationXdr()
    {
        return new RestoreFootprintOp
        {
            Ext = ExtensionPoint.ToXdr()
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
        private ExtensionPoint _extensionPoint = new ExtensionPointZero(); 
      
        private KeyPair? _sourceAccount;

        public Builder()
        {
        }

        public Builder(RestoreFootprintOp operationXdr)
        {
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
     
        public RestoreFootprintOperation Build()
        {
            if (_extensionPoint == null)
                throw new InvalidOperationException("Extension point cannot be null");
            var operation = new RestoreFootprintOperation()
            {
                ExtensionPoint = _extensionPoint
            };
            if (_sourceAccount != null)
            {
                operation.SourceAccount = _sourceAccount;
            }
            return operation;
        }
    }
}
