using GestionOrdinateurs.Shared.classes;
using GestionOrdinateurs.API.Services.Interfaces;
using GestionOrdinateurs.API.Records;
using Microsoft.EntityFrameworkCore; 

namespace GestionOrdinateurs.API.Services.Implementations;

public class ReparationService : IReparationService
{
    private readonly ParcInformatiqueContext _context;
    private readonly JournalService _journalService;

    // Pour initialiser le contexte de la base de données et permettre l'injection de dépendances dans Program.cs
    public ReparationService(ParcInformatiqueContext context, JournalService journalService)
    {
        _context = context;
        _journalService = journalService;
    }

    //Fontion refactorisée depuis le TP3 à cause de la mauvaise compréhension des consignes sur les réparations
    public async Task SimulerReparationAsync(Ordinateur ordinateur)
    {
        // Création d'une nouvelle réparation pour l'ordinateur donné
        var reparationTest = new Reparation (
            id: 0,
            typeReparation: "Test",
            dateDebut: DateTime.Now,
            statut: "En cours",
            technicien: "Technicien Test"
        );
        reparationTest.OrdinateurID = ordinateur.ID;
        // Création object reparation (test)
        ordinateur.Reparations.Add(reparationTest);
        await _context.Reparations.AddAsync(reparationTest);
        //Méthode de création d'une réparation en base.
        //await Task.Delay(2000);
        // On modifie bien la reparation et non le retour de la tâche d'ajout à la base réparation.
        reparationTest.Statut = "Terminée";
        await _context.SaveChangesAsync();
        _journalService.Ecrire($"{ordinateur.Nom} réparé");
        Console.WriteLine(
            $"{ordinateur.Nom} réparé");
        return ;
    }

    public async Task<List<Reparation>> ListerReparations()
    {
        return await _context.Reparations.ToListAsync();
    }

    public async Task<List<HistoriqueReparationRecord>> ObtenirHistoriqueReparations()
    {
        return await _context.Reparations
            .Join(
                _context.Ordinateurs,
                reparation => reparation.OrdinateurID,
                ordinateur => ordinateur.ID,
                (reparation, ordinateur) => new HistoriqueReparationRecord(
                    OrdinateurId: ordinateur.ID,
                    NomOrdinateur: ordinateur.Nom,
                    Technicien: reparation.Technicien,
                    TypeReparation: reparation.TypeReparation,
                    DateIntervention: reparation.DateDebut))
            .ToListAsync();
    }
}