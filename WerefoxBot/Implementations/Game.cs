using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using WerefoxBot.Interfaces;

namespace WerefoxBot.Implementations
{
    internal class Game : BaseGame
    {
        private readonly DiscordChannel channel;
        
        public Game(DiscordChannel channel, IEnumerable<IPlayer> currentGamePlayers)
        {
            this.channel = channel;
            Players = currentGamePlayers.ToList();
        }

        public override async Task SendMessageAsync(string message)
        {
            await channel.SendMessageAsync(message);
        }
    }
}