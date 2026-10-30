using System.Text.Json;
using System.Text.Json.Serialization;

namespace MySecureApi
{
    public sealed class KeycloakAuthService(
     HttpClient httpClient,
     IConfiguration configuration)
    {
     public async Task<TokenResponse?> LoginAsync(
         string username,
         string password,
         CancellationToken cancellationToken)
     {
         var baseUrl = configuration["Keycloak:BaseUrl"]
             ?? throw new InvalidOperationException("La configuration Keycloak:BaseUrl est manquante.");
         var realm = configuration["Keycloak:Realm"]
             ?? throw new InvalidOperationException("La configuration Keycloak:Realm est manquante.");
         var clientId = configuration["Keycloak:ClientId"]
             ?? throw new InvalidOperationException("La configuration Keycloak:ClientId est manquante.");
         var clientSecret = configuration["Keycloak:ClientSecret"]
             ?? throw new InvalidOperationException("La configuration Keycloak:ClientSecret est manquante.");

         var tokenEndpoint = $"{baseUrl.TrimEnd('/')}/realms/{Uri.EscapeDataString(realm)}/protocol/openid-connect/token";
         using var content = new FormUrlEncodedContent(new Dictionary<string, string>
         {
             ["grant_type"] = "password",
             ["client_id"] = clientId,
             ["client_secret"] = clientSecret,
             ["username"] = username,
             ["password"] = password
         });

         using var response = await httpClient.PostAsync(tokenEndpoint, content, cancellationToken);
         var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
         if (!response.IsSuccessStatusCode)
         {
             if (response.StatusCode == System.Net.HttpStatusCode.BadRequest
                 && IsInvalidCredentials(responseContent))
             {
                 return null;
             }

             throw new HttpRequestException(
                 $"Keycloak returned HTTP {(int)response.StatusCode}.",
                 null,
                 response.StatusCode);
         }

         var tokenResult = JsonSerializer.Deserialize<TokenResponse>(responseContent);
         if (string.IsNullOrWhiteSpace(tokenResult?.AccessToken))
         {
             throw new JsonException("Keycloak returned a token response without an access token.");
         }

         return tokenResult;
     }

     private static bool IsInvalidCredentials(string responseContent)
     {
         using var document = JsonDocument.Parse(responseContent);
         return document.RootElement.TryGetProperty("error", out var error)
             && error.GetString() == "invalid_grant";
     }
    }

    public sealed record TokenResponse
    {
     [JsonPropertyName("access_token")]
     public required string AccessToken { get; init; }

     [JsonPropertyName("expires_in")]
     public int ExpiresIn { get; init; }

     [JsonPropertyName("token_type")]
     public string? TokenType { get; init; }
    }
}