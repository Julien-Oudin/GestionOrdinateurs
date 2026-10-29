var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient("ClientGestionAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5040/api/");
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy.WithOrigins("http://localhost:5000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
