using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Werefox.Engine;
using WerefoxBot.Implementations;

namespace WerefoxBot
{
    // note that in here we explicitly ask for duration. This is optional,
    // since we set the defaults.
    [SuppressMessage("ReSharper", "CA2007")]
    public class BotCommands : BaseCommandModule
    {
        private WerefoxService Service { get; set; }

        public BotCommands()
        {
            Service = new WerefoxService(ConfigJson.Load().Result.CommandPrefix);
        }
        
        private bool GameInCreation = false;
        
        [Command("start"), Description("Start a new game.")]
        public async Task Start(CommandContext ctx, [Description("How long should last the poll.")] TimeSpan duration)
        {
            try
            {
                if (GameInCreation)
                {
                    await ctx.RespondAsync(":warning: A game is already waiting for player. Join it!");
                    return;
                }
                CheckContext(ctx, false);
                if (Service.IsStated())
                {
                    await ctx.RespondAsync($":no_entry: A game is already started. You can stop it with {ctx.Prefix}stop .");
                    return;
                }
                var emoji = DiscordEmoji.FromName(ctx.Client, ":+1:");
                var interactivity = ctx.Client.GetInteractivity();
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = $"Start a game, who's in ? React {emoji} to join the game. ",
                    Description = $"React {emoji} to join the game. You have {duration} to do it."
                };
                DiscordMessage msg = await ctx.RespondAsync(embed: embed);
                GameInCreation = true;
                await msg.CreateReactionAsync(emoji);

                // wait for anyone who types it
                var reply = await interactivity.CollectReactionsAsync(msg, duration);
                var discordUsers = reply
                    .Where(r => r.Emoji == emoji)
                    .SelectMany(r => r.Users)
                    .Where(u => !u.IsBot);
                GameInCreation = false;
                var players = discordUsers.Select(u => new Player(ctx.Guild.Members[u.Id]));
                var game = new Game(ctx.Channel, players);
                await Service.Start(ctx.User.Id, game);
            }
            catch (CommandContextException e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }

        [Command("stop"), Description("stop the game.")]
        public async Task Stop(CommandContext ctx)
        {
            try
            {
                CheckContext(ctx, false);
                await Service.Stop(ctx.User.Id);
            }
            catch (CommandContextException e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }
        [Command("sacrifice"), Description("Vote for a player to sacrifice")]
        public async Task Sacrifice (CommandContext ctx, [Description("player to sacrifice")]
            string playerToSacrifice)
        {
            try
            {
                CheckContext(ctx, false);
                await Service.Sacrifice(ctx.User.Id, playerToSacrifice);
            }
            catch (CommandContextException e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }

        [Command("eat"), Description("Eat a player")]
        public async Task Eat(CommandContext ctx, [Description("Player you want to eat")]
            string playerToEat)
        {
            try
            {
                CheckContext(ctx, true);
                await Service.Eat(ctx.User.Id, playerToEat);
            }
            catch (CommandContextException e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }

        [Command("leave"), Description("Leave the game.")]
        public async Task Leave(CommandContext ctx)
        {
            try
            {
                CheckContext(ctx, null);
                await Service.Leave(ctx.User.Id);
            }
            catch (CommandContextException e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }
        
        [Command("reveal"), Description("Reveal your card.")]
        public async Task Reveal(CommandContext ctx)
        {
            try
            {
                CheckContext(ctx, false);
                await ctx.RespondAsync("Answer 'yes' to confirm. This will reveal who you are.");
                var interactivity = ctx.Client.GetInteractivity();
                var msg = await interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id,
                    TimeSpan.FromSeconds(60));
                if (!msg.TimedOut && msg.Result.Content.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
                {
                    await Service.Reveal(ctx.User.Id);
                }
            }
            catch (CommandContextException e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }
        
        [Command("whoIsWho"), Description("Reveal the card of every body. (dead player only)")]
        public async Task WhoIsWho(CommandContext ctx)
        {
            try
            {
                CheckContext(ctx, true);
                await Service.WhoIsWho(ctx.User.Id);
            }
            catch (CommandContextException e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }
        
        [Command("status"), Description("Status of the current game.")]
        public async Task Status(CommandContext ctx)
        {
            try
            {
                CheckContext(ctx, null);
                await Service.Status(ctx.User.Id);
            }
            catch (CommandContextException e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }

        private void CheckContext(CommandContext ctx, bool? needPrivateChannel)
        {
            if (ctx == null)
            {
                throw new ArgumentException("missing parameter", nameof(ctx));
            }
            var prefix = $":no_entry: The command {ctx.Prefix}{ctx.Command.Name} must be use ";
            if (needPrivateChannel != null)
            {
                if (needPrivateChannel.Value && ctx.Channel.Type != ChannelType.Private)
                {
                    throw new CommandContextException(prefix + "only in private chanel.");
                }
                if (!needPrivateChannel.Value && ctx.Channel.Type == ChannelType.Private)
                {
                    throw new CommandContextException(prefix + "only in public chanel.");
                }
            }
        }
    }
}
