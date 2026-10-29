
using Microsoft.AspNetCore.Mvc;
using GestionOrdinateurs.API.Records;
using GestionOrdinateurs.API.Services.Implementations;
using GestionOrdinateurs.API.Services.Interfaces;
using GestionOrdinateurs.Shared.classes;
using Microsoft.AspNetCore.Authorization;

namespace GestionOrdinateurs.API.Controllers;


[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReparationsController : ControllerBase
{
    private readonly IReparationService _reparationService;
    private readonly IOrdinateurService _ordinateurService;
    private StockService _stockService;
    // Nous n'utilisons pas la configuration pour l'instant, mais elle est injectée au cas où.
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReparationsController> _logger;


    public ReparationsController(IReparationService reparationService, IOrdinateurService ordinateurService, ILogger<ReparationsController> logger, IConfiguration configuration, StockService stockService)
    {
        _reparationService = reparationService;
        _ordinateurService = ordinateurService;
        _logger = logger;
        _configuration = configuration;
        //Initialisé à 10 par défaut
        _stockService = stockService;
        
    }

    // GET: api/SimulerReparations
    [HttpPost("SimulerReparations")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<Reparation>>> SimulerReparations()
    {
        _logger.LogInformation("Simulation de réparations pour les 3 premiers ordinateurs");
        var ordinateurs = await _ordinateurService.ListerOrdinateurs();

        // On vérifie qu'il y a au moins 3 ordinateurs pour simuler les réparations. Si ce n'est pas le cas, on retourne un message d'erreur.
        if (ordinateurs.Count < 3)
        {
            _logger.LogWarning("Il n'y a pas assez d'ordinateurs pour simuler les réparations.");
            return BadRequest("Il n'y a pas assez d'ordinateurs pour simuler les réparations.");
        }

        /* J'ai rencontré des problèmes avec l'instanciation en trois lignes car il y a une concurrence, j'ai donc décidé de faire une boucle pour simuler les réparations des trois premiers ordinateurs.
         Les tests ont été concluants en appellant l'API GET /api/Reparations pour vérifier mais ce n'est pas une executin parallèle, c'est une exécution séquentielle.
         J'ai donc laissé WhenAll avec le défaut
         Ci-dessous, j'ai laissé le code de la boucle qui fonctionne
            foreach (var ordinateur in ordinateurs.Take(3))
            {
                await _reparationService.SimulerReparationAsync(ordinateur);
            }

        */

        foreach (var ordinateur in ordinateurs.Take(3))
        {
            await _reparationService.SimulerReparationAsync(ordinateur);
        }

        return Ok();
    }

    // GET: api/Reparations
    [HttpGet]
    public async Task<ActionResult<List<Reparation>>> GetAll()
    {
        var reparations = await _reparationService.ListerReparations();
        return Ok(reparations);
    }

    [HttpGet("historique")]
    public async Task<ActionResult<List<HistoriqueReparationRecord>>> GetHistorique()
    {
        var historique = await _reparationService.ObtenirHistoriqueReparations();
        return Ok(historique);
    }

    [HttpGet("StockSSD")]
    public ActionResult<int> GetStockSSD()
    {
        
        Parallel.For(0, 10, i =>
        {
            Task.Delay(10).Wait();
             _stockService.ConsommerSSD();
    });
        return Ok(_stockService.StockSSD);
    }
}