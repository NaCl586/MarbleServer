using MarbleServer.DTOs.Requests;
using MarbleServer.DTOs.Responses;
using MarbleServer.Exceptions;
using MarbleServer.Models;
using MarbleServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarbleServer.Controllers
{
    [ApiController]
    [Route("api/ratings")]
    public class RatingController : ControllerBase
    {
        private readonly GlobalRatingService _globalRatingService;
        private readonly RatingService _ratingService;

        public RatingController(
            GlobalRatingService globalRatingService,
            RatingService ratingService)
        {
            _globalRatingService = globalRatingService;
            _ratingService = ratingService;
        }

        // =========================================================
        // MY GLOBAL RATING
        // =========================================================

        [Authorize]
        [HttpGet("me")]
        public async Task<
            ActionResult<ApiResponse<GlobalRatingResponse>>>
            GetMyRating()
        {
            string? playerIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                playerIdClaim,
                out int playerId))
            {
                return Unauthorized();
            }

            GlobalRatingResponse? result =
                await _globalRatingService
                    .GetMyGlobalRatingAsync(
                        playerId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<GlobalRatingResponse>.Success(
                    result));
        }

        // =========================================================
        // GLOBAL RATING LEADERBOARD
        // =========================================================

        [Authorize]
        [HttpGet("global")]
        public async Task<
            ActionResult<ApiResponse<GlobalRatingLeaderboardResponse>>>
            GetGlobalLeaderboard(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            GlobalRatingLeaderboardResponse result =
                await _globalRatingService
                    .GetGlobalLeaderboardAsync(
                        page,
                        pageSize);

            return Ok(
                ApiResponse<GlobalRatingLeaderboardResponse>.Success(
                    result));
        }

        // =========================================================
        // CALCULATE PERSONAL RECORD RATINGS
        //
        // Used by the online level-select screen.
        //
        // Unity supplies the personal-record times from
        // PlayerPrefs. The server calculates the authoritative
        // rating for each time.
        // =========================================================

        [Authorize]
        [HttpPost("calculate")]
        public ActionResult<
            ApiResponse<CalculateRatingsResponse>>
            CalculateRatings(
                [FromBody]
                CalculateRatingsRequest request)
        {
            if (request == null)
            {
                throw new ValidationException(
                    "Request is required.");
            }

            if (string.IsNullOrWhiteSpace(
                    request.Level))
            {
                throw new ValidationException(
                    "Level is required.");
            }

            if (request.TimesMs == null)
            {
                throw new ValidationException(
                    "TimesMs is required.");
            }

            List<int?> ratings =
                _ratingService.CalculateRatings(
                    request.Level,
                    request.TimesMs);

            return Ok(
                ApiResponse<CalculateRatingsResponse>.Success(
                    new CalculateRatingsResponse
                    {
                        Ratings = ratings
                    }));
        }

        // =========================================================
        // SYNC ACHIEVEMENTS
        //
        // Updates the player's achievement-based global rating.
        // =========================================================

        [Authorize]
        [HttpPost("achievements")]
        public async Task<
            ActionResult<ApiResponse<SyncAchievementsResponse>>>
            SyncAchievements(
                [FromBody]
                SyncAchievementsRequest request)
        {
            if (request == null)
            {
                throw new ValidationException(
                    "Request is required.");
            }

            string? playerIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                playerIdClaim,
                out int playerId))
            {
                return Unauthorized();
            }

            int achievementRating =
                await _globalRatingService
                    .SyncAchievementsAsync(
                        playerId,
                        request.AchievementIds);

            return Ok(
                ApiResponse<SyncAchievementsResponse>.Success(
                    new SyncAchievementsResponse
                    {
                        AchievementRating =
                            achievementRating
                    }));
        }

        // =========================================================
        // GAME-SPECIFIC RATING LEADERBOARD
        // =========================================================

        [Authorize]
        [HttpGet("total")]
        public async Task<
            ActionResult<ApiResponse<GameRatingLeaderboardResponse>>>
            GetGameRatingLeaderboard(
                [FromQuery] string game,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(game))
            {
                throw new ValidationException(
                    "Game is required.");
            }

            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            GameRatingLeaderboardResponse result =
                await _globalRatingService
                    .GetGameRatingLeaderboardAsync(
                        game,
                        page,
                        pageSize);

            return Ok(
                ApiResponse<GameRatingLeaderboardResponse>.Success(
                    result));
        }
    }
}