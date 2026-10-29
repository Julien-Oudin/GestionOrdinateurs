using GestionOrdinateurs.Shared.classes;
using GestionOrdinateurs.Shared.enums;
using GestionOrdinateurs.API.Services.Interfaces;
using GestionOrdinateurs.API.Abonnements;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services.Implementations;

public class OrdinateurService : IOrdinateurService
{
    private readonly ParcInformatiqueContext _context;
    private readonly JournalService _journalService;

    // Déclare les événements pour notifier les abonnés lorsque des ordinateurs sont ajoutés, supprimés ou mis à jour.
    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;


    public OrdinateurService(ParcInformatiqueContext context, JournalService journalService)
    {
        // Initialise le contexte de la base de données et abonne un afficheur aux événements.
        _context = context;
        _journalService = journalService;
        var afficheur = new AfficheurAbonne();
        OrdinateurAjoute += afficheur.AfficherMessageAjout;
        OrdinateurSupprime += afficheur.AfficherMessageSuppression;
        OrdinateurMisAJour += afficheur.AfficherMessageMiseAJour;
    }

    public async Task<List<Ordinateur>> ListerOrdinateurs()
    {
        return await OrdinateursAvecReparations().ToListAsync();
    }

    // Permet de lister les ordinateurs par marque. Ignore la casse
    public async Task<List<Ordinateur>> ListerOrdinateursParMarque(string marque)
    {
        var ordinateurs = await OrdinateursAvecReparations().ToListAsync();
        return ordinateurs
            .Where(o => string.Equals(
                o.Caracteristiques.Marque,
                marque,
                StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Récupère un ordinateur par son ID. Retourne null si l'ordinateur n'existe pas. Il renvoie le premier qu'il trouve avec l'ID correspondant. Les suivants sont ignorés.
    public async Task<Ordinateur?> ObtenirParId(int id)
    {
        return await OrdinateursAvecReparations().FirstOrDefaultAsync(o => o.ID == id);
    }

    public async Task AjouterOrdinateurs(List<Ordinateur> ordinateurs)
    {
        ArgumentNullException.ThrowIfNull(ordinateurs);

        if (ordinateurs.Any(ordinateur => ordinateur is null))
            throw new ArgumentException("La liste ne peut pas contenir d'ordinateur null.", nameof(ordinateurs));

        var ids = ordinateurs.Select(ordinateur => ordinateur.ID).ToList();
        if (ids.Count != ids.Distinct().Count())
            throw new InvalidOperationException("La liste contient plusieurs ordinateurs avec le même ID.");

        var idsExistants = await _context.Ordinateurs
            .Where(ordinateur => ids.Contains(ordinateur.ID))
            .Select(ordinateur => ordinateur.ID)
            .ToListAsync();
        if (idsExistants.Count > 0)
            throw new InvalidOperationException($"Un ordinateur avec l'ID {idsExistants[0]} existe déjà.");

        _context.Ordinateurs.AddRange(ordinateurs);
        await _context.SaveChangesAsync();

        foreach (var ordinateur in ordinateurs)
        {
            OrdinateurAjoute?.Invoke(this, new OrdinateurAjouteEventArgs(ordinateur));
            _journalService.EcrireOrdinateur($"{ordinateur.Nom} ajouté");
        }
    }

    public async Task<bool> SupprimerOrdinateur(int id)
    {
        var ordinateur = _context.Ordinateurs.FirstOrDefault(o => o.ID == id);
        if (ordinateur is null)
            return false;

        OrdinateurSupprime?.Invoke(this, new OrdinateurSupprimeEventArgs(ordinateur));
        _context.Ordinateurs.Remove(ordinateur);
        await _context.SaveChangesAsync();
        _journalService.EcrireOrdinateur($"{ordinateur.Nom} supprimé");
        return true;
    }

    // Met à jour un ordinateur existant. Si l'ordinateur n'existe pas, il retourne false. Sinon, il met à jour l'ordinateur et retourne true.
    public async Task<bool> MettreAJourOrdinateur(int id, Ordinateur ordinateurModifie)
    {
        if (ordinateurModifie is null)
            throw new ArgumentNullException(nameof(ordinateurModifie));

        var ordinateur = _context.Ordinateurs.FirstOrDefault(o => o.ID == id);
        if (ordinateur is null)
            return false;

        // Copie les propriétés modifiées sur l'ordinateur
        ordinateur.Caracteristiques = ordinateurModifie.Caracteristiques;
        ordinateur.Nom = ordinateurModifie.Nom;
        ordinateur.Prix = ordinateurModifie.Prix;
        ordinateur.Vendu = ordinateurModifie.Vendu;
        ordinateur.Reparations = ordinateurModifie.Reparations;
        ordinateur.Etat = ordinateurModifie.Etat;

        OrdinateurMisAJour?.Invoke(this, new OrdinateurMisAJourEventArgs(ordinateur)); // après SaveChanges
        _context.Ordinateurs.Update(ordinateur);
        await _context.SaveChangesAsync();
        _journalService.EcrireOrdinateur($"{ordinateur.Nom} modifié");
        return true;
    }

    // Cherche tous les ordinateurs avec une certaine marque. Retourne une liste vide si aucun ordinateur n'est trouvé.
    public async Task<List<Ordinateur>> ChercherParMarque(string marque)
    {
        return await ListerOrdinateursParMarque(marque);
    }

    // Récupère tous les ordinateurs par ordre décroissant de prix. Retourne une liste vide si aucun ordinateur n'est trouvé.
    public async Task<List<Ordinateur>> RenvoyerParPrixDecroissant()
    {
        return await OrdinateursAvecReparations()
            .OrderByDescending(o => o.Prix)
            .ToListAsync();
    }

    // Récupère tous les ordinateurs disponibles (stock > 0). Retourne une liste vide si aucun ordinateur n'est trouvé.
    public async Task<List<Ordinateur>> OrdinateursDisponibles()
    {
        return await OrdinateursAvecReparations()
            .Where(o => o.Stock > 0)
            .ToListAsync();
    }

    private IQueryable<Ordinateur> OrdinateursAvecReparations()
    {
        return _context.Ordinateurs.Include(o => o.Reparations);
    }

    public async Task<string> Diagnostiquer(Ordinateur ordinateur)
    {
        return await Task.FromResult(ordinateur switch
        {
            {
                Etat: EtatOrdinateur.HorsService
            }
                => "Remplacement",

            {
                Etat: EtatOrdinateur.Lent,
                Caracteristiques:
                {
                    RamGo: < 8
                }
            }
                => "Ajouter RAM",

            _
                => "Aucune action"
        });
    }

    public async Task<(int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)> CalculerStatistiquesAsync()
    {
        var total = await _context.Ordinateurs.CountAsync();
        var disponibles = await _context.Ordinateurs.CountAsync(o => o.Stock > 0);
        var valeurStock = await _context.Ordinateurs
            .SumAsync(o => (decimal?)(o.Prix * o.Stock)) ?? 0m;
        var nbReparations = await _context.Reparations.CountAsync();

        return (total, disponibles, valeurStock, nbReparations);
    }

    
}
