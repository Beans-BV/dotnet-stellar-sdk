using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.IntegrationTests.Horizon;

[TestFixture]
[CancelAfter(120_000)]
public class SubmitTransactionTests : IntegrationTestBase
{
    [Test]
    public async Task SubmitTransaction_WithSignedPayment_Succeeds()
    {
        var source = await CreateFundedAccountAsync();
        var destination = await CreateFundedAccountAsync();

        var tx = await BuildPaymentTransactionAsync(source, destination, "1");
        tx.Sign(source);

        var response = await Server.SubmitTransaction(tx);

        response.Should().NotBeNull();
        response!.IsSuccess.Should().BeTrue();
        response.Hash.Should().Be(Convert.ToHexString(tx.Hash()).ToLowerInvariant());
    }

    [Test]
    public async Task SubmitTransactionAsync_WithSignedPayment_ReturnsPendingThenResolves()
    {
        var source = await CreateFundedAccountAsync();
        var destination = await CreateFundedAccountAsync();

        var tx = await BuildPaymentTransactionAsync(source, destination, "1");
        tx.Sign(source);

        var response = await Server.SubmitTransactionAsync(tx);
        response.Should().NotBeNull();
        response!.TxStatus.Should().BeOneOf(
            SubmitTransactionAsyncResponse.TransactionStatus.PENDING,
            SubmitTransactionAsyncResponse.TransactionStatus.DUPLICATE);

        var hash = response.Hash;
        hash.Should().NotBeNullOrEmpty();

        // Poll up to 30s for the transaction to be included in a ledger.
        var deadline = DateTime.UtcNow.AddSeconds(30);
        TransactionResponse? included = null;
        while (DateTime.UtcNow < deadline && included is null)
        {
            try
            {
                included = await Server.Transactions.Transaction(hash!);
            }
            catch (HttpResponseException ex) when (ex.StatusCode == 404)
            {
                // Not yet in a ledger; back off briefly and retry.
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        included.Should().NotBeNull($"transaction {hash} did not appear in a ledger within 30s");
        included!.Successful.Should().BeTrue();
    }

    [Test]
    public async Task SubmitTransaction_WithFeeBump_Succeeds()
    {
        var source = await CreateFundedAccountAsync();
        var destination = await CreateFundedAccountAsync();
        var feeSource = await CreateFundedAccountAsync();

        var inner = await BuildPaymentTransactionAsync(source, destination, "1");
        inner.Sign(source);

        var feeBump = TransactionBuilder.BuildFeeBumpTransaction(feeSource, inner);
        feeBump.Sign(feeSource);

        var response = await Server.SubmitTransaction(feeBump);

        response.Should().NotBeNull();
        response!.IsSuccess.Should().BeTrue();
    }

    private async Task<Transaction> BuildPaymentTransactionAsync(KeyPair source, KeyPair destination, string amountXlm)
    {
        var sourceAccount = await Server.Accounts.Account(source.AccountId);
        var payment = new PaymentOperation(destination, new AssetTypeNative(), amountXlm);
        return new TransactionBuilder(sourceAccount)
            .AddOperation(payment)
            .Build();
    }
}