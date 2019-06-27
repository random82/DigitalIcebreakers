using System.Threading.Tasks;
using DigitalIcebreakers.Games;
using DigitalIcebreakers.Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

public class StartStopContinue : IGame 
{
    public string Name => "startstopcontinue";
    
    public async Task JsonMessage(dynamic payload, GameHub hub)
    {
        var player = hub.GetPlayerByConnectionId();

        string idea = payload.idea;

        if (!string.IsNullOrWhiteSpace(idea))
            await hub.Clients.Client(hub.GetAdmin().ConnectionId).SendAsync("gameUpdate", player.Name,  idea);
    }

    public Task Start(GameHub hub)
    {
        return Task.CompletedTask;
    }
}