using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using WerefoxBot.Interface;

namespace WerefoxBot.Model
{
    internal class Game : IGame
    {
        private readonly DiscordChannel channel;
        public IList<IPlayer> Players { get; set; } = new List<IPlayer>();

        public GameStep Step { get; set; } = GameStep.Night;

        public Game(DiscordChannel channel, IEnumerable<IPlayer> currentGamePlayers)
        {
            this.channel = channel;
            Players = currentGamePlayers.ToList();
        }

        public async Task SendMessageAsync(string message)
        {
            await channel.SendMessageAsync(message);
        }
        
        public IEnumerable<IPlayer> GetAlivePlayers()
        {
            return Players.Where(p => p.State == PlayerState.Alive);
        }
        public IEnumerable<IPlayer> GetDeadPlayers()
        {
            return Players.Where(p => p.State == PlayerState.Dead);
        }
        
        public IEnumerable<IPlayer> GetAliveWerefoxes()
        {
            return GetAlivePlayers().Where(p => p.Card == Card.Werefox);
        }


        public IPlayer? GetByName(string? displayName)
        {
            displayName = displayName.Replace("@", "", StringComparison.InvariantCultureIgnoreCase);
            return Players.FirstOrDefault(p => displayName.Equals(p.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
        }
        
        public IPlayer? GetById(ulong? id)
        {
            return Players.FirstOrDefault(p => p.GetId() == id);
        }
    }
}