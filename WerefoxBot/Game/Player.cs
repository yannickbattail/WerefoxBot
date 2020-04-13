using DSharpPlus.Entities;

namespace WerefoxBot.Game
{
    internal class Player
    {
        public DiscordDmChannel? dmChannel;
        public DiscordMember User { get; set; }
        public Player? Vote { get; set; }
        public bool IsWerefox { get; set; } = false;
        public bool IsAlive { get; set; } = true;
        public string WerefoxToString()
        {
            return  IsWerefox ? "Werefox" : "village people";
        }

        public Player(DiscordMember user)
        {
            User = user;
        }
    }
}
