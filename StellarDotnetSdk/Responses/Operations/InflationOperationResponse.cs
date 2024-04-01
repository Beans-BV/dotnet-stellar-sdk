namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents Inflation operation response.
///     See: https://www.stellar.org/developers/horizon/reference/resources/operation.html
///     <seealso cref="Requests.OperationsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
public class InflationOperationResponse : OperationResponse
{
    public override int TypeId => 9;
}