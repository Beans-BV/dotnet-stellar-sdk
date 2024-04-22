using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="ClaimClaimableBalanceOperation" />.
///     Use <see cref="Builder" /> to create a new ClaimClaimableBalanceOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#claim-claimable-balance">
///         Claim
///         claimable balance
///     </a>
/// </summary>
public class ClaimClaimableBalanceOperation : Operation
{
    private ClaimClaimableBalanceOperation(byte[] balanceId)
    {
        // Backwards compat - was previously expecting no type to be set, set CLAIMABLE_BALANCE_ID_TYPE_V0 to match previous behaviour.
        if (balanceId.Length == 32)
        {
            var expanded = new byte[36];
            Array.Copy(balanceId, 0, expanded, 4, 32);
            balanceId = expanded;
        }

        if (balanceId.Length != 36) throw new ArgumentException("Must be 36 bytes long", nameof(balanceId));

        BalanceId = balanceId;
    }

    /// <summary>
    ///     The ID of the ClaimableBalanceEntry being claimed.
    /// </summary>
    public byte[] BalanceId { get; }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE),
            ClaimClaimableBalanceOp = new ClaimClaimableBalanceOp
            {
                BalanceID = ClaimableBalanceID.Decode(new XdrDataInputStream(BalanceId))
            }
        };
        return body;
    }

    /// <summary>
    ///     Builds ClaimClaimableBalanceOperation operation.
    /// </summary>
    /// <see cref="ClaimClaimableBalanceOperation" />
    public class Builder
    {
        private readonly byte[] _balanceId;
        private KeyPair? _sourceAccount;

        public Builder(ClaimClaimableBalanceOp op)
        {
            _balanceId = op.BalanceID.V0.InnerValue;
        }

        /// <summary>
        ///     Constructs a new ClaimClaimableBalanceOperation builder.
        /// </summary>
        /// <param name="balanceId">The hex-encoded ID of the ClaimableBalanceEntry being claimed.</param>
        public Builder(string balanceId)
        {
            _balanceId = Util.HexToBytes(balanceId);
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
        public ClaimClaimableBalanceOperation Build()
        {
            var operation = new ClaimClaimableBalanceOperation(_balanceId);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}