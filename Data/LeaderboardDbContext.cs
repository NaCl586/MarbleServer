using MarbleServer.Models.Leaderboard;
using Microsoft.EntityFrameworkCore;

namespace MarbleServer.Data
{
    public class LeaderboardDbContext : DbContext
    {
        public LeaderboardDbContext(
            DbContextOptions<LeaderboardDbContext> options)
            : base(options)
        {
        }

        public DbSet<Mission> Missions =>
            Set<Mission>();

        public DbSet<MissionRatingInfo> MissionRatingInfo =>
            Set<MissionRatingInfo>();

        public DbSet<MissionGame> MissionGames =>
            Set<MissionGame>();

        public DbSet<AchievementName> AchievementNames =>
            Set<AchievementName>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Mission>(entity =>
            {
                entity.ToTable("missions");

                entity.HasKey(m => m.Id);

                entity.Property(m => m.Id)
                    .HasColumnName("id");

                entity.Property(m => m.GameId)
                    .HasColumnName("game_id");

                entity.Property(m => m.File)
                    .HasColumnName("file");

                entity.Property(m => m.Basename)
                    .HasColumnName("basename");

                entity.Property(m => m.Name)
                    .HasColumnName("name");

                entity.Property(m => m.Gamemode)
                    .HasColumnName("gamemode");

                entity.Property(m => m.IsCustom)
                    .HasColumnName("is_custom");
            });

            modelBuilder.Entity<MissionRatingInfo>(entity =>
            {
                entity.ToTable("mission_rating_info");

                entity.HasKey(r => r.MissionId);

                entity.Property(r => r.MissionId)
                    .HasColumnName("mission_id");

                entity.Property(r => r.ParTime)
                    .HasColumnName("par_time");

                entity.Property(r => r.PlatinumTime)
                    .HasColumnName("platinum_time");

                entity.Property(r => r.UltimateTime)
                    .HasColumnName("ultimate_time");

                entity.Property(r => r.CompletionBonus)
                    .HasColumnName("completion_bonus");

                entity.Property(r => r.SetBaseScore)
                    .HasColumnName("set_base_score");

                entity.Property(r => r.MultiplierSetBase)
                    .HasColumnName("multiplier_set_base");

                entity.Property(r => r.PlatinumBonus)
                    .HasColumnName("platinum_bonus");

                entity.Property(r => r.UltimateBonus)
                    .HasColumnName("ultimate_bonus");

                entity.Property(r => r.Standardiser)
                    .HasColumnName("standardiser");

                entity.Property(r => r.TimeOffset)
                    .HasColumnName("time_offset");

                entity.Property(r => r.Difficulty)
                    .HasColumnName("difficulty");

                entity.Property(r => r.PlatinumDifficulty)
                    .HasColumnName("platinum_difficulty");

                entity.Property(r => r.UltimateDifficulty)
                    .HasColumnName("ultimate_difficulty");

                entity.Property(r => r.Disabled)
                    .HasColumnName("disabled");
            });

            modelBuilder.Entity<MissionGame>(entity =>
            {
                entity.ToTable("mission_games");

                entity.HasKey(g => g.Id);

                entity.Property(g => g.Id)
                    .HasColumnName("id");

                entity.Property(g => g.Name)
                    .HasColumnName("name");

                entity.Property(g => g.Display)
                    .HasColumnName("display");

                entity.Property(g => g.LongDisplay)
                    .HasColumnName("long_display");

                entity.Property(g => g.RatingColumn)
                    .HasColumnName("rating_column");

                entity.Property(g => g.GameType)
                    .HasColumnName("game_type");
            });

            modelBuilder.Entity<AchievementName>(entity =>
            {
                entity.ToTable("achievement_names");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.Id)
                    .HasColumnName("id");

                entity.Property(a => a.CategoryId)
                    .HasColumnName("category_id");

                entity.Property(a => a.Title)
                    .HasColumnName("title");

                entity.Property(a => a.Description)
                    .HasColumnName("description");

                entity.Property(a => a.Rating)
                    .HasColumnName("rating");
            });
        }
    }
}