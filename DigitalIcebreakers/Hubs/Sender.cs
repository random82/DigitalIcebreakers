using System;
using System.Linq;
using System.Threading.Tasks;
using DigitalIcebreakers.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DigitalIcebreakers
{
    public class Sender
    {
        private readonly ClientHelper _clients;

        public Sender(ClientHelper clients)
        {
            _clients = clients;
        }

        public async Task SendPayloadToPlayers(Lobby lobby, object payload)
        {
            var clients = _clients.Players(lobby);
            await SendPayload(clients, payload);
        }

        public async Task SendPayloadToPlayer(Player player, object payload)
        {
            var clients = _clients.Player(player);
            await SendPayload(clients, payload);
        }

        private async Task SendPayload(IClientProxy clients, object payload)
        {
            await clients.SendAsync("gameMessage", payload);
        }

        public async virtual Task SendGameMessageToPresenter<T>(Lobby lobby, T payload, Player player = null)
        {
            var client = _clients.Admin(lobby);
            await SendPayload(client, new GameMessage<T>(payload, player));
        }

        public async Task Reconnect(Lobby lobby, Player player)
        {
            var players = lobby.Players.Where(p => !p.IsPresenter).Select(p => new User { Id = p.ExternalId, Name = p.Name }).ToList();
            await _clients.Self(player.ConnectionId).SendAsync("Reconnect", new Reconnect { PlayerId = player.Id, PlayerName = player.Name, LobbyName = lobby.Name, LobbyId = lobby.Id, IsPresenter = player.IsPresenter, Players = players, CurrentGame = lobby.CurrentGame?.Name, IsRegistered = player.IsRegistered });

            if (player.IsPresenter)
                await _clients.Admin(lobby).SendAsync("Players",
                    lobby.Players
                    .Where(p => p.IsConnected && p.IsRegistered && !p.IsPresenter)
                    .Select(p => new { id = p.ExternalId, name = p.Name })
                    .ToArray());
        }

        public async Task PlayerLeft(Lobby lobby, Player player)
        {
            await _clients.Admin(lobby).SendAsync("left", new User { Id = player.ExternalId, Name = player.Name });
        }

        internal async Task CloseLobby(string connectionId, Lobby lobby = null)
        {
            await (lobby != null ? _clients.EveryoneInLobby(lobby) : _clients.Self(connectionId))
                .SendAsync("closelobby");
        }

        internal async Task EndGame(Lobby lobby)
        {
            await _clients.EveryoneInLobby(lobby).SendAsync("endgame");
        }

        internal async Task NewGame(Lobby lobby, string gameName)
        {
            await _clients.EveryoneInLobby(lobby).SendAsync("newgame", gameName);
        }

        internal async Task Joined(Lobby lobby, Player player)
        {
            await _clients.Admin(lobby).SendAsync("joined", new User { Id = player.ExternalId, Name = player.Name });
        }

        internal async Task Connected(string connectionId)
        {
            await _clients.Self(connectionId).SendAsync("Connected");
        }
    }
}