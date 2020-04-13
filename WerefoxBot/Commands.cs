using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using WerefoxBot.Game;
// ReSharper disable CA2007

namespace WerefoxBot
{
    // note that in here we explicitly ask for duration. This is optional,
    // since we set the defaults.
    [SuppressMessage("ReSharper", "CA2007")]
    public class Commands : BaseCommandModule
    {
        private Game.Game? CurrentGame { get; set; }

        [Command("vote"), Description("Vote for a player to sacrifice")]
        public async Task Vote(CommandContext context, [Description("player to sacrifice")]
            string playerToSacrifice)
        {
            if (context == null)
            {
                throw new ArgumentException("missing parameter", nameof(context));
            }
            if (context.Channel.IsPrivate || context.Channel.Type == ChannelType.Private)
            {
                await context.RespondAsync("The command ;;vote must not be use only in private chanel.");
                return;
            }
            if (CurrentGame == null)
            {
                 await context.RespondAsync("No game is started");
                 return;
            }
            var currentPlayer = CurrentGame.GetById(((DiscordDmChannel) context.Channel).Recipients[0].Id);
            if (!currentPlayer.IsAlive)
            {
                await context.RespondAsync("You are dead. :skull:");
                return;
            }

            currentPlayer.Vote = await CheckNickname(context, playerToSacrifice);
            await CheckVotes(context);
        }
                
        private async Task<Player?> CheckNickname(CommandContext context, string playerToEat)
        {
            Player? playerEaten = CurrentGame.GetByName(playerToEat);
            if (playerEaten == null)
            {
                await context.RespondAsync($"no player with this nickname ({playerToEat})");
                return null;
            }
            if (!playerEaten.IsAlive)
            {
                await context.RespondAsync($"{playerEaten.User.Mention}, is dead. Choose somebody else.");
                return null;
            }

            await context.RespondAsync($"You vote to sacrifice: ({playerEaten.User.Mention})");
            return playerEaten;
        }

        private async Task CheckVotes(CommandContext context)
        {
            if (CurrentGame.GetAlivePlayers().Any(p => p.Vote != null))
            {
                var playerEaten = CurrentGame.GetAlivePlayers().GroupBy(p => p.Vote).OrderByDescending(p => p.Count()).First().Key;
                playerEaten.IsAlive = false;
                CurrentGame.ResetVotes();
                await CurrentGame.Channel.SendMessageAsync($"{playerEaten.User.Mention} has been sacrificed. He was a " + playerEaten.WerefoxToString());
                await CurrentGame.Channel.SendMessageAsync("Remaining players: " + DisplayPlayerList(CurrentGame.GetAlivePlayers()));
                if (!await CheckWin())
                {
                    Night(context);
                }
            }
        }
        
        [Command("eat"), Description("Eat a player")]
        public async Task Eat(CommandContext context, [Description("Player you want to eat")]
            string playerToEat)
        {
            if (context == null)
            {
                throw new ArgumentException("missing parameter", nameof(context));
            }
            if (!context.Channel.IsPrivate || context.Channel.Type != ChannelType.Private)
            {
                await context.RespondAsync("The command ;;eat must be use only in private chanel.");
                return;
            }

            if (CurrentGame == null)
            {
                 await context.RespondAsync("No game is started");
                 return;
            }
            var currentPlayer = CurrentGame.GetById(((DiscordDmChannel) context.Channel).Recipients[0].Id);
            if (!currentPlayer.IsWerefox)
            {
                await context.RespondAsync("You are not a werefox, you don't eat people.");
                return;
            }
            if (!currentPlayer.IsAlive)
            {
                await context.RespondAsync("You are dead. :skull:");
                return;
            }

            currentPlayer.Vote = await CheckNicknameWerefox(context, playerToEat);
            await CheckVotesEat();
        }
        
        private async Task CheckVotesEat()
        {
            if (CurrentGame.GetWerefoxes().Any(p => p.Vote != null))
            {
                var playerEaten = CurrentGame.GetWerefoxes().GroupBy(p => p.Vote).OrderByDescending(p => p.Count()).First().Key;
                playerEaten.IsAlive = false;
                CurrentGame.ResetVotes();
                await CurrentGame.Channel.SendMessageAsync($"{playerEaten.User.Mention} has been eaten by werefoxes. He was a " + playerEaten.WerefoxToString());
                await CurrentGame.Channel.SendMessageAsync("Remaining players: " + DisplayPlayerList(CurrentGame.GetAlivePlayers()));
                if (!await CheckWin())
                {
                    await Day();
                }
            }
        }

        private async Task<bool> CheckWin()
        {
            var alivePlayers = CurrentGame.Players.Where(p => p.IsAlive).ToList();
            if (alivePlayers.Count != 1)
            {
                return false;
            }
            var winner = alivePlayers.First();
            await CurrentGame.Channel.SendMessageAsync("The Game has finished.\n"
                                                       + $"And the winner is: {winner.User.Mention}");
            CurrentGame = null;
            return true;
        }
        
        private async Task<Player?> CheckNicknameWerefox(CommandContext context, string playerToEat)
        {
            Player? playerEaten = CurrentGame.GetByName(playerToEat);
            if (playerEaten == null)
            {
                await context.RespondAsync($"no player with this nickname ({playerToEat})");
                return null;
            }
            if (!playerEaten.IsAlive)
            {
                await context.RespondAsync($"{playerEaten.User.Mention}, is dead. Choose somebody else.");
                return null;
            }
            if (playerEaten.IsWerefox)
            {
                await context.RespondAsync($"{playerEaten.User.Mention}, is a werefox. Choose somebody else.");
                return null;
            }

            await context.RespondAsync($"You vote to eat: ({playerEaten.User.Mention})");
            return playerEaten;
        }

        [Command("start"), Description("Start a new game.")]
        public async Task Start(CommandContext context, [Description("How long should the poll last.")] TimeSpan duration)
        {
            if (context == null)
            {
                throw new ArgumentException("missing parameter", nameof(context));
            }
            if (context.Channel.IsPrivate || context.Channel.Type == ChannelType.Private)
            {
                await context.RespondAsync("The command ;;start must not be use only in private chanel.");
                return;
            }
            
            if (CurrentGame != null)
            {
                await context.RespondAsync("A game is already started. You can stop it with ;;stop .");
                return;
            }

            DiscordEmoji emoji = DiscordEmoji.FromName(context.Client, ":+1:");
            // first retrieve the interactivity module from the client
            InteractivityExtension interactivity = context.Client.GetInteractivity();

            // then let's present the poll
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = $"Start a game, who's in ? React {emoji} to join the game. ",
                Description = $"React {emoji} to join the game. You have {duration} to do it."
            };
            DiscordMessage msg = await context.RespondAsync(embed: embed);
            await msg.CreateReactionAsync(emoji);

            CurrentGame = new Game.Game(context.Channel);
            // wait for anyone who types it
            var reply = await interactivity.CollectReactionsAsync(msg, duration);

            if (reply.Any())
            {
                CurrentGame.Players.AddRange(reply
                    .Where(r => r.Emoji == emoji)
                    .SelectMany(r => r.Users)
                    .Where(u => !u.IsBot)
                    .Select(u => new Player( context.Guild.Members[u.Id])));
                await context.RespondAsync("Players: " + DisplayPlayerList(CurrentGame.Players));
                StartGame(context);
            }
            else
            {
                await context.RespondAsync("Nobody? Really?");
            }
        }

        private string DisplayPlayerList(IEnumerable<Player> players)
        {
            return string.Join(", ", players.Select(p => p.User.Mention));
        }

        [Command("stop"), Description("stop a new game.")]
        public async Task Stop(CommandContext context)
        {
            if (context == null)
            {
                throw new ArgumentException("missing parameter", nameof(context));
            }
            if (context.Channel.IsPrivate || context.Channel.Type == ChannelType.Private)
            {
                await context.RespondAsync("The command ;;stop must not be use only in private chanel.");
                return;
            }

            if (CurrentGame == null)
            {
                await context.RespondAsync("No game started.");
            }
            else
            {
                CurrentGame = null;
                await context.RespondAsync("Game Stopped."); 
            }
        }

        private void StartGame(CommandContext context)
        {
            CurrentGame.ShuffleWereFoxes();
            TellWereFox();
            Night(context);
        }
        
        private async void TellWereFox()
        {
            foreach (var player in CurrentGame.Players)
            {
                player.dmChannel = await player.User.CreateDmChannelAsync();
                await player.dmChannel.SendMessageAsync("You are a " + player.WerefoxToString());
            }
            
            foreach (var werefox in CurrentGame.GetWerefoxes())
            {
                await werefox.dmChannel.SendMessageAsync("The other werefoxes are: " + DisplayPlayerList(CurrentGame.GetWerefoxes()));
            }
        }
        
        private async Task Day()
        {
            CurrentGame.IsDay = true;
            await CurrentGame.Channel.SendMessageAsync(
                "The sun is rising, the village awakes. Now you have to vote for who you will sacrifice, use the command ;;vote ");
        }

        private async void Night(CommandContext context)
        {
            CurrentGame.IsDay = false;
            await context.RespondAsync("The night is falling.\n"
                                       +"The village is sleeping.");
            await context.RespondAsync("The werefoxes go out!");
            await context.RespondAsync("Werefoxes! It's time to decide who you will eat. Go to the direct message with WereFoxBot.");
            foreach (var werefox in CurrentGame.Players.Where(p => p.IsWerefox))
            {
                await  werefox.dmChannel.SendMessageAsync("Werefoxes! It's time to decide who you will eat. Send ;;eat NICKNAME in direct message to WereFoxBot.");
            }
        }
    }
}
