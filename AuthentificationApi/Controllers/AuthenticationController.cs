using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthentificationApi.Contracts;
using GestionOrdinateurs.Shared.classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthenticationController(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    MySecureApi.KeycloakAuthService keycloakAuthService,
    ILogger<AuthenticationController> logger) : ControllerBase
{
    /// <summary>
    /// Échange les identifiants de l'utilisateur contre un jeton d'accès Keycloak.
    /// </summary>
    [HttpPost("keycloak/login")]
    [ProducesResponseType<MySecureApi.TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<MySecureApi.TokenResponse>> LoginWithKeycloak(
        [FromBody] KeycloakLoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await keycloakAuthService.LoginAsync(
                request.Username,
                request.Password,
                cancellationToken);

            return token is null ? Unauthorized() : Ok(token);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Keycloak could not complete the login request.");
            return StatusCode(StatusCodes.Status502BadGateway, "Le service Keycloak est indisponible.");
        }
        catch (System.Text.Json.JsonException exception)
        {
            logger.LogError(exception, "Keycloak returned an invalid token response.");
            return StatusCode(StatusCodes.Status502BadGateway, "Keycloak a renvoyé une réponse invalide.");
        }
    }

    /// <summary>
    /// Récupère l'utilisateur par son login et renvoie un JWT valable 15 minutes.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<string>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("ClientGestionAPI");
        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(
                $"users/userbylogin/{Uri.EscapeDataString(request.Login)}",
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "User API could not be reached during authentication.");
            return StatusCode(StatusCodes.Status502BadGateway, "Le service utilisateur est indisponible.");
        }

        using (response)
        {
            // Gestion des erreurs, faite par IA pour avoir un maximum de détails en retour
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("User API returned HTTP {StatusCode} during authentication.", response.StatusCode);
                return StatusCode(StatusCodes.Status502BadGateway, "Le service utilisateur est indisponible.");
            }

            User? user;
            try
            {
                user = await response.Content.ReadFromJsonAsync<User>(cancellationToken: cancellationToken);
            }
            catch (System.Text.Json.JsonException exception)
            {
                logger.LogError(exception, "User API returned invalid JSON during authentication.");
                return StatusCode(StatusCodes.Status502BadGateway, "Le service utilisateur a renvoyé une réponse invalide.");
            }

            if (user is null)
            {
                logger.LogError("User API returned an empty user response during authentication.");
                return StatusCode(StatusCodes.Status502BadGateway, "Le service utilisateur a renvoyé une réponse invalide.");
            }

            var jwtKey = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException("La configuration Jwt:Key doit contenir au moins 32 octets.");
            }

            return Ok(GenerateJwt(Encoding.UTF8.GetBytes(jwtKey), user));
        }
    }

    private static string GenerateJwt(byte[] key, User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            // Mets le token a 15min d'expiration
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}
