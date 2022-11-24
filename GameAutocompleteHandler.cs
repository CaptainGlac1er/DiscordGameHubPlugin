using Discord;
using Discord.Interactions;
namespace GlacierByte.Discord.Plugin;

public class GameAutocompleteHandler : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        try {
            Console.WriteLine("Suggestions");
            var service = services.GetService(typeof(GameHubInteractionService)) as GameHubInteractionService;
            if(service is null) {
                return AutocompletionResult.FromSuccess();
            }
            Console.WriteLine(autocompleteInteraction.ToString());
            List<AutocompleteResult> results = new List<AutocompleteResult>();
            foreach(var key in service.GetGameNames()) {
                results.Add(new AutocompleteResult(key, key));
            }

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        } catch(Exception e) {
            Console.WriteLine(e);
            return AutocompletionResult.FromError(e);
        }

    }
}