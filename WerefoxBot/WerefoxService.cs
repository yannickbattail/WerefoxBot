using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WerefoxBot.Interface;
using WerefoxBot.Model;

namespace WerefoxBot
{
    // note that in here we explicitly ask for duration. This is optional,
    // since we set the defaults.
    [SuppressMessage("ReSharper", "CA2007")]
    internal class WerefoxService
    {
        private Game? CurrentGame { get; set; }
        private readonly string CommandPrefix;

        internal bool IsStated() => CurrentGame != null;

        public WerefoxService()
        {
            CommandPrefix = ConfigJson.Load().Result.CommandPrefix;
        }
        
        internal async Task Sacrifice(ulong currentPlayerId, string playerToSacrifice)
        {
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            currentPlayer.Vote = await CheckNickname(CurrentGame, playerToSacrifice, false);
            
            var electedPlayer = GetVotes(CurrentGame.GetAlivePlayers());
            if (electedPlayer != null)
            {
                await Sacrifice(electedPlayer);
            }
        }

        private async Task Sacrifice(IPlayer electedPlayer)
        {
            await Die(electedPlayer, "has been sacrificed :dagger:.");
            if (!await CheckWin())
            {
                Night();
            }
        }

        internal async Task Eat(ulong currentPlayerId, string playerToEat)
        {
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            currentPlayer.Vote = await CheckNickname(currentPlayer, playerToEat, true);
            var electedPlayer = GetVotes(CurrentGame.GetAliveWerefoxes());
            if (electedPlayer != null)
            {
                await Eat(electedPlayer);
            }
        }

        private async Task Eat(IPlayer electedPlayer)
        {
            await Die(electedPlayer, "has been eaten by werefoxes :fox:. Yum yum :yum:.");
            if (!await CheckWin())
            {
                await Day();
            }
        }

        private IPlayer? GetVotes(IEnumerable<IPlayer> players)
        {
            if (players.Any(p => p.Vote == null))
            {
                return null;
            }

            var votes = players.GroupBy(p => p.Vote).OrderByDescending(p => p.Count());
            CurrentGame.SendMessageAsync($"Result of the vote: \r\n"
             + String.Join("\r\n", votes.Select(
                 v => "- " + v.Key.GetMention() + " " + v.Count() + " votes" 
                )));
            IPlayer? playerEaten = votes.First().Key;
            foreach (var p in CurrentGame.Players)
            {
                p.Vote = null;
            }
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
            await CurrentGame.SendMessageAsync("The Game has finished.\n"
                                                       + $"And the winner is: {winner.GetMention()}");
            Stop();
            return true;
        }
        
        private async Task<IPlayer?> CheckNickname(ISendMessage sendMessage, string playerToEat, bool restrictOnWerefox)
        {
            IPlayer? playerEaten = CurrentGame.GetByName(playerToEat);
            if (playerEaten == null)
            {
                await sendMessage.SendMessageAsync($":name_badge: no player with the nickname: {playerToEat}.");
                return null;
            }
            if (playerEaten.State == PlayerState.Dead)
            {
                await sendMessage.SendMessageAsync($":name_badge: {playerEaten.GetMention()} is {Utils.AliveToS(PlayerState.Dead)}. Choose somebody else.");
                return null;
            }
            if (restrictOnWerefox && playerEaten.Card == Card.Werefox)
            {
                await sendMessage.SendMessageAsync($":name_badge: {playerEaten.GetMention()} is a {Utils.CardToS(playerEaten.Card)}. Choose somebody else.");
                return null;
            }

            await sendMessage.SendMessageAsync($":envelope_with_arrow: You vote for: ({playerEaten.GetMention()}) !");
            return playerEaten;
        }

        private async Task Die(IPlayer player, string message)
        {
            player.State = PlayerState.Dead;
            await CurrentGame.SendMessageAsync(
                $"{player.GetMention()} {message} He was a {Utils.CardToS(player.Card)}.");
            await CurrentGame.SendMessageAsync("Remaining players: " +
                                                       Utils.DisplayPlayerList(CurrentGame.GetAlivePlayers()));
        }

        internal async Task Start(Game game)
        {
            CurrentGame = game;
            if (CurrentGame.Players.Count >= 2)
            {
                await StartGame();
            }
            else
            {
                await CurrentGame.SendMessageAsync(":open_mouth:  Nobody? Really? (need at last 2 players)");
                Stop();
            }
        }
        private async Task StartGame()
        {
            await CurrentGame.SendMessageAsync(":white_check_mark: Game started with players: " + Utils.DisplayPlayerList(CurrentGame.Players));
            ShufflePlayerCards();
            TellWereFox();
            Night();
        }
        internal async Task Stop()
        {
            await CurrentGame.SendMessageAsync(":stop_sign: Game Ended.");
            CurrentGame = null;
        }
        
        public void ShufflePlayerCards()
        {
            var indexWereFox = new Random().Next(CurrentGame.Players.Count);
            CurrentGame.Players[indexWereFox].Card = Card.Werefox;
        }

        private async void TellWereFox()
        {
            foreach (var player in CurrentGame.Players)
            {
                await player.SendMessageAsync($"You are a {Utils.CardToS(player.Card)}");
            }
            
            foreach (var werefox in CurrentGame.GetAliveWerefoxes())
            {
                await werefox.SendMessageAsync("The other werefoxes :fox: are: " + Utils.DisplayPlayerList(CurrentGame.GetAliveWerefoxes()));
            }
        }
        
        private async Task Day()
        {
            CurrentGame.Step = GameStep.Day;
            await CurrentGame.SendMessageAsync("The sun is rising :sunny:, the village awakes.");
            await CurrentGame.SendMessageAsync($"Now you have to vote for who you will sacrifice :dagger:, use the command {CommandPrefix}sacrifice NICKNAME ");
        }

        private async void Night()
        {
            CurrentGame.Step = GameStep.Night;
            await CurrentGame.SendMessageAsync("The night is falling :crescent_moon:. The village is sleeping :sleeping: . The werefoxes :fox: go out!");
            await CurrentGame.SendMessageAsync("Werefoxes! It's time to decide who you will eat :yum:. Go to the direct message with WereFoxBot.");
            foreach (var werefox in CurrentGame.Players.Where(p => p.Card == Card.Werefox))
            {
                await  werefox.SendMessageAsync($"Werefoxes! :fox: It's time to decide who you will eat :yum:. Send {CommandPrefix}eat NICKNAME in direct message to WereFoxBot.");
            }
        }

        public async Task Leave(ulong currentPlayerId)
        {
            await Die(GetCurrentPlayer(currentPlayerId), "has left the game :door:.");
            await CheckWin();
        }
        
        internal async Task Reveal(ulong currentPlayerId)
        {
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            await currentPlayer.SendMessageAsync($"REVELATION: {currentPlayer.GetMention()} is a {Utils.CardToS(currentPlayer.Card)}.");
        }
                
        private IPlayer GetCurrentPlayer(ulong playerId)
        {
            var currentPlayer = CurrentGame.GetById(playerId);
            if (currentPlayer == null)
            {
                throw new InvalidOperationException("No current player can be found "+playerId);
            }
            return currentPlayer;
        }

        public async Task WhoIsWho(ulong currentPlayerId)
        {
            await GetCurrentPlayer(currentPlayerId).SendMessageAsync($"Result of the vote: \r\n"
                                                                     + String.Join("\r\n", CurrentGame.Players.Select(PlayerStatus)));
        }

        private static string PlayerStatus(IPlayer p)
        {
            return $"- {p.GetMention()} is {Utils.AliveToS(p.State)} and is a  {Utils.CardToS(p.Card)}.";
        }
                
        internal async Task Status()
        {
            await CurrentGame.SendMessageAsync($"It's now the {Utils.StepToS(CurrentGame.Step)}.");
            await CurrentGame.SendMessageAsync(Utils.AliveToS(PlayerState.Alive) + " players are: " +
                                               Utils.DisplayPlayerList(CurrentGame.GetAlivePlayers()));
            await CurrentGame.SendMessageAsync(Utils.AliveToS(PlayerState.Dead) + " players are: " +
                                               Utils.DisplayPlayerList(CurrentGame.GetDeadPlayers()));
        }
        


        internal string? CheckPlayerStatus(ulong currentPlayerId, GameStep? step, PlayerState? onlyAlivePlayer, Card? onlyCard,
            string prefix)
        {
            if (CurrentGame == null)
            {
                return prefix + "during the game. (No game started)";
            }
            if (step != null && CurrentGame.Step != step)
            {
                return prefix + $"during the {Utils.StepToS(step.Value)}. (It's now the {Utils.StepToS(CurrentGame.Step)})";
            }

            var currentPlayer = GetCurrentPlayer(currentPlayerId);
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