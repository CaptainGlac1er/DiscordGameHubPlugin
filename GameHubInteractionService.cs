using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GlacierByte.Discord.Server.Api;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlacierByte.Discord.Plugin
{
    public class GameHubInteractionService : ICustomService {
        private readonly DiscordSocketClient Client;
        public GameHubInteractionService(DiscordSocketClient client)
        {
            Client = client;
            client.ModalSubmitted += ModalHandler;
        }

        public async Task AddGameAsync(IInteractionContext context) {
            var newGameModal = new ModalBuilder()
                .WithTitle("Add Game")
                .WithCustomId("new_game")
                .AddTextInput("New Game", "game_name");

            await context.Interaction.RespondWithModalAsync(newGameModal.Build());
        }

        public async Task ModalHandler(SocketModal modal) {
            var components = modal.Data.Components.ToList();
            switch(modal.Data.CustomId) {
                case "new_game":
                    var newGame = components.Single(x => x.CustomId == "game_name");
                    await modal.RespondAsync($"You added {newGame.Value}");
                    break;
                default:
                    await modal.RespondAsync();
                    break;
            }
        }

    }
}