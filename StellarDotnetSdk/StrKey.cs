using System;
using System.Collections.Generic;
using System.Linq;
using FormatException = StellarDotnetSdk.Exceptions.FormatException;
using xdrSDK = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk;

using MuxedAccount = xdrSDK.MuxedAccount;

public class StrKey
{
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

    private static readonly byte[] Base32LookupTable = DecodingTable();

    /// <summary>
    ///     Encodes raw bytes to an Ed25519 public key.
    /// </summary>
    /// <param name="data">A raw byte array.</param>
    /// <returns>Base32-encoded representation (G...) of the provided data.</returns>
    public static string EncodeEd25519PublicKey(byte[] data)
    {
        return EncodeCheck(VersionByte.ACCOUNT_ID, data);
    }

    [Obsolete("Deprecated. Use EncodeEd25519PublicKey instead.")]
    public static string EncodeStellarAccountId(byte[] data)
    {
        return EncodeCheck(VersionByte.ACCOUNT_ID, data);
    }

    /// <summary>
    ///     Encodes raw bytes to a Muxed_Ed25519 public key.
    /// </summary>
    /// <param name="data">A raw byte array.</param>
    /// <returns>Base32-encoded representation (M...) of the provided data.</returns>
    public static string EncodeMed25519PublicKey(byte[] data)
    {
        return EncodeCheck(VersionByte.MUXED_ED25519, data);
    }

    /// <summary>
    ///     Encodes raw bytes to an Ed25519 seed.
    /// </summary>
    /// <param name="data">A raw byte array.</param>
    /// <returns>Base32-encoded representation (S...) of the provided data.</returns>
    public static string EncodeEd25519SecretSeed(byte[] data)
    {
        return EncodeCheck(VersionByte.SEED, data);
    }

    /// <summary>
    ///     Encodes raw bytes to a PreAuthTx string.
    /// </summary>
    /// <param name="data">A raw byte array.</param>
    /// <returns>Base32-encoded representation (T...) of the provided data.</returns>
    public static string EncodePreAuthTx(byte[] data)
    {
        return EncodeCheck(VersionByte.PRE_AUTH_TX, data);
    }

    /// <summary>
    ///     Encodes raw bytes to an SHA256 hash.
    /// </summary>
    /// <param name="data">A raw byte array.</param>
    /// <returns>Base32-encoded representation (X...) of the provided data.</returns>
    public static string EncodeSha256Hash(byte[] data)
    {
        return EncodeCheck(VersionByte.SHA256_HASH, data);
    }

    [Obsolete("Deprecated. Use EncodeEd25519SecretSeed instead.")]
    public static string EncodeStellarSecretSeed(byte[] data)
    {
        return EncodeCheck(VersionByte.SEED, data);
    }

    /// <summary>
    ///     Encodes raw bytes to a signed payload.
    /// </summary>
    /// <param name="signedPayloadSigner">A signed payload.</param>
    /// <returns>Base32-encoded representation (P...) of the provided payload.</returns>
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

    /// <summary>
    ///     Encodes raw bytes to a contract ID.
    /// </summary>
    /// <param name="data">A raw byte array.</param>
    /// <returns>Base32-encoded representation (C...) of the provided data.</returns>
    public static string EncodeContractId(byte[] data)
    {
        return EncodeCheck(VersionByte.CONTRACT, data);
    }

    /// <summary>
    ///     Encodes raw bytes to a liquidity pool ID.
    /// </summary>
    /// <param name="data">A raw byte array.</param>
    /// <returns>Base32-encoded representation (L...) of the provided key.</returns>
    public static string EncodeLiquidityPoolId(byte[] data)
    {
        return EncodeCheck(VersionByte.LIQUIDITY_POOL, data);
    }

    /// <summary>
    ///     Encodes raw bytes to a claimable balance ID.
    /// </summary>
    /// <param name="data">A raw byte array.</param>
    /// <returns>Base32-encoded representation (B...) of the provided key.</returns>
    public static string EncodeClaimableBalanceId(byte[] data)
    {
        return EncodeCheck(VersionByte.CLAIMABLE_BALANCE, data);
    }

    [Obsolete("Deprecated. Use EncodeMed25519PublicKey instead.")]
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
                throw new ArgumentException("Invalid discriminant");
        }
    }

    public static bool TryDecodeEd25519PublicKey(string publicKey, out byte[] decoded)
    {
        return TryDecode(VersionByte.ACCOUNT_ID, publicKey, out decoded);
    }

    public static bool TryDecodeMed25519PublicKey(string publicKey, out byte[] decoded)
    {
        return TryDecode(VersionByte.MUXED_ED25519, publicKey, out decoded);
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

    /// <summary>
    ///     Decodes a Stellar Ed25519 public key to raw bytes.
    /// </summary>
    /// <param name="publicKey">A base32-encoded Ed25519 public key (G...).</param>
    /// <returns>Raw bytes of the provided Ed25519 public key.</returns>
    public static byte[] DecodeEd25519PublicKey(string publicKey)
    {
        return DecodeCheck(VersionByte.ACCOUNT_ID, publicKey);
    }

    [Obsolete("Deprecated. Use DecodeEd25519PublicKey instead.")]
    public static byte[] DecodeStellarAccountId(string data)
    {
        return DecodeCheck(VersionByte.ACCOUNT_ID, data);
    }

    /// <summary>
    ///     Decodes a Stellar Muxed_Ed25519 public key to raw bytes.
    /// </summary>
    /// <param name="publicKey">A base32-encoded Muxed_Ed25519 public key (M...).</param>
    /// <returns>Raw bytes of the provided Muxed_Ed25519 public key.</returns>
    public static byte[] DecodeMed25519PublicKey(string publicKey)
    {
        return DecodeCheck(VersionByte.MUXED_ED25519, publicKey);
    }

    /// <summary>
    ///     Decodes an Ed25519 seed string to raw bytes.
    /// </summary>
    /// <param name="data">A base32-encoded Ed25519 seed (S...).</param>
    /// <returns>Raw bytes of the provided Ed25519 seed.</returns>
    public static byte[] DecodeEd25519SecretSeed(string data)
    {
        return DecodeCheck(VersionByte.SEED, data);
    }

    [Obsolete("Deprecated. Use DecodeEd25519SecretSeed instead.")]
    public static byte[] DecodeStellarSecretSeed(string data)
    {
        return DecodeCheck(VersionByte.SEED, data);
    }

    /// <summary>
    ///     Decodes a PreAuthTx to raw bytes.
    /// </summary>
    /// <param name="data">A base32-encoded PreAuthTx string (T...).</param>
    /// <returns>Raw bytes of the provided PreAuthTx string.</returns>
    public static byte[] DecodePreAuthTx(string data)
    {
        return DecodeCheck(VersionByte.PRE_AUTH_TX, data);
    }

    /// <summary>
    ///     Decodes an SHA256 hash to raw bytes.
    /// </summary>
    /// <param name="data">A base32-encoded SHA-256 hash (X...).</param>
    /// <returns>Raw bytes of the provided SHA-256 hash.</returns>
    public static byte[] DecodeSha256Hash(string data)
    {
        return DecodeCheck(VersionByte.SHA256_HASH, data);
    }

    /// <summary>
    ///     Decodes a signed payload to raw bytes.
    /// </summary>
    /// <param name="data">A base32-encoded signed payload (P...).</param>
    /// <returns>Raw bytes of the provided signed payload.</returns>
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

    /// <summary>
    ///     Decodes contract ID to raw bytes.
    /// </summary>
    /// <param name="data">A base32-encoded contract ID (C...).</param>
    /// <returns>Raw bytes of the provided contract ID.</returns>
    public static byte[] DecodeContractId(string data)
    {
        return DecodeCheck(VersionByte.CONTRACT, data);
    }

    /// <summary>
    ///     Decodes liquidity pool ID to raw bytes.
    /// </summary>
    /// <param name="data">A base32-encoded liquidity pool ID (L...).</param>
    /// <returns>Raw bytes of the provided liquidity pool ID.</returns>
    public static byte[] DecodeLiquidityPoolId(string data)
    {
        return DecodeCheck(VersionByte.LIQUIDITY_POOL, data);
    }

    /// <summary>
    ///     Decodes claimable balance ID to raw bytes.
    /// </summary>
    /// <param name="data">A base32-encoded claimable balance ID (B...).</param>
    /// <returns>Raw bytes of the provided claimable balance ID.</returns>
    public static byte[] DecodeClaimableBalanceId(string data)
    {
        return DecodeCheck(VersionByte.CLAIMABLE_BALANCE, data);
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
                    muxed.Ed25519 = xdrSDK.Uint256.Decode(new xdrSDK.XdrDataInputStream(DecodeEd25519PublicKey(data)));
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

    /// <summary>
    ///     Checks validity of an Ed25519 public key.
    /// </summary>
    /// <param name="publicKey">The public key (G...) to check.</param>
    /// <returns>True if the given public key is a valid Stellar account ID, false otherwise.</returns>
    public static bool IsValidEd25519PublicKey(string publicKey)
    {
        return IsValid(VersionByte.ACCOUNT_ID, publicKey);
    }

    /// <summary>
    ///     Checks validity of a Muxed_Ed25519 public key.
    /// </summary>
    /// <param name="publicKey">The public key (M...) to check.</param>
    /// <returns>True if the given public key is a valid Stellar muxed account ID, false otherwise.</returns>
    public static bool IsValidMed25519PublicKey(string publicKey)
    {
        return IsValid(VersionByte.MUXED_ED25519, publicKey);
    }

    /// <summary>
    ///     Checks validity of an Ed25519 seed.
    /// </summary>
    /// <param name="seed">The secret seed (S...) to check.</param>
    /// <returns>True if the given seed is a valid Stellar secret seed, false otherwise.</returns>
    public static bool IsValidEd25519SecretSeed(string seed)
    {
        return IsValid(VersionByte.SEED, seed);
    }

    /// <summary>
    ///     Checks validity of a contract ID.
    /// </summary>
    /// <param name="contractId">The contract ID (C...) to check.</param>
    /// <returns>True if the given contract ID is valid, false otherwise.</returns>
    public static bool IsValidContractId(string contractId)
    {
        return IsValid(VersionByte.CONTRACT, contractId);
    }

    /// <summary>
    ///     Checks validity of a liquidity pool ID.
    /// </summary>
    /// <param name="liquidityPoolId">The liquidity pool ID (L...) to check.</param>
    /// <returns>True if the given liquidity pool ID is valid, false otherwise.</returns>
    public static bool IsValidLiquidityPoolId(string liquidityPoolId)
    {
        return IsValid(VersionByte.LIQUIDITY_POOL, liquidityPoolId);
    }

    /// <summary>
    ///     Checks validity of a claimable balance ID.
    /// </summary>
    /// <param name="claimableBalanceId">The claimable balance ID (B...) to check.</param>
    /// <returns>True if the given claimable balance ID is valid, false otherwise.</returns>
    public static bool IsValidClaimableBalanceId(string claimableBalanceId)
    {
        return IsValid(VersionByte.CLAIMABLE_BALANCE, claimableBalanceId);
    }

    [Obsolete("Deprecated. Use IsValidMed25519PublicKey instead.")]
    public static bool IsValidMuxedAccount(string publicKey)
    {
        return IsValid(VersionByte.MUXED_ED25519, publicKey);
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

    private static bool TryDecode(VersionByte versionByte, string publicKey, out byte[] decoded)
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