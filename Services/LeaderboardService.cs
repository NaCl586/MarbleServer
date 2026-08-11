using MarbleServer.Data;
using MarbleServer.DTOs.Responses;
using MarbleServer.Models;
using Microsoft.EntityFrameworkCore;

namespace MarbleServer.Services
{
    public class LeaderboardService
    {
        private readonly MarbleDbContext _db;

        public LeaderboardService(
            MarbleDbContext db)
        {
            _db = db;
        }

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

                result.Add(
                    new ScoreResponse
                    {
                        ScoreId = score.Id,

                        Rank =
                            skip + i + 1,

                        PlayerName =
                            score.Player.Username,

                        TimeMs =
                            score.TimeMs
                    });
            }

            return new LeaderboardResponse
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Scores = result
            };
        }

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
                Rank = index + 1,
                PlayerName = playerScore.Player.Username,
                TimeMs = playerScore.TimeMs,
                TotalPlayers = scores.Count
            };
        }
    }
}