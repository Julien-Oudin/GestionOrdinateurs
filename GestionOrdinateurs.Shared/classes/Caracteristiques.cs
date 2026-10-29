using GestionOrdinateurs.Shared.enums;

namespace GestionOrdinateurs.Shared.classes;


public class Caracteristiques
{
    public int RamGo { get; set; }
    public TypeOrdinateur TypeOrdi { get; set; }
    public int StockageGo { get; set; }
    public string Marque { get; set; }
    public string Processeur { get; set; }


    public Caracteristiques(int ramGo, TypeOrdinateur typeOrdi, int stockageGo, string marque, string processeur)
    {
        RamGo = ramGo;
        TypeOrdi = typeOrdi;        
        StockageGo = stockageGo;
        Marque = marque;
        Processeur = processeur;
    }

    public Caracteristiques()
    {
        RamGo = 8;
        TypeOrdi = TypeOrdinateur.Portable;
        StockageGo = 512;
        Marque = "Lenovo";
        Processeur = "Intel i5";
    }
}