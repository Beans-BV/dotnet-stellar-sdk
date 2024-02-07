using System.Linq;

namespace stellar_dotnet_sdk;

public class SorobanTransactionMeta
{
    public ExtensionPoint ExtensionPoint { get; set; } = new ExtensionPointZero();

    public SCVal ReturnValue { get; set; }

    public ContractEvent[] Events { get; set; }

    public DiagnosticEvent[] DiagnosticEvents { get; set; }

    public xdr.SorobanTransactionMeta ToXdr()
    {
        return new xdr.SorobanTransactionMeta
        {
            Ext = ExtensionPoint.ToXdr(),
            Events = Events.Select(x => x.ToXdr()).ToArray(),
            ReturnValue = ReturnValue.ToXdr(),
            DiagnosticEvents = DiagnosticEvents.Select(x => x.ToXdr()).ToArray()
        };
    }

    public static SorobanTransactionMeta? FromXdr(xdr.SorobanTransactionMeta? xdr)
    {
        if (xdr == null) return null;
        return new SorobanTransactionMeta
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdr.Ext),
            ReturnValue = SCVal.FromXdr(xdr.ReturnValue),
            Events = xdr.Events.Select(ContractEvent.FromXdr).ToArray(),
            DiagnosticEvents = xdr.DiagnosticEvents.Select(DiagnosticEvent.FromXdr).ToArray()
        };
    }

    public string ToString()
    {
        var value = "";
        for (var i = 0; i < Events.Length; i++) value += $"Event {i + 1}\n:{Events[i].ToString()}\n";
        for (var i = 0; i < DiagnosticEvents.Length; i++)
            value += $"Diagnostic event {i + 1}:\n{DiagnosticEvents[i].ToString()}\n";
        return value;
    }
}