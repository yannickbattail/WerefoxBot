using System;
using System.Threading.Tasks;
using Werefox.Interfaces;

namespace Werefox.Implementations
{
    public abstract class BasePlayer : IPlayer
    {
        public IPlayer? Vote { get; set; }
        public Card Card { get; set; } = Card.VillagePeople;

        public bool IsLover { get; set; } = false;
        public PlayerState State { get; set; } = PlayerState.Alive;

        public abstract Task SendMessageAsync(string message);

        public abstract ulong GetId();

        public abstract string GetMention();

        public abstract string GetDisplayName();
    }
}