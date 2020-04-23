using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using WerefoxBot.Interface;
using WerefoxBot.Model;

namespace WerefoxBot
{
    // note that in here we explicitly ask for duration. This is optional,
    // since we set the defaults.
    [SuppressMessage("ReSharper", "CA2007")]
    public class Commands : BaseCommandModule
    {
        private WerefoxService Service { get; set; } = new WerefoxService();

        [Command("sacrifice"), Description("Vote for a player to sacrifice")]
        public async Task Sacrifice (CommandContext ctx, [Description("player to sacrifice")]
            string playerToSacrifice)
        {
            var errorMessage = Service.CheckCommandContext(ctx, false, GameStep.Day, PlayerState.Alive, null);
            if (errorMessage != null)
            {
                await ctx.RespondAsync(errorMessage);
                return;
            }

            await Service.Sacrifice(ctx, playerToSacrifice);
        }

        [Command("eat"), Description("Eat a player")]
        public async Task Eat(CommandContext ctx, [Description("Player you want to eat")]
            string playerToEat)
        {
            var errorMessage = Service.CheckCommandContext(ctx, true, GameStep.Night, PlayerState.Alive, Card.Werefox);
            if (errorMessage != null)
            {
                ctx.RespondAsync(errorMessage);
                return;
            }
            await Service.Eat(ctx, playerToEat);
        }

        private bool GameInCreation = false;
        
        [Command("start"), Description("Start a new game.")]
        public async Task Start(CommandContext ctx, [Description("How long should last the poll.")] TimeSpan duration)
        {
            if (ctx == null)
            {
                throw new ArgumentException("missing parameter", nameof(ctx));
            }
            if (ctx.Channel.IsPrivate || ctx.Channel.Type == ChannelType.Private)
            {
                var prefix = $":no_entry: The command {ctx.Prefix}{ctx.Command.Name} must be use ";
                await ctx.RespondAsync(prefix + "only in private chanel.");
                return;
            }
            if (GameInCreation)
            {
                await ctx.RespondAsync(":warning: A game is already waiting for player. Join it!");
                return;
            }
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
            await Service.Start(ctx, players);
        }

        [Command("stop"), Description("stop the game.")]
        public async Task Stop(CommandContext ctx)
        {
            var errorMessage = Service.CheckCommandContext(ctx, false, null, null, null);
            if (errorMessage != null)
            {
                ctx.RespondAsync(errorMessage);
                return;
            }
            await Service.Stop(ctx);
        }

        [Command("leave"), Description("Leave the game.")]
        public async Task Leave(CommandContext ctx)
        {
            var errorMessage = Service.CheckCommandContext(ctx, false, null, null, null);
            if (errorMessage != null)
            {
                ctx.RespondAsync(errorMessage);
                return;
            }
            await Service.Leave(ctx);
        }
        
        [Command("reveal"), Description("Reveal your card.")]
        public async Task Reveal(CommandContext ctx)
        {
            var errorMessage = Service.CheckCommandContext(ctx, false, null, null, null);
            if (errorMessage != null)
            {
                ctx.RespondAsync(errorMessage);
                return;
            }
            await ctx.RespondAsync("Answer 'yes' to confirm. This will reveal who you are.");
            var interactivity = ctx.Client.GetInteractivity();
            var msg = await interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id, TimeSpan.FromSeconds(60));
            if (!msg.TimedOut && msg.Result.Content.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                await Service.Reveal(ctx);
            }
        }
        
        [Command("whoIsWho"), Description("Reveal the card of every body. (dead player only)")]
        public async Task WhoIsWho(CommandContext ctx)
        {
            var errorMessage = Service.CheckCommandContext(ctx, true, null, PlayerState.Dead, null);
            if (errorMessage != null)
            {
                ctx.RespondAsync(errorMessage);
                return;
            }
            await Service.WhoIsWho(ctx);
        }
        
        [Command("status"), Description("Status of the current game.")]
        public async Task Status(CommandContext? ctx)
        {
            var errorMessage = Service.CheckCommandContext(ctx, null, null, null, null);
            if (errorMessage != null)
            {
                ctx.RespondAsync(errorMessage);
                return;
            }
            await Service.Status(ctx);
        }
    }
}
