using GestionOrdinateurs.Shared.classes;

namespace GestionOrdinateurs.API.Abonnements;

public class OrdinateurAjouteEventArgs : EventArgs
{
    public Ordinateur? NouvelOrdinateur { get;}

    public OrdinateurAjouteEventArgs(Ordinateur nouvelOrdinateur)
    {
        NouvelOrdinateur = nouvelOrdinateur;
    }


}