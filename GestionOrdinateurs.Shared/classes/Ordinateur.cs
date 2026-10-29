using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GestionOrdinateurs.Shared.enums;

namespace GestionOrdinateurs.Shared.classes;


public class Ordinateur
{
    [Key]
    public int ID { get; init; }
    public string Nom { get; set; }
    public EtatOrdinateur Etat { get; set; }
    [DefaultValue(0)]
    public decimal Prix { get; set; }
    [DefaultValue(0)]

    public int Stock { get; set; }
    public bool Vendu { get; set; } = false;
    public List<Reparation> Reparations { get; set; } = new();
    public Caracteristiques Caracteristiques;


    // Constructeur par défaut pour Entity Framework
    public Ordinateur()
    {
        ID = 0;
        Nom = string.Empty;
        Etat = EtatOrdinateur.HorsService;
        Prix = 500;
        Stock = 1;
        Caracteristiques = new Caracteristiques();
        // Pas de raison d'avoir des réparations déjà existantes quand on créé un ordinateur
        Reparations = new List<Reparation>();
    }

    public Ordinateur(int id, string nom, string marque, int ram, string processeur, Caracteristiques caracteristiques, decimal prix, int stock = 1)
    {
        ID = id;
        Nom = nom;
        Etat = EtatOrdinateur.HorsService;
        Prix = prix;
        Stock = stock;
        Caracteristiques = caracteristiques;
        // Pas de raison d'avoir des réparations déjà existantes quand on créé un ordinateur
        Reparations = new List<Reparation>();
    }

}
