using System.Net;
using System.Net.Http.Json;
using BattleShip.App.Services;
using BattleShip.App.Tests.Fakes;
using BattleShip.Models.Contracts;

namespace BattleShip.App.Tests.Services;

public class GameApiClientTests
{
    private static CellDto[] EmptyBoard()
    {
        var cells = new List<CellDto>();
        for (var row = 0; row < 10; row++)
        {
            for (var col = 0; col < 10; col++)
                cells.Add(new CellDto(row, col, "Unknown"));
        }

        return cells.ToArray();
    }

    private static GameApiClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        new(new HttpClient(new FakeHttpMessageHandler(responder)) { BaseAddress = new Uri("http://test/") });

    [Fact]
    public async Task GetStateAsync_Retourne_Letat_De_La_Partie()
    {
        var gameId = Guid.NewGuid();
        var state = new GameStateDto(gameId, "InProgress", 10, EmptyBoard(), EmptyBoard());
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(state) });

        var result = await client.GetStateAsync(gameId);

        Assert.Equal(gameId, result.GameId);
        Assert.Equal("InProgress", result.Phase);
    }

    [Fact]
    public async Task GetStateAsync_Sur_Une_Reponse_En_Erreur_Leve_Une_Exception_Avec_Le_Corps_De_La_Reponse()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Partie inconnue.") });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetStateAsync(Guid.NewGuid()));

        Assert.Contains("Partie inconnue.", exception.Message);
    }
}
