using System.Net.Http.Json;
using BattleShip.Models.Contracts;

namespace BattleShip.App.Services;

public sealed class GameApiClient(HttpClient http)
{
    public async Task<GameStateDto> CreateGameAsync()
    {
        var response = await http.PostAsync("games", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameStateDto>())!;
    }

    public async Task<GameStateDto> GetStateAsync(Guid gameId)
    {
        var response = await http.GetAsync($"games/{gameId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameStateDto>())!;
    }

    public async Task<TurnResultDto> ShootAsync(Guid gameId, int row, int col)
    {
        var response = await http.PostAsJsonAsync($"games/{gameId}/shots", new ShotRequest(row, col));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TurnResultDto>())!;
    }
}
