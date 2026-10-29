namespace GestionOrdinateurs.API.Services.Implementations;

public class JournalService
{
    private readonly string _dossierRacine;

    public JournalService(string dossierRacine)
    {
        _dossierRacine = dossierRacine;
    }

    // Ajoute une entrée dans le journal des réparations.
    public void Ecrire(string message)
    {
        EcrireDansFichier("Reparations", "reparations.log", message);
    }

    // Ajoute une entrée dans le journal des ordinateurs.
    public void EcrireOrdinateur(string message)
    {
        EcrireDansFichier("Ordinateurs", "ordinateurs.log", message);
    }

    private void EcrireDansFichier(string dossier, string nomFichier, string message)
    {
        var cheminDossier = Path.Combine(_dossierRacine, "Logs", dossier);
        Directory.CreateDirectory(cheminDossier);

        using var writer = new StreamWriter(
            Path.Combine(cheminDossier, nomFichier),
            append: true);
        writer.WriteLine($"{DateTime.Now:dd/MM/yyyy} - {message}");
    }
}
