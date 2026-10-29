namespace GestionOrdinateurs.Tests;
using GestionOrdinateurs.API;
using GestionOrdinateurs.API.Controllers;
using GestionOrdinateurs.API.Services.Implementations;
using GestionOrdinateurs.Shared.classes;
using GestionOrdinateurs.Shared.enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

public class OrdinateurServiceTest : IDisposable
{
    private readonly string _journalRoot = Path.Combine(
        Path.GetTempPath(),
        "GestionOrdinateurs.Tests",
        Guid.NewGuid().ToString());

    private ParcInformatiqueContext CreerContexte()
    {
        var options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new ParcInformatiqueContext(options);
    }

    private OrdinateurService CreerService(ParcInformatiqueContext context)
    {
        return new OrdinateurService(context, new JournalService(_journalRoot));
    }


    [Fact]
    public async Task Ajouter_Plusieurs_Ordinateurs_Permet_De_Les_Recuperer_Apres()
    {
        // Arrange
        var context = CreerContexte();
        var service = CreerService(context);
        var ordinateurs = new List<Ordinateur>
        {
            CreerOrdinateur(1, "Ordinateur Fixe 1", "Dell"),
            CreerOrdinateur(2, "Ordinateur Fixe 2", "HP")
        };

        // Act
        await service.AjouterOrdinateurs(ordinateurs);
        var ordinateursRecuperes = await service.ListerOrdinateurs();

        // Assert
        ordinateursRecuperes.Should().HaveCount(2);
        ordinateursRecuperes.Select(ordinateur => ordinateur.ID).Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public async Task Supprimer_Ordinateur_Retourne_False_Si_Non_Existant()
    {
        // Arrange
        var context = CreerContexte();
        var service = CreerService(context);
        var idNonExistant = context.Ordinateurs.Count() + 1; // Un ID qui n'existe pas dans la liste

        // Act
        var resultat = await service.SupprimerOrdinateur(idNonExistant);

        // Assert
        resultat.Should().BeFalse();
    }

    [Fact]
    public async Task Supprimer_Ordinateur_Enleve_Ordinateur_De_La_Liste()
    {
        // Arrange
        var context = CreerContexte();
        var service = CreerService(context);

        // Act
        await service.SupprimerOrdinateur(1);
        var resultat = await service.ObtenirParId(1);

        // Assert
        resultat.Should().BeNull();
    }

    [Fact]
    public async Task Appel_Marque_Inexistante_Retourne_Liste_Vide()
    {
        // Arrange
        var context = CreerContexte();
        var service = CreerService(context);

        // Act
        var resultat = await service.ListerOrdinateursParMarque("MarqueInexistante");

        // Assert
        resultat.Should().BeEmpty();
    }

    [Fact]
    public async Task Appel_Marque_Existante_Retourne_Liste()
    {
        // Arrange
        var context = CreerContexte();
        var service = CreerService(context);
        var nouvelOrdinateur = CreerOrdinateur(1, "Ordinateur Test", "HP");

        // Act
        await service.AjouterOrdinateurs(new List<Ordinateur> { nouvelOrdinateur });
        var resultat = await service.ListerOrdinateursParMarque("HP");
        
        // Assert
        resultat.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetStatistiques_RetourneUnObjetJsonAvecLesStatistiques()
    {
        await using var context = CreerContexte();
        var ordinateurs = new[]
        {
            CreerOrdinateur(1, "Ordinateur disponible", "Dell"),
            CreerOrdinateur(2, "Ordinateur indisponible", "HP")
        };
        ordinateurs[1].Stock = 0;
        context.Ordinateurs.AddRange(ordinateurs);
        context.Reparations.Add(new GestionOrdinateurs.Shared.classes.Reparation(
            0,
            "Test",
            DateTime.UtcNow,
            "Terminée",
            "Technicien")
        {
            OrdinateurID = ordinateurs[0].ID
        });
        await context.SaveChangesAsync();

        var service = CreerService(context);
        var controller = new OrdinateursController(
            service,
            NullLogger<OrdinateursController>.Instance,
            new ConfigurationBuilder().Build());

        var response = await controller.GetStatistiques();

        var result = response.Result.Should().BeOfType<OkObjectResult>().Subject;
        var json = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        root.GetProperty("totalOrdinateur").GetInt32().Should().Be(2);
        root.GetProperty("ordiDisponibles").GetInt32().Should().Be(1);
        root.GetProperty("valeurStock").GetDecimal().Should().Be(5000m);
        root.GetProperty("nbReparations").GetInt32().Should().Be(1);
    }

    private static Ordinateur CreerOrdinateur(int id, string nom, string marque)
    {
        var caracteristiques = new Caracteristiques(
            16,
            TypeOrdinateur.Portable,
            512,
            marque,
            "Intel Core i7");

        return new Ordinateur(
            id,
            nom,
            marque,
            16,
            "Intel Core i7",
            caracteristiques,
            500,
            10);
    }

    public void Dispose()
    {
        if (Directory.Exists(_journalRoot))
            Directory.Delete(_journalRoot, recursive: true);
    }

}
