using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Creates and funds a new account with the specified starting balance.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#create-account">
///         Create account
///     </a>
/// </summary>
public class CreateAccountOperation : Operation
{
    /// <summary>
    ///     Constructs a <see cref="CreateAccountOperation" />.
    /// </summary>
    /// <param name="destination">Account address that is created and funded.</param>
    /// <param name="startingBalance">
    ///     Amount of XLM to send to the newly created account. This XLM comes from the source
    ///     account.
    /// </param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public CreateAccountOperation(KeyPair destination, string startingBalance, IAccountId? sourceAccount = null) :
        base(sourceAccount)
    {
        Destination = destination ?? throw new ArgumentNullException(nameof(destination), "destination cannot be null");
        StartingBalance = startingBalance ??
                          throw new ArgumentNullException(nameof(startingBalance), "startingBalance cannot be null");
    }

    /// <summary>
    ///     Account address that is created and funded.
    /// </summary>
    public KeyPair Destination { get; }

    /// <summary>
    ///     Amount of XLM to send to the newly created account. This XLM comes from the source account.
    /// </summary>
    public string StartingBalance { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CREATE_ACCOUNT),
            CreateAccountOp = new CreateAccountOp
            {
                Destination = new AccountID(Destination.XdrPublicKey),
                StartingBalance = new Int64(ToXdrAmount(StartingBalance))
            }
        };
    }

    public static CreateAccountOperation FromXdr(CreateAccountOp createAccountOp)
    {
        return new CreateAccountOperation(
            KeyPair.FromXdrPublicKey(createAccountOp.Destination.InnerValue),
            FromXdrAmount(createAccountOp.StartingBalance.InnerValue)
        );
    }
}