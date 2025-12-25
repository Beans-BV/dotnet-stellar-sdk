namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when attempting to use a memo with a muxed account (M... address).
/// </summary>
public class NoMemoForMuxedAccountsException : WebAuthException
{
    public NoMemoForMuxedAccountsException()
        : base("Memo cannot be used if account is a muxed account")
    {
    }
}