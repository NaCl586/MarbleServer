using MarbleServer.Data;
using MarbleServer.DTOs.Requests;
using MarbleServer.DTOs.Responses;
using MarbleServer.Exceptions;
using MarbleServer.Models;
using MarbleServer.Models.Leaderboard;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MarbleServer.Services
{
    public class ScoreService
    {
        private const int MaxTimeMs = 5_999_999;

        private readonly MarbleDbContext _db;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly ChatMessageHistoryService _chatHistory;
        private readonly RatingService _ratingService;

        public ScoreService(
            MarbleDbContext db,
            IHubContext<ChatHub> chatHub,
            ChatMessageHistoryService chatHistory,
            RatingService ratingService)
        {
            _db = db;
            _chatHub = chatHub;
            _chatHistory = chatHistory;
            _ratingService = ratingService;
        }

        public async Task<SubmitScoreResponse> SubmitScoreAsync(
            int playerId,
            SubmitScoreRequest request)
        {
            var player = await _db.Players
                .FirstOrDefaultAsync(p => p.Id == playerId);

            if (player == null)
                throw new ValidationException(
                    "Player not found.");

            if (string.IsNullOrWhiteSpace(request.Level))
            {
                throw new ValidationException(
                    "Level is required.");
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

            Score? score =
                await _db.Scores
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

            // =====================================================
            // RATING
            // =====================================================

            int rating = 0;

            if (_ratingService.TryGetMission(
                request.Level,
                out RatingMissionData? missionData) &&
            missionData != null)
            {
                rating =
                    _ratingService.CalculateRating(
                        request.Level,
                        score.TimeMs);

                if (isNewPersonalBest &&
                    rating >= 0)
                {
                    UserMissionRating? userMissionRating =
                        await _db.UserMissionRatings
                            .FirstOrDefaultAsync(r =>
                                r.PlayerId == playerId &&
                                r.MissionId ==
                                    missionData.Mission.Id);

                    if (userMissionRating == null)
                    {
                        userMissionRating =
                            new UserMissionRating
                            {
                                PlayerId = playerId,
                                MissionId =
                                    missionData.Mission.Id,
                                Rating = rating
                            };

                        _db.UserMissionRatings.Add(
                            userMissionRating);
                    }
                    else if (rating >
                             userMissionRating.Rating)
                    {
                        userMissionRating.Rating = rating;
                    }
                }
            }

            // =====================================================
            // SAVE SCORE + RATING
            // =====================================================

            await _db.SaveChangesAsync();

            // =====================================================
            // WORLD RECORD
            // =====================================================

            bool isWorldRecord = false;

            if (isNewPersonalBest)
            {
                int bestTime =
                    await _db.Scores
                        .Where(s =>
                            s.Level == request.Level)
                        .MinAsync(s => s.TimeMs);

                isWorldRecord =
                    score.TimeMs == bestTime;

                if (isWorldRecord)
                {
                    string worldRecordMessage =
                        $"{player.Username} has just achieved " +
                        $"a world record on " +
                        $"\"{request.LevelName}\" of " +
                        $"{FormatTime(score.TimeMs)}";

                    _chatHistory.AddWorldRecordMessage(
                        worldRecordMessage);

                    await _chatHub.Clients.All.SendAsync(
                        "WorldRecord",
                        worldRecordMessage);
                }
            }

            // =====================================================
            // RESPONSE
            // =====================================================

            return new SubmitScoreResponse
            {
                ScoreId =
                    score.Id,

                IsNewPersonalBest =
                    isNewPersonalBest,

                TimeMs =
                    score.TimeMs,

                IsWorldRecord =
                    isWorldRecord,

                Rating =
                    rating
            };
        }

        private static string FormatTime(
            int timeMs)
        {
            int minutes =
                timeMs / 60000;

            int seconds =
                (timeMs % 60000) / 1000;

            int milliseconds =
                timeMs % 1000;

            return
                $"{minutes:00}:" +
                $"{seconds:00}." +
                $"{milliseconds:000}";
        }
    }
}