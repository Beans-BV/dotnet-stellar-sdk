// Regenerates the Protocol 27 Soroban-authorization known-answer vectors asserted in
// SorobanAuthorizationSigningTest (BuildAddressAuthPreimageHash_MatchesCrossSdkKnownAnswerVector
// and BuildAuthPreimageHash_MatchesCrossSdkKnownAnswerVector) using the JS reference SDK.
//
// The committed constants were produced with @stellar/stellar-sdk v16.0.0-rc.1, the first JS
// release with Protocol 27 (CAP-71) support. To re-derive them:
//
//   npm install @stellar/stellar-sdk@16.0.0-rc.1
//   node generate-p27-auth-kat.mjs
//
// Expected output:
//   WITH_ADDRESS (V2/delegated): 79b329f646bafd6ba1de82e2d470af9b9f324f4c3165d415456cc4f8847d5ea8
//   Legacy (V1):                 f9410dc7ac854c6f53e9f0ba1a905dcd2eaf368390231713e41145777562972d
//
// The preimages are assembled through the raw generated XDR types rather than a convenience
// helper, so the script keeps working across helper-API changes; cross-check against
// buildAuthorizationEntryPreimage when available.

import { Address, Networks, hash, xdr } from '@stellar/stellar-sdk';

// Inputs — keep in sync with the constants in SorobanAuthorizationSigningTest.
const NONCE = '1234567890';
const VALID_UNTIL = 2000;
const SIGNER_ADDRESS = 'GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP';
const CONTRACT = 'CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7';

const networkId = hash(Buffer.from(Networks.PUBLIC));

const invocation = new xdr.SorobanAuthorizedInvocation({
  function: xdr.SorobanAuthorizedFunction.sorobanAuthorizedFunctionTypeContractFn(
    new xdr.InvokeContractArgs({
      contractAddress: new Address(CONTRACT).toScAddress(),
      functionName: 'hello',
      args: [xdr.ScVal.scvString('world')],
    }),
  ),
  subInvocations: [],
});

const withAddressPreimage = xdr.HashIdPreimage.envelopeTypeSorobanAuthorizationWithAddress(
  new xdr.HashIdPreimageSorobanAuthorizationWithAddress({
    networkId,
    nonce: xdr.Int64.fromString(NONCE),
    signatureExpirationLedger: VALID_UNTIL,
    address: new Address(SIGNER_ADDRESS).toScAddress(),
    invocation,
  }),
);

const legacyPreimage = xdr.HashIdPreimage.envelopeTypeSorobanAuthorization(
  new xdr.HashIdPreimageSorobanAuthorization({
    networkId,
    nonce: xdr.Int64.fromString(NONCE),
    signatureExpirationLedger: VALID_UNTIL,
    invocation,
  }),
);

console.log('WITH_ADDRESS (V2/delegated):', hash(withAddressPreimage.toXDR()).toString('hex'));
console.log('Legacy (V1):                ', hash(legacyPreimage.toXDR()).toString('hex'));
