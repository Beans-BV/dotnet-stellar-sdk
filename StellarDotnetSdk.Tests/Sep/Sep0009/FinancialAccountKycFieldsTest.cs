using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Sep.Sep0009;

namespace StellarDotnetSdk.Tests.Sep.Sep0009;

/// <summary>
///     Tests for FinancialAccountKycFields class functionality.
/// </summary>
[TestClass]
public class FinancialAccountKycFieldsTest
{
    /// <summary>
    ///     Verifies that all properties can be set and retrieved correctly.
    /// </summary>
    [TestMethod]
    public void Properties_SetAndGet_WorkCorrectly()
    {
        // Arrange
        var fields = new FinancialAccountKycFields
        {
            BankName = "Test Bank",
            BankAccountType = "checking",
            BankAccountNumber = "1234567890",
            BankNumber = "987654321",
            BankPhoneNumber = "+14155552671",
            BankBranchNumber = "001",
            ExternalTransferMemo = "memo123",
            ClabeNumber = "032180000118359719",
            CbuNumber = "1234567890123456789012",
            CbuAlias = "alias123",
            MobileMoneyNumber = "+14155552672",
            MobileMoneyProvider = "M-Pesa",
            CryptoAddress = "GDJK...",
            CryptoMemo = "cryptomemo123",
        };

        // Assert
        fields.BankName.Should().Be("Test Bank");
        fields.BankAccountType.Should().Be("checking");
        fields.BankAccountNumber.Should().Be("1234567890");
        fields.BankNumber.Should().Be("987654321");
        fields.BankPhoneNumber.Should().Be("+14155552671");
        fields.BankBranchNumber.Should().Be("001");
        fields.ExternalTransferMemo.Should().Be("memo123");
        fields.ClabeNumber.Should().Be("032180000118359719");
        fields.CbuNumber.Should().Be("1234567890123456789012");
        fields.CbuAlias.Should().Be("alias123");
        fields.MobileMoneyNumber.Should().Be("+14155552672");
        fields.MobileMoneyProvider.Should().Be("M-Pesa");
        fields.CryptoAddress.Should().Be("GDJK...");
        fields.CryptoMemo.Should().Be("cryptomemo123");
    }

    /// <summary>
    ///     Verifies that GetFields returns empty dictionary when no fields are set.
    /// </summary>
    [TestMethod]
    public void GetFields_WithNoFieldsSet_ReturnsEmptyDictionary()
    {
        // Arrange
        var fields = new FinancialAccountKycFields();

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     Verifies that GetFields includes all set fields with correct keys.
    /// </summary>
    [TestMethod]
    public void GetFields_WithAllFieldsSet_ReturnsAllFields()
    {
        // Arrange
        var fields = new FinancialAccountKycFields
        {
            BankName = "Test Bank",
            BankAccountType = "savings",
            BankAccountNumber = "1234567890",
            BankNumber = "987654321",
            BankPhoneNumber = "+14155552671",
            BankBranchNumber = "001",
            ExternalTransferMemo = "memo123",
            ClabeNumber = "032180000118359719",
            CbuNumber = "1234567890123456789012",
            CbuAlias = "alias123",
            MobileMoneyNumber = "+14155552672",
            MobileMoneyProvider = "M-Pesa",
            CryptoAddress = "GDJK...",
            CryptoMemo = "cryptomemo123",
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().HaveCount(14);
        result.Should().ContainKey(FinancialAccountKycFields.BankNameFieldKey).WhoseValue.Should().Be("Test Bank");
        result.Should().ContainKey(FinancialAccountKycFields.BankAccountTypeFieldKey).WhoseValue.Should().Be("savings");
        result.Should().ContainKey(FinancialAccountKycFields.BankAccountNumberFieldKey).WhoseValue.Should()
            .Be("1234567890");
        result.Should().ContainKey(FinancialAccountKycFields.BankNumberFieldKey).WhoseValue.Should().Be("987654321");
        result.Should().ContainKey(FinancialAccountKycFields.BankPhoneNumberFieldKey).WhoseValue.Should()
            .Be("+14155552671");
        result.Should().ContainKey(FinancialAccountKycFields.BankBranchNumberFieldKey).WhoseValue.Should().Be("001");
        result.Should().ContainKey(FinancialAccountKycFields.ExternalTransferMemoFieldKey).WhoseValue.Should()
            .Be("memo123");
        result.Should().ContainKey(FinancialAccountKycFields.ClabeNumberFieldKey).WhoseValue.Should()
            .Be("032180000118359719");
        result.Should().ContainKey(FinancialAccountKycFields.CbuNumberFieldKey).WhoseValue.Should()
            .Be("1234567890123456789012");
        result.Should().ContainKey(FinancialAccountKycFields.CbuAliasFieldKey).WhoseValue.Should().Be("alias123");
        result.Should().ContainKey(FinancialAccountKycFields.MobileMoneyNumberFieldKey).WhoseValue.Should()
            .Be("+14155552672");
        result.Should().ContainKey(FinancialAccountKycFields.MobileMoneyProviderFieldKey).WhoseValue.Should()
            .Be("M-Pesa");
        result.Should().ContainKey(FinancialAccountKycFields.CryptoAddressFieldKey).WhoseValue.Should().Be("GDJK...");
        result.Should().ContainKey(FinancialAccountKycFields.CryptoMemoFieldKey).WhoseValue.Should()
            .Be("cryptomemo123");
    }

    /// <summary>
    ///     Verifies that GetFields only includes set fields, excluding null values.
    /// </summary>
    [TestMethod]
    public void GetFields_WithPartialFieldsSet_ReturnsOnlySetFields()
    {
        // Arrange
        var fields = new FinancialAccountKycFields
        {
            BankName = "Test Bank",
            BankAccountNumber = "1234567890",
            CryptoAddress = "GDJK...",
        };

        // Act
        var result = fields.GetFields();

        // Assert
        result.Should().HaveCount(3);
        result.Should().ContainKey(FinancialAccountKycFields.BankNameFieldKey);
        result.Should().ContainKey(FinancialAccountKycFields.BankAccountNumberFieldKey);
        result.Should().ContainKey(FinancialAccountKycFields.CryptoAddressFieldKey);
        result.Should().NotContainKey(FinancialAccountKycFields.BankAccountTypeFieldKey);
        result.Should().NotContainKey(FinancialAccountKycFields.BankNumberFieldKey);
    }

    /// <summary>
    ///     Verifies that GetFields correctly applies key prefix.
    /// </summary>
    [TestMethod]
    public void GetFields_WithKeyPrefix_PrefixesAllKeys()
    {
        // Arrange
        var fields = new FinancialAccountKycFields
        {
            BankName = "Test Bank",
            BankAccountNumber = "1234567890",
        };
        const string prefix = "organization.";

        // Act
        var result = fields.GetFields(prefix);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainKey(prefix + FinancialAccountKycFields.BankNameFieldKey);
        result.Should().ContainKey(prefix + FinancialAccountKycFields.BankAccountNumberFieldKey);
        result.Should().NotContainKey(FinancialAccountKycFields.BankNameFieldKey);
        result.Should().NotContainKey(FinancialAccountKycFields.BankAccountNumberFieldKey);
    }

    /// <summary>
    ///     Verifies that GetFields with empty prefix works the same as default.
    /// </summary>
    [TestMethod]
    public void GetFields_WithEmptyPrefix_WorksSameAsDefault()
    {
        // Arrange
        var fields = new FinancialAccountKycFields
        {
            BankName = "Test Bank",
            BankAccountNumber = "1234567890",
        };

        // Act
        var resultDefault = fields.GetFields();
        var resultEmpty = fields.GetFields();

        // Assert
        resultDefault.Should().BeEquivalentTo(resultEmpty);
    }

    /// <summary>
    ///     Verifies that field key constants match SEP-9 specification.
    /// </summary>
    [TestMethod]
    public void FieldKeyConstants_MatchSep9Specification()
    {
        // Assert
        FinancialAccountKycFields.BankNameFieldKey.Should().Be("bank_name");
        FinancialAccountKycFields.BankAccountTypeFieldKey.Should().Be("bank_account_type");
        FinancialAccountKycFields.BankAccountNumberFieldKey.Should().Be("bank_account_number");
        FinancialAccountKycFields.BankNumberFieldKey.Should().Be("bank_number");
        FinancialAccountKycFields.BankPhoneNumberFieldKey.Should().Be("bank_phone_number");
        FinancialAccountKycFields.BankBranchNumberFieldKey.Should().Be("bank_branch_number");
        FinancialAccountKycFields.ExternalTransferMemoFieldKey.Should().Be("external_transfer_memo");
        FinancialAccountKycFields.ClabeNumberFieldKey.Should().Be("clabe_number");
        FinancialAccountKycFields.CbuNumberFieldKey.Should().Be("cbu_number");
        FinancialAccountKycFields.CbuAliasFieldKey.Should().Be("cbu_alias");
        FinancialAccountKycFields.MobileMoneyNumberFieldKey.Should().Be("mobile_money_number");
        FinancialAccountKycFields.MobileMoneyProviderFieldKey.Should().Be("mobile_money_provider");
        FinancialAccountKycFields.CryptoAddressFieldKey.Should().Be("crypto_address");
        FinancialAccountKycFields.CryptoMemoFieldKey.Should().Be("crypto_memo");
    }
}