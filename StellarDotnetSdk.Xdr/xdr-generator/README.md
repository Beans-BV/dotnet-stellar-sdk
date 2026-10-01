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

Copy from a release tag (e.g. `v28.0`) that matches the protocol the network runs, not from `main`: `main` usually carries staging work for the next protocol.

### Strip feature-gated blocks by hand

Upstream wraps unreleased or test-only definitions in C preprocessor blocks such as `#ifdef CAP_0084_MUXED_CONTRACT` or `#ifdef TEST_FEATURE`. xdrgen has no preprocessor and fails with an unhelpful parse error on a directive, and simply un-wrapping a gated block would ship types that are not part of the protocol. When copying the files:

1. Delete every gated block completely, including its `#ifdef`/`#endif` lines and everything between them.
2. Inside enums, a gated member usually brings its own leading `,` inside the block (`#ifdef X` / `,` / `MEMBER = 5` / `#endif`); that comma goes with the block. Leave the preceding member without a trailing comma.
3. Diff the result against the upstream tag: the stripped blocks must be the only differences.

`generate.rb` refuses to run, and the snapshot suite fails (`test_schemes_have_no_preprocessor_directives`), if any `schemes/*.x` still contains a preprocessor directive.

For v28.0 the stripped blocks are the three `CAP_0084_MUXED_CONTRACT` blocks in `Stellar-contract.x` (`SC_ADDRESS_TYPE_MUXED_CONTRACT`, `struct MuxedContract`, and the matching `SCAddress` arm) and the `TEST_FEATURE` block in `Stellar-types.x` (`struct TestNextType`).

### String typedefs keep their raw bytes

XDR strings are byte strings with no encoding guarantee. A generated string typedef (e.g. `SCString`, `SCSymbol`, `String32`) stores the wire bytes in `InnerBytes` and encodes and decodes them verbatim; `InnerValue` is a UTF-8 view of those bytes. Use `InnerBytes` whenever the exact bytes matter, for example CAP-85 executable tags, which form part of a ledger key. Strings declared inline in a struct or union (e.g. `string text<28>`) are still generated as plain `string` properties.

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
    generator.rb                # C# code generator
    preprocessor_check.rb       # Rejects #ifdef blocks left in schemes/*.x
    templates/
      XdrDataInputStream.erb    # Binary input stream template
      XdrDataOutputStream.erb   # Binary output stream template
  test/
    generator_snapshot_test.rb  # Minitest snapshot tests
    fixtures/xdrgen/*.x         # XDR test fixtures
    snapshots/*/                # Expected output per fixture
```
