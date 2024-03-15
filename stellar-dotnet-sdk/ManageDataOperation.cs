using System;
using System.Text;
using stellar_dotnet_sdk.xdr;
using sdkxdr = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="ManageDataOp" />.
///     Use <see cref="Builder" /> to create a new ManageDataOperation.
///     See also:
///     <see href="https://www.stellar.org/developers/guides/concepts/list-of-operations.html#manage-data">Manage Data</see>
/// </summary>
public class ManageDataOperation : Operation
{
    private ManageDataOperation(string name, byte[]? value)
    {
        if (name == null) throw new ArgumentNullException(nameof(name), "name cannot be null");
        if (name.Length > 64) throw new ArgumentException("Data name cannot exceed 64 characters.", nameof(name));
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public byte[]? Value { get; }

    public override sdkxdr.Operation.OperationBody ToOperationBody()
    {
        var op = new ManageDataOp
        {
            DataName = new String64(Name),
            DataValue = Value != null ? new DataValue(Value) : null
        };

        var body = new sdkxdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.MANAGE_DATA),
            ManageDataOp = op
        };

        return body;
    }

    public class Builder
    {
        private readonly string _name;
        private readonly byte[]? _value;

        private IAccountId? _mSourceAccount;

        /// <summary>
        ///     Construct a new ManageOffer builder from a ManageDataOp XDR.
        /// </summary>
        /// <param name="op">
        ///     <see cref="sdkxdr.ManageDataOp" />
        /// </param>
        public Builder(ManageDataOp op)
        {
            _name = op.DataName.InnerValue;
            _value = op.DataValue?.InnerValue;
        }

        public Builder(string name, string? value)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name), "name cannot be null");
            _value = value != null ? Encoding.UTF8.GetBytes(value) : null;
        }

        /// <summary>
        ///     Creates a new ManageData builder. If you want to delete data entry pass null as a <code>value</code> param.
        /// </summary>
        /// <param name="name">The name of data entry</param>
        /// <param name="value">The value of data entry. <code>null</code>null will delete data entry.</param>
        public Builder(string name, byte[]? value)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name), "name cannot be null");
            _value = value;
        }

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="sourceAccount">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(IAccountId sourceAccount)
        {
            _mSourceAccount = sourceAccount ??
                              throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null");
            return this;
        }

        /// <summary>
        ///     Builds an operation
        /// </summary>
        public ManageDataOperation Build()
        {
            var operation = new ManageDataOperation(_name, _value);
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}