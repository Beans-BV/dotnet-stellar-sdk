using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.IntegrationTests.Soroban;

[TestFixture]
[CancelAfter(240_000)]
public class InvokeHostFunctionTests : SorobanIntegrationTestBase
{
    [Test]
    public async Task InvokeContract_HelloWorld_ReturnsGreeting()
    {
        var account = await CreateFundedAccountAsync();
        var contractId = await DeployHelloWorldAsync(account);

        var arg = new SCSymbol("integration");
        var invoke = new InvokeContractOperation(contractId, "hello", [arg]);
        var result = await RunSorobanAsync(account, invoke);

        // hello(to) returns a vector of symbols: ["Hello", to].
        var vec = result.ResultValue.Should().BeOfType<SCVec>().Subject;
        var words = vec.InnerValue.Cast<SCSymbol>().Select(s => s.InnerValue).ToArray();
        words.Should().Equal("Hello", "integration");
    }
}