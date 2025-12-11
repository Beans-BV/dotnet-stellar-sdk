namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents an inflation operation response (deprecated operation).
///     Runs the weekly inflation process and distributes new lumens.
/// </summary>
/// <remarks>
///     This operation is deprecated and no longer functional on the Stellar network as of Protocol 12.
/// </remarks>
public class InflationOperationResponse : OperationResponse
{
    public override int TypeId => 9;
}