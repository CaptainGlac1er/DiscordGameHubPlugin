using Discord.Commands;
using Discord.Interactions;

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
    }

}
