namespace GlacierByte.Discord.Plugin;

class Game {
    public string GameName;
    public IDictionary<ulong, bool> PlayersThatWantToPlay;
    public Game(string gameName) {
        GameName = gameName;
        PlayersThatWantToPlay = new Dictionary<ulong, bool>();
    }

    public void addPlayerThatWantsToPlay(ulong playerId) {
        PlayersThatWantToPlay.Add(playerId, true);
    }
    public void removePlayerThatWantsToPlay(ulong playerId) {
        PlayersThatWantToPlay.Remove(playerId);
    }

    public List<ulong> getListOfPlayersPlayingGame() {
        return PlayersThatWantToPlay.Keys.ToList();
    }
}