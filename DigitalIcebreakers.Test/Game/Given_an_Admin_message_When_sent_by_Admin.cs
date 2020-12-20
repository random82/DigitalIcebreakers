﻿using DigitalIcebreakers.Games;
using Newtonsoft.Json;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace DigitalIcebreakers.Test
{
    public class Given_an_Admin_message_When_sent_by_Admin
    {
        private MockGame _game;

        public Given_an_Admin_message_When_sent_by_Admin()
        {
            var playerId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var gameHub = ObjectMother.GetMockGameHub(adminId);
            _game = new MockGame(gameHub.Sender, gameHub.Lobbys);
            var lobby = ObjectMother.CreateLobby(gameHub, adminId, _game);
            lobby.Players.Add(ObjectMother.GetPlayer(playerId));
            var payload = JsonConvert.SerializeObject(new {
                admin = new {content = "CONTENT", lane = 0}
            });
            Should.NotThrow(async () => { await gameHub.HubMessage(payload); });
        }

        [Fact]
        public void Then_is_forwarded_to_Game()
        {
            _game.AdminMessages.ShouldNotBeEmpty();
            _game.ClientMessages.ShouldBeEmpty();
            _game.SystemMessages.ShouldBeEmpty();   
        }
    }
}
