namespace BattleShip.Models.Contracts;

public sealed record CreateGameRequest(
    IReadOnlyList<ShipPlacementDto>? PlayerFleet,
    string? Difficulty = null,
    bool StormMode = false);
