using BattleShip.API;
using BattleShip.Models;
using Microsoft.Extensions.Time.Testing;

namespace BattleShip.Tests.Api;

public class GameStoreTests
{
    [Fact]
    public void RemoveExpired_Supprime_Une_Partie_Inactive_Depuis_Plus_Longtemps_Que_Le_Delai()
    {
        var time = new FakeTimeProvider();
        var store = new GameStore(time);
        var game = store.CreateRandom();

        time.Advance(TimeSpan.FromMinutes(31));

        var removed = store.RemoveExpired(TimeSpan.FromMinutes(30));

        Assert.Equal(1, removed);
        Assert.False(store.TryGet(game.Id, out _));
    }

    [Fact]
    public void RemoveExpired_Conserve_Une_Partie_Encore_Dans_Le_Delai()
    {
        var time = new FakeTimeProvider();
        var store = new GameStore(time);
        var game = store.CreateRandom();

        time.Advance(TimeSpan.FromMinutes(10));

        var removed = store.RemoveExpired(TimeSpan.FromMinutes(30));

        Assert.Equal(0, removed);
        Assert.True(store.TryGet(game.Id, out _));
    }

    [Fact]
    public void TryGet_Rafraichit_Lactivite_Et_Empeche_Lexpiration_Prematuree()
    {
        var time = new FakeTimeProvider();
        var store = new GameStore(time);
        var game = store.CreateRandom();

        time.Advance(TimeSpan.FromMinutes(20));
        store.TryGet(game.Id, out _); // simule un accès (GET /games/{id} ou un tir)
        time.Advance(TimeSpan.FromMinutes(20));

        // 40 minutes se sont écoulées depuis la création, mais seulement 20 depuis le dernier accès.
        var removed = store.RemoveExpired(TimeSpan.FromMinutes(30));

        Assert.Equal(0, removed);
        Assert.True(store.TryGet(game.Id, out _));
    }

    [Fact]
    public void RemoveExpired_Ne_Supprime_Pas_Une_Partie_Toujours_Active_Mais_Nettoie_Les_Autres()
    {
        var time = new FakeTimeProvider();
        var store = new GameStore(time);
        var oldGame = store.CreateRandom();
        time.Advance(TimeSpan.FromMinutes(31));
        var recentGame = store.CreateRandom();

        var removed = store.RemoveExpired(TimeSpan.FromMinutes(30));

        Assert.Equal(1, removed);
        Assert.False(store.TryGet(oldGame.Id, out _));
        Assert.True(store.TryGet(recentGame.Id, out _));
        Assert.Equal(1, store.Count);
    }

    [Fact]
    public void CreateStorm_Genere_Deux_Plateaux_Agrandis_Avec_La_Flotte_Elargie()
    {
        var store = new GameStore(new FakeTimeProvider());

        var game = store.CreateStorm();

        Assert.Equal(FleetFactory.StormBoardSize, game.PlayerBoard.Size);
        Assert.Equal(FleetFactory.StormBoardSize, game.ComputerBoard.Size);
        Assert.Equal(FleetFactory.StormFleet.Count, game.PlayerBoard.Ships.Count);
        Assert.Equal(FleetFactory.StormFleet.Count, game.ComputerBoard.Ships.Count);
    }

    [Fact]
    public void ListAll_Rapporte_Le_Mode_Tempete_Des_Parties_Concernees()
    {
        var store = new GameStore(new FakeTimeProvider());
        var stormGame = store.CreateStorm();
        var standardGame = store.CreateRandom();

        var summaries = store.ListAll();

        Assert.True(summaries.Single(s => s.Id == stormGame.Id).StormMode);
        Assert.False(summaries.Single(s => s.Id == standardGame.Id).StormMode);
    }

    [Fact]
    public void GetGlobalStats_Sans_Partie_Ne_Renvoie_Aucune_Moyenne()
    {
        var store = new GameStore(new FakeTimeProvider());

        var stats = store.GetGlobalStats();

        Assert.Equal(0, stats.TotalGames);
        Assert.Null(stats.AverageShotsToFinish);
    }

    [Fact]
    public void GetGlobalStats_Compte_Les_Victoires_Et_Les_Parties_En_Cours_Separement()
    {
        var store = new GameStore(new FakeTimeProvider());
        var finishedGame = store.CreateRandom();
        store.CreateRandom(); // reste en cours, ne doit pas compter comme terminée.

        SinkAllComputerShips(finishedGame);

        var stats = store.GetGlobalStats();

        Assert.Equal(2, stats.TotalGames);
        Assert.Equal(1, stats.PlayerWins);
        Assert.Equal(0, stats.ComputerWins);
        Assert.Equal(1, stats.InProgressCount);
        Assert.NotNull(stats.AverageShotsToFinish);
    }

    // Le tir agit directement sur l'instance suivie par le store (Game est une classe, pas une copie) :
    // couler la flotte via l'objet retourné par CreateRandom met donc bien à jour la partie du store.
    private static void SinkAllComputerShips(Game game)
    {
        while (game.Phase == GamePhase.InProgress)
        {
            var target = game.ComputerBoard.Ships
                .SelectMany(s => s.Cells)
                .First(c => !game.ComputerBoard.HasBeenShotAt(c));
            game.Shoot(target);
        }
    }
}
