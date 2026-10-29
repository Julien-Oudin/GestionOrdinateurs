using FluentAssertions;
using GestionOrdinateurs.Shared.classes;

namespace GestionOrdinateurs.Tests;

public class ReparationTest
{
    [Fact]
    public void Creer_Reparation_InitialiseSesProprietes()
    {
        var dateDebut = new DateTime(2026, 10, 15);

        var reparation = new Reparation(
            1,
            "Remplacement écran",
            dateDebut,
            "Terminée",
            "Technicien");

        reparation.Id.Should().Be(1);
        reparation.TypeReparation.Should().Be("Remplacement écran");
        reparation.DateDebut.Should().Be(dateDebut);
        reparation.Statut.Should().Be("Terminée");
        reparation.Technicien.Should().Be("Technicien");
    }
}
