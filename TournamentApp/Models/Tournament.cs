using System.ComponentModel.DataAnnotations;

namespace TournamentApp.Models
{
    public class Tournament
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; } = "Created"; // Created, Started, Finished

        public List<User> Participants { get; set; } = new();
        public Bracket? Bracket { get; set; }
    }
}