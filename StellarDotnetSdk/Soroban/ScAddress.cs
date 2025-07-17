using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using SCAddressTypeEnum = StellarDotnetSdk.Xdr.SCAddressType.SCAddressTypeEnum;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     <c>ScAddress</c> represents a single address in the Stellar network that can be
///     inputted to or outputted by a smart contract. An address can represent an
///     account, muxed account, contract, claimable balance, or a liquidity pool
///     (the latter two can only be present as the *output* of Core in the form
///     of an event, never an input to a smart contract).
/// </summary>
public abstract class ScAddress : SCVal
{
    public static ScAddress FromXdr(SCAddress xdrScAddress)
    {
        return xdrScAddress.Discriminant.InnerValue switch
        {
            SCAddressTypeEnum.SC_ADDRESS_TYPE_ACCOUNT =>
                ScAccountId.FromXdr(xdrScAddress),
            SCAddressTypeEnum.SC_ADDRESS_TYPE_CONTRACT =>
                ScContractId.FromXdr(xdrScAddress),
            SCAddressTypeEnum.SC_ADDRESS_TYPE_MUXED_ACCOUNT =>
                ScMuxedAccountId.FromXdr(xdrScAddress),
            SCAddressTypeEnum.SC_ADDRESS_TYPE_CLAIMABLE_BALANCE =>
                ScClaimableBalanceId.FromXdr(xdrScAddress),
            SCAddressTypeEnum.SC_ADDRESS_TYPE_LIQUIDITY_POOL =>
                ScLiquidityPoolId.FromXdr(xdrScAddress),
            _ => throw new ArgumentOutOfRangeException(nameof(xdrScAddress), "Invalid address type."),
        };
    }

    public static ScAddress FromSCValXdr(Xdr.SCVal xdrVal)
    {
        if (xdrVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_ADDRESS)
        {
            throw new ArgumentException("Not an SCAddress.", nameof(xdrVal));
        }

        return FromXdr(xdrVal.Address);
    }

    public Xdr.SCVal ToSCValXdr()
    {
        return new Xdr.SCVal
        {
            Discriminant = new SCValType
            {
                InnerValue = SCValType.SCValTypeEnum.SCV_ADDRESS,
            },
            Address = ToXdr(),
        };
    }

    public abstract SCAddress ToXdr();
}

public class ScAccountId : ScAddress
{
    /// <summary>
    ///     Creates an ScAccountId instance from an Ed25519 public key (G...).
    /// </summary>
    /// <param name="publicKey">A base32-encoded Ed25519 public key (G...).</param>
    public ScAccountId(string publicKey)
    {
        if (!StrKey.IsValidEd25519PublicKey(publicKey))
        {
            throw new ArgumentException($"Invalid Ed25519 public key {publicKey}.");
        }

        InnerValue = publicKey;
    }

    public string InnerValue { get; set; }

    public static ScAccountId FromXdr(SCAddress xdr)
    {
        return new ScAccountId(
            KeyPair.FromXdrPublicKey(xdr.AccountId.InnerValue).AccountId
        );
    }

    public override SCAddress ToXdr()
    {
        return new SCAddress
        {
            Discriminant = new SCAddressType
            {
                InnerValue = SCAddressTypeEnum.SC_ADDRESS_TYPE_ACCOUNT,
            },
            AccountId = new AccountID(KeyPair.FromAccountId(InnerValue).XdrPublicKey),
        };
    }
}

public class ScClaimableBalanceId : ScAddress
{
    /// <summary>
    ///     Creates an <c>ScClaimableBalanceId</c> instance from a claimable balance ID (B...).
    /// </summary>
    /// <param name="claimableBalanceId">A base32-encoded claimable balance ID (B...).</param>
    public ScClaimableBalanceId(string claimableBalanceId)
    {
        if (!StrKey.IsValidClaimableBalanceId(claimableBalanceId))
        {
            throw new ArgumentException($"Invalid claimable balance ID {claimableBalanceId}.");
        }

        InnerValue = claimableBalanceId;
    }

    public string InnerValue { get; }

    public static ScClaimableBalanceId FromXdr(SCAddress xdr)
    {
        return new ScClaimableBalanceId(
            ClaimableBalanceUtils.FromXdr(xdr.ClaimableBalanceId)
        );
    }

    public override SCAddress ToXdr()
    {
        return new SCAddress
        {
            Discriminant = SCAddressType.Create(SCAddressTypeEnum.SC_ADDRESS_TYPE_CLAIMABLE_BALANCE),
            ClaimableBalanceId = ClaimableBalanceUtils.ToXdr(InnerValue),
        };
    }
}

public class ScContractId : ScAddress
{
    /// <summary>
    ///     Creates an ScContractId instance from a contract ID (C...).
    /// </summary>
    /// <param name="contractId">A base32-encoded contract pool ID (C...).</param>
    public ScContractId(string contractId)
    {
        if (!StrKey.IsValidContractId(contractId))
        {
            throw new ArgumentException($"Invalid contract ID {contractId}.");
        }

        InnerValue = contractId;
    }

    public string InnerValue { get; }

    public static ScContractId FromXdr(SCAddress xdr)
    {
        return new ScContractId(
            StrKey.EncodeContractId(xdr.ContractId.InnerValue.InnerValue)
        );
    }

    public override SCAddress ToXdr()
    {
        return new SCAddress
        {
            Discriminant = new SCAddressType
            {
                InnerValue = SCAddressTypeEnum.SC_ADDRESS_TYPE_CONTRACT,
            },
            ContractId = new ContractID(
                new Hash(StrKey.DecodeContractId(InnerValue))
            ),
        };
    }
}

public class ScLiquidityPoolId : ScAddress
{
    /// <summary>
    ///     Creates an ScLiquidityPool instance from a liquidity pool ID (L...).
    /// </summary>
    /// <param name="liquidityPoolId">A base32-encoded liquidity pool ID (L...).</param>
    public ScLiquidityPoolId(string liquidityPoolId)
    {
        if (!StrKey.IsValidLiquidityPoolId(liquidityPoolId))
        {
            throw new ArgumentException($"Invalid liquidity pool ID {liquidityPoolId}.");
        }
        InnerLiquidityPoolId = liquidityPoolId;
    }

    public string InnerLiquidityPoolId { get; }

    public static ScLiquidityPoolId FromXdr(SCAddress xdr)
    {
        return new ScLiquidityPoolId(
            StrKey.EncodeLiquidityPoolId(xdr.LiquidityPoolId.InnerValue.InnerValue)
        );
    }

    public override SCAddress ToXdr()
    {
        return new SCAddress
        {
            Discriminant = new SCAddressType
            {
                InnerValue = SCAddressTypeEnum.SC_ADDRESS_TYPE_LIQUIDITY_POOL,
            },
            LiquidityPoolId = new PoolID
            {
                InnerValue = new Hash(StrKey.DecodeLiquidityPoolId(InnerLiquidityPoolId)),
            },
        };
    }
}

public class ScMuxedAccountId : ScAddress
{
    /// <summary>
    ///     Creates an ScMuxedAccountId instance from a Muxed Ed25519 public key (M...).
    /// </summary>
    /// <param name="publicKey">A base32-encoded Muxed Ed25519 public key (M...).</param>
    public ScMuxedAccountId(string publicKey)
    {
        if (!StrKey.IsValidMuxedAccount(publicKey))
        {
            throw new ArgumentException($"Invalid muxed Ed25519 public key {publicKey}.");
        }

        InnerValue = publicKey;
    }

    public string InnerValue { get; set; }

    public static ScMuxedAccountId FromXdr(SCAddress xdr)
    {
        var writer = new XdrDataOutputStream();
        MuxedEd25519Account.Encode(writer, xdr.MuxedAccount);
        var bytes = writer.ToArray();
        return new ScMuxedAccountId(
            StrKey.EncodeMed25519PublicKey(bytes)
        );
    }

    public override SCAddress ToXdr()
    {
        var decoded = StrKey.DecodeMed25519PublicKey(InnerValue);
        var muxedEd25519Account = MuxedEd25519Account.Decode(new XdrDataInputStream(decoded));
        return new SCAddress
        {
            Discriminant = new SCAddressType
            {
                InnerValue = SCAddressTypeEnum.SC_ADDRESS_TYPE_MUXED_ACCOUNT,
            },
            MuxedAccount = muxedEd25519Account,
        };
    }
}