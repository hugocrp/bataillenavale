namespace BattleShip.Models.Contracts;

public sealed record GameSummaryDto(Guid GameId, string Phase, string Difficulty, bool StormMode, DateTimeOffset LastActivityUtc);
