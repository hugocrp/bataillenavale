using BattleShip.API.Grpc;

namespace BattleShip.App.Services;

public interface IGameStatsClient
{
    Task<GameStatsReply> GetStatsAsync(Guid gameId);
}
