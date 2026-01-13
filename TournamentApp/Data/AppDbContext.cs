using Microsoft.EntityFrameworkCore;
using TournamentApp.Models;

namespace TournamentApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Bracket> Brackets { get; set; }
        public DbSet<Match> Matches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfiguracja relacji User <-> Tournament (wiele do wielu)
            modelBuilder.Entity<Tournament>()
                .HasMany(t => t.Participants)
                .WithMany(u => u.ParticipatedTournaments);
        }
    }
}