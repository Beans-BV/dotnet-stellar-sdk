# XDR Code Generator

Generates C# classes from [XDR](https://datatracker.ietf.org/doc/html/rfc4506) schema files (`.x`). Built on the [stellar/xdrgen](https://github.com/stellar/xdrgen) Ruby gem for parsing, with a custom C# generator.

## Prerequisites

- Ruby 3.4+
- Bundler (`gem install bundler`)

## Regenerating XDR classes

From the repo root:

```bash
# Linux / macOS
./StellarDotnetSdk.Xdr/xdr-generator/generate.sh

# Windows
StellarDotnetSdk.Xdr\xdr-generator\generate.bat
```

Or manually:

```bash
cd StellarDotnetSdk.Xdr/xdr-generator
bundle install
bundle exec ruby generate.rb
```

This reads `StellarDotnetSdk.Xdr/schemes/*.x` and writes `.cs` files into `StellarDotnetSdk.Xdr/`.

## Updating XDR schemas

Replace the `.x` files in `StellarDotnetSdk.Xdr/schemes/` with the new versions from [stellar/stellar-xdr](https://github.com/stellar/stellar-xdr), then regenerate.

## Running tests

Snapshot tests verify the generator output against expected files:

```bash
cd StellarDotnetSdk.Xdr/xdr-generator
bundle install
bundle exec ruby -Itest test/generator_snapshot_test.rb
```

After intentional generator changes, update the snapshots:

```bash
UPDATE_SNAPSHOTS=1 bundle exec ruby -Itest test/generator_snapshot_test.rb
```

## Directory layout

```
xdr-generator/
  generate.sh / generate.bat    # Shell wrappers
  generate.rb                   # Entry point
  Gemfile                       # Ruby dependencies (xdrgen gem)
  generator/
    generator.rb                # C# code generator (~700 lines)
    templates/
      XdrDataInputStream.erb    # Binary input stream template
      XdrDataOutputStream.erb   # Binary output stream template
  test/
    generator_snapshot_test.rb  # Minitest snapshot tests
    fixtures/xdrgen/*.x         # XDR test fixtures
    snapshots/*/                # Expected output per fixture
```
