using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Werefox.Interfaces;

namespace Werefox.Engine
{
    /// <summary>
    /// Werefox Service
    /// </summary>
    [SuppressMessage("ReSharper", "CA2007")]
    [SuppressMessage("ReSharper", "CA1303")]
    [SuppressMessage("ReSharper", "SA1028")]
    [SuppressMessage("ReSharper", "SA1101")]
    [SuppressMessage("ReSharper", "SA1202")]
    public class WerefoxService
    {
        internal IGame? CurrentGame { get; set; }
        
        private readonly string commandPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="WerefoxService"/> class.
        /// </summary>
        /// <param name="commandPrefix">commandPrefix.</param>
        public WerefoxService(string commandPrefix)
        {
            this.commandPrefix = commandPrefix;
        }
        
        /// <summary>
        /// Tells if game is started.
        /// </summary>
        /// <returns>true if game is started.</returns>
        public bool IsStarted()
        {
            return CurrentGame != null;
        }

        /// <summary>
        /// start a new game
        /// </summary>
        /// <param name="currentPlayerId">current player ID.</param>
        /// <param name="game">the new game.</param>
        /// <returns>nothing.</returns>
        public async Task Start(string currentPlayerId, IGame game)
        {
            CurrentGame = game;
            CheckPlayerStatus(currentPlayerId, null, null, null, "Stop");
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
            await CurrentGame.SendMessageAsync(":white_check_mark: Game started with players: " +
                                               Utils.DisplayPlayerList(CurrentGame.Players));
            ShufflePlayerCards();
            await TellCardToPlayers();
            await Night();
        }
        
        /// <summary>
        /// Shuffle cards for players.
        /// </summary>
        internal void ShufflePlayerCards()
        {
            var ranPlayer = CurrentGame.GetAlivePlayers().ToList().GetRandomItem();
            ranPlayer.Card = Card.Werefox;
        }

        private async Task TellCardToPlayers()
        {
            foreach (var player in CurrentGame.GetAlivePlayers())
            {
                await player.SendMessageAsync($"You are a {player.Card.ToDescription()}");
            }

            foreach (var werefox in CurrentGame.GetAliveWerefoxes())
            {
                await werefox.SendMessageAsync("The other werefoxes :fox: are: " +
                                               Utils.DisplayPlayerList(CurrentGame.GetAliveWerefoxes()));
            }
        }

        /// <summary>
        /// Stop the game.
        /// </summary>
        /// <param name="currentPlayerId">current player ID.</param>
        /// <returns>nothing.</returns>
        public async Task Stop(string currentPlayerId)
        {
            CheckPlayerStatus(currentPlayerId, null, null, null, "Stop");
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
            await CurrentGame.SendMessageAsync(
                $"Now you have to vote for who you will sacrifice :dagger:, use the command {commandPrefix}sacrifice NICKNAME ");
        }

        private async Task Night()
        {
            CurrentGame.Step = GameStep.Night;
            await CurrentGame.SendMessageAsync(
                "The night is falling :crescent_moon:. The village is sleeping :sleeping: . The werefoxes :fox: go out!");
            await CurrentGame.SendMessageAsync(
                "Werefoxes! It's time to decide who you will eat :yum:. Go to the direct message with WereFoxBot.");
            foreach (var werefox in CurrentGame.Players.Where(p => p.Card == Card.Werefox))
            {
                await werefox.SendMessageAsync(
                    $"Werefoxes! :fox: It's time to decide who you will eat :yum:. Send {commandPrefix}eat NICKNAME in direct message to WereFoxBot.");
            }
        }

        /// <summary>
        /// Sacrifice a player
        /// </summary>
        /// <param name="currentPlayerId">current player ID.</param>
        /// <param name="playerToSacrifice">player you vote to sacrifice.</param>
        /// <returns>nothing.</returns>
        public async Task Sacrifice(string currentPlayerId, string playerToSacrifice)
        {
            CheckPlayerStatus(currentPlayerId, GameStep.Day, PlayerState.Alive, null, "Sacrifice");
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            currentPlayer.Vote = await CheckNickname(CurrentGame, playerToSacrifice, false);

            var electedPlayer = GetVotes(CurrentGame.GetAlivePlayers().ToList());
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

        /// <summary>
        /// Eat a player
        /// </summary>
        /// <param name="currentPlayerId">current player ID.</param>
        /// <param name="playerToEat">player you vote to eat.</param>
        /// <returns>nothing.</returns>
        public async Task Eat(string currentPlayerId, string playerToEat)
        {
            CheckPlayerStatus(currentPlayerId, GameStep.Night, PlayerState.Alive, Card.Werefox, "Eat");
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            currentPlayer.Vote = await CheckNickname(currentPlayer, playerToEat, true);
            var electedPlayer = GetVotes(CurrentGame.GetAliveWerefoxes().ToList());
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

        private IPlayer? GetVotes(IList<IPlayer> players)
        {
            if (players.Any(p => p.Vote == null))
            {
                return null;
            }

            var votes = players.GroupBy(p => p.Vote)
                .OrderByDescending(p => p.Count())
                .ToList();
            var voteList = votes.Select(
                v => "- " + v.Key?.GetMention() + " " + v.Count() + " votes");
            CurrentGame.SendMessageAsync("Result of the vote: \r\n"
                                         + string.Join("\r\n", voteList));
            var playerEaten = votes.First().Key;
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
            var playerEaten = CurrentGame.GetByName(playerToEat);
            if (playerEaten == null)
            {
                await sendMessage.SendMessageAsync($":name_badge: no player with the nickname: {playerToEat}.");
                return null;
            }

            if (playerEaten.State == PlayerState.Dead)
            {
                await sendMessage.SendMessageAsync(
                    $":name_badge: {playerEaten.GetMention()} is {PlayerState.Dead.ToDescription()}. Choose somebody else.");
                return null;
            }

            if (restrictOnWerefox && playerEaten.Card == Card.Werefox)
            {
                await sendMessage.SendMessageAsync(
                    $":name_badge: {playerEaten.GetMention()} is a {playerEaten.Card.ToDescription()}. Choose somebody else.");
                return null;
            }

            await sendMessage.SendMessageAsync($":envelope_with_arrow: You vote for: ({playerEaten.GetMention()}) !");
            return playerEaten;
        }

        private async Task Die(IPlayer player, string message)
        {
            player.State = PlayerState.Dead;
            await CurrentGame.SendMessageAsync(
                $"{player.GetMention()} {message} He was a {player.Card.ToDescription()}.");
            await CurrentGame.SendMessageAsync("Remaining players: " +
                                               Utils.DisplayPlayerList(CurrentGame.GetAlivePlayers()));
        }

        /// <summary>
        /// Leave the game.
        /// </summary>
        /// <param name="currentPlayerId">current player ID.</param>
        /// <returns>nothing.</returns>
        public async Task Leave(string currentPlayerId)
        {
            CheckPlayerStatus(currentPlayerId, null, PlayerState.Alive, null, "Leave");
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            await Die(currentPlayer, "has left the game :door:.");
            await CheckWin();
        }

        /// <summary>
        /// Reveal you card to everybody.
        /// </summary>
        /// <param name="currentPlayerId">current player ID.</param>
        /// <returns>nothing.</returns>
        public async Task Reveal(string currentPlayerId)
        {
            CheckPlayerStatus(currentPlayerId, null, PlayerState.Alive, null, "Reveal");
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            await currentPlayer.SendMessageAsync(
                $"REVELATION: {currentPlayer.GetMention()} is a {currentPlayer.Card.ToDescription()}.");
        }

        /// <summary>
        /// Show you who is who, the card to everybody.
        /// </summary>
        /// <param name="currentPlayerId">current player ID.</param>
        /// <returns>nothing.</returns>
        public async Task WhoIsWho(string currentPlayerId)
        {
            CheckPlayerStatus(currentPlayerId, null, PlayerState.Dead, null, "WhoIsWho");
            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            var statuses = CurrentGame.Players.Select(
                p => $"- {p.GetMention()} is {p.State.ToDescription()} and is a  {p.Card.ToDescription()}.");
            await currentPlayer.SendMessageAsync("Result of the vote: \r\n" + string.Join("\r\n", statuses));
        }

        /// <summary>
        /// Show the status of the game and who is alive. 
        /// </summary>
        /// <param name="currentPlayerId">current player ID.</param>
        /// <returns>nothing.</returns>
        public async Task Status(string currentPlayerId)
        {
            CheckPlayerStatus(currentPlayerId, null, null, null, "Status");
            await CurrentGame.SendMessageAsync($"It's now the {CurrentGame.Step.ToDescription()}.");
            await CurrentGame.SendMessageAsync(PlayerState.Alive.ToDescription() + " players are: " +
                                               Utils.DisplayPlayerList(CurrentGame.GetAlivePlayers()));
            await CurrentGame.SendMessageAsync(PlayerState.Dead.ToDescription() + " players are: " +
                                               Utils.DisplayPlayerList(CurrentGame.GetDeadPlayers()));
        }

        private IPlayer GetCurrentPlayer(string playerId)
        {
            var currentPlayer = CurrentGame.GetById(playerId);
            if (currentPlayer == null)
            {
                throw new InvalidOperationException("No current player can be found " + playerId);
            }

            return currentPlayer;
        }

        /// <summary>
        /// CheckPlayerStatus for commands.
        /// </summary>
        /// <param name="currentPlayerId">current player ID.</param>
        /// <param name="step">check the game step, null no check.</param>
        /// <param name="onlyAlivePlayer">true: only alive player, false: only dead player, null no check.</param>
        /// <param name="onlyCard">check if the player has this card, null no check.</param>
        /// <param name="commandName">command name.</param>
        /// <exception cref="CommandContextException">throws CommandContextException if check fails.</exception>
        internal void CheckPlayerStatus(string currentPlayerId, GameStep? step, PlayerState? onlyAlivePlayer,
            Card? onlyCard, string commandName)
        {
            var prefix = $":no_entry: The command {commandPrefix}{commandName} must be use ";
            if (CurrentGame == null)
            {
                throw new CommandContextException(prefix + "during the game. (No game started)");
            }

            if (step != null && CurrentGame.Step != step)
            {
                throw new CommandContextException(
                    prefix +
                    $"during the {step.Value.ToDescription()}. (It's now the {CurrentGame.Step.ToDescription()})");
            }

            var currentPlayer = GetCurrentPlayer(currentPlayerId);
            if (currentPlayer == null)
            {
                throw new CommandContextException(prefix + "when you are part of the game.");
            }

            if (onlyAlivePlayer != null && currentPlayer.State != onlyAlivePlayer)
            {
                throw new CommandContextException(
                    prefix +
                    $"when you are {onlyAlivePlayer.Value.ToDescription()}. (Now you are {currentPlayer.State.ToDescription()})");
            }

            if (onlyCard != null && currentPlayer.Card != onlyCard)
            {
                throw new CommandContextException(
                    prefix +
                    $"when you are a {onlyCard.Value.ToDescription()}. (Now you are {currentPlayer.Card.ToDescription()})");
            }
        }
    }
}