// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  union SCAddress switch (SCAddressType type)
//  {
//  case SC_ADDRESS_TYPE_ACCOUNT:
//      AccountID accountId;
//  case SC_ADDRESS_TYPE_CONTRACT:
//      ContractID contractId;
//  case SC_ADDRESS_TYPE_MUXED_ACCOUNT:
//      MuxedEd25519Account muxedAccount;
//  case SC_ADDRESS_TYPE_CLAIMABLE_BALANCE:
//      ClaimableBalanceID claimableBalanceId;
//  case SC_ADDRESS_TYPE_LIQUIDITY_POOL:
//      PoolID liquidityPoolId;
//  };

//  ===========================================================================
public class SCAddress
{
    public SCAddressType Discriminant { get; set; } = new();

    public AccountID AccountId { get; set; }
    public ContractID ContractId { get; set; }
    public MuxedEd25519Account MuxedAccount { get; set; }
    public ClaimableBalanceID ClaimableBalanceId { get; set; }
    public PoolID LiquidityPoolId { get; set; }

    public static void Encode(XdrDataOutputStream stream, SCAddress encodedSCAddress)
    {
        stream.WriteInt((int)encodedSCAddress.Discriminant.InnerValue);
        switch (encodedSCAddress.Discriminant.InnerValue)
        {
            case SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_ACCOUNT:
                AccountID.Encode(stream, encodedSCAddress.AccountId);
                break;
            case SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_CONTRACT:
                ContractID.Encode(stream, encodedSCAddress.ContractId);
                break;
            case SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_MUXED_ACCOUNT:
                MuxedEd25519Account.Encode(stream, encodedSCAddress.MuxedAccount);
                break;
            case SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_CLAIMABLE_BALANCE:
                ClaimableBalanceID.Encode(stream, encodedSCAddress.ClaimableBalanceId);
                break;
            case SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_LIQUIDITY_POOL:
                PoolID.Encode(stream, encodedSCAddress.LiquidityPoolId);
                break;
        }
    }

    public static SCAddress Decode(XdrDataInputStream stream)
    {
        var decodedSCAddress = new SCAddress();
        var discriminant = SCAddressType.Decode(stream);
        decodedSCAddress.Discriminant = discriminant;
        switch (decodedSCAddress.Discriminant.InnerValue)
        {
            case SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_ACCOUNT:
                decodedSCAddress.AccountId = AccountID.Decode(stream);
                break;
            case SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_CONTRACT:
                decodedSCAddress.ContractId = ContractID.Decode(stream);
                break;
            case SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_MUXED_ACCOUNT:
                decodedSCAddress.MuxedAccount = MuxedEd25519Account.Decode(stream);
                break;
            case SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_CLAIMABLE_BALANCE:
                decodedSCAddress.ClaimableBalanceId = ClaimableBalanceID.Decode(stream);
                break;
            case SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_LIQUIDITY_POOL:
                decodedSCAddress.LiquidityPoolId = PoolID.Decode(stream);
                break;
        }
        return decodedSCAddress;
    }
}