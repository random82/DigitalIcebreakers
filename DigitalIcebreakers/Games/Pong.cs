﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalIcebreakers.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DigitalIcebreakers.Games
{
    public class Pong : IGame
    {
        public string Name => "pong";

        public Pong() { }

        public Pong (Dictionary<Guid, int> leftTeam, Dictionary<Guid, int> rightTeam)
        {
            _leftTeam = leftTeam;
            _rightTeam = rightTeam;
        }

        Dictionary<Guid, int> _leftTeam = new Dictionary<Guid, int>();
        Dictionary<Guid, int> _rightTeam = new Dictionary<Guid, int>();

        public async Task Message(string payload, GameHub hub)
        {
            var player = hub.GetPlayerByConnectionId();
            var externalId = player.ExternalId;
            switch (payload)
            {
                case "up": Move(1, externalId); break;
                case "down": Move(-1, externalId); break;
                case "release": Move(0, externalId); break;
                case "leave": Leave(externalId); break;
                case "join": await Join(hub, player); break;
                default: return;
            }
            await hub.Clients.Client(hub.GetAdmin().ConnectionId).SendAsync("gameUpdate", new Result(Speed(_leftTeam), Speed(_rightTeam)));
        }

        private decimal Speed(Dictionary<Guid, int> team)
        {
            if (team.Count() == 0)
                return 0;
            return ((decimal)team.Sum(p => p.Value)) / team.Count();
        }

        private void PerformOnDictionary(Guid id, Action<Dictionary<Guid, int>> action)
        {
            if (_leftTeam.ContainsKey(id))
            {
                action(_leftTeam);
            }

            else if (_rightTeam.ContainsKey(id))
            {
                action(_rightTeam);
            }
        }

        private void Move(int direction, Guid id)
        {
            PerformOnDictionary(id, (d) => d[id] = direction);
        }

        internal void Leave(Guid id)
        {
            PerformOnDictionary(id, (d) => d.Remove(id));
        }

        internal async Task Join(GameHub hub, Player player)
        {
            if (!player.IsAdmin)
            {
                var id = player.ExternalId;

                if (_leftTeam.Count <= _rightTeam.Count)
                    _leftTeam[id] = 0;
                else
                    _rightTeam[id] = 0;
                PerformOnDictionary(id, (d) => d[id] = 0);

                await hub.Clients.Client(player.ConnectionId).SendAsync("gameUpdate", GetGameData(player));
            }
        }

        public async Task Start(GameHub hub)
        {
            var players = hub.GetLobby().Players.ToList();

            foreach (var player in players)
            {
                await Join(hub, player);
            }
        }

        public string GetGameData(Player player)
        {
            if (_leftTeam.ContainsKey(player.ExternalId))
                return "team:0";
            else if (_rightTeam.ContainsKey(player.ExternalId))
                return "team:1";

            return null;
        }

        public class Result
        {
            public Result(decimal left, decimal right)
            {
                Left = left;
                Right = right;
            }

            public decimal Left { get; set; }

            public decimal Right { get; set; }
        }
    }
}