#!/bin/bash

# Store the current ulimit
original_ulimit=$(ulimit -n)
echo "Original ulimit: $original_ulimit"

# Set the new ulimit
ulimit -n 4096
echo "New ulimit: $(ulimit -n)"

# Run the xdrgen command
sudo xdrgen -o ./ ./schemes/*.x --language=csharp --namespace=StellarDotnetSdk.Xdr
echo "Ran xdrgen command"

# Reset the ulimit to its original value
ulimit -n "$original_ulimit"
echo "Reset ulimit to original value: $original_ulimit"