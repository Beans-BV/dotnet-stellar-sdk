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
            return xdr.Discriminant.InnerValue switch
            {
                LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT => LedgerKeyAccount.FromXdr(xdr.Account),
                LedgerEntryType.LedgerEntryTypeEnum.DATA => LedgerKeyData.FromXdr(xdr.Data),
                LedgerEntryType.LedgerEntryTypeEnum.OFFER => LedgerKeyOffer.FromXdr(xdr.Offer),
                LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE => LedgerKeyTrustline.FromXdr(xdr.TrustLine),
                LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE => LedgerKeyClaimableBalance.FromXdr(
                    xdr.ClaimableBalance),
                LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL => LedgerKeyLiquidityPool.FromXdr(xdr.LiquidityPool),
                LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA => LedgerKeyContractData.FromXdr(xdr.ContractData),
                LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE => LedgerKeyContractCode.FromXdr(xdr.ContractCode),
                LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING => LedgerKeyConfigSetting.FromXdr(xdr.ConfigSetting),
                LedgerEntryType.LedgerEntryTypeEnum.TTL => LedgerKeyTTL.FromXdr(xdr.Ttl),
                _ => throw new Exception("Unknown ledger key " + xdr.Discriminant.InnerValue)
            };
        }
        
        /// <summary>
        ///     Creates a new <see cref="LedgerKey"/> object from the given LedgerEntry XDR base64 string.
        /// </summary>
        /// <param name="xdrBase64"></param>
        /// <returns><see cref="LedgerKey"/> object</returns>
        public static LedgerKey FromXdrBase64(string xdrBase64)
        {
            var bytes = Convert.FromBase64String(xdrBase64);
            var reader = new XdrDataInputStream(bytes);
            var thisXdr = xdr.LedgerKey.Decode(reader);
            return FromXdr(thisXdr);
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