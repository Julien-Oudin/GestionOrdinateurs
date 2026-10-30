using System.ComponentModel.DataAnnotations;

namespace AuthentificationApi.Contracts;

/// <summary>Identifiants transmis à Keycloak pour obtenir un jeton d'accès.</summary>
public sealed record KeycloakLoginRequest
{
    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }
}
