using BattleShip.App.Components;
using BattleShip.Models.Contracts;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BattleShip.App.Tests.Components;

public class BoardGridTests : BunitContext
{
    [Fact]
    public void Affiche_Une_Grille_10x10_Avec_Les_Reperes_De_Coordonnees()
    {
        var cut = Render<BoardGrid>(p => p.Add(c => c.Cells, Array.Empty<CellDto>()));

        Assert.Equal(100, cut.FindAll(".board-cell").Count);

        var headerLabels = cut.FindAll(".board-row")[0].QuerySelectorAll(".board-label");
        Assert.Equal("A", headerLabels[1].TextContent);
        Assert.Equal("J", headerLabels[10].TextContent);

        var firstDataRowLabel = cut.FindAll(".board-row")[1].QuerySelector(".board-label");
        Assert.Equal("1", firstDataRowLabel!.TextContent);
    }

    [Fact]
    public void Une_Case_Inconnue_Sans_Etat_Fourni_Est_Rendue_Comme_Unknown()
    {
        var cut = Render<BoardGrid>(p => p.Add(c => c.Cells, Array.Empty<CellDto>()));

        var firstCell = cut.Find(".board-cell");

        Assert.Contains("unknown", firstCell.ClassList);
        Assert.Equal(string.Empty, firstCell.TextContent);
    }

    [Theory]
    [InlineData("Hit", "✹")]
    [InlineData("Sunk", "☠")]
    [InlineData("Miss", "•")]
    [InlineData("Ship", "▦")]
    [InlineData("Mine", "☢")]
    [InlineData("MineHit", "💥")]
    public void Chaque_Etat_De_Case_Affiche_La_Bonne_Icone_Et_La_Bonne_Classe(string state, string expectedIcon)
    {
        CellDto[] cells = [new(0, 0, state)];
        var cut = Render<BoardGrid>(p => p.Add(c => c.Cells, cells));

        var cell = cut.Find(".board-cell");

        Assert.Contains(state.ToLowerInvariant(), cell.ClassList);
        Assert.Equal(expectedIcon, cell.TextContent);
    }

    [Theory]
    [InlineData("Hit")]
    [InlineData("Miss")]
    [InlineData("Sunk")]
    [InlineData("MineHit")]
    public void Une_Case_Deja_Jouee_Est_Desactivee_Meme_En_Mode_Interactif(string state)
    {
        CellDto[] cells = [new(0, 0, state)];
        var cut = Render<BoardGrid>(p => p
            .Add(c => c.Cells, cells)
            .Add(c => c.Interactive, true));

        Assert.True(cut.Find(".board-cell").HasAttribute("disabled"));
    }

    [Fact]
    public void Une_Case_Non_Jouee_Nest_Pas_Desactivee_En_Mode_Interactif()
    {
        var cut = Render<BoardGrid>(p => p
            .Add(c => c.Cells, Array.Empty<CellDto>())
            .Add(c => c.Interactive, true));

        Assert.False(cut.Find(".board-cell").HasAttribute("disabled"));
    }

    [Fact]
    public void En_Mode_Non_Interactif_Toutes_Les_Cases_Sont_Desactivees()
    {
        var cut = Render<BoardGrid>(p => p
            .Add(c => c.Cells, Array.Empty<CellDto>())
            .Add(c => c.Interactive, false));

        Assert.All(cut.FindAll(".board-cell"), cell => Assert.True(cell.HasAttribute("disabled")));
    }

    [Fact]
    public void Cliquer_Une_Case_Interactive_Declenche_OnCellClick_Avec_Les_Bonnes_Coordonnees()
    {
        (int Row, int Col)? clicked = null;
        var cut = Render<BoardGrid>(p => p
            .Add(c => c.Cells, Array.Empty<CellDto>())
            .Add(c => c.Interactive, true)
            .Add(c => c.OnCellClick, EventCallback.Factory.Create<(int Row, int Col)>(this, target => clicked = target)));

        // Deuxième ligne de données (index 2 après l'en-tête), troisième case : (row=1, col=2).
        var targetCell = cut.FindAll(".board-row")[2].QuerySelectorAll(".board-cell")[2];
        targetCell.Click();

        Assert.Equal((1, 2), clicked);
    }
}
