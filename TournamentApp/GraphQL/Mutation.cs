using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TournamentApp.Data;
using TournamentApp.Models;

namespace TournamentApp.GraphQL
{
    public class Mutation
    {
        // --- AUTH ---

        public async Task<AuthPayload> Register(RegisterInput input, AppDbContext context)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(input.Password);
            var user = new User
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                Email = input.Email,
                PasswordHash = hashedPassword
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var token = GenerateToken(user);
            return new AuthPayload(token, user);
        }

        public async Task<AuthPayload> Login(LoginInput input, AppDbContext context)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == input.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
            {
                throw new GraphQLException("Invalid credentials");
            }

            var token = GenerateToken(user);
            return new AuthPayload(token, user);
        }

        private string GenerateToken(User user)
        {
            var key = Encoding.UTF8.GetBytes("SuperSecretKey1234567890_VerySecureKeyNeededHere");
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "TournamentApp",
                audience: "TournamentApp",
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // --- TOURNAMENT LOGIC ---

        [Authorize]
        public async Task<Tournament> CreateTournament(string name, string startDate, AppDbContext context)
        {
            
            if (!DateTime.TryParse(startDate, out var parsedDate))
            {
                throw new GraphQLException("Nieprawidłowy format daty. Użyj formatu RRRR-MM-DD");
            }

            var tournament = new Tournament
            {
                Name = name,
                StartDate = parsedDate
            };

            context.Tournaments.Add(tournament);
            await context.SaveChangesAsync();
            return tournament;
        }

        [Authorize]
        public async Task<Tournament> AddParticipant(int tournamentId, ClaimsPrincipal claimsPrincipal, AppDbContext context)
        {
            var userId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var tournament = await context.Tournaments.Include(t => t.Participants).FirstOrDefaultAsync(t => t.Id == tournamentId);
            var user = await context.Users.FindAsync(userId);

            if (tournament == null || user == null) throw new GraphQLException("Tournament or User not found");
            if (tournament.Status != "Created") throw new GraphQLException("Cannot join started tournament");

            tournament.Participants.Add(user);
            await context.SaveChangesAsync();
            return tournament;
        }

        [Authorize]
        public async Task<Bracket> StartTournament(int tournamentId, AppDbContext context)
        {
            var tournament = await context.Tournaments.Include(t => t.Participants).FirstOrDefaultAsync(t => t.Id == tournamentId);
            if (tournament == null) throw new GraphQLException("Tournament not found");
            if (tournament.Participants.Count < 2) throw new GraphQLException("Not enough participants");

            tournament.Status = "Started";

            // GENEROWANIE DRABINKI (Uproszczona logika: tylko 1 runda dla przykładu)
            var bracket = new Bracket { Tournament = tournament };
            var participants = tournament.Participants.OrderBy(x => Guid.NewGuid()).ToList();

            for (int i = 0; i < participants.Count; i += 2)
            {
                if (i + 1 < participants.Count)
                {
                    bracket.Matches.Add(new Match
                    {
                        Round = 1,
                        Player1 = participants[i],
                        Player2 = participants[i + 1]
                    });
                }
            }

            context.Brackets.Add(bracket);
            await context.SaveChangesAsync();
            return bracket;
        }

        [Authorize]
        public async Task<Match> PlayMatch(int matchId, int winnerId, AppDbContext context)
        {
            var match = await context.Matches.FindAsync(matchId);
            if (match == null) throw new GraphQLException("Match not found");

            if (match.Player1Id != winnerId && match.Player2Id != winnerId)
                throw new GraphQLException("Winner must be one of the players");

            match.WinnerId = winnerId;

            await context.SaveChangesAsync();
            return match;
        }
    }
}