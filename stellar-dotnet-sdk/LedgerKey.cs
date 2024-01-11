using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk
{
    public abstract class LedgerKey
    {
        public abstract xdr.LedgerKey ToXdr();

        public static LedgerKey Account(KeyPair account) => new LedgerKeyAccount(account);
        public static LedgerKey ClaimableBalance(byte[] balanceId) => new LedgerKeyClaimableBalance(balanceId);
        public static LedgerKey Data(KeyPair account, string dataName) => new LedgerKeyData(account, dataName);
        public static LedgerKey Offer(KeyPair seller, long offerId) => new LedgerKeyOffer(seller, offerId);
        public static LedgerKey Trustline(KeyPair account, TrustlineAsset asset) => new LedgerKeyTrustline(account, asset);
        public static LedgerKey LiquidityPool(LiquidityPoolID poolId) => new LedgerKeyLiquidityPool(poolId);
        public static LedgerKey ContractData(SCAddress contractId, SCVal key, ContractDataDurability durability) => new LedgerKeyContractData(contractId, key, durability);
        public static LedgerKey ContractCode(string code) => new LedgerKeyContractCode(code);
        public static LedgerKey ConfigSetting(ConfigSettingID settingId) => new LedgerKeyConfigSetting(settingId);
        public static LedgerKey TTL(string key) => new LedgerKeyTTL(key);

        public static LedgerKey FromXdr(xdr.LedgerKey xdr)
        {
            switch (xdr.Discriminant.InnerValue)
            {
                case LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT:
                    return LedgerKeyAccount.FromXdr(xdr.Account);
                case LedgerEntryType.LedgerEntryTypeEnum.DATA:
                    return LedgerKeyData.FromXdr(xdr.Data);
                case LedgerEntryType.LedgerEntryTypeEnum.OFFER:
                    return LedgerKeyOffer.FromXdr(xdr.Offer);
                case LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE:
                    return LedgerKeyTrustline.FromXdr(xdr.TrustLine);
                case LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE:
                    return LedgerKeyClaimableBalance.FromXdr(xdr.ClaimableBalance);
                case LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL:
                    return LedgerKeyLiquidityPool.FromXdr(xdr.LiquidityPool);
                case LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA:
                    return LedgerKeyContractData.FromXdr(xdr.ContractData);
                case LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE:
                    return LedgerKeyContractCode.FromXdr(xdr.ContractCode);
                case LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING:
                    return LedgerKeyConfigSetting.FromXdr(xdr.ConfigSetting);
                case LedgerEntryType.LedgerEntryTypeEnum.TTL:
                    return LedgerKeyTTL.FromXdr(xdr.Ttl);
                default:
                    throw new Exception("Unknown ledger key " + xdr.Discriminant.InnerValue);
            }
        }
        
        ///<summary>
        /// Returns base64-encoded LedgerKey XDR object.
        ///</summary>
        public string ToXdrBase64()
        {
            var xdrValue = ToXdr();
            var writer = new XdrDataOutputStream();
            xdr.LedgerKey.Encode(writer, xdrValue);
            return Convert.ToBase64String(writer.ToArray());
        }
    }
}