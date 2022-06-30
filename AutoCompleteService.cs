using Discord;
using Discord.Interactions;
namespace GlacierByte.Discord.Plugin;

public class GameAutocompleteHandler : AutocompleteHandler
{
    private readonly GameHubInteractionService Service;
    public GameAutocompleteHandler(GameHubInteractionService service) {
        this.Service = service;
    }
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        List<AutocompleteResult> results = new List<AutocompleteResult>();
        foreach(var key in this.Service.GetGameNames()) {
            results.Add(new AutocompleteResult(key, key));
        }

        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}