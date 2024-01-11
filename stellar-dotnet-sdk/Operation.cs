﻿using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk
{
    /// <summary>
    ///     An individual command that modifies the ledger. Operations are used to send payments, enter orders into the decentralized exchange,
    ///     change settings on accounts, and authorize accounts to hold assets.
    /// </summary>
    public abstract class Operation
    {
        private IAccountId? _sourceAccount;

        /// <summary>
        ///     The account to execute this operation upon.
        /// </summary>
        public IAccountId? SourceAccount
        {
            get => _sourceAccount;
            set => _sourceAccount = value ?? throw new ArgumentNullException(nameof(SourceAccount), "source account cannot be null");
        }

        /// <summary>
        /// Threshold level for the operation.
        /// </summary>
        public virtual OperationThreshold Threshold => OperationThreshold.Medium;

        public static long ToXdrAmount(string value)
        {
            return Amount.ToXdr(value);
        }

        public static string FromXdrAmount(long value)
        {
            return Amount.FromXdr(value);
        }
        
        /// <summary>
        /// Creates a new Operation object from the given Operation XDR base64-encoded string.
        /// </summary>
        /// <param name="xdrBase64"></param>
        /// <returns>Operation object</returns>
        public static Operation FromXdrBase64(string xdrBase64)
        {
            var bytes = Convert.FromBase64String(xdrBase64);
            var reader = new XdrDataInputStream(bytes);
            var thisXdr = xdr.Operation.Decode(reader);
            return FromXdr(thisXdr);
        }

        ///<summary>
        /// Generates Operation XDR object.
        ///</summary>
        public xdr.Operation ToXdr()
        {
            var thisXdr = new xdr.Operation();
            if (SourceAccount != null)
            {
                thisXdr.SourceAccount = SourceAccount.MuxedAccount;
            }

            thisXdr.Body = ToOperationBody();
            return thisXdr;
        }

        ///<summary>
        /// Returns base64-encoded <see cref="stellar_dotnet_sdk.xdr.Operation"/> XDR string.
        ///</summary>
        public string ToXdrBase64()
        {
            var operation = ToXdr();
            var writer = new XdrDataOutputStream();
            xdr.Operation.Encode(writer, operation);
            return Convert.ToBase64String(writer.ToArray());
        }

        ///<summary>
        ///</summary>
        /// <returns>new Operation object from Operation XDR object.</returns>
        /// <param name="thisXdr">XDR object</param>
        public static Operation FromXdr(xdr.Operation thisXdr)
        {
            var body = thisXdr.Body;
            Operation operation;
            switch (body.Discriminant.InnerValue)
            {
                case OperationType.OperationTypeEnum.CREATE_ACCOUNT:
                    operation = new CreateAccountOperation.Builder(body.CreateAccountOp).Build();
                    break;
                case OperationType.OperationTypeEnum.PAYMENT:
                    operation = new PaymentOperation.Builder(body.PaymentOp).Build();
                    break;
                case OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_RECEIVE:
                    operation = new PathPaymentStrictReceiveOperation.Builder(body.PathPaymentStrictReceiveOp).Build();
                    break;
                case OperationType.OperationTypeEnum.MANAGE_SELL_OFFER:
                    operation = new ManageSellOfferOperation.Builder(body.ManageSellOfferOp).Build();
                    break;
                case OperationType.OperationTypeEnum.CREATE_PASSIVE_SELL_OFFER:
                    operation = new CreatePassiveSellOfferOperation.Builder(body.CreatePassiveSellOfferOp).Build();
                    break;
                case OperationType.OperationTypeEnum.SET_OPTIONS:
                    operation = new SetOptionsOperation.Builder(body.SetOptionsOp).Build();
                    break;
                case OperationType.OperationTypeEnum.CHANGE_TRUST:
                    operation = new ChangeTrustOperation.Builder(body.ChangeTrustOp).Build();
                    break;
                case OperationType.OperationTypeEnum.ALLOW_TRUST:
                    operation = new AllowTrustOperation.Builder(body.AllowTrustOp).Build();
                    break;
                case OperationType.OperationTypeEnum.ACCOUNT_MERGE:
                    operation = new AccountMergeOperation.Builder(body).Build();
                    break;
                case OperationType.OperationTypeEnum.INFLATION:
                    operation = new InflationOperation.Builder().Build();
                    break;
                case OperationType.OperationTypeEnum.MANAGE_DATA:
                    operation = new ManageDataOperation.Builder(body.ManageDataOp).Build();
                    break;
                case OperationType.OperationTypeEnum.BUMP_SEQUENCE:
                    operation = new BumpSequenceOperation.Builder(body.BumpSequenceOp).Build();
                    break;
                case OperationType.OperationTypeEnum.MANAGE_BUY_OFFER:
                    operation = new ManageBuyOfferOperation.Builder(body.ManageBuyOfferOp).Build();
                    break;
                case OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_SEND:
                    operation = new PathPaymentStrictSendOperation.Builder(body.PathPaymentStrictSendOp).Build();
                    break;
                case OperationType.OperationTypeEnum.CREATE_CLAIMABLE_BALANCE:
                    operation = new CreateClaimableBalanceOperation.Builder(body.CreateClaimableBalanceOp).Build();
                    break;
                case OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE:
                    operation = new ClaimClaimableBalanceOperation.Builder(body.ClaimClaimableBalanceOp).Build();
                    break;
                case OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES:
                    operation = new BeginSponsoringFutureReservesOperation.Builder(body.BeginSponsoringFutureReservesOp).Build();
                    break;
                case OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES:
                    operation = new EndSponsoringFutureReservesOperation.Builder().Build();
                    break;
                case OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP:
                    operation = new RevokeSponsorshipOperation.Builder(body.RevokeSponsorshipOp).Build();
                    break;
                case OperationType.OperationTypeEnum.CLAWBACK:
                    operation = new ClawbackOperation.Builder(body.ClawbackOp).Build();
                    break;
                case OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE:
                    operation = new ClawbackClaimableBalanceOperation.Builder(body.ClawbackClaimableBalanceOp).Build();
                    break;
                case OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS:
                    operation = new SetTrustlineFlagsOperation.Builder(body.SetTrustLineFlagsOp).Build();
                    break;
                case OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT:
                    operation = new LiquidityPoolDepositOperation.Builder(body.LiquidityPoolDepositOp).Build();
                    break;
                case OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW:
                    operation = new LiquidityPoolWithdrawOperation.Builder(body.LiquidityPoolWithdrawOp).Build();
                    break;
                case OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION:
                    operation = body.InvokeHostFunctionOp.HostFunction.Discriminant.InnerValue switch
                    {
                        HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_INVOKE_CONTRACT =>
                            new InvokeContractOperation.Builder(body.InvokeHostFunctionOp).Build(),
                        HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_CREATE_CONTRACT =>
                            new CreateContractOperation.Builder(body.InvokeHostFunctionOp).Build(),
                        HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_UPLOAD_CONTRACT_WASM =>
                            new UploadContractOperation.Builder(body.InvokeHostFunctionOp).Build(),
                    };
                    break;
                case OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL:
                    operation = new ExtendFootprintOperation.Builder(body.ExtendFootprintTTLOp).Build();
                    break;
                case OperationType.OperationTypeEnum.RESTORE_FOOTPRINT:
                    operation = new RestoreFootprintOperation.Builder(body.RestoreFootprintOp).Build();
                    break;
                default:
                    throw new Exception("Unknown operation body " + body.Discriminant.InnerValue);
            }

            if (thisXdr.SourceAccount != null)
            {
                operation.SourceAccount = MuxedAccount.FromXdrMuxedAccount(thisXdr.SourceAccount);
            }

            return operation;
        }

        ///<summary>
        /// Generates OperationBody XDR object
        ///</summary>
        ///<returns>OperationBody XDR object</returns>
        public abstract xdr.Operation.OperationBody ToOperationBody();
    }
}