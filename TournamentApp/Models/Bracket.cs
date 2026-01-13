using System.ComponentModel.DataAnnotations;

namespace TournamentApp.Models
{
    public class Bracket
    {
        [Key]
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public List<Match> Matches { get; set; } = new();
    }
}