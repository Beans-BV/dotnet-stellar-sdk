using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Memos;
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
        var senderAccount = await LoadAccountAsync(sender);
        var tx = new TransactionBuilder(senderAccount)
            .AddOperation(new PaymentOperation(destination, new AssetTypeNative(), "1"))
            .Build(); // no memo -> Memo.None()

        var act = async () => await Server.CheckMemoRequired(tx);
        await act.Should().ThrowAsync<AccountRequiresMemoException>();
    }

    [Test]
    public async Task CheckMemoRequired_WhenExemptionApplies_DoesNotThrow()
    {
        // Marker destination: requires a memo per SEP-29 — but both transactions below are exempt.
        var destination = await CreateFundedAccountAsync();
        await SubmitAsync(destination, new ManageDataOperation("config.memo_required", "1"));
        var sender = await CreateFundedAccountAsync();
        // Sequence numbers don't matter here: these transactions are only checked, never submitted.
        var senderAccount = await LoadAccountAsync(sender);

        // Exemption 1: the transaction already carries a memo.
        var withMemo = new TransactionBuilder(senderAccount)
            .AddOperation(new PaymentOperation(destination, new AssetTypeNative(), "1"))
            .AddMemo(Memo.Text("order-42"))
            .Build();
        var actWithMemo = async () => await Server.CheckMemoRequired(withMemo);
        await actWithMemo.Should().NotThrowAsync("a transaction that already has a memo is exempt");

        // Exemption 2: a muxed (M...) destination already encodes the memo id.
        var muxed = new MuxedAccountMed25519(destination, 42);
        var toMuxed = new TransactionBuilder(senderAccount)
            .AddOperation(new PaymentOperation(muxed, new AssetTypeNative(), "1"))
            .Build();
        var actToMuxed = async () => await Server.CheckMemoRequired(toMuxed);
        await actToMuxed.Should().NotThrowAsync("a muxed destination already encodes the memo id");
    }

    [Test]
    public async Task CheckMemoRequired_WithFeeBump_ChecksInnerTransaction()
    {
        var destination = await CreateFundedAccountAsync();
        await SubmitAsync(destination, new ManageDataOperation("config.memo_required", "1"));
        var sender = await CreateFundedAccountAsync();
        var senderAccount = await LoadAccountAsync(sender);

        var inner = new TransactionBuilder(senderAccount)
            .AddOperation(new PaymentOperation(destination, new AssetTypeNative(), "1"))
            .Build(); // no memo -> Memo.None()
        inner.Sign(sender);
        var feeBump = TransactionBuilder.BuildFeeBumpTransaction(sender, inner);

        var act = async () => await Server.CheckMemoRequired(feeBump);
        await act.Should().ThrowAsync<AccountRequiresMemoException>(
            "the check should unwrap the fee-bump and inspect the inner transaction's destinations");
    }

    [Test]
    public async Task CheckMemoRequired_WhenDestinationHasNoMarker_DoesNotThrow()
    {
        var destination = await CreateFundedAccountAsync();
        var sender = await CreateFundedAccountAsync();
        var senderAccount = await LoadAccountAsync(sender);
        var tx = new TransactionBuilder(senderAccount)
            .AddOperation(new PaymentOperation(destination, new AssetTypeNative(), "1"))
            .Build();

        var act = async () => await Server.CheckMemoRequired(tx);
        await act.Should().NotThrowAsync();
    }
}