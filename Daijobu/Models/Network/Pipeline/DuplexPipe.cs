using System.IO.Pipelines;
using System.Net.Sockets;

namespace Daijobu.Models.Network.Pipeline;

internal sealed class DuplexPipe : IDuplexPipe
{
    public PipeReader Input { get; }

    public PipeWriter Output { get; }

    private DuplexPipe(PipeReader input, PipeWriter output)
    {
        Input = input;
        Output = output;
    }

    public static IDuplexPipe Create(Socket socket)
    {
        var networkStream = new NetworkStream(socket);

        return new DuplexPipe(PipeReader.Create(networkStream), PipeWriter.Create(networkStream));
    }
}