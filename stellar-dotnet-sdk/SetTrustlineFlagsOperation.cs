using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="SetTrustlineFlagsOperation" />.
///     Use <see cref="Builder" /> to create a new SetTrustlineFlagsOperation.
///     <seealso
///         href="https://developers.stellar.org/docs/fundamentals-and-concepts/list-of-operations#set-trustline-flags">
///         Set
///         Trustline Flags
///     </seealso>
/// </summary>
public class SetTrustlineFlagsOperation : Operation
{
    private SetTrustlineFlagsOperation(Asset asset, KeyPair trustor, uint setFlags, uint clearFlags)
    {
        Asset = asset;
        Trustor = trustor;
        SetFlags = setFlags;
        ClearFlags = clearFlags;
    }

    /// <summary>
    ///     The asset trustline whose flags are being modified.
    /// </summary>
    public Asset Asset { get; }

    /// <summary>
    ///     The account that established this trustline.
    /// </summary>
    public KeyPair Trustor { get; }


    /// <summary>
    ///     One or more flags (combined via bitwise-OR) indicating which flags to set.
    ///     Possible flags are: 1 if the trustor is authorized to transact with the asset or 2 if the trustor is authorized
    ///     to maintain offers but not to perform other transactions.
    /// </summary>
    public uint SetFlags { get; }

    /// <summary>
    ///     One or more flags (combined via bitwise OR) indicating which flags to clear. Possibilities include those for
    ///     SetFlags as well as 4, which prevents the issuer from clawing back its asset (both from accounts and claimable
    ///     balances).
    /// </summary>
    public uint ClearFlags { get; }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        return new xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS),
            SetTrustLineFlagsOp = new SetTrustLineFlagsOp
            {
                Asset = Asset.ToXdr(),
                Trustor = new AccountID(Trustor.XdrPublicKey),
                SetFlags = new Uint32 { InnerValue = SetFlags },
                ClearFlags = new Uint32 { InnerValue = ClearFlags }
            }
        };
    }

    /// <summary>
    ///     Builds SetTrustlineFlagsOperation operation.
    /// </summary>
    /// <see cref="SetTrustlineFlagsOperation" />
    public class Builder
    {
        private readonly Asset _asset;
        private readonly uint _clearFlags;
        private readonly uint _setFlags;
        private readonly KeyPair _trustor;

        private KeyPair? _sourceAccount;

        public Builder(SetTrustLineFlagsOp op)
        {
            _asset = Asset.FromXdr(op.Asset);
            _trustor = KeyPair.FromXdrPublicKey(op.Trustor.InnerValue);
            _setFlags = op.SetFlags.InnerValue;
            _clearFlags = op.ClearFlags.InnerValue;
        }

        public Builder(Asset asset, KeyPair trustor, uint setFlags, uint clearFlags)
        {
            _asset = asset;
            _trustor = trustor;
            _setFlags = setFlags;
            _clearFlags = clearFlags;
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
        public SetTrustlineFlagsOperation Build()
        {
            var operation = new SetTrustlineFlagsOperation(_asset, _trustor, _setFlags, _clearFlags);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}