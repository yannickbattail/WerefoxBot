using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace WerefoxBot.Implementations
{
    internal class Player : BasePlayer
    {
        private readonly DiscordDmChannel dmChannel;
        private readonly DiscordMember user;

        public Player(DiscordMember user)
        {
            this.user = user;
            dmChannel = user.CreateDmChannelAsync().Result;
        }

        public override async Task SendMessageAsync(string message)
        {
            await dmChannel.SendMessageAsync(message);
        }

        public override ulong GetId()
        {
            return user.Id;
        }
        
        public override String GetMention()
        {
            return user.Mention;
        }
        public override String GetDisplayName()
        {
            return user.DisplayName;
        }

    }
}
