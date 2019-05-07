using System.Threading.Tasks;
using DigitalIcebreakers.Games;
using DigitalIcebreakers.Hubs;
using Microsoft.AspNetCore.SignalR;

public class Broadcast : IGame 
{
    public string Name => "broadcast";

    public async Task Message(string payload, GameHub hub)
    {
        var admin = hub.GetAdmin();
        if (hub.GetPlayerByConnectionId() == admin) {
            await hub.Clients.All.SendAsync("gameUpdate", payload);
        }
    }

    public Task Start(GameHub hub)
    {
        return Task.CompletedTask;
    }
}