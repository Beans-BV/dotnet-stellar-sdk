// Regenerates the Protocol 30 known-answer vectors asserted in Protocol30Test using the Go reference SDK.
//
// The committed constants were produced with go-stellar-sdk v0.8.0 (stellar-xdr v30.0). The JS SDK had no
// Protocol 30 release yet. To re-derive them, from an empty directory holding a copy of this file:
//
//   go mod init kat
//   go get github.com/stellar/go-stellar-sdk@v0.8.0
//   go mod tidy
//   go run .
//
// Each line prints NAME: value. Paste the values into the matching constants in Protocol30Test.
//
// Covered:
//   - CAP-84 SC_ADDRESS_TYPE_MUXED_CONTRACT and SC_ADDRESS_TYPE_MUXED_ACCOUNT as SCV_ADDRESS values, for the SEP-23
//     contract/key with an ID above the int64 maximum, plus the strkeys Go derives for them (both equal the SEP-23
//     vectors). ScvMuxedAccount is asserted in AddressTest rather than Protocol30Test;
//   - CAP-88 StellarValue with the STELLAR_VALUE_SIGNED_MS and STELLAR_VALUE_EMPTY_TX_SET_MS ext arms;
//   - a LedgerHeader whose scpValue uses STELLAR_VALUE_SIGNED_MS.

package main

import (
	"fmt"

	"github.com/stellar/go-stellar-sdk/strkey"
	"github.com/stellar/go-stellar-sdk/xdr"
)

func b64(v interface{}) string {
	s, err := xdr.MarshalBase64(v)
	if err != nil {
		panic(err)
	}
	return s
}

func seq(start byte, n int) []byte {
	b := make([]byte, n)
	for i := range b {
		b[i] = start + byte(i)
	}
	return b
}

func main() {
	contract := strkey.MustDecode(strkey.VersionByteContract, "CA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUWDA")
	ed := strkey.MustDecode(strkey.VersionByteAccountID, "GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ")
	const id = uint64(9223372036854775808)

	var h xdr.Hash
	copy(h[:], contract)
	var u xdr.Uint256
	copy(u[:], ed)

	mc := xdr.ScAddress{Type: xdr.ScAddressTypeScAddressTypeMuxedContract,
		MuxedContract: &xdr.MuxedContract{Id: xdr.Uint64(id), ContractId: xdr.ContractId(h)}}
	ma := xdr.ScAddress{Type: xdr.ScAddressTypeScAddressTypeMuxedAccount,
		MuxedAccount: &xdr.MuxedEd25519Account{Id: xdr.Uint64(id), Ed25519: u}}

	w, _ := mc.String()
	m, _ := ma.String()
	fmt.Println("MuxedContractStrKey:", w)
	fmt.Println("MuxedAccountStrKey:", m)
	fmt.Println("ScvMuxedContract:", b64(xdr.ScVal{Type: xdr.ScValTypeScvAddress, Address: &mc}))
	fmt.Println("ScvMuxedAccount:", b64(xdr.ScVal{Type: xdr.ScValTypeScvAddress, Address: &ma}))

	// CAP-88 StellarValue arms.
	var node xdr.Uint256
	copy(node[:], ed)
	sig := xdr.LedgerCloseValueSignature{
		NodeId:    xdr.NodeId{Type: xdr.PublicKeyTypePublicKeyTypeEd25519, Ed25519: &node},
		Signature: xdr.Signature(seq(0x40, 64)),
	}
	var txSet, prev xdr.Hash
	copy(txSet[:], seq(0x01, 32))
	copy(prev[:], seq(0x81, 32))
	const closeTimeMs = uint64(1793725200123) // 2026-11-03T17:00:00.123Z

	signed := xdr.StellarValue{TxSetHash: txSet, CloseTime: xdr.TimePoint(closeTimeMs / 1000), Upgrades: []xdr.UpgradeType{},
		Ext: xdr.StellarValueExt{V: xdr.StellarValueTypeStellarValueSignedMs,
			SignedMsValue: &xdr.StellarValueSignedMsValue{CloseTimeMs: xdr.TimePointMs(closeTimeMs), LcValueSignature: sig}}}
	var zero xdr.Hash
	proposed := xdr.StellarValue{TxSetHash: zero, CloseTime: xdr.TimePoint(closeTimeMs / 1000), Upgrades: []xdr.UpgradeType{},
		Ext: xdr.StellarValueExt{V: xdr.StellarValueTypeStellarValueEmptyTxSetMs,
			ProposedMsValue: &xdr.StellarValueProposedMsValue{CloseTimeMs: xdr.TimePointMs(closeTimeMs), TxSetHash: txSet,
				PreviousLedgerHash: prev, PreviousLedgerVersion: 30, LcValueSignature: sig}}}
	fmt.Println("StellarValueSignedMs:", b64(signed))
	fmt.Println("StellarValueEmptyTxSetMs:", b64(proposed))

	header := xdr.LedgerHeader{LedgerVersion: 30, PreviousLedgerHash: prev, ScpValue: signed, LedgerSeq: 1000000,
		SkipList: [4]xdr.Hash{}}
	fmt.Println("LedgerHeaderSignedMs:", b64(header))
}
