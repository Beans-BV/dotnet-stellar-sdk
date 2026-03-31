using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Set options for an account such as flags, inflation destination, signers, home domain, and master key weight.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#set-options">
///         Set
///         options
///     </a>
/// </summary>
public class SetOptionsOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>SetOptionsOperation</c>.
    /// </summary>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public SetOptionsOperation(IAccountId? sourceAccount = null) : base(sourceAccount)
    {
    }

    /// <summary>
    ///     Account that is set as the inflation destination for the source account.
    /// </summary>
    public KeyPair? InflationDestination { get; private set; }

    /// <summary>
    ///     The account flags to clear (combined via bitwise-OR).
    /// </summary>
    public uint? ClearFlags { get; private set; }

    /// <summary>
    ///     The account flags to set (combined via bitwise-OR).
    /// </summary>
    public uint? SetFlags { get; private set; }

    /// <summary>
    ///     Weight of the master key (0-255). Setting to 0 disables the master key.
    /// </summary>
    public uint? MasterKeyWeight { get; private set; }

    /// <summary>
    ///     A number from 0-255 representing the threshold for low-security operations.
    /// </summary>
    public uint? LowThreshold { get; private set; }

    /// <summary>
    ///     A number from 0-255 representing the threshold for medium-security operations.
    /// </summary>
    public uint? MediumThreshold { get; private set; }

    /// <summary>
    ///     A number from 0-255 representing the threshold for high-security operations.
    /// </summary>
    public uint? HighThreshold { get; private set; }

    /// <summary>
    ///     The home domain of the account, used for federation and SEP lookups (up to 32 characters).
    /// </summary>
    public string? HomeDomain { get; private set; }

    /// <summary>
    ///     The signer to add, update, or remove from the account.
    /// </summary>
    public Signer? Signer { get; private set; }

    /// <summary>
    ///     The threshold level for this operation. SetOptions is a high-threshold operation.
    /// </summary>
    public override OperationThreshold Threshold => OperationThreshold.HIGH;

    /// <summary>
    ///     Sets the inflation destination for the account.
    /// </summary>
    /// <param name="inflationDestination">The inflation destination account.</param>
    /// <returns>SetOptionsOperation object so you can chain methods.</returns>
    public SetOptionsOperation SetInflationDestination(KeyPair inflationDestination)
    {
        InflationDestination = inflationDestination;
        return this;
    }

    /// <summary>
    ///     Clears the given flags from the account.
    /// </summary>
    /// <param name="clearFlags">
    ///     For details about the flags, please refer to the
    ///     <a href="https://developers.stellar.org/docs/learn/glossary#account">accounts doc</a>.
    /// </param>
    /// <returns>SetOptionsOperation object so you can chain methods.</returns>
    public SetOptionsOperation SetClearFlags(int clearFlags)
    {
        if (clearFlags < 0)
        {
            throw new ArgumentException("clearFlags must be non negative.", nameof(clearFlags));
        }
        ClearFlags = (uint)clearFlags;
        return this;
    }

    /// <summary>
    ///     Sets the given flags on the account.
    /// </summary>
    /// <param name="setFlags">
    ///     For details about the flags, please refer to the
    ///     <a href="https://developers.stellar.org/docs/learn/glossary#account">accounts doc</a>.
    /// </param>
    /// <returns>SetOptionsOperation object so you can chain methods.</returns>
    public SetOptionsOperation SetSetFlags(int setFlags)
    {
        if (setFlags < 0)
        {
            throw new ArgumentException("setFlags must be non negative.", nameof(setFlags));
        }
        SetFlags = (uint)setFlags;
        return this;
    }


    /// <summary>
    ///     Weight of the master key.
    /// </summary>
    /// <param name="masterKeyWeight">Number between 0 and 255</param>
    /// <returns>SetOptionsOperation object so you can chain methods.</returns>
    public SetOptionsOperation SetMasterKeyWeight(int masterKeyWeight)
    {
        if (masterKeyWeight is < 0 or > 255)
        {
            throw new ArgumentException("masterKeyWeight must be an integer between 0 and 255 (inclusive).",
                nameof(masterKeyWeight));
        }
        MasterKeyWeight = (uint)masterKeyWeight;
        return this;
    }

    /// <summary>
    ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a low
    ///     threshold.
    /// </summary>
    /// <param name="lowThreshold">Number between 0 and 255</param>
    /// <returns>SetOptionsOperation object so you can chain methods.</returns>
    public SetOptionsOperation SetLowThreshold(int lowThreshold)
    {
        if (lowThreshold is < 0 or > 255)
        {
            throw new ArgumentException("lowThreshold must be an integer between 0 and 255 (inclusive).",
                nameof(lowThreshold));
        }
        LowThreshold = (uint)lowThreshold;
        return this;
    }

    /// <summary>
    ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a medium
    ///     threshold.
    /// </summary>
    /// <param name="mediumThreshold">Number between 0 and 255</param>
    /// <returns>SetOptionsOperation object so you can chain methods.</returns>
    public SetOptionsOperation SetMediumThreshold(int mediumThreshold)
    {
        if (mediumThreshold is < 0 or > 255)
        {
            throw new ArgumentException("mediumThreshold must be an integer between 0 and 255 (inclusive).",
                nameof(mediumThreshold));
        }
        MediumThreshold = (uint)mediumThreshold;
        return this;
    }

    /// <summary>
    ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a high
    ///     threshold.
    /// </summary>
    /// <param name="highThreshold">Number between 0 and 255</param>
    /// <returns>SetOptionsOperation object so you can chain methods.</returns>
    public SetOptionsOperation SetHighThreshold(int highThreshold)
    {
        if (highThreshold is < 0 or > 255)
        {
            throw new ArgumentException("highThreshold must be an integer between 0 and 255 (inclusive).",
                nameof(highThreshold));
        }
        HighThreshold = (uint)highThreshold;
        return this;
    }

    /// <summary>
    ///     Sets the account's home domain address used in.
    /// </summary>
    /// <param name="homeDomain">A string of the address which can be up to 32 characters.</param>
    /// <returns>SetOptionsOperation object so you can chain methods.</returns>
    public SetOptionsOperation SetHomeDomain(string homeDomain)
    {
        if (homeDomain.Length > 32)
        {
            throw new ArgumentException("Home domain must be <= 32 characters");
        }
        HomeDomain = homeDomain;
        return this;
    }

    /// <summary>
    ///     Add, update, or remove a signer from the account. Signer is deleted if the weight = 0;
    /// </summary>
    /// <param name="signer">The signer's Ed25519 public key.</param>
    /// <param name="weight">The weight to attach to the signer (0-255).</param>
    /// <returns>SetOptionsOperation object so you can chain methods.</returns>
    public SetOptionsOperation SetSigner(string accountId, int weight)
    {
        if (accountId == null)
        {
            throw new ArgumentNullException(nameof(accountId), "signer cannot be null");
        }

        return SetSigner(SignerUtil.Ed25519PublicKey(KeyPair.FromAccountId(accountId)), weight);
    }

    /// <summary>
    ///     Add, update, or remove a signer from the account. Signer is deleted if the weight = 0;
    /// </summary>
    /// <param name="signer">The signer key. Use <see cref="SignerUtil" /> helper to create this object.</param>
    /// <param name="weight">The weight to attach to the signer (0-255).</param>
    /// <returns>SetOptionsOperation object so you can chain methods.</returns>
    public SetOptionsOperation SetSigner(SignerKey signer, int weight)
    {
        Signer = new Signer(signer, (uint)weight);
        return this;
    }

    /// <summary>
    ///     Generates the XDR operation body for this operation.
    /// </summary>
    /// <returns>The XDR operation body.</returns>
    public override xdr_Operation.OperationBody ToOperationBody()
    {
        var op = new SetOptionsOp();
        if (InflationDestination != null)
        {
            op.InflationDest = new AccountID(InflationDestination.XdrPublicKey);
        }

        if (ClearFlags != null)
        {
            op.ClearFlags = new Uint32(ClearFlags.Value);
        }

        if (SetFlags != null)
        {
            op.SetFlags = new Uint32(SetFlags.Value);
        }

        if (MasterKeyWeight != null)
        {
            op.MasterWeight = new Uint32(MasterKeyWeight.Value);
        }

        if (LowThreshold != null)
        {
            op.LowThreshold = new Uint32(LowThreshold.Value);
        }

        if (MediumThreshold != null)
        {
            op.MedThreshold = new Uint32(MediumThreshold.Value);
        }

        if (HighThreshold != null)
        {
            op.HighThreshold = new Uint32(HighThreshold.Value);
        }

        if (HomeDomain != null)
        {
            op.HomeDomain = new String32(HomeDomain);
        }

        if (Signer != null)
        {
            op.Signer = Signer.ToXdr();
        }

        return new xdr_Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.SET_OPTIONS),
            SetOptionsOp = op,
        };
    }

    /// <summary>
    ///     Creates a <see cref="SetOptionsOperation" /> from its XDR representation.
    /// </summary>
    /// <param name="setOptionsOp">The XDR SetOptionsOp object.</param>
    /// <returns>A new <see cref="SetOptionsOperation" /> instance.</returns>
    public static SetOptionsOperation FromXdr(SetOptionsOp setOptionsOp)
    {
        var operation = new SetOptionsOperation();
        if (setOptionsOp.InflationDest != null)
        {
            operation.SetInflationDestination(KeyPair.FromXdrPublicKey(setOptionsOp.InflationDest.InnerValue));
        }
        if (setOptionsOp.ClearFlags != null)
        {
            operation.SetClearFlags((int)setOptionsOp.ClearFlags.InnerValue);
        }
        if (setOptionsOp.SetFlags != null)
        {
            operation.SetSetFlags((int)setOptionsOp.SetFlags.InnerValue);
        }
        if (setOptionsOp.MasterWeight != null)
        {
            operation.SetMasterKeyWeight((int)setOptionsOp.MasterWeight.InnerValue);
        }
        if (setOptionsOp.LowThreshold != null)
        {
            operation.SetLowThreshold((int)setOptionsOp.LowThreshold.InnerValue);
        }
        if (setOptionsOp.MedThreshold != null)
        {
            operation.SetMediumThreshold((int)setOptionsOp.MedThreshold.InnerValue);
        }
        if (setOptionsOp.HighThreshold != null)
        {
            operation.SetHighThreshold((int)setOptionsOp.HighThreshold.InnerValue);
        }
        if (setOptionsOp.HomeDomain != null)
        {
            operation.SetHomeDomain(setOptionsOp.HomeDomain.InnerValue);
        }
        if (setOptionsOp.Signer != null)
        {
            operation.SetSigner(setOptionsOp.Signer.Key, (int)setOptionsOp.Signer.Weight.InnerValue);
        }
        return operation;
    }
}