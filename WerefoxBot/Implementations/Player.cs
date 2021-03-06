﻿using System.Threading.Tasks;
using DSharpPlus.Entities;
using Werefox.Implementations;

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

        public override string GetId()
        {
            return user.Id.ToString();
        }

        public override string GetMention()
        {
            return user.Mention;
        }

        public override string GetDisplayName()
        {
            return user.DisplayName;
        }
    }
}