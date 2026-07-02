using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.IntegrationTests.Horizon;

[TestFixture]
[CancelAfter(90_000)]
public class TransactionsRequestBuilderTests : OperationsTestBase
{
    [Test]
    public async Task Transaction_ByHash_ReturnsSubmittedTransaction()
    {
        var source = await CreateFundedAccountAsync();
        var destination = await CreateFundedAccountAsync();

        var submitted = await SubmitAsync(source, new PaymentOperation(destination, new AssetTypeNative(), "1"));

        submitted.Hash.Should().NotBeNull();
        var fetched = await Server.Transactions.Transaction(submitted.Hash!);
        fetched.Hash.Should().Be(submitted.Hash);
        fetched.Successful.Should().BeTrue();
    }

    [Test]
    public async Task Transactions_ForAccount_PaginatesWithLimitAndNextPage()
    {
        var source = await CreateFundedAccountAsync();
        var destination = await CreateFundedAccountAsync();

        // Friendbot create_account + these two payments => at least 3 transactions on `source`.
        await SubmitAsync(source, new PaymentOperation(destination, new AssetTypeNative(), "1"));
        await SubmitAsync(source, new PaymentOperation(destination, new AssetTypeNative(), "1"));

        var firstPage = await Server.Transactions
            .ForAccount(source.AccountId)
            .Order(OrderDirection.ASC)
            .Limit(1)
            .Execute();
        firstPage.Records.Should().HaveCount(1);

        var nextPage = await firstPage.NextPage()!;
        nextPage.Records.Should().NotBeEmpty();
        nextPage.Records[0].Hash.Should().NotBe(firstPage.Records[0].Hash);
    }
}