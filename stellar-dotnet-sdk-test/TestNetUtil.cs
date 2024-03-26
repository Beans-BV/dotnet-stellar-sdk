using System.Threading.Tasks;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.requests;
using stellar_dotnet_sdk.responses;

namespace stellar_dotnet_sdk_test;

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