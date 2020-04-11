using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace WerefoxBot.Game
{
    internal class Game
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public DiscordChannel? WerefoxesChannel { get; set; }

        public void ShuffleWereFoxes()
        {
            var indexWereFox = new Random().Next(Players.Count);
            Players[indexWereFox].IsWereFox = true;
        }
    }
}