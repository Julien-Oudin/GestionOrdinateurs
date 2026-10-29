using GestionOrdinateurs.Shared.classes;

namespace GestionOrdinateurs.API.Abonnements;

public class OrdinateurSupprimeEventArgs : EventArgs
{
    public Ordinateur? OrdinateurSupprime { get;}

    public OrdinateurSupprimeEventArgs(Ordinateur ordinateurSupprime)
    {
        OrdinateurSupprime = ordinateurSupprime;
    }
}