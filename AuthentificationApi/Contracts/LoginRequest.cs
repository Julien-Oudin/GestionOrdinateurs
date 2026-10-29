using System.ComponentModel.DataAnnotations;

namespace AuthentificationApi.Contracts;

/// <summary>Identifiant de connexion transmis pour obtenir un jeton JWT.</summary>
public sealed record LoginRequest
{
    [Required]
    public required string Login { get; init; }
}
