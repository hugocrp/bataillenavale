using System.Net;
using System.Net.Http.Json;
using BattleShip.Models.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BattleShip.Tests.Api;

public class GamesEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static async Task<GameStateDto> CreateRandomGameAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/games", new CreateGameRequest(null));
        return (await response.Content.ReadFromJsonAsync<GameStateDto>())!;
    }

    [Fact]
    public async Task Get_Fleet_Retourne_La_Composition_Standard()
    {
        var response = await _client.GetAsync("/fleet");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fleet = await response.Content.ReadFromJsonAsync<FleetSlotDto[]>();
        Assert.NotNull(fleet);
        Assert.Equal(5, fleet!.Length);
        Assert.Contains(fleet, s => s is { Name: "Porte-avions", Size: 5 });
    }

    [Fact]
    public async Task Post_Games_Sans_Flotte_Cree_Une_Partie_En_Cours_Avec_Les_Deux_Grilles()
    {
        var response = await _client.PostAsJsonAsync("/games", new CreateGameRequest(null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var state = await response.Content.ReadFromJsonAsync<GameStateDto>();
        Assert.NotNull(state);
        Assert.NotEqual(Guid.Empty, state!.GameId);
        Assert.Equal("InProgress", state.Phase);
        Assert.Equal(100, state.PlayerBoard.Count);
        Assert.Equal(100, state.ComputerBoard.Count);
    }

    [Fact]
    public async Task Post_Games_Avec_Flotte_Manuelle_Valide_Place_Les_Navires_Aux_Cases_Demandees()
    {
        var request = new CreateGameRequest(
        [
            new ShipPlacementDto("Porte-avions", 0, 0, "Horizontal"),
            new ShipPlacementDto("Croiseur", 2, 0, "Horizontal"),
            new ShipPlacementDto("Contre-torpilleur", 4, 0, "Horizontal"),
            new ShipPlacementDto("Sous-marin", 6, 0, "Horizontal"),
            new ShipPlacementDto("Torpilleur", 8, 0, "Horizontal")
        ]);

        var response = await _client.PostAsJsonAsync("/games", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var state = await response.Content.ReadFromJsonAsync<GameStateDto>();
        Assert.NotNull(state);
        Assert.Equal("Ship", state!.PlayerBoard.Single(c => c is { Row: 0, Col: 0 }).State);
        Assert.Equal("Ship", state.PlayerBoard.Single(c => c is { Row: 8, Col: 1 }).State);
    }

    [Fact]
    public async Task Post_Games_Avec_Nombre_De_Navires_Incorrect_Retourne_400()
    {
        var request = new CreateGameRequest([new ShipPlacementDto("Porte-avions", 0, 0, "Horizontal")]);

        var response = await _client.PostAsJsonAsync("/games", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Games_Avec_Navires_Qui_Se_Chevauchent_Retourne_400()
    {
        var request = new CreateGameRequest(
        [
            new ShipPlacementDto("Porte-avions", 0, 0, "Horizontal"),
            new ShipPlacementDto("Croiseur", 0, 0, "Horizontal"),
            new ShipPlacementDto("Contre-torpilleur", 4, 0, "Horizontal"),
            new ShipPlacementDto("Sous-marin", 6, 0, "Horizontal"),
            new ShipPlacementDto("Torpilleur", 8, 0, "Horizontal")
        ]);

        var response = await _client.PostAsJsonAsync("/games", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Games_Avec_Difficulte_Easy_Cree_Une_Partie()
    {
        var response = await _client.PostAsJsonAsync("/games", new CreateGameRequest(null, "Easy"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_Games_Avec_Difficulte_Invalide_Retourne_400()
    {
        var response = await _client.PostAsJsonAsync("/games", new CreateGameRequest(null, "Insane"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_Games_Liste_Les_Parties_Creees_Avec_Leur_Difficulte()
    {
        var created = await (await _client.PostAsJsonAsync("/games", new CreateGameRequest(null, "Easy")))
            .Content.ReadFromJsonAsync<GameStateDto>();

        var response = await _client.GetAsync("/games");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summaries = await response.Content.ReadFromJsonAsync<GameSummaryDto[]>();
        Assert.NotNull(summaries);
        Assert.Contains(summaries, s => s.GameId == created!.GameId && s.Phase == "InProgress" && s.Difficulty == "Easy");
    }

    [Fact]
    public async Task Post_Games_Avec_Difficulte_Expert_Cree_Une_Partie()
    {
        var response = await _client.PostAsJsonAsync("/games", new CreateGameRequest(null, "Expert"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_Games_En_Mode_Tempete_Cree_Une_Partie_Sur_Une_Grille_Agrandie()
    {
        var response = await _client.PostAsJsonAsync("/games", new CreateGameRequest(null, StormMode: true));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var state = await response.Content.ReadFromJsonAsync<GameStateDto>();
        Assert.NotNull(state);
        Assert.Equal(12, state!.BoardSize);
        Assert.Equal(144, state.PlayerBoard.Count);
        Assert.Equal(144, state.ComputerBoard.Count);
    }

    [Fact]
    public async Task Post_Games_En_Mode_Tempete_Avec_Flotte_Manuelle_Retourne_400()
    {
        var request = new CreateGameRequest(
            [new ShipPlacementDto("Porte-avions", 0, 0, "Horizontal")],
            StormMode: true);

        var response = await _client.PostAsJsonAsync("/games", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_Stats_Reflete_Les_Parties_Creees()
    {
        await _client.PostAsJsonAsync("/games", new CreateGameRequest(null));

        var response = await _client.GetAsync("/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stats = await response.Content.ReadFromJsonAsync<GlobalStatsDto>();
        Assert.NotNull(stats);
        Assert.True(stats!.TotalGames >= 1);
    }

    [Fact]
    public async Task Post_Shots_Hors_Des_Bornes_De_Cette_Partie_Retourne_400()
    {
        var created = await CreateRandomGameAsync(_client);

        // 11 respecte la borne large de FluentValidation (jusqu'à 11 pour le mode Tempête)
        // mais dépasse la grille 10x10 de cette partie standard.
        var response = await _client.PostAsJsonAsync($"/games/{created.GameId}/shots", new ShotRequest(11, 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
        var created = await CreateRandomGameAsync(_client);

        var response = await _client.PostAsJsonAsync($"/games/{created.GameId}/shots", new ShotRequest(0, 0));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var turn = await response.Content.ReadFromJsonAsync<TurnResultDto>();
        Assert.NotNull(turn);
        Assert.Equal(0, turn!.PlayerShot.Row);
        Assert.Equal(0, turn.PlayerShot.Col);
    }

    [Fact]
    public async Task Post_Shots_Avec_Coordonnees_Hors_Grille_Retourne_400()
    {
        var created = await CreateRandomGameAsync(_client);

        var response = await _client.PostAsJsonAsync($"/games/{created.GameId}/shots", new ShotRequest(42, 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Shots_Sur_Partie_Inconnue_Retourne_404()
    {
        var response = await _client.PostAsJsonAsync($"/games/{Guid.NewGuid()}/shots", new ShotRequest(0, 0));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Shots_Sur_Partie_Deja_Terminee_Retourne_409()
    {
        var created = await CreateRandomGameAsync(_client);
        var phase = "InProgress";

        // Balaie toute la grille 10x10 : couvre nécessairement tous les navires (position aléatoire
        // inconnue du test), la partie se termine donc forcément avant la fin du balayage.
        for (var row = 0; row < 10 && phase == "InProgress"; row++)
        {
            for (var col = 0; col < 10 && phase == "InProgress"; col++)
            {
                var turn = await (await _client.PostAsJsonAsync($"/games/{created.GameId}/shots", new ShotRequest(row, col)))
                    .Content.ReadFromJsonAsync<TurnResultDto>();
                phase = turn!.State.Phase;
            }
        }

        Assert.NotEqual("InProgress", phase);

        var response = await _client.PostAsJsonAsync($"/games/{created.GameId}/shots", new ShotRequest(0, 0));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_Shots_Avec_Un_Corps_Json_Malforme_Retourne_Un_ProblemDetails_Sans_Details_Internes()
    {
        var created = await CreateRandomGameAsync(_client);
        var malformed = new StringContent("""{"row":"abc","col":0}""", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"/games/{created.GameId}/shots", malformed);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        var fields = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(body);
        Assert.NotNull(fields);
        Assert.False(fields!.ContainsKey("exception"), "La réponse ne doit pas exposer la pile d'appel interne au client.");
        Assert.DoesNotContain("   at ", body);
    }
}
