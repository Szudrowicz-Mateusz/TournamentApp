using System.ComponentModel.DataAnnotations;

namespace TournamentApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Dodane dla bezpieczeństwa

        // Relacje
        public List<Tournament> ParticipatedTournaments { get; set; } = new();
    }
}