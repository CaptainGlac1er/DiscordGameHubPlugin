using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace GlacierByte.Discord.Plugin;
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
    [SlashCommand("available_games", "What Games do people play?")]
    public async Task GetGames() {
        
        var games = Service.GetGameNames();
        var reply = new StringBuilder("Available Games to Play\r\n");
        foreach (var gameName in games)
        {
            reply.AppendLine($"{gameName}");
        }

        await RespondAsync(reply.ToString());
    }
    [SlashCommand("who_plays_this_game", "Get List of players that play a game")]
    public async Task GetPlayersThatPlayGame(
        [
            Summary("game_name", "Game Name"),
            Autocomplete(typeof(GameAutocompleteHandler))
        ] string gameName
        ) {
            var embedBuilder = this._getPlayersThatPlayGame(gameName);
            var replyCommandBuilder = this._getWhoPlaysGameButtons(gameName);
            await RespondAsync(embed: embedBuilder.Build(), components: replyCommandBuilder.Build(), options: new RequestOptions(), ephemeral: true);
    }

    [ComponentInteraction("addUserToGame:game(*)", ignoreGroupNames: true)]
    public async Task AddUserToGame(string gameName)
    {
        Service.AddUserToGame(gameName, Context.User);
        await Context.Interaction.ModifyOriginalResponseAsync(newComponent =>
        {
            var embedBuilder = this._getPlayersThatPlayGame(gameName);
            newComponent.Embed = embedBuilder.Build();
            newComponent.Components = this._getWhoPlaysGameButtons(gameName).Build();
        });
    }

    [ComponentInteraction("removeUserFromGame:game(*)", ignoreGroupNames: true)]
    public async Task RemoveUserFromGame(string gameName)
    {
        Service.RemoveUserToGame(gameName, Context.User);
        await Context.Interaction.ModifyOriginalResponseAsync(newComponent =>
        {
            var embedBuilder = this._getPlayersThatPlayGame(gameName);
            newComponent.Embed = embedBuilder.Build();
            newComponent.Components = this._getWhoPlaysGameButtons(gameName).Build();
        });
    }
    
    
    [ComponentInteraction("pingForGame(game:*)", ignoreGroupNames: true)]
    public async Task PingForGame(string gameName)
    {
        var playerEmbed = _getPlayersThatPlayGame(gameName);
        await Context.Interaction.DeleteOriginalResponseAsync();
        await Context.Channel.SendMessageAsync(text: $"{Context.User.Mention} wants to play {gameName}", embed: playerEmbed.Build(),
                                                           allowedMentions: AllowedMentions.All);
    }

    private EmbedBuilder _getPlayersThatPlayGame(string gameName)
    {
        var players = Service.GetListOfPlayersThatPlayGame(gameName);
        var playerString = new StringBuilder($"Players playing {gameName}\r\n");

        foreach (var player in players)
        {
            var user = Context.Guild.GetUser(player);
            playerString.AppendLine($"{user.Mention},");
        }

        var embedBuilder = new EmbedBuilder();
        embedBuilder.WithTitle(gameName);
        embedBuilder.WithDescription(playerString.ToString());
        return embedBuilder;
    }

    private ComponentBuilder _getWhoPlaysGameButtons(string gameName)
    {
        var replyCommandBuilder = new ComponentBuilder();
        ActionRowBuilder rowBuilder = new ActionRowBuilder();
        replyCommandBuilder.AddRow(rowBuilder);
        rowBuilder.AddComponent(Service.IsUserInGame(gameName, Context.User)
            ? _getRemoveMeButton(gameName).Build()
            : _getAddMeButton(gameName).Build()
        );
        rowBuilder.AddComponent(new ButtonBuilder().WithLabel("Ping").WithStyle(ButtonStyle.Primary)
            .WithCustomId($"pingForGame(game:{gameName})").Build());
        return replyCommandBuilder;
    }

    private ButtonBuilder _getAddMeButton(string gameName)
    {
        return new ButtonBuilder().WithStyle(ButtonStyle.Primary).WithLabel("Add Me In").WithCustomId($"addUserToGame:game({gameName})");
    }
    
    private ButtonBuilder _getRemoveMeButton(string gameName)
    {
        return new ButtonBuilder().WithStyle(ButtonStyle.Danger).WithLabel("Remove Me").WithCustomId($"removeUserFromGame:game({gameName})");
    }
    
    [SlashCommand("save_state", "Save state of Game Service")]
    public async Task SaveState() {
        try {
            await Service.SaveState();
            await RespondAsync("State saved", ephemeral: true);
        } catch (Exception e) {
            await RespondAsync(e.Message, ephemeral: true);
        }
    }

    [SlashCommand("load_state", "Save state of Game Service")]
    public async Task LoadState() {
        await Service.LoadState();
        await RespondAsync("State loaded", ephemeral: true);
    }
}

