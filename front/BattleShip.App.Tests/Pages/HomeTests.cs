using System.Net;
using System.Net.Http.Json;
using BattleShip.API.Grpc;
using BattleShip.App.Components;
using BattleShip.App.Pages;
using BattleShip.App.Services;
using BattleShip.App.Tests.Fakes;
using BattleShip.Models.Contracts;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace BattleShip.App.Tests.Pages;

public class HomeTests : BunitContext
{
    private static readonly FleetSlotDto[] Fleet = [new("Torpilleur", 2), new("Sous-marin", 3)];

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

    private static HttpResponseMessage Json<T>(HttpStatusCode status, T body) =>
        new(status) { Content = JsonContent.Create(body) };

    private static bool IsGet(HttpRequestMessage req, string path) =>
        req.Method.Method == "GET" && req.RequestUri!.AbsolutePath == path;

    private static bool IsPost(HttpRequestMessage req, string path) =>
        req.Method.Method == "POST" && req.RequestUri!.AbsolutePath == path;

    /// Route par défaut : flotte, historique vide et création de partie renvoyant <paramref name="createResponse"/>.
    private static HttpResponseMessage DefaultRoute(HttpRequestMessage req, GameStateDto createResponse)
    {
        if (IsGet(req, "/fleet")) return Json(HttpStatusCode.OK, Fleet);
        if (IsGet(req, "/games")) return Json(HttpStatusCode.OK, Array.Empty<GameSummaryDto>());
        if (IsGet(req, "/stats")) return Json(HttpStatusCode.OK, new GlobalStatsDto(0, 0, 0, 0, null));
        if (IsPost(req, "/games")) return Json(HttpStatusCode.Created, createResponse);
        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    private GameApiClient RegisterApiClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(responder)) { BaseAddress = new Uri("http://test/") };
        var client = new GameApiClient(httpClient);
        Services.AddScoped(_ => client);
        return client;
    }

    private FakeGameStatsClient RegisterStatsClient(FakeGameStatsClient? client = null)
    {
        client ??= new FakeGameStatsClient();
        Services.AddScoped<IGameStatsClient>(_ => client);
        return client;
    }

    [Fact]
    public void Affiche_Le_Bouton_Nouvelle_Partie_Au_Demarrage()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => DefaultRoute(req, state));
        RegisterStatsClient();

        var cut = Render<Home>();

        Assert.Contains("Nouvelle partie", cut.Find(".empty-state .btn-primary").TextContent);
    }

    [Fact]
    public void Activer_Le_Placement_Manuel_Affiche_Lecran_De_Placement()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => DefaultRoute(req, state));
        RegisterStatsClient();

        var cut = Render<Home>();
        cut.Find(".toggle-row input").Change(true);

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindComponents<FleetSetup>()));
    }

    [Fact]
    public void Nouvelle_Partie_Aleatoire_Affiche_Les_Deux_Grilles_Et_Le_Statut()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => DefaultRoute(req, state));
        RegisterStatsClient();

        var cut = Render<Home>();
        cut.Find(".empty-state .btn-primary").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(2, cut.FindAll(".board-card").Count);
            Assert.Contains("Partie en cours", cut.Find(".status-badge").TextContent);
        });
    }

    [Fact]
    public void La_Difficulte_Par_Defaut_Est_Difficile_Et_Le_Choix_Est_Envoye_A_La_Creation()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        CreateGameRequest? capturedRequest = null;
        RegisterApiClient(req =>
        {
            if (IsPost(req, "/games"))
            {
                capturedRequest = req.Content!.ReadFromJsonAsync<CreateGameRequest>().GetAwaiter().GetResult();
                return Json(HttpStatusCode.Created, state);
            }

            return DefaultRoute(req, state);
        });
        RegisterStatsClient();

        var cut = Render<Home>();
        cut.Find(".difficulty-toggle .btn-outline-secondary").Click(); // bascule vers "Facile" (l'autre bouton, "Difficile", est sélectionné par défaut)
        cut.Find(".empty-state .btn-primary").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(capturedRequest);
            Assert.Equal("Easy", capturedRequest!.Difficulty);
        });
    }

    [Fact]
    public void Lhistorique_Affiche_Les_Parties_Renvoyees_Par_Lapi()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        GameSummaryDto[] history =
        [
            new(Guid.NewGuid(), "PlayerWon", "Hard", false, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "ComputerWon", "Easy", false, DateTimeOffset.UtcNow.AddMinutes(-5))
        ];
        RegisterApiClient(req => IsGet(req, "/games") ? Json(HttpStatusCode.OK, history) : DefaultRoute(req, state));
        RegisterStatsClient();

        var cut = Render<Home>();

        cut.WaitForAssertion(() =>
        {
            var items = cut.FindAll(".history-list li");
            Assert.Equal(2, items.Count);
            Assert.Contains("Victoire", items[0].TextContent);
            Assert.Contains("Difficile", items[0].TextContent);
            Assert.Contains("Défaite", items[1].TextContent);
            Assert.Contains("Facile", items[1].TextContent);
        });
    }

    [Fact]
    public void Sans_Historique_Disponible_La_Section_Nest_Pas_Affichee()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => DefaultRoute(req, state));
        RegisterStatsClient();

        var cut = Render<Home>();

        Assert.Empty(cut.FindAll(".history"));
    }

    [Fact]
    public void Un_Tir_Met_A_Jour_Letat_Et_Affiche_Le_Message_Du_Dernier_Tour()
    {
        var gameId = Guid.NewGuid();
        var initialState = new GameStateDto(gameId, "InProgress", 10, EmptyBoard(), EmptyBoard());
        var turnResult = new TurnResultDto(
            new ShotResultDto(0, 0, "Miss"),
            new ShotResultDto(5, 5, "Hit"),
            new GameStateDto(gameId, "InProgress", 10, EmptyBoard(), EmptyBoard()));

        var shotsPath = $"/games/{gameId}/shots";
        RegisterApiClient(req => IsPost(req, shotsPath) ? Json(HttpStatusCode.OK, turnResult) : DefaultRoute(req, initialState));
        RegisterStatsClient();

        var cut = Render<Home>();
        cut.Find(".empty-state .btn-primary").Click();
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".board-card").Count));

        var opponentGrid = cut.FindAll(".board-card")[1];
        opponentGrid.QuerySelectorAll(".board-row")[1].QuerySelectorAll(".board-cell")[0].Click();

        cut.WaitForAssertion(() =>
        {
            var message = cut.Find(".last-turn").TextContent;
            Assert.Contains("manqué", message);
            Assert.Contains("touché", message);
        });
    }

    [Fact]
    public void Une_Erreur_Serveur_A_La_Creation_Affiche_Un_Message_Derreur()
    {
        RegisterApiClient(req =>
        {
            if (IsGet(req, "/fleet")) return Json(HttpStatusCode.OK, Fleet);
            if (IsGet(req, "/games")) return Json(HttpStatusCode.OK, Array.Empty<GameSummaryDto>());
            if (IsPost(req, "/games")) return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        RegisterStatsClient();

        var cut = Render<Home>();
        cut.Find(".empty-state .btn-primary").Click();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".error")));
    }

    [Fact]
    public void Statistiques_Grpc_Affiche_Les_Compteurs_Retournes()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => DefaultRoute(req, state));
        var statsClient = RegisterStatsClient(new FakeGameStatsClient
        {
            Reply = new GameStatsReply { Phase = "InProgress", PlayerShipsRemaining = 4, ComputerShipsRemaining = 5, ShotsFiredByPlayer = 1 }
        });

        var cut = Render<Home>();
        cut.Find(".empty-state .btn-primary").Click();
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".board-card").Count));

        cut.Find(".actions .btn-outline-secondary").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("4", cut.Find(".stats").TextContent);
            Assert.Equal(state.GameId, statsClient.LastRequestedGameId);
        });
    }

    [Fact]
    public void Une_Erreur_Grpc_Affiche_Un_Message_Derreur()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => DefaultRoute(req, state));
        RegisterStatsClient(new FakeGameStatsClient { ExceptionToThrow = new InvalidOperationException("indisponible") });

        var cut = Render<Home>();
        cut.Find(".empty-state .btn-primary").Click();
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".board-card").Count));

        cut.Find(".actions .btn-outline-secondary").Click();

        cut.WaitForAssertion(() => Assert.Contains("indisponible", cut.Find(".error").TextContent));
    }

    [Fact]
    public void Activer_Le_Mode_Tempete_Affiche_Lencart_De_Regles_Et_Desactive_Le_Placement_Manuel()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => DefaultRoute(req, state));
        RegisterStatsClient();

        var cut = Render<Home>();
        var checkboxes = cut.FindAll(".toggle-row input");
        checkboxes[1].Change(true); // Mode Tempête

        cut.WaitForAssertion(() =>
        {
            Assert.NotEmpty(cut.FindAll(".storm-rules"));
            Assert.True(cut.FindAll(".toggle-row input")[0].HasAttribute("disabled"));
        });
    }

    [Fact]
    public void Activer_Le_Placement_Manuel_Desactive_Le_Mode_Tempete()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => DefaultRoute(req, state));
        RegisterStatsClient();

        var cut = Render<Home>();
        cut.FindAll(".toggle-row input")[0].Change(true); // Placement manuel

        cut.WaitForAssertion(() => Assert.True(cut.FindAll(".toggle-row input")[1].HasAttribute("disabled")));
    }

    [Fact]
    public void Le_Mode_Tempete_Est_Envoye_A_La_Creation_De_Partie()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 12, EmptyBoard(), EmptyBoard());
        CreateGameRequest? capturedRequest = null;
        RegisterApiClient(req =>
        {
            if (IsPost(req, "/games"))
            {
                capturedRequest = req.Content!.ReadFromJsonAsync<CreateGameRequest>().GetAwaiter().GetResult();
                return Json(HttpStatusCode.Created, state);
            }

            return DefaultRoute(req, state);
        });
        RegisterStatsClient();

        var cut = Render<Home>();
        cut.FindAll(".toggle-row input")[1].Change(true); // Mode Tempête
        cut.Find(".empty-state .btn-primary").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest!.StormMode);
        });
    }

    [Fact]
    public void La_Difficulte_Experte_Peut_Etre_Selectionnee()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        CreateGameRequest? capturedRequest = null;
        RegisterApiClient(req =>
        {
            if (IsPost(req, "/games"))
            {
                capturedRequest = req.Content!.ReadFromJsonAsync<CreateGameRequest>().GetAwaiter().GetResult();
                return Json(HttpStatusCode.Created, state);
            }

            return DefaultRoute(req, state);
        });
        RegisterStatsClient();

        var cut = Render<Home>();
        cut.FindAll(".difficulty-toggle button").Single(b => b.TextContent.Contains("Experte")).Click();
        cut.Find(".empty-state .btn-primary").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(capturedRequest);
            Assert.Equal("Expert", capturedRequest!.Difficulty);
        });
    }

    [Fact]
    public void Les_Statistiques_Globales_Saffichent_Quand_Des_Parties_Existent()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => IsGet(req, "/stats")
            ? Json(HttpStatusCode.OK, new GlobalStatsDto(5, 3, 2, 0, 12.4))
            : DefaultRoute(req, state));
        RegisterStatsClient();

        var cut = Render<Home>();

        cut.WaitForAssertion(() =>
        {
            var text = cut.Find(".global-stats").TextContent;
            Assert.Contains("5", text);
            Assert.Contains("3", text);
        });
    }

    [Fact]
    public void Erreur_Au_Chargement_De_La_Flotte_Affiche_Un_Message_Derreur()
    {
        RegisterApiClient(req =>
        {
            if (IsGet(req, "/fleet")) return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            if (IsGet(req, "/games")) return Json(HttpStatusCode.OK, Array.Empty<GameSummaryDto>());
            if (IsGet(req, "/stats")) return Json(HttpStatusCode.OK, new GlobalStatsDto(0, 0, 0, 0, null));
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        RegisterStatsClient();

        var cut = Render<Home>();

        cut.WaitForAssertion(() => Assert.Contains("flotte", cut.Find(".error").TextContent));
    }

    [Fact]
    public void Erreur_Au_Chargement_De_Lhistorique_Naffiche_Pas_La_Section()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => IsGet(req, "/games")
            ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
            : DefaultRoute(req, state));
        RegisterStatsClient();

        var cut = Render<Home>();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".history")));
    }

    [Fact]
    public void Un_Tir_En_Echec_Affiche_Un_Message_Derreur()
    {
        var gameId = Guid.NewGuid();
        var initialState = new GameStateDto(gameId, "InProgress", 10, EmptyBoard(), EmptyBoard());
        var shotsPath = $"/games/{gameId}/shots";
        RegisterApiClient(req => IsPost(req, shotsPath)
            ? new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent("La partie est terminée.") }
            : DefaultRoute(req, initialState));
        RegisterStatsClient();

        var cut = Render<Home>();
        cut.Find(".empty-state .btn-primary").Click();
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".board-card").Count));

        var opponentGrid = cut.FindAll(".board-card")[1];
        opponentGrid.QuerySelectorAll(".board-row")[1].QuerySelectorAll(".board-cell")[0].Click();

        cut.WaitForAssertion(() => Assert.Contains("tir n'a pas pu être joué", cut.Find(".error").TextContent));
    }

    [Fact]
    public void Nouvelle_Partie_Depuis_Lecran_De_Jeu_Retourne_A_Lecran_De_Configuration()
    {
        var state = new GameStateDto(Guid.NewGuid(), "InProgress", 10, EmptyBoard(), EmptyBoard());
        RegisterApiClient(req => DefaultRoute(req, state));
        RegisterStatsClient();

        var cut = Render<Home>();
        cut.Find(".empty-state .btn-primary").Click();
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".board-card").Count));

        cut.Find(".actions .btn-secondary").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.NotEmpty(cut.FindAll(".empty-state"));
            Assert.Empty(cut.FindAll(".board-card"));
        });
    }
}
