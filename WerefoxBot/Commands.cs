// THIS FILE IS A PART OF EMZI0767'S BOT EXAMPLES
//
// --------
// 
// Copyright 2017 Emzi0767
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// --------
//
// This is an interactivity example. It shows how to properly utilize 
// Interactivity module.

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using WerefoxBot.Game;

namespace WerefoxBot
{
    // note that in here we explicitly ask for duration. This is optional,
    // since we set the defaults.
    public class Commands : BaseCommandModule
    {
        private Game.Game? CurrentGame { get; set; }

        [Command("eat"), Description("Eat a player")]
        public async Task Eat(CommandContext context, [Description("Player you want to eat")]
            string playerToEat)
        {
            await context.RespondAsync("eat "+playerToEat);
            if (CurrentGame == null)
            {
                 context.RespondAsync("No game is started");
                 return;
            }

            Player? playerEaten = await CheckNickname(context, playerToEat);

        }

        private async Task<Player?> CheckNickname(CommandContext context, string playerToEat)
        {
            Player? playerEaten = CurrentGame.Players.FirstOrDefault(p => p.User.DisplayName == playerToEat);
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
            if (playerEaten.IsWereFox)
            {
                await context.RespondAsync($"{playerEaten.User.Mention}, is a werefox. Choose somebody else.");
                return null;
            }

            await context.RespondAsync($"Your vote to eat: ({playerEaten.User.Mention})");
            return playerEaten;
        }

        [Command("start"), Description("Start a new game.")]
        public async Task Start(CommandContext context, [Description("How long should the poll last.")] TimeSpan duration)
        {
            if (context == null)
            {
                throw new ArgumentException(nameof(context));
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

            CurrentGame = new Game.Game();
            // wait for anyone who types it
            var reply = await interactivity.CollectReactionsAsync(msg, duration);

            if (reply.Any())
            {
                CurrentGame.Players.AddRange(reply
                    .Where(r => r.Emoji == emoji)
                    .SelectMany(r => r.Users)
                    .Where(u => !u.IsBot)
                    .Select(u => new Player( context.Guild.Members[u.Id])));
                await context.RespondAsync("Players: " + string.Join(", ", CurrentGame.Players.Select(p => p.User.Mention)));
                StartGame(context);
            }
            else
            {
                await context.RespondAsync("Nobody? Really?");
            }
        }

        [Command("stop"), Description("stop a new game.")]
        public async Task Stop(CommandContext context)
        {
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
            TellWereFox(context);
            Night(context);
        }
        
        private async void TellWereFox(CommandContext context)
        {
            /*
            CurrentGame.WerefoxesChannel = await context.Guild.
                    CreateChannelAsync("WerefoxesOnly", ChannelType.Text, null, "", null, null, null, false, null, null);
            var invite = await CurrentGame.WerefoxesChannel.CreateInviteAsync();
            */
            foreach (var player in CurrentGame.Players)
            {
                player.dmChannel = await player.User.CreateDmChannelAsync();
                await player.dmChannel.SendMessageAsync("You are a " + player.WerefoxToString());
            }

            var werefoxes = CurrentGame.Players.Where(p => p.IsWereFox);
            foreach (var werefox in werefoxes)
            {
                await werefox.dmChannel.SendMessageAsync("The other werefoxes are: "
                                                         + String.Join(", ", werefoxes.Select(w => w.User.Mention)));
            }
            /*
            CurrentGame.WerefoxesChannel.SendMessageAsync("Hello werefoxes, are you hungry?");
            */
        }
        
        private async void Night(CommandContext context)
        {
            await context.RespondAsync("The night is falling.\n"
                                       +"The village is sleeping.");
            await context.RespondAsync("The werefoxes go out!");
            await context.RespondAsync("Werefoxes! It's time to decide who you will eat. Send ;;eat NICKNAME in direct message to WereFoxBot.");
            foreach (var werefox in CurrentGame.Players.Where(p => p.IsWereFox))
            {
                await  werefox.dmChannel.SendMessageAsync("Werefoxes! It's time to decide who you will eat. Send ;;eat NICKNAME in direct message to WereFoxBot.");
            }
        }
    }
}
