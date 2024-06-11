using System.Net;
using System.Net.Sockets;

namespace Diajobu.Cli.Services.Network;

internal sealed class Client
    : IClient
{
    private readonly Socket _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

    public Task ConnectAsync(string ip, int port) =>
        _socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
}