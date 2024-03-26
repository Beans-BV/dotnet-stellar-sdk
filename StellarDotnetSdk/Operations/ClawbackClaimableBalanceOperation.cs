using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Claws back an unclaimed ClaimableBalanceEntry, burning the pending amount of the asset.
///     <p>Use <see cref="Builder" /> to create a new <c>ClawbackClaimableBalanceOperation</c>.</p>
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#clawback-claimable-balance">
///         Clawback claimable balance
///     </a>
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

        BalanceIdInBytes = balanceId;
    }

    /// <summary>
    /// Hex-encoded ID of the ClaimableBalanceEntry to be clawed back.
    /// </summary>
    public string BalanceId => Util.BytesToHex(BalanceIdInBytes);
    
    /// <summary>
    /// ID of the ClaimableBalanceEntry to be clawed back as a byte array.
    /// </summary>
    public byte[] BalanceIdInBytes { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE),
            ClawbackClaimableBalanceOp = new ClawbackClaimableBalanceOp
            {
                BalanceID = ClaimableBalanceID.Decode(new XdrDataInputStream(BalanceIdInBytes))
            }
        };
    }

    /// <summary>
    ///     Builder for <c>ClawbackClaimableBalanceOperation</c>.
    /// </summary>
    public class Builder
    {
        private readonly byte[] _balanceId;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Constructs a new <c>ClawbackClaimableBalanceOperation</c> builder.
        /// </summary>
        /// <param name="balanceId">The hex-encoded ID of the ClaimableBalanceEntry to be clawed back.</param>
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
        ///     Builds an operation.
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