using AngleSharp.Dom;
using BattleShip.App.Components;
using BattleShip.Models.Contracts;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BattleShip.App.Tests.Components;

public class FleetSetupTests : BunitContext
{
    private static readonly FleetSlotDto[] TwoShipFleet =
    [
        new("Torpilleur", 2),
        new("Sous-marin", 3)
    ];

    private static IElement CellAt(IRenderedComponent<FleetSetup> cut, int row, int col) =>
        cut.FindAll(".board-row")[row + 1].QuerySelectorAll(".board-cell")[col];

    [Fact]
    public void Affiche_La_Flotte_Fournie_Avec_Le_Premier_Navire_Selectionne()
    {
        var cut = Render<FleetSetup>(p => p.Add(c => c.Fleet, TwoShipFleet));

        var items = cut.FindAll(".fleet-list li");
        Assert.Equal(2, items.Count);
        Assert.Contains("selected", items[0].ClassList);
        Assert.DoesNotContain("placed", items[0].ClassList);
    }

    [Fact]
    public void Cliquer_Une_Case_Place_Le_Navire_Selectionne_Et_Avance_La_Selection()
    {
        var cut = Render<FleetSetup>(p => p.Add(c => c.Fleet, TwoShipFleet));

        CellAt(cut, 0, 0).Click();

        var items = cut.FindAll(".fleet-list li");
        Assert.Contains("placed", items[0].ClassList);
        Assert.Contains("selected", items[1].ClassList);
        Assert.Equal(2, cut.FindAll(".board-cell.ship").Count);
    }

    [Fact]
    public void Placement_Vertical_Etend_Le_Navire_Vers_Le_Bas()
    {
        var cut = Render<FleetSetup>(p => p.Add(c => c.Fleet, TwoShipFleet));

        cut.FindAll("button").First(b => b.TextContent.Contains("Vertical")).Click();
        CellAt(cut, 0, 0).Click();

        Assert.Contains("ship", CellAt(cut, 0, 0).ClassList);
        Assert.Contains("ship", CellAt(cut, 1, 0).ClassList);
        Assert.DoesNotContain("ship", CellAt(cut, 0, 1).ClassList);
    }

    [Fact]
    public void Un_Chevauchement_Affiche_Une_Erreur_Et_Ne_Place_Pas_Le_Navire()
    {
        var cut = Render<FleetSetup>(p => p.Add(c => c.Fleet, TwoShipFleet));
        CellAt(cut, 0, 0).Click(); // Torpilleur (2) en (0,0)-(0,1)

        CellAt(cut, 0, 1).Click(); // Sous-marin chevauchant en (0,1)

        Assert.Contains("chevauche", cut.Find(".error").TextContent);
        var items = cut.FindAll(".fleet-list li");
        Assert.DoesNotContain("placed", items[1].ClassList);
    }

    [Fact]
    public void Un_Debordement_Affiche_Une_Erreur_Et_Ne_Place_Pas_Le_Navire()
    {
        var cut = Render<FleetSetup>(p => p.Add(c => c.Fleet, TwoShipFleet));

        // Sous-marin (taille 3) tenté en position verticale à partir de la ligne 8 : déborde (8,9,10).
        cut.FindAll("button").First(b => b.TextContent.Contains("Vertical")).Click();
        CellAt(cut, 8, 0).Click(); // Torpilleur, taille 2, vertical : (8,0)-(9,0), tient dans la grille.

        // Sélectionne maintenant le Sous-marin (taille 3) et tente un débordement vertical.
        CellAt(cut, 8, 5).Click();

        Assert.Contains("déborde", cut.Find(".error").TextContent);
    }

    [Fact]
    public void Recommencer_Efface_Tous_Les_Placements()
    {
        var cut = Render<FleetSetup>(p => p.Add(c => c.Fleet, TwoShipFleet));
        CellAt(cut, 0, 0).Click();

        cut.Find(".fleet-setup-actions button:not(.btn-primary)").Click();

        Assert.Empty(cut.FindAll(".board-cell.ship"));
        Assert.DoesNotContain("placed", cut.Find(".fleet-list li").ClassList);
    }

    [Fact]
    public void Le_Bouton_Valider_Est_Desactive_Tant_Que_Toute_La_Flotte_Nest_Pas_Placee()
    {
        var cut = Render<FleetSetup>(p => p.Add(c => c.Fleet, TwoShipFleet));

        Assert.True(cut.Find(".btn-primary").HasAttribute("disabled"));

        CellAt(cut, 0, 0).Click();
        CellAt(cut, 2, 0).Click();

        Assert.False(cut.Find(".btn-primary").HasAttribute("disabled"));
    }

    [Fact]
    public void Valider_Invoque_OnConfirmed_Avec_Les_Placements_Effectues()
    {
        IReadOnlyList<ShipPlacementDto>? confirmed = null;
        var cut = Render<FleetSetup>(p => p
            .Add(c => c.Fleet, TwoShipFleet)
            .Add(c => c.OnConfirmed, EventCallback.Factory.Create<IReadOnlyList<ShipPlacementDto>>(this, list => confirmed = list)));

        CellAt(cut, 0, 0).Click();
        CellAt(cut, 2, 0).Click();
        cut.Find(".btn-primary").Click();

        Assert.NotNull(confirmed);
        Assert.Equal(2, confirmed!.Count);
        Assert.Contains(confirmed, p => p.Name == "Torpilleur" && p.Row == 0 && p.Col == 0);
        Assert.Contains(confirmed, p => p.Name == "Sous-marin" && p.Row == 2 && p.Col == 0);
    }
}
