using MarbleServer.Data;
using MarbleServer.Models.Leaderboard;
using Microsoft.EntityFrameworkCore;

namespace MarbleServer.Services
{
    public class RatingReferenceData
    {
        private readonly IDbContextFactory<LeaderboardDbContext>
            _dbFactory;

        private readonly Dictionary<string, RatingMissionData>
            _missions = new(
                StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<int, RatingMissionData>
            _missionsById = new();

        private readonly Dictionary<int, int>
            _achievementRatings = new();

        private bool _initialized;

        public RatingReferenceData(
            IDbContextFactory<LeaderboardDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            await using LeaderboardDbContext db =
                await _dbFactory.CreateDbContextAsync();

            // =====================================================
            // MISSIONS
            // =====================================================

            List<Mission> missions =
                await db.Missions
                    .AsNoTracking()
                    .ToListAsync();

            List<MissionGame> games =
                await db.MissionGames
                    .AsNoTracking()
                    .ToListAsync();

            List<MissionRatingInfo> ratingInfos =
                await db.MissionRatingInfo
                    .AsNoTracking()
                    .ToListAsync();

            Dictionary<int, MissionGame> gameById =
                games.ToDictionary(g => g.Id);

            Dictionary<int, MissionRatingInfo>
                ratingByMissionId =
                    ratingInfos.ToDictionary(
                        r => r.MissionId);

            foreach (Mission mission in missions)
            {
                if (mission.File == null)
                    continue;

                if (!gameById.TryGetValue(
                        mission.GameId,
                        out MissionGame? game))
                {
                    continue;
                }

                if (!ratingByMissionId.TryGetValue(
                        mission.Id,
                        out MissionRatingInfo? ratingInfo))
                {
                    continue;
                }

                if (!IsSupportedGame(
                        mission,
                        game))
                {
                    continue;
                }

                string key =
                    NormalizeLevel(
                        mission.File);

                RatingMissionData data =
                    new RatingMissionData
                    {
                        Mission = mission,
                        Game = game,
                        RatingInfo = ratingInfo
                    };

                _missions[key] = data;
                _missionsById[mission.Id] = data;
            }

            // =====================================================
            // ACHIEVEMENTS
            // =====================================================

            List<AchievementName> achievements =
                await db.AchievementNames
                    .AsNoTracking()
                    .ToListAsync();

            foreach (AchievementName achievement
                in achievements)
            {
                _achievementRatings[
                    achievement.Id] =
                    achievement.Rating ?? 0;
            }

            _initialized = true;
        }

        // =========================================================
        // MISSION LOOKUP
        // =========================================================

        public bool TryGetMission(
            string level,
            out RatingMissionData? data)
        {
            string key =
                NormalizeLevel(level);

            return _missions.TryGetValue(
                key,
                out data);
        }

        public bool TryGetMissionById(
            int missionId,
            out RatingMissionData? data)
        {
            return _missionsById.TryGetValue(
                missionId,
                out data);
        }

        // =========================================================
        // ACHIEVEMENT RATING
        // =========================================================

        public Task<int> CalculateAchievementRatingAsync(
            IEnumerable<int> achievementIds)
        {
            int total = 0;

            foreach (int achievementId
                in achievementIds.Distinct())
            {
                if (_achievementRatings.TryGetValue(
                        achievementId,
                        out int rating))
                {
                    total += rating;
                }
            }

            return Task.FromResult(total);
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private static string NormalizeLevel(
            string level)
        {
            level = level.Trim();

            if (level.EndsWith(
                    ".mis",
                    StringComparison.OrdinalIgnoreCase))
            {
                level = level[..^4];
            }

            return level.Replace(
                '\\',
                '/');
        }

        private static bool IsSupportedGame(
            Mission mission,
            MissionGame game)
        {
            // Marble Blast Gold
            if (game.Id == 1)
                return true;

            // Marble Blast Platinum
            if (game.Id == 2)
                return true;

            // MBP Bonus / Official Custom
            if (game.Id == 5 &&
                mission.File != null &&
                mission.File.StartsWith(
                    "marble/data/missions/custom/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }

    public class RatingMissionData
    {
        public required Mission Mission { get; init; }

        public required MissionGame Game { get; init; }

        public required MissionRatingInfo RatingInfo { get; init; }
    }
}