using MarbleServer.Data;
using MarbleServer.DTOs.Requests;
using MarbleServer.DTOs.Responses;
using MarbleServer.Exceptions;
using MarbleServer.Models;
using Microsoft.EntityFrameworkCore;

namespace MarbleServer.Services
{
    public class ScoreService
    {
        private const int MaxTimeMs = 5_999_999;

        private readonly MarbleDbContext _db;

        public ScoreService(MarbleDbContext db)
        {
            _db = db;
        }

        public async Task<SubmitScoreResponse> SubmitScoreAsync(
            int playerId,
            SubmitScoreRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Level))
            {
                throw new ValidationException("Level is required.");
            }

            if (request.TimeMs <= 0)
            {
                throw new ValidationException(
                    "Time must be greater than zero.");
            }

            if (request.TimeMs > MaxTimeMs)
            {
                throw new ValidationException(
                    "Time exceeds the maximum allowed value.");
            }

            bool isNewPersonalBest = false;

            Score? score = await _db.Scores
                .FirstOrDefaultAsync(s =>
                    s.PlayerId == playerId &&
                    s.Level == request.Level);

            if (score == null)
            {
                score = new Score
                {
                    PlayerId = playerId,
                    Level = request.Level,
                    TimeMs = request.TimeMs,
                    SubmittedAt = DateTime.UtcNow
                };

                _db.Scores.Add(score);

                isNewPersonalBest = true;
            }
            else if (request.TimeMs < score.TimeMs)
            {
                score.TimeMs = request.TimeMs;
                score.SubmittedAt = DateTime.UtcNow;

                isNewPersonalBest = true;
            }

            bool isWorldRecord = false;

            await _db.SaveChangesAsync();

            if (isNewPersonalBest)
            {
                int bestTime = await _db.Scores
                    .Where(s => s.Level == request.Level)
                    .MinAsync(s => s.TimeMs);

                isWorldRecord =
                    score.TimeMs == bestTime;
            }

            return new SubmitScoreResponse
            {
                ScoreId = score.Id,
                IsNewPersonalBest = isNewPersonalBest,
                TimeMs = score.TimeMs,
                IsWorldRecord = isWorldRecord
            };
        }
    }
}