using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using Daijobu.Models.Network.Pipeline;

namespace Daijobu.Models.Network;

public sealed class Session
{
    private readonly Socket _socket;
    private readonly IDuplexPipe _pipe;
    private readonly CancellationTokenSource _cts = new();

    private bool _disposed;

    public int Id { get; }

    public Session(int id, Socket socket)
    {
        _socket = socket;
        _pipe = DuplexPipe.Create(socket);

        Id = id;
    }

    public async Task SendAsync(ReadOnlyMemory<byte> buffer)
    {
        if (!_cts.IsCancellationRequested)
        {
            await _pipe.Output.WriteAsync(buffer, _cts.Token)
                .ConfigureAwait(false);
            await _pipe.Output.FlushAsync()
                .ConfigureAwait(false);
        }
    }

    public async Task StartListeningAsync()
    {
        try
        {
            while (!_cts.IsCancellationRequested)
            {
                var result = await _pipe.Input.ReadAsync(_cts.Token)
                    .ConfigureAwait(false);

                if (result.IsCanceled)
                    break;

                try
                {
                    // Manage message here
                    Console.WriteLine(Encoding.UTF8.GetString(result.Buffer));

                    if (result is { IsCompleted: true, Buffer.IsEmpty: false })
                        throw new InvalidOperationException("Wrong message received...");
                }
                finally
                {
                    _pipe.Input.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                }
            }
        }
        catch
        {
            // LOG HERE
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _cts.CancelAsync()
                .ConfigureAwait(false);
            _cts.Dispose();

            await Task.WhenAll(_pipe.Input.CompleteAsync().AsTask(),
                _pipe.Output.CompleteAsync().AsTask());

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();

            _disposed = true;
        }
    }
}