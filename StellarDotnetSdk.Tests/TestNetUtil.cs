using System.Threading.Tasks;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests;

internal static class TestNetUtil
{
    public static async Task CheckAndCreateAccountOnTestnet(string accountId)
    {
        using Server server = new("https://horizon-testnet.stellar.org");
        try
        {
            await server.Accounts.Account(accountId);
        }
        catch (HttpResponseException)
        {
            bool isSuccess;
            do
            {
                var fundResponse = await server.TestNetFriendBot.FundAccount(accountId).Execute();
                var result = TransactionResult.FromXdrBase64(fundResponse.ResultXdr);
                isSuccess = result.IsSuccess && result is TransactionResultSuccess;
            } while (!isSuccess);
        }
    }
}