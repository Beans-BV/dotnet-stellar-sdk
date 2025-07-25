// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  enum TransactionEventStage {
//      // The event has happened before any one of the transactions has its 
//      // operations applied.
//      TRANSACTION_EVENT_STAGE_BEFORE_ALL_TXS = 0,
//      // The event has happened immediately after operations of the transaction
//      // have been applied.
//      TRANSACTION_EVENT_STAGE_AFTER_TX = 1,
//      // The event has happened after every transaction had its operations 
//      // applied.
//      TRANSACTION_EVENT_STAGE_AFTER_ALL_TXS = 2
//  };

//  ===========================================================================
public class TransactionEventStage
{
    public enum TransactionEventStageEnum
    {
        TRANSACTION_EVENT_STAGE_BEFORE_ALL_TXS = 0,
        TRANSACTION_EVENT_STAGE_AFTER_TX = 1,
        TRANSACTION_EVENT_STAGE_AFTER_ALL_TXS = 2,
    }

    public TransactionEventStageEnum InnerValue { get; set; }

    public static TransactionEventStage Create(TransactionEventStageEnum v)
    {
        return new TransactionEventStage
        {
            InnerValue = v,
        };
    }

    public static TransactionEventStage Decode(XdrDataInputStream stream)
    {
        var value = stream.ReadInt();
        switch (value)
        {
            case 0: return Create(TransactionEventStageEnum.TRANSACTION_EVENT_STAGE_BEFORE_ALL_TXS);
            case 1: return Create(TransactionEventStageEnum.TRANSACTION_EVENT_STAGE_AFTER_TX);
            case 2: return Create(TransactionEventStageEnum.TRANSACTION_EVENT_STAGE_AFTER_ALL_TXS);
            default:
                throw new Exception("Unknown enum value: " + value);
        }
    }

    public static void Encode(XdrDataOutputStream stream, TransactionEventStage value)
    {
        stream.WriteInt((int)value.InnerValue);
    }
}