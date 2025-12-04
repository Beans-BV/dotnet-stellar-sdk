using System.Linq;
using LedgerEntryChange = StellarDotnetSdk.LedgerEntries.LedgerEntryChange;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Transaction metadata V3.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/contract-development/events#transactionmetav3">TransactionMetaV3</a>
/// </summary>
public class TransactionMetaV3 : TransactionMeta
{
    /// <summary>
    ///     We can use this to add more fields.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; private set; } = new ExtensionPointZero();

    /// <summary>
    ///     Transaction level changes before operations are applied if any.
    /// </summary>
    public LedgerEntryChange[] TransactionChangesBefore { get; private set; } = [];

    /// <summary>
    ///     Transaction level changes after operations are applied if any.
    /// </summary>
    public LedgerEntryChange[] TransactionChangesAfter { get; private set; } = [];

    /// <summary>
    ///     Meta for each operation.
    /// </summary>
    public LedgerEntryChange[][] Operations { get; private set; } = [];
    // TODO Unit test with actual data can be done with the case of sponsoring trustline. AAAAAwAAAAAAAAACAAAAAwAIZwEAAAAAAAAAACMRtl/+ZI8994htM6K35GWqLqFTU3LGv/gzRqx0bXTQAAAAFd6pPdoAAB82AAAAegAAAAkAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAAAAAAGAAAAAAAAAAMAAAAAAAhmpwAAAABl7pTNAAAAAAAAAAEACGcBAAAAAAAAAAAjEbZf/mSPPfeIbTOit+Rlqi6hU1Nyxr/4M0asdG100AAAABXeqT3aAAAfNgAAAHsAAAAJAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAABgAAAAAAAAADAAAAAAAIZwEAAAAAZe6WoQAAAAAAAAADAAAAAAAAAAUAAAAAAAhnAQAAAAEAAAAAWOReMQoIuRGVqVdU95hGUv9sNNBwapkLkukv70rxmD4AAAABQUFBAAAAAAAjEbZf/mSPPfeIbTOit+Rlqi6hU1Nyxr/4M0asdG100AAAAAAAAAAAf/////////8AAAABAAAAAAAAAAEAAAABAAAAACMRtl/+ZI8994htM6K35GWqLqFTU3LGv/gzRqx0bXTQAAAAAAAAAAMACGcBAAAAAAAAAAAjEbZf/mSPPfeIbTOit+Rlqi6hU1Nyxr/4M0asdG100AAAABXeqT3aAAAfNgAAAHsAAAAJAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAABgAAAAAAAAADAAAAAAAIZwEAAAAAZe6WoQAAAAAAAAABAAhnAQAAAAAAAAAAIxG2X/5kjz33iG0zorfkZaouoVNTcsa/+DNGrHRtdNAAAAAV3qk92gAAHzYAAAB7AAAACQAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAAAcAAAAAAAAAAwAAAAAACGcBAAAAAGXulqEAAAAAAAAAAwAHr8EAAAAAAAAAAFjkXjEKCLkRlalXVPeYRlL/bDTQcGqZC5LpL+9K8Zg+AAAAAAAAAAAAB6/BAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAgAAAAIAAAAAAAAAAAAAAAAAAAABAAAAAQAAAAC0nk2pUBTErnUCqsB7QlRaLeMjIrls0Ry+aM4Mz43/lgAAAAAAAAABAAhnAQAAAAAAAAAAWOReMQoIuRGVqVdU95hGUv9sNNBwapkLkukv70rxmD4AAAAAAAAAAAAHr8EAAAAAAAAAAQAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAwAAAAAAAAAAAAAAAAAAAAEAAAABAAAAALSeTalQFMSudQKqwHtCVFot4yMiuWzRHL5ozgzPjf+WAAAAAAAAAAAAAAAAAAAAAA==

    /// <summary>
    ///     (Optional) Holds the Soroban transaction metadata.
    /// </summary>
    public SorobanTransactionMeta? SorobanMeta { get; private set; }

    /// <summary>
    ///     Creates the corresponding <c>TransactionMetaV3</c> object from an <c>xdr.TransactionMetaV3</c> object.
    /// </summary>
    /// <param name="xdrTransactionMetaV3">An <c>xdr.TransactionMetaV3</c> object to be converted.</param>
    /// <returns>A <c>TransactionMetaV3</c> object.</returns>
    internal static TransactionMetaV3 FromXdr(Xdr.TransactionMetaV3 xdrTransactionMetaV3)
    {
        return new TransactionMetaV3
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdrTransactionMetaV3.Ext),
            TransactionChangesBefore = xdrTransactionMetaV3.TxChangesBefore.InnerValue
                .Select(LedgerEntryChange.FromXdr)
                .ToArray(),
            TransactionChangesAfter = xdrTransactionMetaV3.TxChangesAfter.InnerValue
                .Select(LedgerEntryChange.FromXdr)
                .ToArray(),
            Operations = xdrTransactionMetaV3.Operations
                .Select(x => x.Changes.InnerValue.Select(LedgerEntryChange.FromXdr).ToArray())
                .ToArray(),
            SorobanMeta = SorobanTransactionMeta.FromXdr(xdrTransactionMetaV3.SorobanMeta),
        };
    }
}