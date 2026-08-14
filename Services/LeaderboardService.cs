using MarbleServer.Data;
using MarbleServer.DTOs.Responses;
using MarbleServer.Models;
using MarbleServer.Models.Leaderboard;
using Microsoft.EntityFrameworkCore;

namespace MarbleServer.Services
{
    public class LeaderboardService
    {
        private readonly MarbleDbContext _db;
        private readonly RatingReferenceData _referenceData;
        private readonly RatingService _ratingService;

        public LeaderboardService(
            MarbleDbContext db,
            RatingReferenceData referenceData,
            RatingService ratingService)
        {
            _db = db;
            _referenceData = referenceData;
            _ratingService = ratingService;
        }

        // =========================================================
        // LEADERBOARD
        // =========================================================

        public async Task<LeaderboardResponse> GetLeaderboardAsync(
            string level,
            int page,
            int pageSize)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 10;

            List<Score> scores =
                await _db.Scores
                    .Include(s => s.Player)
                    .Where(s => s.Level == level)
                    .OrderBy(s => s.TimeMs)
                    .ToListAsync();

            int totalCount =
                scores.Count;

            int totalPages =
                (int)Math.Ceiling(
                    totalCount / (double)pageSize);

            int skip =
                (page - 1) * pageSize;

            scores = scores
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            List<ScoreResponse> result =
                new();

            for (int i = 0;
                 i < scores.Count;
                 i++)
            {
                Score score =
                    scores[i];

                int rating =
                    _ratingService.CalculateRating(
                        score.Level,
                        score.TimeMs);

                result.Add(
                    new ScoreResponse
                    {
                        ScoreId =
                            score.Id,

                        Rank =
                            skip + i + 1,

                        PlayerName =
                            score.Player.Username,

                        TimeMs =
                            score.TimeMs,

                        Rating =
                            rating
                    });
            }

            return new LeaderboardResponse
            {
                Page =
                    page,

                PageSize =
                    pageSize,

                TotalCount =
                    totalCount,

                TotalPages =
                    totalPages,

                Scores =
                    result
            };
        }

        // =========================================================
        // MY RANK
        // =========================================================

        public async Task<MyRankResponse?> GetMyRankAsync(
            int playerId,
            string level)
        {
            Console.WriteLine(
                $"GetMyRankAsync: PlayerId={playerId}, " +
                $"Level='{level}'");

            // DEBUG: Get ALL scores belonging to this player.
            List<Score> playerScores =
                await _db.Scores
                    .Include(s => s.Player)
                    .Where(s => s.PlayerId == playerId)
                    .OrderBy(s => s.Level)
                    .ThenBy(s => s.TimeMs)
                    .ToListAsync();

            Console.WriteLine(
                $"Total scores for PlayerId={playerId}: " +
                $"{playerScores.Count}");

            foreach (Score score in playerScores)
            {
                Console.WriteLine(
                    $"ScoreId={score.Id}, " +
                    $"PlayerId={score.PlayerId}, " +
                    $"Username={score.Player?.Username}, " +
                    $"Level='{score.Level}', " +
                    $"TimeMs={score.TimeMs}");
            }

            // Actual requested leaderboard.
            List<Score> scores =
                await _db.Scores
                    .Include(s => s.Player)
                    .Where(s => s.Level == level)
                    .OrderBy(s => s.TimeMs)
                    .ToListAsync();

            Console.WriteLine(
                $"Scores found for requested level: " +
                $"{scores.Count}");

            foreach (Score score in scores)
            {
                Console.WriteLine(
                    $"Leaderboard Score: " +
                    $"ScoreId={score.Id}, " +
                    $"PlayerId={score.PlayerId}, " +
                    $"Username={score.Player?.Username}, " +
                    $"Level='{score.Level}', " +
                    $"TimeMs={score.TimeMs}");
            }

            int index =
                scores.FindIndex(
                    s => s.PlayerId == playerId);

            Console.WriteLine(
                $"Matching PlayerId index: {index}");

            if (index < 0)
                return null;

            Score playerScore =
                scores[index];

            return new MyRankResponse
            {
                Rank =
                    index + 1,

                PlayerName =
                    playerScore.Player.Username,

                TimeMs =
                    playerScore.TimeMs,

                TotalPlayers =
                    scores.Count
            };
        }

        // =========================================================
        // ALL LEVEL RECORDS
        //
        // Used by the online level-select screen.
        //
        // Returns the player's personal record and the current
        // global record for every level in Scores.
        // =========================================================

        public async Task<List<LevelRecordResponse>>
            GetAllLevelRecordsAsync(
                int playerId)
        {
            // -----------------------------------------------------
            // Load all scores once.
            // -----------------------------------------------------

            List<Score> scores =
                await _db.Scores
                    .AsNoTracking()
                    .Include(s => s.Player)
                    .OrderBy(s => s.Level)
                    .ThenBy(s => s.TimeMs)
                    .ToListAsync();

            // -----------------------------------------------------
            // Load all stored mission ratings once.
            // -----------------------------------------------------

            List<UserMissionRating> ratings =
                await _db.UserMissionRatings
                    .AsNoTracking()
                    .ToListAsync();

            // -----------------------------------------------------
            // Build a fast lookup:
            //
            // (PlayerId, MissionId) -> Rating
            // -----------------------------------------------------

            Dictionary<
                (int PlayerId, int MissionId),
                int>
                ratingLookup =
                    ratings.ToDictionary(
                        r => (
                            r.PlayerId,
                            r.MissionId),
                        r => r.Rating);

            List<LevelRecordResponse> result =
                new();

            // -----------------------------------------------------
            // Process each level.
            // -----------------------------------------------------

            foreach (IGrouping<string, Score> levelScores
                in scores.GroupBy(s => s.Level))
            {
                // -------------------------------------------------
                // Global record
                // -------------------------------------------------

                Score globalScore =
                    levelScores
                        .OrderBy(s => s.TimeMs)
                        .First();

                // -------------------------------------------------
                // Personal record
                // -------------------------------------------------

                Score? personalScore =
                    levelScores
                        .FirstOrDefault(
                            s =>
                                s.PlayerId ==
                                playerId);

                // -------------------------------------------------
                // Global rating
                // -------------------------------------------------

                int? globalRating =
                    null;

                if (_referenceData.TryGetMission(
                        globalScore.Level,
                        out RatingMissionData? missionData) &&
                    missionData != null)
                {
                    if (ratingLookup.TryGetValue(
                            (
                                globalScore.PlayerId,
                                missionData.Mission.Id),
                            out int rating))
                    {
                        globalRating =
                            rating;
                    }
                }

                // -------------------------------------------------
                // Personal rating
                // -------------------------------------------------

                int? personalRating =
                    null;

                if (personalScore != null &&
                    _referenceData.TryGetMission(
                        personalScore.Level,
                        out RatingMissionData?
                            personalMissionData) &&
                    personalMissionData != null)
                {
                    if (ratingLookup.TryGetValue(
                            (
                                personalScore.PlayerId,
                                personalMissionData.Mission.Id),
                            out int rating))
                    {
                        personalRating =
                            rating;
                    }
                }

                // -------------------------------------------------
                // Result
                // -------------------------------------------------

                result.Add(
                    new LevelRecordResponse
                    {
                        Level =
                            levelScores.Key,

                        HasPersonalRecord =
                            personalScore != null,

                        PersonalTimeMs =
                            personalScore?.TimeMs,

                        PersonalRating =
                            personalRating,

                        HasGlobalRecord =
                            true,

                        GlobalPlayerName =
                            globalScore.Player.Username,

                        GlobalTimeMs =
                            globalScore.TimeMs,

                        GlobalRating =
                            globalRating
                    });
            }

            return result;
        }
    }
}