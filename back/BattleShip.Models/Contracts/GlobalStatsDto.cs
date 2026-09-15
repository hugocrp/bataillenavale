namespace BattleShip.Models.Contracts;

public sealed record GlobalStatsDto(
    int TotalGames,
    int PlayerWins,
    int ComputerWins,
    int InProgressCount,
    double? AverageShotsToFinish);
