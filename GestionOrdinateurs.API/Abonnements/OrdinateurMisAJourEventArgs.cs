using GestionOrdinateurs.Shared.classes;

namespace GestionOrdinateurs.API.Abonnements;

public class OrdinateurMisAJourEventArgs : EventArgs
{
    public Ordinateur? OrdinateurMisAJour { get;}

    public OrdinateurMisAJourEventArgs(Ordinateur ordinateurMisAJour)
    {
        OrdinateurMisAJour = ordinateurMisAJour;
    }
}