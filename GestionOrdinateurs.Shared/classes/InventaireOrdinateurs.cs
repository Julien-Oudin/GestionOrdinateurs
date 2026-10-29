namespace GestionOrdinateurs.Shared.classes;

public class InventaireOrdinateurs
{
    public List<Ordinateur> Ordinateurs { get; set; } = new();

    // Recherche un ordinateur par son nom.
    public Ordinateur? this[string reference] =>
        Ordinateurs.FirstOrDefault(ordinateur =>
            ordinateur.Nom.Equals(reference, StringComparison.OrdinalIgnoreCase));

    // Récupère un ordinateur par sa position dans la liste.
    public Ordinateur this[int index] => Ordinateurs[index];
}
