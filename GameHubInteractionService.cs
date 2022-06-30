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

        private IDictionary<string, Game> GamesAvailable;

        public GameHubInteractionService(DiscordSocketClient client)
        {
            GamesAvailable = new Dictionary<string, Game>();
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

        public List<string> GetGameNames() {
            return GamesAvailable.Keys.ToList();
        }

        public List<ulong> GetListOfPlayersThatPlayGame(string gameName) {
            if(!GamesAvailable.ContainsKey(gameName)) {
                return new List<ulong>();
            }
            return GamesAvailable[gameName].getListOfPlayersPlayingGame();
        }

        public async Task ModalHandler(SocketModal modal) {
            var components = modal.Data.Components.ToList();
            switch(modal.Data.CustomId) {
                case "new_game":
                    var newGameName = components.Single(x => x.CustomId == "game_name");
                    await modal.RespondAsync($"You added {newGameName.Value}");
                    var gameName = newGameName.Value;
                    if(!GamesAvailable.ContainsKey(gameName)) {
                        var newGame = new Game(gameName);
                        newGame.addPlayerThatWantsToPlay(Client.CurrentUser.Id);
                        GamesAvailable.Add(gameName, newGame);
                    }
                    break;
                default:
                    await modal.RespondAsync();
                    break;
            }
        }

    }
}