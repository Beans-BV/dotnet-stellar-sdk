# Encoding To and From Base-64 Encoded XDR

This guide demonstrates how to work with XDR (External Data Representation) encoding and decoding in the Stellar .NET SDK.

## Understanding XDR in Stellar

XDR (External Data Representation) is a standardized data serialization format used by Stellar to represent network objects like transactions, operations, and ledger entries. When interacting with the Stellar network, you'll frequently need to:

- Decode XDR strings received from Horizon or Soroban RPC servers
- Encode Stellar objects to XDR for submission or storage
- Inspect and modify XDR data for debugging or advanced use cases

The Stellar .NET SDK provides tools to work with these XDR formats, typically encoded as base-64 strings for transmission.

## Converting Transactions to XDR
To be updated

## Reconstructing Transactions from XDR
To be updated

## Best Practices for XDR Handling

1. **Always validate XDR**: Check for null values and use proper error handling when decoding XDR.

2. **Type verification**: When casting from base classes (like `Operation` to `PaymentOperation`), use type checking (e.g., `is` operator) to ensure safety.

3. **Keep XDR for debugging**: Store raw XDR values along with parsed objects during critical operations to aid in debugging.

4. **Handle version differences**: Be aware that XDR formats can change between network protocol versions. Your code should handle these gracefully.

5. **Persistence considerations**: If storing XDR for later use, document the protocol version to ensure compatibility.

## Additional Resources

- [XDR format](https://developers.stellar.org/docs/learn/encyclopedia/data-format/xdr)
- [Stellar XDR Repository](https://github.com/stellar/stellar-xdr)
- [Stellar Type Conversions](https://developers.stellar.org/docs/build/guides/conversions)
