using System.Net.Http.Json;
using BattleShip.Models.Contracts;

namespace BattleShip.App.Services;

public sealed class GameApiClient(HttpClient http)
{
    public async Task<IReadOnlyList<FleetSlotDto>> GetFleetAsync()
    {
        var response = await http.GetAsync("fleet");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<IReadOnlyList<FleetSlotDto>>())!;
    }

    public async Task<GameStateDto> CreateGameAsync(IReadOnlyList<ShipPlacementDto>? playerFleet, string difficulty, bool stormMode = false)
    {
        var response = await http.PostAsJsonAsync("games", new CreateGameRequest(playerFleet, difficulty, stormMode));
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<GameStateDto>())!;
    }

    public async Task<IReadOnlyList<GameSummaryDto>> ListGamesAsync()
    {
        var response = await http.GetAsync("games");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<IReadOnlyList<GameSummaryDto>>())!;
    }

    public async Task<GlobalStatsDto> GetGlobalStatsAsync()
    {
        var response = await http.GetAsync("stats");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<GlobalStatsDto>())!;
    }

    public async Task<GameStateDto> GetStateAsync(Guid gameId)
    {
        var response = await http.GetAsync($"games/{gameId}");
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<GameStateDto>())!;
    }

    public async Task<TurnResultDto> ShootAsync(Guid gameId, int row, int col)
    {
        var response = await http.PostAsJsonAsync($"games/{gameId}/shots", new ShotRequest(row, col));
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<TurnResultDto>())!;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(body) ? $"HTTP {(int)response.StatusCode}" : body);
    }
}
