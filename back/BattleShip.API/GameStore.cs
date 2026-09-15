using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using BattleShip.Models;

namespace BattleShip.API;

public sealed class GameStore
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public Game Create()
    {
        var game = new Game();
        _games[game.Id] = game;
        return game;
    }

    public bool TryGet(Guid id, [NotNullWhen(true)] out Game? game) => _games.TryGetValue(id, out game);
}
