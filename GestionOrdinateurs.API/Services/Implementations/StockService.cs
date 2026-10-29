namespace GestionOrdinateurs.API.Services.Implementations;
using GestionOrdinateurs.API.Services.Interfaces;
using GestionOrdinateurs.API.Abonnements;


public class StockService : IStockService
{
    public int StockSSD { get; set; } = 10;

    public void ConsommerSSD()
    {
        // Question 17 : pb de concurrence : plusieurs threads essaient de consommer le stock en même temps.
        // On verrouille pour éviter les problèmes de concurrence si plusieurs threads essaient de consommer le stock en même temps.
        // Aucun problème lors de la première exécution, mais lors de la deuxième exécution, le stock est déjà à 0 et on ne peut pas consommer de SSD.
        lock (this)
        {
            if (StockSSD > 0)
            {
                StockSSD--;
            }
            else
            {
                throw new InvalidOperationException("Stock de SSD épuisé.");
            }
        }
    }
}