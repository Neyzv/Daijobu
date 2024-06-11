namespace Daijobu.Services.Network;

public interface IServerService
    : IDisposable
{
    Task StartAsync();
}