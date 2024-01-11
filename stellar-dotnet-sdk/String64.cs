using System;

namespace stellar_dotnet_sdk;

public class String64
{
    private readonly string _value;

    private String64(string value)
    {
        if (value.Length > 64) throw new ArgumentException("String64 cannot exceed 64 characters", nameof(value));
        _value = value;
    }

    public static implicit operator string(String64 s)
    {
        return s._value;
    }

    public static implicit operator String64(string s)
    {
        return new String64(s);
    }

    public xdr.String64 ToXdr()
    {
        return new xdr.String64(_value);
    }

    public static String64 FromXdr(xdr.String64 xdrString64)
    {
        return new String64(xdrString64.InnerValue);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is not String64 string64)
            return false;

        return _value == string64._value;
    }
}