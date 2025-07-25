// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  union TransactionPhase switch (int v)
//  {
//  case 0:
//      TxSetComponent v0Components<>;
//  case 1:
//      ParallelTxsComponent parallelTxsComponent;
//  };

//  ===========================================================================
public class TransactionPhase
{
    public int Discriminant { get; set; }

    public TxSetComponent[] V0Components { get; set; }
    public ParallelTxsComponent ParallelTxsComponent { get; set; }

    public static void Encode(XdrDataOutputStream stream, TransactionPhase encodedTransactionPhase)
    {
        stream.WriteInt(encodedTransactionPhase.Discriminant);
        switch (encodedTransactionPhase.Discriminant)
        {
            case 0:
                var v0Componentssize = encodedTransactionPhase.V0Components.Length;
                stream.WriteInt(v0Componentssize);
                for (var i = 0; i < v0Componentssize; i++)
                {
                    TxSetComponent.Encode(stream, encodedTransactionPhase.V0Components[i]);
                }
                break;
            case 1:
                ParallelTxsComponent.Encode(stream, encodedTransactionPhase.ParallelTxsComponent);
                break;
        }
    }

    public static TransactionPhase Decode(XdrDataInputStream stream)
    {
        var decodedTransactionPhase = new TransactionPhase();
        var discriminant = stream.ReadInt();
        decodedTransactionPhase.Discriminant = discriminant;
        switch (decodedTransactionPhase.Discriminant)
        {
            case 0:
                var v0Componentssize = stream.ReadInt();
                decodedTransactionPhase.V0Components = new TxSetComponent[v0Componentssize];
                for (var i = 0; i < v0Componentssize; i++)
                {
                    decodedTransactionPhase.V0Components[i] = TxSetComponent.Decode(stream);
                }
                break;
            case 1:
                decodedTransactionPhase.ParallelTxsComponent = ParallelTxsComponent.Decode(stream);
                break;
        }
        return decodedTransactionPhase;
    }
}