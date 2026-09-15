using BattleShip.Models;

namespace BattleShip.Tests.Domain;

public class ExpertComputerStrategyTests
{
    [Fact]
    public void Apres_Deux_Touches_Alignees_Vise_Une_Extremite_De_La_Ligne()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Croiseur", [new Coordinate(4, 4), new Coordinate(4, 5), new Coordinate(4, 6), new Coordinate(4, 7)]));
        board.ReceiveShot(new Coordinate(4, 5));
        board.ReceiveShot(new Coordinate(4, 6));

        var strategy = new ExpertComputerStrategy(new Random(1));
        var target = strategy.ChooseTarget(board);

        Assert.Contains(target, new[] { new Coordinate(4, 4), new Coordinate(4, 7) });
    }

    [Fact]
    public void Si_Une_Extremite_De_Ligne_Sort_De_La_Grille_Vise_Lautre_Extremite()
    {
        // Navire collé au bord gauche : l'extrémité "avant" (colonne -1) sort de la grille,
        // seule l'extrémité "après" (colonne 2) est un choix valide.
        var board = new Board();
        board.PlaceShip(new Ship("Croiseur", [new Coordinate(4, 0), new Coordinate(4, 1), new Coordinate(4, 2), new Coordinate(4, 3)]));
        board.ReceiveShot(new Coordinate(4, 0));
        board.ReceiveShot(new Coordinate(4, 1));

        var strategy = new ExpertComputerStrategy(new Random(1));
        var target = strategy.ChooseTarget(board);

        Assert.Equal(new Coordinate(4, 2), target);
    }

    [Fact]
    public void Ne_Vise_Jamais_Une_Case_Deja_Jouee()
    {
        var board = FleetFactory.CreateRandomBoard(new Random(7));
        var strategy = new ExpertComputerStrategy(new Random(8));

        for (var i = 0; i < Board.DefaultSize * Board.DefaultSize; i++)
        {
            if (board.AllShipsSunk)
                break;

            var target = strategy.ChooseTarget(board);
            Assert.False(board.HasBeenShotAt(target));
            board.ReceiveShot(target);
        }

        Assert.True(board.AllShipsSunk);
    }

    [Fact]
    public void En_Mode_Chasse_Privilegie_Une_Case_Compatible_Avec_Le_Seul_Navire_Restant()
    {
        // Grille où toutes les cases sont déjà jouées (Miss) sauf une bande horizontale de 2 cases libres :
        // le seul navire restant (taille 2, Torpilleur) ne peut se placer que sur ces deux cases.
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 8), new Coordinate(0, 9)]));

        for (var row = 0; row < Board.DefaultSize; row++)
        {
            for (var col = 0; col < Board.DefaultSize; col++)
            {
                if (row == 0 && col is 8 or 9)
                    continue;

                board.ReceiveShot(new Coordinate(row, col));
            }
        }

        var strategy = new ExpertComputerStrategy(new Random(1));
        var target = strategy.ChooseTarget(board);

        Assert.Contains(target, new[] { new Coordinate(0, 8), new Coordinate(0, 9) });
    }

    [Fact]
    public void Quand_Les_Deux_Extremites_De_Ligne_Sont_Bloquees_Vise_Une_Case_Adjacente_A_Une_Touche()
    {
        // Deux touches non adjacentes (colonnes 0 et 2, case 1 non touchée entre les deux) :
        // l'extrémité "avant" sort de la grille, l'extrémité "après" (colonne 3) est déjà jouée.
        var board = new Board();
        board.PlaceShip(new Ship("Contre-torpilleur", [new Coordinate(4, 0), new Coordinate(4, 1), new Coordinate(4, 2)]));
        board.ReceiveShot(new Coordinate(4, 0));
        board.ReceiveShot(new Coordinate(4, 2));
        board.ReceiveShot(new Coordinate(4, 3));

        var strategy = new ExpertComputerStrategy(new Random(1));
        var target = strategy.ChooseTarget(board);

        Coordinate[] expectedCandidates =
        [
            new Coordinate(3, 0), new Coordinate(5, 0), new Coordinate(4, 1),
            new Coordinate(3, 2), new Coordinate(5, 2)
        ];
        Assert.Contains(target, expectedCandidates);
    }

    [Fact]
    public void Quand_Aucun_Placement_Nest_Possible_Utilise_Un_Tir_Aleatoire_De_Secours()
    {
        var board = new Board();
        board.PlaceShip(new Ship("Torpilleur", [new Coordinate(0, 0), new Coordinate(0, 1)]));
        board.ReceiveShot(new Coordinate(0, 0));
        board.ReceiveShot(new Coordinate(0, 1));
        Assert.True(board.AllShipsSunk);

        var strategy = new ExpertComputerStrategy(new Random(1));
        var target = strategy.ChooseTarget(board);

        Assert.True(target.IsWithin(Board.DefaultSize));
        Assert.False(board.HasBeenShotAt(target));
    }
}
