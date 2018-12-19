﻿using DigitalIcebreakers;
using DigitalIcebreakers.Games;
using DigitalIcebreakers.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;

namespace DigitalIceBreakers.Test
{
    public static class ObjectMother
    {
        public static GameHub GetMockGameHub(Guid playerId, List<Lobby> lobbys)
        {
            var gameHub = new GameHub(new Mock<ILogger<GameHub>>().Object, lobbys);
            var context = new Mock<HubCallerContext>();
            context.Setup(p => p.ConnectionId).Returns(playerId.ToString());
            gameHub.Context = context.Object;
            var clients = new Mock<IHubCallerClients>();
            clients.Setup(p => p.Client(It.IsAny<string>())).Returns(new Mock<IClientProxy>().Object);
            clients.SetupGet(p => p.Caller).Returns(new Mock<IClientProxy>().Object);
            gameHub.Clients = clients.Object;
            return gameHub;
        }

        public static GameHub GetMockGameHub(Guid adminId, IGame game)
        {
            var lobby = new Lobby
            {
                CurrentGame = game,
                Id = Guid.NewGuid(),
                Players = new List<Player> { new Player { ConnectionId = adminId.ToString(), Id = adminId, ExternalId = adminId, IsAdmin = true } }
            };

            return GetMockGameHub(adminId, new List<Lobby> { lobby });
        }
    }
}
