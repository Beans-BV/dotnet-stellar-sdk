namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents ManageData operation response.
/// </summary>
public class ManageDataOperationResponse : OperationResponse
{
    public override int TypeId => 10;

    public string Name { get; init; }

    public string Value { get; init; }
}