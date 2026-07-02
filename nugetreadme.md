The .NET Stellar SDK facilitates client integration with the Stellar Horizon API server and submission of Stellar
transactions. It has two main uses: querying Horizon and building, signing, and submitting transactions to the Stellar
network.

## Supported target frameworks

| TFM | Platforms |
|-----|-----------|
| `net10.0` | .NET 10 |
| `net8.0` | .NET 8 |
| `netstandard2.1` | Unity, Tizen, portable libraries |

On `netstandard2.1`, SEP-0009 date properties are `string?` (ISO `yyyy-MM-dd`); on `net8.0` / `net10.0` they are `DateOnly?`.
