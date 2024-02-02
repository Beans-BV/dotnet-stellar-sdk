namespace stellar_dotnet_sdk.responses.sorobanrpc;

public class SendTransactionResponse
{
    public enum SendTransactionStatus
    {
        PENDING,
        DUPLICATE,
        TRY_AGAIN_LATER,
        ERROR
    }

    public string ErrorResultXdr;

    public string Hash;

    public long LatestLedger;

    public long LatestLedgerCloseTime;
    public SendTransactionStatus Status;
}