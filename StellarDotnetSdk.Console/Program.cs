using System.Threading.Tasks;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Console;

public static class Program
{
    //For testing use the following account info, this only exists on test network and may be wiped at any time...
    //Public: GAZHWW2NBPDVJ6PEEOZ2X43QV5JUDYS3XN4OWOTBR6WUACTUML2CCJLI
    //Secret: SCD74D46TJYXOUXFC5YOA72UTPCCVHK2GRSLKSPRB66VK6UJHQX2Y3R3
    
    public static async Task Main(string[] args)
    {
        using (var server = new Server("https://horizon.stellar.org"))
                        {
            System.Console.WriteLine("-- Streaming All New Ledgers On The Network --");
            await server.Ledgers
                .Cursor("now")
                .Stream((sender, response) => { ShowOperationResponse(server, sender, response); })
                .Connect();
                        }

        System.Console.ReadLine();
    }

    private static async void ShowOperationResponse(Server server, object sender, LedgerResponse lr)
    {
        var operationRequestBuilder = server.Operations.ForLedger(lr.Sequence);
        var operations = await operationRequestBuilder.Execute();

        var accts = 0;
        var payments = 0;
        var offers = 0;
        var options = 0;

        foreach (var op in operations.Records)
            switch (op.Type)
            {
                case "create_account":
                    accts++;
                    break;
                case "payment":
                    payments++;
                    break;
                case "manage_offer":
                    offers++;
                    break;
                case "set_options":
                    options++;
                    break;
            }

        System.Console.WriteLine(
            $"id: {lr.Sequence}, tx/ops: {lr.SuccessfulTransactionCount + "/" + lr.OperationCount}, accts: {accts}, payments: {payments}, offers: {offers}, options: {options}");
        System.Console.WriteLine($"Uri: {((LedgersRequestBuilder)sender).Uri}");
    }
}