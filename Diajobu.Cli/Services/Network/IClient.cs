namespace Diajobu.Cli.Services.Network;

internal interface IClient
{
    Task ConnectAsync(string ip, int port);
}