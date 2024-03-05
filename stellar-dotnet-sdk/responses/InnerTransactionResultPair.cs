using System;
using Newtonsoft.Json;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk.responses;

[JsonObject]
public class InnerTransactionResultPair
{
    public InnerTransactionResultPair(string transactionHash, TransactionResult result)
    {
        TransactionHash = transactionHash;
        Result = result;
    }

    private InnerTransactionResultPair()
    {
    }

    public string TransactionHash { get; set; }

    public TransactionResult Result { get; set; }

    public static InnerTransactionResultPair FromXdr(string encoded)
    {
        var bytes = Convert.FromBase64String(encoded);
        var result = xdr.InnerTransactionResultPair.Decode(new XdrDataInputStream(bytes));
        return FromXdr(result);
    }

    public static InnerTransactionResultPair FromXdr(xdr.InnerTransactionResultPair result)
    {
        var writer = new XdrDataOutputStream();
        InnerTransactionResult.Encode(writer, result.Result);
        var xdrTransaction = Convert.ToBase64String(writer.ToArray());

        return new InnerTransactionResultPair
        {
            TransactionHash = Convert.ToBase64String(result.TransactionHash.InnerValue),
            Result = TransactionResult.FromXdrBase64(xdrTransaction)
        };
    }
}