using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Tests.Sep.Sep0007.Fixtures;

/// <summary>
///     A raw HTTP/1.1 server on 127.0.0.1, so the HTTP tests can drive the real <see cref="SocketsHttpHandler" />
///     (redirect handling, a body cut off mid-stream, a body trickled byte by byte) instead of a mock handler that
///     answers in-process.
/// </summary>
internal sealed class LoopbackHttpServer : IDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private readonly TcpListener _listener;
    private readonly Func<string, NetworkStream, CancellationToken, Task> _respond;

    /// <param name="respond">
    ///     Writes the raw response for a request, given the request line (e.g. <c>POST /cb HTTP/1.1</c>).
    /// </param>
    public LoopbackHttpServer(Func<string, NetworkStream, CancellationToken, Task> respond)
    {
        _respond = respond;
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        _ = AcceptLoopAsync();
    }

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    /// <summary>Each request's request line, with <c>[xdr]</c> appended when its body carries an <c>xdr</c> field.</summary>
    public ConcurrentQueue<string> Requests { get; } = new();

    public void Dispose()
    {
        _stop.Cancel();
        _listener.Stop();
    }

    public static Task WriteAsync(NetworkStream stream, string text, CancellationToken cancellationToken)
    {
        var bytes = Encoding.ASCII.GetBytes(text);
        return stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
    }

    private async Task AcceptLoopAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync();
            }
            catch (Exception) when (_stop.IsCancellationRequested)
            {
                return;
            }
            _ = HandleAsync(client);
        }
    }

    private async Task HandleAsync(TcpClient client)
    {
        using (client)
        {
            try
            {
                var stream = client.GetStream();
                var request = await ReadRequestAsync(stream);
                var requestLine = request.Substring(0, request.IndexOf("\r\n", StringComparison.Ordinal));
                Requests.Enqueue(requestLine + (request.Contains("\r\n\r\nxdr=") ? " [xdr]" : ""));
                await _respond(requestLine, stream, _stop.Token);
            }
            catch (Exception)
            {
                // The client gave up (timeout, cancellation) or the test tore the server down.
            }
        }
    }

    private static async Task<string> ReadRequestAsync(NetworkStream stream)
    {
        var received = new StringBuilder();
        var buffer = new byte[8192];
        while (true)
        {
            var text = received.ToString();
            var headerEnd = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            if (headerEnd >= 0)
            {
                var contentLength = 0;
                foreach (var line in text.Substring(0, headerEnd).Split(new[] { "\r\n" }, StringSplitOptions.None))
                {
                    if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                    {
                        contentLength = int.Parse(line.Substring("Content-Length:".Length).Trim());
                    }
                }
                if (text.Length >= headerEnd + 4 + contentLength)
                {
                    return text;
                }
            }
            var read = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0)
            {
                return received.ToString();
            }
            received.Append(Encoding.ASCII.GetString(buffer, 0, read));
        }
    }
}

/// <summary>
///     Sends every request to a <see cref="LoopbackHttpServer" /> over plain http, keeping the path. Lets the
///     stellar.toml tests, whose URL is always https://domain/..., reach the loopback server through a real
///     <see cref="SocketsHttpHandler" />.
/// </summary>
internal sealed class RedirectToLoopbackHandler : DelegatingHandler
{
    private readonly int _port;

    public RedirectToLoopbackHandler(int port, bool followRedirects = false)
        : base(new SocketsHttpHandler { AllowAutoRedirect = followRedirects })
    {
        _port = port;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.RequestUri = new Uri($"http://127.0.0.1:{_port}{request.RequestUri!.PathAndQuery}");
        return base.SendAsync(request, cancellationToken);
    }
}
