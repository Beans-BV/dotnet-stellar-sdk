// Regenerates the Protocol 28 known-answer vectors asserted in Protocol28Test using the JS reference SDK.
//
// The committed constants were produced with @stellar/stellar-sdk v17.2.0 (stellar-xdr v28.0). To
// re-derive them:
//
//   npm install @stellar/stellar-sdk@17.2.0
//   node generate-p28-kat.mjs
//
// Each line prints NAME: base64-XDR. Paste the values into the matching constants in Protocol28Test.
//
// Covered:
//   - CAP-85 SCV_EXECUTABLE_TAG for a text tag, a non-ASCII text tag and a binary (non-UTF-8) tag;
//   - CAP-85 CONTRACT_EXECUTABLE_EXTERNAL_REF for both tags;
//   - the CREATE_CONTRACT_V2 operation built by Operation.createCustomContract({ externalRef });
//   - the persistent contract-data ledger key that getExternalRefWasmHash looks up, and a contract-data
//     ledger entry holding a 32-byte Wasm hash under it;
//   - CAP-83 StellarValue with the STELLAR_VALUE_EMPTY_TX_SET ext arm (outer txSetHash 0x0, as the CAP specifies).

import { Address, Keypair, Operation, xdr } from '@stellar/stellar-sdk';

// Inputs — keep in sync with the constants in Protocol28Test.
const OWNER = 'CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7';
const DEPLOYER = 'GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP';
const TEXT_TAG = 'fleet-v1';
// Non-ASCII text, so the string overloads are pinned to UTF-8 rather than to any single-byte encoding.
const NON_ASCII_TAG = 'flotte-\u00e9-\u{1F680}';
// 0xff never appears in UTF-8, and 0xc3 0x28 is a truncated two-byte sequence.
const BINARY_TAG = Uint8Array.from([0xff, 0xfe, 0x00, 0xc3, 0x28]);
const SALT = Uint8Array.from({ length: 32 }, (_, i) => i + 1);
const WASM_HASH = Uint8Array.from({ length: 32 }, (_, i) => 0xa0 + i);
const TX_SET_HASH = Uint8Array.from({ length: 32 }, (_, i) => 0x10 + i);
const PREVIOUS_LEDGER_HASH = Uint8Array.from({ length: 32 }, (_, i) => 0x40 + i);
const CLOSE_TIME = 1758000000n;
const PREVIOUS_LEDGER_VERSION = 28;
const SIGNATURE = Uint8Array.from({ length: 64 }, (_, i) => 0x80 + i);

const owner = new Address(OWNER).toScAddress();

const print = (name, value) => console.log(`${name}: ${value.toXDR('base64')}`);

print('SCV_EXECUTABLE_TAG_NON_ASCII', xdr.ScVal.scvExecutableTag(NON_ASCII_TAG));

for (const [label, tag] of [['TEXT', TEXT_TAG], ['BINARY', BINARY_TAG]]) {
  print(`SCV_EXECUTABLE_TAG_${label}`, xdr.ScVal.scvExecutableTag(tag));
  print(
    `CONTRACT_EXECUTABLE_EXTERNAL_REF_${label}`,
    xdr.ContractExecutable.contractExecutableExternalRef(
      new xdr.ContractExecutableExternalRef({ executableOwner: owner, tag }),
    ),
  );
  print(
    `CREATE_CONTRACT_FROM_EXTERNAL_REF_OP_${label}`,
    Operation.createCustomContract({
      address: new Address(DEPLOYER),
      externalRef: { owner: OWNER, tag },
      salt: SALT,
      constructorArgs: [xdr.ScVal.scvU32(7)],
    }),
  );
  print(
    `EXTERNAL_REF_LEDGER_KEY_${label}`,
    xdr.LedgerKey.contractData(
      new xdr.LedgerKeyContractData({
        contract: owner,
        key: xdr.ScVal.scvExecutableTag(tag),
        durability: xdr.ContractDataDurability.persistent,
      }),
    ),
  );
  print(
    `EXTERNAL_REF_LEDGER_ENTRY_DATA_${label}`,
    xdr.LedgerEntryData.contractData(
      new xdr.ContractDataEntry({
        ext: xdr.ExtensionPoint.v0(),
        contract: owner,
        key: xdr.ScVal.scvExecutableTag(tag),
        durability: xdr.ContractDataDurability.persistent,
        val: xdr.ScVal.scvBytes(WASM_HASH),
      }),
    ),
  );
}

print(
  'STELLAR_VALUE_EMPTY_TX_SET',
  new xdr.StellarValue({
    // CAP-83: a value that drops its transaction set externalizes txSetHash 0x0; the dropped set's hash is kept
    // in proposedValue.
    txSetHash: new Uint8Array(32),
    closeTime: CLOSE_TIME,
    upgrades: [],
    ext: xdr.StellarValueExt.stellarValueEmptyTxSet(
      new xdr.StellarValueProposedValue({
        txSetHash: TX_SET_HASH,
        previousLedgerHash: PREVIOUS_LEDGER_HASH,
        previousLedgerVersion: PREVIOUS_LEDGER_VERSION,
        lcValueSignature: new xdr.LedgerCloseValueSignature({
          nodeId: Keypair.fromPublicKey(DEPLOYER).xdrPublicKey(),
          signature: SIGNATURE,
        }),
      }),
    ),
  }),
);
