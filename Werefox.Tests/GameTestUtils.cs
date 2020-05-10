using System.Linq;
using Moq;
using Werefox.Implementations;
using Werefox.Interfaces;

namespace Werefox.Tests
{
    public class GameTestUtils
    {
        internal static Mock<BaseGame> CreateMockedGame(int playerNumber)
        {
            var game = new Mock<BaseGame>();
            game.Setup(x => x.SendMessageAsync(string.Empty));
            game.Object.Players = Enumerable.Range(0, playerNumber)
                .Select(i => CreateMockedPlayer(i.ToString()).Object)
                .ToList<IPlayer>();
            return game;
        }
        
        internal static Mock<BasePlayer> CreateMockedPlayer(string id)
        {
            var player = new Mock<BasePlayer>();
            player.Setup(x => x.GetId()).Returns(id);
            player.Setup(x => x.GetMention()).Returns("<!" + id + ">");
            player.Setup(x => x.GetDisplayName()).Returns("player" + id);
            player.Setup(x => x.SendMessageAsync(string.Empty));
            return player;
        }
    }
}