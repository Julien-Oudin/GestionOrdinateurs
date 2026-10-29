using GestionOrdinateurs.API;
using GestionOrdinateurs.Shared.classes;
using GestionOrdinateurs.Shared.enums;
using GestionOrdinateurs.API.Services.Implementations;
using GestionOrdinateurs.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Utilisateur utilisant un Token Valide --- {context.Principal?.Identity?.Name ?? "Anonymous"}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = _ =>
        {
            Console.WriteLine("Token Invalide");
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy.WithOrigins("http://localhost:5000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddScoped<IOrdinateurService, OrdinateurService>();
builder.Services.AddScoped<IReparationService, ReparationService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddDbContext<ParcInformatiqueContext>();
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

app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
