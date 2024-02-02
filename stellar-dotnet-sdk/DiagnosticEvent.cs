namespace stellar_dotnet_sdk;

public class DiagnosticEvent
{
    public bool InSuccessfulContractCall { get; set; }

    public ContractEvent Event { get; set; }

    public xdr.DiagnosticEvent ToXdr()
    {
        return new xdr.DiagnosticEvent
        {
            Event = Event.ToXdr(),
            InSuccessfulContractCall = InSuccessfulContractCall
        };
    }

    public static DiagnosticEvent FromXdr(xdr.DiagnosticEvent xdrEvent)
    {
        return new DiagnosticEvent
        {
            InSuccessfulContractCall = xdrEvent.InSuccessfulContractCall,
            Event = ContractEvent.FromXdr(xdrEvent.Event)
        };
    }

    public string ToString()
    {
        return Event.ToString();
    }
}