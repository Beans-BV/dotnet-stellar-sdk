using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0009;

/// <summary>
///     Financial account information for KYC verification.
///     Contains bank account, mobile money, and cryptocurrency account details
///     for receiving or sending payments.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0009.md">SEP-0009</a>
/// </summary>
public sealed record FinancialAccountKycFields
{
    /// <summary>
    ///     Field key for the <c>bank_name</c> field as defined in SEP-0009.
    /// </summary>
    public const string BankNameFieldKey = "bank_name";

    /// <summary>
    ///     Field key for the <c>bank_account_type</c> field (e.g., checking or savings) as defined in SEP-0009.
    /// </summary>
    public const string BankAccountTypeFieldKey = "bank_account_type";

    /// <summary>
    ///     Field key for the <c>bank_account_number</c> field as defined in SEP-0009.
    /// </summary>
    public const string BankAccountNumberFieldKey = "bank_account_number";

    /// <summary>
    ///     Field key for the <c>bank_number</c> field (routing number in the US) as defined in SEP-0009.
    /// </summary>
    public const string BankNumberFieldKey = "bank_number";

    /// <summary>
    ///     Field key for the <c>bank_phone_number</c> field as defined in SEP-0009.
    /// </summary>
    public const string BankPhoneNumberFieldKey = "bank_phone_number";

    /// <summary>
    ///     Field key for the <c>bank_branch_number</c> field as defined in SEP-0009.
    /// </summary>
    public const string BankBranchNumberFieldKey = "bank_branch_number";

    /// <summary>
    ///     Field key for the <c>external_transfer_memo</c> field (destination tag/memo) as defined in SEP-0009.
    /// </summary>
    public const string ExternalTransferMemoFieldKey = "external_transfer_memo";

    /// <summary>
    ///     Field key for the <c>clabe_number</c> field (Mexico bank account number) as defined in SEP-0009.
    /// </summary>
    public const string ClabeNumberFieldKey = "clabe_number";

    /// <summary>
    ///     Field key for the <c>cbu_number</c> field (CBU/CVU for Argentina) as defined in SEP-0009.
    /// </summary>
    public const string CbuNumberFieldKey = "cbu_number";

    /// <summary>
    ///     Field key for the <c>cbu_alias</c> field (alias for CBU/CVU) as defined in SEP-0009.
    /// </summary>
    public const string CbuAliasFieldKey = "cbu_alias";

    /// <summary>
    ///     Field key for the <c>mobile_money_number</c> field (E.164 format) as defined in SEP-0009.
    /// </summary>
    public const string MobileMoneyNumberFieldKey = "mobile_money_number";

    /// <summary>
    ///     Field key for the <c>mobile_money_provider</c> field as defined in SEP-0009.
    /// </summary>
    public const string MobileMoneyProviderFieldKey = "mobile_money_provider";

    /// <summary>
    ///     Field key for the <c>crypto_address</c> field as defined in SEP-0009.
    /// </summary>
    public const string CryptoAddressFieldKey = "crypto_address";

    /// <summary>
    ///     Field key for the <c>crypto_memo</c> field as defined in SEP-0009. Deprecated: use <c>external_transfer_memo</c>.
    /// </summary>
    public const string CryptoMemoFieldKey = "crypto_memo";

    /// <summary>
    ///     Name of the bank. May be necessary in regions that don't have a unified routing system.
    /// </summary>
    public string? BankName { get; init; }

    /// <summary>
    ///     Type of bank account (e.g., checking or savings)
    /// </summary>
    public string? BankAccountType { get; init; }

    /// <summary>
    ///     Number identifying bank account
    /// </summary>
    public string? BankAccountNumber { get; init; }

    /// <summary>
    ///     Number identifying bank in national banking system (routing number in US)
    /// </summary>
    public string? BankNumber { get; init; }

    /// <summary>
    ///     Phone number with country code for bank
    /// </summary>
    public string? BankPhoneNumber { get; init; }

    /// <summary>
    ///     Number identifying bank branch
    /// </summary>
    public string? BankBranchNumber { get; init; }

    /// <summary>
    ///     A destination tag/memo used to identify a transaction
    /// </summary>
    public string? ExternalTransferMemo { get; init; }

    /// <summary>
    ///     Bank account number for Mexico
    /// </summary>
    public string? ClabeNumber { get; init; }

    /// <summary>
    ///     Clave Bancaria Uniforme (CBU) or Clave Virtual Uniforme (CVU).
    /// </summary>
    public string? CbuNumber { get; init; }

    /// <summary>
    ///     The alias for a Clave Bancaria Uniforme (CBU) or Clave Virtual Uniforme (CVU).
    /// </summary>
    public string? CbuAlias { get; init; }

    /// <summary>
    ///     Mobile phone number in E.164 format with which a mobile money account is associated.
    ///     Note that this number may be distinct from the same customer's mobile_number.
    /// </summary>
    public string? MobileMoneyNumber { get; init; }

    /// <summary>
    ///     Name of the mobile money service provider.
    /// </summary>
    public string? MobileMoneyProvider { get; init; }

    /// <summary>
    ///     Address for a cryptocurrency account
    /// </summary>
    public string? CryptoAddress { get; init; }

    /// <summary>
    ///     A destination tag/memo used to identify a transaction.
    ///     Deprecated: Use <see cref="ExternalTransferMemo" /> instead.
    /// </summary>
    public string? CryptoMemo { get; init; }

    /// <summary>
    ///     Converts all financial account KYC fields to a map for SEP-9 submission with optional key prefix.
    /// </summary>
    /// <param name="keyPrefix">Optional prefix to prepend to all field keys (e.g., "organization.")</param>
    /// <returns>Dictionary containing all non-null field values</returns>
    internal IReadOnlyDictionary<string, string> GetFields(string keyPrefix = "")
    {
        var result = new Dictionary<string, string>();
        if (BankName is not null)
        {
            result[keyPrefix + BankNameFieldKey] = BankName;
        }
        if (BankAccountType is not null)
        {
            result[keyPrefix + BankAccountTypeFieldKey] = BankAccountType;
        }
        if (BankAccountNumber is not null)
        {
            result[keyPrefix + BankAccountNumberFieldKey] = BankAccountNumber;
        }
        if (BankNumber is not null)
        {
            result[keyPrefix + BankNumberFieldKey] = BankNumber;
        }
        if (BankPhoneNumber is not null)
        {
            result[keyPrefix + BankPhoneNumberFieldKey] = BankPhoneNumber;
        }
        if (BankBranchNumber is not null)
        {
            result[keyPrefix + BankBranchNumberFieldKey] = BankBranchNumber;
        }
        if (ExternalTransferMemo is not null)
        {
            result[keyPrefix + ExternalTransferMemoFieldKey] = ExternalTransferMemo;
        }
        if (ClabeNumber is not null)
        {
            result[keyPrefix + ClabeNumberFieldKey] = ClabeNumber;
        }
        if (CbuNumber is not null)
        {
            result[keyPrefix + CbuNumberFieldKey] = CbuNumber;
        }
        if (CbuAlias is not null)
        {
            result[keyPrefix + CbuAliasFieldKey] = CbuAlias;
        }
        if (MobileMoneyNumber is not null)
        {
            result[keyPrefix + MobileMoneyNumberFieldKey] = MobileMoneyNumber;
        }
        if (MobileMoneyProvider is not null)
        {
            result[keyPrefix + MobileMoneyProviderFieldKey] = MobileMoneyProvider;
        }
        if (CryptoAddress is not null)
        {
            result[keyPrefix + CryptoAddressFieldKey] = CryptoAddress;
        }
        if (CryptoMemo is not null)
        {
            result[keyPrefix + CryptoMemoFieldKey] = CryptoMemo;
        }
        return result;
    }
}