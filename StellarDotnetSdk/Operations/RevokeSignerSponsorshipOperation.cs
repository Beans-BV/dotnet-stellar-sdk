using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Sponsoring account can remove or transfer sponsorships of existing signers; the logic of this operation depends on
///     the state of the source account.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#revoke-sponsorship">
///         Revoke sponsorship
///     </a>
/// </summary>
public class RevokeSignerSponsorshipOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>RevokeSignerSponsorshipOperation</c>.
    /// </summary>
    /// <param name="account">Account of a signer that may have its sponsorship modified.</param>
    /// <param name="accountId">The signer's Ed25519 public key.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public RevokeSignerSponsorshipOperation(KeyPair account, string accountId, IAccountId? sourceAccount = null)
        : this(account, SignerUtil.Ed25519PublicKey(KeyPair.FromAccountId(accountId)), sourceAccount)
    {
    }

    /// <summary>
    ///     Constructs a new <c>RevokeSignerSponsorshipOperation</c>.
    /// </summary>
    /// <param name="account">Account of a signer that may have its sponsorship modified.</param>
    /// <param name="signerKey">Signer key of a signer that may have its sponsorship modified.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public RevokeSignerSponsorshipOperation(KeyPair account, SignerKey signerKey, IAccountId? sourceAccount = null)
        : base(sourceAccount)
    {
        Account = account ?? throw new ArgumentNullException(nameof(account));
        SignerKey = signerKey ?? throw new ArgumentNullException(nameof(signerKey));
    }

    /// <summary>
    ///     Account of a signer that may have its sponsorship modified.
    /// </summary>
    public KeyPair Account { get; }

    /// <summary>
    ///     Signer key of a signer that may have its sponsorship modified.
    /// </summary>
    public SignerKey SignerKey { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP),
            RevokeSponsorshipOp = new RevokeSponsorshipOp
            {
                Discriminant = RevokeSponsorshipType.Create(RevokeSponsorshipType.RevokeSponsorshipTypeEnum
                    .REVOKE_SPONSORSHIP_SIGNER),
                Signer = new RevokeSponsorshipOp.RevokeSponsorshipOpSigner
                {
                    AccountID = new AccountID(Account.XdrPublicKey),
                    SignerKey = SignerKey,
                },
            },
        };
    }

    public static RevokeSignerSponsorshipOperation FromXdr(
        RevokeSponsorshipOp.RevokeSponsorshipOpSigner revokeSignerSponsorshipOp)
    {
        return new RevokeSignerSponsorshipOperation(
            KeyPair.FromXdrPublicKey(revokeSignerSponsorshipOp.AccountID.InnerValue),
            revokeSignerSponsorshipOp.SignerKey);
    }
}