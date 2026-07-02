using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;

namespace StellarDotnetSdk.IntegrationTests.Operations;

[TestFixture]
[CancelAfter(60_000)]
public class SetOptionsOperationTests : OperationsTestBase
{
    [Test]
    public async Task SetOptions_SetsHomeDomainThresholdsAndFlags()
    {
        var account = await CreateFundedAccountAsync();

        var operation = new SetOptionsOperation()
            .SetHomeDomain("integration.example.com")
            .SetLowThreshold(1)
            .SetMediumThreshold(2)
            .SetHighThreshold(3)
            .SetSetFlags(3); // AUTH_REQUIRED | AUTH_REVOCABLE
        await SubmitAsync(account, operation);

        var loaded = await LoadAccountAsync(account);
        loaded.HomeDomain.Should().Be("integration.example.com");
        loaded.Thresholds.LowThreshold.Should().Be(1);
        loaded.Thresholds.MedThreshold.Should().Be(2);
        loaded.Thresholds.HighThreshold.Should().Be(3);
        loaded.Flags.AuthRequired.Should().BeTrue();
        loaded.Flags.AuthRevocable.Should().BeTrue();
    }

    [Test]
    public async Task SetOptions_AddsAdditionalSigner()
    {
        var account = await CreateFundedAccountAsync();
        var extraSigner = KeyPair.Random();

        await SubmitAsync(account, new SetOptionsOperation().SetSigner(extraSigner.AccountId, 1));

        var loaded = await LoadAccountAsync(account);
        loaded.Signers.Should().Contain(s => s.Key == extraSigner.AccountId && s.Weight == 1);
    }

    [Test]
    public async Task SetOptions_ClearFlags_RemovesPreviouslySetFlag()
    {
        var account = await CreateFundedAccountAsync();

        // Set AUTH_REQUIRED | AUTH_REVOCABLE, then clear only AUTH_REVOCABLE.
        await SubmitAsync(account, new SetOptionsOperation().SetSetFlags(3));
        await SubmitAsync(account, new SetOptionsOperation().SetClearFlags(2));

        var loaded = await LoadAccountAsync(account);
        loaded.Flags.AuthRequired.Should().BeTrue("AUTH_REQUIRED was set and never cleared");
        loaded.Flags.AuthRevocable.Should().BeFalse("AUTH_REVOCABLE was cleared");
    }
}