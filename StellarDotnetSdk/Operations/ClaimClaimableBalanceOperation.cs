using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Operations;

/// <summary>
///         Claims a ClaimableBalanceEntry that corresponds to the BalanceID and adds the amount of an asset on the entry to
///     the source account.
///     <p>Use <see cref="Builder" /> to create a new <c>ClaimClaimableBalanceOperation</c>.</p>
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#claim-claimable-balance">
///         Claim claimable balance
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

        BalanceIdInBytes = balanceId;
    }

    /// <summary>
    /// Hex-encoded ID of the ClaimableBalanceEntry to be claimed.
    /// </summary>
    public string BalanceId => Util.BytesToHex(BalanceIdInBytes);
    
    /// <summary>
    /// ID of the ClaimableBalanceEntry to be claimed as a byte array.
    /// </summary>
    public byte[] BalanceIdInBytes { get; }
    
    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE),
            ClaimClaimableBalanceOp = new ClaimClaimableBalanceOp
            {
                BalanceID = ClaimableBalanceID.Decode(new XdrDataInputStream(BalanceIdInBytes))
            }
        };
    }

    /// <summary>
    ///     Builder for <c>ClaimClaimableBalanceOperation</c>.
    /// </summary>
    public class Builder
    {
        private readonly byte[] _balanceId;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Constructs a new <c>ClaimClaimableBalanceOperation</c> builder.
        /// </summary>
        /// <param name="balanceId">The hex-encoded ID of the ClaimableBalanceEntry to be claimed.</param>
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
        public ClaimClaimableBalanceOperation Build()
        {
            var operation = new ClaimClaimableBalanceOperation(_balanceId);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}