using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.LedgerKeys;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Sponsoring account can remove or transfer sponsorships of existing ledgerEntries; the logic of this operation
///     depends on the state of the source account.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#revoke-sponsorship">
///         Revoke sponsorship
///     </a>
/// </summary>
public class RevokeLedgerEntrySponsorshipOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>RevokeSignerSponsorshipOperation</c>.
    /// </summary>
    /// <param name="ledgerKey">
    ///     Ledger key that holds information to identify a specific ledgerEntry that may have its
    ///     sponsorship modified.
    /// </param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public RevokeLedgerEntrySponsorshipOperation(
        LedgerKey ledgerKey,
        IAccountId? sourceAccount = null)
        : base(sourceAccount)
    {
        LedgerKey = ledgerKey;
    }

    /// <summary>
    ///     Ledger key that holds information to identify a specific ledgerEntry that may have its sponsorship modified
    /// </summary>
    public LedgerKey LedgerKey { get; }

    /// <summary>
    ///     Creates a new revoke account entry sponsorship operation.
    /// </summary>
    /// <param name="account">Key pair of an account to be revoked.</param>
    public static RevokeLedgerEntrySponsorshipOperation ForAccount(KeyPair account, IAccountId? sourceAccount = null)
    {
        return new RevokeLedgerEntrySponsorshipOperation(new LedgerKeyAccount(account), sourceAccount);
    }

    /// <summary>
    ///     Constructs new revoke balance entry sponsorship operation.
    /// </summary>
    /// <param name="balanceIdHexString">
    ///     Hex-encoded ID of the claimable balance entry to be revoked.
    ///     For example: <c>d1d73327fc560cc09f54a11c7a64180611e1f480f3bf60117e41d19d9593b780</c>.
    /// </param>
    public static RevokeLedgerEntrySponsorshipOperation ForClaimableBalance(
        string balanceIdHexString,
        IAccountId? sourceAccount = null)
    {
        return new RevokeLedgerEntrySponsorshipOperation(
            new LedgerKeyClaimableBalance(balanceIdHexString),
            sourceAccount);
    }

    /// <summary>
    ///     Creates a new revoke data entry sponsorship operation.
    /// </summary>
    /// <param name="accountId">Id of the account holding the data entry that being sponsored.</param>
    /// <param name="dataName">Name of the data entry.</param>
    public static RevokeLedgerEntrySponsorshipOperation ForData(
        string accountId,
        string dataName,
        IAccountId? sourceAccount = null)
    {
        return new RevokeLedgerEntrySponsorshipOperation(
            new LedgerKeyData(KeyPair.FromAccountId(accountId), dataName),
            sourceAccount);
    }

    /// <summary>
    ///     Creates a new revoke offer entry sponsorship operation.
    /// </summary>
    /// <param name="sellerId">Id of the account that owns the offer that being sponsored.</param>
    /// <param name="offerId">Id of the offer.</param>
    public static RevokeLedgerEntrySponsorshipOperation ForOffer(
        string sellerId,
        long offerId,
        IAccountId? sourceAccount = null)
    {
        return new RevokeLedgerEntrySponsorshipOperation(
            new LedgerKeyOffer(KeyPair.FromAccountId(sellerId), offerId),
            sourceAccount);
    }

    /// <summary>
    ///     Creates a new revoke trustline entry sponsorship operation.
    /// </summary>
    /// <param name="accountId">Id of the account that owns the trustline that being sponsored.</param>
    /// <param name="offerId">Id of the offer.</param>
    public static RevokeLedgerEntrySponsorshipOperation ForTrustline(
        string accountId,
        Asset asset,
        IAccountId? sourceAccount = null)
    {
        return new RevokeLedgerEntrySponsorshipOperation(
            new LedgerKeyTrustline(accountId, asset),
            sourceAccount);
    }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP),
            RevokeSponsorshipOp = new RevokeSponsorshipOp
            {
                Discriminant = RevokeSponsorshipType.Create(RevokeSponsorshipType.RevokeSponsorshipTypeEnum
                    .REVOKE_SPONSORSHIP_LEDGER_ENTRY),
                LedgerKey = LedgerKey.ToXdr(),
            },
        };
    }
}