using System.Net;
using System.Net.Http.Json;
using BattleShip.Models.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BattleShip.Tests.Api;

public class GamesEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_Games_Cree_Une_Partie_En_Cours_Avec_Les_Deux_Grilles()
    {
        var response = await _client.PostAsync("/games", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var state = await response.Content.ReadFromJsonAsync<GameStateDto>();
        Assert.NotNull(state);
        Assert.NotEqual(Guid.Empty, state!.GameId);
        Assert.Equal("InProgress", state.Phase);
        Assert.Equal(100, state.PlayerBoard.Count);
        Assert.Equal(100, state.ComputerBoard.Count);
    }

    [Fact]
    public async Task Get_Games_Id_Inconnu_Retourne_404()
    {
        var response = await _client.GetAsync($"/games/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Shots_Avec_Coordonnees_Valides_Retourne_Le_Resultat_Du_Tour()
    {
        var created = await (await _client.PostAsync("/games", null)).Content.ReadFromJsonAsync<GameStateDto>();

        var response = await _client.PostAsJsonAsync($"/games/{created!.GameId}/shots", new ShotRequest(0, 0));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var turn = await response.Content.ReadFromJsonAsync<TurnResultDto>();
        Assert.NotNull(turn);
        Assert.Equal(0, turn!.PlayerShot.Row);
        Assert.Equal(0, turn.PlayerShot.Col);
    }

    [Fact]
    public async Task Post_Shots_Avec_Coordonnees_Hors_Grille_Retourne_400()
    {
        var created = await (await _client.PostAsync("/games", null)).Content.ReadFromJsonAsync<GameStateDto>();

        var response = await _client.PostAsJsonAsync($"/games/{created!.GameId}/shots", new ShotRequest(42, 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Shots_Sur_Partie_Inconnue_Retourne_404()
    {
        var response = await _client.PostAsJsonAsync($"/games/{Guid.NewGuid()}/shots", new ShotRequest(0, 0));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
