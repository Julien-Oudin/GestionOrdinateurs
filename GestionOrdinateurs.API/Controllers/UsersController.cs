

using Microsoft.AspNetCore.Mvc;

using GestionOrdinateurs.API.Services.Interfaces;
using GestionOrdinateurs.Shared.classes;
namespace GestionOrdinateurs.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{

    private readonly IConfiguration _configuration;
    private readonly ILogger<OrdinateursController> _logger;
    private readonly IUsersService _service;

    public UsersController(IUsersService service, ILogger<OrdinateursController> logger, IConfiguration configuration)
    {
        _service = service;
        _logger = logger;
        _configuration = configuration;
    }

    // GET: api/Ordinateurs
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAll()
    {
        return Ok(_service.RecupererUsers());
    }

    [HttpGet("userbylogin/{login}")]
    public ActionResult<User> GetByLogin(string login)
    {
        var user = _service.RecupererUserParLogin(login);
        return user is null ? NotFound() : Ok(user);
    }

    


}
