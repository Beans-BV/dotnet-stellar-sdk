using System;
using System.Text;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Sets, modifies, or deletes a data entry (name/value pair) that is attached to an account.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#manage-data">Manage data</a>
/// </summary>
public class ManageDataOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>ManageDataOperation</c>.
    /// </summary>
    /// <param name="name">
    ///     The name of data entry - string up to 64 bytes long. If this is a new Name it will add the given
    ///     name/value pair to the account. If this Name is already present then the associated value will be modified.
    /// </param>
    /// <param name="value">
    ///     (optional) If not present then the existing Name will be deleted. If present then this value will
    ///     be set in the DataEntry. Up to 64 bytes long.
    /// </param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ManageDataOperation(string name, byte[]? value, IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name), "name cannot be null");
        }
        if (name.Length > 64)
        {
            throw new ArgumentException("Data name cannot exceed 64 characters.", nameof(name));
        }
        Name = name;
        Value = value;
    }

    public ManageDataOperation(string name, string? value, IAccountId? sourceAccount = null)
        : this(name, value != null ? Encoding.UTF8.GetBytes(value) : null, sourceAccount)
    {
    }

    /// <summary>
    ///     Name of data entry.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Value of data entry.
    /// </summary>
    public byte[]? Value { get; }

    public override xdr_Operation.OperationBody ToOperationBody()
    {
        var body = new xdr_Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.MANAGE_DATA),
            ManageDataOp = new ManageDataOp
            {
                DataName = new String64(Name),
                DataValue = Value != null ? new DataValue(Value) : null,
            },
        };
        return body;
    }

    public static ManageDataOperation FromXdr(ManageDataOp manageDataOp)
    {
        return new ManageDataOperation(manageDataOp.DataName.InnerValue, manageDataOp.DataValue?.InnerValue);
    }
}