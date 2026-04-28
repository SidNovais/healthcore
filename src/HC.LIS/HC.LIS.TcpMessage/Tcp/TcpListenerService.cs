using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.TcpMessage.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HC.LIS.TcpMessage.Tcp;

internal sealed partial class TcpListenerService : BackgroundService
{
    private readonly TcpOptions _options;
    private readonly ConnectionHandler _handler;
    private readonly ILogger<TcpListenerService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private X509Certificate2? _tlsCert;

    public TcpListenerService(
        IOptions<TcpOptions> options,
        ConnectionHandler handler,
        ILogger<TcpListenerService> logger)
    {
        _options = options.Value;
        _handler = handler;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.UseTls)
        {
            if (!File.Exists(_options.TlsCertificatePath))
                throw new InvalidOperationException(
                    $"TLS certificate not found: {_options.TlsCertificatePath}");

            _tlsCert = X509CertificateLoader.LoadPkcs12FromFile(
                _options.TlsCertificatePath,
                _options.TlsCertificatePassword);
        }

        using var listener = new TcpListener(IPAddress.Any, _options.Port);
        listener.Start();
        Log.ListeningOnPort(_logger, _options.Port);

        while (!stoppingToken.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await listener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            _ = HandleConnectionAsync(client, stoppingToken);
        }
    }

    private async Task HandleConnectionAsync(TcpClient client, CancellationToken ct)
    {
        string remoteIp = (client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "unknown";
        Stream stream = client.GetStream();

        try
        {
            if (_options.UseTls)
            {
                var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
                stream = sslStream;
#pragma warning disable CA5398
                await sslStream.AuthenticateAsServerAsync(
                    new SslServerAuthenticationOptions
                    {
                        ServerCertificate = _tlsCert,
                        EnabledSslProtocols = SslProtocols.Tls13
                    },
                    ct).ConfigureAwait(false);
#pragma warning restore CA5398
            }

            await _semaphore.WaitAsync(ct).ConfigureAwait(false);

            // Semaphore is released by ConnectionHandler in its own finally block.
            await _handler.HandleAsync(stream, remoteIp, _semaphore, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown
        }
        catch (AuthenticationException ex)
        {
            Log.TlsHandshakeFailed(_logger, ex, remoteIp);
        }
        catch (IOException ex)
        {
            Log.IoError(_logger, ex, remoteIp);
        }
        finally
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            client.Dispose();
        }
    }

    public override void Dispose()
    {
        _semaphore.Dispose();
        _tlsCert?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Listening on port {Port}")]
        internal static partial void ListeningOnPort(ILogger logger, int port);

        [LoggerMessage(Level = LogLevel.Error, Message = "TLS handshake failed from {RemoteIp}")]
        internal static partial void TlsHandshakeFailed(ILogger logger, Exception ex, string remoteIp);

        [LoggerMessage(Level = LogLevel.Error, Message = "IO error from {RemoteIp}")]
        internal static partial void IoError(ILogger logger, Exception ex, string remoteIp);
    }
}
