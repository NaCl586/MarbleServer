using MarbleServer.Data;
using MarbleServer.DTOs.Responses;
using MarbleServer.Models;
using MarbleServer.Models.Leaderboard;
using Microsoft.EntityFrameworkCore;

namespace MarbleServer.Services
{
    public class GlobalRatingService
    {
        private readonly MarbleDbContext _db;
        private readonly RatingReferenceData _referenceData;
        private readonly RatingService _ratingService;

        public GlobalRatingService(
            MarbleDbContext db,
            RatingReferenceData referenceData,
            RatingService ratingService)
        {
            _db = db;
            _referenceData = referenceData;
            _ratingService = ratingService;
        }

        // =========================================================
        // GET ONE PLAYER'S RATING TOTALS
        // =========================================================

        public async Task<GlobalRatingData> GetPlayerRatingAsync(
            int playerId)
        {
            Dictionary<int, GlobalRatingData> totals =
                await BuildRatingTotalsAsync();

            if (!totals.TryGetValue(
                    playerId,
                    out GlobalRatingData? rating))
            {
                return new GlobalRatingData();
            }

            return rating;
        }

        // =========================================================
        // GET GLOBAL LEADERBOARD
        // =========================================================

        public async Task<GlobalRatingLeaderboardResponse>
        GetGlobalLeaderboardAsync(
            int page = 1,
            int pageSize = 10)
        {
            Dictionary<int, GlobalRatingData> totals =
                await BuildRatingTotalsAsync();

            List<Player> players =
                await _db.Players
                    .AsNoTracking()
                    .ToListAsync();

            List<GlobalRatingResponse> result =
                new();

            foreach (Player player in players)
            {
                if (!totals.TryGetValue(
                        player.Id,
                        out GlobalRatingData? rating))
                {
                    rating =
                        new GlobalRatingData();
                }

                result.Add(
                    new GlobalRatingResponse
                    {
                        PlayerId =
                            player.Id,

                        PlayerName =
                            player.Username,

                        MbgRating =
                            rating.MbgRating,

                        MbpRating =
                            rating.MbpRating,

                        MbpBonusRating =
                            rating.MbpBonusRating,

                        AchievementRating =
                            rating.AchievementRating,

                        GlobalRating =
                            rating.GlobalRating
                    });
            }

            // =========================================================
            // SORT GLOBAL RANKING
            // =========================================================

            List<GlobalRatingResponse> globalRanking =
                result
                    .OrderByDescending(r =>
                        r.GlobalRating)
                    .ThenBy(r =>
                        r.PlayerName)
                    .ToList();

            // =========================================================
            // ASSIGN GLOBAL RANK
            // =========================================================

            for (int i = 0;
                 i < globalRanking.Count;
                 i++)
            {
                globalRanking[i].GlobalRank =
                    i + 1;
            }

            // =========================================================
            // PAGINATION
            // =========================================================

            page = Math.Max(1, page);

            pageSize =
                Math.Clamp(
                    pageSize,
                    1,
                    100);

            int totalPlayers =
                globalRanking.Count;

            int totalPages =
                (int)Math.Ceiling(
                    totalPlayers /
                    (double)pageSize);

            List<GlobalRatingResponse> pagedPlayers =
                globalRanking
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

            return new GlobalRatingLeaderboardResponse
            {
                Players =
                    pagedPlayers,

                Page =
                    page,

                PageSize =
                    pageSize,

                TotalPlayers =
                    totalPlayers,

                TotalPages =
                    totalPages
            };
        }

        // =========================================================
        // GET GAME-SPECIFIC RATING LEADERBOARD
        // =========================================================
        //
        // game:
        //   "gold"     -> Marble Blast Gold
        //   "platinum" -> Marble Blast Platinum
        //   "custom"   -> MBP Bonus / Official Custom
        //
        // Pagination is performed AFTER calculating the rating
        // for every player.
        // =========================================================

        public async Task<GameRatingLeaderboardResponse>
            GetGameRatingLeaderboardAsync(
                string game,
                int page = 1,
                int pageSize = 10)
        {
            page =
                Math.Max(
                    1,
                    page);

            pageSize =
                Math.Clamp(
                    pageSize,
                    1,
                    100);

            // ---------------------------------------------------------
            // Determine which rating field to use.
            // ---------------------------------------------------------

            Func<GlobalRatingData, int> ratingSelector;

            string normalizedGame =
                game.Trim()
                    .ToLowerInvariant();

            switch (normalizedGame)
            {
                case "gold":
                    ratingSelector =
                        rating => rating.MbgRating;
                    break;

                case "platinum":
                    ratingSelector =
                        rating => rating.MbpRating;
                    break;

                case "custom":
                    ratingSelector =
                        rating => rating.MbpBonusRating;
                    break;

                default:
                    throw new ArgumentException(
                        $"Unknown rating game '{game}'.");
            }

            // ---------------------------------------------------------
            // Build all player totals.
            //
            // This already includes every player, including players
            // who only have achievement rating.
            // ---------------------------------------------------------

            Dictionary<int, GlobalRatingData> totals =
                await BuildRatingTotalsAsync();

            // ---------------------------------------------------------
            // Load players.
            // ---------------------------------------------------------

            List<Player> players =
                await _db.Players
                    .AsNoTracking()
                    .ToListAsync();

            // ---------------------------------------------------------
            // Build the game-specific leaderboard.
            // ---------------------------------------------------------

            List<GameRatingResponse> ranking =
                new();

            foreach (Player player in players)
            {
                if (!totals.TryGetValue(
                        player.Id,
                        out GlobalRatingData? rating))
                {
                    rating =
                        new GlobalRatingData();
                }

                ranking.Add(
                    new GameRatingResponse
                    {
                        PlayerId =
                            player.Id,

                        PlayerName =
                            player.Username,

                        Rating =
                            ratingSelector(rating)
                    });
            }

            // ---------------------------------------------------------
            // Sort.
            //
            // Higher rating first.
            // Username breaks ties consistently.
            // ---------------------------------------------------------

            ranking =
                ranking
                    .OrderByDescending(
                        player => player.Rating)
                    .ThenBy(
                        player => player.PlayerName)
                    .ToList();

            // ---------------------------------------------------------
            // Assign ranks.
            // ---------------------------------------------------------

            for (int i = 0;
                 i < ranking.Count;
                 i++)
            {
                ranking[i].Rank =
                    i + 1;
            }

            // ---------------------------------------------------------
            // Pagination.
            // ---------------------------------------------------------

            int totalPlayers =
                ranking.Count;

            int totalPages =
                (int)Math.Ceiling(
                    totalPlayers /
                    (double)pageSize);

            List<GameRatingResponse> playersPage =
                ranking
                    .Skip(
                        (page - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToList();

            return new GameRatingLeaderboardResponse
            {
                Players =
                    playersPage,

                Page =
                    page,

                PageSize =
                    pageSize,

                TotalPlayers =
                    totalPlayers,

                TotalPages =
                    totalPages
            };
        }

        // =========================================================
        // BUILD ALL PLAYER RATING TOTALS
        // =========================================================

        private async Task<Dictionary<int, GlobalRatingData>>
            BuildRatingTotalsAsync()
        {
            // -----------------------------------------------------
            // Start with EVERY player.
            //
            // This is important because a player can have:
            //
            //   0 MBG rating
            //   0 MBP rating
            //   0 MBP Bonus rating
            //   but still have Achievement Rating.
            // -----------------------------------------------------

            List<Player> players =
                await _db.Players
                    .AsNoTracking()
                    .ToListAsync();

            Dictionary<int, GlobalRatingData> totals =
                players.ToDictionary(
                    player => player.Id,
                    player => new GlobalRatingData
                    {
                        PlayerId =
                            player.Id,

                        AchievementRating =
                            player.AchievementRating
                    });

            // -----------------------------------------------------
            // Mission ratings
            // -----------------------------------------------------

            List<UserMissionRating> ratings =
                await _db.UserMissionRatings
                    .AsNoTracking()
                    .ToListAsync();

            foreach (UserMissionRating rating in ratings)
            {
                if (!_referenceData.TryGetMissionById(
                        rating.MissionId,
                        out RatingMissionData? missionData) ||
                    missionData == null)
                {
                    continue;
                }

                if (!totals.TryGetValue(
                        rating.PlayerId,
                        out GlobalRatingData? total))
                {
                    // This should normally not happen because
                    // totals was initialized from Players.
                    //
                    // Keep this as a safety fallback.
                    total =
                        new GlobalRatingData();

                    totals.Add(
                        rating.PlayerId,
                        total);
                }

                switch (missionData.Game.Id)
                {
                    // =================================================
                    // Marble Blast Gold
                    // =================================================

                    case 1:

                        total.MbgRating +=
                            rating.Rating;

                        break;

                    // =================================================
                    // Marble Blast Platinum
                    // =================================================

                    case 2:

                        total.MbpRating +=
                            rating.Rating;

                        break;

                    // =================================================
                    // MBP Bonus / Official Custom
                    // =================================================

                    case 5:

                        total.MbpBonusRating +=
                            rating.Rating;

                        break;
                }
            }

            // -----------------------------------------------------
            // GLOBAL RATING
            // -----------------------------------------------------

            foreach (GlobalRatingData total
                in totals.Values)
            {
                total.GlobalRating =
                    total.MbgRating +
                    total.MbpRating +
                    total.MbpBonusRating +
                    total.AchievementRating;
            }

            return totals;
        }

        // =========================================================
        // SYNC ACHIEVEMENT RATING
        // =========================================================
        //
        // Unity sends the IDs of achievements currently unlocked.
        //
        // The actual rating values are looked up on the server
        // from leaderboards.db through RatingReferenceData.
        //
        // The stored value can ONLY increase.
        // =========================================================

        public async Task<int> SyncAchievementsAsync(
            int playerId,
            IEnumerable<int> achievementIds)
        {
            Player? player =
                await _db.Players
                    .FirstOrDefaultAsync(
                        p => p.Id == playerId);

            if (player == null)
                throw new InvalidOperationException(
                    $"Player {playerId} was not found.");

            int achievementRating =
                await _referenceData
                    .CalculateAchievementRatingAsync(
                        achievementIds);

            if (achievementRating >
                player.AchievementRating)
            {
                player.AchievementRating =
                    achievementRating;

                await _db.SaveChangesAsync();
            }

            return player.AchievementRating;
        }

        // =========================================================
        // AUTOMATIC BACKFILL
        //
        // Recalculates UserMissionRatings from existing PB scores.
        //
        // Intended to be called once when the server starts.
        // =========================================================

        public async Task BackfillMissionRatingsAsync()
        {
            List<Score> scores =
                await _db.Scores
                    .AsNoTracking()
                    .ToListAsync();

            // -----------------------------------------------------
            // Load every existing rating once.
            // -----------------------------------------------------

            List<UserMissionRating> existingRatings =
                await _db.UserMissionRatings
                    .ToListAsync();

            Dictionary<
                (int PlayerId, int MissionId),
                UserMissionRating>
                ratingLookup =
                    existingRatings.ToDictionary(
                        r => (
                            r.PlayerId,
                            r.MissionId));

            int createdCount = 0;
            int updatedCount = 0;
            int skippedCount = 0;

            foreach (Score score in scores)
            {
                // -------------------------------------------------
                // Find the mission in leaderboards.db.
                // -------------------------------------------------

                if (!_referenceData.TryGetMission(
                        score.Level,
                        out RatingMissionData? missionData) ||
                    missionData == null)
                {
                    skippedCount++;
                    continue;
                }

                int rating =
                    _ratingService.CalculateRating(
                        score.Level,
                        score.TimeMs);

                // -------------------------------------------------
                // Invalid / bad rating.
                // -------------------------------------------------

                if (rating < 0)
                {
                    skippedCount++;
                    continue;
                }

                int missionId =
                    missionData.Mission.Id;

                var key =
                    (
                        score.PlayerId,
                        missionId
                    );

                // -------------------------------------------------
                // Create missing rating.
                // -------------------------------------------------

                if (!ratingLookup.TryGetValue(
                        key,
                        out UserMissionRating? existing))
                {
                    UserMissionRating newRating =
                        new UserMissionRating
                        {
                            PlayerId =
                                score.PlayerId,

                            MissionId =
                                missionId,

                            Rating =
                                rating
                        };

                    _db.UserMissionRatings.Add(
                        newRating);

                    ratingLookup.Add(
                        key,
                        newRating);

                    createdCount++;
                }
                else
                {
                    // -------------------------------------------------
                    // True recalculation.
                    //
                    // If rating parameters changed, update even if
                    // the new rating is lower.
                    // -------------------------------------------------

                    if (existing.Rating != rating)
                    {
                        existing.Rating =
                            rating;

                        updatedCount++;
                    }
                }
            }

            await _db.SaveChangesAsync();

            Console.WriteLine(
                "Rating backfill completed. " +
                $"Scores={scores.Count}, " +
                $"Created={createdCount}, " +
                $"Updated={updatedCount}, " +
                $"Skipped={skippedCount}");
        }

        public async Task<GlobalRatingResponse?>
    GetMyGlobalRatingAsync(
        int playerId)
        {
            Dictionary<int, GlobalRatingData> totals =
                await BuildRatingTotalsAsync();

            if (!totals.TryGetValue(
                    playerId,
                    out GlobalRatingData? rating))
            {
                return null;
            }

            Player? player =
                await _db.Players
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        p => p.Id == playerId);

            if (player == null)
                return null;

            List<GlobalRatingData> ranking =
                totals.Values
                    .OrderByDescending(
                        r => r.GlobalRating)
                    .ThenBy(
                        r => r.PlayerId)
                    .ToList();

            int rank = 0;

            for (int i = 0;
                 i < ranking.Count;
                 i++)
            {
                if (ranking[i].PlayerId == playerId)
                {
                    rank = i + 1;
                    break;
                }
            }

            return new GlobalRatingResponse
            {
                PlayerId = player.Id,
                PlayerName = player.Username,

                MbgRating =
                    rating.MbgRating,

                MbpRating =
                    rating.MbpRating,

                MbpBonusRating =
                    rating.MbpBonusRating,

                AchievementRating =
                    rating.AchievementRating,

                GlobalRating =
                    rating.GlobalRating,

                GlobalRank =
                    rank
            };
        }
    }
}