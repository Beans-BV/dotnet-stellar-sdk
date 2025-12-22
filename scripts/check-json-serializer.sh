#!/bin/bash
# Script to check for JsonSerializer usage without JsonSerializerOptions
# This is run as part of CI to ensure consistent JSON serialization with custom converters.
#
# Usage: ./scripts/check-json-serializer.sh
# Returns: 0 if all usages are correct, 1 if violations found

set -e

echo "Checking for JsonSerializer usage without JsonSerializerOptions..."

# Find all .cs files that use JsonSerializer.Serialize or JsonSerializer.Deserialize
# We want to find calls that DON'T pass any options parameter

VIOLATIONS=""

while IFS= read -r file; do
    # Skip Xdr project (generated code)
    if [[ "$file" == *"StellarDotnetSdk.Xdr"* ]]; then
        continue
    fi
    
    # Skip Console project (examples)
    if [[ "$file" == *"StellarDotnetSdk.Console"* ]]; then
        continue
    fi
    
    # Skip Examples folder
    if [[ "$file" == *"/Examples/"* ]]; then
        continue
    fi
    
    # Check for JsonSerializer.Serialize or JsonSerializer.Deserialize
    while IFS= read -r line_info; do
        line_num=$(echo "$line_info" | cut -d: -f1)
        line_content=$(echo "$line_info" | cut -d: -f2-)
        
        # Skip XML documentation comments
        if echo "$line_content" | grep -qE '^\s*///'; then
            continue
        fi
        
        # Skip if line contains "options" parameter (converters pass options internally)
        if echo "$line_content" | grep -qi ', options)'; then
            continue
        fi
        
        # Skip if line contains JsonOptions.DefaultOptions
        if echo "$line_content" | grep -q "JsonOptions.DefaultOptions"; then
            continue
        fi
        
        # Check if it's a Serialize or Deserialize call with only one argument (no options)
        # Pattern matches: JsonSerializer.Serialize(something) or JsonSerializer.Deserialize<Type>(something)
        # where "something" doesn't contain a comma (meaning no second options argument)
        if echo "$line_content" | grep -qE 'JsonSerializer\.(Serialize|Deserialize)[^(]*\([^,)]+\)'; then
            VIOLATIONS="${VIOLATIONS}${file}:${line_num}: ${line_content}\n"
        fi
    done < <(grep -n 'JsonSerializer\.\(Serialize\|Deserialize\)' "$file" 2>/dev/null || true)
    
done < <(find . -name "*.cs" -type f 2>/dev/null)

if [ -n "$VIOLATIONS" ]; then
    echo ""
    echo "❌ Found JsonSerializer calls without JsonSerializerOptions:"
    echo ""
    echo -e "$VIOLATIONS"
    echo ""
    echo "Please use JsonSerializer.Serialize(value, JsonOptions.DefaultOptions) or"
    echo "JsonSerializer.Deserialize<T>(json, JsonOptions.DefaultOptions) to ensure"
    echo "custom converters are applied consistently."
    echo ""
    echo "If this is a false positive (e.g., inside a custom converter that receives options),"
    echo "please add an exception to this script."
    echo ""
    exit 1
else
    echo "✅ All JsonSerializer calls use JsonSerializerOptions correctly."
    exit 0
fi
