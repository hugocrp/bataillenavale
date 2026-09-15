using BattleShip.Models;
using BattleShip.Models.Contracts;

namespace BattleShip.API;

public static class GameStateMapper
{
    public static GameStateDto ToDto(Game game) =>
        new(
            game.Id,
            game.Phase.ToString(),
            ToCells(game.PlayerBoard, revealShips: true),
            ToCells(game.ComputerBoard, revealShips: false));

    public static ShotResultDto ToDto(ShotResult result) =>
        new(result.Target.Row, result.Target.Col, result.Outcome.ToString());

    public static TurnResultDto ToDto(TurnResult result, Game game) =>
        new(
            ToDto(result.PlayerShot),
            result.ComputerShot is null ? null : ToDto(result.ComputerShot),
            ToDto(game));

    private static IReadOnlyList<CellDto> ToCells(Board board, bool revealShips)
    {
        var cells = new List<CellDto>(Board.Size * Board.Size);
        for (var row = 0; row < Board.Size; row++)
        {
            for (var col = 0; col < Board.Size; col++)
            {
                var coordinate = new Coordinate(row, col);
                cells.Add(new CellDto(row, col, board.GetState(coordinate, revealShips).ToString()));
            }
        }

        return cells;
    }
}
