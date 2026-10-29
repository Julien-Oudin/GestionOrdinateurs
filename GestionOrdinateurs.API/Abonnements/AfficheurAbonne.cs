using GestionOrdinateurs.Shared.classes;

namespace GestionOrdinateurs.API.Abonnements;

// Abonné : réagit aux événements en affichant des messages.
public class AfficheurAbonne
{
    public void AfficherMessageAjout(object? sender, OrdinateurAjouteEventArgs e)
    {
        Console.WriteLine("Evenement AfficherMessageAjout déclenché\n Ordinateur ajouté : " +
            $"ID={e.NouvelOrdinateur?.ID}, " +
            $"Stock={e.NouvelOrdinateur?.Stock}, " +
            $"Nom={e.NouvelOrdinateur?.Nom}, " +
            $"Etat={e.NouvelOrdinateur?.Etat}, " +
            $"Caracteristiques={e.NouvelOrdinateur?.Caracteristiques}, " +
            $"Prix={e.NouvelOrdinateur?.Prix}");
    }

    public void AfficherMessageSuppression(object? sender, OrdinateurSupprimeEventArgs e)
    {
        Console.WriteLine("Evenement AfficherMessageSuppression déclenché\n Ordinateur supprimé : " +
            $"ID={e.OrdinateurSupprime?.ID}, " +
            $"Stock={e.OrdinateurSupprime?.Stock}, " +
            $"Nom={e.OrdinateurSupprime?.Nom}, " +
            $"Etat={e.OrdinateurSupprime?.Etat}, " +
            $"Caracteristiques={e.OrdinateurSupprime?.Caracteristiques}" +
            $"Prix={e.OrdinateurSupprime?.Prix}");

    }

    public void AfficherMessageMiseAJour(object? sender, OrdinateurMisAJourEventArgs e)
    {
        Console.WriteLine("Evenement AfficherMessageMiseAJour déclenché\n Ordinateur mis à jour : " +
            $"ID={e.OrdinateurMisAJour?.ID}, " +
            $"Stock={e.OrdinateurMisAJour?.Stock}, " +
            $"Nom={e.OrdinateurMisAJour?.Nom}, " +
            $"Etat={e.OrdinateurMisAJour?.Etat}, " +
            $"Caracteristiques={e.OrdinateurMisAJour?.Caracteristiques}" +
            $"Prix={e.OrdinateurMisAJour?.Prix}");
    }
}