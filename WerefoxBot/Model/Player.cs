using DSharpPlus.Entities;

namespace WerefoxBot.Model
{
    internal class Player
    {
        public DiscordDmChannel? dmChannel;
        public DiscordMember User { get; private set; }
        public Player? Vote { get; set; }
        public Card Card { get; set; } = Card.VillagePeople;
        public bool IsWerefox() => Card == Card.Werefox;
        public PlayerState State { get; set; } = PlayerState.Alive;
        
        public Player(DiscordMember user)
        {
            User = user;
        }
    }
}
