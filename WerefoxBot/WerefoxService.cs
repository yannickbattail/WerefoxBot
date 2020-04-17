using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using WerefoxBot.Model;

namespace WerefoxBot
{
    // note that in here we explicitly ask for duration. This is optional,
    // since we set the defaults.
    [SuppressMessage("ReSharper", "CA2007")]
    internal class WerefoxService
    {
        private Game? CurrentGame { get; set; }

        internal bool IsStated() => CurrentGame != null;

        internal async Task sacrifice(CommandContext ctx, string playerToSacrifice)
        {
            GetCurrentPlayer(ctx).Vote = await CheckNickname(ctx, playerToSacrifice, false);
            
            var electedPlayer = GetVotes(ctx, CurrentGame.GetAlivePlayers());
            if (electedPlayer != null)
            {
                await Sacrifice(ctx, electedPlayer);
            }
        }

        private async Task Sacrifice(CommandContext ctx, Player electedPlayer)
        {
            electedPlayer.State = PlayerState.Dead;
            await CurrentGame.Channel.SendMessageAsync(
                $"{electedPlayer.User.Mention} has been sacrificed :dagger: . He was a {Utils.CardToS(electedPlayer.Card)}.");
            await CurrentGame.Channel.SendMessageAsync("Remaining players: " +
                                                       Utils.DisplayPlayerList(CurrentGame.GetAlivePlayers()));
            if (!await CheckWin())
            {
                Night(ctx);
            }
        }

        internal async Task Eat(CommandContext ctx, string playerToEat)
        {
            GetCurrentPlayer(ctx).Vote = await CheckNickname(ctx, playerToEat, true);
            var electedPlayer = GetVotes(ctx, CurrentGame.GetAliveWerefoxes());
            if (electedPlayer != null)
            {
                await Eat(ctx, electedPlayer);
            }
        }

        private async Task Eat(CommandContext ctx, Player electedPlayer)
        {
            electedPlayer.State = PlayerState.Dead;
            await CurrentGame.Channel.SendMessageAsync(
                $"{electedPlayer.User.Mention} has been eaten by werefoxes :fox:. Yum yum :yum:. He was a {Utils.CardToS(electedPlayer.Card)}.");
            await CurrentGame.Channel.SendMessageAsync("Remaining players: " +
                                                       Utils.DisplayPlayerList(CurrentGame.GetAlivePlayers()));
            if (!await CheckWin())
            {
                await Day(ctx);
            }
        }

        private Player? GetVotes(CommandContext ctx, IEnumerable<Player> players)
        {
            if (players.Any(p => p.Vote == null))
            {
                return null;
            }

            var votes = players.GroupBy(p => p.Vote).OrderByDescending(p => p.Count());
            ctx.RespondAsync($"Result of the vote: \r\n"
             + String.Join("\r\n", votes.Select(
                 v => "- " + v.Key.User.Mention + " " + v.Count() + " votes" 
                )));
            Player? playerEaten = votes.First().Key;
            CurrentGame.Players.ForEach(p => p.Vote = null);
            return playerEaten;
        }

        private async Task<bool> CheckWin()
        {
            var alivePlayers = CurrentGame.Players.Where(p => p.State == PlayerState.Alive).ToList();
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
        
        private async Task<Player?> CheckNickname(CommandContext ctx, string playerToEat, bool restrictOnWerefox)
        {
            Player? playerEaten = CurrentGame.GetByName(playerToEat);
            if (playerEaten == null)
            {
                await ctx.RespondAsync($":name_badge: no player with the nickname: {playerToEat}.");
                return null;
            }
            if (playerEaten.State == PlayerState.Dead)
            {
                await ctx.RespondAsync($":name_badge: {playerEaten.User.Mention} is {Utils.AliveToS(PlayerState.Dead)}. Choose somebody else.");
                return null;
            }
            if (restrictOnWerefox && playerEaten.IsWerefox())
            {
                await ctx.RespondAsync($":name_badge: {playerEaten.User.Mention} is a {Utils.CardToS(playerEaten.Card)}. Choose somebody else.");
                return null;
            }

            await ctx.RespondAsync($":envelope_with_arrow: You vote for: ({playerEaten.User.Mention}) !");
            return playerEaten;
        }

        internal async Task Start(CommandContext ctx, IEnumerable<DiscordUser> users)
        {
            var players = users.Select(u => new Player(ctx.Guild.Members[u.Id])).ToList();
            CurrentGame = new Game(ctx.Channel, players);
            if (CurrentGame.Players.Count >= 2)
            {
                await StartGame(ctx);
            }
            else
            {
                await ctx.RespondAsync(":open_mouth:  Nobody? Really? (need at last 2 players)");
                Stop(ctx);
            }
        }
        private async Task StartGame(CommandContext ctx)
        {
            await ctx.RespondAsync(":white_check_mark: Game started with players: " + Utils.DisplayPlayerList(CurrentGame.Players));
            ShuffleWereFoxes();
            TellWereFox();
            Night(ctx);
            //Day(ctx);
        }
        internal async Task Stop(CommandContext ctx)
        {
            CurrentGame = null;
            await ctx.RespondAsync(":stop_sign: Game Stopped.");
        }
        
        internal async Task Status(CommandContext ctx)
        {
            await ctx.RespondAsync($"It's now the {Utils.StepToS(CurrentGame.Step)}.");
            await ctx.RespondAsync(Utils.AliveToS(PlayerState.Alive) + " players are: " +
                                       Utils.DisplayPlayerList(CurrentGame.GetAlivePlayers()));
            await ctx.RespondAsync(Utils.AliveToS(PlayerState.Dead) + " players are: " +
                                       Utils.DisplayPlayerList(CurrentGame.GetDeadPlayers()));
        }

        
        public void ShuffleWereFoxes()
        {
            var indexWereFox = new Random().Next(CurrentGame.Players.Count);
            CurrentGame.Players[indexWereFox].Card = Card.Werefox;
        }

        private async void TellWereFox()
        {
            foreach (var player in CurrentGame.Players)
            {
                player.dmChannel = await player.User.CreateDmChannelAsync();
                await player.dmChannel.SendMessageAsync($"You are a {Utils.CardToS(player.Card)}");
            }
            
            foreach (var werefox in CurrentGame.GetAliveWerefoxes())
            {
                await werefox.dmChannel.SendMessageAsync("The other werefoxes :fox: are: " + Utils.DisplayPlayerList(CurrentGame.GetAliveWerefoxes()));
            }
        }
        
        private async Task Day(CommandContext ctx)
        {
            CurrentGame.Step = GameStep.Day;
            await CurrentGame.Channel.SendMessageAsync(
                "The sun is rising :sunny:, the village awakes.");
            await CurrentGame.Channel.SendMessageAsync(
                $"Now you have to vote for who you will sacrifice :dagger:, use the command {ctx.Prefix}sacrifice NICKNAME ");
        }

        private async void Night(CommandContext ctx)
        {
            CurrentGame.Step = GameStep.Night;
            await ctx.RespondAsync("The night is falling :crescent_moon:. The village is sleeping :sleeping: . The werefoxes :fox: go out!");
            await ctx.RespondAsync("Werefoxes! It's time to decide who you will eat :yum:. Go to the direct message with WereFoxBot.");
            foreach (var werefox in CurrentGame.Players.Where(p => p.IsWerefox()))
            {
                await  werefox.dmChannel.SendMessageAsync($"Werefoxes! :fox: It's time to decide who you will eat :yum:. Send {ctx.Prefix}eat NICKNAME in direct message to WereFoxBot.");
            }
        }

        internal async Task Reveal(CommandContext ctx)
        {
            await ctx.RespondAsync($"REVELATION: {GetCurrentPlayer(ctx).User.Mention} is a {Utils.CardToS(GetCurrentPlayer(ctx).Card)}.");
        }
                
        private Player GetCurrentPlayer(CommandContext ctx)
        {
            var currentPlayer = CurrentGame.GetById(ctx.User.Id);
            if (currentPlayer == null)
            {
                throw new InvalidOperationException("No current player can be found "+ctx.User);
            }
            return currentPlayer;
        }
        
        internal string? CheckCommandContext(CommandContext? ctx,
            bool? needPrivateChannel, GameStep? step, PlayerState? onlyAlivePlayer, Card? onlyCard)
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
                    return prefix + "only in private chanel.";
                }
                if (!needPrivateChannel.Value && ctx.Channel.Type == ChannelType.Private)
                {
                    return prefix + "only in public chanel.";
                }
            }
            if (CurrentGame == null)
            {
                return prefix + "during the game. (No game started)";
            }
            if (step != null && CurrentGame.Step != step)
            {
                return prefix + $"during the {Utils.StepToS(step.Value)}. (It's now the {Utils.StepToS(CurrentGame.Step)})";
            }
            var currentPlayer = CurrentGame.GetById(ctx.User.Id);
            if (currentPlayer == null)
            {
                return prefix + "when you are part of the game.";
            }
            if (onlyAlivePlayer != null && currentPlayer.State != onlyAlivePlayer)
            {
                return prefix + $"when you are {Utils.AliveToS(onlyAlivePlayer.Value)}. (Now you are {Utils.AliveToS(currentPlayer.State)})";
            }
            if (onlyCard != null && currentPlayer.Card != onlyCard)
            {
                return prefix + $"when you are a {Utils.CardToS(onlyCard.Value)}. (Now you are {Utils.CardToS(currentPlayer.Card)})";
            }
            return null;
        }

    }
}