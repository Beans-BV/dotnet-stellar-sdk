﻿using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <summary>
///     Represents ManageData operation response.
/// </summary>
public class ManageDataOperationResponse : OperationResponse
{
    public override int TypeId => 10;

    [JsonProperty] public string Name { get; init; }

    [JsonProperty] public string Value { get; init; }
}