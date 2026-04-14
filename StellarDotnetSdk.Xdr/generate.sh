#!/bin/bash
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

cd "$SCRIPT_DIR/xdr"
bundle install --quiet
bundle exec ruby generate_xdr.rb "$SCRIPT_DIR/schemes" "$SCRIPT_DIR" "StellarDotnetSdk.Xdr"
