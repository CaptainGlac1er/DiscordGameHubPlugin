using Discord;
using Discord.WebSocket;
using GlacierByte.Discord.Server.Api;

namespace GlacierByte.Discord.Plugin;
public class GameHubInteractionService : ICustomService {
    private readonly DiscordSocketClient _client;
    private readonly LongTermFileService _fileService;

    private IDictionary<string, Game> _gamesAvailable;

    public GameHubInteractionService(
        DiscordSocketClient client,
        LongTermFileService fileStorage
        )
    {
        _gamesAvailable = new Dictionary<string, Game>();
        _client = client;
        client.ModalSubmitted += ModalHandler;
        _fileService = fileStorage;
    }

    public async Task AddGameAsync(IInteractionContext context) {
        var newGameModal = new ModalBuilder()
            .WithTitle("Add Game")
            .WithCustomId("new_game")
            .AddTextInput("New Game", "game_name");

        await context.Interaction.RespondWithModalAsync(newGameModal.Build());
    }

    public List<string> GetGameNames() {
        return _gamesAvailable.Keys.ToList();
    }

    public List<ulong> GetListOfPlayersThatPlayGame(string gameName) {
        if(!_gamesAvailable.ContainsKey(gameName)) {
            return new List<ulong>();
        }
        return _gamesAvailable[gameName].getListOfPlayersPlayingGame();
    }

    public async Task ModalHandler(SocketModal modal) {
        var components = modal.Data.Components.ToList();
        switch(modal.Data.CustomId) {
            case "new_game":
                var newGameName = components.Single(x => x.CustomId == "game_name");
                await modal.RespondAsync($"You added {newGameName.Value}");
                var gameName = newGameName.Value;
                if(!_gamesAvailable.ContainsKey(gameName)) {
                    var newGame = new Game(gameName);
                    newGame.addPlayerThatWantsToPlay(_client.CurrentUser.Id);
                    _gamesAvailable.Add(gameName, newGame);
                }
                break;
            default:
                await modal.RespondAsync();
                break;
        }
    }

    public async Task SaveState() {
        await _fileService.setFileData("state.GameHub.json", _gamesAvailable);
    }

    public async Task LoadState() {
        _gamesAvailable = await _fileService.getFileData<IDictionary<string, Game>>("state.GameHub.json");
    }

}