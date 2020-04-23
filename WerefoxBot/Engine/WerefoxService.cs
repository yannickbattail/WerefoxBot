using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WerefoxBot.Implementations;
using WerefoxBot.Interfaces;

namespace WerefoxBot.Engine
{
    [SuppressMessage("ReSharper", "CA2007")]
    [SuppressMessage("ReSharper", "CA1303")]
    internal class WerefoxService
    {
        private IGame? CurrentGame { get; set; }
        private readonly string CommandPrefix;

        public bool IsStated() => CurrentGame != null;

        public async Task Start(ulong currentPlayerId, Game game)
        {
            CurrentGame = game;
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            CheckPlayerStatus(currentPlayer, null, null, null, "Stop");
            if (CurrentGame.Players.Count >= 2)
            {
                await StartGame();
            }
            else
            {
                await CurrentGame.SendMessageAsync(":open_mouth:  Nobody? Really? (need at last 2 players)");
                await StopGame();
            }
        }
        private async Task StartGame()
        {
            await CurrentGame.SendMessageAsync(":white_check_mark: Game started with players: " + Utils.DisplayPlayerList(CurrentGame.Players));
            ShufflePlayerCards();
            await TellCardToPlayers();
            await Night();
        }

        
        private void ShufflePlayerCards()
        {
            var indexWereFox = new Random().Next(CurrentGame.Players.Count);
            CurrentGame.Players[indexWereFox].Card = Card.Werefox;
        }

        private async Task TellCardToPlayers()
        {
            foreach (var player in CurrentGame.GetAlivePlayers())
            {
                await player.SendMessageAsync($"You are a {Utils.CardToS(player.Card)}");
            }
            foreach (var werefox in CurrentGame.GetAliveWerefoxes())
            {
                await werefox.SendMessageAsync("The other werefoxes :fox: are: " + Utils.DisplayPlayerList(CurrentGame.GetAliveWerefoxes()));
            }
        }
        
        public async Task Stop(ulong currentPlayerId)
        {
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            CheckPlayerStatus(currentPlayer, null, null, null, "Stop");
            await StopGame();
        }    
        private async Task StopGame()
        {
            await CurrentGame.SendMessageAsync(":stop_sign: Game Ended.");
            CurrentGame = null;
        }
        
        private async Task Day()
        {
            CurrentGame.Step = GameStep.Day;
            await CurrentGame.SendMessageAsync("The sun is rising :sunny:, the village awakes.");
            await CurrentGame.SendMessageAsync($"Now you have to vote for who you will sacrifice :dagger:, use the command {CommandPrefix}sacrifice NICKNAME ");
        }

        private async Task Night()
        {
            CurrentGame.Step = GameStep.Night;
            await CurrentGame.SendMessageAsync("The night is falling :crescent_moon:. The village is sleeping :sleeping: . The werefoxes :fox: go out!");
            await CurrentGame.SendMessageAsync("Werefoxes! It's time to decide who you will eat :yum:. Go to the direct message with WereFoxBot.");
            foreach (var werefox in CurrentGame.Players.Where(p => p.Card == Card.Werefox))
            {
                await  werefox.SendMessageAsync($"Werefoxes! :fox: It's time to decide who you will eat :yum:. Send {CommandPrefix}eat NICKNAME in direct message to WereFoxBot.");
            }
        }

        public WerefoxService()
        {
            CommandPrefix = ConfigJson.Load().Result.CommandPrefix;
        }
        
        public async Task Sacrifice(ulong currentPlayerId, string playerToSacrifice)
        {
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            CheckPlayerStatus(currentPlayer, GameStep.Day, PlayerState.Alive, null, "Sacrifice");
            currentPlayer.Vote = await CheckNickname(CurrentGame, playerToSacrifice, false);
            
            var electedPlayer = GetVotes(CurrentGame.GetAlivePlayers());
            if (electedPlayer != null)
            {
                await SacrificePlayer(electedPlayer);
            }
        }

        private async Task SacrificePlayer(IPlayer electedPlayer)
        {
            await Die(electedPlayer, "has been sacrificed :dagger:.");
            if (!await CheckWin())
            {
                await Night();
            }
        }

        public async Task Eat(ulong currentPlayerId, string playerToEat)
        {
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            CheckPlayerStatus(currentPlayer, GameStep.Night, PlayerState.Alive, Card.Werefox, "Eat");
            currentPlayer.Vote = await CheckNickname(currentPlayer, playerToEat, true);
            var electedPlayer = GetVotes(CurrentGame.GetAliveWerefoxes());
            if (electedPlayer != null)
            {
                await EatPlayer(electedPlayer);
            }
        }

        private async Task EatPlayer(IPlayer electedPlayer)
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
            await StopGame();
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
            await CurrentGame.SendMessageAsync($"{player.GetMention()} {message} He was a {Utils.CardToS(player.Card)}.");
            await CurrentGame.SendMessageAsync("Remaining players: " +
                                                       Utils.DisplayPlayerList(CurrentGame.GetAlivePlayers()));
        }
        
        public async Task Leave(ulong currentPlayerId)
        {
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            CheckPlayerStatus(currentPlayer, null, PlayerState.Alive, null, "Leave");
            await Die(currentPlayer, "has left the game :door:.");
            await CheckWin();
        }
        
        public async Task Reveal(ulong currentPlayerId)
        {
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            CheckPlayerStatus(currentPlayer, null, PlayerState.Alive, null, "Reveal");
            await currentPlayer.SendMessageAsync($"REVELATION: {currentPlayer.GetMention()} is a {Utils.CardToS(currentPlayer.Card)}.");
        }

        public async Task WhoIsWho(ulong currentPlayerId)
        {
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            CheckPlayerStatus(currentPlayer, null, PlayerState.Dead, null, "WhoIsWho");
            var statuses = CurrentGame.Players.Select(
                    p => $"- {p.GetMention()} is {Utils.AliveToS(p.State)} and is a  {Utils.CardToS(p.Card)}."
                );
            await currentPlayer.SendMessageAsync($"Result of the vote: \r\n" + String.Join("\r\n", statuses));
        }
        
        public async Task Status(ulong currentPlayerId)
        {
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            CheckPlayerStatus(currentPlayer, null, null, null, "Status");
            await CurrentGame.SendMessageAsync($"It's now the {Utils.StepToS(CurrentGame.Step)}.");
            await CurrentGame.SendMessageAsync(Utils.AliveToS(PlayerState.Alive) + " players are: " +
                                               Utils.DisplayPlayerList(CurrentGame.GetAlivePlayers()));
            await CurrentGame.SendMessageAsync(Utils.AliveToS(PlayerState.Dead) + " players are: " +
                                               Utils.DisplayPlayerList(CurrentGame.GetDeadPlayers()));
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

        internal void CheckPlayerStatus(IPlayer currentPlayer, GameStep? step, PlayerState? onlyAlivePlayer, Card? onlyCard,
            string commandName)
        {
            var prefix = $":no_entry: The command {CommandPrefix}{commandName} must be use ";
            if (CurrentGame == null)
            {
                throw new CommandContextException(prefix + "during the game. (No game started)");
            }
            if (step != null && CurrentGame.Step != step)
            {
                throw new CommandContextException(prefix + $"during the {Utils.StepToS(step.Value)}. (It's now the {Utils.StepToS(CurrentGame.Step)})");
            }
            if (currentPlayer == null)
            {
                throw new CommandContextException(prefix + "when you are part of the game.");
            }
            if (onlyAlivePlayer != null && currentPlayer.State != onlyAlivePlayer)
            {
                throw new CommandContextException( prefix + $"when you are {Utils.AliveToS(onlyAlivePlayer.Value)}. (Now you are {Utils.AliveToS(currentPlayer.State)})");
            }
            if (onlyCard != null && currentPlayer.Card != onlyCard)
            {
                throw new CommandContextException( prefix + $"when you are a {Utils.CardToS(onlyCard.Value)}. (Now you are {Utils.CardToS(currentPlayer.Card)})");
            }
        }
    }
}