using System.Collections.Generic;
using FluentAssertions;
using Moq;
using WerefoxBot.Engine;
using WerefoxBot.Interfaces;
using Xunit;

namespace WerefoxBotTest.Engine
{
    public class UtilsTest
    {
        [Fact]
        public void DisplayPlayerListEmpty()
        {
            var players = new List<IPlayer>();
            
            var actual = Utils.DisplayPlayerList(players);

            actual.Should().Be("");
        }
        
        [Fact]
        public void DisplayPlayerList()
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
    }
}