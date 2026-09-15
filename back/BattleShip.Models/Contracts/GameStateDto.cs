namespace BattleShip.Models.Contracts;

public sealed record GameStateDto(
    Guid GameId,
    string Phase,
    int BoardSize,
    IReadOnlyList<CellDto> PlayerBoard,
    IReadOnlyList<CellDto> ComputerBoard);
