﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Werefox.Interfaces;

namespace Werefox.Implementations
{
    public abstract class BaseGame : IGame
    {
        public IList<IPlayer> Players { get; set; }

        public GameStep Step { get; set; } = GameStep.Night;

        public abstract Task SendMessageAsync(string message);

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
            displayName ??= string.Empty;
            displayName = displayName.Replace("@", string.Empty, StringComparison.InvariantCultureIgnoreCase);
            return Players.FirstOrDefault(
                p => displayName.Equals(p.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase)
                );
        }

        public IPlayer? GetById(string? id)
        {
            return Players.FirstOrDefault(p => p.GetId() == id);
        }
    }
}