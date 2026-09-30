using System;
using System.Globalization;
using System.Text;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Sep.Sep0012;

/// <summary>
///     Verifies (and, for anchors and tests, computes) the signature SEP-0012 attaches to the status callbacks an
///     anchor POSTs to the URL registered with <c>PUT /customer/callback</c>.
/// </summary>
/// <remarks>
///     <para>
///         The anchor sends a <c>Signature</c> header (or the deprecated <c>X-Stellar-Signature</c>) of the form
///         <c>t=&lt;unix timestamp&gt;, s=&lt;base64 signature&gt;</c>, where the signature is an Ed25519 signature,
///         by the <c>SIGNING_KEY</c> in the anchor's stellar.toml, over the UTF-8 bytes of
///         <c>&lt;timestamp&gt;.&lt;host&gt;.&lt;body&gt;</c>. The wallet's host stops a callback being relayed to another
///         wallet, and the timestamp stops it being replayed after the freshness window. Within the window the same
///         signed callback verifies every time it is delivered, so a wallet that must act on each status change exactly
///         once should deduplicate callbacks itself.
///     </para>
///     <para>
///         SEP-0012 makes checking it the wallet's responsibility: verify the signature against the anchor's
///         <c>SIGNING_KEY</c>, and discard callbacks older than a threshold of one to two minutes. Only then parse the
///         body with <see cref="Responses.GetCustomerInfoResponse.FromJson" />.
///     </para>
///     <para>
///         The host must be spelled exactly as the anchor signs it. SEP-0012 says only "the wallet host"; the
///         reference Anchor Platform signs the host as it appears in the registered callback URL, with
///         <c>:port</c> appended when the URL names a port. <see cref="GetSignedHost" /> derives that string from the
///         URL; <c>new Uri(url).Host</c> does not, because it drops the port and lower-cases the host.
///     </para>
/// </remarks>
public static class KycCallbackSignature
{
    /// <summary>The name of the callback signature header.</summary>
    public const string SignatureHeaderName = "Signature";

    /// <summary>
    ///     The name of the deprecated callback signature header. Wallets should accept it until SEP-0012 removes it.
    /// </summary>
    public const string DeprecatedSignatureHeaderName = "X-Stellar-Signature";

    /// <summary>
    ///     The default freshness window, two minutes: the upper end of the threshold SEP-0012 recommends.
    /// </summary>
    public static TimeSpan DefaultMaxAge { get; } = TimeSpan.FromMinutes(2);

    /// <summary>
    ///     Verifies a callback's signature header against the anchor's signing key and checks that it is fresh.
    /// </summary>
    /// <param name="signatureHeader">
    ///     The value of the <see cref="SignatureHeaderName" /> header, or of <see cref="DeprecatedSignatureHeaderName" />
    ///     if only that one is present.
    /// </param>
    /// <param name="body">The raw callback request body, exactly as received.</param>
    /// <param name="host">
    ///     The host the callback was sent to, derived from the URL registered with <c>PUT /customer/callback</c> —
    ///     use <see cref="GetSignedHost" /> — and not from the request's <c>Host</c> header, which a relaying party
    ///     controls.
    /// </param>
    /// <param name="anchorSigningKey">
    ///     The anchor's <c>SIGNING_KEY</c> from its stellar.toml, for example
    ///     <c>KeyPair.FromAccountId(toml.GeneralInformation.SigningKey)</c>.
    /// </param>
    /// <param name="now">The current time; defaults to <see cref="DateTimeOffset.UtcNow" />.</param>
    /// <param name="maxAge">
    ///     The largest accepted difference between <paramref name="now" /> and the signed timestamp, in either
    ///     direction; defaults to <see cref="DefaultMaxAge" />.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the header is well formed, its timestamp is within <paramref name="maxAge" /> of
    ///     <paramref name="now" />, and the signature verifies; otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="body" />, <paramref name="host" /> or <paramref name="anchorSigningKey" /> is
    ///     <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxAge" /> is negative.</exception>
    public static bool Verify(
        string? signatureHeader,
        string body,
        string host,
        KeyPair anchorSigningKey,
        DateTimeOffset? now = null,
        TimeSpan? maxAge = null)
    {
        if (body == null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        return Verify(signatureHeader, Encoding.UTF8.GetBytes(body), host, anchorSigningKey, now, maxAge);
    }

    /// <summary>
    ///     Verifies a callback's signature header against the raw bytes of its body. Prefer this overload when the
    ///     body is available as bytes: decoding it to a string first can change it (a byte order mark is dropped,
    ///     invalid UTF-8 is replaced), and the signature covers the bytes exactly as the anchor sent them.
    /// </summary>
    /// <param name="signatureHeader">
    ///     The value of the <see cref="SignatureHeaderName" /> header, or of <see cref="DeprecatedSignatureHeaderName" />
    ///     if only that one is present.
    /// </param>
    /// <param name="body">The raw callback request body, exactly as received.</param>
    /// <param name="host">The host the callback was sent to; see <see cref="GetSignedHost" />.</param>
    /// <param name="anchorSigningKey">The anchor's <c>SIGNING_KEY</c> from its stellar.toml.</param>
    /// <param name="now">The current time; defaults to <see cref="DateTimeOffset.UtcNow" />.</param>
    /// <param name="maxAge">
    ///     The largest accepted difference between <paramref name="now" /> and the signed timestamp, in either
    ///     direction; defaults to <see cref="DefaultMaxAge" />.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the header is well formed, its timestamp is within <paramref name="maxAge" /> of
    ///     <paramref name="now" />, and the signature verifies; otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="body" />, <paramref name="host" /> or <paramref name="anchorSigningKey" /> is
    ///     <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxAge" /> is negative.</exception>
    public static bool Verify(
        string? signatureHeader,
        byte[] body,
        string host,
        KeyPair anchorSigningKey,
        DateTimeOffset? now = null,
        TimeSpan? maxAge = null)
    {
        if (body == null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        if (host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        if (anchorSigningKey == null)
        {
            throw new ArgumentNullException(nameof(anchorSigningKey));
        }

        var window = maxAge ?? DefaultMaxAge;
        if (window < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAge), "The freshness window must not be negative.");
        }

        if (!TryParseHeader(signatureHeader, out var timestampText, out var timestamp, out var signature))
        {
            return false;
        }

        var currentSeconds = (now ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
        // Compare in decimal so an adversarial timestamp near long.MaxValue, against a caller-supplied `now` far in
        // the past, cannot overflow the subtraction.
        var age = Math.Abs((decimal)currentSeconds - timestamp);
        if (age > (decimal)window.TotalSeconds)
        {
            return false;
        }

        // Sign over the timestamp exactly as the anchor sent it, so the payload matches byte for byte.
        var prefix = Encoding.UTF8.GetBytes($"{timestampText}.{host}.");
        var payload = new byte[prefix.Length + body.Length];
        Buffer.BlockCopy(prefix, 0, payload, 0, prefix.Length);
        Buffer.BlockCopy(body, 0, payload, prefix.Length, body.Length);
        return anchorSigningKey.Verify(payload, signature);
    }

    /// <summary>
    ///     Derives the host string an anchor signs from the callback URL the wallet registered: the host exactly as
    ///     it is written in the URL, followed by <c>:port</c> when the URL names a port — how the reference Anchor
    ///     Platform builds it. For <c>https://wallet.example.com/kyc</c> this is <c>wallet.example.com</c>; for
    ///     <c>https://wallet.example.com:8443/kyc</c> it is <c>wallet.example.com:8443</c>.
    /// </summary>
    /// <param name="callbackUrl">The absolute http(s) URL registered with <c>PUT /customer/callback</c>.</param>
    /// <returns>The host (and port, when the URL names one) to pass to <see cref="Verify(string, string, string, KeyPair, DateTimeOffset?, TimeSpan?)" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callbackUrl" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="callbackUrl" /> is not an absolute http(s) URL.</exception>
    public static string GetSignedHost(string callbackUrl)
    {
        if (callbackUrl == null)
        {
            throw new ArgumentNullException(nameof(callbackUrl));
        }

        if (!KycService.TryParseHttpUrl(callbackUrl, out var url, out _))
        {
            throw new ArgumentException("The callback URL must be an absolute http(s) URL.", nameof(callbackUrl));
        }

        // Take the authority as written: System.Uri lower-cases the host and hides an explicit default port, while
        // the anchor signs the text of the URL it was given.
        var hostAndPort = KycService.GetAuthorityAsWritten(url);

        // Split off the port after the last ':' that is not inside an IPv6 literal. The anchor reads it as a number
        // (Java's URL.getPort()), so an empty port is dropped and leading zeros are not signed.
        var colon = hostAndPort.LastIndexOf(':');
        if (colon < 0 || colon < hostAndPort.LastIndexOf(']'))
        {
            return hostAndPort;
        }

        var host = hostAndPort.Substring(0, colon);
        var portText = hostAndPort.Substring(colon + 1);
        return portText.Length == 0
            ? host
            : host + ":" + int.Parse(portText, NumberStyles.None, CultureInfo.InvariantCulture)
                .ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Computes a callback signature header value, <c>t=&lt;timestamp&gt;, s=&lt;base64 signature&gt;</c>, as an
    ///     anchor sends it. Wallets need only <see cref="Verify(string, byte[], string, KeyPair, DateTimeOffset?, TimeSpan?)" />; this is for anchor implementations and tests.
    /// </summary>
    /// <param name="signer">The anchor's signing key pair; it must hold the secret seed.</param>
    /// <param name="host">The host of the wallet's callback URL.</param>
    /// <param name="body">The callback request body.</param>
    /// <param name="timestamp">The signing time; defaults to <see cref="DateTimeOffset.UtcNow" />.</param>
    /// <returns>The header value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when an argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="timestamp" /> is before the Unix epoch, which the header cannot express.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="signer" /> holds no secret seed.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when <paramref name="signer" /> has been disposed.</exception>
    public static string CreateSignatureHeader(KeyPair signer, string host, string body,
        DateTimeOffset? timestamp = null)
    {
        if (signer == null)
        {
            throw new ArgumentNullException(nameof(signer));
        }

        if (host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        if (body == null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        var unixSeconds = (timestamp ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
        if (unixSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp), "The timestamp must not be before the Unix epoch.");
        }

        var seconds = unixSeconds.ToString(CultureInfo.InvariantCulture);
        var signature = signer.Sign(Encoding.UTF8.GetBytes($"{seconds}.{host}.{body}"));
        return $"t={seconds}, s={Convert.ToBase64String(signature)}";
    }

    /// <summary>
    ///     Parses <c>t=&lt;timestamp&gt;, s=&lt;base64&gt;</c>. Each key must appear exactly once; unknown keys are
    ///     ignored. A repeated key is rejected rather than resolved first- or last-wins, so a header cannot carry one
    ///     value for this parser and another for a different one.
    /// </summary>
    private static bool TryParseHeader(string? header, out string timestampText, out long timestamp,
        out byte[] signature)
    {
        timestampText = string.Empty;
        timestamp = 0;
        signature = Array.Empty<byte>();
        if (string.IsNullOrWhiteSpace(header))
        {
            return false;
        }

        string? t = null;
        string? s = null;
        foreach (var rawPart in header!.Split(','))
        {
            var part = rawPart.Trim();
            var separator = part.IndexOf('=');
            if (separator <= 0)
            {
                return false;
            }

            var key = part.Substring(0, separator).Trim();
            var value = part.Substring(separator + 1).Trim();
            switch (key)
            {
                case "t":
                    if (t != null)
                    {
                        return false;
                    }

                    t = value;
                    break;
                case "s":
                    if (s != null)
                    {
                        return false;
                    }

                    s = value;
                    break;
            }
        }

        if (t == null || s == null || t.Length == 0 || s.Length == 0)
        {
            return false;
        }

        // Digits only: long.TryParse would also accept a sign, which the anchor never sends.
        foreach (var c in t)
        {
            if (c < '0' || c > '9')
            {
                return false;
            }
        }

        if (!long.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out timestamp))
        {
            return false;
        }

        try
        {
            signature = Convert.FromBase64String(s);
        }
        catch (FormatException)
        {
            return false;
        }

        timestampText = t;
        return true;
    }
}
