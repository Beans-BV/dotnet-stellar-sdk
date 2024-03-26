using System;
using System.Linq;
using sdkxdr = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="PathPaymentStrictSendOperation" />.
///     Use <see cref="Builder" /> to create a new PathPaymentStrictSendOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#path-payment-strict-send">
///         Path
///         payment strict send
///     </a>
/// </summary>
public class PathPaymentStrictSendOperation : Operation
{
    private PathPaymentStrictSendOperation(
        Asset sendAsset,
        string sendAmount,
        IAccountId destination,
        Asset destAsset,
        string destMin,
        Asset[]? path)
    {
        SendAsset = sendAsset ?? throw new ArgumentNullException(nameof(sendAsset), "sendAsset cannot be null");
        SendAmount = sendAmount ?? throw new ArgumentNullException(nameof(sendAmount), "sendAmount cannot be null");
        Destination = destination ?? throw new ArgumentNullException(nameof(destination), "destination cannot be null");
        DestAsset = destAsset ?? throw new ArgumentNullException(nameof(destAsset), "destAsset cannot be null");
        DestMin = destMin ?? throw new ArgumentNullException(nameof(destMin), "destMin cannot be null");

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

    public string SendAmount { get; }

    public IAccountId Destination { get; }

    public Asset DestAsset { get; }

    public string DestMin { get; }

    public Asset[] Path { get; }

    public override sdkxdr.Operation.OperationBody ToOperationBody()
    {
        var body = new sdkxdr.Operation.OperationBody
        {
            Discriminant = sdkxdr.OperationType.Create(sdkxdr.OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_SEND),
            PathPaymentStrictSendOp = new sdkxdr.PathPaymentStrictSendOp
            {
                SendAsset = SendAsset.ToXdr(),
                SendAmount = new sdkxdr.Int64 { InnerValue = ToXdrAmount(SendAmount) },
                Destination = Destination.MuxedAccount,
                DestAsset = DestAsset.ToXdr(),
                DestMin = new sdkxdr.Int64 { InnerValue = ToXdrAmount(DestMin) },
                Path = Path.Select(a => a.ToXdr()).ToArray()
            }
        };
        return body;
    }

    /// <summary>
    ///     Builds a <see cref="PathPaymentStrictSendOperation" />.
    /// </summary>
    public class Builder
    {
        private readonly Asset _destAsset;
        private readonly IAccountId _destination;
        private readonly string _destMin;
        private readonly string _sendAmount;
        private readonly Asset _sendAsset;

        private IAccountId? _mSourceAccount;
        private Asset[]? _path;

        public Builder(sdkxdr.PathPaymentStrictSendOp op)
        {
            _sendAsset = Asset.FromXdr(op.SendAsset);
            _sendAmount = FromXdrAmount(op.SendAmount.InnerValue);
            _destination = MuxedAccount.FromXdrMuxedAccount(op.Destination);
            _destAsset = Asset.FromXdr(op.DestAsset);
            _destMin = FromXdrAmount(op.DestMin.InnerValue);
            _path = op.Path.Select(Asset.FromXdr).ToArray();
        }

        /// <summary>
        ///     Creates a new PathPaymentStrictSendOperation builder.
        /// </summary>
        /// <param name="sendAsset"> The asset deducted from the sender's account.</param>
        /// <param name="sendAmount"> The asset deducted from the sender's account.</param>
        /// <param name="destination"> Payment destination.</param>
        /// <param name="destAsset"> The asset the destination account receives.</param>
        /// <param name="destMin"> The amount of destination asset the destination account receives.</param>
        /// <exception cref="ArithmeticException"> When sendAmount or destMin has more than 7 decimal places.</exception>
        public Builder(Asset sendAsset, string sendAmount, IAccountId destination, Asset destAsset, string destMin)
        {
            _sendAsset = sendAsset ?? throw new ArgumentNullException(nameof(sendAsset), "sendAsset cannot be null");
            _sendAmount = sendAmount ??
                          throw new ArgumentNullException(nameof(sendAmount), "sendAmount cannot be null");
            _destination = destination ??
                           throw new ArgumentNullException(nameof(destination), "destination cannot be null");
            _destAsset = destAsset ?? throw new ArgumentNullException(nameof(destAsset), "destAsset cannot be null");
            _destMin = destMin ?? throw new ArgumentNullException(nameof(destMin), "destMin cannot be null");
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
        public PathPaymentStrictSendOperation Build()
        {
            var operation = new PathPaymentStrictSendOperation(
                _sendAsset,
                _sendAmount,
                _destination,
                _destAsset,
                _destMin,
                _path);
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}