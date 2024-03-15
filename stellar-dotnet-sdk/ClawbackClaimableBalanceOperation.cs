using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="ClawbackClaimableBalanceOperation" />.
///     Use <see cref="Builder" /> to create a new ClawbackClaimableBalanceOperation.
///     See also:
///     <see href="https://www.stellar.org/developers/guides/concepts/list-of-operations.html">Clawback Claimable Balance</see>
/// </summary>
public class ClawbackClaimableBalanceOperation : Operation
{
    private ClawbackClaimableBalanceOperation(byte[] balanceId)
    {
        // Backwards compatibility - was previously expecting no type to be set.
        if (balanceId.Length == 32)
        {
            var expanded = new byte[36];
            Array.Copy(balanceId, 0, expanded, 4, 32);
            balanceId = expanded;
        }

        if (balanceId.Length != 36) throw new ArgumentException("Must be 36 bytes long", nameof(balanceId));

        BalanceId = balanceId;
    }

    public byte[] BalanceId { get; }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        return new xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE),
            ClawbackClaimableBalanceOp = new ClawbackClaimableBalanceOp
            {
                BalanceID = ClaimableBalanceID.Decode(new XdrDataInputStream(BalanceId))
            }
        };
    }

    /// <summary>
    ///     Builds ClawbackClaimableBalanceOperation operation.
    /// </summary>
    /// <see cref="ClawbackClaimableBalanceOperation" />
    public class Builder
    {
        private readonly byte[] _balanceId;

        private KeyPair? _sourceAccount;

        public Builder(ClawbackClaimableBalanceOp op)
        {
            _balanceId = op.BalanceID.V0.InnerValue;
        }

        public Builder(byte[] balanceId)
        {
            _balanceId = balanceId;
        }

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
        public ClawbackClaimableBalanceOperation Build()
        {
            var operation = new ClawbackClaimableBalanceOperation(_balanceId);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}