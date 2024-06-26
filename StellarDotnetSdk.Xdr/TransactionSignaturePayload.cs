// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct TransactionSignaturePayload
//  {
//      Hash networkId;
//      union switch (EnvelopeType type)
//      {
//      // Backwards Compatibility: Use ENVELOPE_TYPE_TX to sign ENVELOPE_TYPE_TX_V0
//      case ENVELOPE_TYPE_TX:
//          Transaction tx;
//      case ENVELOPE_TYPE_TX_FEE_BUMP:
//          FeeBumpTransaction feeBump;
//      }
//      taggedTransaction;
//  };

//  ===========================================================================
public class TransactionSignaturePayload
{
    public Hash NetworkId { get; set; }
    public TransactionSignaturePayloadTaggedTransaction TaggedTransaction { get; set; }

    public static void Encode(XdrDataOutputStream stream,
        TransactionSignaturePayload encodedTransactionSignaturePayload)
    {
        Hash.Encode(stream, encodedTransactionSignaturePayload.NetworkId);
        TransactionSignaturePayloadTaggedTransaction.Encode(stream,
            encodedTransactionSignaturePayload.TaggedTransaction);
    }

    public static TransactionSignaturePayload Decode(XdrDataInputStream stream)
    {
        var decodedTransactionSignaturePayload = new TransactionSignaturePayload();
        decodedTransactionSignaturePayload.NetworkId = Hash.Decode(stream);
        decodedTransactionSignaturePayload.TaggedTransaction =
            TransactionSignaturePayloadTaggedTransaction.Decode(stream);
        return decodedTransactionSignaturePayload;
    }

    public class TransactionSignaturePayloadTaggedTransaction
    {
        public EnvelopeType Discriminant { get; set; } = new();

        public Transaction Tx { get; set; }
        public FeeBumpTransaction FeeBump { get; set; }

        public static void Encode(XdrDataOutputStream stream,
            TransactionSignaturePayloadTaggedTransaction encodedTransactionSignaturePayloadTaggedTransaction)
        {
            stream.WriteInt((int)encodedTransactionSignaturePayloadTaggedTransaction.Discriminant.InnerValue);
            switch (encodedTransactionSignaturePayloadTaggedTransaction.Discriminant.InnerValue)
            {
                case EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX:
                    Transaction.Encode(stream, encodedTransactionSignaturePayloadTaggedTransaction.Tx);
                    break;
                case EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_FEE_BUMP:
                    FeeBumpTransaction.Encode(stream, encodedTransactionSignaturePayloadTaggedTransaction.FeeBump);
                    break;
            }
        }

        public static TransactionSignaturePayloadTaggedTransaction Decode(XdrDataInputStream stream)
        {
            var decodedTransactionSignaturePayloadTaggedTransaction =
                new TransactionSignaturePayloadTaggedTransaction();
            var discriminant = EnvelopeType.Decode(stream);
            decodedTransactionSignaturePayloadTaggedTransaction.Discriminant = discriminant;
            switch (decodedTransactionSignaturePayloadTaggedTransaction.Discriminant.InnerValue)
            {
                case EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX:
                    decodedTransactionSignaturePayloadTaggedTransaction.Tx = Transaction.Decode(stream);
                    break;
                case EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_FEE_BUMP:
                    decodedTransactionSignaturePayloadTaggedTransaction.FeeBump = FeeBumpTransaction.Decode(stream);
                    break;
            }

            return decodedTransactionSignaturePayloadTaggedTransaction;
        }
    }
}