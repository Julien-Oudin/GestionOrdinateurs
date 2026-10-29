namespace GestionOrdinateurs.Shared.classes;

public class Reparation
{
    public int Id { get; set; }

    public int OrdinateurID { get; set; }

    public string TypeReparation { get; set; }

    public DateTime DateDebut { get; set; }

    public string Statut { get; set; }

    public string Technicien { get; set; }

    public Reparation(int id, string typeReparation, DateTime dateDebut, string statut, string technicien)
    {
        Id = id;
        TypeReparation = typeReparation;
        DateDebut = dateDebut;
        Statut = statut;
        Technicien = technicien;
    }
    public Reparation()
    {
        Id = 0;
        TypeReparation = string.Empty;
        DateDebut = DateTime.Now;
        Statut = "En cours";
        Technicien = string.Empty;
    }
}