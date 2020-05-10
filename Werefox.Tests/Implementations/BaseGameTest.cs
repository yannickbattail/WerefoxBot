using FluentAssertions;
using Xunit;

namespace Werefox.Tests.Implementations
{
    public class BaseGameTest
    {
        [Fact]
        internal void GetByNameTest()
        {
            var game = GameTestUtils.CreateMockedGame(2);

            var actual = game.Object.GetByName("player1");

            actual.Should().Be(game.Object.Players[1]);
        }

        [Fact]
        internal void GetByNameAtTest()
        {
            var game = GameTestUtils.CreateMockedGame(2);

            var actual = game.Object.GetByName("@player1");

            actual.Should().Be(game.Object.Players[1]);
        }

        [Fact]
        internal void GetByNameEmptyTest()
        {
            var game = GameTestUtils.CreateMockedGame(2);

            var actual = game.Object.GetByName("player3");

            actual.Should().BeNull();
        }
    }
}