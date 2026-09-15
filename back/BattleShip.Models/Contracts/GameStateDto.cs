namespace BattleShip.Models.Contracts;

public sealed record GameStateDto(
    Guid GameId,
    string Phase,
    IReadOnlyList<CellDto> PlayerBoard,
    IReadOnlyList<CellDto> ComputerBoard);
