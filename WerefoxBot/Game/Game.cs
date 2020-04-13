using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;

namespace WerefoxBot.Game
{
    internal class Game
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public DiscordChannel Channel { get; set; }

        public bool IsDay { get; set; } = false;

        public Game(DiscordChannel channel)
        {
            Channel = channel;
        }

        public void ShuffleWereFoxes()
        {
            var indexWereFox = new Random().Next(Players.Count);
            Players[indexWereFox].IsWerefox = true;
        }
        
        public void ResetVotes()
        {
            Players.ForEach(p => p.Vote = null);
        }

        public IEnumerable<Player> GetAlivePlayers()
        {
            return Players.Where(p => p.IsAlive);
        }
        
        public IEnumerable<Player> GetWerefoxes()
        {
            return GetAlivePlayers().Where(p => p.IsWerefox);
        }


        public Player? GetByName(string? displayName)
        {
            displayName = displayName.Replace("@", "", StringComparison.InvariantCultureIgnoreCase);
            Player? playerEaten = Players.FirstOrDefault(p => p.User.DisplayName.Equals(displayName, StringComparison.InvariantCultureIgnoreCase));
            return Players.FirstOrDefault(p => p.User.DisplayName == displayName);
        }
        
        public Player? GetById(ulong? id)
        {
            return Players.FirstOrDefault(p => p.User.Id == id);
        }
    }
}