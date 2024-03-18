using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <c>RevokeSponsorshipOperation</c>.
///     Use <see cref="Builder" /> to create a new RevokeSignerSponsorshipOperation.
///     See also:
///     <see href="https://www.stellar.org/developers/guides/concepts/list-of-operations.html">Revoke Sponsorship</see>
/// </summary>
public class RevokeSignerSponsorshipOperation : Operation
{
    private RevokeSignerSponsorshipOperation(KeyPair accountId, SignerKey signerKey)
    {
        AccountId = accountId;
        SignerKey = signerKey;
    }

    public KeyPair AccountId { get; }
    public SignerKey SignerKey { get; }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP),
            RevokeSponsorshipOp = new RevokeSponsorshipOp
            {
                Discriminant = RevokeSponsorshipType.Create(RevokeSponsorshipType.RevokeSponsorshipTypeEnum
                    .REVOKE_SPONSORSHIP_SIGNER),
                Signer = new RevokeSponsorshipOp.RevokeSponsorshipOpSigner
                {
                    AccountID = new AccountID(AccountId.XdrPublicKey),
                    SignerKey = SignerKey
                }
            }
        };
        return body;
    }

    /// <summary>
    ///     Builds BeginSponsoringFutureReserves operation.
    /// </summary>
    /// <see cref="BeginSponsoringFutureReservesOperation" />
    public class Builder
    {
        private readonly KeyPair _accountId;
        private readonly SignerKey _signerKey;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Construct a new BeginSponsoringFutureReserves builder from a BeginSponsoringFutureReservesOp XDR.
        /// </summary>
        /// <param name="op"></param>
        public Builder(RevokeSponsorshipOp.RevokeSponsorshipOpSigner op)
        {
            _accountId = KeyPair.FromXdrPublicKey(op.AccountID.InnerValue);
            _signerKey = op.SignerKey;
        }

        /// <summary>
        ///     Create a new  builder with the given sponsoredId.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="signerKey"></param>
        public Builder(KeyPair accountId, SignerKey signerKey)
        {
            _accountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
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
            var operation = new RevokeSignerSponsorshipOperation(_accountId, _signerKey);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}