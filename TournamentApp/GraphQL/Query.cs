using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TournamentApp.Data;
using TournamentApp.Models;

namespace TournamentApp.GraphQL
{
    public class Query
    {
        // 1. Pobieranie wszystkich turniejów (dostępne dla wszystkich)
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Tournament> GetTournaments(AppDbContext context) => context.Tournaments;

        // 2. Pobieranie informacji o zalogowanym użytkowniku (WYMAGANIE: bez podawania ID)
        [Authorize]
        public async Task<User?> Me(ClaimsPrincipal claimsPrincipal, AppDbContext context)
        {
            var userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            return await context.Users.FindAsync(userId);
        }

        // 3. Pobieranie SWOICH meczów (WYMAGANIE: bez podawania ID)
        [Authorize]
        [UseProjection]
        [UseFiltering]
        public IQueryable<Match> GetMyMatches(ClaimsPrincipal claimsPrincipal, AppDbContext context)
        {
            var userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));

            // Zwraca mecze, gdzie użytkownik jest graczem 1 LUB graczem 2
            return context.Matches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.Bracket)
                .ThenInclude(b => b.Tournament)
                .Where(m => m.Player1Id == userId || m.Player2Id == userId);
        }
    }
}