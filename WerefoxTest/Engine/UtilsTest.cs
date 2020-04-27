using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Werefox.Engine;
using Werefox.Interfaces;
using Xunit;

namespace WerefoxTest.Engine
{
    public class UtilsTest
    {
        [Fact]
        public void DisplayPlayerListEmptyTest()
        {
            var players = new List<IPlayer>();

            var actual = Utils.DisplayPlayerList(players);

            actual.Should().Be("");
        }

        [Fact]
        public void DisplayPlayerListTest()
        {
            var player1 = new Mock<IPlayer>();
            player1.Setup(x => x.GetMention()).Returns("player1");
            var player2 = new Mock<IPlayer>();
            player2.Setup(x => x.GetMention()).Returns("player2");
            var players = new List<IPlayer>()
            {
                player1.Object,
                player2.Object
            };

            var actual = Utils.DisplayPlayerList(players);

            actual.Should().Be("player1, player2");
        }

        [Fact]
        public void CardToSTest()
        {
            Card.Werefox.ToDescription().Should().Be("werefox :fox:");
            Card.VillagePeople.ToDescription().Should().Be("village people :man_farmer:");
            Card.LittleGirl.ToDescription().Should().Be("littleGirl :girl:");
            Card.Seer.ToDescription().Should().Be("seer :crystal_ball:");
            Card.Thief.ToDescription().Should().Be("thief :supervillain:");
            Card.Hunter.ToDescription().Should().Be("hunter :gun:");
            Card.Cupid.ToDescription().Should().Be("Cupid :angel:");
            Card.Witch.ToDescription().Should().Be("witch :woman_mage:");
        }

        [Fact]
        public void AliveToSTest()
        {
            PlayerState.Alive.ToDescription().Should().Be("alive :star_struck:");
            PlayerState.Dead.ToDescription().Should().Be("dead :skull:");
            PlayerState.SchrödingersCat.ToDescription().Should().Be("Schrödinger's cat :scream_cat:");
        }

        [Fact]
        public void StepToSTest()
        {
            GameStep.ThiefStep.ToDescription().Should().Be("thief step :supervillain:");
            GameStep.CupidStep.ToDescription().Should().Be("cupid/lover Step :angel:");
            GameStep.SeerStep.ToDescription().Should().Be("seer :crystal_ball:");
            GameStep.Night.ToDescription().Should().Be("night :crescent_moon:");
            GameStep.WitchStep.ToDescription().Should().Be("witch step :woman_mage:");
            GameStep.Day.ToDescription().Should().Be("day :sunny:");
        }
    }
}