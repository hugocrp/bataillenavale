using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using BattleShip.Models;

namespace BattleShip.API;

public sealed class GameStore(TimeProvider timeProvider)
{
    private readonly ConcurrentDictionary<Guid, GameEntry> _games = new();

    public int Count => _games.Count;

    public Game CreateRandom(ComputerDifficulty difficulty = ComputerDifficulty.Hard)
    {
        var game = new Game(ComputerStrategyFactory.Create(difficulty, Random.Shared));
        Track(game, difficulty, stormMode: false);
        return game;
    }

    public Game CreateWithPlayerFleet(Board playerBoard, ComputerDifficulty difficulty = ComputerDifficulty.Hard)
    {
        var game = new Game(playerBoard, ComputerStrategyFactory.Create(difficulty, Random.Shared));
        Track(game, difficulty, stormMode: false);
        return game;
    }

    public Game CreateStorm(ComputerDifficulty difficulty = ComputerDifficulty.Hard)
    {
        var random = Random.Shared;
        var playerBoard = FleetFactory.CreateStormBoard(random);
        var computerBoard = FleetFactory.CreateStormBoard(random);
        var game = new Game(playerBoard, computerBoard, ComputerStrategyFactory.Create(difficulty, random));
        Track(game, difficulty, stormMode: true);
        return game;
    }

    private void Track(Game game, ComputerDifficulty difficulty, bool stormMode) =>
        _games[game.Id] = new GameEntry(game, timeProvider.GetUtcNow(), difficulty, stormMode);

    public bool TryGet(Guid id, [NotNullWhen(true)] out Game? game)
    {
        if (_games.TryGetValue(id, out var entry))
        {
            entry.LastActivityUtc = timeProvider.GetUtcNow();
            game = entry.Game;
            return true;
        }

        game = null;
        return false;
    }

    public IReadOnlyList<GameSummary> ListAll() =>
        _games
            .Select(kv => new GameSummary(kv.Key, kv.Value.Game.Phase, kv.Value.Difficulty, kv.Value.StormMode, kv.Value.LastActivityUtc))
            .OrderByDescending(s => s.LastActivityUtc)
            .ToList();

    public GlobalStats GetGlobalStats()
    {
        var games = _games.Values.Select(e => e.Game).ToList();
        var finished = games.Where(g => g.Phase != GamePhase.InProgress).ToList();
        var playerWins = finished.Count(g => g.Phase == GamePhase.PlayerWon);
        var computerWins = finished.Count(g => g.Phase == GamePhase.ComputerWon);
        double? averageShots = finished.Count > 0
            ? finished.Average(g => g.ComputerBoard.ShotsFired + g.PlayerBoard.ShotsFired)
            : null;

        return new GlobalStats(games.Count, playerWins, computerWins, games.Count - finished.Count, averageShots);
    }

    public int RemoveExpired(TimeSpan maxIdleTime)
    {
        var cutoff = timeProvider.GetUtcNow() - maxIdleTime;
        var expiredIds = _games
            .Where(kv => kv.Value.LastActivityUtc < cutoff)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var id in expiredIds)
            _games.TryRemove(id, out _);

        return expiredIds.Count;
    }

    public sealed record GameSummary(Guid Id, GamePhase Phase, ComputerDifficulty Difficulty, bool StormMode, DateTimeOffset LastActivityUtc);

    public sealed record GlobalStats(int TotalGames, int PlayerWins, int ComputerWins, int InProgressCount, double? AverageShotsToFinish);

    private sealed class GameEntry(Game game, DateTimeOffset lastActivityUtc, ComputerDifficulty difficulty, bool stormMode)
    {
        public Game Game { get; } = game;
        public DateTimeOffset LastActivityUtc { get; set; } = lastActivityUtc;
        public ComputerDifficulty Difficulty { get; } = difficulty;
        public bool StormMode { get; } = stormMode;
    }
}
