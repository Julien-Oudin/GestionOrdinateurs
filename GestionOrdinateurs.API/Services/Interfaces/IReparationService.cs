using GestionOrdinateurs.Shared.classes;
using GestionOrdinateurs.API.Records;

namespace GestionOrdinateurs.API.Services.Interfaces;



// Interface pour le service de réparation, qui sera implémenté par la classe ReparationService
// Elle permettra l'injection de dépendances dans Program.cs (presque un clone de IOrdinateurService)
public interface IReparationService
{

    // Simule une réparation pour un ordinateur donné. Cette méthode crée une nouvelle entrée de réparation dans la base de données, attend 2 secondes pour simuler le temps de réparation, puis met à jour le statut de la réparation à "Terminée".
    public Task SimulerReparationAsync(Ordinateur ordinateur);

    // Récupère toutes les réparations de la base de données, y compris les informations sur l'ordinateur associé à chaque réparation.
    public Task<List<Reparation>> ListerReparations();

    public Task<List<HistoriqueReparationRecord>> ObtenirHistoriqueReparations();
    
}