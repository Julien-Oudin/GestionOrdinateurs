

using Microsoft.AspNetCore.Mvc;

using GestionOrdinateurs.API.Services.Interfaces;
using GestionOrdinateurs.Shared.classes;
using Microsoft.AspNetCore.Authorization;
namespace GestionOrdinateurs.API.Controllers;


[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdinateursController : ControllerBase
{
    private readonly IOrdinateurService _service;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrdinateursController> _logger;

    public OrdinateursController(IOrdinateurService service, ILogger<OrdinateursController> logger, IConfiguration configuration)
    {
        _service = service;
        _logger = logger;
        _configuration = configuration;
    }

    // GET: api/Ordinateurs
    [HttpGet]
    public async Task<ActionResult<List<Ordinateur>>> GetAll()
    {
        return Ok(await _service.ListerOrdinateurs());
    }

    // GET: api/Ordinateurs/inventaire/{nom}
    [HttpGet("inventaire/{nom}")]
    public async Task<IActionResult> ChercherDansInventaire(string nom)
    {
        var inventaire = new InventaireOrdinateurs
        {
            Ordinateurs = await _service.ListerOrdinateurs()
        };
        var ordinateur = inventaire[nom];

        if (ordinateur is null)
            return NotFound();

        return Ok(ordinateur);
    }

    // GET: api/Ordinateurs/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Ordinateur>> GetById(int id)
    {
        var ordinateur = await _service.ObtenirParId(id);
        if (ordinateur is null)
        {
            // Permet d'avoir le 404 Not Found si l'ordinateur n'existe pas
            return NotFound();
        }
        
        // Retour 200 OK avec l'ordinateur trouvé
        _logger.LogInformation($"Ordinateur avec ID {id} trouvé : {ordinateur.Nom}");
        return Ok(ordinateur);
    }

    // POST: api/Ordinateurs
    [HttpPost]
    [Authorize(Roles = "Admin")]
    // Ajoute plusieurs ordinateurs et retourne le lot créé.
    public async Task<ActionResult<List<Ordinateur>>> Create([FromBody] List<Ordinateur> ordinateurs)
    {
        if (ordinateurs.Count == 0)
            return BadRequest("La liste des ordinateurs ne peut pas être vide.");

        await _service.AjouterOrdinateurs(ordinateurs);
        _logger.LogInformation("{Count} ordinateurs ajoutés.", ordinateurs.Count);
        return CreatedAtAction(nameof(GetAll), null, ordinateurs);
    }

    // PUT: api/Ordinateurs/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    //Vérifie si l'ordinateur existe, si oui, il le met à jour avec les nouvelles informations. Sinon, il retourne 404 Not Found.
    public async Task<IActionResult> Update(int id, [FromBody] Ordinateur ordinateur)
    {
        var existe = await _service.MettreAJourOrdinateur(id, ordinateur);
        if (!existe)
        {
            _logger.LogWarning($"Ordinateur avec ID {id} non trouvé pour mise à jour.");
            return NotFound();
        }
        _logger.LogInformation($"Ordinateur avec ID {id} mis à jour : {ordinateur.Nom}");
        return Ok();
    }

    // DELETE: api/Ordinateurs/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    //Vérifie si l'ordinateur existe, si oui, il le supprime. Sinon, il retourne 404 Not Found.
    public async Task<IActionResult> Delete(int id)
    {
        var estSupprime = await _service.SupprimerOrdinateur(id);
        if (!estSupprime)
        {
            _logger.LogWarning($"Ordinateur avec ID {id} non trouvé pour suppression.");
            return NotFound();
        }

        _logger.LogInformation($"Ordinateur avec ID {id} supprimé.");
        return NoContent();
    }

    // GET: api/Ordinateurs/marque/{marque}
    [HttpGet("marque/{marque}")]
    public async Task<ActionResult<List<Ordinateur>>> GetByMarque(string marque)
    {
        var ordinateurs = await _service.ChercherParMarque(marque);
        if (ordinateurs is null)
        {
            // Renvoie 204 car body de renvoi vide
            _logger.LogWarning($"Ordinateurs avec marque {marque} non trouvés.");
            return NoContent();
        }
        _logger.LogInformation($"Ordinateurs avec marque {marque} trouvés : {ordinateurs.Count}");
        return Ok(ordinateurs);
    }

    // GET: api/prixDecroissant
    [HttpGet("prixDecroissant")]
    public async Task<ActionResult<List<Ordinateur>>> GetByPrixDecroissant()
    {
        var ordinateurs = await _service.RenvoyerParPrixDecroissant();
        return Ok(ordinateurs);
    }


    // GET: api/ordinateurs/info
    // Pas besoin de paser cette fonction en asynchrone car elle ne fait pas d'appel à la base de données
    [HttpGet("/api/ordinateurs/info")]
    public ActionResult<string> GetApplicationInfo()
    {
        return Ok($"Application: {_configuration["Application:Name"]}, Version: {_configuration["Application:Version"]}");
    }

    // GET: api/ordinateurs/disponibles
    // Récupère tous les ordinateurs disponibles (stock > 0). Retourne une liste vide si aucun ordinateur n'est trouvé.
    [HttpGet("/api/ordinateurs/disponibles")]
    public async Task<ActionResult<List<Ordinateur>>> GetDisponibles()
    {
        var ordinateurs = await _service.OrdinateursDisponibles();
        return Ok(ordinateurs);
    }

    // GET: api/ordinateurs/{id}/diagnostic
    [HttpGet("/api/ordinateurs/{id}/diagnostic")]
    public async Task<ActionResult<string>> GetDiagnostic(int id)
    {
        var ordinateur = await _service.ObtenirParId(id);
        if (ordinateur is null)
        {
            return NotFound();
        }
        return Ok(await _service.Diagnostiquer(ordinateur));
    }

    // GET: api/ordinateurs/statistiques
    [HttpGet("/api/ordinateurs/statistiques")]
    public async Task<ActionResult<object>> GetStatistiques()
    {
        var statistiques = await _service.CalculerStatistiquesAsync();
        // Le new permet d'obtenir un retour json, sans quoi nous avons un retour vide
        return Ok(new
        {
            statistiques.TotalOrdinateur,
            statistiques.OrdiDisponibles,
            statistiques.ValeurStock,
            statistiques.NbReparations
        });
    }

}
