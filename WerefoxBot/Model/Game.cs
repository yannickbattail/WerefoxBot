using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;

namespace WerefoxBot.Model
{
    internal class Game
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public DiscordChannel Channel { get; set; }

        public GameStep Step { get; set; } = GameStep.Night;

        public Game(DiscordChannel channel, List<Player> currentGamePlayers)
        {
            Channel = channel;
            Players = currentGamePlayers;
        }
        
        public IEnumerable<Player> GetAlivePlayers()
        {
            return Players.Where(p => p.State == PlayerState.Alive);
        }
        public IEnumerable<Player> GetDeadPlayers()
        {
            return Players.Where(p => p.State == PlayerState.Dead);
        }
        
        public IEnumerable<Player> GetAliveWerefoxes()
        {
            return GetAlivePlayers().Where(p => p.IsWerefox());
        }


        public Player? GetByName(string? displayName)
        {
            displayName = displayName.Replace("@", "", StringComparison.InvariantCultureIgnoreCase);
            return Players.FirstOrDefault(p => displayName.Equals(p.User.DisplayName, StringComparison.InvariantCultureIgnoreCase));
        }
        
        public Player? GetById(ulong? id)
        {
            return Players.FirstOrDefault(p => p.User.Id == id);
        }
    }
}