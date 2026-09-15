using BattleShip.API.Grpc;
using BattleShip.App.Services;

namespace BattleShip.App.Tests.Fakes;

public sealed class FakeGameStatsClient : IGameStatsClient
{
    public GameStatsReply? Reply { get; set; }
    public Exception? ExceptionToThrow { get; set; }
    public Guid? LastRequestedGameId { get; private set; }

    public Task<GameStatsReply> GetStatsAsync(Guid gameId)
    {
        LastRequestedGameId = gameId;
        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;

        return Task.FromResult(Reply ?? new GameStatsReply());
    }
}
