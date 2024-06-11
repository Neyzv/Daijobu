using Diajobu.Cli.Services.Network;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddTransient<IClient, Client>()
    .BuildServiceProvider();
    
var client = services.GetRequiredService<IClient>();
