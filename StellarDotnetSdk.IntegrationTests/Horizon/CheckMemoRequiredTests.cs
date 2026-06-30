using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.IntegrationTests.Horizon;

[TestFixture]
[CancelAfter(60_000)]
public class CheckMemoRequiredTests : OperationsTestBase
{
    [Test]
    public async Task CheckMemoRequired_WhenDestinationRequiresMemo_Throws()
    {
        var destination = await CreateFundedAccountAsync();
        // SEP-29 marker: a "config.memo_required" data entry with value "1" (Horizon returns "MQ==").
        await SubmitAsync(destination, new ManageDataOperation("config.memo_required", "1"));

        var sender = await CreateFundedAccountAsync();
        var senderAccount = await Server.Accounts.Account(sender.AccountId);
        var tx = new TransactionBuilder(senderAccount)
            .AddOperation(new PaymentOperation(destination, new AssetTypeNative(), "1"))
            .Build(); // no memo -> Memo.None()

        var act = async () => await Server.CheckMemoRequired(tx);
        await act.Should().ThrowAsync<AccountRequiresMemoException>();
    }

    [Test]
    public async Task CheckMemoRequired_WhenDestinationHasNoMarker_DoesNotThrow()
    {
        var destination = await CreateFundedAccountAsync();
        var sender = await CreateFundedAccountAsync();
        var senderAccount = await Server.Accounts.Account(sender.AccountId);
        var tx = new TransactionBuilder(senderAccount)
            .AddOperation(new PaymentOperation(destination, new AssetTypeNative(), "1"))
            .Build();

        var act = async () => await Server.CheckMemoRequired(tx);
        await act.Should().NotThrowAsync();
    }
}