using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Xdr;
using Assets_Asset = StellarDotnetSdk.Assets.Asset;
using LedgerKey = StellarDotnetSdk.Xdr.LedgerKey;
using ledgerkeys_LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Represents a <c>RevokeSponsorshipOperation</c>.
///     Use <see cref="Builder" /> to create a new RevokeLedgerEntrySponsorshipOperation.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#revoke-sponsorship">
///         Revoke
///         sponsorship
///     </a>
/// </summary>
public class RevokeLedgerEntrySponsorshipOperation : Operation
{
    private RevokeLedgerEntrySponsorshipOperation(ledgerkeys_LedgerKey ledgerKey)
    {
        LedgerKey = ledgerKey;
    }

    public ledgerkeys_LedgerKey LedgerKey { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP),
            RevokeSponsorshipOp = new RevokeSponsorshipOp
            {
                Discriminant = RevokeSponsorshipType.Create(RevokeSponsorshipType.RevokeSponsorshipTypeEnum
                    .REVOKE_SPONSORSHIP_LEDGER_ENTRY),
                LedgerKey = LedgerKey.ToXdr()
            }
        };
        return body;
    }

    /// <summary>
    ///     Builds RevokeLedgerEntrySponsorshipOperation operation.
    /// </summary>
    /// <see cref="RevokeLedgerEntrySponsorshipOperation" />
    public class Builder
    {
        private readonly ledgerkeys_LedgerKey _ledgerKey;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Constructs a new RevokeLedgerEntrySponsorshipOperation builder from a LedgerKey XDR.
        /// </summary>
        /// <param name="ledgerKey"></param>
        public Builder(LedgerKey ledgerKey)
        {
            _ledgerKey = ledgerkeys_LedgerKey.FromXdr(ledgerKey);
        }

        /// <summary>
        ///     Constructs a new revoke account entry sponsorship operation.
        /// </summary>
        /// <param name="account">Key pair of an account to be revoked.</param>
        public Builder(KeyPair account) : this(new LedgerKeyAccount(account))
        {
        }

        /// <summary>
        ///     Constructs a new revoke balance entry sponsorship operation.
        /// </summary>
        /// <param name="account">Id of the balance entry to be revoked.</param>
        public Builder(string balanceId) : this(new LedgerKeyClaimableBalance(balanceId))
        {
        }

        /// <summary>
        ///     Constructs a new revoke data entry sponsorship operation.
        /// </summary>
        /// <param name="accountId">Id of the account holding the data entry that being sponsored.</param>
        /// <param name="dataName">Name of the data entry.</param>
        public Builder(string accountId, string dataName) : this(new LedgerKeyData(KeyPair.FromAccountId(accountId),
            dataName))
        {
        }

        /// <summary>
        ///     Constructs a new revoke offer entry sponsorship operation.
        /// </summary>
        /// <param name="sellerId">Id of the account that owns the offer that being sponsored.</param>
        /// <param name="offerId">Id of the offer.</param>
        public Builder(string sellerId, long offerId) : this(new LedgerKeyOffer(KeyPair.FromAccountId(sellerId),
            offerId))
        {
        }

        /// <summary>
        ///     Constructs a new revoke ledger entry trustline asset sponsorship operation.
        /// </summary>
        /// <param name="accountId">Id of the trustline owner that being sponsored.</param>
        /// <param name="asset">The asset of the trustline.</param>
        public Builder(string accountId, Assets_Asset asset) : this(new LedgerKeyTrustline(
            KeyPair.FromAccountId(accountId),
            asset))
        {
        }

        /// <summary>
        ///     Creates a new <c>RevokeLedgerEntrySponsorshipOperation</c> builder from the provided <c>LedgerKey</c>.
        /// </summary>
        /// <param name="ledgerKey">key of the ledger entry to be revoked.</param>
        public Builder(ledgerkeys_LedgerKey ledgerKey)
        {
            _ledgerKey = ledgerKey ?? throw new ArgumentNullException(nameof(ledgerKey));
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
        public RevokeLedgerEntrySponsorshipOperation Build()
        {
            var operation = new RevokeLedgerEntrySponsorshipOperation(_ledgerKey);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}