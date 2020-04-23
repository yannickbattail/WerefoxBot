using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using WerefoxBot.Interface;

namespace WerefoxBot.Implementation
{
    internal class Player : IPlayer
    {
        private readonly DiscordDmChannel dmChannel;
        private readonly DiscordMember user;
        public IPlayer? Vote { get; set; }
        public Card Card { get; set; } = Card.VillagePeople;
        
        public bool IsLover { get; set; } = false;
        public PlayerState State { get; set; } = PlayerState.Alive;

        public Player(DiscordMember user)
        {
            this.user = user;
            dmChannel = user.CreateDmChannelAsync().Result;
        }

        public async Task SendMessageAsync(string message)
        {
            await dmChannel.SendMessageAsync(message);
        }

        public ulong GetId()
        {
            return user.Id;
        }
        
        public String GetMention()
        {
            return user.Mention;
        }
        public String GetDisplayName()
        {
            return user.DisplayName;
        }

    }
}
