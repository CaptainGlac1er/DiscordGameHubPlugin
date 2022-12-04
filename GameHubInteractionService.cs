using Discord;
using Discord.WebSocket;
using GlacierByte.Discord.Server.Api;

namespace GlacierByte.Discord.Plugin;
public class GameHubInteractionService : ICustomService {
    private readonly LongTermFileService _fileService;
    private readonly Dictionary<String, Func<SocketMessageComponent, Task>> _tempButtonResponse = new();

    private IDictionary<string, Game> _gamesAvailable;

    public GameHubInteractionService(
        DiscordSocketClient client,
        LongTermFileService fileStorage
        )
    {
        _gamesAvailable = new Dictionary<string, Game>();
        client.ModalSubmitted += ModalHandler;
        client.ButtonExecuted += ButtonHandler;
        _fileService = fileStorage;
    }

    public async Task AddGameAsync(IInteractionContext context) {
        var newGameModal = new ModalBuilder()
            .WithTitle("Add Game")
            .WithCustomId("new_game")
            .AddTextInput("New Game", "game_name");

        await context.Interaction.RespondWithModalAsync(newGameModal.Build());
    }

    public void AddUserToGame(string game, SocketUser user)
    {
        this._gamesAvailable[game].addPlayerThatWantsToPlay(user.Id);
    }
    
    public void RemoveUserToGame(string game, SocketUser user)
    {
        this._gamesAvailable[game].removePlayerThatWantsToPlay(user.Id);
    }

    public List<string> GetGameNames() {
        return _gamesAvailable.Keys.ToList();
    }

    public Boolean IsUserInGame(string gameName, SocketUser user)
    {
        return _gamesAvailable.ContainsKey(gameName) &&
               _gamesAvailable[gameName].PlayersThatWantToPlay.ContainsKey(user.Id);
    }

    public List<ulong> GetListOfPlayersThatPlayGame(string gameName) {
        if(!_gamesAvailable.ContainsKey(gameName)) {
            return new List<ulong>();
        }
        return _gamesAvailable[gameName].getListOfPlayersPlayingGame();
    }
    
    private async Task ButtonHandler(SocketMessageComponent component)
    {
        if (this._tempButtonResponse.ContainsKey(component.Data.CustomId))
        {
            await this._tempButtonResponse[component.Data.CustomId].Invoke(component);
            return;
        }
        
        switch (component.Data.CustomId)
        {
            case "ADD_ME_IN":
                await component.RespondAsync($"{component.User.Mention} has clicked the button", ephemeral: true);
                break;
            default:
                await component.DeferAsync(ephemeral: true);
                break;
        }
    }

    public void addTempCommand(string id, Func<SocketMessageComponent, Task> action)
    {
        this._tempButtonResponse.Add(id, action);
    }
    
    public void removeTempCommand(string id)
    {
        this._tempButtonResponse.Remove(id);
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
                    newGame.addPlayerThatWantsToPlay(modal.User.Id);
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