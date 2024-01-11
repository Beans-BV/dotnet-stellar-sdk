using System;

namespace stellar_dotnet_sdk;

public class Hash
{
    public Hash(string hexString) : this(Convert.FromHexString(hexString))
    {
    }
    public Hash(byte[] hash)
    {
        if (hash.Length != 32)
            throw new ArgumentException("Hash must have exactly 32 bytes.", nameof(hash));
        
        InnerValue = hash;
    }

    public byte[] InnerValue { get; }

    public static Hash FromXdr(xdr.Hash xdrHash)
    {
        return new Hash(xdrHash.InnerValue);
    }

    public xdr.Hash ToXdr()
    {
        return new xdr.Hash(InnerValue);
    }
}