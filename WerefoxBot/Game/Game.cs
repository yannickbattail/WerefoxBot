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

        public Game(DiscordChannel channel)
        {
            Channel = channel;
        }

        public void ShuffleWereFoxes()
        {
            var indexWereFox = new Random().Next(Players.Count);
            Players[indexWereFox].IsWereFox = true;
        }

        public Player? GetByName(string? displayName)
        {
            return Players.FirstOrDefault(p => p.User.DisplayName == displayName);
        }
        
        public Player? GetById(ulong? id)
        {
            return Players.FirstOrDefault(p => p.User.Id == id);
        }
    }
}