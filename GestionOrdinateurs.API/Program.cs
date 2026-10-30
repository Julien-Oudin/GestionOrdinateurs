using GestionOrdinateurs.API;
using GestionOrdinateurs.Shared.classes;
using GestionOrdinateurs.Shared.enums;
using GestionOrdinateurs.API.Services.Implementations;
using GestionOrdinateurs.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestionOrdinateurs",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez : Bearer {token}"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
var jwtKey = builder.Configuration["Jwt:Key"] ?? "";
if (Encoding.ASCII.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("La configuration Jwt:Key doit contenir au moins 32 octets.");
}

var key = Encoding.ASCII.GetBytes(jwtKey);
var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"]
    ?? throw new InvalidOperationException("La configuration Keycloak:BaseUrl est manquante.");
var keycloakRealm = builder.Configuration["Keycloak:Realm"]
    ?? throw new InvalidOperationException("La configuration Keycloak:Realm est manquante.");
var keycloakIssuer = builder.Configuration["Keycloak:Issuer"]
    ?? throw new InvalidOperationException("La configuration Keycloak:Issuer est manquante.");
var keycloakAudience = builder.Configuration["Keycloak:Audience"] ?? "account";
var keycloakAuthority = $"{keycloakBaseUrl.TrimEnd('/')}/realms/{Uri.EscapeDataString(keycloakRealm)}";
var publicKeycloakIssuer = new Uri(keycloakIssuer);
var internalKeycloakAddress = new Uri(keycloakBaseUrl);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddPolicyScheme(JwtBearerDefaults.AuthenticationScheme, "Keycloak and legacy JWT", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authorization = context.Request.Headers.Authorization.ToString();
        var token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..].Trim()
            : string.Empty;

        return new JwtSecurityTokenHandler().CanReadToken(token)
            && new JwtSecurityTokenHandler().ReadJwtToken(token).Issuer == keycloakIssuer
                ? "Keycloak"
                : "LegacyBearer";
    };
})
.AddJwtBearer("LegacyBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
})
.AddJwtBearer("Keycloak", options =>
{
    options.Authority = keycloakAuthority;
    options.MetadataAddress = $"{keycloakAuthority}/.well-known/openid-configuration";
    options.RequireHttpsMetadata = false;
    options.BackchannelHttpHandler = new SocketsHttpHandler
    {
        ConnectCallback = async (context, cancellationToken) =>
        {
            var endpoint = context.DnsEndPoint;
            // Keycloak publishes its host-visible issuer; route those backchannel requests through Compose DNS.
            EndPoint target = string.Equals(endpoint.Host, publicKeycloakIssuer.Host, StringComparison.OrdinalIgnoreCase)
                && endpoint.Port == publicKeycloakIssuer.Port
                    ? new DnsEndPoint(internalKeycloakAddress.Host, internalKeycloakAddress.Port)
                    : endpoint;
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(target, cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = keycloakIssuer,
        ValidateAudience = true,
        ValidAudience = keycloakAudience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        NameClaimType = "preferred_username",
        RoleClaimType = ClaimTypes.Role
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("KeycloakJwt");
            logger.LogError(context.Exception, "Keycloak access token validation failed.");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
            if (realmAccess is null || context.Principal?.Identity is not ClaimsIdentity identity)
            {
                return Task.CompletedTask;
            }

            try
            {
                using var document = JsonDocument.Parse(realmAccess);
                if (document.RootElement.TryGetProperty("roles", out var roles)
                    && roles.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        if (role.ValueKind == JsonValueKind.String
                            && !string.IsNullOrWhiteSpace(role.GetString()))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                        }
                    }
                }
            }
            catch (JsonException exception)
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("KeycloakJwt");
                logger.LogWarning(exception, "Keycloak returned an invalid realm role claim.");
                context.Fail("The Keycloak role claim is invalid.");
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddScoped<IOrdinateurService, OrdinateurService>();
builder.Services.AddScoped<IReparationService, ReparationService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddDbContext<ParcInformatiqueContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("ParcInformatique")
        ?? "Data Source=parcinfo.db"));
builder.Services.AddSingleton<StockService>();
builder.Services.AddSingleton(serviceProvider =>
    new JournalService(serviceProvider.GetRequiredService<IWebHostEnvironment>().ContentRootPath));

var app = builder.Build();

//Initialisation de la base de données
using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParcInformatiqueContext>();
    
    db.Database.Migrate();

    if(!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Login="user1", Role=RoleEnum.Admin },
            new User { Login="user2", Role=RoleEnum.Gestionnaire },
            new User { Login="user3", Role=RoleEnum.Gestionnaire }
        );

        db.SaveChanges();
    }
}

// Middleware


app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
