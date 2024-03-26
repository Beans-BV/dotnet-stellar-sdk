using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Represents a <c>RevokeSponsorshipOperation</c>.
///     Use <see cref="Builder" /> to create a new RevokeSignerSponsorshipOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#revoke-sponsorship">
///         Revoke
///         Sponsorship
///     </a>
/// </summary>
public class RevokeSignerSponsorshipOperation : Operation
{
    private RevokeSignerSponsorshipOperation(KeyPair account, SignerKey signerKey)
    {
        Account = account;
        SignerKey = signerKey;
    }

    public KeyPair Account { get; }
    public SignerKey SignerKey { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP),
            RevokeSponsorshipOp = new RevokeSponsorshipOp
            {
                Discriminant = RevokeSponsorshipType.Create(RevokeSponsorshipType.RevokeSponsorshipTypeEnum
                    .REVOKE_SPONSORSHIP_SIGNER),
                Signer = new RevokeSponsorshipOp.RevokeSponsorshipOpSigner
                {
                    AccountID = new AccountID(Account.XdrPublicKey),
                    SignerKey = SignerKey
                }
            }
        };
        return body;
    }

    /// <summary>
    ///     Builder for <c>RevokeSignerSponsorshipOperation</c> operation.
    /// </summary>
    /// <see cref="BeginSponsoringFutureReservesOperation" />
    public class Builder
    {
        private readonly KeyPair _account;
        private readonly SignerKey _signerKey;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Constructs a new <c>RevokeSignerSponsorshipOperation</c> builder from a RevokeSponsorshipOpSigner XDR.
        /// </summary>
        public Builder(RevokeSponsorshipOp.RevokeSponsorshipOpSigner op)
        {
            _account = KeyPair.FromXdrPublicKey(op.AccountID.InnerValue);
            _signerKey = op.SignerKey;
        }

        /// <summary>
        ///     Constructs a new <c>RevokeSignerSponsorshipOperation</c> builder with the given account and signer key.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="signerKey"></param>
        public Builder(KeyPair account, SignerKey signerKey)
        {
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _signerKey = signerKey ?? throw new ArgumentNullException(nameof(signerKey));
        }

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="account">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair account)
        {
            _sourceAccount = account;
            return this;
        }

        /// <summary>
        ///     Builds an operation
        /// </summary>
        public RevokeSignerSponsorshipOperation Build()
        {
            var operation = new RevokeSignerSponsorshipOperation(_account, _signerKey);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}