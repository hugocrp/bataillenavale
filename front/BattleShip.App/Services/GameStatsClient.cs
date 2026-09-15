using BattleShip.API.Grpc;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

namespace BattleShip.App.Services;

public sealed class GameStatsClient(string apiBaseUrl) : IGameStatsClient
{
    public async Task<GameStatsReply> GetStatsAsync(Guid gameId)
    {
        using var channel = GrpcChannel.ForAddress(apiBaseUrl, new GrpcChannelOptions
        {
            HttpHandler = new GrpcWebHandler(new HttpClientHandler())
        });

        var client = new GameStats.GameStatsClient(channel);
        return await client.GetStatsAsync(new GameStatsRequest { GameId = gameId.ToString() });
    }
}
