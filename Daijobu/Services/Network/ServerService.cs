using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Daijobu.Models.Network;

namespace Daijobu.Services.Network;

internal class ServerService
    : IServerService
{
    private readonly ConcurrentDictionary<int, Session> _sessions = new();
    private readonly Socket _socket = new(SocketType.Stream, ProtocolType.Tcp);
    private readonly CancellationTokenSource _cts = new();

    private int _id;
    private bool _disposed;

    ~ServerService() =>
        Dispose(false);

    private async IAsyncEnumerable<Socket> AcceptConnectionAsync()
    {
        while (!_cts.IsCancellationRequested)
            yield return await _socket.AcceptAsync(_cts.Token);
    }

    private void CreateSession(Socket socket)
    {
        var session = new Session(_id++, socket);

        if (_sessions.TryAdd(session.Id, session))
            _ = session.StartListeningAsync()
                .ContinueWith(_ => session.DisposeAsync().AsTask())
                .Unwrap()
                .ConfigureAwait(false);
        else
            _ = session.DisposeAsync().AsTask();
    }

    public async Task StartAsync()
    {
        _socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 443));
        _socket.Listen();

        await foreach (var socket in AcceptConnectionAsync())
            CreateSession(socket);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cts.Cancel();
                _cts.Dispose();

                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}