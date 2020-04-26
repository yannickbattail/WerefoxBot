using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Werefox.Engine;
using Werefox.Implementations;
using Werefox.Interfaces;
using Xunit;

namespace WerefoxTest.Implementations
{
    public class BaseGameTest
    {

        [Fact]
        internal void GetByNameTest()
        {
            var player1 = new Mock<IPlayer>();
            player1.Setup(x => x.GetDisplayName()).Returns("player1");
            var player2 = new Mock<IPlayer>();
            player2.Setup(x => x.GetDisplayName()).Returns("player2");
            var game = new Mock<BaseGame>();
            game.Object.Players = new List<IPlayer>()
            {
                player1.Object,
                player2.Object,
            };

            var actual = game.Object.GetByName("player2");

            actual.Should().Be(player2.Object);
        }

        [Fact]
        internal void GetByNameAtTest()
        {
            var player1 = new Mock<IPlayer>();
            player1.Setup(x => x.GetDisplayName()).Returns("player1");
            var player2 = new Mock<IPlayer>();
            player2.Setup(x => x.GetDisplayName()).Returns("player2");
            var game = new Mock<BaseGame>();
            game.Object.Players = new List<IPlayer>()
            {
                player1.Object,
                player2.Object,
            };

            var actual = game.Object.GetByName("@player2");

            actual.Should().Be(player2.Object);
        }

        [Fact]
        internal void GetByNameEmptyTest()
        {
            var player1 = new Mock<IPlayer>();
            player1.Setup(x => x.GetDisplayName()).Returns("player1");
            var player2 = new Mock<IPlayer>();
            player2.Setup(x => x.GetDisplayName()).Returns("player2");
            var game = new Mock<BaseGame>();
            game.Object.Players = new List<IPlayer>()
            {
                player1.Object,
                player2.Object,
            };

            var actual = game.Object.GetByName("player3");

            actual.Should().BeNull();
        }

        [Fact]
        internal void StartTest()
        {
            var game = new Mock<BaseGame>();
            game.Setup(x => x.SendMessageAsync(""));
            var players = new List<IPlayer>()
            {
                CreateMockedPlayer(1).Object,
                CreateMockedPlayer(2).Object,
            };
            var service = new WerefoxService(";;");
            service.Start(1, game.Object);

            var actual = game.Object.GetByName("player3");

            actual.Should().BeNull();
        }

        private Mock<IPlayer> CreateMockedPlayer(ulong id)
        {
            var player = new Mock<IPlayer>();
            player.Setup(x => x.GetId()).Returns(id);
            player.Setup(x => x.GetMention()).Returns("<!"+id+">");
            player.Setup(x => x.GetDisplayName()).Returns("player"+id);
            player.Setup(x => x.SendMessageAsync(""));
            return player;
        }
    }
}