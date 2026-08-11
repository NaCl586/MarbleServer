using MarbleServer.Models;
using Microsoft.EntityFrameworkCore;

namespace MarbleServer.Data
{
    public class MarbleDbContext : DbContext
    {
        public MarbleDbContext(
            DbContextOptions<MarbleDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players => Set<Player>();
        public DbSet<Score> Scores => Set<Score>();
        public DbSet<Replay> Replays => Set<Replay>();

        protected override void OnModelCreating(
        ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Score>()
                .HasIndex(s => new
                {
                    s.PlayerId,
                    s.Level
                })
                .IsUnique();

            modelBuilder.Entity<Score>()
                .HasOne(s => s.Replay)
                .WithOne(r => r.Score)
                .HasForeignKey<Replay>(
                    r => r.ScoreId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}