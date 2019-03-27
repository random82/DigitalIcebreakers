﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalIceBreakers.Test.Pong
{
    [TestClass]
    public class Given_unequal_teams_When_a_player_joins
    {

        Guid playerId = Guid.NewGuid();
        Dictionary<Guid, int> _leftTeam = new Dictionary<Guid, int> { { Guid.NewGuid(), 0 } };
        Dictionary<Guid, int> _rightTeam = new Dictionary<Guid, int>();

        public Given_unequal_teams_When_a_player_joins()
        {
            var game = new DigitalIcebreakers.Games.Pong(_leftTeam, _rightTeam);
            var lobby = ObjectMother.GetLobby(Guid.NewGuid(), game);
            lobby.Players.Add(ObjectMother.GetPlayer(playerId));
            var gameHub = ObjectMother.GetMockGameHub(playerId, lobby);
            game.Message("join", gameHub).Wait();
        }

        [TestMethod]
        public void Then_join_them_to_right_team()
        {
            _leftTeam.Count.ShouldBe(1);
            _rightTeam.Count.ShouldBe(1);
        }
    }
}