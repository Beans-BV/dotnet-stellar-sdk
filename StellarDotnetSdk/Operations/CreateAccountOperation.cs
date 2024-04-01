using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Creates and funds a new account with the specified starting balance.
///     Use <see cref="Builder" /> to create a new <c>CreateAccountOperation</c>.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#create-account">
///         Create account
///     </a>
/// </summary>
public class CreateAccountOperation : Operation
{
    private CreateAccountOperation(KeyPair destination, string startingBalance)
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

    /// <summary>
    ///     Builder for <c>CreateAccountOperation</c>.
    /// </summary>
    public class Builder
    {
        private readonly KeyPair _destination;
        private readonly string _startingBalance;
        private KeyPair? _mSourceAccount;

        /// <summary>
        ///     Constructs a new <c>ClawbackClaimableBalanceOperation</c> builder.
        /// </summary>
        /// <param name="createAccountOp">A <c>CreateAccountOp</c> XDR object.</param>
        public Builder(CreateAccountOp createAccountOp)
        {
            _destination = KeyPair.FromXdrPublicKey(createAccountOp.Destination.InnerValue);
            _startingBalance = FromXdrAmount(createAccountOp.StartingBalance.InnerValue);
        }

        /// <summary>
        ///     Constructs a new <c>ClawbackClaimableBalanceOperation</c> builder.
        /// </summary>
        /// <param name="destination">Account address that is created and funded.</param>
        /// <param name="startingBalance">Amount of XLM to send to the newly created account.</param>
        public Builder(KeyPair destination, string startingBalance)
        {
            _destination = destination;
            _startingBalance = startingBalance;
        }

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="account">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair account)
        {
            _mSourceAccount = account;
            return this;
        }

        /// <summary>
        ///     Builds an operation.
        /// </summary>
        public CreateAccountOperation Build()
        {
            var operation = new CreateAccountOperation(_destination, _startingBalance);
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}