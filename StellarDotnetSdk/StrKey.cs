using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FormatException = StellarDotnetSdk.Exceptions.FormatException;
using xdrSDK = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk;

using MuxedAccount = xdrSDK.MuxedAccount;

public class StrKey
{
    private static readonly byte[] Base32LookupTable = DecodingTable();

    public enum VersionByte : byte
    {
        ACCOUNT_ID = 6 << 3,
        MUXED_ED25519 = 12 << 3,
        SEED = 18 << 3,
        PRE_AUTH_TX = 19 << 3,
        SHA256_HASH = 23 << 3,
        SIGNED_PAYLOAD = 15 << 3,
        CONTRACT = 2 << 3,
        LIQUIDITY_POOL = 11 << 3,
        CLAIMABLE_BALANCE = 1 << 3,
    }

    public static string EncodeStellarAccountId(byte[] data)
    {
        return EncodeCheck(VersionByte.ACCOUNT_ID, data);
    }

    public static string EncodeClaimableBalanceId(byte[] data)
    {
        return EncodeCheck(VersionByte.CLAIMABLE_BALANCE, data);
    }

    public static string EncodeLiquidityPool(byte[] data)
    {
        return EncodeCheck(VersionByte.LIQUIDITY_POOL, data);
    }

    public static string EncodeSignedPayload(SignedPayloadSigner signedPayloadSigner)
    {
        try
        {
            var xdrPayloadSigner = new xdrSDK.SignerKey.SignerKeyEd25519SignedPayload
            {
                Payload = signedPayloadSigner.Payload,
                Ed25519 = signedPayloadSigner.SignerAccountId.InnerValue.Ed25519,
            };

            var stream = new xdrSDK.XdrDataOutputStream();
            xdrSDK.SignerKey.SignerKeyEd25519SignedPayload.Encode(stream, xdrPayloadSigner);

            return EncodeCheck(VersionByte.SIGNED_PAYLOAD, stream.ToArray());
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    [Obsolete("Deprecated. Use EncodeMuxedEd25519PublicKey instead.")]
    public static string EncodeStellarMuxedAccount(MuxedAccount muxedAccount)
    {
        switch (muxedAccount.Discriminant.InnerValue)
        {
            case xdrSDK.CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_MUXED_ED25519:
                var bytes = muxedAccount.Med25519.Ed25519.InnerValue
                    .Concat(Util.ToByteArray(muxedAccount.Med25519.Id.InnerValue))
                    .ToArray();
                return EncodeCheck(VersionByte.MUXED_ED25519, bytes);

            case xdrSDK.CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_ED25519:
                return EncodeCheck(VersionByte.ACCOUNT_ID, muxedAccount.Ed25519.InnerValue);

            default:
                throw new ArgumentException("invalid discriminant");
        }
    }

    public static string EncodeStellarSecretSeed(byte[] data)
    {
        return EncodeCheck(VersionByte.SEED, data);
    }

    public static string EncodeContractId(byte[] data)
    {
        return EncodeCheck(VersionByte.CONTRACT, data);
    }

    public static string EncodeMuxedEd25519PublicKey(byte[] data)
    {
        return EncodeCheck(VersionByte.MUXED_ED25519, data);
    }

    public static byte[] DecodeMuxedEd25519PublicKey(string data)
    {
        return DecodeCheck(VersionByte.MUXED_ED25519, data);
    }

    public static bool TryDecodeMuxedEd25519PublicKey(string publicKey, out byte[] decoded)
    {
        return TryDecode(VersionByte.MUXED_ED25519, publicKey, out decoded);
    }

    public static bool IsValidMuxedEd25519PublicKey(string publicKey)
    {
        return IsValid(VersionByte.MUXED_ED25519, publicKey);
    }

    public static byte[] DecodeStellarAccountId(string data)
    {
        return DecodeCheck(VersionByte.ACCOUNT_ID, data);
    }

    public static SignedPayloadSigner DecodeSignedPayload(string data)
    {
        try
        {
            var signedPayloadRaw = DecodeCheck(VersionByte.SIGNED_PAYLOAD, data);

            var xdrPayloadSigner =
                xdrSDK.SignerKey.SignerKeyEd25519SignedPayload.Decode(new xdrSDK.XdrDataInputStream(signedPayloadRaw));

            return new SignedPayloadSigner(xdrPayloadSigner.Ed25519.InnerValue, xdrPayloadSigner.Payload);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    [Obsolete("Deprecated. Use DecodeMuxedEd25519PublicKey instead.")]
    public static MuxedAccount DecodeStellarMuxedAccount(string data)
    {
        var muxed = new MuxedAccount();

        if (data.Length == 0)
        {
            throw new ArgumentException("Address is empty");
        }

        switch (DecodeVersionByte(data))
        {
            case VersionByte.ACCOUNT_ID:
                muxed.Discriminant.InnerValue = xdrSDK.CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_ED25519;

                try
                {
                    muxed.Ed25519 = xdrSDK.Uint256.Decode(new xdrSDK.XdrDataInputStream(DecodeStellarAccountId(data)));
                }
                catch (InvalidOperationException e)
                {
                    throw new ArgumentException("invalid address: " + data, e);
                }

                break;

            case VersionByte.MUXED_ED25519:
                var input = new xdrSDK.XdrDataInputStream(DecodeCheck(VersionByte.MUXED_ED25519, data));
                muxed.Discriminant.InnerValue = xdrSDK.CryptoKeyType.CryptoKeyTypeEnum.KEY_TYPE_MUXED_ED25519;
                var med = new MuxedAccount.MuxedAccountMed25519();

                try
                {
                    med.Ed25519 = xdrSDK.Uint256.Decode(input);
                    med.Id = xdrSDK.Uint64.Decode(input);
                }
                catch (InvalidOperationException e)
                {
                    throw new ArgumentException("invalid address: " + data, e);
                }

                muxed.Med25519 = med;
                break;
            default:
                throw new FormatException("Version byte is invalid");
        }

        return muxed;
    }

    public static byte[] DecodeStellarSecretSeed(string data)
    {
        return DecodeCheck(VersionByte.SEED, data);
    }

    public static byte[] DecodeContractId(string data)
    {
        return DecodeCheck(VersionByte.CONTRACT, data);
    }

    public static byte[] DecodeClaimableBalanceId(string data)
    {
        return DecodeCheck(VersionByte.CLAIMABLE_BALANCE, data);
    }

    /// <summary>
    /// Decodes liquidity pool ID to raw bytes. 
    /// </summary>
    /// <param name="data">A base32 liquidity pool ID (L...).</param>
    /// <returns>Raw bytes of the provided liquidity pool ID.</returns>
    public static byte[] DecodeLiquidityPool(string data)
    {
        return DecodeCheck(VersionByte.LIQUIDITY_POOL, data);
    }

    public static VersionByte DecodeVersionByte(string encoded)
    {
        var decoded = Base32Encoding.ToBytes(encoded);
        var versionByte = decoded[0];
        if (!Enum.IsDefined(typeof(VersionByte), versionByte))
        {
            throw new FormatException("Version byte is invalid");
        }
        return (VersionByte)versionByte;
    }

    public static string EncodeCheck(VersionByte versionByte, byte[] data)
    {
        var bytes = new List<byte>
        {
            (byte)versionByte,
        };

        bytes.AddRange(data);
        var checksum = CalculateChecksum(bytes.ToArray());
        bytes.AddRange(checksum);
        return Base32Encoding.ToString(bytes.ToArray(), options => options.OmitPadding = true);
    }

    public static byte[] DecodeCheck(VersionByte versionByte, string encoded)
    {
        // The minimal length is 3 bytes (version byte and 2-byte CRC) which,
        // in unpadded base32 (since each character provides 5 bits) corresponds to ceiling(8*3/5) = 5
        if (encoded.Length < 5)
        {
            throw new ArgumentException("Encoded string must have a length of at least 5.");
        }

        var leftoverBits = encoded.Length * 5 % 8;
        switch (leftoverBits)
        {
            // Make sure there is no full unused leftover byte at the end
            // (i.e. there shouldn't be 5 or more leftover bits)
            case >= 5:
                throw new ArgumentException("Encoded string has leftover characters.");
            case > 0:
            {
                var lastChar = encoded[^1];
                var decodedLastChar = Base32LookupTable[lastChar];

                var leftoverBitsMask = (byte)(0x0f >> (4 - leftoverBits));
                if ((decodedLastChar & leftoverBitsMask) != 0)
                {
                    throw new ArgumentException("Unused bits should be set to 0.");
                }
                break;
            }
        }

        var decoded = Base32Encoding.ToBytes(encoded);

        var decodedVersionByte = decoded[0];
        if (!Enum.IsDefined(typeof(VersionByte), decodedVersionByte))
        {
            throw new ArgumentException("Version byte is invalid");
        }
        var decodedVersionByteEnum = (VersionByte)decodedVersionByte;

        if (decodedVersionByteEnum != versionByte)
        {
            throw new ArgumentException("Version byte mismatch");
        }

        ReadOnlySpan<byte> decodedSpan = decoded;
        var payload = decodedSpan[..^2].ToArray(); // All except last 2 bytes for checksum  
        var data = decodedSpan[1..^2].ToArray(); // All except first 1 for version byte and last 2 for checksum
        var checksum = decodedSpan[^2..].ToArray(); // Last 2 bytes

        ValidateDataLength(decodedVersionByteEnum, data);

        var expectedChecksum = CalculateChecksum(payload);

        if (!expectedChecksum.SequenceEqual(checksum))
        {
            throw new ArgumentException("Checksum invalid");
        }

        return data;
    }

    private static void ValidateDataLength(VersionByte decodedVersionByteEnum, byte[] data)
    {
        switch (decodedVersionByteEnum)
        {
            case VersionByte.SIGNED_PAYLOAD:
                if (data.Length is < 32 + 4 + 4 or > 32 + 4 + 64)
                {
                    throw new ArgumentException(
                        "Invalid data length, the length should be between 40 and 100 bytes, got "
                        + data.Length);
                }
                break;
            case VersionByte.MUXED_ED25519:
                if (data.Length != 32 + 8)
                {
                    throw new ArgumentException(
                        "Invalid data length, expected 40 bytes, got " + data.Length);
                }
                break;
            case VersionByte.CLAIMABLE_BALANCE:
                if (data.Length != 32 + 1)
                {
                    // If we are encoding a claimable balance, the binary bytes of the key has a length of
                    // 33-bytes:
                    // 1-byte value indicating the type of claimable balance, where 0x00 maps to V0, and a
                    // 32-byte SHA256 hash.
                    throw new ArgumentException(
                        "Invalid data length, expected 33 bytes, got " + data.Length);
                }
                break;
            default:
                if (data.Length != 32)
                {
                    throw new ArgumentException(
                        "Invalid data length, expected 32 bytes, got " + data.Length);
                }
                break;
        }
    }

    private static byte[] CalculateChecksum(byte[] bytes)
    {
        // This code calculates CRC16-XModem checksum
        // Ported from https://github.com/alexgorbatchev/node-crc
        var crc = 0x0000;
        var count = bytes.Length;
        var i = 0;
        int code;

        while (count > 0)
        {
            code = (int)(uint)crc >> (8 & 0xFF);
            code ^= bytes[i++] & 0xFF;
            code ^= (int)(uint)code >> 4;
            crc = (crc << 8) & 0xFFFF;
            crc ^= code;
            code = (code << 5) & 0xFFFF;
            crc ^= code;
            code = (code << 7) & 0xFFFF;
            crc ^= code;
            count--;
        }

        // little-endian
        return
        [
            (byte)crc,
            (byte)((uint)crc >> 8),
        ];
    }

    private static bool IsValid(VersionByte versionByte, string encoded)
    {
        try
        {
            DecodeCheck(versionByte, encoded);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryDecode(VersionByte versionByte, string publicKey, out byte[] decoded)
    {
        decoded = null;
        try
        {
            decoded = DecodeCheck(versionByte, publicKey);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidEd25519PublicKey(string publicKey)
    {
        return IsValid(VersionByte.ACCOUNT_ID, publicKey);
    }

    public static bool TryDecodeEd25519PublicKey(string publicKey, out byte[] decoded)
    {
        return TryDecode(VersionByte.ACCOUNT_ID, publicKey, out decoded);
    }

    public static bool IsValidMuxedAccount(string publicKey)
    {
        return IsValid(VersionByte.MUXED_ED25519, publicKey);
    }

    public static bool IsValidEd25519SecretSeed(string seed)
    {
        return IsValid(VersionByte.SEED, seed);
    }

    public static bool IsValidContractId(string contractId)
    {
        return IsValid(VersionByte.CONTRACT, contractId);
    }

    public static bool IsValidClaimableBalanceId(string claimableBalanceId)
    {
        return IsValid(VersionByte.CLAIMABLE_BALANCE, claimableBalanceId);
    }

    public static bool IsValidLiquidityPoolId(string liquidityPoolId)
    {
        return IsValid(VersionByte.LIQUIDITY_POOL, liquidityPoolId);
    }

    private static byte[] DecodingTable()
    {
        var table = new byte[128];
        Array.Fill(table, (byte)0xff);
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        for (var i = 0; i < alphabet.Length; i++)
        {
            table[alphabet[i]] = (byte)i;
        }
        return table;
    }
}