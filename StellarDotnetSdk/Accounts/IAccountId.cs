namespace StellarDotnetSdk.Accounts;

public interface IAccountId
{
    Xdr.MuxedAccount MuxedAccount { get; }
    KeyPair SigningKey { get; }
    byte[] PublicKey { get; }
    string Address { get; }
    string AccountId { get; }
    bool IsMuxedAccount { get; }
}