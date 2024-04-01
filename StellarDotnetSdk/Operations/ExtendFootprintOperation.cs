using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Extend the time to live (TTL) of entries for Soroban smart contracts.
///     <p>Use <see cref="Builder" /> to to create a new <c>ExtendFootprintOperation</c>.</p>
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#extend-footprint-ttl">
///         Extend footprint TTL
///     </a>
/// </summary>
/// <remarks>Note that Soroban transactions can only contain one operation per transaction.</remarks>
public class ExtendFootprintOperation : Operation
{
    private ExtendFootprintOperation(uint extendTo, ExtensionPoint? extensionPoint = null)
    {
        ExtendTo = extendTo;
        ExtensionPoint = extensionPoint ?? new ExtensionPointZero();
    }

    /// <summary>
    ///     The ledger sequence number the entries will live until.
    /// </summary>
    public uint ExtendTo { get; }

    /// <summary>
    ///     Reserved for later use.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL),
            ExtendFootprintTTLOp = new ExtendFootprintTTLOp
            {
                Ext = ExtensionPoint.ToXdr(),
                ExtendTo = new Uint32(ExtendTo)
            }
        };
    }

    /// <summary>
    ///     Builder for <c>ExtendFootprintOperation</c>.
    /// </summary>
    public class Builder
    {
        private readonly uint _extendTo;
        private ExtensionPoint? _extensionPoint;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Construct a new <c>ExtendFootprintOperation</c> builder.
        /// </summary>
        /// <param name="extendTo">The ledger sequence number the entries will live until.</param>
        /// <param name="extensionPoint">Optional. Reserved for later use. Defaults to <see cref="ExtensionPointZero" />.</param>
        public Builder(uint extendTo, ExtensionPoint? extensionPoint = null)
        {
            _extendTo = extendTo;
            _extensionPoint = extensionPoint;
        }

        /// <summary>
        ///     Constructs a new <c>ClawbackClaimableBalanceOperation</c> builder.
        /// </summary>
        /// <param name="extendFootprintTTLOp">A <c>ExtendFootprintTTLOp</c> XDR object.</param>
        public Builder(ExtendFootprintTTLOp extendFootprintTTLOp)
        {
            _extendTo = extendFootprintTTLOp.ExtendTo.InnerValue;
            _extensionPoint = ExtensionPoint.FromXdr(extendFootprintTTLOp.Ext);
        }

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="account">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }

        public Builder SetExtensionPoint(ExtensionPoint ext)
        {
            _extensionPoint = ext;
            return this;
        }

        /// <summary>
        ///     Builds an operation.
        /// </summary>
        public ExtendFootprintOperation Build()
        {
            var operation = new ExtendFootprintOperation(_extendTo, _extensionPoint);
            if (_sourceAccount != null) operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}