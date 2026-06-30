using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.IntegrationTests.Horizon;

[TestFixture]
[CancelAfter(60_000)]
public class PaymentsRequestBuilderTests : OperationsTestBase
{
    [Test]
    public async Task Payments_ForAccount_ReturnsNativePayment()
    {
        var source = await CreateFundedAccountAsync();
        var destination = await CreateFundedAccountAsync();
        await SubmitAsync(source, new PaymentOperation(destination, new AssetTypeNative(), "5"));

        var page = await Server.Payments.ForAccount(destination.AccountId).Execute();

        var payment = page.Records
            .OfType<PaymentOperationResponse>()
            .Single(p => p.From == source.AccountId);
        payment.To.Should().Be(destination.AccountId);
        payment.AssetType.Should().Be("native");
        decimal.Parse(payment.Amount, CultureInfo.InvariantCulture).Should().Be(5m);
    }
}