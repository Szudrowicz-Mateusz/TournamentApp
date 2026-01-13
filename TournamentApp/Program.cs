using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HotChocolate.Data; // Wa¿ne dla wersji 15
using System.Text;
using TournamentApp.Data;
using TournamentApp.GraphQL;

var builder = WebApplication.CreateBuilder(args);

// 1. ZMIANA TUTAJ: U¿ywamy AddPooledDbContextFactory zamiast AddDbContext
// To jest wymagane, aby .RegisterDbContextFactory na dole dzia³a³o poprawnie
builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
    options.UseSqlite("Data Source=tournament.db"));

// 2. Konfiguracja JWT
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey1234567890_VerySecureKeyNeededHere"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "TournamentApp",
            ValidAudience = "TournamentApp",
            IssuerSigningKey = signingKey
        };
    });

builder.Services.AddAuthorization();

// 3. Konfiguracja GraphQL
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .RegisterDbContextFactory<AppDbContext>(); // To pasuje do AddPooledDbContextFactory wy¿ej

var app = builder.Build();

// Automatyczna migracja bazy danych (Przy u¿yciu Factory trzeba pobraæ fabrykê najpierw)
using (var scope = app.Services.CreateScope())
{
    // Pobieramy fabrykê, tworzymy kontekst, tworzymy bazê
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL(); // To sprawia, ¿e API dzia³a pod /graphql

app.Run();