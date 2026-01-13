using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HotChocolate.Data;
using System.Text;
using TournamentApp.Data;
using TournamentApp.GraphQL;

var builder = WebApplication.CreateBuilder(args);


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
    .RegisterDbContextFactory<AppDbContext>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

app.Run();