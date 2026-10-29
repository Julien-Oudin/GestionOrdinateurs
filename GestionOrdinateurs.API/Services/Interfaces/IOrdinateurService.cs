
using GestionOrdinateurs.Shared.classes;

namespace GestionOrdinateurs.API.Services.Interfaces;

public interface IOrdinateurService
{
    public Task<List<Ordinateur>> ListerOrdinateurs();

    // Permet de lister les ordinateurs par marque. Ignore la casse
    public Task<List<Ordinateur>> ListerOrdinateursParMarque(string marque);

    // Récupère un ordinateur par son ID. Retourne null si l'ordinateur n'existe pas. Il renvoie le premier qu'il trouve avec l'ID correspondant. Les suivants sont ignorés.
    public Task<Ordinateur?> ObtenirParId(int id);

    // Après vérification, ajoute les ordinateurs en une seule sauvegarde.
    public Task AjouterOrdinateurs(List<Ordinateur> ordinateurs);

    public Task<bool> SupprimerOrdinateur(int id);

    // Met à jour un ordinateur existant. Si l'ordinateur n'existe pas, il retourne false. Sinon, il met à jour l'ordinateur et retourne true.
    public Task<bool> MettreAJourOrdinateur(int id, Ordinateur ordinateurModifie);

    // Cherche tous les ordinateurs avec une certaine marque. Retourne une liste vide si aucun ordinateur n'est trouvé.
    public Task<List<Ordinateur>> ChercherParMarque(string marque);

    // Récupère tous les ordianteurs par ordre décroissant de prix. Retourne une liste vide si aucun ordinateur n'est trouvé.
    public Task<List<Ordinateur>> RenvoyerParPrixDecroissant();

    // Récupère tous les ordinateurs disponibles (stock > 0). Retourne une liste vide si aucun ordinateur n'est trouvé.
    public Task<List<Ordinateur>> OrdinateursDisponibles();

    // Diagnostique un ordinateur donné. Selon l'Etat de l'ordinateur la réponse va changer.
    public Task<string> Diagnostiquer(Ordinateur ordinateur);

    // Calcule les statistiques sur les ordinateurs et les réparations.
    public Task<(int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)> CalculerStatistiquesAsync();
}
