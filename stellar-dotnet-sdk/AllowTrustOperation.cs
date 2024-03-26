using System;
using System.Text;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="AllowTrustOp" />.
///     Use <see cref="Builder" /> to create a new AllowTrustOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#allow-trust">Allow Trust</a>
/// </summary>
[Obsolete("Deprecated in favor of 'SetTrustlineFlagsOperation'")]
public class AllowTrustOperation : Operation
{
    private AllowTrustOperation(KeyPair trustor, string assetCode, bool authorize, bool authorizeToMaintainLiabilities)
    {
        Trustor = trustor ?? throw new ArgumentNullException(nameof(trustor), "trustor cannot be null");
        AssetCode = assetCode ?? throw new ArgumentNullException(nameof(assetCode), "assetCode cannot be null");
        Authorize = authorize;
        AuthorizeToMaintainLiabilities = authorizeToMaintainLiabilities;
    }

    /// <summary>
    ///     The asset code being authorized.
    /// </summary>
    public string AssetCode { get; }

    /// <summary>
    ///     The trusting account (the one being authorized)
    /// </summary>
    public KeyPair Trustor { get; }

    /// <summary>
    ///     True to authorize the line, false to de-authorize.
    /// </summary>
    public bool Authorize { get; }

    public bool AuthorizeToMaintainLiabilities { get; }

    public override OperationThreshold Threshold => OperationThreshold.LOW;

    /// <summary>
    ///     Returns the Allow Trust XDR Operation Body
    /// </summary>
    /// <returns></returns>
    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var op = new AllowTrustOp
        {
            // trustor
            Trustor = new AccountID(Trustor.XdrPublicKey)
        };

        // asset
        var asset = new AssetCode();
        if (AssetCode.Length <= 4)
        {
            asset.Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4);
            asset.AssetCode4 = new AssetCode4(Util.PaddedByteArray(AssetCode, 4));
        }
        else
        {
            asset.Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12);
            asset.AssetCode12 = new AssetCode12(Util.PaddedByteArray(AssetCode, 12));
        }

        op.Asset = asset;

        // authorize
        var trustlineFlag = new Uint32();

        if (Authorize)
            trustlineFlag.InnerValue = (uint)TrustLineFlags.TrustLineFlagsEnum.AUTHORIZED_FLAG;
        else if (AuthorizeToMaintainLiabilities)
            trustlineFlag.InnerValue = (uint)TrustLineFlags.TrustLineFlagsEnum.AUTHORIZED_TO_MAINTAIN_LIABILITIES_FLAG;
        else
            trustlineFlag.InnerValue = 0;

        op.Authorize = trustlineFlag;

        var body = new xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.ALLOW_TRUST),
            AllowTrustOp = op
        };
        return body;
    }

    /// <summary>
    ///     Builds AllowTrust operation.
    /// </summary>
    /// <see cref="AllowTrustOperation" />
    public class Builder
    {
        private readonly string _assetCode;
        private readonly bool _authorize;
        private readonly bool _authorizeToMaintainLiabilities;
        private readonly KeyPair _trustor;

        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Builder to build the AllowTrust Operation given an XDR AllowTrustOp
        /// </summary>
        /// <exception cref="Exception"></exception>
        public Builder(AllowTrustOp op)
        {
            _trustor = KeyPair.FromXdrPublicKey(op.Trustor.InnerValue);
            switch (op.Asset.Discriminant.InnerValue)
            {
                case AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4:
                    _assetCode = Encoding.UTF8.GetString(op.Asset.AssetCode4.InnerValue);
                    break;
                case AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12:
                    _assetCode = Encoding.UTF8.GetString(op.Asset.AssetCode12.InnerValue);
                    break;
                case AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE:
                    break;
                default:
                    throw new Exception("Unknown asset code");
            }

            var trustlineFlag = op.Authorize.InnerValue;
            switch (trustlineFlag)
            {
                case (uint)TrustLineFlags.TrustLineFlagsEnum.AUTHORIZED_FLAG:
                    _authorize = true;
                    _authorizeToMaintainLiabilities = false;
                    break;
                case (uint)TrustLineFlags.TrustLineFlagsEnum.AUTHORIZED_TO_MAINTAIN_LIABILITIES_FLAG:
                    _authorize = false;
                    _authorizeToMaintainLiabilities = true;
                    break;
                default:
                    _authorize = false;
                    _authorizeToMaintainLiabilities = false;
                    break;
            }
        }

        /// <summary>
        ///     Creates a new AllowTrust builder.
        /// </summary>
        /// <param name="trustor">The account of the recipient of the trustline.</param>
        /// <param name="assetCode">
        ///     The asset of the trustline the source account is authorizing. For example, if a gateway wants
        ///     to allow another account to hold its USD credit, the type is USD.
        /// </param>
        /// <param name="authorize">Flag indicating whether the trustline is authorized.</param>
        /// <param name="authorizeToMaintainLiabilities">
        ///     Flag indicating whether the trustline is authorized to maintain
        ///     liabilities
        /// </param>
        public Builder(KeyPair trustor, string assetCode, bool authorize, bool authorizeToMaintainLiabilities)
        {
            _trustor = trustor;
            _assetCode = assetCode;
            _authorize = authorize;
            _authorizeToMaintainLiabilities = authorizeToMaintainLiabilities;
        }

        /// <summary>
        ///     Set source account of this operation
        /// </summary>
        /// <param name="sourceAccount">Source account</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }

        /// <summary>
        ///     Builds an operation
        /// </summary>
        public AllowTrustOperation Build()
        {
            var operation = new AllowTrustOperation(_trustor, _assetCode, _authorize, _authorizeToMaintainLiabilities);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}