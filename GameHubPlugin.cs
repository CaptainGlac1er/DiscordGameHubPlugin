﻿using Discord.Interactions;

namespace GlacierByte.Discord.Plugin {
    // [Group("gamehub")]
    public class GameHubPlugin : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly GameHubInteractionService Service;
        public GameHubPlugin(GameHubInteractionService service) {
            this.Service = service;
        }
        [SlashCommand("new_game", "Add a new game to play")]
        public async Task AddGame() {
            await Service.AddGameAsync(Context);
        }
        [SlashCommand("who_plays_this_game", "Get List of players that play a game")]
        public async Task GetPlayersThatPlayGame([Summary("game_name"), Autocomplete(typeof(GameAutocompleteHandler))] string gameName) {
            var players = Service.GetListOfPlayersThatPlayGame(gameName);
            var playerString = $"Players playing {gameName}\r\n[";

            foreach (var player in players) {
                playerString += $"{player},\r\n";
            }
            playerString += "]";
            await RespondAsync(playerString);
        }
    }

}
