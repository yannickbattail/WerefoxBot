using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Werefox.Engine;
using Werefox.Implementations;
using Werefox.Interfaces;
using Xunit;

namespace Werefox.Tests.Engine
{
    public class WerefoxServiceTest
    {
        [Fact]
        internal void ShufflePlayerCards()
        {
            var game = CreateMockedGame(2);
            var service = new WerefoxService(";;")
            {
                CurrentGame = game.Object,
            };

            service.ShufflePlayerCards();

            game.Object.GetAliveWerefoxes().Count().Should().Be(1);
        }

        private static Mock<IPlayer> CreateMockedPlayer(ulong id)
        {
            var player = new Mock<IPlayer>();
            player.Setup(x => x.GetId()).Returns(id);
            player.Setup(x => x.GetMention()).Returns("<!" + id + ">");
            player.Setup(x => x.GetDisplayName()).Returns("player" + id);
            player.Setup(x => x.SendMessageAsync(string.Empty));
            return player;
        }

        private static Mock<BaseGame> CreateMockedGame(int playerNumber)
        {
            var game = new Mock<BaseGame>();
            game.Setup(x => x.SendMessageAsync(string.Empty));
            game.Object.Players = Enumerable.Range(0, playerNumber)
                .Select(i => CreateMockedPlayer((ulong) i).Object)
                .ToList();
            return game;
        }
    }
}
