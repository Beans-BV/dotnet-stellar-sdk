using System;
using sdkxdr = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="PathPaymentStrictReceiveOperation" />.
///     Use <see cref="Builder" /> to create a new PathPaymentStrictReceiveOperation.
///     See also:
///     <see href="https://www.stellar.org/developers/guides/concepts/list-of-operations.html#path-payment">Path Payment</see>
/// </summary>
public class PathPaymentStrictReceiveOperation : Operation
{
    private PathPaymentStrictReceiveOperation(Asset sendAsset, string sendMax, IAccountId destination,
        Asset destAsset, string destAmount, Asset[]? path)
    {
        SendAsset = sendAsset ?? throw new ArgumentNullException(nameof(sendAsset), "sendAsset cannot be null");
        SendMax = sendMax ?? throw new ArgumentNullException(nameof(sendMax), "sendMax cannot be null");
        Destination = destination ?? throw new ArgumentNullException(nameof(destination), "destination cannot be null");
        DestAsset = destAsset ?? throw new ArgumentNullException(nameof(destAsset), "destAsset cannot be null");
        DestAmount = destAmount ?? throw new ArgumentNullException(nameof(destAmount), "destAmount cannot be null");

        if (path == null)
        {
            Path = Array.Empty<Asset>();
        }
        else
        {
            if (path.Length > 5)
                throw new ArgumentException("The maximum number of assets in the path is 5", nameof(path));
            Path = path;
        }
    }

    public Asset SendAsset { get; }

    public string SendMax { get; }

    public IAccountId Destination { get; }

    public Asset DestAsset { get; }

    public string DestAmount { get; }

    public Asset[] Path { get; }

    public override sdkxdr.Operation.OperationBody ToOperationBody()
    {
        var op = new sdkxdr.PathPaymentStrictReceiveOp
        {
            // sendAsset
            SendAsset = SendAsset.ToXdr()
        };

        // sendMax
        var sendMax = new sdkxdr.Int64
        {
            InnerValue = ToXdrAmount(SendMax)
        };
        op.SendMax = sendMax;
        // destination
        op.Destination = Destination.MuxedAccount;
        // destAsset
        op.DestAsset = DestAsset.ToXdr();
        // destAmount
        var destAmount = new sdkxdr.Int64
        {
            InnerValue = ToXdrAmount(DestAmount)
        };
        op.DestAmount = destAmount;
        // path
        var path = new sdkxdr.Asset[Path.Length];
        for (var i = 0; i < Path.Length; i++)
            path[i] = Path[i].ToXdr();
        op.Path = path;

        var body = new sdkxdr.Operation.OperationBody
        {
            Discriminant =
                sdkxdr.OperationType.Create(sdkxdr.OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_RECEIVE),
            PathPaymentStrictReceiveOp = op
        };
        return body;
    }

    /// <summary>
    ///     Builds a <see cref="PathPaymentStrictReceiveOperation" />.
    /// </summary>
    public class Builder
    {
        private readonly string _destAmount;
        private readonly Asset _destAsset;
        private readonly IAccountId _destination;
        private readonly Asset _sendAsset;
        private readonly string _sendMax;

        private IAccountId? _mSourceAccount;
        private Asset[]? _path;

        public Builder(sdkxdr.PathPaymentStrictReceiveOp op)
        {
            _sendAsset = Asset.FromXdr(op.SendAsset);
            _sendMax = FromXdrAmount(op.SendMax.InnerValue);
            _destination = MuxedAccount.FromXdrMuxedAccount(op.Destination);
            _destAsset = Asset.FromXdr(op.DestAsset);
            _destAmount = FromXdrAmount(op.DestAmount.InnerValue);
            _path = new Asset[op.Path.Length];
            for (var i = 0; i < op.Path.Length; i++)
                _path[i] = Asset.FromXdr(op.Path[i]);
        }

        /// <summary>
        ///     Creates a new PathPaymentStrictReceiveOperation builder.
        /// </summary>
        /// <param name="sendAsset"> The asset deducted from the sender's account.</param>
        /// <param name="sendMax"> The asset deducted from the sender's account.</param>
        /// <param name="destination"> Payment destination.</param>
        /// <param name="destAsset"> The asset the destination account receives.</param>
        /// <param name="destAmount"> The amount of destination asset the destination account receives.</param>
        /// <exception cref="ArithmeticException"> When sendMax or destAmount has more than 7 decimal places.</exception>
        public Builder(Asset sendAsset, string sendMax, IAccountId destination, Asset destAsset, string destAmount)
        {
            _sendAsset = sendAsset ?? throw new ArgumentNullException(nameof(sendAsset), "sendAsset cannot be null");
            _sendMax = sendMax ?? throw new ArgumentNullException(nameof(sendMax), "sendMax cannot be null");
            _destination = destination ??
                           throw new ArgumentNullException(nameof(destination), "destination cannot be null");
            _destAsset = destAsset ?? throw new ArgumentNullException(nameof(destAsset), "destAsset cannot be null");
            _destAmount = destAmount ??
                          throw new ArgumentNullException(nameof(destAmount), "destAmount cannot be null");
        }

        /// <summary>
        ///     Sets path for this operation
        ///     <param name="path">
        ///         The assets (other than send asset and destination asset) involved in the offers the path takes.
        ///         For example, if you can only find a path from USD to EUR through XLM and BTC, the path would be USD ->
        ///         XLM -> BTC -> EUR and the path field would contain XLM and BTC.
        ///     </param>
        ///     <returns>Builder object so you can chain methods</returns>
        /// </summary>
        public Builder SetPath(Asset[] path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), "path cannot be null");

            if (path.Length > 5)
                throw new ArgumentException("The maximum number of assets in the path is 5", nameof(path));
            _path = path;
            return this;
        }

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="sourceAccount"> The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(IAccountId sourceAccount)
        {
            _mSourceAccount = sourceAccount ??
                              throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null");
            return this;
        }

        /// <summary>
        ///     Builds an operation
        /// </summary>
        /// <returns></returns>
        public PathPaymentStrictReceiveOperation Build()
        {
            var operation =
                new PathPaymentStrictReceiveOperation(_sendAsset, _sendMax, _destination, _destAsset, _destAmount,
                    _path);
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}