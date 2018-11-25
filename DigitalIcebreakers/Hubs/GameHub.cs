﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalIcebreakers.Controllers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace DigitalIcebreakers.Hubs
{
    public class GameHub : Hub
    {
        private ILogger<GameHub> _logger;
        public static List<Lobby> Lobbys = new List<Lobby>();

        public GameHub(ILogger<GameHub> logger)
        {
            _logger = logger;
        }

        public void StartGame(Guid id)
        {
            var player = new Player { Name = "Admin", IsAdmin = true, ConnectionId = Context.ConnectionId, Id = Guid.NewGuid() };
            
        }

        public async Task StopGame()
        {
            var games = Lobbys.SelectMany(p => p.Players, (g, p) => new { g, p }).Where(p => p.p.IsAdmin && p.p.ConnectionId == Context.ConnectionId).Select(p => p.g).ToList();
            foreach (var game in games)
            {
                foreach (var player in game.Players)
                {
                    await Clients.Client(player.ConnectionId).SendAsync("stop");
                }
                Lobbys.Remove(Lobbys.Single(p => p.Id == game.Id));
            }
        }

        public async Task<Player> TryRejoin(Guid id)
        {
            var game = Lobbys.SingleOrDefault(g => g.Id == id);
            if (game != null)
                return game.Players.SingleOrDefault(p => p.ConnectionId == Context.ConnectionId);
            await Clients.Caller.SendAsync("Stop");
            return null;
        }

        public async Task CreateLobby(Guid id, string name, User user)
        {
            Lobbys.Add(new Lobby
            {
                Id = id,
                Players = new List<Player>
                {
                    new Player { ConnectionId = Context.ConnectionId, Id = user.Id, IsAdmin = true, IsConnected = true, Name = user.Name }
                },
                Name = name
            });
            await Connect(user);
        }

        public async Task CloseLobby()
        {
            var lobby = Lobbys.SingleOrDefault(l => l.Players.Any(p => p.IsAdmin && p.ConnectionId == Context.ConnectionId));
            if (lobby != null)
            {
                Lobbys.Remove(lobby);
                foreach (var player in lobby.Players)
                {
                    await Clients.Client(player.ConnectionId).SendAsync("closelobby");
                }
            }
        }

        public async Task Connect(User user)
        {
            var player = Lobbys.SelectMany(p => p.Players).SingleOrDefault(p => p.Id == user.Id);
            if (player != null)
            {
                player.ConnectionId = Context.ConnectionId;
            }
            var lobby = Lobbys.SingleOrDefault(p => p.Players.Contains(player));
            if (player != null)
            {
                await Clients.Caller.SendAsync("Reconnect", new Reconnect { Id = player.Id, Name = player.Name, LobbyName = lobby.Name, LobbyId = lobby.Id, IsAdmin = player.IsAdmin, Players = lobby.Players.Where(p => !player.IsAdmin).Select(p => new User { Id = p.ExternalId, Name = p.Name }).ToList() });
            }
            else
                await Clients.Caller.SendAsync("Connected");
        }

        public async Task ConnectToLobby(User user, Guid lobbyId)
        {
            _logger.LogInformation($"Joined: {Context.ConnectionId}");
            var game = Lobbys.SingleOrDefault(p => p.Id == lobbyId);
            var player = new Player { ConnectionId = Context.ConnectionId, Name = user.Name, Id = user.Id };
            if (game != null)
            {
                game.Players.Add(player);

                var adminConnectionId = game.Players.SingleOrDefault(p => p.IsAdmin)?.ConnectionId;
                if (adminConnectionId == null)
                    await Clients.Caller.SendAsync("Stop");
                else
                {
                    var admin = Clients.Client(adminConnectionId);
                    await admin.SendAsync("Joined", new Player { Name = player.Name, Id = player.Id }, game.Players.Count);
                }
            } else
                await Clients.Caller.SendAsync("Stop");
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
