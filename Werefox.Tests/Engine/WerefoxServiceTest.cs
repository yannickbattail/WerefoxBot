using System.Linq;
using FluentAssertions;
using Werefox.Engine;
using Xunit;

namespace Werefox.Tests.Engine
{
    public class WerefoxServiceTest
    {
        [Fact]
        internal void ShufflePlayerCards()
        {
            var game = GameTestUtils.CreateMockedGame(2);
            var service = new WerefoxService(";;")
            {
                CurrentGame = game.Object,
            };

            service.ShufflePlayerCards();

            game.Object.GetAliveWerefoxes().Count().Should().Be(1);
        }
    }
}
