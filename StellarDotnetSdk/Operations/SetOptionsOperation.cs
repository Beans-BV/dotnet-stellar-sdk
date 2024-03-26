using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;
using xdr_Signer = StellarDotnetSdk.Xdr.Signer;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Represents a <see cref="SetOptionsOp" />.
///     Use <see cref="Builder" /> to create a new SetOptionsOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#set-options">Set Options</a>
/// </summary>
public class SetOptionsOperation : Operation
{
    private SetOptionsOperation(
        KeyPair? inflationDestination,
        uint? clearFlags,
        uint? setFlags,
        uint? masterKeyWeight,
        uint? lowThreshold,
        uint? mediumThreshold,
        uint? highThreshold,
        string? homeDomain,
        SignerKey? signer,
        uint? signerWeight)
    {
        InflationDestination = inflationDestination;
        ClearFlags = clearFlags;
        SetFlags = setFlags;
        MasterKeyWeight = masterKeyWeight;
        LowThreshold = lowThreshold;
        MediumThreshold = mediumThreshold;
        HighThreshold = highThreshold;
        HomeDomain = homeDomain;
        Signer = signer;
        SignerWeight = signerWeight;
    }

    public KeyPair? InflationDestination { get; }

    public uint? ClearFlags { get; }

    public uint? SetFlags { get; }

    public uint? MasterKeyWeight { get; }

    public uint? LowThreshold { get; }

    public uint? MediumThreshold { get; }

    public uint? HighThreshold { get; }

    public string? HomeDomain { get; }

    public SignerKey? Signer { get; }

    public uint? SignerWeight { get; }

    public override OperationThreshold Threshold => OperationThreshold.HIGH;

    /// <summary>
    ///     Creates a new SetOptionsOperation object from the given base64-encoded XDR Operation.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>SetOptionsOperation object</returns>
    /// <exception cref="InvalidOperationException">Thrown when the base64-encoded XDR value is invalid.</exception>
    public static SetOptionsOperation FromOperationXdrBase64(string xdrBase64)
    {
        var operation = FromXdrBase64(xdrBase64);

        if (operation == null)
            throw new InvalidOperationException("Operation XDR is invalid");
        if (operation is not SetOptionsOperation setOptionsOperation)
            throw new InvalidOperationException("Operation is not SetOptionsOperation");

        return setOptionsOperation;
    }

    public override xdr_Operation.OperationBody ToOperationBody()
    {
        var op = new SetOptionsOp();
        if (InflationDestination != null) op.InflationDest = new AccountID(InflationDestination.XdrPublicKey);

        if (ClearFlags != null) op.ClearFlags = new Uint32(ClearFlags.Value);

        if (SetFlags != null) op.SetFlags = new Uint32(SetFlags.Value);

        if (MasterKeyWeight != null) op.MasterWeight = new Uint32(MasterKeyWeight.Value);

        if (LowThreshold != null) op.LowThreshold = new Uint32(LowThreshold.Value);

        if (MediumThreshold != null) op.MedThreshold = new Uint32(MediumThreshold.Value);

        if (HighThreshold != null) op.HighThreshold = new Uint32(HighThreshold.Value);

        if (HomeDomain != null) op.HomeDomain = new String32(HomeDomain);

        if (Signer != null && SignerWeight != null)
        {
            var signer = new xdr_Signer
            {
                Key = Signer,
                Weight = new Uint32(SignerWeight.Value)
            };
            op.Signer = signer;
        }

        var body = new xdr_Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.SET_OPTIONS),
            SetOptionsOp = op
        };
        return body;
    }

    /// <summary>
    ///     Constructs a <see cref="SetOptionsOperation" /> operation.
    /// </summary>
    public class Builder
    {
        private uint? _clearFlags;
        private uint? _highThreshold;
        private string? _homeDomain;
        private KeyPair? _inflationDestination;
        private uint? _lowThreshold;
        private uint? _masterKeyWeight;
        private uint? _mediumThreshold;
        private uint? _setFlags;
        private SignerKey? _signer;
        private uint? _signerWeight;
        private KeyPair? _sourceAccount;

        public Builder(SetOptionsOp op)
        {
            if (op.InflationDest != null)
                _inflationDestination = KeyPair.FromXdrPublicKey(op.InflationDest.InnerValue);
            if (op.ClearFlags != null)
                _clearFlags = op.ClearFlags.InnerValue;
            if (op.SetFlags != null)
                _setFlags = op.SetFlags.InnerValue;
            if (op.MasterWeight != null)
                _masterKeyWeight = op.MasterWeight.InnerValue;
            if (op.LowThreshold != null)
                _lowThreshold = op.LowThreshold.InnerValue;
            if (op.MedThreshold != null)
                _mediumThreshold = op.MedThreshold.InnerValue;
            if (op.HighThreshold != null)
                _highThreshold = op.HighThreshold.InnerValue;
            if (op.HomeDomain != null)
                _homeDomain = op.HomeDomain.InnerValue;
            if (op.Signer != null)
            {
                _signer = op.Signer.Key;
                _signerWeight = op.Signer.Weight.InnerValue & 0xFF;
            }
        }

        /// <summary>
        ///     Creates a new SetOptionsOperation builder.
        /// </summary>
        public Builder()
        {
        }

        /// <summary>
        ///     Sets the inflation destination for the account.
        /// </summary>
        /// <param name="inflationDestination">The inflation destination account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetInflationDestination(KeyPair inflationDestination)
        {
            _inflationDestination = inflationDestination;
            return this;
        }

        /// <summary>
        ///     Clears the given flags from the account.
        /// </summary>
        /// <param name="clearFlags">
        ///     For details about the flags, please refer to the
        ///     <a href="https://www.stellar.org/developers/learn/concepts/accounts.html" target="_blank">accounts doc</a>.
        /// </param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetClearFlags(uint clearFlags)
        {
            _clearFlags = clearFlags;
            return this;
        }

        public Builder SetClearFlags(int clearFlags)
        {
            if (clearFlags < 0) throw new ArgumentException("clearFlags must be non negative");
            return SetClearFlags((uint)clearFlags);
        }

        /// <summary>
        ///     Sets the given flags on the account.
        /// </summary>
        /// <param name="setFlags">
        ///     For details about the flags, please refer to the
        ///     <a href="https://www.stellar.org/developers/learn/concepts/accounts.html" target="_blank">accounts doc</a>.
        /// </param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSetFlags(uint setFlags)
        {
            _setFlags = setFlags;
            return this;
        }

        public Builder SetSetFlags(int setFlags)
        {
            if (setFlags < 0) throw new ArgumentException("setFlags must be non negative");
            return SetSetFlags((uint)setFlags);
        }

        /// <summary>
        ///     Weight of the master key.
        /// </summary>
        /// <param name="masterKeyWeight">Number between 0 and 255</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetMasterKeyWeight(uint masterKeyWeight)
        {
            _masterKeyWeight = masterKeyWeight;
            return this;
        }

        public Builder SetMasterKeyWeight(int masterKeyWeight)
        {
            if (masterKeyWeight < 0) throw new ArgumentException("masterKeyWeight must be non negative");
            return SetMasterKeyWeight((uint)masterKeyWeight);
        }

        /// <summary>
        ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a low
        ///     threshold.
        /// </summary>
        /// <param name="lowThreshold">Number between 0 and 255</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetLowThreshold(uint lowThreshold)
        {
            _lowThreshold = lowThreshold;
            return this;
        }

        public Builder SetLowThreshold(int lowThreshold)
        {
            if (lowThreshold < 0) throw new ArgumentException("lowThreshold must be non negative");
            return SetLowThreshold((uint)lowThreshold);
        }

        /// <summary>
        ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a medium
        ///     threshold.
        /// </summary>
        /// <param name="mediumThreshold">Number between 0 and 255</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetMediumThreshold(uint mediumThreshold)
        {
            _mediumThreshold = mediumThreshold;
            return this;
        }

        public Builder SetMediumThreshold(int mediumThreshold)
        {
            if (mediumThreshold < 0) throw new ArgumentException("mediumThreshold must be non negative");
            return SetMediumThreshold((uint)mediumThreshold);
        }

        /// <summary>
        ///     A number from 0-255 representing the threshold this account sets on all operations it performs that have a high
        ///     threshold.
        /// </summary>
        /// <param name="highThreshold">Number between 0 and 255</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetHighThreshold(uint highThreshold)
        {
            _highThreshold = highThreshold;
            return this;
        }

        public Builder SetHighThreshold(int highThreshold)
        {
            if (highThreshold < 0) throw new ArgumentException("highThreshold must be non negative");
            return SetHighThreshold((uint)highThreshold);
        }

        /// <summary>
        ///     Sets the account's home domain address used in
        ///     <a href="https://www.stellar.org/developers/learn/concepts/federation.html" target="_blank">Federation</a>.
        /// </summary>
        /// <param name="homeDomain">A string of the address which can be up to 32 characters.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetHomeDomain(string homeDomain)
        {
            if (homeDomain.Length > 32)
                throw new ArgumentException("Home domain must be <= 32 characters");
            _homeDomain = homeDomain;
            return this;
        }

        /// <summary>
        ///     Add, update, or remove a signer from the account. Signer is deleted if the weight = 0;
        /// </summary>
        /// <param name="signer">The signer's Ed25519 public key.</param>
        /// <param name="weight">The weight to attach to the signer (0-255).</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSigner(string accountId, int weight)
        {
            if (accountId == null) throw new ArgumentNullException(nameof(accountId), "signer cannot be null");
            return SetSigner(SignerUtil.Ed25519PublicKey(KeyPair.FromAccountId(accountId)), weight);
        }

        /// <summary>
        ///     Add, update, or remove a signer from the account. Signer is deleted if the weight = 0;
        /// </summary>
        /// <param name="signer">The signer key. Use <see cref="SignerUtil" /> helper to create this object.</param>
        /// <param name="weight">The weight to attach to the signer (0-255).</param>
        /// <returns>Builder object so you can chain methods.</returns>
        [Obsolete("Use SetSigner(SignerKey signer, int weight) instead.")]
        public Builder SetSigner(SignerKey signer, uint weight)
        {
            _signer = signer ?? throw new ArgumentNullException(nameof(signer), "signer cannot be null");

            _signerWeight = weight & 0xFF;
            return this;
        }

        public Builder SetSigner(SignerKey signer, int weight)
        {
            _signer = signer ?? throw new ArgumentNullException(nameof(signer), "signer cannot be null");
            if (weight is < 0 or > 255)
                throw new ArgumentException("weight must be an integer between 0 and 255 (inclusive).", nameof(weight));
            _signerWeight = (uint)weight;
            return this;
        }

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="sourceAccount">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _sourceAccount = sourceAccount;
            return this;
        }

        /// <summary>
        ///     Builds an operation
        /// </summary>
        public SetOptionsOperation Build()
        {
            var operation = new SetOptionsOperation(_inflationDestination, _clearFlags,
                _setFlags, _masterKeyWeight, _lowThreshold, _mediumThreshold, _highThreshold,
                _homeDomain, _signer, _signerWeight);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}